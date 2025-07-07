/*******************************************************************
** 文件名:	ResourceData.cs
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

using System;

using UnityEngine;
using XGame.Asset.Load;
using XGame.Poolable;

namespace Game.InnerResources
{
    /// <summary>
    /// 加载的资源信息
    /// </summary>
    class ResourceData : IPoolable
    {
        public enum LoadState
        {
            None,
            Loading,
            Done,
        }

        private static uint MAX_RES_HANDLE = 1;
        private static uint MAX_KEEP_FRAME_COUNT = 300;

        public uint handle { get; private set; }
        public string assetPath = String.Empty;
        public UnityEngine.Object res;

        public IResLoadGenericFuncs loadFunc;

        public LoadState state;

        private int refCount;

        public int RefCount => refCount;

        public ResourceRequest request { get; private set; }

        //未被使用的帧数
        public int  zeroRefCountFrameIndex{ get; private set; }

        //记载管理器
        public InnerResourceLoader loadManager;

        //增加使用计数
        public int IncRef()
        {
            if(refCount == 0)
                loadManager?.OnResourceUse(this);

            refCount++;
            zeroRefCountFrameIndex = 0;
            return refCount;
        }

        //减少使用计数
        public int DecRef()
        {
            refCount--;
            
            if (refCount <= 0)
            {
                refCount = 0;
                zeroRefCountFrameIndex = Time.frameCount;

                loadManager?.OnResourceNoUse(this);
            }

            return refCount;
        }

        public bool IsDone()
        {
            if (state == LoadState.Done)
                return true;

            if(request != null && request.isDone)
            {
                res = request.asset;
                state = LoadState.Done;
            }

            return state == LoadState.Done;
        }

        public void LoadRes()
        {
            if(state == LoadState.None)
            {
                state = LoadState.Loading;
                request = loadFunc.LoadResourcesAssetAsync(assetPath);
            }
        }

        public bool IsCanUnloadRes()
        {
            if (refCount > 0 || state == LoadState.Loading || (Time.frameCount - zeroRefCountFrameIndex) < MAX_KEEP_FRAME_COUNT)
                return false;

            return true;
        }

        public void UnloadRes()
        {
            if(!IsCanUnloadRes())
            {
                Debug.LogError("强制卸载资源，可能会出错哦！");
            }

            if (res != null)
            {
                Resources.UnloadAsset(res);
                res = null;
            }
        }

        public bool Create()
        {
            handle = MAX_RES_HANDLE++;
            return true;
        }

        public void Init(object context = null)
        {
        }

        public void Reset()
        {
            assetPath = String.Empty;
            handle = 0;
            res = null;
            refCount = 0;
            request = null;
            zeroRefCountFrameIndex = 0;
            loadManager = null;

            if (loadFunc != null)
            {
                ResGenericFuncsFactory.ReleaseResLoadGenericFuncs(loadFunc);
                loadFunc = null;
            }    
        }

        public void Release()
        {
            res = null;
            loadManager = null;
            request = null;
            if (loadFunc != null)
            {
                ResGenericFuncsFactory.ReleaseResLoadGenericFuncs(loadFunc);
                loadFunc = null;
            }
        }
    }
}

