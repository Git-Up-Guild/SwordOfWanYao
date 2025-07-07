
using System;

/// <summary>
/// 一条聊天记录
/// </summary>
namespace XGame.AssetScript.Chat
{
    /// <summary>
    /// 具体的内容
    /// </summary>
    [Serializable]
    public class ChatMessageContent
    {
        /// <summary>
        /// 模板ID，如果指定了模板ID，那么模板ID决定了要显示的信息
        /// </summary>
        public int templateID;

        /// <summary>
        /// 附加数据，这里存放一个字符串，可能是将一个luatable转换而来
        /// </summary>
        public string attachData = string.Empty;

        /// <summary>
        /// 聊天内容，通常是玩家输入的内容
        /// </summary>
        public string message = string.Empty;

        /// <summary>
        /// 扩展数据，用来存放额外的数据，主要是用来做扩展，可能是将一个luatable转换而来
        /// </summary>
        public string extraData = string.Empty;

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            templateID = 0;
            attachData = string.Empty;
            message = string.Empty;
            extraData = string.Empty;
        }

        /// <summary>
        /// 数据拷贝
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(ChatMessageContent other)
        {
            other.templateID = templateID;
            other.attachData = attachData;
            other.message = message;
            other.extraData = extraData;
        }

        public override string ToString()
        {
            return $"templateID={templateID}, attachData={attachData}, message={message}, extraData={extraData}";
        }
    }

    /// <summary>
    /// 一条消息的数据
    /// </summary>
    [Serializable]
    public class ChatMessageData
    {
        /// <summary>
        /// 消息唯一ID
        /// </summary>
        public string id;

        /// <summary>
        /// 主ID
        /// </summary>
        public uint mainID;

        /// <summary>
        /// 子ID
        /// </summary>
        public uint subID;

        /// <summary>
        /// 发送者类型
        /// </summary>
        public int senderType;

        /// <summary>
        /// 发送者头像信息
        /// </summary>
        public int senderTitleID;

        /// <summary>
        /// 头像框ID
        /// </summary>
        public int senderFrameID;

        /// <summary>
        /// 头像ID
        /// </summary>
        public int senderFaceID;

        /// <summary>
        /// 发送者名称
        /// </summary>
        public string senderName;

        /// <summary>
        /// 玩家ID
        /// </summary>
        public uint senderID;

        /// <summary>
        /// 发送者服务器ID
        /// </summary>
        public int worldID;

        /// <summary>
        ///发送者时间
        /// </summary>
        public uint time;

        /// <summary>
        /// 消息类型
        /// </summary>
        public int msgType;

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool isReaded;

        /// <summary>
        /// 聊天内容
        /// </summary>
        public ChatMessageContent content = new ChatMessageContent();

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            id = string.Empty;
            mainID = 0;
            subID = 0;
            senderType = 0;
            senderTitleID = 0;
            senderFrameID = 0;
            senderFaceID = 0;
            senderID = 0;
            senderName = string.Empty;
            worldID = 0;
            time = 0;
            msgType = 0;
            isReaded = false;
            content.Clear();
    }

        /// <summary>
        /// 数据拷贝
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(ChatMessageData other)
        {
            other.id = id;
            other.mainID = mainID;
            other.subID = subID;
            other.senderType = senderType;
            other.senderTitleID = senderTitleID;
            other.senderFrameID = senderFrameID;
            other.senderFaceID = senderFaceID;
            other.senderName = senderName;
            other.senderID = senderID;
            other.worldID = worldID;
            other.time = time;
            other.msgType = msgType;
            other.isReaded = isReaded;
            content.CopyTo(other.content);
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"id={id}, mainID={mainID}, subID={subID}, senderType={senderType}, senderTitleID={senderTitleID}," +
                $" senderID={senderID}, senderFrameID={senderFrameID}, senderFaceID={senderFaceID}, senderName={senderName}, worldID={worldID}, time={time}," +
                $" msgType={msgType}, isReaded={isReaded}, content={content}";
        }
    }
}
