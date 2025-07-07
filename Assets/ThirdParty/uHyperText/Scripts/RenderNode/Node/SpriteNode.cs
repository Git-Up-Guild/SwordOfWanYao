﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WXB
{
    public class SpriteNode : RectNode
    {
        public string spriteName;
        public Sprite sprite;
        private uint handle;

        private void OnLoadSprite(Sprite sp, uint handle)
        {
            sprite = sp;
            if(handle > 0)
            {
                owner.SetRenderNodeDirty();
            }
        }

        public void LoadSprite()
        {
            if (handle > 0)
            {
                Tools.UnloadSprite(handle);
                handle = 0;
            }

            if (sprite == null)
            {
                handle = Tools.LoadSprite(spriteName, OnLoadSprite);
            }
        }

        protected override void OnRectRender(RenderCache cache, Line line, Rect rect)
        {
            cache.cacheSprite(line, this, sprite, rect);
        }

        public override void Release()
        {
            base.Release();
            spriteName = null;
            sprite = null;
            if (handle > 0)
            {
                Tools.UnloadSprite(handle);
                handle = 0;
            }
        }
    }
}
