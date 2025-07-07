using System;
using System.Collections.Generic;
using gamepol;
using UnityEngine;
using XClient.Client;
using XGame;
namespace XClient.Common.VirtualServer
{
    /// <summary>
    /// 消息处理器记录
    /// </summary>
    public class MessageHandlerRecord
    {
        // 处理器 -> 状态的字典（模拟Lua表的结构）
        private readonly Dictionary<Action<TCSMessage>, bool> handlers = new Dictionary<Action<TCSMessage>, bool>();

        public bool IsEmpty => handlers.Count == 0;

        public void AddHandler(Action<TCSMessage> handler)
        {
            if (handler != null)
            {
                handlers[handler] = true;
            }
        }

        public void RemoveHandler(Action<TCSMessage> handler)
        {
            if (handler != null)
            {
                handlers.Remove(handler);
            }
        }

        public void InvokeHandlers(TCSMessage message)
        {
            foreach (var handler in handlers.Keys)
            {
                handler(message);
            }
        }
    }
    
    public class VirtualNet
    {
        private readonly Dictionary<uint, MessageHandlerRecord> msgHandlers = new Dictionary<uint, MessageHandlerRecord>();

        private INetModule gNetModule
        {
            get
            {
                return GameGlobal.NetModule;
            }
        }
        public void Start()
        {
            gNetModule.SetSendMessageHandler(SendMessage_CS);
        }

        public void Stop()
        {
            gNetModule.RestoreSendMessage();
        }
        
        public void SendMessage_CS(byte srcEndPoint, byte dstEndPoint, TCSMessage msg)
        {
            if (msg != null)
            {
                uint id = msg.stHead.get_iMsgID();
                if (msgHandlers.ContainsKey(id))
                {
                    if (!OnRecvClientMessage(msg))
                    {
                        gNetModule.SendMessage_CS_Backup(srcEndPoint, dstEndPoint, msg);
                    }
                }
                else
                {
                    gNetModule.SendMessage_CS_Backup(srcEndPoint, dstEndPoint, msg);
                }
            }
        }
        
        public void SendMessageToClient(uint getIMsgID, TCSMessage sendObj)
        {
            gNetModule.DispatchGameMessage(getIMsgID, sendObj);
        }

        public bool OnRecvClientMessage(TCSMessage msg)
        {
            uint msgId = msg.stHead.get_iMsgID();
            if (msgHandlers.TryGetValue(msgId, out var handlersRecord))
            {
                // 调用所有注册的处理器
                handlersRecord.InvokeHandlers(msg);
                return true;
            }
            return false;
        }

        public void RegisterMsgHandler(uint msgId, Action<TCSMessage> handler)
        {
            if (!msgHandlers.ContainsKey(msgId))
            {
                msgHandlers[msgId] = new MessageHandlerRecord();
            }
            msgHandlers[msgId].AddHandler(handler);
        }

        // 注销消息处理器
        public void UnregisterMsgHandler(uint msgId, Action<TCSMessage> handler)
        {
            if (msgHandlers.TryGetValue(msgId, out var record))
            {
                record.RemoveHandler(handler);
                if (record.IsEmpty)
                {
                    msgHandlers.Remove(msgId);
                }
            }
        }
    }

    
}
