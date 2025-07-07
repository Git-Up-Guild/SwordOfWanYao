using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D;

namespace WXB
{
    [System.Serializable]
    public class Cartoon
    {
        // 动画名
        public string name; 

        // 播放速度
        public float fps;

        // 与其他元素之间的间隔
        public float space = 2f; 

        //宽
        public int width;

        //高
        public int height;

        //图集
        [HideInInspector]
        public SpriteAtlas spriteAtlas;

        //图片的名称
        [HideInInspector]
        public string[] spriteNames;

        //图片列表
        [HideInInspector]
        public Sprite[] sprites;

        //帧数
        public int frameCount
        {
            get
            {
                if (spriteNames == null)
                    return 0;
                return spriteNames.Length;
            }
        }

        public Sprite GetSprite(int index)
        {
            if (index >= frameCount || index < 0)
                return null;

            if (sprites == null || sprites.Length < frameCount)
                sprites = new Sprite[frameCount];

            Sprite sp = sprites[index];
            if(sp == null)
            {
                sp = spriteAtlas.GetSprite(spriteNames[index]);
                sprites[index] = sp;
            }
            return sp;
        }
    }
}