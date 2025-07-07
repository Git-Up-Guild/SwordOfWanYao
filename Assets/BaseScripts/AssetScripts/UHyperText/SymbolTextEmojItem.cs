/**************************************************************************    
文　　件：SymbolTextEmojItem
作　　者：郑秀程
创建时间：2022/6/11 12:53:53
描　　述：富文本点击处理
***************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using WXB;

namespace XGame.AssetScript.UHyperText
{
    public class SymbolTextEmojItem : Button
    {
        private string m_EmojName;
        private Image m_Image;
        private Cartoon m_Cartoon;
        private int m_CartoonSpriteIndex = -1;
        private float m_CartoonSpriteSwitchInterval = 0.1f;
        private float m_CartoonSpriteSwitchTick = 0f;

        public string emojName => m_EmojName;

        private Action<string> m_ClickListener;

        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(OnEmojItemClick);
            m_Image = GetComponent<Image>();
        }

        public void SetEmojName(string emojName, Action<string> clickListener)
        {
            m_EmojName = emojName;
            m_CartoonSpriteIndex = -1;
            m_ClickListener = clickListener;
            SetupCartoon();
            name = "Emoj_" + emojName;
        }

        public void Clear()
        {
            if (m_Image != null)
                m_Image.sprite = null;

            if (m_Cartoon != null)
                m_Cartoon = null;

            m_ClickListener = null;
        }

        private void SetupCartoon()
        {
            if (!string.IsNullOrEmpty(m_EmojName))
            {
                m_Cartoon = SymbolTextSettings.GetCartoon(m_EmojName);
                if(m_Cartoon != null && m_Cartoon.frameCount > 0)
                    m_CartoonSpriteSwitchInterval = 1f / m_Cartoon.fps;

                if (m_Image != null && m_Cartoon != null)
                    m_Image.sprite = m_Cartoon.GetSprite(0);
            }
        }

        private void OnEmojItemClick()
        {
            m_ClickListener?.Invoke(emojName);
        }

        private void Update()
        {
            if (m_Cartoon == null || m_Image == null || m_Cartoon.frameCount == 0)
                return;

            if(m_CartoonSpriteIndex == -1 || ((Time.time - m_CartoonSpriteSwitchTick) > m_CartoonSpriteSwitchInterval))
            {
                m_CartoonSpriteIndex++;
                if (m_CartoonSpriteIndex >= m_Cartoon.frameCount)
                    m_CartoonSpriteIndex = 0;
                m_CartoonSpriteSwitchTick = Time.time;
            }

            var sprite = m_Cartoon.GetSprite(m_CartoonSpriteIndex);
            m_Image.sprite = sprite;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }
    }
}