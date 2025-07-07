﻿using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace WXB
{
    public partial class TextNode : NodeBase
	{
        public bool isFontSame(TextNode n)
        {
            if (d_font == n.d_font && d_fontSize == n.d_fontSize && d_fontStyle == n.d_fontStyle)
                return true;

            return false;
        }

        public override void Reset(IOwner o, Anchor hf)
        {
            base.Reset(o, hf);

            d_font = null;
            d_bUnderline = false;
            d_bStrickout = false;
        }

		protected virtual bool isUnderLine()
		{
			return d_bUnderline;
		}

        public virtual Color currentColor
        {
            get { return d_color; }
        }

        public override float getHeight()
        {
            return m_Size.y;
        }

        public override float getWidth()
        {
            return m_Size.x;
        }

        //节点的尺寸
        private Vector2 m_Size = Vector2.zero;

        //元素列表
        private List<Element> m_Elements = new List<Element>();

        //更新元素列表
        protected override void UpdateElements(out List<Element> elements, float pixelsPerUnit)
        {
            elements = m_Elements;

            m_Elements.Clear();

            if (d_text.Length == 0)
                return;

            float unitsPerPixel = 1f / pixelsPerUnit;
            int fontsize = (int)(d_fontSize * pixelsPerUnit);
            m_Size.x = 0;
            m_Size.y = FontCache.GetLineHeight(d_font, fontsize, d_fontStyle) * unitsPerPixel;

            //获得字符宽度的函数
            Func<char, float> fontwidth = (char code) => { return FontCache.GetAdvance(d_font, fontsize, d_fontStyle, code) * unitsPerPixel; };

            ElementSegment es = owner.elementSegment;
            if (es == null)
            {
                for (int i = 0; i < d_text.Length; ++i)
                {
                    var e = new Element(fontwidth(d_text[i]));
#if UNITY_EDITOR
                    e.text = "" + d_text[i];
#endif
                    elements.Add(e);
                }
            }
            else
            {
                es.Segment(d_text, elements, fontwidth);
            }

            for (int i = 0; i < m_Elements.Count; ++i)
                m_Size.x += m_Elements[i].totalwidth;

            //size.x *= pixelsPerUnit;
            //size.y *= pixelsPerUnit;
        }

        public virtual bool IsHyText()
        {
            return false;
        }

        public override void render(float maxWidth, RenderCache cache, ref float x, ref uint yline, List<Line> lines, float offsetX, float offsetY)
		{
            if (d_font == null)
                return;

            using (PD<StringBuilder> psb = Pool.GetSB())
            {
                TextNodeHelper helper = new TextNodeHelper(maxWidth, cache, x, yline, lines, formatting, offsetX, offsetY, psb.value);
                helper.Draw(this, NextLineX);

                x = helper.x;
                yline = helper.yline;
            }
        }

		public string d_text;
        public Font d_font;
        public int d_fontSize;
        public FontStyle d_fontStyle;
        public bool d_bUnderline;
        public bool d_bStrickout;
        public bool d_bDynUnderline;
        public bool d_bDynStrickout;
        public int d_dynSpeed;

        public EffectType effectType;
        public Color effectColor;
        public Vector2 effectDistance;

        public new long keyPrefix
        {
            get
            {
                long key = base.keyPrefix;
                if (d_bDynStrickout)
                    key += 1 << 45;

                if (d_bDynUnderline)
                    key += 1 << 45;

                return key;
            }
        }

        public override void SetConfig(ref TextParser.Config c)
        {
            base.SetConfig(ref c);

            d_font = c.font;
            d_bUnderline = c.isUnderline;
            d_fontSize = c.fontSize;
            d_fontStyle = c.fontStyle;
            d_bStrickout = c.isStrickout;
            d_bDynUnderline = c.isDyncUnderline;
            d_bDynStrickout = c.isDyncStrickout;
            d_dynSpeed = c.dyncSpeed;

            effectType = c.effectType;
            effectColor = c.effectColor;
            effectDistance = c.effectDistance;
        }

        public void GetLineCharacterInfo(out CharacterInfo info)
        {
            if (!d_font.GetCharacterInfo('_', out info, 20, FontStyle.Bold))
            {
                d_font.RequestCharactersInTexture("_", 20, FontStyle.Bold);
                d_font.GetCharacterInfo('_', out info, 20, FontStyle.Bold);
            }
        }

        public override void Release()
        {
            base.Release();

            d_text = null;
            d_font = null;
            d_fontSize = 0;
            d_bUnderline = false;
            d_bDynUnderline = false;
            d_bDynStrickout = false;
            d_dynSpeed = 0;
        }
	};
}