using gamepol;
using I18N.Common;
using minigame;
using ORM;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Game;

namespace XClient.Net
{

    //通用游戏demo的网络包的中转模块
    public class NetTransferModule : INetTransferModule,OnMessageSink
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        //游戏消息现场
        private TGameMessage m_gameMessage = new TGameMessage();

        //消息订阅列表
        private  Dictionary<uint, TransferMessageSinkEvent> m_msgEventDict = new Dictionary<uint, TransferMessageSinkEvent>();

        //解包助手
        private CORM_packaux m_recvGameOrmPacker = new CORM_packaux();
        //打包助手
        private CORM_packaux m_sendGameOrmPacker = new CORM_packaux();

        public void AddMessageEvent(uint nMsgID, IGameMessageSink messageSink, string desc)
        {
            TransferMessageSinkEvent messageSinkEvent = null;
            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                messageSinkEvent = m_msgEventDict[nMsgID];
            }
            else
            {
                messageSinkEvent = new TransferMessageSinkEvent(nMsgID, null);
                m_msgEventDict.Add(nMsgID, messageSinkEvent);
            }
            messageSinkEvent.AddSink(messageSink, desc);
        }

        public bool Create(object context, object config = null)
        {
            return true;
        }

        public void FixedUpdate()
        {
        }

        public TGameMessage GetGameMsg(uint msgID)
        {
            m_gameMessage.Init(msgID);
            return m_gameMessage;
        }

        public void LateUpdate()
        {
        }

        public void Release()
        {
            m_gameMessage = null;
        }

        public void RemoveMessageEvent(uint nMsgID, IGameMessageSink messageSink)
        {
            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                TransferMessageSinkEvent messageSinkEvent = null;
                messageSinkEvent = m_msgEventDict[nMsgID];
                messageSinkEvent.RemoveSink(messageSink);
            }
        }

        public bool SendMessage(TGameMessage message,bool bSendToSelf)
        {
            // 发送进入房间请求
            TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_MINIGAME_BROADCAST_MSG_REQ);
            TMSG_MINIGAME_BROADCAST_MSG_REQ rep = tcsMessage.stTMSG_MINIGAME_BROADCAST_MSG_REQ;
            byte[] arrPacked = rep.set_arrPacked();
            m_sendGameOrmPacker.Init(arrPacked, arrPacked.Length);
            message.Pack(m_sendGameOrmPacker);

            rep.set_iLen(m_sendGameOrmPacker.GetDataOffset());
            rep.set_bStart(1);
            rep.set_bEnd(1);
            int sendToSelf = bSendToSelf?1:0;
            rep.set_bSendToSelf((sbyte)sendToSelf);

            GameHelp.SendMessage_CS(tcsMessage);

            return true;
        }

        public bool Start()
        {
            INetModule netModule = CGame.Instance.NetModule;
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_BROADCAST_MSG_RSP, this, "NetTransferModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_BROADCAST_MSG_NTF, this, "NetTransferModule:Start");
            return true;
        }

        public void Stop()
        {
            INetModule netModule = CGame.Instance.NetModule;
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_BROADCAST_MSG_RSP, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_BROADCAST_MSG_NTF, this);
        }

        public void Update()
        {
            
        }

        public void OnMessage(TCSMessage msg)
        {
            uint msgID = msg.stHead.get_iMsgID();
            switch (msgID)
            {
                case TCSMessage.MSG_MINIGAME_BROADCAST_MSG_RSP:
                    {
                        TMSG_MINIGAME_BROADCAST_MSG_RSP context = msg.stTMSG_MINIGAME_BROADCAST_MSG_RSP;
                        __OnMSG_MINIGAME_BROADCAST_MSG_RSP(context);
                    }
                    break;
                case TCSMessage.MSG_MINIGAME_BROADCAST_MSG_NTF:
                    {
                        TMSG_MINIGAME_BROADCAST_MSG_NTF context = msg.stTMSG_MINIGAME_BROADCAST_MSG_NTF;
                        __OnMSG_MINIGAME_BROADCAST_MSG_NTF(context);
                    }
                    break;
            
                default:
                    break;
            }

        }

        //广播回复
        private void __OnMSG_MINIGAME_BROADCAST_MSG_RSP(TMSG_MINIGAME_BROADCAST_MSG_RSP context)
        {
            int iError = context.get_iError();
            if (0 != iError)
            {
                Debug.LogError("__OnMSG_MINIGAME_BROADCAST_MSG_RSP error = " + iError);

                return;
            }


        }

        //广播通知
        private void __OnMSG_MINIGAME_BROADCAST_MSG_NTF(TMSG_MINIGAME_BROADCAST_MSG_NTF context)
        {
            int iLen = context.get_iLen();
            byte[] arrPacked = context.get_arrPacked();
            m_recvGameOrmPacker.Init(arrPacked, iLen);
            m_gameMessage.Unpack(m_recvGameOrmPacker);

            //回调广播消息
            __FireMessage(m_gameMessage);


        }

        //回调通知外部
        void __FireMessage(TGameMessage msg)
        {

            uint nMsgID = msg.stHead.get_iMsgID();

            if (m_msgEventDict.ContainsKey(nMsgID))
            {
                TransferMessageSinkEvent messageSinkEvent = m_msgEventDict[nMsgID];
                if (messageSinkEvent != null)
                {
                    messageSinkEvent.Fire(nMsgID, msg);
                }
            }
        }

        public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了

        public void Clear(int param)
        {
        }

        public void OnRoleDataReady()
        {
        }
    }



}
