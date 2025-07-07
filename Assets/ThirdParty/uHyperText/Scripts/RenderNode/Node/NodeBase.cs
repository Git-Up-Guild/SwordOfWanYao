using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WXB
{
    public enum EffectType
    {
        Null, // 无类型

        // 特效类型
        Shadow, // 阴影
        Outline, // 描边
    }

    /// <summary>
    /// 节点基类
    /// </summary>
    public abstract class NodeBase
    {
        public IOwner owner;
        public string id = string.Empty;

        //下一行的偏移量
        float d_nextLineX = 0;

        //下一行的实际偏移量
        protected float NextLineX = 0;

        public virtual void Reset(IOwner o, Anchor hf)
        {
            d_bNewLine = false;
            owner = o;
            formatting = hf;
            d_nextLineX = 0;
        }

        protected static float AlignedFormatting(IOwner owner, Anchor formatting, float maxWidth, float curWidth, float lineX)
        {
            if (formatting == Anchor.Null)
                formatting = owner.anchor;

            float value = 0.0f;
            switch (formatting)
            {
            case Anchor.UpperRight:
            case Anchor.MiddleRight:
            case Anchor.LowerRight:
                value = (maxWidth - curWidth);
                break;

            case Anchor.UpperLeft:
            case Anchor.MiddleLeft:
            case Anchor.LowerLeft:
                value = lineX;
                break;

            case Anchor.MiddleCenter:
            case Anchor.UpperCenter:
            case Anchor.LowerCenter:
                value = (maxWidth - curWidth) / 2;
                break;
            }

            return value;
        }

        public abstract float getHeight();

        public abstract float getWidth();

        public void setNewLine(bool line)
        {
            d_bNewLine = line;
        }

        public bool isNewLine()
        {
            return d_bNewLine;
        }

        public Anchor formatting = Anchor.Null;

        public abstract void render(float maxWidth, RenderCache cache, ref float x, ref uint yline, List<Line> lines, float offsetX, float offsetY);

        protected virtual void AlterX(ref float x, float maxWidth)
        {
        }

        /// <summary>
        /// 填充到Line中
        /// </summary>
        /// <param name="currentpos">当前顶部偏移</param>
        /// <param name="lines">行列表</param>
        /// <param name="maxWidth">最大宽度限制，超过这个宽度要进行丢弃或者换行等处理</param>
        /// <param name="pixelsPerUnit"></param>
        public virtual void FillToLine(ref Vector2 currentpos, List<Line> lines, float maxWidth, float pixelsPerUnit)
        {
            NextLineX = d_nextLineX * pixelsPerUnit;

            //获取子节点的元素的列表。此时的元素列表已经是根据分隔符分隔好的。一个节点由很多的元素组成
            List<Element> elements;
            UpdateElements(out elements, pixelsPerUnit);

            //获取节点的高度
            float height = getHeight();

            AlterX(ref currentpos.x, maxWidth);

            //没有元素
            if (elements.Count == 0)
                return;

            Around around = owner.around;

            // 当前行是否包含此元素
            bool isContain = false; 
            for (int i = 0; i < elements.Count;)
            {
                float totalwidth = elements[i].totalwidth;
                float newx;

                //超过了最大宽度了，需要添加到下一行
                if (((currentpos.x + totalwidth) > maxWidth))
                {
                    currentpos = elements[i].Next(this, currentpos, lines, maxWidth, NextLineX, height, around, totalwidth, ref isContain);
                    ++i;
                }
                //TODO: //这里没看明白，为什么与图片重叠的时候这里就当做放下了呢？
                else if (around != null && !around.isContain(currentpos.x, currentpos.y, totalwidth, height, out newx))
                {
                    // 放置不下了
                    currentpos.x = newx;
                }
                //可以在放在本行
                else
                {
                    currentpos.x += totalwidth;
                    isContain = true;
                    ++i;
                }
            }

            Line bl = lines.back();
            bl.x = currentpos.x;
            bl.y = Mathf.Max(height, bl.y);

            if (d_bNewLine)
            {
                lines.Add(UHyperTextFactory.CreateLine(Vector2.zero));
                currentpos.y += height;
                currentpos.x = NextLineX;
            }
        }

        // 一个元素，此元素尽量要在同一行显示，如果当前是在行首，一行还放不下，那只能分行处理了
        public struct Element
        {
            /// <summary>
            /// 构建元素
            /// </summary>
            /// <param name="ws">宽度列表</param>
            public Element(List<float> ws)
            {
#if UNITY_EDITOR
                text = string.Empty;
#endif
                widthList = ws;
                totalWidth = 0f;
                for (int i = 0; i < widthList.Count; ++i)
                {
                    totalWidth += ws[i];
                }
            }

            public Element(float width)
            {
#if UNITY_EDITOR
                text = string.Empty;
#endif
                totalWidth = width;

                widthList = null;
            }

            List<float> widthList;

            float totalWidth;

            public float totalwidth
            {
                get
                {
                    return totalWidth;
                }
            }

            public List<float> widths
            {
                get { return widthList; }
            }

            public int count
            {
                get { return widthList == null ? 1 : widthList.Count; }
            }

#if UNITY_EDITOR
            public string text;

            public override string ToString()
            {
                return string.Format("text:{0} w:{1}", text, totalwidth);
            }
#endif

            /// <summary>
            /// 换行处理
            /// </summary>
            /// <param name="n">要换行的节点</param>
            /// <param name="currentPos">在行中插入的位置</param>
            /// <param name="lines">行列表</param>
            /// <param name="maxWidth">最大宽度</param>
            /// <param name="lineOffsetX">行偏移</param>
            /// <param name="nodeHeight">节点的高度</param>
            /// <param name="round"></param>
            /// <param name="tw">元素的总宽度</param>
            /// <param name="currentLineContain">当前行是否包含此节点</param>
            /// <returns></returns>
            public Vector2 Next(NodeBase n, Vector2 currentPos, List<Line> lines, float maxWidth, float lineOffsetX, float nodeHeight, 
                Around round, float tw, ref bool currentLineContain)
            {
                //当前行已经存在元素了，则进行换行处理
                if (currentPos.x != 0f)
                {
                    //获得当前行
                    Line bl = lines.back();
                    bl.x = currentPos.x;

                    //当前节点的高度作为行高
                    if (currentLineContain)
                        bl.y = Mathf.Max(bl.y, nodeHeight);

                    currentLineContain = false;

                    //设置新行的偏移
                    currentPos.x = lineOffsetX;
                    currentPos.y += bl.y;

                    //添加新的行
                    lines.Add(UHyperTextFactory.CreateLine(new Vector2(lineOffsetX, 0)));
                }
                else
                {
                    // 当前行没有数据，直接在此行处理
                }

                if (round != null)
                {
                    float newx;
                    while (!round.isContain(currentPos.x, currentPos.y, tw, nodeHeight, out newx))
                    {
                        currentPos.x = newx;
                        if (currentPos.x + tw > maxWidth)
                        {
                            currentPos.x = lineOffsetX;
                            lines.Add(UHyperTextFactory.CreateLine(new Vector2(lineOffsetX, nodeHeight)));
                            currentPos.y += nodeHeight;
                        }
                    }
                }

                if (widthList != null)
                {
                    for (int i = 0; i < widthList.Count; ++i)
                    {
                        currentPos = Add(n, currentPos, widthList[i], maxWidth, lineOffsetX, lines, nodeHeight, ref currentLineContain);
                    }
                }
                else
                {
                    currentPos = Add(n, currentPos, totalWidth, maxWidth, lineOffsetX, lines, nodeHeight, ref currentLineContain);
                }

                lines.back().x = currentPos.x;

                return currentPos;
            }

            Vector2 Add(NodeBase n, Vector2 currentPos, float width, float maxWidth, float lineX, List<Line> lines, float height, ref bool currentLineContain)
            {
                if (currentPos.x + width > maxWidth)
                {
                    // 需要换新行了
                    Line bl = lines.back();
                    bl.x = currentPos.x;
                    if (currentLineContain)
                        bl.y = Mathf.Max(bl.y, height);

                    currentPos.x = lineX + width;
                    lines.Add(UHyperTextFactory.CreateLine(new Vector2(currentPos.x, height)));
                    currentPos.y += height;
                }
                else
                {
                    currentPos.x += width;
                }

                currentLineContain = true;
                return currentPos;
            }
        }

        protected static List<Element> s_TempElements = new List<Element>();

        protected virtual void UpdateElements(out List<Element> elements, float pixelsPerUnit)
        {
            s_TempElements.Clear();
            s_TempElements.Add(new Element(getWidth()));
            elements = s_TempElements;
        }

        public virtual void onMouseEnter()
        {
        }

        public virtual void onMouseLeave()
        {
        }

        //是否为新的行
        protected bool d_bNewLine;

        public bool d_bBlink; // 是否闪烁
        public bool d_bOffset; // 偏移效果

        public Rect d_rectOffset; // 偏移范围

        public Color d_color;

        public LineAlignment lineAlignment = LineAlignment.Default;

        // 用户数据
        public object userdata { get; set; }

        public long keyPrefix
        {
            get
            {
                long key = 0;
                if (d_bBlink)
                    key = 1 << 63;

                if (d_bOffset)
                {
                    key += 1 << 62;
                    key += ((byte)(d_rectOffset.xMin)) << 58;
                    key += ((byte)(d_rectOffset.xMax)) << 54;
                    key += ((byte)(d_rectOffset.yMin)) << 50;
                    key += ((byte)(d_rectOffset.yMax)) << 46;
                }

                return key;
            }
        }

        public virtual void SetConfig(ref TextParser.Config c)
        {
            d_nextLineX = c.nextLineX;
            d_bBlink = c.isBlink;
            lineAlignment = c.lineAlignment;

            d_color = c.fontColor;
            d_bOffset = c.isOffset;
            if (c.isOffset)
                d_rectOffset = c.offsetRect;
            else
                d_rectOffset.Set(0, 0, 0, 0);
        }

        public virtual void Release()
        {
            d_color = Color.white;
            d_bNewLine = false;
            owner = null;
            formatting = Anchor.Null;
            d_bBlink = false;
            d_bOffset = false;
            d_rectOffset.Set(0, 0, 0, 0);
            userdata = null;
        }
    };
}