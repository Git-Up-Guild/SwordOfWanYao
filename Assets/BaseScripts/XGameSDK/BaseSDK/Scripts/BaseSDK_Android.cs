/*******************************************************************
** 文件名:	XGameBaseSDK__Android.cs
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
using UnityEngine;
using XGame.AssetScript.SDK.Core;

namespace XGame.AssetScript.SDK.Base
{
    public class BaseSDK_Android : SDK_Android, IBaseSDK
    {
        private AndroidJavaClass m_SDKClass;
        private Action<string, string> m_RequestDeviceIdentifyCallback;
        private Action<string> m_LoadAppUserDataCallback;

        public override string Name => "BaseSDK";

        protected override void OnDestroy()
        {
            if(m_SDKClass != null)
            {
                m_SDKClass.Dispose();
                m_SDKClass = null;
            }
        }

        protected override bool OnInitialize()
        {
            m_SDKClass = new AndroidJavaClass("com.xgame.plugin.base.BaseSDK");
            if (m_SDKClass == null)
            {
                Debug.LogError("初始化BaseSDK失败，找不到 com.xgame.plugin.base.BaseSDK 类！");
                IsInitialized = false;
                return false;
            }

            bool bRet = m_SDKClass.CallStatic<bool>("init", new object[] { unityReceiver, unityReceiverFuncName });
            if(!bRet)
            {
                IsInitialized = false;
                Debug.LogError("初始化BaseSDK失败, com.xgame.plugin.base.BaseSDK.init 执行失败！");
                m_SDKClass.Dispose();
                m_SDKClass = null;
                return false;
            }

            IsInitialized = true;
            return true;
        }

        public override bool IsSupport()
        {
            return true;
        }

        public override void OnReceiveSDKMessage(int messageID, string messageDetail)
        {
            if(messageID == BaseSDKMessage.DeviceIdentity)
            {
                if(string.IsNullOrEmpty(messageDetail))
                {
                    m_RequestDeviceIdentifyCallback?.Invoke("NONE", string.Empty);
                }
                else
                {
                    string[] strs = messageDetail.Split('#');
                    string idType = strs[0];
                    string deviceID = strs.Length >= 2 ? strs[1] : string.Empty;
                    if(!string.IsNullOrEmpty(deviceID))
                        deviceID = SDKUtility.ComputeStringMD5("XGame_" + deviceID);
                    m_RequestDeviceIdentifyCallback?.Invoke(idType, deviceID);
                }
                m_RequestDeviceIdentifyCallback = null;
            }
            else if(messageID == BaseSDKMessage.AppUserData)
            {
                m_LoadAppUserDataCallback?.Invoke(messageDetail);
                m_LoadAppUserDataCallback = null;
            }

            InvokeSDKMessageCallbacks(messageID, messageDetail);
        }

        public bool HasNotchInScreen()
        {
            if (m_SDKClass != null)
            {
                bool bRet = false;
                try
                {
                    bRet = m_SDKClass.CallStatic<bool>("hasNotchInScreen");
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
                return bRet;
            }
            else
            {
                Debug.LogError("m_SDKClass is null. 无法判断是否为刘海屏！");
                return true;
            }
        }

        public override void Update()
        {
        }

        public void RestartApp()
        {
            if (m_SDKClass != null)
            {
                try
                {
                    Debug.Log("请求重启App");
                    m_SDKClass.CallStatic("restartApp", 0);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                Debug.LogError("m_SDKClass is null. 无法重启App！");
            }
        }

        public void  VibratorApp(long delay, long time)
        {
            if (m_SDKClass != null)
            {
                try
                {
                    m_SDKClass.CallStatic("vibratorApp", delay,time);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                Debug.LogError("m_SDKClass is null. 震动App！");
            }
        }

        public void RequestDeviceIdentify(Action<string, string> callback)
        {
            if (m_SDKClass != null)
            {
                try
                {
                    m_RequestDeviceIdentifyCallback = callback;
                    m_SDKClass.CallStatic("requestDeviceIdentify");
                }
                catch (Exception e)
                {
                    Debug.LogError("获取设备唯一标识失败, " + e.Message);
                    callback("NONE", string.Empty);
                }
            }
            else
            {
                Debug.LogError("m_SDKClass is null. 获取设备唯一标识失败！");
                callback("NONE", string.Empty);
            }
        }

        public override void SetDebug(bool isDebug)
        {
            if (m_SDKClass != null)
            {
                try
                {
                    if(isDebug)
                        m_SDKClass.CallStatic("enableDebug");
                    else
                        m_SDKClass.CallStatic("disableDebug");
                }
                catch (Exception e)
                {
                    Debug.LogError("调用 enableDebug 失败：" + e.Message);
                }
            }
            else
            {
                Debug.LogError("调用enableDebug 失败：m_SDKClass is null.");
            }
        }

        public RectOffset GetNotchScreenPadding()
        {
            return new RectOffset(0, 0, 0, 0);
        }

        private string GetApplicationDataSaveKey(string key)
        {
            return $"XGame_AppData_{key}";
        }

        public void SaveApplicationData(string data)
        {
            if (m_SDKClass != null)
            {
                try
                {
                    m_SDKClass.CallStatic("saveApplicationData", new object[] { data });
                }
                catch (Exception e)
                {
                    Debug.LogError("保存应用用户数据失败, " + e.Message);
                }
            }
            else
            {
                Debug.LogError("m_SDKClass is null. 保存应用用户数据失败！");
            }
        }

        public void LoadApplicationData(Action<string> callback)
        {
            if (m_SDKClass != null)
            {
                try
                {
                    m_LoadAppUserDataCallback = callback;
                    m_SDKClass.CallStatic("loadApplicationData");
                }
                catch (Exception e)
                {
                    Debug.LogError("获取应用用户数据失败, " + e.Message);
                    m_LoadAppUserDataCallback(string.Empty);
                }
            }
            else
            {
                Debug.LogError("m_SDKClass is null. 获取应用用户数据失败！");
                m_LoadAppUserDataCallback(string.Empty);
            }
        }

        public void LoadApplicationDataWithKey(string key, Action<string> callback)
        {
            string saveKey = GetApplicationDataSaveKey(key);
            string data = PlayerPrefs.GetString(saveKey);
            Debug.Log($"加载 {key} 完成: data: {data}");
            callback?.Invoke(data);
        }

        public void SaveApplicationDataWithKey(string key, string data)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(key))
                return;

            string saveKey = GetApplicationDataSaveKey(key);
            PlayerPrefs.SetString(saveKey, data);
            PlayerPrefs.Save();

            Debug.Log($"保存 {key} 成功: data: {data}");
        }
    }
}