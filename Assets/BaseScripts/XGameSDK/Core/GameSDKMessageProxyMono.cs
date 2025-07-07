/*****************************************************************
** 文件名:	GameSDKManagerCorMono.cs
** 版  权:	(C) 冰川网络网络科技
** 创建人:  郑秀程
** 日  期:	2021/5
** 版  本:	1.0
** 描  述:	游戏SDK管理器
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;

namespace XGame.AssetScript.SDK.Core
{
    /// <summary>
    /// 游戏SDK管理器
    /// </summary>
    class GameSDKMessageProxyMono : MonoBehaviour
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        public static GameSDKMessageProxyMono Instance { get; private set; }

        /// <summary>
        /// 接收消息的方法
        /// </summary>
        public static readonly string RecieveGameSDKMethodName = "OnReceiveGameSDKMessage";

        private void Awake()
        {
            if(Instance != null)
                Debug.LogError("GameSDKManager重复添加，请检查！");

            Instance = this;
        }

        /// <summary>
        /// 收到SDK消息
        /// </summary>
        /// <param name="message"></param>
        protected void OnReceiveGameSDKMessage(string message)
        {
            GameSDKManager.Instance.OnReceiveSDKMessages(message);
        }

        /// <summary>
        /// 更新
        /// </summary>
        private void Update()
        {
            GameSDKManager.Instance.Update();
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        private void OnApplicationQuit()
        {
            GameSDKManager.Instance.DestroyOnApplicationQuit();
        }
    }
}