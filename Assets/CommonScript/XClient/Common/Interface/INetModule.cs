using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gamepol;
using XClient.Common;

namespace XClient.Common
{
    /// <summary>
    /// 网络消息分发器
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="nMsgID"></param>
        /// <param name="onMessageSink">回调sink</param>
        /// <param name="desc">描述</param>
        void AddMessageHandler(uint nMsgID, OnMessageSink onMessageSink, string desc);

        /// <summary>
        ///退订消息
        /// </summary>
        /// <param name="nMsgID">事件id</param>
        /// <param name="onMessageSink">回调sink</param>
        void RemoveMessageHandler(uint nMsgID, OnMessageSink onMessageSink);

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="nMsgID">消息id</param>
        /// <param name="action">回调方法</param>
        /// <param name="desc">描述</param>
        void AddMessageHandler(uint nMsgID, OnMessageAction action, string desc);

        /// <summary>
        /// 退订消息
        /// </summary>
        /// <param name="nMsgID">消息id</param>
        /// <param name="action">回调方法</param>
        void RemoveMessageHandler(uint nMsgID, OnMessageAction action);

        /// <summary>
        /// 订阅网关消息
        /// </summary>
        /// <param name="nMsgID">消息id</param>
        /// <param name="action">回调方法</param>
        /// <param name="desc">描述</param>
        void AddGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage action, string desc);

        /// <summary>
        /// 退订网关消息
        /// </summary>
        /// <param name="nMsgID">消息id</param>
        /// <param name="action">回调方法</param>
        void RemoveGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage action);

    }

    public interface INetModule : IModule, IMessageDispatcher
    {
        /***************************游戏数据接口**************************************/
        uint GetLatency();

        void SetKick(bool kick);

        void ReConnect(bool resetRetryCount = true);

        /***************************网络接口**************************************/

        //连接网络
        bool Connect(string ip, int port, int type);

        //断开网络连接
        void Disconnect();

        /// <summary>
        /// 接收消息，并将消息转发出去
        /// </summary>
        /// <param name="msg"></param>
        void OnReceiveMsg(gamepol.TCSMessage msg);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="srcEndPoint"></param>
        /// <param name="dstEndPoint"></param>
        /// <param name="msg"></param>
        void SendMessage_CS(byte srcEndPoint, byte dstEndPoint, gamepol.TCSMessage msg);

        /// <summary>
        /// 是否连接上了
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        // 获取游戏协议对象
        gamepol.TCSMessage GetGameMsg(bool bSendFlag);

        // 获取游戏协议对象
        gamepol.TCSMessage GetAndInitGameMsg(bool bSendFlag, uint msgID);

        // 获取网关协议对象
        cgpol.TCGMessage GetGatewayMsg(bool bSendFlag);

        //测试时增加手动解析协议接口，模拟客户端收到了该条协议
        void OnReceive(byte[] data, int nLen, int srcEndPoint);

        /// <summary>
        /// 获取最后活跃时间
        /// </summary>
        /// <returns></returns>
        float GetLastActiveTime();

        /// <summary>
        /// 最后收到消息时间
        /// </summary>
        /// <returns></returns>
        float GetLastReceiveTime();

        Dictionary<string, int> GetNetNodeInfo();

        Dictionary<string, uint> GetCallInfoCount();

        #region 虚拟器用-装饰
        void SetSendMessageHandler(Action<byte, byte, gamepol.TCSMessage> sendMessageCs);
        
        void RestoreSendMessage();
        
        void SendMessage_CS_Backup(byte srcEndPoint, byte dstEndPoint, gamepol.TCSMessage msg);

        #endregion

        void DispatchGameMessage(uint getIMsgID, TCSMessage sendObj);

        void OnLoginFail(EnNetConnFailType type, EnNetLoginFailType errcode1, uint reason);
        void ShowReconnectMessageBox();

        void SetFirstConnected(bool first = false);
    }
}
