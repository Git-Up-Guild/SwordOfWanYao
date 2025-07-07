/*******************************************************************
** 文件名: ResLoadApdater.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:   郑秀程
** 日  期:    2019/10/12
** 版  本:    1.0
** 描  述:    资源加载适配
** 应  用:               
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.Asset;
using XGame.Def;
using XGame.Poolable;
using XGame.UnityObjPool;
using System.Reflection;
using UnityEngine;
using XGame;
using XGame.Load;

/// <summary>
/// 加载适配器
/// </summary>
namespace XClient.Scripts.Api
{
    class ResourceLoadAdapter : IResLoader, IUnityObjectPoolSinkWithObj
    {
        private IUnityObjectPool m_Pool;
        private IGAssetLoader m_AssetsLoader;

        public bool Create()
        {
            m_Pool = XGameComs.Get<IUnityObjectPool>();
            m_AssetsLoader = XGameComs.Get<IGAssetLoader>();
            return true;
        }

        public bool IsResCaching<T>(string assetName) where T : UnityEngine.Object
        {
            if (m_Pool != null)
                return m_Pool.IsResChaching<T>(assetName);
            return false;
        }

        public bool IsExistLocalCache(string assetName)
        {
            return m_AssetsLoader.IsExistLocalCache(assetName);
        }

        public uint LoadRes<T>(string assetBundle, int userData, bool bInstantiate, bool copyFromInstance, 
            IResLoaderSink loadSink, Transform parent = null) where T : UnityEngine.Object
        {
            UserData ud = LitePoolableObject.Instantiate<UserData>();
            ud.i0 = userData;
            ud.o0 = typeof(T);
            ud.o1 = loadSink;

            var context = LoadContextStore.Get<T, object>();
            context.assetPath = assetBundle;
            context.SetSink(this, ud);
            context.instantiate = bInstantiate;
            context.copyInstance = copyFromInstance;
            return m_Pool.LoadRes<T>(assetBundle, ud, this, bInstantiate, copyFromInstance, parent);
        }

        public void OnUnityObjectLoadCancel(uint handle, object ud)
        {
            UserData userData = ud as UserData;
            if (userData != null)
            {
                LitePoolableObject.Recycle(userData);
            }
        }

        public void OnUnityObjectLoadComplete(Object res, uint handle, object ud)
        {
            UserData userData = ud as UserData;
            if (userData == null)
            {
                Debug.LogError("加载回调异常，UserData获取失败！");
                return;
            }

            var intUserData = userData.i0;
            var T = (System.Type)userData.o0;
            var loadSink = userData.o1 as IResLoaderSink;
            LitePoolableObject.Recycle(userData);
            if (res != null /*&& res.GetType() == T*/)
            {
                loadSink?.OnLoadResSucess(res, handle, intUserData);
            }
            else
            {
                Debug.LogError($"加载资源失败！！ handle：{handle}");
                loadSink?.OnLoadResFail(handle, intUserData);
                UnloadRes(handle,false);
            }
        }

        public void Release()
        {
        }

        public void UnloadRes(uint handle,bool bCache)
        {
            IUnityObjectPool pool = XGameComs.Get<IUnityObjectPool>();
            if(pool != null)
            {
                pool.UnloadRes(handle,false,false,false, bCache);
            }
        }
    }
}
