/*****************************************************
** 文 件 名：SymbolTextSettings.cs
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 17:04:20
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace WXB
{
    public class SymbolTextSettings : ScriptableObject
    {
        // 当前所有的字库
        static Dictionary<string, Font> Fonts;

        // 当前所有的精灵
        static Dictionary<string, Sprite> Sprites;

        // 当前所有的动画
        static Dictionary<string, Cartoon> Cartoons;

        //图集对象
        public SpriteAtlas animSpriteAtlas;

        //动画文件目录
        public string animSpriteDir;

        public Font[] fonts = null;
        public Sprite[] sprites = null;
        public Cartoon[] cartoons = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private static void Init()
        {
            if(Fonts == null)
            {
                Fonts = new Dictionary<string, Font>();
                Sprites = new Dictionary<string, Sprite>();
                Cartoons = new Dictionary<string, Cartoon>();
                SymbolTextInitialize sti = Resources.Load<SymbolTextInitialize>("SymbolTextInitialize");
                sti.settings.Initialize();
            }
        }

        private void Initialize()
        {
            if (fonts != null)
            {
                for (int i = 0; i < fonts.Length; ++i)
                    Fonts.Add(fonts[i].name, fonts[i]);
            }

            if (sprites != null)
            {
                for (int i = 0; i < sprites.Length; ++i)
                    Sprites.Add(sprites[i].name, sprites[i]);
            }

            if (cartoons != null)
            {
                for (int i = 0; i < cartoons.Length; ++i)
                    Cartoons.Add(cartoons[i].name, cartoons[i]);
            }
        }

        public static Font GetFont(string name)
        {
            Init();
              
            Font font;
            if (Fonts.TryGetValue(name, out font))
                return font;

            return null;
        }

        public static Sprite GetSprite(string name)
        {
            Init();

            Sprite sprite;
            if (Sprites.TryGetValue(name, out sprite))
                return sprite;

            return null;
        }

        public static Cartoon GetCartoon(string name)
        {
            Init();

            Cartoon cartoon;
            if (Cartoons.TryGetValue(name, out cartoon))
                return cartoon;

            return null;
        }

        public static void GetCartoons(List<Cartoon> cartoons)
        {
            Init();

            foreach (var itor in Cartoons)
            {
                cartoons.Add(itor.Value);
            }
        }
    }
}
