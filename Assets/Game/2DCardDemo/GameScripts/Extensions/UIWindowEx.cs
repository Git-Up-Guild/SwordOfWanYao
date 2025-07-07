/**************************************************************************    
文　　件：.cs
作　　者：
创建时间：2025.05.09
描　　述：游戏中所有窗口的基类
***************************************************************************/

using UnityEngine.UI;
using XGame;
using XGame.Atlas;
using XGame.UI.Framework;

namespace GameScripts.CardDemo
{

 public abstract partial class UIWindowEx : UIWindow
    {
        /// <summary>
        /// 加载图标
        /// </summary>
        /// <param name="img"></param>
        /// <param name="iconID"></param>
        public void LoadIcon(Image img, int iconID, OnSpriteLoadedCallback callback = null, int userData = 0)
        {
            if (img == null)
                return;

            if (iconID == 0)
            {
                UnloadIcon(img);
                return;
            }

            cfg_Icon config = GameGlobalEx.GameScheme.Icon_0((uint)iconID);
            if (config == null)
            {
                UnloadIcon(img);
                return;
            }

            var altasPath = GameConfig.Instance.gameResDir + $"/Artist/Icon/{config.strAtlasPath}/{config.strAtlasPath}.spriteatlas";
            SpriteAltasToImg.Load(img, altasPath, config.strIconName, callback, userData);
        }

        /// <summary>
        /// 卸载图标
        /// </summary>
        /// <param name="img"></param>
        public void UnloadIcon(Image img)
        {
            SpriteAltasToImg.Unload(img);
        }
    }

}

