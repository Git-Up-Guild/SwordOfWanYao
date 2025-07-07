/*******************************************************************
** 文件名: UnityObjPoolLoaderAdapter.cs
** 版  权:    (C) 深圳冰川网络网络科技有限公司 
** 创建人:   郑秀程
** 日  期:    2019/10/12
** 版  本:    1.0
** 描  述:    GO对象池专用资源加载适配器
** 应  用:               
**
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.Asset;
using XGame.Asset.Load;
using XGame.Def;
using XGame.Poolable;
using XGame.UnityObjPool;
using UnityEngine;

namespace Game
{
    public class UnityObjectPoolResLoader : IUnityObjectPoolResLoader, IGAssetLoaderSink
    {
        private IGAssetLoader m_assetLoader;
        private IGAssetLoader assetLoader
        {
            get
            {
                if (m_assetLoader == null)
                    m_assetLoader = XGame.XGameComs.Get<IGAssetLoader>();
                return m_assetLoader;
            }
        }

        public void SetAssetLoader(IGAssetLoader assetLoader)
        {
            m_assetLoader = assetLoader;
        }

        public bool Create()
        {
            return true;
        }

        public bool IsOverWork()
        {
            return false;
        }

        public T LoadRes<T>(string assetName, out uint handle) where T : UnityEngine.Object
        {
            return assetLoader.LoadResSync<T>(assetName, out handle) as T;
        }

        public uint LoadRes<T>(string assetName, IUnityObjectPoolResLoaderSink sink, LoadQueue queue = LoadQueue.Normal) where T : UnityEngine.Object
        {
            UserData userData = LitePoolableObject.Instantiate<UserData>();
            userData.o0 = sink;
            return assetLoader.LoadRes<T>(assetName, userData, this, queue);
        }

        public void OnLoadAssetFail(uint nResKey, object ud = null)
        {
            if(null!= ud)
            {
                UserData userData = ud as UserData;
                IUnityObjectPoolResLoaderSink sink = userData.o0 as IUnityObjectPoolResLoaderSink;

                LitePoolableObject.Recycle(userData);

                if(null!= sink)
                 sink.OnLoadAssetFail(nResKey);
            }

            UnloadRes(nResKey);
        }

        public void OnLoadAssetSuccess(Object obj, uint nResKey, object ud = null)
        {
            UserData userData = ud as UserData;
            IUnityObjectPoolResLoaderSink sink = userData.o0 as IUnityObjectPoolResLoaderSink;

            LitePoolableObject.Recycle(userData);

            sink.OnLoadAssetSuccess(obj, nResKey);
        }

        public void Release()
        {
        }

        public void UnloadRes(uint handle)
        {
            assetLoader.UnloadRes(handle);
        }
    }
}
