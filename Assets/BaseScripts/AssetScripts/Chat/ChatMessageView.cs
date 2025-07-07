/************************************************************************************
* Copyright (c) 2020 All Rights Reserved.
*命名空间：XGame.UI
*文件名： ChatMessageView.cs
*创建人： 郑秀程
*创建时间：2020/7/7 17:33:18 
*描述
*=========================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XGame.UI;

namespace XGame.AssetScript.Chat
{
    /// <summary>
    /// 点击类型
    /// </summary>
    public enum EnChatViewClickType
    {
        None,
        Empty,
        Sprite,
        Link,
    }

    [ExecuteAlways]
    public class ChatMessageView : VerticalScrollPane 
    {
        //延迟删除Item的时间
        public static float REMOVE_ITEM_DELAY_TIME = 0.5f;

        //保留的要删除的Item的最大数量，超过这个数量，就会马上进行删除操作
        public static int REMOVE_ITEM_KEEP_MAX_COUNT = 50;

        //消息显示的Item原型
        public List<GameObject> messageItemProtos;

        //超过最大条数，则将最开始的一条的内容替换，并且放到末尾
        public int maxShowCount = 200;

        //视图名称
        public string viewName = "ChatMessageView";

        //当前显示的消息Item
        private LinkedList<ChatMessageItem> m_MessageItems;

        //当前显示的消息Item
        private List<ChatMessageItem> m_RemovedMessageItems;

        //对象池
        private List<Stack<ChatMessageItem>> m_MessageItemPools;

        //回调函数
        private Action<long, string> m_OnLinkClicklistener;
        private Action<long, string> m_OnSpriteClicklistener;
        private Action<long, int, string, string> m_OnAnyClicklistener;

        private Action<int> m_OnItemInitListenter;
        private Action<int> m_OnItemResetListener;
        private Action m_OnTopBoundrayListener;
        private Action m_OnDownBoundrayListener;
        private Action<int> m_OnCloseBoundaryListener;

        private int[] m_ItemGenerateCount;

        //删除Item延迟时间
        private float m_RemoveItemDelayStartTick = 0f;

        //帧数
        private int m_FrameNum = 0;

        //最新的消息ID
        private long m_NewestMessageID = 0;

        private static Vector3 s_InvalidPosition = new Vector3(0, 99999999, 0);

        protected override void Awake()
        {
            base.Awake();

            m_ItemGenerateCount = new int[messageItemProtos.Count];
            m_MessageItems = new LinkedList<ChatMessageItem>();
            m_RemovedMessageItems = new List<ChatMessageItem>();
            m_MessageItemPools = new List<Stack<ChatMessageItem>>();

            for (int i = 0; i < messageItemProtos.Count; i++)
            {
                m_MessageItemPools.Add(new Stack<ChatMessageItem>());
            }

            topBoundaryEvent.AddListener(OnTopBoundaryEvent);
            bottomBoundaryEvent.AddListener(OnDownBoundaryEvent);
            noticeWillShowChecker = IsNoticeWillShow;
        }

        private void OnTopBoundaryEvent()
        {
            m_OnTopBoundrayListener?.Invoke();
        }

        private void OnDownBoundaryEvent()
        {
            m_OnDownBoundrayListener?.Invoke();
        }

        private void OnCloseBoundaryEvent(int boundary)
        {
            m_OnCloseBoundaryListener?.Invoke(boundary);
        }

        public void SetBoundaryListener(Action top, Action down)
        {
            m_OnTopBoundrayListener = top;
            m_OnDownBoundrayListener = down;
        }

        public void SetCloseBoundaryListener(Action<int> listener)
        {
            m_OnCloseBoundaryListener = listener;
        }

        public void ScrollToOldest()
        {
            ScrollToTop();
        }

        public void ScrollToNewest()
        {            
            ScrollToBottom();            
        }

        public void SetNewestMessageID(long messageID)
        {
            m_NewestMessageID = messageID;
        }

        private bool IsNoticeWillShow(Boundary boundary)
        {
            if (boundary == Boundary.Top)
                return true;

            if ( m_NewestMessageID == 0)
                return false;

            if (m_MessageItems.Count == 0)
                return true;

            var item = m_MessageItems.Last.Value;           //m_MessageItems[m_MessageItems.Count() - 1];
            if (item.messageID == m_NewestMessageID)
                return false;

            return true;
        }

        protected override void Update()
        {
            base.Update();

            m_FrameNum++;

            //删除掉需要移除的Item
            if (m_RemovedMessageItems.Count > 0)
            {
                bool isItemTooMuch = m_RemovedMessageItems.Count > REMOVE_ITEM_KEEP_MAX_COUNT;
                bool isRemoveItemDelayFinish = Time.time - m_RemoveItemDelayStartTick > REMOVE_ITEM_DELAY_TIME;
                bool isWillRemoveItem = isItemTooMuch || (isRemoveItemDelayFinish && !isScrolling && !isDraging);
                if (isWillRemoveItem)
                {
                    if (isItemTooMuch)
                    {
                        foreach (var item in m_RemovedMessageItems)
                        {
                            item.Clear();
                            RecycleMessageItem(item);
                        }
                        m_RemovedMessageItems.Clear();

                        //必须要强制刷新一次布局，否则会出现闪烁现象
                        RefreshViewLayout(true);

                        //滚到到停靠点
                        ScrollToStopPosition();
                    }
                }
            }
        }

        public void RefreshViewLayout(bool isImmediate = false)
        {
            if (isImmediate)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
            else
                SetLayoutDirty();
        }

        public void OnItemReset(ChatMessageItem item)
        {
            m_OnItemResetListener?.Invoke(item.LOPID);
        }

        public void OnItemInitialize(ChatMessageItem item)
        {
            m_OnItemInitListenter?.Invoke(item.LOPID);
        }

        public void OnChatMessageLinkClick(long msgId, string url)
        {
            m_OnLinkClicklistener?.Invoke(msgId,url);
        }

        public void OnChatMessageSpriteClick(long msgId, string id)
        {
            m_OnSpriteClicklistener?.Invoke(msgId, id);
        }

        public void OnChatMessageAnyClick(long msgId, EnChatViewClickType clickType, string data1, string data2)
        {
            m_OnAnyClicklistener?.Invoke(msgId, (int)clickType, data1, data2);
        }

        /// <summary>
        /// 设置任何点击回调
        /// </summary>
        /// <param name="callback"></param>
        public void SetAnyClickListener(Action<long, int, string, string> callback)
        {
            m_OnAnyClicklistener = callback;
        }

        /// <summary>
        /// 链接点击回调
        /// </summary>
        /// <returns></returns>
        public void SetLinkClickListener(Action<long, string> callback)
        {
            m_OnLinkClicklistener = callback;
        }

        /// <summary>
        /// 设置图片点击回调
        /// </summary>
        /// <param name="callback"></param>
        public void SetSpriteClickListener(Action<long, string> callback)
        {
            m_OnSpriteClicklistener = callback;
        }

        /// <summary>
        /// 设置Item的回调
        /// </summary>
        /// <param name="onItemInit"></param>
        /// <param name="onItemReset"></param>
        public void SetItemListener(Action<int> onItemInit, Action<int> onItemReset)
        {
            m_OnItemInitListenter = onItemInit;
            m_OnItemResetListener = onItemReset;
        }

        public ChatMessageItem FindMessageByID(long messageID)
        {
            bool isFinded = false;

            foreach (var item in m_MessageItems)
            {
                if (item.messageID == messageID && item.gameObject.activeSelf == true)
                {
                    isFinded = true;
                    OnTopBoundaryEvent();
                    //ScrollToOldest();
                    return item;
                }
            }

            if (!isFinded)
            {
                OnTopBoundaryEvent();
                //ScrollToOldest();
            }

            return null;
        }

        /// <summary>
        /// 滚动到指定消息ID的Item
        /// </summary>
        public void ScrollToMessageByID(long messageID)
        {
            var refreshCount = 0;            
            bool isFinded = false;            

            while (!isFinded)
            {
                if (refreshCount >= 10)
                {
                    ScrollToBottom();
                    return;
                }

                refreshCount += 1;

                foreach (var item in m_MessageItems)
                {
                    if (item.messageID == messageID && item.gameObject.activeSelf == true)
                    {
                        isFinded = true;                        
                        ScrollToSpecifiedPosition(item.transform as RectTransform);
                        return;
                    }
                }

                if (!isFinded)
                {
                    OnTopBoundaryEvent();
                    ScrollToOldest();
                }
            }

            if (!isFinded)
            {
                ScrollToBottom();
            }            
        }

        /// <summary>
        /// 移除超出数量限制的Item
        /// </summary>
        /// <param name="isHead">是否从头部移除</param>
        /// <param name="keepCount">要保留的数量</param>
        public void RemoveChatMessages(bool isHead, int keepCount)
        {
            if (m_MessageItems.Count() <= keepCount)
                return;

            bool isRemovedAny = false;
            while (m_MessageItems.Count() >= keepCount)
            {
                ChatMessageItem item;
                if (isHead)
                    item = m_MessageItems.First.Value;// m_MessageItems[0];
                else
                    item = m_MessageItems.Last.Value;// m_MessageItems[m_MessageItems.Count()-1];

                m_MessageItems.Remove(item);
                item.Clear();
                RecycleMessageItem(item);
                isRemovedAny = true;
            }

            //必须要强制刷新一次布局，否则会出现闪烁现象
            if(isRemovedAny)
                RefreshViewLayout(true);

            //滚到到停靠点
            ScrollToStopPosition();
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="isHead"></param>
        public void RemoveChatMessage(long messageID, bool isHead)
        {
            if (m_MessageItems.Count == 0)
                return;

            if (isHead)
            {
                var item = m_MessageItems.First.Value;//[0];
                if(item.messageID != messageID)
                {
                    Error("移头部除消息失败，消息ID不匹配！");
                }
                else
                {
                    m_MessageItems.Remove(item);
                    RecycleMessageItem(item);
                    //m_RemovedMessageItems.Add(item);
                }
            }
            else
            {
                var item = m_MessageItems.Last.Value;// [m_MessageItems.Count - 1];
                if (item.messageID != messageID)
                {
                    Error("移头尾部消息失败，消息ID不匹配！");
                }
                else
                {
                    m_MessageItems.Remove(item);
                    RecycleMessageItem(item);
                    //m_RemovedMessageItems.Add(item);
                }
            }
        }

        /// <summary>
        /// 末尾的Item是否在显示区域内
        /// </summary>
        /// <returns></returns>
        public bool IsTailMessageItemInRect(int offset)
        {
            if (m_MessageItems.Count == 0)
                return true;

            var item = m_MessageItems.Last.Value;// [m_MessageItems.Count - 1];
            var bound = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform, item.transform);

            if(Math.Abs(Math.Abs(bound.center.x) - rectTransform.rect.width * 0.5f) >= offset
                || Math.Abs(Math.Abs(bound.center.y) - rectTransform.rect.height * 0.5f) >= offset)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 添加聊天消息
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="messageID">消息ID</param>
        /// <param name="protoIndex">要使用哪一个消息原型来显示</param>
        /// <param name="isHead">是否添加到头部</param>
        /// <param name="isScroll">是否滚动</param>
        /// <returns></returns>
        public void AddChatMessage(int messageType, long messageID, int protoIndex, bool isHead,object userData)
        {
            ChatMessageItem item;

            if (isHead)
                HideTopBoundaryNotice();
            else
                HideBottomBoundaryNotice();

            //超过最大条数，则将最开始的一条的内容替换，并且放到末尾
            if (m_MessageItems.Count() >= maxShowCount)
            {
                //要添加到队列的头部，则回收尾部的Item
                if (isHead)
                {
                    int nTail = m_MessageItems.Count() - 1;
                    item = m_MessageItems.Last.Value;// [nTail];
                    //m_MessageItems.RemoveAt(nTail);
                    m_MessageItems.RemoveLast();
                }
                //要添加到队列的尾部，则回收头部的Item
                else
                {
                    item = m_MessageItems.First.Value;// [0];
                    m_MessageItems.RemoveFirst();
                    //m_MessageItems.RemoveAt(0);
                }
                
                item.Clear();
                RecycleMessageItem(item);
            }

            //重新分配一个
            item = AllocateMessageItem(protoIndex);
            item.gameObject.BetterSetActive(false);
            item.transform.localPosition = Vector3.zero;

            //调整顺序
            if (isHead)
            {
                item.transform.SetAsFirstSibling();
                m_MessageItems.AddFirst(item);
               // m_MessageItems.Insert(0, item);
            }
            else
            {
                item.transform.SetAsLastSibling();
                // m_MessageItems.Add(item);
                m_MessageItems.AddLast(item);
            }

            //绑定消息ID
            item.Initialize(this, messageType, messageID, userData);

            //延迟删除
            m_RemoveItemDelayStartTick = Time.time;

            //重新布局
            SetLayoutDirty();
        }

        /// <summary>
        /// 分配Item
        /// </summary>
        /// <returns></returns>
        private ChatMessageItem AllocateMessageItem(int protoIndex)
        {
            var itemPool = m_MessageItemPools[protoIndex];
            if (itemPool.Count() > 0)
            {
                var itemCached = itemPool.Pop();
                itemCached.transform.localPosition = s_InvalidPosition;
                return itemCached;
            }

            var goProto = messageItemProtos[protoIndex];
            m_ItemGenerateCount[protoIndex] = m_ItemGenerateCount[protoIndex] + 1;

            GameObject itemGo = Instantiate(goProto);

            var itemTransform = itemGo.transform as RectTransform;
            itemTransform.BetterSetParent(content.transform);
            itemTransform.localScale = Vector3.one;
            itemTransform.localPosition = s_InvalidPosition;

            ChatMessageItem item = itemGo.GetComponent<ChatMessageItem>();
            if(item == null)
                item = itemGo.AddComponent<ChatMessageItem>();
            item.protoIndex = protoIndex;
            item.SetupLOP();
            return item;
        }

        /// <summary>
        /// 回收Item
        /// </summary>
        /// <returns></returns>
        private void RecycleMessageItem(ChatMessageItem item)
        {
            item.Clear();

            var itemPool = m_MessageItemPools[item.protoIndex];
            itemPool.Push(item);
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            foreach (var item in m_MessageItems)
            {
                RecycleMessageItem(item);
            }

            m_MessageItems.Clear();
            m_NewestMessageID = 0;
            m_LastLayoutRefChild = null;

            if (isActiveAndEnabled)
            {
                HideTopBoundaryNotice();
                HideBottomBoundaryNotice();
                StopScrolling();
                ResetContent();
                ClampContentPosition();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_OnItemInitListenter = null;
            m_OnItemResetListener = null;
            m_OnLinkClicklistener = null;
            m_OnSpriteClicklistener = null;
            m_OnAnyClicklistener = null;
            m_OnTopBoundrayListener = null;
            m_OnDownBoundrayListener = null;

            //当前正在显示的
            if(m_MessageItems!=null)
            {
                foreach (var item in m_MessageItems)
                {
                    item.Clear();
                }
                m_MessageItems.Clear();
            }
          

            //当前池子中的
            if(m_MessageItemPools != null)
            {
                foreach (var pool in m_MessageItemPools)
                {
                    while (pool.Count > 0)
                    {
                        var item = pool.Pop();
                        item.Clear();
                    }
                }
                m_MessageItemPools.Clear();
            }
        }

        public ChatMessageItem GetHeadMessage()
        {
            if(m_MessageItems.Count>0)
            {
                return m_MessageItems.First.Value;
            }

            return null;
        }

        public ChatMessageItem GetTailMessage()
        {
            if (m_MessageItems.Count > 0)
            {
                return m_MessageItems.Last.Value;
            }

            return null;
        }

        public void Log(string content)
        {
            Debug.Log($"[ChatView]{viewName}#{m_FrameNum}: {content}", gameObject);
        }

        public void Error(string content)
        {
            Debug.LogError($"[ChatView]{viewName}#{m_FrameNum}: {content}", gameObject);
        }
    }

}