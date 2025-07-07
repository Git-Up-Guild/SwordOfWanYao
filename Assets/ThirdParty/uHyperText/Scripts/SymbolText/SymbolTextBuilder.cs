using System.Text;
using WXB;

namespace WXB
{
    public class SymbolTextBuilder
    {
        private static StringBuilder m_StrBuilder = new StringBuilder();

        public static void Clear()
        {
            m_StrBuilder.Clear();
        }

        /// <summary>
        /// 插入一个字符串
        /// </summary>
        /// <param name="str"></param>
        public static void AppendString(string str)
        {
            m_StrBuilder.Append(str);
        }

        /// <summary>
        /// 插入一段字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void AppendText(string text, string color = null)
        {
            //处理颜色
            if (!string.IsNullOrEmpty(color))
            {
                if (color.Length == 1)
                {
                    m_StrBuilder.Append("#").Append(color);
                }
                else
                {
                    m_StrBuilder.Append("#c").Append(color);
                }
            }

            //加入文本
            m_StrBuilder.Append(text);
        }

        /// <summary>
        /// 换行
        /// </summary>
        public static void AppendNewLine()
        {
            m_StrBuilder.Append("\n");
        }

        /// <summary>
        /// 插入一个链接
        /// </summary>
        /// <param name="text"></param>
        /// <param name="link"></param>
        /// <param name="color"></param>
        public static void AppendLink(string text, string link, string color = null)
        {
            m_StrBuilder.Append("#h");

            //处理颜色
            if (!string.IsNullOrEmpty(color))
            {
                if (color.Length == 1)
                {
                    m_StrBuilder.Append("#").Append(color);
                }
                else
                {
                    m_StrBuilder.Append("#c").Append(color);
                }
            }

            m_StrBuilder.Append(text);

            //连接内容
            if (!string.IsNullOrEmpty(link))
            {
                m_StrBuilder.Append("{").Append(link).Append("}");
            }
            m_StrBuilder.Append("#h");
        }

        /// <summary>
        /// 插入表情
        /// </summary>
        /// <param name=""></param>
        public static void AppendFace(string face)
        {
            m_StrBuilder.Append(face);
        }

        public static void Apply(SymbolText symbomText)
        {
            symbomText.text = m_StrBuilder.ToString();
        }
    }
}