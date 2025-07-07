/*******************************************************************
** 文件名:    ButtonAnimation.cs
** 版  权:    (C) 深圳冰川网络网络科技有限公司 2016 - Speed
** 创建人:    杨霜晴
** 日  期:    2016/2/29
** 版  本:    1.0
** 描  述:    按钮动画类
** 应  用:    适用于需要添加动画的按钮

**************************** 修改记录 ******************************
** 修改人:    
** 日  期:    
** 描  述:    
********************************************************************/
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XGame.Effect;
using XGame.Anim.Tween;

namespace XClient.UI
{
    public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TweenScale m_iconClick;
        public TweenImageAlpha m_bgAlpha;
        public TweenScale m_bgScale;

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (m_iconClick != null)
            {
                m_iconClick.Play();
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (m_iconClick != null)
            {
                m_iconClick.PlayFrom();
            }

            if (m_bgAlpha != null)
            {
                m_bgAlpha.Play();
            }

            if (m_bgScale != null)
            {
                m_bgScale.Play();
            }
        }
    }
}
