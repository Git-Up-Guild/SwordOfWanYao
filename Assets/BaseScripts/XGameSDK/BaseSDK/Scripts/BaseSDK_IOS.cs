/*******************************************************************
** 文件名:	XGameBaseSDK_IOS.cs
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
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using XGame.AssetScript.SDK.Core;

namespace XGame.AssetScript.SDK.Base
{
    public class BaseSDK_IOS : IBaseSDK
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern string iosBaseSDK_GetSafeAreaInsets();

        [DllImport("__Internal")]
        private static extern string iosBaseSDK_GenerateDeviceID();

        [DllImport("__Internal")]
        private static extern void iosBaseSDK_VibratorApp(long delay, long time);

        [DllImport("__Internal")]
        private static extern void iosBaseSDK_SetIdleTimerDisabled();

        [DllImport("__Internal")]
        private static extern void iosBaseSDK_ExitApplication();

        //保存应用数据
        [DllImport("__Internal")]
        private static extern void iosBaseSDK_SaveAplicationData(string data);

        //获取应用数据
        [DllImport("__Internal")]
        private static extern string iosBaseSDK_LoadAplicationData();

        //加载交易记录
        [DllImport("__Internal")]
        private static extern string iosBaseSDK_LoadAplicationDataWithKey(string key);

        //保存交易记录
        [DllImport("__Internal")]
        private static extern void iosBaseSDK_SaveAplicationDataWithKey(string key, string data);
#endif

        [System.Serializable]
        public class SafeAreaInsets
        {
            public float top;
            public float bottom;
            public float left;
            public float right;

            public override string ToString()
            {
                return $"top:{top}, bottom:{bottom}, left:{left}, right:{right}";
            }
        }

        public string Name => "BaseSDK";
        public bool IsInitialized { get; private set; }

        private Action<int, string> m_MessageCallbacks;

        public bool Initialize( string unityObjName, string unityHandleFuncName)
        {
            SetDisabledLockScreen();
            IsInitialized = true;
            return IsInitialized;
        }

        public void Destroy()
        {
            m_MessageCallbacks = null;
        }


        public bool IsSupport()
        {
            return true;
        }

        public void OnReceiveSDKMessage(int messageID, string messageDetail)
        {
            m_MessageCallbacks?.Invoke(messageID, messageDetail);
        }

        public bool HasNotchInScreen()
        {
            return true;
        }

        public void Update()
        {
        }

        public void RestartApp()
        {
#if UNITY_IPHONE
             iosBaseSDK_ExitApplication();
#endif
        }

        public void VibratorApp(long delay, long time)
        {
#if UNITY_IPHONE
             iosBaseSDK_VibratorApp(delay,time);
#endif
        }

        public void SetDisabledLockScreen()
        {
#if UNITY_IPHONE
             iosBaseSDK_SetIdleTimerDisabled();
#endif
        }


        public void RequestDeviceIdentify(Action<string, string> callback)
        {
            string deviceID = string.Empty;
            string type = string.Empty;

#if UNITY_IOS
            deviceID = iosBaseSDK_GenerateDeviceID();
            type = "IOS_UUID";
#else
            deviceID = Guid.NewGuid().ToString("N");
            type = "IOS_TEST";
#endif
            deviceID = SDKUtility.ComputeStringMD5(deviceID);
            callback(type, deviceID);
        }

        public void SetDebug(bool isDebug)
        {
        }

        public RectOffset GetNotchScreenPadding()
        {
//            SafeAreaInsets safeAreaInsets = null;
//#if UNITY_IOS
//            string str = iosBaseSDK_GetSafeAreaInsets();
//            safeAreaInsets = JsonUtility.FromJson<SafeAreaInsets>(str);
//#else
//            safeAreaInsets = new SafeAreaInsets();
//#endif

            var safeArea = Screen.safeArea;

            Debug.Log($"IOS safeArea: {safeArea}");

            var padding = new RectOffset();
            padding.left = (int)safeArea.xMin;
            padding.top = (int)safeArea.yMin;
            padding.right = Screen.width - (int)safeArea.xMax;

            if (Screen.height > safeArea.yMax)
                padding.bottom = 50;     
            else
                padding.bottom = 0;

            return padding;
        }

        public void SaveApplicationData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

#if UNITY_IPHONE
            iosBaseSDK_SaveAplicationData(data);
#endif
        }

        public void LoadApplicationData(Action<string> callback)
        {
#if UNITY_IPHONE
            string data = iosBaseSDK_LoadAplicationData();
            callback?.Invoke(data);
#else
            callback?.Invoke(string.Empty);
#endif
        }


        public void AddSDKMessageCallback(Action<int, string> callback)
        {
            m_MessageCallbacks -= callback;
            m_MessageCallbacks += callback;
        }

        public void RemoveSDKMessageCallback(Action<int, string> callback)
        {
            m_MessageCallbacks -= callback;
        }

        private string GetApplicationDataSaveKey(string key)
        {
            return $"XGame_IOS_AppData_{key}";
        }

        public void LoadApplicationDataWithKey(string key, Action<string> callback)
        {
            if (string.IsNullOrEmpty(key))
                return;

            string saveKey = GetApplicationDataSaveKey(key);

#if UNITY_IPHONE
            string data = iosBaseSDK_LoadAplicationDataWithKey(saveKey);
            callback?.Invoke(data);
#else
            callback?.Invoke(string.Empty);
#endif
        }

        public void SaveApplicationDataWithKey(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
                return;

#if UNITY_IPHONE
            string saveKey = GetApplicationDataSaveKey(key);
            iosBaseSDK_SaveAplicationDataWithKey(saveKey, data);
#endif
        }
    }
}