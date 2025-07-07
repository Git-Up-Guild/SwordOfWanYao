using minigame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;


namespace XClient.Net
{

    //游戏收到消息的回调
    public interface IGameMessageSink
    {
        void OnGameMessage(TGameMessage msg);
    }


    //上层游戏网络中转层模块
    public interface INetTransferModule : IModule
    {
        // 获取游戏转接的消息对象
        TGameMessage GetGameMsg(uint msgID);

        //发送消息
        bool SendMessage(TGameMessage message, bool bSendToSelf);

        /// 订阅消息
        void AddMessageEvent(uint nMsgID, IGameMessageSink messageSink, string desc);

        ///退订消息
        void RemoveMessageEvent(uint nMsgID, IGameMessageSink messageSink);


    }


}
