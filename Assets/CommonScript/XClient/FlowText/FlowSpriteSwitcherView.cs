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

using UnityEngine;
using UnityEngine.UI;
using XGame.FlowText;
using XGame.UI;

namespace XClient.FlowText
{
    [RequireComponent(typeof(SpriteSwitcher))]
    public class FlowSpriteSwitcherView : MonoBehaviour, IFlowTextView
    {
        private SpriteSwitcher m_Switcher;
        private Image m_Image;
        private bool m_IsInited = false;

        private Color m_BakeColor;
        private Vector3 m_BakeScale;
        private Vector3 m_BakePosition;
        private Vector3 m_BakeLocalPosition;
        private Sprite m_DefSprite;

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
                m_IsInited = true;
                m_Image = GetComponent<Image>();
                m_Switcher = GetComponent<SpriteSwitcher>();

                m_BakeColor = m_Image.color;
                m_BakeScale = transform.localScale;
                m_BakePosition = transform.position;
                m_BakeLocalPosition = transform.localPosition;
                m_DefSprite = m_Image.sprite;
            }
        }

        private void OnDestroy()
        {
        }

        public void InitView()
        {
            Initialize();
            ResetView();
        }

        public float GetOrginAlpha()
        {
            return m_BakeColor.a;
        }

        public Vector3 GetOrginPosition()
        {
            return m_BakePosition;
        }

        public Vector3 GetOrginScale()
        {
            return m_BakeScale;
        }

        public Vector3 GetOrginLocalPosition()
        {
            return m_BakeLocalPosition;
        }

        public void UpdateContent(FlowTextContent ctx)
        {
            m_Switcher.Switch(ctx.iData);
        }

        public void SetColor(Color newColor)
        {
            if (m_Image != null)
            {
                m_Image.color = newColor;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (m_Image != null)
            {
                var color = m_Image.color;
                color.a = alpha;
                m_Image.color = color;
            }
        }

        public Color GetColor()
        {
            if (m_Image != null)
                return m_Image.color;
            return Color.white;
        }

        public float GetAlpha()
        {
            if (m_Image != null)
                return m_Image.color.a;
            return 0f;
        }

        public void ResetView()
        {
            m_Image.color = m_BakeColor;
            m_Image.sprite = m_DefSprite;
            transform.localScale = m_BakeScale;
            transform.localPosition = m_BakeLocalPosition;
        }

        public FlowTextContent GetDefaultContent()
        {
            FlowTextContent ctx = new FlowTextContent();
            ctx.iData = 0;
            return ctx; 
        }
    }
}