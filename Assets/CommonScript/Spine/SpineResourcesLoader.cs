/*******************************************************************
** 文件名:	SpineResourcesLoader.cs
** 版  权:	(C) 幻引力有限公司
** 创建人:	许德纪
** 日  期:	2021.11.27
** 版  本:	1.0
** 描  述:	
** 应  用:  spine特殊资源的加载器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
***/

using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XGame;
using XGame.Asset;
using XGame.LOP;
using XGame.Poolable;
using XGame.UnityObjPool;
using XGame.Load;
using XGame.Ini;

namespace XClient.Scripts.Api
{

    //图集信息
    internal class AtlasInfo : IPoolable
    {
        public bool Create()
        {
            return true;

        }
        public void Init(object context = null)
        {

        }
        public void Release()
        {

        }
        public void Reset()
        {
            data = null;
            index = 0;
        }

        public int index;
        public TextAsset data;
    }

    internal class SpineResInfo : IPoolable
    {
        //骨骼数据资源
        public SkeletonDataAsset skeletonDataAsset;

        public OnResLoadCallback callback;
        public int userData;
        public int nResID;

        //动画数据加载句柄
        public int aniReqkey;

        //动画配置数据
        public TextAsset texAniData;

        //图集配置数据加载句柄
        public Dictionary<int, AtlasInfo> dicAtlasData = new Dictionary<int, AtlasInfo>();


        public SkeletonAnimation sa = null;
        public SkeletonGraphic sg = null;

        public SpineResInfo() { }

        public bool Create()
        {


            return true;
        }

        public void Init(object context) { }

        public void Reset()
        {
            callback = null;
            userData = 0;
            nResID = 0;
            aniReqkey = 0;
            texAniData = null;
            dicAtlasData.Clear();
            sa = null;
            sg = null;

        }

        public void Release() { Reset(); }
    }

    class SpineResourcesLoader<T> : ResourcesLoader<UnityEngine.Object> where T : UnityEngine.Object
    {
        //全局对象
        static public SpineResourcesLoader<T> _Instance = null;


        //是否已经初始化过了
        private bool m_init = false;


        static public SpineResourcesLoader<T> Instance()
        {
            if (null == _Instance)
            {
                _Instance = new SpineResourcesLoader<T>();
                _Instance.Create(); 

                //资源加载器
                ResourceLoadAdapter resLoadAdapter = new ResourceLoadAdapter();
                resLoadAdapter.Create();
                _Instance.SetLoader(resLoadAdapter);

               

            }

            return _Instance;
        }



        Dictionary<int, SpineResInfo> m_dicLoadReq = new Dictionary<int, SpineResInfo>();




        private OnResLoadCallback m_OnSpineModelCallback;
        private OnResLoadCallback m_OnSpineAniCallback;
        private OnResLoadCallback m_OnSpineAtlasCallback;

        public override bool Create(object p = null)
        {

            if(m_init)
            {
                return true;
            }

            m_init = true;

            m_OnSpineModelCallback = OnSpineModelCallback;
            m_OnSpineAniCallback = OnSpineAniCallback;
            m_OnSpineAtlasCallback = OnSpineAtlasCallback;

            XGameComs.Get<IItemPoolManager>().Register<SpineResInfo>(50);
            XGameComs.Get<IItemPoolManager>().Register<AtlasInfo>(50);
            this.bIncKeys = true;
            base.bCopyFromInstance = false;
            return base.Create(p);
        }
        public override int LoadRes(string assetPath, OnResLoadCallback callback, bool bInstantiate = false, int userData = 0, int parentLopID = 0, Transform parent = null)
        {
            int reqKey = base.LoadRes(assetPath, m_OnSpineModelCallback, bInstantiate, userData, parentLopID, parent);

            SpineResInfo info = XGameComs.Get<IItemPoolManager>().Pop<SpineResInfo>(null);
            info.callback = callback;
            info.userData = userData;

            m_dicLoadReq.Add(reqKey, info);

            return reqKey;
        }



        public override void UnloadRes(int nReqKey, bool bCache = true)
        {
            SpineResInfo info = null;
            if (m_dicLoadReq.TryGetValue(nReqKey, out info))
            {
                //清除回调
                info.callback = null;
                //进行一次完成处理
                OnLoadFinish(nReqKey);
            }

            //释放主资源
            base.UnloadRes(nReqKey, bCache);
        }


        public int LoadSpineAni(GameObject spineObj, OnResLoadCallback callback, int userData = 0, SkeletonAnimation sa = null, SkeletonGraphic sg = null)
        {
            int reqKey = base.AlocKey();

            SpineResInfo info = XGameComs.Get<IItemPoolManager>().Pop<SpineResInfo>(null);
            info.callback = callback;
            info.userData = userData;

            m_dicLoadReq.Add(reqKey, info);

            OnLoadSpineAni(spineObj, reqKey, 0, sa, sg);

            return reqKey;
        }

        public void UnLoadSpineAni(int nReqKey)
        {
            SpineResInfo info = null;
            if (m_dicLoadReq.TryGetValue(nReqKey, out info))
            {
                //清除回调
                info.callback = null;
                //进行一次完成处理
                OnLoadFinish(nReqKey);
            }
        }

        private Vector3 s_InViviblePos = new Vector3(-10000000, 0, 0);
        //主体 gameobject 加载回来
        public void OnSpineModelCallback(int resID, int reqKey, int userData = 0)
        {

            GameObject obj = LOPObjectManagerInstance.obj.Get(resID) as GameObject;


            OnLoadSpineAni(obj, reqKey, resID);


        }

        private void OnLoadSpineAni(GameObject obj, int reqKey, int resID, SkeletonAnimation sa = null, SkeletonGraphic sg = null)
        {
            if (obj == null)
            {
                Debug.LogError("GameObject==null:OnLoadSpineAni");
                return;
            }

            //保存资源ID
            SpineResInfo info = null;
            if (m_dicLoadReq.TryGetValue(reqKey, out info))
            {
                info.nResID = resID;
            }

            if (null == sa && sg == null)
            {
                SpineComponent sc = obj.GetComponentInChildren<SpineComponent>();
                if (sc == null)
                {
                  
                    sa = obj.GetComponentInChildren<SkeletonAnimation>();
                    sg = obj.GetComponentInChildren<SkeletonGraphic>();

                    //spine资源，报错警告
                    if (sa != null|| sg!=null)
                    {
                        Debug.LogError("Spine预制体没有添加 SpineComponent name=" + obj.name);
                    }

                }
                else
                {
                    sa = sc.skeAni;
                    sg = sc.skeGra;
                }
            }


            SkeletonDataAsset skeletonDataAsset = null;
            if (sa)
            {
                skeletonDataAsset = sa.skeletonDataAsset;
            }
            else if (sg)
            {
                skeletonDataAsset = sg.skeletonDataAsset;
            }

            //保存骨骼数据资源
            info.skeletonDataAsset = skeletonDataAsset;
            info.sa = sa;
            info.sg = sg;

            if (info == null || skeletonDataAsset == null || skeletonDataAsset.IsLoaded)
            {
                //Debug.LogError("没有找到组件 SkeletonAnimation");
                OnLoadFinish(reqKey);
                return;
            }


            if (sa != null)
            {
                sa.enabled = false;
            }
            else if (sg != null)
            {
                sg.enabled = false;
            }


            //有父亲的，先隐藏，免得闪烁
            /*
            if(obj != null&& obj.activeSelf)
            {
                obj.BetterSetActive(false);
            }*/

            /*
            if (obj != null && obj.transform.parent != null)
            {
                RectTransform rf = obj.transform as RectTransform;
                if(null!=rf)
                {
                    rf.anchoredPosition3D = s_InViviblePos;
                }else
                {
                    obj.transform.localPosition = s_InViviblePos;
                }
                

            }
            */



            //加载图集配置数据
            AtlasAssetBase[] atlasAssets = skeletonDataAsset.atlasAssets;
            int handle = 0;
            string atlasPath = null;
            for (int i = 0; i < atlasAssets.Length; ++i)
            {
                atlasPath = atlasAssets[i].GetAtlasPath();
                if (String.IsNullOrEmpty(skeletonDataAsset.skeletonJsonPath) == false)
                {
                    handle = base.LoadRes(atlasPath, m_OnSpineAtlasCallback, false, reqKey);

                    AtlasInfo atlasInfo = XGameComs.Get<IItemPoolManager>().Pop<AtlasInfo>(null);
                    atlasInfo.index = i;
                    atlasInfo.data = null;
                    info.dicAtlasData.Add(handle, atlasInfo);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError(obj.name + ":tlasAssets[i].GetAtlasPath()==null i =" + i);
                }
#endif

            }

            //获取骨骼数据,加载骨骼动画数据
            if (String.IsNullOrEmpty(skeletonDataAsset.skeletonJsonPath) == false)
            {
                info.aniReqkey = base.LoadRes(skeletonDataAsset.skeletonJsonPath, m_OnSpineAniCallback, false, reqKey);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError(obj.name + ":skeletonDataAsset.skeletonJsonPath==null");
            }
#endif           

        }


        //动作配置信息加载回来
        public void OnSpineAniCallback(int resID, int reqKey, int userData = 0)
        {
            SpineResInfo info = null;
            //加载已经取消,马上 unload 动画数据， userData 是主资源handle
            if (m_dicLoadReq.TryGetValue(userData, out info) == false)
            {
                base.UnloadRes(reqKey, false);
                return;
            }

            //同步动画资源
            TextAsset obj = LOPObjectManagerInstance.obj.Get(resID) as TextAsset;
            info.texAniData = obj;
            OnCheckLoad(userData, info);


        }

        //图集配置信息加载回来
        public void OnSpineAtlasCallback(int resID, int reqKey, int userData = 0)
        {
            SpineResInfo info = null;
            //加载已经取消,马上 unload 动画数据， userData 是主资源handle
            if (m_dicLoadReq.TryGetValue(userData, out info) == false)
            {
                base.UnloadRes(reqKey, false);
                return;
            }

            //设置图集数据
            TextAsset obj = LOPObjectManagerInstance.obj.Get(resID) as TextAsset;
            info.dicAtlasData[reqKey].data = obj;
            OnCheckLoad(userData, info);

        }

        //检查是否加载完成
        private void OnCheckLoad(int reqKey, SpineResInfo info)
        {


            //没有被其他对象初始化的
            if (info.skeletonDataAsset.IsLoaded == false)
            {
                //动画数据没有加载回来
                if (info.texAniData == null)
                {
                    return;
                }

                //初始化图集的加载
                foreach (AtlasInfo value in info.dicAtlasData.Values)
                {
                    if (value.data == null)
                    {
                        return;
                    }

                    info.skeletonDataAsset.LoadAtlas(value.index, value.data);
                }

                //加载骨骼
                info.skeletonDataAsset.GetSkeletonData(true, info.texAniData.bytes);
            }
#if UNITY_EDITOR
            else
            {
                if (GameIni.Instance.enableDebug)
                    Debug.Log("跳过解析 skeletonDataAsset name = " + info.skeletonDataAsset.name);
            }
#endif


            //进行加载完成回调
            OnLoadFinish(reqKey);
        }

        //完全加载完成
        private void OnLoadFinish(int reqKey)
        {
            SpineResInfo info = null;
            if (m_dicLoadReq.TryGetValue(reqKey, out info))
            {
                m_dicLoadReq.Remove(reqKey);

                if (null != info)
                {
                    if (info.sa != null)
                    {
                        info.sa.enabled = true;
                        info.sa.Initialize(false, false);
                        /*
                        if(info.sa.gameObject.activeSelf==false)
                        {
                            info.sa.gameObject.BetterSetActive(true);
                        }
                        */
                        info.sa.Update(0);
                        info.sa.LateUpdate();
                    }
                    else if (info.sg != null)
                    {
                        info.sg.enabled = true;
                        info.sg.Initialize(false);
                        /*
                        if (info.sg.gameObject.activeSelf == false)
                        {
                            info.sg.gameObject.BetterSetActive(true);
                        }
                        */
                        info.sg.Update(0);
                        info.sg.LateUpdate();
                    }

                    /*
                    GameObject obj = LOPObjectManagerInstance.obj.Get(info.nResID) as GameObject;
                    if(obj!=null&& obj.activeSelf==false)
                    {
                        obj.BetterSetActive(true);
                    }
                    */

                    if (info.callback != null)
                    {
                        info.callback.Invoke(info.nResID, reqKey, info.userData);
                        info.callback = null;
                    }




                    if (info.aniReqkey > 0)
                    {
                        base.UnloadRes(info.aniReqkey, false);
                    }

                    AtlasInfo atlasInfo = null;
                    foreach (var key in info.dicAtlasData.Keys)
                    {
                        atlasInfo = info.dicAtlasData[key];
                        if (null != atlasInfo)
                        {
                            atlasInfo.Reset();
                            XGameComs.Get<IItemPoolManager>().Push(atlasInfo);
                            base.UnloadRes(key, false);

                        }

                    }



                    info.Reset();
                    XGameComs.Get<IItemPoolManager>().Push(info);

                }
            }
        }
    }
}
