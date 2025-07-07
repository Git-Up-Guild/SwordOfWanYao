/************************************************************************************
* Copyright (c) 2020 All Rights Reserved.
*命名空间：XGame.UI
*文件名： ChatMessageItem.cs
*创建人： 郑秀程
*创建时间：2020/7/7 17:33:18 
*描述
*=========================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using XGame.LOP;

namespace XGame.AssetScript.Chat
{
    public class ChatMessageItem : UIBehaviour
    {
        private static int ALLOC_ID = 1;

        //操作ID
        private int m_LOPID;
        public int LOPID => m_LOPID;

        //所关联的消息ID
        //[HideInInspector]
        public long messageID;

        //所关联的消息类型
        //[HideInInspector]
        public int messageType;

        [HideInInspector]
        public int protoIndex;

        [HideInInspector]
        public object userData;

        //聊天视图对象
        private ChatMessageView m_View;

        private int m_AllocID;

        protected override void Awake()
        {
            SetupLOP();
        }

        public void SetupLOP()
        {
            if (m_LOPID == 0)
            {
                m_AllocID = ALLOC_ID++;
                m_LOPID = LOPObjectRegister.Register(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            if (m_LOPID > 0)
            {
                LOPObjectRegister.UnRegister(m_LOPID);
                m_LOPID = 0;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="view"></param>
        /// <param name="msgType"></param>
        /// <param name="msgId"></param>
        /// <param name="playAnim">是否播放动画</param>
        public void Initialize(ChatMessageView view, int msgType, long msgId, object userData)
        {
            m_View = view;
            messageType = msgType;
            messageID = msgId;
            this.userData = userData;

            if (!gameObject.activeSelf)
                gameObject.BetterSetActive(true);

            //先初始化
            m_View.OnItemInitialize(this);

            name = "ChatMessage#" + m_AllocID;
        }

        //重置
        public void Clear()
        {
            m_View?.OnItemReset(this);
            if (gameObject.activeSelf)
                gameObject.BetterSetActive(false);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            //Debug.Log("name=" + name + ", OnRectTransformDimensionsChange=" + (transform as RectTransform).rect.size);
            m_View.SetLayoutDirty();
        }

        /// <summary>
        /// 连接点击处理
        /// </summary>
        /// <param name="url"></param>
        public void OnLinkClick(string url)
        {
            m_View?.OnChatMessageLinkClick(messageID, url);
        }

        /// <summary>
        /// 点击到空白处了
        /// </summary>
        public void OnAnyClick(EnChatViewClickType clickType, string clickData1, string clickData2)
        {
            if(clickType == EnChatViewClickType.Link)
            {
                OnLinkClick(clickData1);
            }
            else if(clickType == EnChatViewClickType.Sprite)
            {
                OnSpriteClick(clickData1);
            }

            m_View?.OnChatMessageAnyClick(messageID, clickType, clickData1, clickData2);
        }

        /// <summary>
        /// 点击图片处理
        /// </summary>
        /// <param name="id"></param>
        public void OnSpriteClick(string id)
        {
            m_View?.OnChatMessageSpriteClick(messageID, id);
        }
    }
}
