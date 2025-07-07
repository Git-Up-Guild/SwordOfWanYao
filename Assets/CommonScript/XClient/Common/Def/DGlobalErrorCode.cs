/*******************************************************************
** 文件名:	DGlobalErrorCode.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	昔文博
** 日  期:	2019/1/14
** 版  本:	1.0
** 描  述:	错误码定义
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;

namespace XClient.Common
{
    public enum DGlobalErrorCode : int
    {
        None = 0,

        //公共部分 1-200
        #region Common

        #endregion

        //微端模块 201-300  移到微端组件定义
        //#region MicroClient
        //MicroClientDownloadFailed = 201,     //资源下载失败
        //MicroClientDownloadNotFound,         //资源文件未找到
        //MicroClientDownloadPipeError,        //资源下载管道出错
        //MicroClientVerifyFailed,             //资源校验失败
        //#endregion

        //资源加载相关错误码，范围301-500
        #region AssetManager
        PackageLoadFailed = 301,                    //Package加载失败
        PackageDecryptFailed = 302,                 //Package解密失败
        PackageDownloadFailed = 303,                //Package下载失败
        PackageFileNotReady = 304,                  //Package未准备就绪
        AssetLoadFailed_NoPackage = 401,            //Asset加载失败-所在的Package未正确加载
        AssetLoadFailed_DependencyNotReady = 402,   //Asset加载失败-依赖项未准备好（同步加载时）
        AssetLoadFailed_NoAsset = 403,              //Asset加载失败-Package中没有对应的Asset
        AssetLoadFailed_Canceled = 404,             //Asset加载失败-主动取消加载
        AssetLoadFailed_MultiSubAsset = 405,
        #endregion
    }
}
