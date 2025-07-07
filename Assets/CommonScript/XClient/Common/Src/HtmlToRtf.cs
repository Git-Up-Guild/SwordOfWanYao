/*******************************************************************
** 文件名:	HtmlToRtf.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	刘芳洲
** 日  期:	2018/11/16
** 版  本:	1.0
** 描  述:	 HTML文本转换Rich Text Format	
** 应  用:	
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XClient.Common
{

    public class HtmlToRtf
    {
        private string mFilePath = "input";
        private string mContent;
        private int mLineNo;
        private int mLinePos;
        private int mPos;
        private string mLexText;
        private char mCurrent;

        static HtmlToRtf _Instance;

        public static HtmlToRtf Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new HtmlToRtf();
                return _Instance;
            }
        }

        private char pop_char()
        {
            if (mPos >= mContent.Length)
                return '\0';
            return mCurrent = mContent[mPos++];
        }

        private void unpop()
        {
            if (mPos > 0)
                --mPos;
        }

        private int error(string msg)
        {
            Console.WriteLine("{0} ({1},{2}) error: {3}", mFilePath, mLineNo, mLinePos, msg);
            mCurrent = (char)0xff;
            return (int)eHtmlTag.HTML_ERROR;
        }

        private int pop_comment()
        {
            int pos = mContent.IndexOf("-->", mPos);
            if (pos < 0)
                return error("comment syntex error.");
            mLexText = mContent.Substring(mPos, pos - mPos);
            mLineNo += pos + 3 - mPos;
            mPos = pos + 3;
            return (int)eHtmlTag.HTML_COMMENT;
        }

        private int pop_string()
        {
            int pos = mContent.IndexOf('"', mPos);
            if (pos < 0)
                return error("syntex error, string not close.");
            mLexText = mContent.Substring(mPos, pos - mPos);
            mLinePos += pos + 1 - mPos;
            mPos = pos + 1;
            return (int)eHtmlTag.HTML_STRING;
        }

        private int pop_symbol()
        {
            int pos = mContent.IndexOf(';', mPos);
            if (pos < 0)
                return (int)error("syntex error.");
            mLexText = mContent.Substring(mPos, pos - mPos);
            mLinePos += pos - mPos + 1;
            mPos = pos + 1;
            return (int)eHtmlTag.HTML_SYMBOL;
        }

        private int pop_content()
        {
            mLexText = new string(mCurrent, 1);
            char ch = pop_char();
            while (ch != 0 && ch != '<' && ch != '&')
            {
                if (ch == '\n')
                {
                    ++mLineNo;
                    mLinePos = 1;
                }
                else if (ch != '\r')
                {
                    ++mLinePos;
                    if (ch != ' ' && ch != '\t')
                        mLexText += ch;
                }
                ch = pop_char();
            }
            if (ch == '<' || ch == '&')
                unpop();
            return (int)eHtmlTag.HTML_CONTENT;
        }

        private int next_token()
        {
            char ch = pop_char();
            if (ch == 0)
                return 0;
            //while (char.IsSeparator(ch) || char.IsControl(ch))
            //{
            //    if (ch == ' ' || ch == '\t' || ch == '　')
            //        //++mLinePos;
            //        break;
            //    else if (ch == '\n')
            //    {
            //        ++mLineNo;
            //        mLinePos = 1;
            //    }
            //    ch = pop_char();
            //}
            if (ch == '<')
            {
                if (mPos < mContent.Length - 2 && mContent[mPos + 1] == '-' && mContent[mPos + 2] == '-')
                {
                    pop_char();
                    pop_char();
                    return pop_comment();
                }
                return ch;
            }
            else if (ch == '&')
                return pop_symbol();
            else
                return pop_content();
        }

        private int tag_ident()
        {
            int pos = mPos;
            while (pos < mContent.Length && char.IsLetterOrDigit(mContent, pos) || mContent[pos] == '_')
                ++pos;
            mLexText = mContent.Substring(mPos - 1, pos - mPos + 1);
            mLinePos += pos - mPos + 1;
            mPos = pos;

            return (int)eHtmlTag.HTML_IDENT;
        }

        private int next_tag_token()
        {
            char ch = pop_char();
            if (ch == 0)
                return 0;
            while (ch != 0 && (char.IsControl(ch) || char.IsSeparator(ch)))
            {
                if (ch == ' ' || ch == '\t')
                    ++mLinePos;
                else if (ch == '\n')
                {
                    ++mLineNo;
                    mLinePos = 1;
                }
                ch = pop_char();
            }
            if (ch == '"')
                return pop_string();
            else if (ch == '=' || ch == '/' || ch == '>')
                return ch;
            else if (char.IsLetter(ch) || ch == '_')
                return tag_ident();
            else
                return error("syntex error.");
        }

        private HtmlTag pop_tag()
        {
            int tok = next_tag_token();
            if (tok == 0)
                return null;
            HtmlTag tag = new HtmlTag();
            while (tok != 0 && tok != '>')
            {
                if (tok == '/')
                {
                    tag.closed = true;
                }
                else if (tok == (int)eHtmlTag.HTML_IDENT)
                {
                    if (string.IsNullOrEmpty(tag.ident))
                        tag.ident = mLexText;
                    else
                    {
                        string key = mLexText;
                        if (next_tag_token() != '=')
                        {
                            error("syntex error.");
                            return null;
                        }
                        if (next_tag_token() != (int)eHtmlTag.HTML_STRING)
                        {
                            error("syntex error.");
                            return null;
                        }
                        if (tag.attributes == null)
                            tag.attributes = new Dictionary<string, string>();
                        tag.attributes.Add(key, mLexText);
                    }
                }
                else
                {
                    error("syntex error.");
                    return null;
                }
                tok = next_tag_token();
            }
            return tag;
        }

        private HtmlItem pop_item()
        {

            int tok = next_token();
            if (tok == '<')
            {
                HtmlTag tag = pop_tag();
                if (tag == null)
                    return null;
                if (tag.closed)
                {
                    return tag;
                }

                HtmlItem item = pop_item();
                while (true)
                {
                    if (item == null)
                    {
                        error("error syntex tag " + tag.ident + " not closed.");
                        return null;
                    }
                    if (item.is_tag)
                    {
                        HtmlTag endtag = item as HtmlTag;
                        if (endtag.closed && endtag.ident == tag.ident)
                            return tag;
                    }
                    if (tag.content == null)
                        tag.content = new List<HtmlItem>() { item };
                    else
                        tag.content.Add(item);
                    item = pop_item();
                }
            }
            else if (tok == (int)eHtmlTag.HTML_SYMBOL)
                return new HtmlSymbol(mLexText);
            else if (tok == (int)eHtmlTag.HTML_CONTENT)
                return new HtmlContent(mLexText);
            else
                return null;
        }

        public HtmlBody parse(string body)
        {
            mContent = body;
            mPos = 0;
            mLineNo = 1;
            mLinePos = 1;
            mLexText = null;
            HtmlBody result = new HtmlBody();
            result.content = new List<HtmlItem>();
            while (true)
            {
                HtmlItem item = pop_item();
                if (item == null && mCurrent == 0XFF)
                    return null;
                else if (item != null)
                {
                    result.content.Add(item);
                }
                else
                    return result;
            }
        }

        private string content_to_rt(List<HtmlItem> items)
        {
            if (items == null)
                return "";
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < items.Count; ++index)
            {
                HtmlItem item = items[index];
                //if (item.is_tag)
                //{
                //    if ((item as HtmlTag).ident == "br")
                //    {
                //        if (index + 1 < items.Count)
                //        {
                //            HtmlItem Nextitem = items[index + 1];
                //            if (Nextitem.is_tag)
                //                if (!checkHaveContentAndTag(Nextitem, "br")) continue;
                //        }
                //        else
                //            continue; ;
                //    }
                //}
                sb.Append(to_richtext(item));
            }
            return sb.ToString();
        }

        private string rt_color(string val)
        {
            return "<color=" + val + ">";
        }

        private string rt_size(string val)
        {
            int size = 0;
            if (int.TryParse(val, out size))
                return "<size=34>";
            return "<size=" + (34 + (size - 3) * 2.5f) + ">";
        }

        private string rt_font(HtmlTag tag)
        {
            StringBuilder sb = new StringBuilder();
            if (tag.attributes != null)
            {
                foreach (KeyValuePair<string, string> val in tag.attributes)
                {
                    if (val.Key == "color")
                        sb.Append(rt_color(val.Value));
                    else if (val.Key == "size")
                        sb.Append(rt_size(val.Value));
                    else
                        Console.WriteLine("Invalid font attribute : {0}", val.Key);
                }
            }

            return sb.ToString();
        }

        private string rt_font_close(HtmlTag tag)
        {
            StringBuilder sb = new StringBuilder();
            if (tag.attributes != null)
            {
                foreach (KeyValuePair<string, string> val in tag.attributes)
                {
                    if (val.Key == "color")
                        sb.Insert(0, "</color>");
                    else if (val.Key == "size")
                        sb.Insert(0, "</size>");
                    else
                        Console.WriteLine("Invalid font attribute : {0}", val.Key);
                }
            }
            return sb.ToString();
        }

        private int style_pop(string style, int pos, out string key, out string value)
        {
            int next = style.IndexOf(';', pos);
            int comma_pos = style.IndexOf(':', pos);
            if (comma_pos < 0 || (next > 0 && comma_pos > next))
            {
                key = value = "";
                return style.Length;
            }
            key = style.Substring(pos, comma_pos - pos);
            if (next >= 0)
            {
                value = style.Substring(comma_pos + 1, next - comma_pos - 1);
                return next + 1;
            }
            else
            {
                value = style.Substring(comma_pos + 1);
                return style.Length;
            }
        }

        private string font_size(string desc)
        {
            desc = desc.Trim().TrimEnd();
            // xx-small, x-small, smaller, small, medium, large, larger, x-large, xx-large
            switch (desc)
            {
                case "xx-small": return "15";
                case "x-small": return "20";
                case "smaller": return "25";
                case "small": return "30";
                case "medium": return "35";
                case "large": return "40";
                case "larger": return "45";
                case "x-large": return "50";
                case "xx-large": return "55";
            }
            int index = 0;
            for (; index < desc.Length && char.IsDigit(desc, index);) ++index;
            // 00
            if (index >= desc.Length)
                return (35 + (int.Parse(desc) - 3) * 5).ToString();
            string unit = desc.Substring(index);
            // 00px
            if (unit == "px")
            {
                string st = desc.Substring(0, index);
                int pxInt = 0;
                if (int.TryParse(st, out pxInt))
                {
                    pxInt *= 2;
                    return pxInt.ToString();
                }
                return st;
            }
            else if (unit == "%") // 00%
                return ((int)35.0f * int.Parse(desc.Substring(0, index)) / 100).ToString();
            Console.WriteLine("size unit \"" + unit + "\" is not support.");
            return "35";
        }

        private char pop_char(string val, ref int pos)
        {
            while (pos < val.Length && (char.IsControl(val, pos) || char.IsSeparator(val, pos))) ++pos;
            if (pos >= val.Length)
                return '\0';
            return val[pos++];
        }

        private bool pop_byte(string desc, ref int pos, ref byte val)
        {
            while (pos < desc.Length && (char.IsControl(desc, pos) || char.IsSeparator(desc, pos))) ++pos;
            if (pos >= desc.Length || !char.IsDigit(desc, pos))
                return false;
            int beg = pos;
            while (pos < desc.Length && char.IsDigit(desc, pos)) ++pos;
            if (!byte.TryParse(desc.Substring(beg, pos - beg), out val))
                return false;
            return true;
        }

        private string font_color(string desc)
        {
            desc = desc.Trim().TrimEnd();
            if (string.IsNullOrEmpty(desc))
                return "";
            // #000000
            if (desc[0] == '#')
                return desc;
            // rgb(00,00,00)
            if (desc.Length > 3 && desc.Substring(0, 3) == "rgb")
            {
                byte red = 0, green = 0, blue = 0;
                int pos = 3;
                if (pop_char(desc, ref pos) != '(' ||
                    !pop_byte(desc, ref pos, ref red) ||
                    pop_char(desc, ref pos) != ',' ||
                    !pop_byte(desc, ref pos, ref green) ||
                    pop_char(desc, ref pos) != ',' ||
                    !pop_byte(desc, ref pos, ref blue) ||
                    pop_char(desc, ref pos) != ')')
                {
                    Console.WriteLine("color value \"" + desc + "\" syntex error");
                    return "";
                }
                return string.Format("#{0:x02}{1:x02}{2:x02}", red, green, blue);
            }
            // red, blue, ...
            int sep = desc.IndexOf(':');
            if (sep > 0 && desc.Substring(0, sep) == "color")
                return desc.Substring(sep + 1);
            else if (sep < 0)
                return desc;
            Console.WriteLine("coloe value \"" + desc + "\" syntex error.");
            return "";
        }

        private string rt_span_style(string val)
        {
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            while (true)
            {
                string key, value;
                pos = style_pop(val, pos, out key, out value);
                if (string.IsNullOrEmpty(key))
                    return sb.ToString();
                if (key == "font-size")
                {
                    sb.Append("<size=");
                    sb.Append(font_size(value));
                    sb.Append(">");
                }
                else if (key == "color")
                {
                    sb.Append("<color=");
                    sb.Append(font_color(value));
                    sb.Append(">");
                }
                else
                    Console.WriteLine("Span style \"" + key + "\" is not support.");
            }
        }

        private string rt_span_style_close(string val)
        {
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            while (true)
            {
                string key, value;
                pos = style_pop(val, pos, out key, out value);
                if (string.IsNullOrEmpty(key))
                    return sb.ToString();
                if (key == "font-size")
                    sb.Insert(0, "</size>");
                else if (key == "color")
                    sb.Insert(0, "</color>");
            }
        }

        private string rt_span(HtmlTag tag)
        {
            StringBuilder sb = new StringBuilder();
            if (tag.attributes != null)
            {
                foreach (KeyValuePair<string, string> val in tag.attributes)
                {
                    if (val.Key == "style")
                        sb.Append(rt_span_style(val.Value));
                    else
                        Console.WriteLine("Invalid span attribute : " + val.Key);
                }
            }
            return sb.ToString();
        }

        private string rt_span_close(HtmlTag tag)
        {
            StringBuilder sb = new StringBuilder();
            if (tag.attributes != null)
            {
                foreach (KeyValuePair<string, string> val in tag.attributes)
                {
                    if (val.Key == "style")
                        sb.Append(rt_span_style_close(val.Value));
                }
            }
            return sb.ToString();
        }

        private string tag_to_richtext(HtmlTag tag)
        {
            StringBuilder sb = new StringBuilder();
            if (tag.ident == "font")
            {
                sb.Append(rt_font(tag));
                sb.Append(content_to_rt(tag.content));
                sb.Append(rt_font_close(tag));
                return sb.ToString();
            }
            else if (tag.ident == "span")
            {
                sb.Append(rt_span(tag));
                sb.Append(content_to_rt(tag.content));
                sb.Append(rt_span_close(tag));
                return sb.ToString();
            }
            else if (tag.ident == "p")
            {
                if (checkHaveContent(tag))
                {
                    sb.Append("\n");//<size=16>\n</size>"
                }
                sb.Append(content_to_rt(tag.content));
                return sb.ToString();
            }
            else if (tag.ident == "b" || tag.ident == "strong")
            {
                sb.Append("<b>");
                sb.Append(content_to_rt(tag.content));
                sb.Append("</b>");
                return sb.ToString();
            }
            else if (tag.ident == "br")
            {
                //if (!checkHaveContent(tag)) return "";
                sb.Append("\n");
                //sb.Append(content_to_rt(tag.content));
                return sb.ToString();
            }
            else if (tag.ident == "i" || tag.ident == "em")
            {
                sb.Append("<i>");
                sb.Append(content_to_rt(tag.content));
                sb.Append("</i>");
                return sb.ToString();
            }
            else if (tag.ident == "del")
                return "";
            Console.WriteLine("Invalid html tag : {0}", tag.ident);
            return "";
        }

        private string symbol_to_string(string symb)
        {
            switch (symb)
            {
                case "lt": return "<";
                case "gt": return ">";
                case "amp": return "&";
                case "quot": return "\"";
                case "ensp": return " ";
                case "emsp": return " ";
                case "nbsp": return " ";
            }
            Console.WriteLine("invalid symbol :" + symb);
            return "";
        }

        private string to_richtext(HtmlItem item)
        {
            if (item.is_tag)
                return tag_to_richtext(item as HtmlTag);
            else if (item.is_content)
                return (item as HtmlContent).content;
            else if (item.is_symbol)
                return symbol_to_string((item as HtmlSymbol).symbol);
            else
                Console.WriteLine("Invalid html style.");
            return "";
        }

        public string to_richtext(string html)
        {
            HtmlBody body = parse(html);

            StringBuilder sb = new StringBuilder();
            if (body.content != null)
            {
                for (int index = 0; index < body.content.Count; index++)
                {
                    HtmlItem item = body.content[index];
                    //if (item.is_tag)
                    //{
                    //    if ((item as HtmlTag).ident == "br")
                    //    {
                    //        if (index + 1 < body.content.Count)
                    //        {
                    //            HtmlItem Nextitem = body.content[index + 1];
                    //            if (Nextitem.is_tag)
                    //                if (!checkHaveContentAndTag(Nextitem, "br")) continue;
                    //        }
                    //        else
                    //            continue;
                    //    }
                    //}
                    sb.Append(to_richtext(item));
                }
            }
            return sb.ToString();
        }

        private bool checkHaveContent(HtmlItem html, bool checkChild = true)
        {
            if (!html.is_tag) return false;
            HtmlTag tag = (HtmlTag)html;
            if (null == tag.content) return false;
            for (int i = 0; i < tag.content.Count; i++)
            {
                HtmlItem tagI = tag.content[i];
                if (tagI.is_tag)
                {
                    string ident = ((HtmlTag)tagI).ident;
                    if (ident == "p") return false;
                }
                else if (tagI.is_content || tagI.is_symbol) return true;

                if (checkChild)
                    if (checkHaveContent(tagI)) return true;
            }

            return false;
        }

        private bool checkHaveTag(HtmlItem html, string tagstr, bool checkChild = true)
        {
            if (!html.is_tag) return false;
            HtmlTag tag = (HtmlTag)html;
            if (null == tag.content) return false;
            for (int i = 0; i < tag.content.Count; i++)
            {
                HtmlItem tagI = tag.content[i];
                if (tagI.is_tag)
                {
                    string ident = ((HtmlTag)tagI).ident;
                    if (ident == "p") return false;
                    if (tagstr == ident)
                        return true;
                }
                if (checkChild)
                    if (checkHaveTag(tagI, tagstr)) return true;
            }

            return false;
        }

        private bool checkHaveContentAndTag(HtmlItem html, string tagstr, bool checkChild = true)
        {
            if (!html.is_tag)
            {
                return false;
            }
            HtmlTag tag = (HtmlTag)html;
            if (null == tag.content) return false;
            for (int i = 0; i < tag.content.Count; i++)
            {
                HtmlItem tagI = tag.content[i];
                if (tagI.is_tag)
                {
                    string ident = ((HtmlTag)tagI).ident;
                    if (ident == "p") return false;
                    if (tagstr == ident)
                        return true;
                }
                else if (tagI.is_content || tagI.is_symbol) return true;

                if (checkChild)
                    if (checkHaveContentAndTag(tagI, tagstr)) return true;
            }

            return false;
        }
    }

    enum eHtmlTag
    {
        HTML_ERROR = -1,
        HTML_COMMENT = 0,
        HTML_IDENT = 1,
        HTML_STRING = 2,
        HTML_TAG = 3,
        HTML_SYMBOL = 4,
        HTML_CONTENT = 5
    }

    public class HtmlItem
    {
        public virtual bool is_tag
        {
            get { return false; }
        }

        public virtual bool is_content { get { return false; } }
        public virtual bool is_symbol { get { return false; } }
    }

    class HtmlTag : HtmlItem
    {
        public string ident;
        public Dictionary<string, string> attributes = null;
        public List<HtmlItem> content = null;
        public bool closed;

        public override bool is_tag { get { return true; } }
    }

    class HtmlSymbol : HtmlItem
    {
        public string symbol;

        public HtmlSymbol(string val)
        {
            symbol = val;
        }

        public override bool is_symbol { get { return true; } }
    }

    class HtmlContent : HtmlItem
    {
        public string content;

        public HtmlContent(string val)
        {
            content = val;
        }

        public override bool is_content { get { return true; } }
    }

    public class HtmlBody : HtmlItem
    {
        public List<HtmlItem> content;
    }


}
