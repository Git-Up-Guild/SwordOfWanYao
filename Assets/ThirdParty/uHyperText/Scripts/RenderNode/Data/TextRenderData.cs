﻿using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 618

namespace WXB
{
    public class TextRenderData : BaseRenderData
    {
        public void Reset(TextNode n, string t, Rect r, Line l)
        {
            //关联的节点
            node = n;

            //要渲染的文本
            text = t;

            //渲染的区域
            rect = r;

            //所在的行
            line = l;
        }

        protected override void OnRelease()
        {
            text = null;
        }

        public override bool isAlignByGeometry
        {
            get { return true; }
        }

        public TextNode Node
        {
            get { return (TextNode)node; }
            set { node = value; }
        }

        public string text;

        static TextAnchor SwitchTextAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    return TextAnchor.UpperLeft;

                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    return TextAnchor.MiddleCenter;

                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    return TextAnchor.LowerLeft;
            }

            return TextAnchor.MiddleCenter;
        }

        const float line_height = 2f;

        public override void OnLineYCheck(float pixelsPerUnit)
        {
            if (line == null)
                return;

            //if (line.y == Node.getHeight())
            //    return;

            var node = Node;
            Font font = node.d_font;
            FontStyle fs = node.d_fontStyle;
            int fontSize = (int)((node.d_fontSize * pixelsPerUnit));
            font.RequestCharactersInTexture(text, fontSize, fs);

            for (int i = 0; i < text.Length; ++i)
            {
                if (font.GetCharacterInfo(text[i], out s_Info, fontSize, fs))
                {
                    line.minY = Mathf.Min(s_Info.minY, line.minY);
                    line.maxY = Mathf.Max(s_Info.maxY, line.maxY);
                }
            }
        }

        public override void OnAlignByGeometry(ref Vector2 offset, float pixelsPerUnit)
        {
            if (line == null)
                return;

            var node = Node;
            Font font = node.d_font;
            FontStyle fs = node.d_fontStyle;
            int fontSize = (int)((node.d_fontSize * pixelsPerUnit));
            font.RequestCharactersInTexture(text, fontSize, fs);

            if (rect.x == 0)
            {
                // 第一个元素
                if (font.GetCharacterInfo(text[0], out s_Info, fontSize, fs))
                    offset.x = s_Info.minX;
            }

            float y = float.MinValue;
            for (int i = 0; i < text.Length; ++i)
            {
                if (font.GetCharacterInfo(text[i], out s_Info, fontSize, fs))
                {
                    y = Mathf.Max(s_Info.maxY, offset.y);
                }
            }

            offset.y = Mathf.Max(offset.y, line.y - y);
        }

        static CharacterInfo s_Info;

        public override Vector2 GetStartLeftBottom(float unitsPerPixel)
        {
            Vector2 leftBottomPos = new Vector2(rect.x, rect.y + rect.height);
            var la = lineAlignment;
            switch (la)
            {
                case LineAlignment.Top:
                    leftBottomPos.y = rect.y + line.maxY * unitsPerPixel;
                    break;
                case LineAlignment.Center:
                    {
                        leftBottomPos.y = rect.y + line.maxY * unitsPerPixel;

                        leftBottomPos.y += (line.y - (line.maxY - line.minY) * unitsPerPixel) * 0.5f;
                    }
                    break;

                case LineAlignment.Bottom:
                    leftBottomPos.y = rect.y + line.y + line.minY * unitsPerPixel;
                    break;
            }

            return leftBottomPos;
        }

        public override void Render(VertexHelper vh, Rect area, Vector2 offset, float pixelsPerUnit)
        {
            var node = Node;

            //文本颜色
            Color currentColor = node.currentColor;
            if (currentColor.a <= 0.01f)
                return;

            //插入顶点的其实索引
            int start = vh.currentIndexCount;
            float unitsPerPixel = 1f / pixelsPerUnit;

            Vector2 leftBottomPos = GetStartLeftBottom(unitsPerPixel) + offset;
            Tools.LB2LT(ref leftBottomPos, area.height);

            // 渲染文本
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            //leftPos = Vector2.zero;
            Font font = node.d_font;
            FontStyle fs = node.d_fontStyle;
            int fontSize = (int)((node.d_fontSize * pixelsPerUnit));

            //请求字符渲染纹理
            font.RequestCharactersInTexture(text, fontSize, fs);

            Vector2 startPos = Vector2.zero;

            for (int i = 0; i < text.Length; ++i)
            {
                if (font.GetCharacterInfo(text[i], out s_Info, fontSize, fs))
                {
                    int startVertCount = vh.currentVertCount;

                    //计算字体的渲染矩形
                    Rect cr = Rect.MinMaxRect(
                        leftBottomPos.x + ((startPos.x + s_Info.minX) * unitsPerPixel),
                        leftBottomPos.y + ((startPos.y + s_Info.minY) * unitsPerPixel),
                        leftBottomPos.x + ((startPos.x + s_Info.maxX) * unitsPerPixel),
                        leftBottomPos.y + ((startPos.y + s_Info.maxY) * unitsPerPixel));

                    //添加字符的四个顶点
                    vh.AddVert(new Vector3(cr.xMin, cr.yMax), currentColor, s_Info.uvTopLeft);
                    vh.AddVert(new Vector3(cr.xMax, cr.yMax), currentColor, s_Info.uvTopRight);
                    vh.AddVert(new Vector3(cr.xMax, cr.yMin), currentColor, s_Info.uvBottomRight);
                    vh.AddVert(new Vector3(cr.xMin, cr.yMin), currentColor, s_Info.uvBottomLeft);

                    //添加渲染三角形
                    vh.AddTriangle(startVertCount + 0, startVertCount + 1, startVertCount + 2);
                    vh.AddTriangle(startVertCount + 2, startVertCount + 3, startVertCount + 0);

                    //累加字符宽度
                    startPos.x += (s_Info.advance);

                    minY = Mathf.Min(cr.yMin, minY);
                    maxY = Mathf.Max(cr.yMax, maxY);
                }
            }

            //下划线处理
            bool isset = false;
            if (node.d_bDynUnderline || node.d_bUnderline)
            {
                isset = true;
                node.GetLineCharacterInfo(out s_Info);

                Vector2 startpos = new Vector2(leftBottomPos.x, minY);

                if (node.d_bDynUnderline)
                {
                    Texture mainTexture = node.d_font.material.mainTexture;
                    OutlineDraw draw = node.owner.GetDraw(DrawType.Outline, node.keyPrefix + mainTexture.GetInstanceID(), (IDraw d, object p) =>
                    {
                        OutlineDraw od = d as OutlineDraw;
                        od.isOpenAlpha = node.d_bBlink;
                        od.texture = mainTexture;
                    }) as OutlineDraw;

                    draw.AddLine(node, startpos, rect.width, line_height * unitsPerPixel, currentColor, s_Info.uv.center, node.d_dynSpeed);
                }
                else
                {
                    Tools.AddLine(vh, startpos, s_Info.uv.center, rect.width, line_height * unitsPerPixel, currentColor);
                }
            }

            //处理删除线
            if (node.d_bDynStrickout || node.d_bStrickout)
            {
                if (!isset)
                {
                    isset = true;
                    node.GetLineCharacterInfo(out s_Info);
                }

                Vector2 startpos = new Vector2(leftBottomPos.x, (maxY + minY) * 0.5f + line_height * 0.5f * unitsPerPixel);

                if (node.d_bDynStrickout)
                {
                    Texture mainTexture = node.d_font.material.mainTexture;
                    OutlineDraw draw = node.owner.GetDraw(DrawType.Outline, node.keyPrefix + mainTexture.GetInstanceID(), (IDraw d, object p) =>
                    {
                        OutlineDraw od = d as OutlineDraw;
                        od.isOpenAlpha = node.d_bBlink;
                        od.texture = mainTexture;
                    }) as OutlineDraw;
                    draw.AddLine(node, startpos, rect.width, line_height * unitsPerPixel, currentColor, s_Info.uv.center, node.d_dynSpeed);
                }
                else
                {
                    Tools.AddLine(vh, startpos, s_Info.uv.center, rect.width, line_height * unitsPerPixel, currentColor);
                }
            }

            //处理描边和阴影效果
            switch (node.effectType)
            {
                case EffectType.Outline:
                    Effect.Outline(vh, start, node.effectColor, node.effectDistance);
                    break;
                case EffectType.Shadow:
                    Effect.Shadow(vh, start, node.effectColor, node.effectDistance);
                    break;
            }
        }

        public override void OnMouseEnter()
        {
            node.onMouseEnter();
        }

        public override void OnMouseLevel()
        {
            node.onMouseLeave();
        }
    }
}