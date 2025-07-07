/*******************************************************************
** 文件名:	RequestData.cs
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

using UnityEngine;
using XGame.Asset.Load;
using XGame.Asset;
using XGame.Poolable;

namespace Game.InnerResources
{
    /// <summary>
    /// 加载请求数据
    /// </summary>
    class RequestData : IPoolable
    {
        public enum RequestState
        {
            WaitQueue,          //在等待队列中
            Wait,                    //进入到加载队列
            Request,               //发起加载
            Finish,                 //加载已经完成（执行过回调）
            Idle,                    //不在任何队列中
            Cancel,                 //用户取消了（调用了Unload接口）
        }

        private static uint MAX_REQ_HANDLE = 1;

        public uint handle { get; private set; }
        public string assetPath = string.Empty;
        public IResCallbackGenericFuncs callback;

        public RequestState state = RequestState.WaitQueue;

        public ResourceData resData { get; private set; }

        public RequestData()
        {
        }

        public void LinkToResourceData(ResourceData r)
        {
            if(resData != null)
            {
                Debug.LogError("重复链接到资源数据！");
                return;
            }

            resData = r;
            resData.IncRef();
        }

        public void UnlinkToResourceData()
        {
            if(resData != null)
            {
                resData.DecRef();
                resData = null;
            }
        }

        public bool Create()
        {
            handle = MAX_REQ_HANDLE++;
            return true;
        }

        public void Init(object context = null)
        {
        }

        public void Release()
        {
            Reset();
        }

        /// <summary>
        /// 添加一个回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="objUserData"></param>
        /// <param name="intUserData"></param>
        /// <param name="sink"></param>
        /// <param name="sinkWithInt"></param>
        public void SetSink<T>(string assetPath, object objUserData, int intUserData, IGAssetLoaderSink sink, IGAssetLoaderSinkWithInt sinkWithInt) where T : UnityEngine.Object
        {
            callback = ResGenericFuncsFactory.CreateResCallbackGenericFuncs<T>(sink, sinkWithInt, objUserData, intUserData);
        }

        /// <summary>
        /// 进行执行回调
        /// </summary>
        public void InvokeSinks(UnityEngine.Object res)
        {
            if(callback != null)
            {
                if (res != null)
                    callback.InvokeLoadSucCallback(res, handle);
                else
                    callback.InvokeLoadFailCallback(handle);

                ResGenericFuncsFactory.ReleaseResCallbackGenericFuncs(callback);
                callback = null;
            }

            state = RequestState.Finish;
        }

        public void Reset()
        {
            if(callback != null)
            {
                ResGenericFuncsFactory.ReleaseResCallbackGenericFuncs(callback);
                callback = null;
            }

            assetPath = string.Empty;
            state = RequestState.WaitQueue;
            resData = null;
        }
    }
}

