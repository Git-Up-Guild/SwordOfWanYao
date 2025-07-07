/*******************************************************************
** 文件名:	LuaFowTextView.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2019/10/23 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using UnityEngine;
using XGame.Attr;
using XGame.FlowText;
using XGame.LOP;

namespace XClient.FlowText
{
    public class CustomFowTextView : MonoBehaviour, IFlowTextView
    {
        //唯一ID
        public static int MAX_ID = 1;

        //创建回调<ID, 类型, 用户数据, LOPID>
        public static Action<int, int, int, int> fnCreate;

        //销毁回调<ID>
        public static Action<int> fnDestroy;

        //更新内容<ID, 内容>
        public static Action<int, string> fnUpdateContent;

        //状态变更回调<ID,状态ID,状态时长>
        public static Action<int, int, float> fnStateChange;

        [Label("类型")]
        [SerializeField]
        private int m_FlowViewType;

        [Label("用户数据")]
        [SerializeField]
        private int m_UserData;

        //如果要支持Alpha则必须要有这个组件
        private CanvasGroup m_CanvasGroup;

        private bool m_IsInited = false;

        private float m_BakeAlpha;
        private Vector3 m_BakeScale;
        private Vector3 m_BakePosition;
        private Vector3 m_BakeLocalPosition;

        private int m_ID;
        private int m_LOPID;

        public FlowTextNode flowText { get; set; }
        public void OnStateChange()
        {
            float duration = 0f;
            if (flowText.currentAnimationSettings != null)
                duration = flowText.currentAnimationSettings.duration;
            fnStateChange?.Invoke(m_ID, (int)flowText.state, duration);
        }

        private void Initialize()
        {
            if (!m_IsInited)
            {
                m_ID = MAX_ID++;
                m_CanvasGroup = GetComponent<CanvasGroup>();
                m_BakeScale = transform.localScale;
                m_BakePosition = transform.position;
                m_BakeLocalPosition = transform.localPosition;

                if (m_CanvasGroup != null)
                    m_BakeAlpha = m_CanvasGroup.alpha;

                m_IsInited = true;

                if (m_LOPID == 0)
                    m_LOPID = LOPObjectRegister.Register(gameObject);

                fnCreate?.Invoke(m_ID, m_FlowViewType, m_UserData, m_LOPID);
            }
        }

        public float GetAlpha()
        {
            if (m_CanvasGroup != null)
                return m_CanvasGroup.alpha;
            return 1f;
        }

        public Color GetColor()
        {
            return Color.white;
        }

        public float GetOrginAlpha()
        {
            return m_BakeAlpha;
        }

        public Vector3 GetOrginLocalPosition()
        {
            return m_BakeLocalPosition;
        }

        public Vector3 GetOrginPosition()
        {
            return m_BakePosition;
        }

        public Vector3 GetOrginScale()
        {
            return m_BakeScale;
        }

        public void InitView()
        {
            Initialize();
        }

        public void ResetView()
        {
            if(m_IsInited)
            {
                m_IsInited = false;

                fnDestroy?.Invoke(m_ID);

                transform.localScale = m_BakeScale;
                transform.localPosition = m_BakeLocalPosition;
                transform.position = m_BakePosition;

                if(m_CanvasGroup != null)
                {
                    m_CanvasGroup.alpha = m_BakeAlpha;
                    m_CanvasGroup = null;
                }

                if (m_LOPID > 0)
                {
                    LOPObjectRegister.UnRegister(m_LOPID);
                    m_LOPID = 0;
                }
            }
        }

        public void SetAlpha(float alpha)
        {
            if (m_CanvasGroup != null)
                m_CanvasGroup.alpha = alpha;
        }

        public void SetColor(Color newColor)
        {
        }

        public void UpdateContent(FlowTextContent content)
        {
            fnUpdateContent?.Invoke(m_ID, content.text);
        }

        public FlowTextContent GetDefaultContent()
        {
            FlowTextContent content = new FlowTextContent();
            content.text = string.Empty;
            return content;
        }
    }
}