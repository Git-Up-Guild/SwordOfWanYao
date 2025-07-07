
using cgpol;
using gamepol;
using ORM;
using XClient.Common;
using System;
using System.Collections.Generic;
using XGame.Asset.Load;
using UnityEngine;
using XClient.Common.VirtualServer;
using XGame.Net;
using XGame.UI.Framework.Box;
using XClient.Login.State;
using XGame.Ini;
using System.Net;
using XGame.FrameUpdate;

namespace XClient.Client
{
    public class NetModule : INetModule, ICheckNetworkSink, IGameNetComSink
    {
        IVirtualServer virtualServer = null;
        
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private Action<IGamePackProcess> m_OnReceive;
        private IGameNetCom m_netCom;

        public IGameNetCom GetNetCom() { return m_netCom; }

        private NetHeartBeatChecker m_HeartBeatChecker = null;

        public NetModule(object context = null)
        {
            m_netCom = new GameNetCom();
            m_netCom.Create(this, context);
            m_netCom.SetSinkObj(this);
            //NetCom.SetNetRecv(OnMessage);
            
            virtualServer = new VirtualServer();
            virtualServer.Initialize();
        }

        public bool Create(object context, object config = null)
        {
            IFrameUpdateManager frameMgr = XGame.XGameComs.Get<XGame.FrameUpdate.IFrameUpdateManager>();
            frameMgr?.RegFixUpdateCallback(FixedUpdate, "NetModule.FixedUpdate");
            frameMgr?.RegLateUpdateCallback(LateUpdate, "NetModule.LateUpdate");

            //心跳检查
            m_HeartBeatChecker = new NetHeartBeatChecker();
            m_HeartBeatChecker.Create(this);

            return true;
        }

        #region 游戏接口
        uint m_Latency;
        public void OnReceiveMessage(IGamePackProcess gamePackProcess)
        {
            uint nMsgID = gamePackProcess.GetMsgID();
            //C#解包&&网关协议
            if (nMsgID > 0 && gamePackProcess.GetSrcEndPoint() != NetDefine.ENDPOINT_NORMAL)
            {
                switch (nMsgID)
                {
                    case cgpol.TCGMessage.MSG_GATEWAY_HEART_CHECK_REQ://心跳
                        TCGMessage msg = GetGatewayMsg(false);
                        cgpol.TMSG_GATEWAY_HEART_CHECK_REQ body = msg.stTMSG_GATEWAY_HEART_CHECK_REQ;
                        if (body != null)
                        {
                            m_Latency = body.get_dwAvgLatency();
                        }

                        //Debug.Log("心跳检查！");

                        break;

                    default:
                        break;
                }
            }

            if (m_OnReceive != null)
            {
                m_OnReceive(gamePackProcess);
            }

        }

        public uint GetLatency()
        {
            return m_Latency;
        }
        #endregion

        public bool Start()
        {
            SendMessageHandlerInit();
            virtualServer.Start();
            
            //this.SetNetConnect(OnNetConnect);
            return true;
        }

        public void FixedUpdate()
        {
            if (m_netCom != null)
            {
                m_netCom.Update();
            }
        }

        public void LateUpdate()
        {
            if (m_netCom != null)
            {
                m_netCom.FlushSend();
            }
        }

        public void Release()
        {
            virtualServer.Stop();
            XGame.XGameComs.Get<XGame.FrameUpdate.IFrameUpdateManager>()?.UnregFixUpdateCallback(FixedUpdate);
            if (m_netCom != null)
            {
                m_netCom.Disconnect();
            }

            if (m_HeartBeatChecker != null)
            {
                m_HeartBeatChecker.Release();
                m_HeartBeatChecker = null;
            }
        }

        public void Stop()
        {
            m_netCom.SetSinkObj(null);
            //this.SetNetConnect(null);
        }

        public void Update()
        {
        }

        private void OnNetConnect(uint errId, uint reason)
        {
            if (errId != 0)
            {
                Debug.LogError($"[NetModule]连接错误 errId[{errId}] reason[{(SOCKET_ERROR)reason}]");
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_NET_CONNECT_ERROR, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            }
        }
        
        #region 新版本网络接口

        public IGameNetCom NetCom
        {
            get
            {
                return m_netCom;
            }
        }

        /// <summary>
        /// 接收消息，并将消息转发出去
        /// </summary>
        /// <param name="msg"></param>
        public void OnReceiveMsg(gamepol.TCSMessage msg)
        {
            NetCom.OnReceiveMsg(msg);
        }

        public void AddMessageHandler(uint nMsgID, OnMessageSink onMessageSink, string desc)
        {
            NetCom.AddMessageEvent(nMsgID, onMessageSink, desc);
        }

        public void AddMessageHandler(uint nMsgID, OnMessageAction action, string desc)
        {
            NetCom.AddMessageEvent(nMsgID, action, desc);
        }

        public void RemoveMessageHandler(uint nMsgID, OnMessageSink onMessageSink)
        {
            NetCom.RemoveMessageEvent(nMsgID, onMessageSink);
        }

        public void RemoveMessageHandler(uint nMsgID, OnMessageAction action)
        {
            NetCom.RemoveMessageEvent(nMsgID, action);
        }
        
        public void SendMessage_CS(byte srcEndPoint, byte dstEndPoint, gamepol.TCSMessage msg)
        {
            this.m_fnSendMessage(srcEndPoint, dstEndPoint, msg);
        }

        /// <summary>
        /// 发送网关消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(TCGMessage msg)
        {
            NetCom.SendMessage(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_GATEWAY, msg.stHead.get_iMsgID());
        }

        private string m_ip;
        private int m_port;
        private int m_connectType;
        public bool Connect(string ip, int port, int type)
        {
            //Debug.Log("Connect");
            //这里有个假设, 就是我们只连接网关服
            m_ip = ip;
            m_port = port;
            m_connectType = type;
            return NetCom.Connect(ref ip, port, type);
        }

        public void Disconnect()
        {
            NetCom.Disconnect();
        }

        public gamepol.TCSMessage GetGameMsg(bool bSendFlag)
        {
            return NetCom.GetGameMsg(bSendFlag);
        }

        public cgpol.TCGMessage GetGatewayMsg(bool bSendFlag)
        {
            return NetCom.GetGatewayMsg(bSendFlag);
        }

        public bool IsConnected()
        {
            return NetCom.IsConnected();
        }

        public void SetNetRecv(Action<IGamePackProcess> func)
        {
            m_OnReceive = func;
        }
#endregion

        public void OnReceive(byte[] data, int nLen, int srcEndPoint)
        {
            NetCom.OnReceive(data, nLen, srcEndPoint);
        }

        public Dictionary<string, int> GetNetNodeInfo()
        {
            return NetCom.GetNetNodeInfo();
        }
        public Dictionary<string, uint> GetCallInfoCount()
        {
            return NetCom.GetCallInfoCount();
        }

    #if !UNITY_WEBGL
        private Ping m_lastPing = null;
    #endif

        private float m_lastConnectTime = 0;
        private float m_pingInterval = 5.0f;
        private bool m_lastConnect = true;

        public bool IsConnectInternet()
        {
            //有连接，网络肯定是通的
            if (null == NetCom || NetCom.IsConnected())
            {
                m_lastConnect = true;
                m_lastConnectTime = Time.realtimeSinceStartup;

#if !UNITY_WEBGL
                if (null != m_lastPing)
                {
                    m_lastPing.DestroyPing();
                    m_lastPing = null;
                }
#endif
                return true;
            }

            //小于断网间隔的，直接当还有链接
            float curTime = Time.realtimeSinceStartup;
            if (curTime - m_lastConnectTime < m_pingInterval)
            {
                return m_lastConnect;
            }

#if !UNITY_WEBGL
            //没有ping的
            if (m_lastPing == null)
            {
                //没有IP，直接返回断网
                string ip = m_netCom.GetConnectIP();
                if (string.IsNullOrEmpty(ip))
                {
                    return false;
                }

                m_lastPing = new Ping(ip);
            }
            else
            {
                //ping还没有完成。直接当作是通的
                if (m_lastPing.isDone)
                {
                    m_lastConnectTime = curTime;
                    //网络能够ping通
                    m_lastConnect = m_lastPing.time > 0;
                    m_lastPing.DestroyPing();
                    m_lastPing = null;
                }
                else
                {
                    m_lastConnect = false;

                    //2倍间隔时间都没有完成ping，那应该是不通了
                    if (curTime - m_lastConnectTime > 2 * m_pingInterval)
                    {
                        //5s后重来
                        m_lastConnectTime = curTime;
                        if (null != m_lastPing)
                        {
                            m_lastPing.DestroyPing();
                            m_lastPing = null;
                        }
                    }
                }
            }
#endif
            return m_lastConnect;
        }

        public float GetLastActiveTime()
        {
            if (m_netCom == null)
                return 0f;
            return m_netCom.GetLastActiveTime();
        }

        public float GetLastReceiveTime()
        {
            if (m_netCom == null)
                return 0f;
            return m_netCom.GetLastReceiveTime();
        }

        public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }

        public void AddGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage action, string desc)
        {
            m_netCom.AddGatewayMessageHandler(nMsgID, action, desc);
        }

        public void RemoveGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage action)
        {
            m_netCom.RemoveGatewayMessageHandler(nMsgID, action);
        }

        public TCSMessage GetAndInitGameMsg(bool bSendFlag, uint msgID)
        {
            var msg = GetGameMsg(bSendFlag);
            msg.Init(msgID);
            return msg;
        }
        
        //装饰协议
        private Action<byte, byte, TCSMessage> m_fnSendMessage = null;

        public void SendMessageHandlerInit()
        {
            m_fnSendMessage = SendMessage_CS_Backup;
        }

        public void SetSendMessageHandler(Action<byte, byte, TCSMessage> sendMessageCs)
        {
            m_fnSendMessage = sendMessageCs;
        }
        
        public void RestoreSendMessage()
        {
            m_fnSendMessage = SendMessage_CS_Backup;
        }

        public void SendMessage_CS_Backup(byte srcEndPoint, byte dstEndPoint, gamepol.TCSMessage msg)
        {
            NetCom.SendMessage(srcEndPoint, dstEndPoint, msg);
        }

        public void DispatchGameMessage(uint getIMsgID, TCSMessage sendObj)
        {
            m_netCom.FireMessage(getIMsgID, sendObj);
        }

        private int m_connectRetryCount = 0;
        private int m_connectRetryCountMax = 5;
        private int m_connectCount = 0;

        public void OnConnectStart()
        {

        }
        private bool m_isKick = false;
        public void SetKick(bool kick)
        { 
            m_isKick = kick;
        }

        public bool GetKick()
        {
            return m_isKick;
        }
        public void ReConnect(bool resetRetryCount = true)
        {
            //if (resetRetryCount)
            //    m_connectRetryCount = 0;
            GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_RECONNECT, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
        }


        private bool m_firstConnected = false;
        public void OnConnectSuccess()
        {
            Debug.Log($"OnConnectSuccess");
            //m_connectRetryCount = 0;
            m_connectCount++;
            m_firstConnected = true;
            //先触发逻辑层的清理
            GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_CLEAR, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            //然后请module
            GameGlobal.Instance.Clear(0);
        }
        public void SetFirstConnected(bool first = false)
        {
            m_firstConnected = first;
        }

        public void OnConnectFail(EnNetConnFailType type, int errcode1, uint reason)
        {
            if (m_isKick)
                return;
            Debug.LogError($"OnConnectFail {type} code:{errcode1} reason:{reason} isKick:{m_isKick}");
            if (type == EnNetConnFailType.ManualDisconn)
                return;
            m_connectRetryCount++;
            m_connectCount++;
            //已经连接过
            if (m_firstConnected && (m_connectRetryCount%5) != 0)
            {
                //Connect(m_ip, m_port, m_connectType);
                GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_RECONNECT, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            }
            else
            {
                ShowReconnectMessageBox();
            }
            m_firstConnected = true;

        }

        public void ShowReconnectMessageBox()
        {
            MessageBoxShareData.Instance.Reset();
            MessageBoxShareData.Instance.title = "网络异常";
            MessageBoxShareData.Instance.content = "点击确定重连？";
            MessageBoxShareData.Instance.style = MessageBoxStyle.YesNo;
            MessageBoxShareData.Instance.callback = (isOK, data) =>
            {
                if (isOK)
                {
                    ReConnect();
                    //m_connectRetryCount = 0;
                    ////Connect(m_ip, m_port, m_connectType);
                    //GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_RECONNECT, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
                }
                else
                {
                    m_connectRetryCount = 0;
                    //返回登录界面
                    GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_RETURN_LOGIN, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);

                }
            };
            if (GameIni.Instance.enableDebug)
            {
                Debug.Log($"OnConnectFail Show");
            }
            Disconnect();
            MessageBoxManager.Instance.ShowBox(MessageBoxShareData.Instance);
        }

        public void OnDisconnect(EnNetDisconnCode type, int code, uint reason)
        {
            //Debug.Log($"OnDisconnect");
        }
        public void OnLoginFail(EnNetConnFailType type, EnNetLoginFailType errcode1, uint reason)
        {
            OnConnectFail(type, (int)errcode1, reason);
        }

        public void Clear(int param)
        {

        }

        public void OnRoleDataReady()
        {
        }
    }
}
