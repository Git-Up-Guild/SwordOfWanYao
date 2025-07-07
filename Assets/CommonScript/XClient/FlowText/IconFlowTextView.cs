using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XClient.Common;
using XGame;
using XGame.Atlas;
using XGame.FlowText;

namespace XClient.FlowText
{ 
    public class IconFlowTextView : BaseFlowTextView
    {
        public TextMeshProUGUI textMeshPro;
        public Image image;

        private ISpriteAtlasManager m_spriteAtlasManager;
        private uint m_spriteHandler;

        public bool isValid => textMeshPro != null && image != null;

        protected override void OnInitView()
        {
            base.OnInitView();

            m_spriteAtlasManager = XGameComs.Get<ISpriteAtlasManager>();
            m_spriteHandler = 0;

            if(isValid)
            {
                orginColor = textMeshPro.color;
            }
        }


        public void OnDestroy()
        {
            UnloadSprite();
        }

        protected override void OnResetView()
        {
            base.OnResetView();

            UnloadSprite();
        }

        protected override void OnUpdateContent()
        {
            if (!isValid)
                return;

            textMeshPro.text = content.text;

            uint iconId = (uint)content.iData;

            if(iconId == 0)
            {
                image.gameObject.BetterSetActive(false);
            }
            else
            {
                image.gameObject.BetterSetActive(true);
                cfg_Icon cfgIcon = GameGlobal.GameScheme.Icon_0(iconId);
                if (cfgIcon != null)
                {
                    if (cfgIcon.strIconPath != "")//根据路径加载
                    {
                        string imgName = cfgIcon.strIconPath;
                        Sprite sprite = Resources.Load<Sprite>(imgName);
                        if (sprite == null)
                        {
                            Debug.LogError("找不到图片资源：Resources/" + imgName);
                            return;
                        }
                        image.sprite = sprite;
                    }
                    else //根据图集加载
                    {
                        UnloadSprite();
                        string altasPath = string.Format(GameConfig.Instance.gameResDir+"/Artist/Icon/{0}/{1}.spriteatlas", cfgIcon.strAtlasPath, cfgIcon.strAtlasPath);

                        m_spriteHandler = m_spriteAtlasManager.LoadSprite2Img(image, altasPath, cfgIcon.strIconName);
                    }
                }
            }
        }

        public void UnloadSprite(bool clearShow = true)
        {
            if(m_spriteHandler != 0)
            {
                m_spriteAtlasManager.UnloadSprite(m_spriteHandler, clearShow);
                m_spriteHandler = 0;
            }
        }

    }
}
