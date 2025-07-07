/*****************************************************************
** 文件名:	GameSDKManager.cs
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

using System;
using System.Collections.Generic;
using UnityEngine;


namespace XGame.AssetScript.SDK.Core
{
    /// <summary>
    /// 游戏SDK管理器
    /// </summary>
    public class GameSDKManager
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        private static GameSDKManager s_Instance;

        /// <summary>
        /// 关联的GameObject对象
        /// </summary>
        private GameObject m_ProxyGameObject;

        /// <summary>
        /// 消息接收Mono
        /// </summary>
        private GameSDKMessageProxyMono m_ProxyMessageReceiver;

        /// <summary>
        /// SDK列表
        /// </summary>
        private Dictionary<Type, ISDK> m_DicSDK = new Dictionary<Type, ISDK>();

        /// <summary>
        /// 管理器实例
        /// </summary>
        public static GameSDKManager Instance
        {
            get 
            { 
                if(s_Instance == null)
                {
                    s_Instance = new GameSDKManager();
                    s_Instance.m_ProxyGameObject = new GameObject("[GameSDKMessageReceiver]");
                    //s_Instance.m_ProxyGameObject.hideFlags = HideFlags.HideAndDontSave;
                    s_Instance.m_ProxyMessageReceiver = s_Instance.m_ProxyGameObject.AddComponent<GameSDKMessageProxyMono>();
                }

                if(s_Instance.m_ProxyGameObject == null)
                {
                    Error("GameSDKManager 已经销毁！！");
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// 获取SDK对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISDK GetSDK<T>()
        {
            var t = typeof(T);
            ISDK sdk;
            if (m_DicSDK.TryGetValue(t, out sdk))
                return sdk;
            return null;
        }

        /// <summary>
        /// 注册SDK
        /// </summary>
        /// <param name="sdk"></param>
        public ISDK RegisterSDK<T>() where T : ISDK, new()
        {
            var t = typeof(T);
            ISDK sdk;
            if (m_DicSDK.TryGetValue(t, out sdk))
                return sdk;

            sdk = new T();
            m_DicSDK.Add(t, sdk);

            //进行初始化操作
            bool bRet = sdk.Initialize(m_ProxyMessageReceiver.name, GameSDKMessageProxyMono.RecieveGameSDKMethodName);
            if(!bRet)
                Error("初始化SDK失败，name=" + sdk.Name);
            else
                Log("初始化SDK成功，name=" + sdk.Name);

            return sdk;
        }

        /// <summary>
        /// 注销SDK，同时SDK对象会被销毁
        /// </summary>
        /// <param name="sdk"></param>
        public void UnregisterSDK(ISDK sdk)
        {
            if (sdk == null)
                return;

            var t = sdk.GetType();
            if (m_DicSDK.ContainsKey(t))
            {
                m_DicSDK.Remove(t);
                sdk.Destroy();
            }
        }

        /// <summary>
        /// 注销SDK，同时SDK对象会被销毁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnregisterSDK<T>() where T : ISDK
        {
            var t = typeof(T);
            if (m_DicSDK.TryGetValue(t, out ISDK sdk))
            {
                m_DicSDK.Remove(t);
                sdk.Destroy();
            }
        }

        /// <summary>
        /// 收到SDK消息
        /// </summary>
        /// <param name="content"></param>
        public void OnReceiveSDKMessages( string content)
        {
            if(string.IsNullOrEmpty(content))
            {
                Error("无效的消息, content is empty.");
                return;
            }

            string[] arrTemp = content.Split("#");
            if(arrTemp.Length < 2)
            {
                Error("无效的消息, content=" + content);
                return;
            }

            string sdkName = arrTemp[0];
            if(string.IsNullOrEmpty(sdkName))
            {
                Error("无效的SDK名称, content=" + content);
                return;
            }

            ISDK sdk = null;
            foreach(var s in m_DicSDK.Values)
            {
                if(s.Name == sdkName)
                {
                    sdk = s;
                    break;
                }
            }

            if (sdk == null)
            {
                Error("找不到对应的SDK, content=" + content);
                return;
            }

            int messageID = 0;
            try
            {
                messageID = Convert.ToInt32(arrTemp[1]);
            }
            catch(Exception e)
            {
                Error("消息ID格式错误, content=" + content + ", err=" + e.Message);
            }

            if (messageID == 0)
            {
                Error("消息ID无效, content=" + content);
                return;
            }

            //消息内容
            string messageDetail = content.Substring(arrTemp[0].Length + arrTemp[1].Length + 2);

            //转发至对应的sdk对象
            sdk.OnReceiveSDKMessage(messageID, messageDetail);
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            foreach (var s in m_DicSDK.Values)
            {
                if (s.IsSupport())
                {
                    s.Update();
                }
            }
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public void DestroyOnApplicationQuit()
        {
            Log("游戏退出， GameSDKManager 销毁");

            foreach (var s in m_DicSDK.Values)
            {
                s.Destroy();
            }

            m_DicSDK.Clear();
            m_ProxyGameObject = null;
            m_ProxyMessageReceiver = null;
        }

        public static void Log(string log)
        {
            Debug.Log($"[SDK] {log}");
        }

        public static void Error(string log)
        {
            Debug.LogError($"[SDK] {log}");
        }
    }
}