/*******************************************************************
** 文件名:	InnerResourceLoader.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	郑秀程
** 日  期:	2018.7.4
** 版  本:	1.0
** 描  述:	
** 应  用:  加载管理器实现

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.Def;
using System;
using System.Collections.Generic;

using UnityEngine;
using XGame.Asset.Load;
using XGame.Asset;
using System.Text;
using XGame.Poolable;
using XGame;

namespace Game.InnerResources
{
    /// <summary>
    /// Reources目录资源加载器
    /// </summary>
    public class InnerResourceLoader : IGAssetLoader
    {
        public int ID { get; set; }

        /// <summary>
        /// 最大并发加载两
        /// </summary>
        public static int MAX_REQUEST_CONCURRENT = 10;

        /// <summary>
        /// 加载全局回调，通常用来做资源录制等
        /// </summary>
        private Action<string, bool> m_Callbacks;

        /// <summary>
        /// 所有已经加载的资源
        /// </summary>
        private Dictionary<string, ResourceData> m_AllLoadedRes;

        /// <summary>
        /// 引用计数为0的资源数据
        /// </summary>
        private Dictionary<uint, ResourceData> m_NoUseRes;

        /// <summary>
        /// 所有的请求
        /// </summary>
        private Dictionary<uint, RequestData> m_AllRequests;

        /// <summary>
        /// 取消的加载请求列表
        /// </summary>
        private Dictionary<uint, RequestData> m_CancelRequests;

        /// <summary>
        /// 正在加载的请求
        /// </summary>
        private List<RequestData> m_Requesting;

        /// <summary>
        /// 等待加载的队列
        /// </summary>
        private Queue<RequestData> m_Waitting;

        /// <summary>
        /// 是否全速加载
        /// </summary>
        public bool IsFullSpeed { get; private set; }

        public void AddLoadSystem(ILoadSystem loader)
        {
            Debug.Log("此加载器不支持外置的加载系统！");
        }

        public void CancelAllAsyncLoad()
        {
        }

        public bool Create(object context, object config = null)
        {
            ID = 99878;

            m_AllLoadedRes = new Dictionary<string, ResourceData>();
            m_AllRequests = new Dictionary<uint, RequestData>();
            m_NoUseRes = new Dictionary<uint, ResourceData>();
            m_CancelRequests = new Dictionary<uint, RequestData>();

            m_Requesting = new List<RequestData>();
            m_Waitting = new Queue<RequestData>();

            return true;
        }

        public void DumpAllAssetsName()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var data in m_AllLoadedRes.Values)
            {
                sb.AppendLine(data.assetPath);
            }
            Debug.Log(sb.ToString());
        }

        public string[] GetLoadedInfo()
        {
            string[] resNames = new string[m_AllLoadedRes.Count];
            int nIndex = 0;
            foreach (var value in m_AllLoadedRes.Values)
            {
                resNames[nIndex++] = value.assetPath;
            }
            return resNames;
        }

        public bool IsExistLocalCache(string path)
        {
            foreach (var data in m_AllLoadedRes.Values)
            {
                if (data.assetPath == path)
                    return true;
            }
            return false;
        }

        public bool IsOverWork()
        {
            return m_Requesting.Count >= MAX_REQUEST_CONCURRENT;
        }

        private RequestData BuildRequestData<T>(string assetPath, object objUserData, int intUserData,
            IGAssetLoaderSink sink, IGAssetLoaderSinkWithInt sinkWithInt) where T : UnityEngine.Object
        {
            var d = XGameComs.Get<IItemPoolManager>().Pop<RequestData>();
            d.assetPath = assetPath;
            if (sink != null || sinkWithInt != null)
                d.SetSink<T>(assetPath, objUserData, intUserData, sink, sinkWithInt);

            m_AllRequests.Add(d.handle, d);

            return d;
        }

        private ResourceData GetOrBuildResourceData<T>(string assetPath) where T : UnityEngine.Object
        {
            ResourceData d = null;
            if(m_AllLoadedRes.TryGetValue(assetPath, out d))
            {
                return d;
            }

            d = XGameComs.Get<IItemPoolManager>().Pop<ResourceData>();
            d.assetPath = assetPath;
            d.loadFunc = ResGenericFuncsFactory.CreateResLoadGenericFuncs<T>();

            m_AllLoadedRes.Add(assetPath, d);

            return d;
        }

        public uint LoadRes<T>(string assetPath, object userData, IGAssetLoaderSink sink, LoadQueue queue = LoadQueue.Normal) where T : UnityEngine.Object
        {
            assetPath = GamePath.ToResourceLoadPath(assetPath);

            RequestData reqData = BuildRequestData<T>(assetPath, userData, 0, sink, null);
            ResourceData resData = GetOrBuildResourceData<T>(assetPath);

            reqData.LinkToResourceData(resData);
            reqData.state = RequestData.RequestState.WaitQueue;

            m_Waitting.Enqueue(reqData);

            return reqData.handle;
        }

        public uint LoadRes<T>(string assetPath, int userData, IGAssetLoaderSinkWithInt sink, LoadQueue queue = LoadQueue.Normal) where T : UnityEngine.Object
        {
            assetPath = GamePath.ToResourceLoadPath(assetPath);

            RequestData reqData = BuildRequestData<T>(assetPath, null, 0, null, sink);
            ResourceData resData = GetOrBuildResourceData<T>(assetPath);
            resData.IncRef();

            reqData.LinkToResourceData(resData);
            reqData.state = RequestData.RequestState.WaitQueue;

            m_Waitting.Enqueue(reqData);

            return reqData.handle;
        }

        public object LoadResSync<T>(string assetPath, out uint handle, bool bLoadAll = false) where T : UnityEngine.Object
        {
            assetPath = GamePath.ToResourceLoadPath(assetPath);

            RequestData reqData = BuildRequestData<T>(assetPath, null, 0, null, null);
            reqData.state = RequestData.RequestState.Finish;

            ResourceData resData = GetOrBuildResourceData<T>(assetPath);
            reqData.LinkToResourceData(resData);

            //直接设置资源数据为完成状态
            if(resData.state != ResourceData.LoadState.Done)
            {
                resData.res = Resources.Load<T>(assetPath) as T;
                resData.state =  ResourceData.LoadState.Done;
            }

            handle = reqData.handle;

            return resData.res as object;
        }

        public void OutputDebugInfo(string resPath = null) {}

        public void Release()
        {
            m_Requesting.Clear();
            m_Waitting.Clear();
            m_AllLoadedRes.Clear();
            m_AllRequests.Clear();

            XGameComs.Get<IItemPoolManager>()?.Unregister<ResourceData>();
            XGameComs.Get<IItemPoolManager>()?.Unregister<RequestData>();
        }

        public void AddLoadCallback(Action<string, bool> callback)
        {
            m_Callbacks -= callback;
            m_Callbacks += callback;
        }

        public void RemoveLoadCallback(Action<string, bool> callback)
        {
            m_Callbacks -= callback;
        }

        public void SetFullSpeedLoad(bool bFullSpeedLoad)
        {
            IsFullSpeed = IsFullSpeed;
        }

        public bool Start()
        {
            XGameComs.Get<IItemPoolManager>().Register<RequestData>();
            XGameComs.Get<IItemPoolManager>().Register<ResourceData>();

            return true;
        }

        public void Stop()
        {
        }

        public void UnloadRes(uint handle)
        {
            RequestData reqData = null;

            if(!m_AllRequests.TryGetValue(handle, out reqData))
            {
                Debug.LogError("要卸载的资源不存在！handle=" + handle);
                return;
            }

            //请求数据回收
            AddToCancelRequests(reqData);
        }

        private void AddToCancelRequests(RequestData reqData)
        {
            if (reqData.state != RequestData.RequestState.Cancel)
            {
                reqData.state = RequestData.RequestState.Cancel;
                m_CancelRequests.Add(reqData.handle, reqData);
            }
        }

        internal void OnResourceNoUse(ResourceData resData)
        {
            m_NoUseRes.Add(resData.handle, resData);
        }

        internal void OnResourceUse(ResourceData resData)
        {
            if(m_NoUseRes.ContainsKey(resData.handle))
                m_NoUseRes.Remove(resData.handle);
        }

        public void Update()
        {
            IPoolableItemPool reqItemPool = null;

            //处理当前加载
            for (int i = m_Requesting.Count - 1; i >= 0; i--)
            {
                var r = m_Requesting[i];

                if (r.state == RequestData.RequestState.Wait
                    || r.state == RequestData.RequestState.Request)
                {
                    //加载完成了
                   if(r.resData.IsDone())
                    {
                        r.InvokeSinks(r.resData.res);
                    }
                    //还没加载完成的
                    else if(r.state == RequestData.RequestState.Wait)
                    {
                        r.state = RequestData.RequestState.Request;
                        r.resData.LoadRes();
                    }
                }

                //已经完成的要移出加载队列
                if(r.state == RequestData.RequestState.Finish
                    || r.state == RequestData.RequestState.Idle)
                {
                    m_Requesting.RemoveAt(i);
                    r.state = RequestData.RequestState.Idle;
                }
                //取消的也要移出队列
                else if (r.state == RequestData.RequestState.Cancel)
                {
                    m_Requesting.RemoveAt(i);
                }
                //其他状态说明存在异常
                else if (r.state != RequestData.RequestState.Request)
                {
                    Debug.LogError("请求数据状态异常，强制取消");
                    m_Requesting.RemoveAt(i);
                    AddToCancelRequests(r);
                }
            }

            //如果没有满负荷，则从等待队列中加载
            int nCount = MAX_REQUEST_CONCURRENT - m_Requesting.Count;
            while (nCount > 0 && m_Waitting.Count > 0)
            {
                var r = m_Waitting.Dequeue();
                if(r.state == RequestData.RequestState.WaitQueue)
                {
                    r.state = RequestData.RequestState.Wait;
                    m_Requesting.Add(r);

                    if (r.resData.IsDone())
                    {
                        //已经加载完成的，不参与到计数
                    }
                    else
                    {
                        nCount--;
                    }
                }
                else
                {
                    if(r.state != RequestData.RequestState.Cancel)
                    {
                        Debug.LogError("等待队列中的额请求数据异常，强制取消！");
                        AddToCancelRequests(r);
                    }
                }
            }

            //清理所有的取消加载，经过上面的处理后，这些取消的请求已经不在加载队列和等待队列中了
            foreach(var r in m_CancelRequests.Values)
            {
                r.UnlinkToResourceData();

                m_AllRequests.Remove(r.handle);

                if (reqItemPool == null)
                    reqItemPool = XGameComs.Get<IItemPoolManager>().GetPool<RequestData>();
                reqItemPool?.Push(r);
            }
            m_CancelRequests.Clear();

            //卸载没有被使用的资源
            List<uint> lsUnloadRes = null;
            foreach(var d in m_NoUseRes.Values)
            {
                if(d.IsCanUnloadRes())
                {
                    if (lsUnloadRes == null)
                        lsUnloadRes = PoolableList.Get<uint>();

                    lsUnloadRes.Add(d.handle);
                }
            }

            if(lsUnloadRes != null)
            {
                IPoolableItemPool resDataPool = null;
                foreach (var d in m_NoUseRes.Values)
                {
                    m_NoUseRes.Remove(d.handle);
                    m_AllLoadedRes.Remove(d.assetPath);

                    //对象回收
                    if (resDataPool == null)
                        resDataPool = XGameComs.Get<IItemPoolManager>().GetPool<ResourceData>();

                    resDataPool.Push(d);
                }

                PoolableList.Recycle<uint>(lsUnloadRes);
            }

        }
    }
}

