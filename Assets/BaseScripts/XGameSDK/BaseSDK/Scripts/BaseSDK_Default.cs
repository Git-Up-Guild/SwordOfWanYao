/*******************************************************************
** 文件名:	XGameBaseSDK_Default.cs
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
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using UnityEngine;
using XGame.AssetScript.SDK.Core;

namespace XGame.AssetScript.SDK.Base
{
    public class BaseSDK_Default : IBaseSDK
    {
        private Action<string, string> m_RequestDeviceIdentifyCallback;

        private string m_AppDataFilePath = $"E:\\{Application.identifier}.AppData";
        private string m_PurchaseRecordsFilePath = $"E:\\{Application.identifier}.PurchaseRecords";

        public string Name => "BaseSDK";

        public bool IsInitialized { get; private set; }

        private Action<int, string> m_MessageCallbacks;

        public void Destroy()
        {
        }

        public bool IsSupport()
        {
            return true;
        }

        public void OnReceiveSDKMessage(int messageID, string messageDetail)
        {
            if (messageID == BaseSDKMessage.DeviceIdentity)
            {
                if (string.IsNullOrEmpty(messageDetail))
                {
                    m_RequestDeviceIdentifyCallback?.Invoke("NONE", string.Empty);
                }
                else
                {
                    string[] strs = messageDetail.Split('#');
                    string idType = strs[0];
                    string deviceID = strs.Length >= 2 ? strs[1] : string.Empty;
                    if (!string.IsNullOrEmpty(deviceID))
                        deviceID = SDKUtility.ComputeStringMD5("XGame_" + deviceID);
                    m_RequestDeviceIdentifyCallback?.Invoke(idType, deviceID);
                }
                m_RequestDeviceIdentifyCallback = null;
            }

            m_MessageCallbacks?.Invoke(messageID, messageDetail);
        }

        public bool HasNotchInScreen()
        {
            return false;
        }

        public bool Initialize(string unityObjName, string unityHandleFuncName)
        {
            IsInitialized = true;
            return IsInitialized;
        }

        public void Update()
        {
        }

        public void RestartApp()
        {
        }
        public void VibratorApp(long delay,long time)
        {
        }

        public void RequestDeviceIdentify(Action<string, string> callback)
        {
            m_RequestDeviceIdentifyCallback = callback;

            List<string> lsMacs = GetMacByNetworkInterface();
            if(lsMacs.Count > 0)
            {
                OnReceiveSDKMessage(BaseSDKMessage.DeviceIdentity, "MAC#" + lsMacs[0]);
            }
            else
            {
                OnReceiveSDKMessage(BaseSDKMessage.DeviceIdentity, "NONE#");
            }
        }

        ///<summary>
        /// 通过NetworkInterface读取网卡Mac
        ///</summary>
        ///<returns></returns>
        private static List<string> GetMacByNetworkInterface()
        {
            List<string> macs = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                macs.Add(ni.GetPhysicalAddress().ToString());
            }
            return macs;
        }

        public void SetDebug(bool isDebug)
        {
        }

        public RectOffset GetNotchScreenPadding()
        {
            return new RectOffset(0, 0, 0, 0);
        }

        public void SetDisabledLockScreen()
        {
        }

        private string GetApplicationDataSaveFilePath(string key)
        {
            return $"E:\\{Application.identifier}.{key}";
        }

        public void SaveApplicationData(string data)
        {
            File.WriteAllText(m_AppDataFilePath, data);
        }

        public void LoadApplicationData(Action<string> callbak)
        {
            string appUserData = string.Empty;

            if (File.Exists(m_AppDataFilePath))
                appUserData = File.ReadAllText(m_AppDataFilePath);
            callbak?.Invoke(appUserData);

            OnReceiveSDKMessage(BaseSDKMessage.AppUserData, appUserData);
        }

        public string GetName()
        {
            return "BaseSDK";
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

        public void LoadApplicationDataWithKey(string key, Action<string> callback)
        {
            string records = string.Empty;
            var filePath = GetApplicationDataSaveFilePath(key);
            if (File.Exists(filePath))
                records = File.ReadAllText(m_PurchaseRecordsFilePath);
            callback?.Invoke(records);

            OnReceiveSDKMessage(BaseSDKMessage.PurchaseRecords, records);
        }

        public void SaveApplicationDataWithKey(string key, string data)
        {
            var filePath = GetApplicationDataSaveFilePath(key);
            File.WriteAllText(filePath, data);
        }
    }
}