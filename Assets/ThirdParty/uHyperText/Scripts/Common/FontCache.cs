using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace WXB
{
    public static class FontCache
    {
        static CharacterInfo s_Info;

        //缓存的行高
        static Dictionary<long, int> FontLineHeight = new Dictionary<long, int>();

        //缓存的字符宽度
        static Dictionary<long, int> FontAdvances = new Dictionary<long, int>();

        static TextGenerator sCachedTextGenerator = new TextGenerator();

        /// <summary>
        /// 获得行高
        /// </summary>
        /// <param name="font"></param>
        /// <param name="size"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        static public int GetLineHeight(Font font, int size, FontStyle fs)
        {
            long key = (long)(((ulong)fs) | (((ulong)size) << 24) | (((ulong)font.GetInstanceID()) << 32));
            int lineHeight = 0;
            if (FontLineHeight.TryGetValue(key, out lineHeight))
                return lineHeight;

            var settings = new TextGenerationSettings();
            settings.generationExtents = new Vector2(1000, 1000);
            if (font != null && font.dynamic)
            {
                settings.fontSize = size;
            }

            // Other settings
            settings.textAnchor = TextAnchor.LowerLeft;
            settings.lineSpacing = 1f;
            settings.alignByGeometry = false;
            settings.scaleFactor = 1f;
            settings.font = font;
            settings.fontSize = size;
            settings.fontStyle = fs;
            settings.resizeTextForBestFit = false;
            settings.updateBounds = false;
            settings.horizontalOverflow = HorizontalWrapMode.Overflow;
            settings.verticalOverflow = VerticalWrapMode.Truncate;

            string text = "a\na";
            sCachedTextGenerator.Populate(text, settings);

            IList<UIVertex> verts = sCachedTextGenerator.verts;
#if UNITY_2021
            lineHeight = (int)(verts[0].position.y - verts[4].position.y);
#else
            lineHeight = (int)(verts[0].position.y - verts[7].position.y);
#endif
            FontLineHeight.Add(key, lineHeight);
            return lineHeight;
        }

        static public int GetAdvance(Font font, int size, FontStyle fs, char ch)
        {
            long key = (long)(((ulong)((uint)ch)) | (((ulong)fs) << 16) | (((ulong)size) << 24) | (((ulong)font.GetInstanceID()) << 32));

            int advance;
            if (FontAdvances.TryGetValue(key, out advance))
                return advance;

            if (font.GetCharacterInfo(ch, out s_Info, size, fs))
            {
                advance = (short)(s_Info.advance);
                FontAdvances.Add(key, advance);
                return advance;
            }

            if (font.HasCharacter(ch))
            {
                //请求字符信息
                if(IsValidChar(ch))
                {
                    font.RequestCharactersInTexture(new string(ch, 1), size, fs);

                    //再次获取
                    if (font.GetCharacterInfo(ch, out s_Info, size, fs))
                    {
                        advance = (short)(s_Info.advance);
                        FontAdvances.Add(key, advance);
                        return advance;
                    }
                }
                else
                {
                    Debug.LogError($"遇到不可识别字符！ch={(int)ch}, unicode={CharUnicode(ch)}");
                }
            }

            FontAdvances.Add(key, 0);

            return 0;

        }

        public static string CharUnicode(char ch)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte[] bytes = CharToByte(ch);
            stringBuilder.Append(string.Format("\\u{0:X2}{1:X2}", bytes[1], bytes[0]));
            return stringBuilder.ToString();
        }

        public static bool IsValidChar(char ch)
        {
            if (char.IsSurrogate(ch))
                return false;
            return true;
        }

        public static byte[] CharToByte(char c)
        {
            byte[] b = new byte[2];
            b[0] = (byte)((c & 0xFF00) >> 8);
            b[1] = (byte)(c & 0xFF);
            return b;
        }

    }
}