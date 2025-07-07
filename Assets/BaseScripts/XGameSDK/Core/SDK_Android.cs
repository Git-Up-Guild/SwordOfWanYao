/*******************************************************************
** 文件名:	SDK_Android.cs
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
    public abstract class SDK_Android : ISDK
    {
        //接收者名称
        protected string unityReceiver;

        //接接收者处理函数
        protected string unityReceiverFuncName;

        public abstract string Name { get; }
        public bool IsInitialized { get; protected set; }

        private Action<int, string> m_MessageCallbacks;

        public void Destroy()
        {
            OnDestroy();
            m_MessageCallbacks = null;
        }

        public bool Initialize(string unityObjName, string unityHandleFuncName)
        {
            unityReceiver = unityObjName;
            unityReceiverFuncName = unityHandleFuncName;
            return OnInitialize();
        }

        public virtual bool IsSupport()
        {
            return true;
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

        protected void InvokeSDKMessageCallbacks(int messageID, string messageDetail)
        {
            m_MessageCallbacks?.Invoke(messageID, messageDetail);
        }

        protected abstract void OnDestroy();
        protected abstract bool OnInitialize();
        public abstract void OnReceiveSDKMessage(int messageID, string messageDetail);
        public abstract void Update();
        public abstract void SetDebug(bool isDebug);
        
    }
}