using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XGame.Load
{
    /// <summary>
    /// 加载完成的回调
    /// </summary>
    /// <param name="gameObject"></param>
    public delegate void OnLoadFinish(GameObject gameObject);

    /// <summary>
    /// 卸载完成的回调
    /// </summary>
    public delegate void OnUnLoadFinish();

    /// <summary>
    /// 请求回调
    /// </summary>
    /// <param name="resID"></param>
    /// <param name="reqKey"></param>
    /// <param name="userData"></param>
    public delegate void OnResLoadCallback(int resID, int reqKey, int userData = 0);


    /// <summary>
    /// 资源加载接口
    /// </summary>
    public interface IResourcesLoader : IResLoaderSink
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool Create(object p = null);

        /// <summary>
        /// 销毁
        /// </summary>
        void Release();

        /// <summary>
        /// 设置加载器
        /// </summary>
        /// <param name="loader"></param>
        void SetLoader(IResLoader loader);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="callback">callback(nObjID, nKey)</param>
        /// <param name="bInstantiate"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        int LoadRes(string assetPath, OnResLoadCallback callback, bool bInstantiate = false, int userData = 0, int parentLopID = 0, Transform parent = null);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="callback"></param>
        /// <param name="bInstantiate"></param>
        /// <param name="userData"></param>
        void LoadResWithReqKey(int reqKey, string assetPath, OnResLoadCallback callback, bool bInstantiate = false, int userData = 0, int parentLopID = 0, Transform parent = null);
        
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name=""></param>
        void UnloadRes(int nKey, bool bCache = true);

        /// <summary>
        /// 推动加载更新
        /// </summary>
        void Update();
        /// <summary>
        /// 加载完成的全局回调，每加载成功一次回调一次
        /// </summary>
        /// <param name="callback"></param>
        void AddLoadCallBack(OnLoadFinish callback);
        /// <summary>
        /// 移除加载完成回调
        /// </summary>
        /// <param name="callback"></param>
        void RemoveLoadCallBack(OnLoadFinish callback);
        /// <summary>
        /// 卸载完成的全局回调，每卸载一次回调一次
        /// </summary>
        /// <param name="onUnLoadFinish"></param>
        void AddUnLoadCallBack(OnUnLoadFinish onUnLoadFinish);
        /// <summary>
        /// 移除卸载回调
        /// </summary>
        /// <param name="onUnLoadFinish"></param>
        void RemoveUnLoadCallBack(OnUnLoadFinish onUnLoadFinish);
    }
}
