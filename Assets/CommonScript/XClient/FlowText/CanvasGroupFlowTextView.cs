/*******************************************************************
** 文件名:	CanvasGroupFlowTextView.CS
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.FlowText;

namespace XClient.FlowText
{
    public class CanvasGroupFlowTextView : MonoBehaviour, IFlowTextView
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private TextMeshProUGUI textMeshPro;

        [SerializeField]
        private Text text;

        public FlowTextNode flowText { get; set; }

        private float m_OrginAlpha;
        private Vector3 m_OrginPosition;
        private Vector3 m_OrginLocalPosition;
        private Vector3 m_OrginScale;
        private Color m_OrginColor;

        public float GetAlpha()
        {
            if (canvasGroup == null)
                return 1f;

            return canvasGroup.alpha;
        }

        public Color GetColor()
        {
            return Color.white;
        }

        public float GetOrginAlpha()
        {
            return m_OrginAlpha;
        }

        public Vector3 GetOrginLocalPosition()
        {
            return m_OrginLocalPosition;
        }

        public Vector3 GetOrginPosition()
        {
            return m_OrginPosition;
        }

        public Vector3 GetOrginScale()
        {
            return m_OrginScale;
        }

        public void InitView()
        {
            m_OrginScale = transform.localScale;

            if(canvasGroup != null)
                m_OrginAlpha = canvasGroup.alpha;
            else
                m_OrginAlpha = 1f;

            m_OrginPosition = transform.position;
            m_OrginLocalPosition = transform.localPosition;

            if (text != null)
                m_OrginColor = text.color;

            if (textMeshPro != null)
                m_OrginColor = textMeshPro.color;
        }

        public void ResetView()
        {
            transform.position = m_OrginPosition;
            transform.localPosition = m_OrginLocalPosition;
            transform.localScale = m_OrginScale;
            if (canvasGroup != null)
                canvasGroup.alpha = m_OrginAlpha;

            if (text != null)
                text.color = m_OrginColor;

            if (textMeshPro != null)
                textMeshPro.color = m_OrginColor;
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
        }

        public void SetColor(Color newColor)
        {
            if (text != null)
                text.color = newColor;

            if (textMeshPro != null)
                textMeshPro.color = newColor;
        }

        public void UpdateContent(FlowTextContent ctx)
        {
            if (string.IsNullOrEmpty(ctx.text))
                return;

            if (textMeshPro != null)
                textMeshPro.text = ctx.text;

            if (text != null)
                text.text = ctx.text;
        }

        public void OnStateChange()
        {
        }

        public FlowTextContent GetDefaultContent()
        {
            FlowTextContent ctx = new FlowTextContent();

            if (textMeshPro != null)
                ctx.text = textMeshPro.text;

            if (text != null)
                ctx.text = text.text;

            return ctx;
        }
    }
}
