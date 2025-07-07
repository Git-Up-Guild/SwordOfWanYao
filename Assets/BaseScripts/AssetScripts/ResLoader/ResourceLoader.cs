/*******************************************************************
** 文件名:	ResourcesLoader.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	郑秀程
** 日  期:	2018.6.22
** 版  本:	1.0
** 描  述:	
** 应  用:  通用资源管理器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using XClient.Scripts.Api;
using XGame;
using XGame.AssetScript.UI;
using XGame.LOP;
using XGame.Poolable;
using XGame.Trace;
using XGame.Load;
using System;

namespace XClient.Scripts.Api
{
    internal class ResourceOpTemp : LitePoolableObject
    {
        //是否为加载操作
        public bool isLoadOp;
        public int reqKey;
        public IResourcesLoader loader;

        public string load_assetPath;
        public OnResLoadCallback load_callback;
        public bool load_bInstantiate;
        public int load_userData;

        public void Execute()
        {
            if (isLoadOp)
                loader.LoadResWithReqKey(reqKey, load_assetPath, load_callback, load_bInstantiate, load_userData);
            else
                loader.UnloadRes(reqKey);
        }

        protected override void OnRecycle()
        {
            load_assetPath = null;
            load_callback = null;
            load_bInstantiate = false;
            load_userData = 0;
            reqKey = 0;
            loader = null;
        }
    }

    internal class ResInfo : IPoolable
    {
        //请求加载的key计数(改成静态的，所有的加载统一一个地址)
        public static int CUR_ALLOC_KEY = 0;

        public int key;
        public uint nHandle;    //资源handle
        public string path;
        public int nObjID;
        public int nRef;
        public bool bInstantiate;
        public int userData;
        public bool bGameObject;
        public LOPObjectComponents lopcomps;

        /// <summary>
        /// 资源等待回调列表 (reqKey - callback)
        /// </summary>
        public Dictionary<int, OnResLoadCallback> ReqKey2CallbackDic = new Dictionary<int, OnResLoadCallback>();

        public ResInfo() { }

        public bool Create()
        {
            return true;
        }

        public void Init(object context) { }

        public void Reset()
        {
            nHandle = 0;
            key = 0;
            path = null;
            nObjID = 0;
            nRef = 0;
            bGameObject = false;
            bInstantiate = false;
            userData = 0;
            lopcomps = null;
            ReqKey2CallbackDic.Clear();
        }

        public void Release() { Reset(); }
    }

    public class ResourcesLoader<T> : IResourcesLoader where T : UnityEngine.Object
    {
        //对象列表
        private Dictionary<int, ResInfo> m_dicResInfo;

        //资源加载器
        private IResLoader m_resLoader;

        //等待回调的列表
        private List<int> m_lsWaiteCallback;

        //请求key-资源hash值key
        private Dictionary<int, int> m_reqKey2ResKeyDic;

        //是否为预制体
        private bool m_isPrefab;

        //资源key
        private int RES_KEY_MAX = 1;

        private OnLoadFinish onLoadFinish;
        private OnUnLoadFinish onUnLoadFinish;

        //是否正在更新
        private bool m_isUpdating;

        //资源操作临时列表
        private List<ResourceOpTemp> m_lsOpTemp = new List<ResourceOpTemp>();

        //是否已经被释放
        private bool bRelease;

        //释放递增加载Key
        protected bool bIncKeys = false;
        protected bool bCache = false;
        protected bool bCopyFromInstance = false;

        public virtual bool Create(object p = null)
        {
            m_dicResInfo = new Dictionary<int, ResInfo>();
            m_lsWaiteCallback = new List<int>();
            m_reqKey2ResKeyDic = new Dictionary<int, int>();
            m_isPrefab = typeof(T).Equals(typeof(GameObject));

            bRelease = false;
            return true;
        }

        public void Release()
        {
            Stop();
            m_resLoader = null;

            m_dicResInfo.Clear();
            m_lsWaiteCallback.Clear();
            m_reqKey2ResKeyDic.Clear();
            bRelease = true;
        }

        public void SetLoader(IResLoader loader)
        {
            m_resLoader = loader;
        }

        public void Stop()
        {
            foreach (ResInfo info in m_dicResInfo.Values)
            {
                //从全局管理器中将对象移出
                if (info.nObjID > 0)
                {
                    if (info.bGameObject)
                    {
                        LOPObjectRegister.UnRegister(info.nObjID);
                    }
                    else
                    {
                        LOPObjectManagerInstance.obj.Remove(info.nObjID);
                    }
                }

                //释放资源
                if (info.key > 0 && m_resLoader != null)
                {
                    m_resLoader.UnloadRes(info.nHandle);
                }
            }

            m_dicResInfo.Clear();
        }

        /// <summary>
        /// 更新，推动加载回调
        /// </summary>
        public void Update()
        {
            m_isUpdating = true;

            int nCount = m_lsWaiteCallback.Count;
            if(nCount>0)
            {
                ResInfo info;
                for (int i = 0; i < nCount; ++i)
                {
                    int nKey = m_lsWaiteCallback[i];
                    if (m_dicResInfo.TryGetValue(nKey, out info))
                    {
                        InvokeCallbacks(info);
                    }
                }
                m_lsWaiteCallback.Clear();
            }
           

            
            m_isUpdating = false;

            //处理缓存的操作
            if(m_lsOpTemp.Count>0)
            {
                foreach (var t in m_lsOpTemp)
                {
                    t.Execute();
                    LitePoolableObject.Recycle(t);
                }
                m_lsOpTemp.Clear();
            }    
           

        }

        public void LoadResWithReqKey(int curReqKey, string assetPath, OnResLoadCallback callback, bool bInstantiate = false,  int userData = 0,int parentLopID = 0,Transform parent = null)
        {
            int resKey;

            //需要实例化的，要用不同的key
            if (bInstantiate|| bIncKeys)
                resKey = RES_KEY_MAX++;
            else
                resKey = assetPath.GetHashCode();

            //Debug.LogError($"LoadKey:{curReqKey}");
            m_reqKey2ResKeyDic.Add(curReqKey, resKey);

            ResInfo info;

            //已经存在记录
            if (m_dicResInfo.TryGetValue(resKey, out info))
            {
                //只要调用这个这个接口就要增加引用
                info.nRef++;

                //说明对象已经加载回来了
                if (info.nObjID > 0)
                {
                    //为了保证外部调用的统一，这里将回调异步处理
                    if (callback != null)
                    {
                        info.ReqKey2CallbackDic.Add(curReqKey, callback);
                        m_lsWaiteCallback.Add(info.key);
                    }
                }
                //正在加载中，则将回调加入
                else
                {
                    info.ReqKey2CallbackDic.Add(curReqKey, callback);
                }
            }
            //记录还不存在，发起加载
            else
            {
                info = XGameComs.Get<IItemPoolManager>().Pop<ResInfo>(null);
                info.nRef++;
                info.key = resKey;
                info.bInstantiate = bInstantiate;
                info.userData = userData;
                info.path = assetPath;
                info.ReqKey2CallbackDic.Add(curReqKey, callback);
                m_dicResInfo.Add(resKey, info);

                //预制体总是要实例化
                if (m_isPrefab)
                    bInstantiate = true;

                info.nHandle = m_resLoader.LoadRes<T>(assetPath, info.key, bInstantiate, bCopyFromInstance, this, parent);
            }
        }

        public int AlocKey()
        {
            //++ResInfo.CUR_ALLOC_KEY;
            return ++ResInfo.CUR_ALLOC_KEY;
        }

        public virtual int LoadRes(string assetPath, OnResLoadCallback callback, bool bInstantiate = false,  int userData = 0, int parentLopID = 0, Transform parent = null)
        {
            //请求key-资源key的映射
            int curReqKey = AlocKey();

            if (m_isUpdating)
            {
                ResourceOpTemp temp = LitePoolableObject.Instantiate<ResourceOpTemp>();
                temp.loader = this;
                temp.reqKey = curReqKey;
                temp.isLoadOp = true;
                temp.load_assetPath = assetPath;
                temp.load_bInstantiate = bInstantiate;
                temp.load_userData = userData;
                temp.load_callback = callback;

                m_lsOpTemp.Add(temp);
            }
            else
            {
                LoadResWithReqKey(curReqKey, assetPath, callback, bInstantiate, userData, parentLopID, parent);
            }

            return curReqKey;
        }

        /// <summary>
        /// 加载成功
        /// </summary>
        /// <param name="res"></param>
        /// <param name="handle"></param>
        /// <param name="ud"></param>
        public void OnLoadResSucess(UnityEngine.Object res, uint handle, int ud)
        {
            ResInfo info;
            if (m_dicResInfo.TryGetValue((int)ud, out info))
            {
                //记住资源句柄，后面要用来卸载
                info.nHandle = handle;

                //预制体对象添加到组件管理
                if (res is GameObject)
                {
                    GameObject go = res as GameObject;

                    //窗口特殊处理
                    LOPObjectComponents lopComponents = go.GetComponent<LOPObjectComponents>();
                    if (lopComponents != null)
                    {
                        info.nObjID = lopComponents.EnableLOP();
                        info.lopcomps = lopComponents;
                    }
                    else
                    {
                        info.nObjID = LOPObjectRegister.Register(res as GameObject);
                        info.lopcomps = null;
                    }
                      
                    info.bGameObject = true;

                    //加载完成预制体回调
                    if (onLoadFinish != null)
                    {
                        onLoadFinish.Invoke(res as GameObject);
                    }
                }
                //普通对象直接到管理器
                else
                {
                    info.nObjID = LOPObjectManagerInstance.obj.Add(res);
                    if (info.nObjID <= 0)
                    {
                        info.nObjID = 0;
                        Debug.LogError("对象id有误：" + info.nObjID);
                    }
                }

                //回调
                InvokeCallbacks(info);
            }
            else
            {
                Debug.LogError("不应该出现这种情况，请检查！！！name=" + res.name);
            }
        }

        /// <summary>
        /// 加载失败
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="ud"></param>
        public void OnLoadResFail(uint handle, int ud)
        {
            //资源加载系统在加载失败一次后会，再次加载同步返回失败
            //所以这里延后一帧回调，这样就可以统一成异步回调到外部
            if (m_dicResInfo.TryGetValue(ud, out ResInfo info))
            {
                info.nObjID = 0;
                m_lsWaiteCallback.Add(info.key);
            }
        }

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="info"></param>
        private void InvokeCallbacks(ResInfo info)
        {
            //申请列表
            var reqKeys = PoolableList.Get<int>();

            //先记录下来，避免在迭代的过程中修改集合报错
            foreach (var call in info.ReqKey2CallbackDic)
                reqKeys.Add(call.Key);

            //调用回调函数
            foreach (var reqKey in reqKeys)
            {
                //再次判断回调是否还有效，因为有可能在回调处理的过程中注销掉了
                if (info.ReqKey2CallbackDic.TryGetValue(reqKey, out OnResLoadCallback call))
                {
                    call(info.nObjID, reqKey, info.userData);
                }
            }

            //回收列表
            PoolableList.Recycle(reqKeys);

            info.ReqKey2CallbackDic.Clear();
        }

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="nResKey"></param>
        /// <param name="resID"></param>
        private void InvokeCallbacks(int nResKey, int resID)
        {
            ResInfo info;
            if (m_dicResInfo.TryGetValue(nResKey, out info))
            {
                info.nObjID = resID;
                InvokeCallbacks(info);
            }
            else
            {
                Debug.LogError("resKey不存在，不应该出现这种情况，请检查！！!");
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name=""></param>
        public virtual void UnloadRes(int nReqKey, bool bCache=true)
        {
            //正在更新，所以先存起来
            if(m_isUpdating)
            {
                ResourceOpTemp temp = LitePoolableObject.Instantiate<ResourceOpTemp>();
                temp.isLoadOp = false;
                temp.reqKey = nReqKey;
                temp.loader = this;
                m_lsOpTemp.Add(temp);
                return;
            }

            if (nReqKey == 0) return;
            if (bRelease) return;
            if (!m_reqKey2ResKeyDic.ContainsKey(nReqKey))
            {
                Debug.LogError("销毁失败，不应该出现这种找不到加载请求Key的情况, nReqKey: " + nReqKey);


#if UNITY_EDITOR
                //故意搞非法，看看lua堆栈
                ResInfo info2 = null;
                info2.bGameObject = false;
                throw new ArgumentNullException("LUA", "resource.");
#endif

                return;
            }

            int nResKey = m_reqKey2ResKeyDic[nReqKey];
            m_reqKey2ResKeyDic.Remove(nReqKey);

            ResInfo info;
            if (m_dicResInfo.TryGetValue(nResKey, out info))
            {
                //Debug.LogError($"UnloadRes key={nReqKey}, path= {info.path} ");

                //清除回调信息
                if (info.ReqKey2CallbackDic.ContainsKey(nReqKey))
                    info.ReqKey2CallbackDic.Remove(nReqKey);

                info.nRef--;

                //没有引用了，要回收
                if (info.nRef <= 0)
                {
                    //大于0说明已经加载完成，需要释放资源
                    if (info.nObjID > 0)
                    {
                        //窗口的会自己处理LOP
                        if(info.lopcomps != null)
                        {
                            info.lopcomps.DisableLOP();
                            info.lopcomps = null;
                        }
                        else
                        {
                            if (info.bGameObject)
                            {
                                LOPObjectRegister.UnRegister(info.nObjID);
                            }
                            else
                            {
                                LOPObjectManagerInstance.obj.Remove(info.nObjID);
                            }
                        }
                        info.nObjID = 0;
                    }

                    //移除记录
                    m_dicResInfo.Remove(nResKey);

                    //卸载资源
                    if (m_resLoader != null)
                    {
                        m_resLoader.UnloadRes(info.nHandle, bCache);
                        if (onUnLoadFinish != null)
                        {
                            onUnLoadFinish.Invoke();
                        }
                    }

                    //回收Info
                    IItemPoolManager poolMgr = XGameComs.Get<IItemPoolManager>();
                    if (poolMgr != null)
                        poolMgr.Push(info);

                    //清理回调列表
                    int length = m_lsWaiteCallback.Count;
                    for (int i = length - 1; i >= 0; i--)
                    {
                        if (m_lsWaiteCallback[i] == nResKey)
                        {
                            m_lsWaiteCallback.RemoveAt(i);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("卸载失败，不存在的nResKey = " + nResKey);
            }
        }

        public void AddLoadCallBack(OnLoadFinish callback)
        {
            onLoadFinish += callback;
        }

        public void AddUnLoadCallBack(OnUnLoadFinish callback)
        {
            onUnLoadFinish += callback;
        }

        public void RemoveLoadCallBack(OnLoadFinish callback)
        {
            onLoadFinish -= callback;
        }

        public void RemoveUnLoadCallBack(OnUnLoadFinish callback)
        {
            onUnLoadFinish -= callback;
        }

        public bool IsResCaching(string assetName)
        {
            return m_resLoader.IsResCaching<T>(assetName);
        }

        public bool IsLocalResExist(string assetName)
        {
            return m_resLoader.IsExistLocalCache(assetName);
        }
    }
}
