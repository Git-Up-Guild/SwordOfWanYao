using System.Collections.Generic;
using UnityEngine;
using XGame.NetCom;
using XGame.Net;
using XGame.Poolable;
using cgpol;

namespace XClient.Common
{
    public class GameNetCom : IGameNetCom, INetComSink
    {
        enum GatewayMessageOp
        {
            Normal,
            Remove,
            Add,
        }

        class GatewayMessageHandlerInfo : LitePoolableObject
        {
            public uint msgID;
            public OnHandleGatewayMessage handler;
            public bool isValid = true;
            public GatewayMessageOp op = GatewayMessageOp.Normal;

            protected override void OnRecycle()
            {
                handler = null;
                isValid = true;
                op = GatewayMessageOp.Normal;
            }
        }

        INetCom m_netCom;

        static CSPackProcess m_cSPackProcess;//C#解包器

        //网络数据
        Dictionary<uint, MessageSinkEvent> m_msgEventDict;

        private List<GatewayMessageHandlerInfo> m_DelayOpGatewayHanlerInfos;
        private bool m_IsFiringGatewayMessage = false;
        private Dictionary<uint, Dictionary<OnHandleGatewayMessage, GatewayMessageHandlerInfo>> m_GatewayMessageHandlers;

#if UNITY_EDITOR
        Dictionary<uint, uint> m_msgRecives = new Dictionary<uint, uint>();
#endif

        public int ID { get; set; }

        private INetMonitor monitorSink;
        public bool Create(object context, object config = null)
        {
            monitorSink = config as INetMonitor;

            NetComSettings.gatewayPointID = NetDefine.ENDPOINT_GATEWAY;
            m_netCom = NetCom.CreateCom();

            m_msgEventDict = new Dictionary<uint, MessageSinkEvent>();
            m_isConnectSuccess = false;

            m_cSPackProcess = new CSPackProcess();
            m_netCom.AddPackProcess(m_cSPackProcess);

            m_GatewayMessageHandlers = new Dictionary<uint, Dictionary<OnHandleGatewayMessage, GatewayMessageHandlerInfo>>();
            m_DelayOpGatewayHanlerInfos = new List<GatewayMessageHandlerInfo>();

            return true;
        }
        private IGameNetComSink m_sink;
        public void SetSinkObj(IGameNetComSink sink)
        {
            m_sink = sink;
        }
        public void OnConnect(uint erroCode, uint nResion)
        {
            if (erroCode == 0)
            {
                OnConnectSuccess();
                return;
            }

            if (erroCode != (int)EnNetDisconnCode.Manual)
            {
                OnDisconnect(EnNetDisconnCode.ConnectError, (int)erroCode, nResion);
                OnConnectFail(EnNetConnFailType.ConnectError, (int)erroCode, nResion);
            }
        }

        private void OnConnectStart()
        {
            if (m_sink != null)
            {
                m_sink.OnConnectStart();
            }
        }

        private void OnConnectSuccess()
        {
            m_isConnectSuccess = true;
            if (m_sink != null)
            {
                m_sink.OnConnectSuccess();
            }
        }

        private bool m_isConnectSuccess = false;

        private void OnDisconnect(EnNetDisconnCode disCode, int code, uint reason)
        {
            if (m_isConnectSuccess)
            {
                m_isConnectSuccess = false;
                if (m_sink != null)
                {
                    m_sink.OnDisconnect(disCode, code, reason);
                }
            }
        }

        private void OnConnectFail(EnNetConnFailType type, int code, uint reason)
        {
            if (m_sink != null)
            {
                m_sink.OnConnectFail(type, code, reason);
            }
        }

        //解包完成 
        public void UnPackComplete(IPackProcess packageData)
        {
            IGamePackProcess gamePackProcess = packageData as IGamePackProcess;
            bool isCSUnPack = gamePackProcess.GetMsgID() > 0;//是否是C#解包 
            int srcEndPoint = gamePackProcess.GetSrcEndPoint();

            //统计接收的消息
#if UNITY_EDITOR
            uint nMsgID = gamePackProcess.GetMsgID();
            if (m_msgRecives.ContainsKey(nMsgID))
            {
                m_msgRecives[nMsgID] = m_msgRecives[nMsgID] + 1;
            }
            else
            {
                m_msgRecives.Add(nMsgID, 1);
            }
#endif

            //不管是lua解包，还是C#解包，都回调出去
            bool bUnpack = false;
            if (m_sink != null)
            {
                m_sink.OnReceiveMessage(gamePackProcess);
                bUnpack = true;
            }

            //C#解包
            if (isCSUnPack)
            {
                //非网关消息
                if (srcEndPoint == NetDefine.ENDPOINT_NORMAL)
                {
                    bUnpack = true;
                    CSPackProcess csPack = gamePackProcess as CSPackProcess;
                    gamepol.TCSMessage msg = csPack.GetGameMsg(false);
                    FireMessage(csPack.GetMsgID(), msg);
                }
                //网关消息
                else
                {
                    bUnpack = true;
                    CSPackProcess csPack = gamePackProcess as CSPackProcess;
                    TCGMessage msg = csPack.GetGatewayMsg(false);
                    OnReceiveGatewayMessage(msg);
                }
            }

            //if (false == bUnpack)
            //    Debug.Log("解包完成，没有回调");
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="nMsgID"></param>
        /// <param name="msg"></param>
        public void FireMessage(uint nMsgID, gamepol.TCSMessage msg)
        {
            //if (nMsgID > 125 && nMsgID < 150)
            //    Debug.LogError("收到网络消息，消息码-" + nMsgID);

            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                MessageSinkEvent messageSinkEvent = m_msgEventDict[nMsgID];
                if (messageSinkEvent != null)
                {
                    messageSinkEvent.Fire(nMsgID, msg);
                }
            }
        }

        #region INetModule接口实现

        public void Update()
        {
            if (m_netCom != null)
            {
                m_netCom.Update();
            }
        }

        /// <summary>
        /// 接收消息，并将消息转发出去
        /// </summary>
        /// <param name="msg"></param>
        public void OnReceiveMsg(gamepol.TCSMessage msg)
        {
            if (msg == null) return;
            FireMessage(msg.stHead.get_iMsgID(), msg);
        }

        //连接网络
        public bool Connect(ref string ip, int port, int type)
        {
            m_isConnectSuccess = false;
            OnConnectStart();
            return m_netCom.Connect(ip, port, type, this);
        }

        //断开网络连接
        public void Disconnect()
        {
            m_netCom.DisConnect();
            if (m_isConnectSuccess)
            {
                OnDisconnect(EnNetDisconnCode.Manual, 0, (int)EnNetManualDisconnReasonType.Unkown);
                OnConnectFail(EnNetConnFailType.ManualDisconn, (int)EnNetDisconnCode.Manual, (int)EnNetManualDisconnReasonType.Unkown);
            }
        }

        public void SendMessage(byte srcEndPoint, byte dstEndPoint, gamepol.TCSMessage msg)
        {
            if (msg == null)
            {
                Debug.LogError("数据发送失败：msg==null");
                return;
            }

            SendMessage(srcEndPoint, dstEndPoint, msg.stHead.get_iMsgID());
        }

        public void SendMessage(byte srcEndPoint, byte dstEndPoint, uint nMsgID)
        {
            ISendMessage sendMessage = m_cSPackProcess.Pack(srcEndPoint, dstEndPoint, nMsgID);
            m_netCom.SendMessage(sendMessage);
        }

        public bool IsConnected()
        {
            return m_netCom.IsConnected();
        }

        //添加网络事件
        public void AddMessageEvent(uint nMsgID, OnMessageSink onMessageSink, string desc)
        {
            MessageSinkEvent messageSinkEvent = null;
            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                messageSinkEvent = m_msgEventDict[nMsgID];
            }
            else
            {
                messageSinkEvent = new MessageSinkEvent(nMsgID, monitorSink);
                m_msgEventDict.Add(nMsgID, messageSinkEvent);
            }
            messageSinkEvent.AddSink(onMessageSink, desc);
        }

        //移除网络事件
        public void RemoveMessageEvent(uint nMsgID, OnMessageSink onMessageSink)
        {
            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                MessageSinkEvent messageSinkEvent = null;
                messageSinkEvent = m_msgEventDict[nMsgID];
                messageSinkEvent.RemoveSink(onMessageSink);
            }
        }
        //添加网络消息事件
        public void AddMessageEvent(uint nMsgID, OnMessageAction action, string desc)
        {
            MessageSinkEvent messageSinkEvent = null;
            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                messageSinkEvent = m_msgEventDict[nMsgID];
            }
            else
            {
                messageSinkEvent = new MessageSinkEvent(nMsgID, monitorSink);
                m_msgEventDict.Add(nMsgID, messageSinkEvent);
            }
            messageSinkEvent.AddSink(action, desc);
        }

        //移除网络消息事件
        public void RemoveMessageEvent(uint nMsgID, OnMessageAction action)
        {
            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                MessageSinkEvent messageSinkEvent = null;
                messageSinkEvent = m_msgEventDict[nMsgID];
                messageSinkEvent.RemoveSink(action);
            }
        }

        // 获取游戏协议对象
        public gamepol.TCSMessage GetGameMsg(bool bSendFlag)
        {
            return m_cSPackProcess.GetGameMsg(bSendFlag);
        }

        // 获取网关协议对象
        public cgpol.TCGMessage GetGatewayMsg(bool bSendFlag)
        {
            return m_cSPackProcess.GetGatewayMsg(bSendFlag);
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
        }

        public void Release()
        {
        }

        #endregion

        public void OnReceive(byte[] data, int nLen, int srcEndPoint)
        {
            m_netCom.OnReceiveMsg(data, nLen, srcEndPoint, m_cSPackProcess);
        }

        public Dictionary<string, int> GetNetNodeInfo()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();

            foreach (var v in m_msgEventDict.Values)
            {
                v.GetNetNodeInfo(ref dict);
            }

            return dict;
        }

        public Dictionary<string, uint> GetCallInfoCount()
        {
#if UNITY_EDITOR
            Dictionary<string, uint> dict = new Dictionary<string, uint>();

            foreach (var v in m_msgEventDict.Values)
            {
                Dictionary<string, uint> infoCount = v.GetCallInfoCount();
                if (infoCount != null)
                {
                    foreach (var v2 in infoCount)
                    {
                        if (dict.ContainsKey(v2.Key))
                        {
                            dict[v2.Key] = dict[v2.Key] + v2.Value;
                        }
                        else
                        {
                            dict.Add(v2.Key, v2.Value);
                        }
                    }
                }
            }

            foreach (var msgID in m_msgRecives.Keys)
            {
                string key = $"接收到的网络包_消息ID-{msgID}";
                dict.Add(key, m_msgRecives[msgID]);
            }

            return dict;
#endif
            return null;
        }

        public string GetConnectIP()
        {
            if (null != m_netCom)
            {
                return m_netCom.GetConnectIP();
            }

            return null;
        }

        public int GetPort()
        {
            if (null != m_netCom)
            {
                return m_netCom.GetPort();
            }

            return 0;
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

        private void OnReceiveGatewayMessage(TCGMessage message)
        {
            m_IsFiringGatewayMessage = true;

            var msgID = message.stHead.get_iMsgID();
            Dictionary<OnHandleGatewayMessage, GatewayMessageHandlerInfo> dicHandlers;
            if (m_GatewayMessageHandlers.TryGetValue(msgID, out dicHandlers))
            {
                foreach (var info in dicHandlers.Values)
                {
                    if (info.isValid)
                    {
                        info.handler.Invoke(message);
                    }
                }
            }

            m_IsFiringGatewayMessage = false;

            //还有延迟的处理
            if (m_DelayOpGatewayHanlerInfos.Count > 0)
            {
                for (var i = 0; i < m_DelayOpGatewayHanlerInfos.Count; i++)
                {
                    var info = m_DelayOpGatewayHanlerInfos[i];
                    if (info.op == GatewayMessageOp.Add)
                        AddGatewayMessageHandler(info.msgID, info.handler, string.Empty);
                    else if (info.op == GatewayMessageOp.Remove)
                        RemoveGatewayMessageHandler(info.msgID, info.handler);
                }
                m_DelayOpGatewayHanlerInfos.Clear();
            }
        }

        public void AddGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage handler, string desc)
        {
            if (m_IsFiringGatewayMessage)
            {
                var delayInfo = LitePoolableObject.Instantiate<GatewayMessageHandlerInfo>();
                delayInfo.handler = handler;
                delayInfo.msgID = nMsgID;
                delayInfo.op = GatewayMessageOp.Add;
                m_DelayOpGatewayHanlerInfos.Add(delayInfo);
                return;
            }

            GatewayMessageHandlerInfo info;
            Dictionary<OnHandleGatewayMessage, GatewayMessageHandlerInfo> dicHandlers;

            if (!m_GatewayMessageHandlers.TryGetValue(nMsgID, out dicHandlers))
            {
                dicHandlers = new Dictionary<OnHandleGatewayMessage, GatewayMessageHandlerInfo>();
                m_GatewayMessageHandlers[nMsgID] = dicHandlers;

                info = LitePoolableObject.Instantiate<GatewayMessageHandlerInfo>();
                info.handler = handler;
                info.msgID = nMsgID;

                dicHandlers.Add(handler, info);

                return;
            }

            //已经注册过了
            if (dicHandlers.TryGetValue(handler, out info))
            {
                info.isValid = true;
                return;
            }

            //新的记录
            info = LitePoolableObject.Instantiate<GatewayMessageHandlerInfo>();
            info.handler = handler;
            info.msgID = nMsgID;
            dicHandlers.Add(handler, info);
        }

        public void RemoveGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage handler)
        {
            if (m_IsFiringGatewayMessage)
            {
                var delayInfo = LitePoolableObject.Instantiate<GatewayMessageHandlerInfo>();
                delayInfo.handler = handler;
                delayInfo.msgID = nMsgID;
                delayInfo.op = GatewayMessageOp.Remove;
                m_DelayOpGatewayHanlerInfos.Add(delayInfo);
                return;
            }

            Dictionary<OnHandleGatewayMessage, GatewayMessageHandlerInfo> dicHandlers;
            if (!m_GatewayMessageHandlers.TryGetValue(nMsgID, out dicHandlers))
                return;

            if (dicHandlers.TryGetValue(handler, out GatewayMessageHandlerInfo info))
            {
                LitePoolableObject.Recycle(info);
                dicHandlers.Remove(handler);
            }
        }

        public void FlushSend()
        {
            if(null!= m_netCom)
            {
                m_netCom.FlushSend();
            }
        }
    }
}
