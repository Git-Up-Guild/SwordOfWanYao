﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WXB
{
    /// <summary>
    /// 标签解析
    /// </summary>
    public partial class TextParser
    {
        /// <summary>
        /// 标签名称定义
        /// </summary>
        public static class TagNames
        {
            //图片
            public static readonly string Sprite = "sprite ";
            public static readonly string Position = "pos ";
            public static readonly string RectSprite = "RectSprite ";
            public static readonly string Hy = "hy ";

            //偏移
            public static readonly string Offset = "offset ";

            //表情
            public static readonly string Face = "face ";

            //外部节点
            public static readonly string External = "external ";

            //颜色
            public static readonly string ColorBegin = "color=";
            public static readonly string ColorEnd = "/color";

            //文字加粗
            public static readonly string FontBoldBegin = "b";
            public static readonly string FontBoldEnd = "/b";

            //文字斜体
            public static readonly string FontItalicBegin = "i";
            public static readonly string FontItalicEnd = "/i";

            //文字大小
            public static readonly string FontSizeBegin = "size=";
            public static readonly string FontSizeEnd = "/size";

            //描边效果
            public static readonly string OutlineBegin = "ol ";
            public static readonly string OutlineEnd = "/ol";

            //阴影效果
            public static readonly string ShadowBegin = "so ";
            public static readonly string ShadowEnd = "/so";

            //垂直空白
            public static readonly string VSpace = "vspace=";
        }

        Dictionary<string, Action<string, string>> TagFuns;

        static TagAttributes s_TagAttributes = new TagAttributes();

        void Reg(string type, Action<string, TagAttributes> fun)
        {
            TagFuns.Add(type, (string key, string param) => 
            {
                s_TagAttributes.Reset();
                s_TagAttributes.Parse(param);

                try
                {
                    fun(type, s_TagAttributes);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                s_TagAttributes.Reset();
            });
        }

        static Color ParserColorName(string name, int startpos, Color c)
        {
            if (string.IsNullOrEmpty(name))
                return c;

            if (name[startpos] == '#')
            {
                return Tools.ParseColor(name, startpos + 1, c);
            }
            else
            {
                return ColorConst.Get(name, c);
            }
        }

        static void SetDefaultConfig(NodeBase nb, TagAttributes att)
        {
            nb.d_bBlink = att.getValueAsBool("b", nb.d_bBlink);
            nb.d_color = ParserColorName(att.getValueAsString("c"), 0, nb.d_color);

            int offsetv = att.getValueAsInteger("x", -1);
            if (offsetv > 0)
            {
                nb.d_bOffset = true;
                nb.d_rectOffset.xMin = -offsetv / 2;
                nb.d_rectOffset.xMax = offsetv / 2;
            }

            offsetv = att.getValueAsInteger("y", -1);
            if (offsetv > 0)
            {
                nb.d_bOffset = true;
                nb.d_rectOffset.yMin = -offsetv / 2;
                nb.d_rectOffset.yMax = offsetv / 2;
            }
        }

        static void SetSizeConfig(RectNode nb, TagAttributes att, Vector2 size, float scale=1f)
        {
            nb.width = att.getValueAsFloat("w", size.x);
            nb.height = att.getValueAsFloat("h", size.y);

            switch (att.getValueAsInteger("t", 0))
            {
                case 1: nb.height = nb.width * size.y / size.x; 
                    break;
                case 2: nb.width = nb.height * size.x / size.y; 
                    break;
            }

            nb.width *= scale;
            nb.height *= scale;
        }

        void RegTag()
        {
            TagFuns = new Dictionary<string, Action<string, string>>();

            Reg(TagNames.Sprite, (string tag, TagAttributes att) =>
            {
                string name = att.getValueAsString("n");
                string id = att.getValueAsString("id");

                SpriteNode sn = CreateNode<SpriteNode>();
                sn.sprite = null;
                sn.spriteName = name;
                sn.id = id;

                //开始加载图片
                sn.LoadSprite();

                sn.SetConfig(ref currentConfig);

                Vector2 size = new Vector2(32, 32);
                if (sn.sprite != null)
                {
                    size = sn.sprite.rect.size;
                }

                SetSizeConfig(sn, att, size, currentConfig.spriteScale);
                SetDefaultConfig(sn, att);

                d_nodeList.Add(sn);
            });

            Reg(TagNames.Position, (string tag, TagAttributes att) =>
            {
                SetPosNode node = CreateNode<SetPosNode>();
                node.d_value = att.getValueAsFloat("v", 0);
                node.type = (TypePosition)att.getValueAsInteger("t", (int)(TypePosition.Absolute));
                d_nodeList.Add(node);
            });

            Reg(TagNames.RectSprite, (string tag, TagAttributes att) =>
            {
                Sprite s = Tools.GetSprite(att.getValueAsString("n")); // 名字
                if (s == null)
                {
                    // 没有查找到
                    Debug.LogErrorFormat("not find sprite:{0}!", att.getValueAsString("n"));
                    return;
                }

                RectSpriteNode sn = CreateNode<RectSpriteNode>();
                sn.SetConfig(ref currentConfig);
                Rect rect = s.rect;
                sn.sprite = s;
                sn.rect.width = att.getValueAsFloat("w", rect.width);
                sn.rect.height = att.getValueAsFloat("h", rect.height);

                switch (att.getValueAsInteger("t", 0))
                {
                case 1: sn.rect.height = sn.rect.width * rect.height / rect.width; break;
                case 2: sn.rect.width = sn.rect.height * rect.width / rect.height; break;
                }

                sn.rect.x = att.getValueAsFloat("px", 0f);
                sn.rect.y = att.getValueAsFloat("py", 0f);

                SetDefaultConfig(sn, att);

                d_nodeList.Add(sn);
            });

            Reg(TagNames.Hy, (string tag, TagAttributes att) => 
            {
                HyperlinkNode node = CreateNode<HyperlinkNode>();
                node.SetConfig(ref currentConfig);
                node.d_text = att.getValueAsString("t");
                node.d_link = att.getValueAsString("l");
                node.d_fontSize = att.getValueAsInteger("fs", node.d_fontSize);
                node.d_fontStyle = (FontStyle)att.getValueAsInteger("ft", (int)node.d_fontStyle);

                if (att.exists("fn"))
                    node.d_font = Tools.GetFont(att.getValueAsString("fn"));

                node.d_color = ParserColorName(att.getValueAsString("fc"), 0, node.d_color);
                node.hoveColor = ParserColorName(att.getValueAsString("fhc"), 0, node.d_color);

                node.d_bUnderline = att.getValueAsBool("ul", node.d_bUnderline);
                node.d_bStrickout = att.getValueAsBool("so", node.d_bStrickout);
                d_nodeList.Add(node);
            });

            Reg(TagNames.Face, (string tag, TagAttributes att) => 
            {
                string name = att.getValueAsString("n");
                Cartoon c = Tools.GetCartoon(name);
                if (c == null)
                    return;

                CartoonNode cn = CreateNode<CartoonNode>();
                cn.cartoon = c;
                cn.width = c.width;
                cn.height = c.height;

                cn.SetConfig(ref currentConfig);

                SetSizeConfig(cn, att, new Vector2(c.width, c.height));
                SetDefaultConfig(cn, att);

                d_nodeList.Add(cn);
            });

            TagFuns.Add(TagNames.ColorBegin, (string tag, string param) => 
            {
                if (string.IsNullOrEmpty(param))
                    return;
                currentConfig.fontColor = ParserColorName(param, 0, currentConfig.fontColor);
            });

            TagFuns.Add(TagNames.ColorEnd, (string tag, string param)=> 
            {
                currentConfig.fontColor = startConfig.fontColor;
            });

            TagFuns.Add(TagNames.FontBoldBegin, (string tag, string param) =>
            {
                currentConfig.fontStyle |= FontStyle.Bold;
            });

            TagFuns.Add(TagNames.FontBoldEnd, (string tag, string param) =>
            {
                currentConfig.fontStyle &= ~FontStyle.Bold;
            });

            TagFuns.Add(TagNames.FontItalicBegin, (string tag, string param) =>
            {
                currentConfig.fontStyle |= FontStyle.Italic;
            });

            TagFuns.Add(TagNames.FontItalicEnd, (string tag, string param) =>
            {
                currentConfig.fontStyle &= ~FontStyle.Italic;
            });

            TagFuns.Add(TagNames.FontSizeBegin, (string tag, string param) =>
            {
                currentConfig.fontSize = (int)Tools.stringToFloat(param, currentConfig.fontSize);
            });

            TagFuns.Add(TagNames.FontSizeEnd, (string tag, string param) =>
            {
                currentConfig.fontSize = startConfig.fontSize;
            });

            //垂直空间
            TagFuns.Add(TagNames.VSpace, (string tag, string param) => 
            {
                save(true);
                var n = d_nodeList.back();
                var ln = n as LineNode;
                if(ln == null)
                    save(true);

                n = d_nodeList.back();
                ln = n as LineNode;
                if (ln != null)
                {
                    var space = Tools.stringToFloat(param, 0f);
                    ln.offset = space;
                }

            });

            // 描边效果
            Reg(TagNames.OutlineBegin, (string tag, TagAttributes att) => 
            {
                currentConfig.effectType = EffectType.Outline;
                ParamEffectType(ref currentConfig, att);
            });

            TagFuns.Add(TagNames.OutlineEnd, (string tag, string param) =>
            {
                currentConfig.effectType = EffectType.Null;
            });

            // 阴影
            Reg(TagNames.ShadowBegin, (string tag, TagAttributes att) =>
            {
                currentConfig.effectType = EffectType.Shadow;
                ParamEffectType(ref currentConfig, att);
            });

            TagFuns.Add(TagNames.ShadowEnd, (string tag, string param) =>
            {
                currentConfig.effectType = EffectType.Null;
            });

            //偏移
            Reg(TagNames.Offset, (string tag, TagAttributes att)=> 
            {
                float x = att.getValueAsFloat("x", 0f);
                float y = att.getValueAsFloat("y", 0f);

                if (x <= 0f && y <= 0f)
                    return;

                currentConfig.isOffset = true;
                currentConfig.offsetRect.xMin = -x / 2f;
                currentConfig.offsetRect.xMax = x / 2f;

                currentConfig.offsetRect.yMin = -x / 2f;
                currentConfig.offsetRect.yMax = x / 2f;
            });

            // 外部结点
            Reg(TagNames.External, (tag, att)=> 
            {
                if (getExternalNode == null)
                {
                    Debug.LogErrorFormat("external node but getExternalNode is null!");
                    return;
                }

                ExternalNode sn = CreateNode<ExternalNode>();
                sn.SetConfig(ref currentConfig);
                sn.Set(getExternalNode(att));

                d_nodeList.Add(sn);
            });
        }

        static void ParamEffectType(ref Config config, TagAttributes att)
        {
            config.effectColor = ParserColorName(att.getValueAsString("c"), 0, Color.black);
            config.effectDistance.x = att.getValueAsFloat("x", 1f);
            config.effectDistance.y = att.getValueAsFloat("y", 1f);
        }

        void TagParam(string tag, string param)
        {
            System.Action<string, string>  fun;
            if (TagFuns.TryGetValue(tag, out fun))
            {
                fun(tag, param);
            }
            else
            {
                Debug.LogErrorFormat("tag:{0} param:{1} not find!", tag, param);
            }
        }
    }
}

