using System.Text;
using UnityEngine;
using System.Collections.Generic;

// 文本解析
namespace WXB
{
    /// <summary>
    /// 文本对齐方式
    /// </summary>
    public enum Anchor
    {
        UpperLeft = 0,
        UpperCenter = 1,
        UpperRight = 2,
        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,
        LowerLeft = 6,
        LowerCenter = 7,
        LowerRight = 8,
        Null,
    }

    public partial class TextParser
    {
        public T CreateNode<T>() where T : NodeBase, new()
        {
            return UHyperTextFactory.CreateNode<T>(mOwner, currentConfig.anchor);
        }

        static bool Get(char c, out Anchor a)
        {
            switch (c)
            {
                case '1': a = Anchor.MiddleLeft; return true;
                case '2': a = Anchor.MiddleCenter; return true;
                case '3': a = Anchor.MiddleRight; return true;
            }

            a = Anchor.MiddleCenter;
            return false;
        }

        static bool Get(char c, out LineAlignment a)
        {
            switch (c)
            {
                case '1': a = LineAlignment.Top; return true;
                case '2': a = LineAlignment.Center; return true;
                case '3': a = LineAlignment.Bottom; return true;
            }

            a = LineAlignment.Default;
            return false;
        }

        public TextParser()
        {
            clear();

            Reg();
            RegTag();
        }

        IOwner mOwner;

        static bool ParserInt(ref int d_curPos, string text, ref int value, int num = 3)
        {
            using (PD<StringBuilder> psb = Pool.GetSB())
            {
                StringBuilder sb = psb.value;
                d_curPos++;
                while (text.Length > d_curPos && ((text[d_curPos] >= '0' && text[d_curPos] <= '9')))
                {
                    sb.Append(text[d_curPos]);
                    d_curPos++;

                    if (sb.Length >= num)
                        break;
                }

                value = Tools.stringToInt(sb.ToString(), -1);
                if (sb.Length == 0)
                {
                    d_curPos--;
                    return false;
                }

                return true;
            }
        }

        static bool ParserFloat(ref int d_curPos, string text, ref float value, int num = 3)
        {
            using (PD<StringBuilder> psb = Pool.GetSB())
            {
                var sb = psb.value;
                d_curPos++;
                bool bInPoint = false;
                while (text.Length > d_curPos && ((text[d_curPos] >= '0' && text[d_curPos] <= '9') || (text[d_curPos] == '.')))
                {
                    if (text[d_curPos] == '.')
                        bInPoint = true;

                    sb.Append(text[d_curPos]);
                    d_curPos++;

                    int size = (bInPoint == true ? (num + 1) : num);
                    if (sb.Length >= size)
                        break;
                }

                value = Tools.stringToFloat(sb.ToString(), 0);
                if (sb.Length == 0)
                {
                    d_curPos--;
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 解析Tag名称的字符
        /// </summary>
        static readonly char[] s_TagNameChars = new char[] { ' ', '=' };

        /// <summary>
        /// 解析配置
        /// </summary>
        public struct Config
        {
            public Anchor anchor;
            public Font font;
            public FontStyle fontStyle;
            public int fontSize;
            public Color fontColor;
            public bool isUnderline;
            public bool isStrickout;
            public bool isBlink;
            public bool isDyncUnderline; // 动态下划线
            public bool isDyncStrickout; // 动态删除线
            public int dyncSpeed;
            public bool isOffset;
            public Rect offsetRect;

            public EffectType effectType;
            public Color effectColor;
            public Vector2 effectDistance;

            public LineAlignment lineAlignment;
            public int nextLineX; // 下一行的起始偏移量
            public float cartonScale;
            public float spriteScale;

            public void Clear()
            {
                anchor = Anchor.Null;
                font = null;
                fontStyle = FontStyle.Normal;
                fontSize = 0;
                fontColor = Color.white;
                isUnderline = false;
                isStrickout = false;
                isBlink = false;
                isDyncUnderline = false;
                isDyncStrickout = false;
                dyncSpeed = 0;
                isOffset = false;
                offsetRect.Set(0, 0, 0, 0);

                effectType = EffectType.Null;
                effectColor = Color.black;
                effectDistance = Vector2.zero;

                lineAlignment = LineAlignment.Default;
                nextLineX = 0;
                cartonScale = 1f;
                spriteScale = 1f;
            }

            public void Set(Config c)
            {
                anchor = c.anchor;
                font = c.font;
                fontStyle = c.fontStyle;
                fontSize = c.fontSize;
                fontColor = c.fontColor;
                isUnderline = c.isUnderline;
                isStrickout = c.isStrickout;
                isBlink = c.isBlink;
                dyncSpeed = c.dyncSpeed;

                isOffset = c.isOffset;
                offsetRect = c.offsetRect;

                effectType = c.effectType;
                effectColor = c.effectColor;
                effectDistance = c.effectDistance;
                isDyncUnderline = c.isDyncUnderline;
                isDyncStrickout = c.isDyncStrickout;
                lineAlignment = c.lineAlignment;
                nextLineX = c.nextLineX;
                cartonScale = c.cartonScale;
                spriteScale = c.spriteScale;
            }

            public bool isSame(Config c)
            {
                return anchor == c.anchor &&
                       font == c.font &&
                       fontStyle == c.fontStyle &&
                       isUnderline == c.isUnderline &&
                       fontColor == c.fontColor &&
                       isStrickout == c.isStrickout &&
                       isBlink == c.isBlink &&
                       fontSize == c.fontSize &&
                       cartonScale == c.cartonScale &&
                       spriteScale == c.spriteScale &&
                       lineAlignment == c.lineAlignment &&
                       isDyncUnderline == c.isDyncUnderline &&
                       isDyncStrickout == c.isDyncStrickout &&
                       nextLineX == c.nextLineX &&
                       dyncSpeed == c.dyncSpeed &&
                       (
                       (effectType == EffectType.Null && c.effectType == EffectType.Null) ||
                       (effectType == c.effectType && effectColor == c.effectColor && effectDistance == c.effectDistance)
                       ) &&
                       (
                       (isOffset == false && c.isOffset == false) ||
                       (isOffset == c.isOffset && offsetRect == c.offsetRect)
                       );
            }
        }

        System.Func<TagAttributes, IExternalNode> getExternalNode = null;

        public void parser(IOwner owner, string text, ref Config config, List<NodeBase> vList, System.Func<TagAttributes, IExternalNode> getExternalNode)
        {
            clear();

            mOwner = owner;
            this.getExternalNode = getExternalNode;
            d_nodeList = vList;
            startConfig.Set(config);
            currentConfig.Set(config);

            if (currentConfig.font == null)
            {
                Debug.LogError("TextParser pFont == null");
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int lenght = text.Length;
            while (lenght > d_curPos)
            {
                if (d_bBegin == false)
                {
                    switch (text[d_curPos])
                    {
                        // #表示一个新的功能
                        case '#':
                            {
                                d_bBegin = true;
                                ++d_curPos;
                            }
                            break;

                        // '<' 富文本标签
                        case '<':
                            {
                                int endpos = text.IndexOf('>', d_curPos);
                                if (endpos != -1)
                                {
                                    string param = null;

                                    //解析标签名称和参数
                                    int tagend = text.IndexOfAny(s_TagNameChars, d_curPos);
                                    string tag;
                                    if (tagend != -1 && tagend < endpos)
                                    {
                                        tag = text.Substring(d_curPos + 1, tagend - d_curPos);
                                        param = text.Substring(tagend + 1, endpos - tagend - 1);
                                    }
                                    else
                                    {
                                        tag = text.Substring(d_curPos + 1, endpos - d_curPos - 1);
                                    }

                                    if (d_text.Length != 0)
                                        save(false);

                                    TagParam(tag, param);

                                    d_curPos = endpos + 1;
                                    break;
                                }
                                else
                                {
                                    d_text.Append(text[d_curPos]);
                                }

                                ++d_curPos;
                            }
                            break;

                        // 这个是换行
                        case '\n':
                            {
                                save(true);
                                d_curPos++;
                            }
                            break;
                        default:
                            {
                                d_text.Append(text[d_curPos]);
                                ++d_curPos;
                            }
                            break;
                    }
                }
                else
                {
                    char c = text[d_curPos];
                    OnFun fun;
                    if (c < 128 && ((fun = OnFuns[c]) != null))
                    {
                        fun(text);
                    }
                    else
                    {
                        d_text.Append(text[d_curPos]);
                        ++d_curPos;
                    }

                    d_bBegin = false;
                }
            }

            if (d_text.Length != 0)
                save(false);

            clear();
        }

        protected void save(bool isNewLine)
        {
            if (d_text.Length == 0)
            {
                if (isNewLine == true)
                {
                    if (d_nodeList.Count != 0)
                    {
                        NodeBase node = d_nodeList.back();
                        if (node.isNewLine() == false)
                        {
                            node.setNewLine(true);
                            return;
                        }
                    }

                    // 添加一个换行的结点
                    LineNode nodeY = CreateNode<LineNode>();
                    nodeY.SetConfig(ref currentConfig);
                    nodeY.font = currentConfig.font;
                    nodeY.fontSize = currentConfig.fontSize;
                    nodeY.fs = currentConfig.fontStyle;
                    nodeY.setNewLine(true);
                    d_nodeList.Add(nodeY);
                    return;
                }
                else
                {
                    return;
                }
            }

            // 为文本 
            TextNode textNode = CreateNode<TextNode>();
            {
                textNode.d_text = d_text.ToString();
                textNode.SetConfig(ref currentConfig);
            }
            textNode.setNewLine(isNewLine);

            d_nodeList.Add(textNode);
            d_text.Remove(0, d_text.Length);
        }

        protected void saveX(float value)
        {
            XSpaceNode node = CreateNode<XSpaceNode>();
            node.d_offset = value;

            d_nodeList.Add(node);
        }

        protected void saveY(float value)
        {
            if (d_nodeList.Count != 0 && d_nodeList.back().isNewLine() == false)
            {
                d_nodeList.back().setNewLine(true);
            }

            YSpaceNode node = CreateNode<YSpaceNode>();
            node.d_offset = value;
            node.setNewLine(true);
            d_nodeList.Add(node);
        }

        protected void saveZ(float value)
        {
            YSpaceNode node = CreateNode<YSpaceNode>();
            node.d_offset = value;
            node.setNewLine(false);
            d_nodeList.Add(node);
        }

        protected void saveHy()
        {
            if (d_text.Length == 0)
                return;

            string text = d_text.ToString();
            d_text.Remove(0, d_text.Length);
            HyperlinkNode node = CreateNode<HyperlinkNode>();
            string hytext = string.Empty;
            if (text[text.Length - 1] == '}')
            {
                int beginPos = text.IndexOf('{', 0);
                if (beginPos != -1)
                {
                    hytext = text.Substring(beginPos, text.Length - beginPos);
                    node.d_link = hytext.Replace("{", "").Replace("}", "");
                    text = text.Remove(beginPos, text.Length - beginPos);
                }
            }

            node.d_text = "";
            node.SetConfig(ref currentConfig);
            ParseHyText(text, node);

            d_nodeList.Add(node);
        }

        protected void clear()
        {
            getExternalNode = null;
            startConfig.Clear();
            currentConfig.Clear();

            d_nodeList = null;
            d_curPos = 0;
            d_text.Clear();
            d_bBegin = false;
            mOwner = null;
        }

        protected int d_curPos = 0;

        //初始解析配置
        protected Config startConfig;

        //运行时解析配置
        protected Config currentConfig;

        //解析出来的节点列表
        protected List<NodeBase> d_nodeList;

        //解析过程中的临时字符串
        protected StringBuilder d_text = new StringBuilder();

        //是否为一个新的功能
        protected bool d_bBegin;

        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        static Color GetColour(uint code)
        {
            return ColorConst.Get(code);
        }
    }
}
