///*******************************************************************
//** 文件名:	ResLoaderProxy.cs
//** 版  权:	(C) 冰川网络网络科技有限公司
//** 创建人:	郑秀程
//** 日  期:	2019.6.28
//** 版  本:	1.0
//** 描  述:	加载代理器，这里主要是用来将加载的资源句柄化，便于外部的统一调用
//** 应  用:  

//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

//using XGame;
//using XGame.Asset;
////using XGame.Asset.Loader;
//using XGame.Poolable;
//using rkt;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Game
//{
//    public delegate void OnLoadSucFromProxy<T>(T t, uint handle, object userData);
//    public delegate void OnLoadFailFromProxy(uint handle, object userData);
//    /// <summary>
//    /// 句柄化化的资源加载器
//    /// </summary>
//    public class HandleableResourceLoader
//    {


//        //public class ResInfo2 : IPoolable
//        //{
//        //    public string assetPath;

//        //    public void Release()
//        //    {
//        //    }

//        //    public bool Create()
//        //    {
//        //        return true;
//        //    }

//        //    public void Init(object context)
//        //    {
//        //    }

//        //    public void Reset()
//        //    {
//        //    }

//        //}

//        public static HandleableResourceLoader Instance;
//        private static uint HANDLE = 1;
//        private Dictionary<uint, ResInfo> m_dicResInfo;
//        private XGame.Asset.IGAssetLoader m_assetLoader;

//        public bool Create()
//        {
//#if UNITY_EDITOR
//            if (Instance != null)
//            {
//                Debug.LogError("ResourceLoaderProxy 重复创建！！！");
//                Instance.Release();
//                Instance = null;
//            }
//#endif
//            Instance = this;
//            m_dicResInfo = new Dictionary<uint, ResInfo>();
//            m_assetLoader = null;

//            //IItemPoolManager itemPool = XGame.XGameComs.Get<IItemPoolManager>();
//            //itemPool.Register<ResInfo>();

//            return true;
//        }

//        public void Release()
//        {
//            m_dicResInfo.Clear();

//            //IItemPoolManager itemPool = XGame.XGameComs.Get<IItemPoolManager>();
//            //if(itemPool != null)
//            //{
//            //    itemPool.Register<ResInfo>();
//            //}
//        }

//        private IGAssetLoader assetLoader
//        {
//            get
//            {
//                if (m_assetLoader == null)
//                {
//                    m_assetLoader = XGame.XGameComs.Get<IGAssetLoader>();
//                }
//                return m_assetLoader;
//            }
//        }

//        ///// <summary>
//        ///// 同步加载接口
//        ///// </summary>
//        ///// <typeparam name="T"></typeparam>
//        ///// <param name="assetPath"></param>
//        ///// <param name="assetName"></param>
//        ///// <param name="handle"></param>
//        ///// <returns></returns>
//        //public T LoadRes<T>(string assetPath, string assetName, out uint handle) where T : UnityEngine.Object
//        //{
//        //    //handle = HANDLE++;

//        //    ////记录Handle与Path的映射关系，用来做释放
//        //    //ResInfo resInfo = XGame.XGameComs.Get<IItemPoolManager>().Pop<ResInfo>();
//        //    //resInfo.assetPath = assetPath;
//        //    //m_dicResInfo.Add(handle, resInfo);

//        //    ////同步加载资源
//        //    //T res = assetLoader.Load<T>(assetPath);
//        //    //return res;

//        //    T res = assetLoader.LoadResSync<T>(assetPath, assetName, out handle);
//        //    return res;
//        //}

//        ///// <summary>
//        ///// 异步加载资源
//        ///// </summary>
//        ///// <typeparam name="T"></typeparam>
//        ///// <param name="assetPath"></param>
//        ///// <param name="assetName"></param>
//        ///// <param name="userData"></param>
//        ///// <param name="callback"></param>
//        ///// <param name="failCallback"></param>
//        ///// <returns></returns>
//        //public uint LoadRes<T>(string assetPath, string assetName, object userData, OnLoadResSucCallback<T> callback, OnLoadResFailCallback failCallback) where T : UnityEngine.Object
//        //{
//        //    //uint handle = HANDLE++;

//        //    //GameObject gameObject = new GameObject();
//        //    //T t = gameObject as T;
//        //    //if(t != null)
//        //    //{

//        //    //}
//        //    ////记录Handle与Path的映射关系，用来做释放
//        //    //ResInfo resInfo = XGame.XGameComs.Get<IItemPoolManager>().Pop<ResInfo>();
//        //    //resInfo.assetPath = assetPath;
//        //    //m_dicResInfo.Add(handle, resInfo);

//        //    //assetLoader.LoadAsync<T>(assetPath, (path, res) => {
//        //    //    ResInfo info;
//        //    //    if (m_dicResInfo.TryGetValue(handle, out info))
//        //    //    {
//        //    //        if (res != null)
//        //    //        {
//        //    //            callback(res, handle, userData);
//        //    //        }
//        //    //        else
//        //    //        {
//        //    //            failCallback(handle, userData);
//        //    //        }
//        //    //    }
//        //    //    else
//        //    //    {
//        //    //        assetLoader.UnLoad(path);
//        //    //    }
//        //    //});
//        //    uint handle = assetLoader.LoadRes<T>(assetPath, assetName, userData, callback, failCallback);
//        //    return handle;

//        //}

//        public void UnloadRes(uint handle)
//        {
//            //ResInfo info;
//            //if(m_dicResInfo.TryGetValue(handle, out info))
//            //{
//            //    m_dicResInfo.Remove(handle);
//            //    XGame.XGameComs.Get<IItemPoolManager>().Push(info);
//            //    assetLoader.UnLoad(info.assetPath);
//            //}
//            //else
//            //{
//            //    UnityEngine.Debug.LogError("ResourceLoaderProxy.UnloadRes Error... ResInfo 没找到。。。handle=" + handle);
//            //}

//            assetLoader.UnloadRes(handle);
//        }
//    }
//}
