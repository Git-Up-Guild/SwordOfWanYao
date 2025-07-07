using System;
using System.Collections.Generic;
using UnityEngine;
using WXB;
using XClient.Common;
using XGame.Atlas;

namespace XGame.AssetScript.UHyperText
{
    public class UHyperTextToolExtend : MonoBehaviour
    {
        //回调列表
        private static Dictionary<uint, Action<Sprite, uint>> s_DicCallbacks = new Dictionary<uint, Action<Sprite, uint>>();

        private OnSpriteLoadedCallback m_SpriteLoadedCallback;

        private void Awake()
        {
            WXB.Tools.s_load_sprite = LoadSprite;
            WXB.Tools.s_unload_sprite = UnloadSprite;
            m_SpriteLoadedCallback = OnLoadSpriteComplete;
        }

        private void OnLoadSpriteComplete(Sprite sp, uint handle, int userData = 0)
        {
            Action<Sprite, uint> callback;
            if (s_DicCallbacks.TryGetValue(handle, out callback))
            {
                callback?.Invoke(sp, handle);
            }
            s_DicCallbacks.Remove(handle);
        }

        private void OnDestroy()
        {
            ISpriteAtlasManager manager = XGameComs.Get<ISpriteAtlasManager>();
            if (manager != null)
            {
               foreach(var h in s_DicCallbacks.Keys)
                {
                    manager.UnloadSprite(h);
                }
            }
            s_DicCallbacks.Clear();
        }

        public uint LoadSprite(string name, Action<Sprite,uint> callback)
        {
            uint iconID = uint.Parse(name);
            if (GameGlobal.GameScheme == null)
                return 0;

            cfg_Icon cfgIcon = GameGlobal.GameScheme.Icon_0(iconID);
            if (cfgIcon == null)
            {
                Debug.LogError("加载图标配置失败，图标ID=" + iconID);
                return 0;
            }

            ISpriteAtlasManager manager = XGameComs.Get<ISpriteAtlasManager>();
            if (manager == null)
                return 0;

            string fullPath = $"G_Resources/Artist/Icon/{cfgIcon.strAtlasPath}/{cfgIcon.strAtlasPath}.spriteatlas";
            uint handle = manager.LoadSprite(fullPath, cfgIcon.strIconName, m_SpriteLoadedCallback);
            s_DicCallbacks.Add(handle, callback);
            return handle;
        }

        public void UnloadSprite(uint handle)
        {
            if (s_DicCallbacks.ContainsKey(handle))
                s_DicCallbacks.Remove(handle);

            ISpriteAtlasManager manager = XGameComs.Get<ISpriteAtlasManager>();
            if (manager != null)
            {
                manager.UnloadSprite(handle);
            }
        }
    }
}
