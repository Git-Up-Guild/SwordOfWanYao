using minigame;
using System;
using System.Collections.Generic;
using XClient.Game;
using XClient.Net;
using XClient.RPC;
using XGame.Poolable;

namespace XClient.Common
{
    /// <summary>
    /// 通用网络消息注册器
    /// </summary>
    public class CommonNetMessageRegister : LitePoolableObject
    {
        private NetTransferMessageRegister m_TransferMessageHandlerRegisters;
        private INetModule m_NetModule => CGame.Instance.NetModule;

        private Dictionary<uint, OnMessageAction> m_CSHandlerDict;

        private Dictionary<uint, OnHandleGatewayMessage> m_CGHandlerDict;

        public void AddHandler(uint msgID, OnMessageAction handler, string desc)
        {
            if (m_CSHandlerDict == null)
                m_CSHandlerDict = new Dictionary<uint, OnMessageAction>();

            m_CSHandlerDict.Add(msgID, handler);

            m_NetModule.AddMessageHandler(msgID, handler, desc);
        }

        public void RemoveHandler(uint msgID, OnMessageAction handler)
        {
            m_NetModule.RemoveMessageHandler(msgID, handler);

            m_CSHandlerDict?.Remove(msgID);
        }



        public void AddHandler(uint msgID, Action<TGameMessage> handler, string desc)
        {
            if (m_TransferMessageHandlerRegisters == null)
                m_TransferMessageHandlerRegisters = LitePoolableObject.Instantiate<NetTransferMessageRegister>();

            m_TransferMessageHandlerRegisters.AddMessageHandler(msgID, handler, desc);
        }

        public void RemoveHandler(uint msgID, Action<TGameMessage> handler)
        {
            m_TransferMessageHandlerRegisters?.RemoveHandler(msgID, handler);
        }

        public void AddHandler(uint msgID, OnHandleGatewayMessage handler, string desc)
        {
            m_NetModule.AddGatewayMessageHandler(msgID, handler, desc);

            if (m_CGHandlerDict == null)
                m_CGHandlerDict = new Dictionary<uint, OnHandleGatewayMessage>();

            m_CGHandlerDict.Add(msgID, handler);
        }

        public void RemoveHandler(uint msgID, OnHandleGatewayMessage handler)
        {
            m_NetModule.RemoveGatewayMessageHandler(msgID, handler);

            m_CGHandlerDict?.Remove(msgID);
        }

        public void AddHandler(string rpcName, OnRecieveRpcCallback handler, string desc)
        {
            GameGlobal.RPC.Register(rpcName, handler);
        }

        public void RemoveHandler(string rpcName, OnRecieveRpcCallback handler, string desc)
        {
            GameGlobal.RPC.Deregister(rpcName, handler);
        }

        protected override void OnRecycle()
        {
            if(m_TransferMessageHandlerRegisters != null)
                LitePoolableObject.Recycle(m_TransferMessageHandlerRegisters);
            m_TransferMessageHandlerRegisters = null;
        }
    }
}
