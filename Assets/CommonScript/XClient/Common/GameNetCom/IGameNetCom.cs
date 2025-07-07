using cgpol;
using System.Collections.Generic;
using XGame;

namespace XClient.Common
{
    public interface IGameNetComSink
    {
        void OnConnectStart();
        void OnConnectSuccess();
        void OnConnectFail(EnNetConnFailType type, int errcode1, uint reason);
        void OnDisconnect(EnNetDisconnCode type, int code, uint reason);
        void OnReceiveMessage(IGamePackProcess gamePackProcess);
    }
    public delegate void OnMessageAction(gamepol.TCSMessage msg);

    public delegate void OnHandleGatewayMessage(TCGMessage msg);

    public interface IGameNetCom : ICom
    {
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="msg"></param>
        void OnReceiveMsg(gamepol.TCSMessage msg);

        //连接网络
        bool Connect(ref string ip, int port, int type);

        //断开网络连接
        void Disconnect();

        void SetSinkObj(IGameNetComSink sink);

        //获取链接的IP
        string GetConnectIP();

        //获取链接端口
        int GetPort();

        void SendMessage(byte srcEndPoint, byte dstEndPoint, gamepol.TCSMessage msg);

        void SendMessage(byte srcEndPoint, byte dstEndPoint, uint nMsgID);

        bool IsConnected();

        //添加网络消息事件
        void AddMessageEvent(uint nMsgID, OnMessageSink onMessageSink, string desc);

        //移除网络事件
        void RemoveMessageEvent(uint nMsgID, OnMessageSink onMessageSink);

        //添加网络消息事件
        void AddMessageEvent(uint nMsgID, OnMessageAction action, string desc);

        //移除网络消息事件
        void RemoveMessageEvent(uint nMsgID, OnMessageAction action);

        //添加网关消息处理器
        void AddGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage action, string desc);

        //移除网关消息处理器
        void RemoveGatewayMessageHandler(uint nMsgID, OnHandleGatewayMessage action);

        // 获取游戏协议对象
        gamepol.TCSMessage GetGameMsg(bool bSendFlag);

        // 获取网关协议对象
        cgpol.TCGMessage GetGatewayMsg(bool bSendFlag);

        //测试时增加手动解析协议接口，模拟收到该条协议
        void OnReceive(byte[] data, int nLen, int srcEndPoint);

        //获取最后活跃时间
        float GetLastActiveTime();

        //最后收到消息时间
        float GetLastReceiveTime();

        Dictionary<string, int> GetNetNodeInfo();

        Dictionary<string, uint> GetCallInfoCount();

        void FireMessage(uint nMsgID, gamepol.TCSMessage msg);

        //推动发送缓存
        void FlushSend();
    }

    public interface INetMonitor
    {
        float GetTime();
        void OnReport(int id, string desc, float costTime);
    }
}