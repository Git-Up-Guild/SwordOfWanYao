
using minigame;
using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Game;
using XGame.Poolable;

namespace XClient.Net
{
    /// <summary>
    /// 通用网络消息处理器注册器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetTransferMessageRegister : LitePoolableObject, IGameMessageSink
    {
        private Dictionary<uint, Action<TGameMessage>> m_HandlerDict;

        protected INetTransferModule netTransfer => CGame.Instance.GetModule(DModuleID.MODULE_ID_NET_TRANSFER) as INetTransferModule;

        protected override void OnInit(object context = null)
        {
            base.OnInit();

            if (m_HandlerDict == null)
                m_HandlerDict = new Dictionary<uint, Action<TGameMessage>>();
        }

        /// <summary>
        /// 添加处理器，需要在Setup之前调用
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="handler"></param>
        public void AddMessageHandler(uint msgID, Action<TGameMessage> handler, string desc)
        {
            if (m_HandlerDict.ContainsKey(msgID))
            {
                Debug.LogError("不能重复注册消息！");
                return;
            }

            m_HandlerDict[msgID] = handler;
            netTransfer?.AddMessageEvent(msgID, this, desc);
        }

        /// <summary>
        /// 移除处理器
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="handler"></param>
        public void RemoveHandler(uint msgID, Action<TGameMessage> handler)
        {
            m_HandlerDict?.Remove(msgID);
            netTransfer?.RemoveMessageEvent(msgID, this);
        }

        public void OnGameMessage(TGameMessage msg)
        {
            if (m_HandlerDict == null)
                return;

            Action<TGameMessage> handler;
            if(m_HandlerDict.TryGetValue(msg.stHead.get_iMsgID(), out handler))
            {
                handler.Invoke(msg);
            }
        }

        protected override void OnRecycle()
        {
            if(m_HandlerDict != null)
            {
                foreach (var msgID in m_HandlerDict.Keys)
                {
                    netTransfer?.RemoveMessageEvent(msgID, this);
                }
            }

            m_HandlerDict.Clear();
        }
    }
}
