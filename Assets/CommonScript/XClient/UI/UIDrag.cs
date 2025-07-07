/*******************************************************************
** 文件名:	
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	许德纪
** 日  期:	2020/9/23 20:29:12
** 版  本:	1.0
** 描  述:	
** 应  用:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using XGame.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XClient.UI
{

    public class UIDrag : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {

        //是否自动位移
        public bool autoMove = false;

        //开始拖动回调
        private UnityAction<float, float> m_BeginCallback;

        //拖动中回调
        private UnityAction<float, float> m_DragingCallback;

        //拖动结束回调
        private UnityAction m_FinishCallback;

        //拖拽移动偏移回调
        private UnityAction<float, float> m_DragOffsetCallback;

        //起始位置
        private Vector2 m_StartPos = Vector2.zero;

        private RectTransform m_CurRecTran;

        private void Awake()
        {
            m_CurRecTran = transform.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 开始拖动
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            m_StartPos = eventData.position;
            m_BeginCallback?.Invoke(eventData.position.x, eventData.position.y);
        }

        /// <summary>
        /// 拖动
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            m_DragingCallback?.Invoke(eventData.position.x, eventData.position.y);
            var offset = eventData.position - m_StartPos;

            if (autoMove)
            {
                m_CurRecTran.anchoredPosition += eventData.delta;
            }

            m_DragOffsetCallback?.Invoke(offset.x, offset.y);

            

        }

        /// <summary>
        /// 结束拖动
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            m_FinishCallback?.Invoke();
            m_StartPos = Vector2.zero;
        }

        /// <summary>
        /// 添加开始拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void AddDragOffsetListener(UnityAction<float, float> func)
        {
            m_DragOffsetCallback = func;
        }

        /// <summary>
        /// 移除拖拽偏移回调
        /// </summary>
        /// <param name="func"></param>
        public void RemoveDragOffsetListener()
        {
            m_DragOffsetCallback = null;
        }

        /// <summary>
        /// 添加开始拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void AddBeginDragListener(UnityAction<float, float> func)
        {
            m_BeginCallback = func;
        }

        /// <summary>
        /// 移除开始拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void RemoveBeginDragListener()
        {
            m_BeginCallback = null;
        }

        /// <summary>
        /// 添加正在拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void AddDragingListener(UnityAction<float, float> func)
        {
             m_DragingCallback = func;
        }

        /// <summary>
        /// 移除正在拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void RemveDragingListener()
        {
            m_DragingCallback = null;
        }

        /// <summary>
        /// 添加结束拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void AddFinishDragListener(UnityAction func)
        {
            m_FinishCallback = func;
        }

        /// <summary>
        /// 移除结束拖动回调
        /// </summary>
        /// <param name="func"></param>
        public void RemoveFinishDragListener()
        {
            m_FinishCallback = null;
        }
    }

}
