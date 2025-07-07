/*******************************************************************
** 文件名:	TMPFlowSpriteView.CS
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

using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using XGame.FlowText;

namespace XClient.FlowText
{
    [System.Serializable]
    public class FlowTextSprite
    {
        public char c;
        public int spIndex;
    }

    public class TMPFlowSpriteView : MonoBehaviour, IFlowTextView
    {
        public TextMeshProUGUI textMeshPro;
        public List<FlowTextSprite> chars;
        private Dictionary<char, int> m_Char2Sprite = null;
        private StringBuilder m_StrBuilder;
        public string defaultContent;

        private bool m_IsInited = false;
        private Color m_BakeColor;
        private Vector3 m_BakeScale;
        private Vector3 m_BakePosition;
        private Vector3 m_BakeLocalPosition;

        public FlowTextNode flowText { get; set; }
        public void OnStateChange()
        {
        }

        private void Update()
        {
        }
        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if(!m_IsInited)
            {
                m_Char2Sprite = new Dictionary<char, int>();
                foreach (var f in chars)
                {
                    m_Char2Sprite.Add(f.c, f.spIndex);
                }
                m_StrBuilder = new StringBuilder();

                if(textMeshPro != null)
                {
                    m_BakeColor = textMeshPro.color;
                    m_BakeScale = transform.localScale;
                    m_BakePosition = transform.position;
                    m_BakeLocalPosition = transform.localPosition;
                }
                m_IsInited = true;
            }
        }

        private void OnDestroy()
        {
        }

        public void InitView()
        {
            if (textMeshPro == null)
                return;

            Initialize();
            ResetView();
        }

        public float GetOrginAlpha()
        {
            if (textMeshPro == null)
                return 0f;
            return m_BakeColor.a;
        }

        public Vector3 GetOrginPosition()
        {
            if (textMeshPro == null)
                return Vector3.zero;
            return m_BakePosition;
        }

        public Vector3 GetOrginScale()
        {
            if (textMeshPro == null)
                return Vector3.one;
            return m_BakeScale;
        }

        public void UpdateContent(FlowTextContent content)
        {
            if (textMeshPro == null)
                return;

            if (string.IsNullOrEmpty(content.text))
                return;

            m_StrBuilder.Clear();
            for (var i = 0; i < content.text.Length; ++i)
            {
                int spIndex;
                if (m_Char2Sprite.TryGetValue(content.text[i], out spIndex))
                {
                    m_StrBuilder.Append($"<sprite={spIndex}>");
                }
            }

            textMeshPro.text = m_StrBuilder.ToString();
        }

        public void SetColor(Color newColor)
        {
            if (textMeshPro == null)
                return;
            textMeshPro.color = newColor;
        }

        public void SetAlpha(float alpha)
        {
            if (textMeshPro == null)
                return;
            textMeshPro.alpha = alpha;
        }

        public Color GetColor()
        {
            if (textMeshPro == null)
                return Color.black;
            return textMeshPro.color;
        }

        public float GetAlpha()
        {
            if (textMeshPro == null)
                return 0f;
            return textMeshPro.alpha;
        }

        public void ResetView()
        {
            if (textMeshPro == null)
                return;

            textMeshPro.color = m_BakeColor;
            transform.localScale = m_BakeScale;
            transform.localPosition = m_BakeLocalPosition;
        }

        public Vector3 GetOrginLocalPosition()
        {
            return m_BakeLocalPosition;
        }

        public FlowTextContent GetDefaultContent()
        {
            FlowTextContent content = new FlowTextContent();
            content.text = textMeshPro.text;
            content.iData = 0;
            return content;
        }
    }
}