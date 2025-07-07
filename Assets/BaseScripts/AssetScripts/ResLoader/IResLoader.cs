/*******************************************************************
** 文件名: ICommonResLoaderAdapter.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:   郑秀程
** 日  期:    2019/10/12
** 版  本:    1.0
** 描  述:    通用资源加载适配接口，用来处理各种组件与各种加载系统的兼容
** 应  用:               
**
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 通用加载器接口
/// </summary>
namespace XGame.Load
{
    /// <summary>
    /// 适配接口
    /// </summary>
    public interface IResLoaderSink
    {
        /// <summary>
        /// 加载成功回调
        /// </summary>
        void OnLoadResSucess(UnityEngine.Object res, uint handle, int userData);

        /// <summary>
        /// 加载失败的回调
        /// </summary>
        void OnLoadResFail(uint handle, int userData);
    }

    /// <summary>
    /// 适配接口
    /// </summary>
    public interface IResLoader
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        bool Create();

        /// <summary>
        /// 销毁
        /// </summary>
        void Release();

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundle"></param>
        /// <param name="userData"></param>
        /// <param name="bInstantiate"></param>
        /// <param name="copyFromInstance"></param>
        /// <param name="loadSink"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        uint LoadRes<T>(string assetBundle, int userData, bool bInstantiate, bool copyFromInstance, 
            IResLoaderSink loadSink, Transform parent = null) where T : UnityEngine.Object;

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        void UnloadRes(uint handle, bool bCache = true);

        /// <summary>
        /// 资源是否有被缓存
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        bool IsResCaching<T>(string assetName) where T : UnityEngine.Object;

        /// <summary>
        /// 是否资源是否在本地
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        bool IsExistLocalCache(string assetName);

    }
}
