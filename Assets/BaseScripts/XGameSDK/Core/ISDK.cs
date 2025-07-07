/*******************************************************************
** 文件名:	ISDK.cs
** 版  权:	(C) 冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2020.10.23
** 版  本:	1.0
** 描  述:	
** 应  用:  SDK

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;

namespace XGame.AssetScript.SDK.Core
{
    public interface ISDK
    {
        /// <summary>
        /// 名称
        /// </summary>
        /// <returns></returns>
        string Name { get; }

        /// <summary>
        /// 是否自持
        /// </summary>
        /// <returns></returns>
        bool IsSupport();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="unityObjName"></param>
        /// <param name="unityHandleFuncName"></param>
        /// <returns></returns>
        bool Initialize(string unityObjName, string unityHandleFuncName);

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 设置调试模式
        /// </summary>
        /// <param name="isDebug"></param>
        void SetDebug(bool isDebug);

        /// <summary>
        /// 更新
        /// </summary>
        void Update();

        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();

        /// <summary>
        /// 接收SDK消息
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="messageDetail"></param>
        void OnReceiveSDKMessage(int messageID, string messageDetail);

        /// <summary>
        /// 注册消息回调
        /// </summary>
        /// <param name="callback"></param>
        void AddSDKMessageCallback(Action<int, string> callback);

        /// <summary>
        /// 注销消息回调
        /// </summary>
        /// <param name="callback"></param>
        void RemoveSDKMessageCallback(Action<int, string> callback);

    }
}