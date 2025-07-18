﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WXB
{
    public class Around
    {
        // 环绕的区别
        private List<Rect> m_Rects = new List<Rect>(); 

        public void Add(Rect rect)
        {
            m_Rects.Add(rect);
        }

        public void Clear()
        {
            m_Rects.Clear();
        }

        public bool isContain(Rect rect, out float ox)
        {
            if (m_Rects.Count == 0)
            {
                ox = 0f;
                return true;
            }

            return isContain(rect.x, rect.y, rect.width, rect.height, out ox);
        }

        public bool isContain(float x, float y, float w, float h, out float ox)
        {
            if (m_Rects.Count == 0)
            {
                ox = 0f;
                return true;
            }

            Rect r = new Rect(x, y, w, h);
            for (int i = 0; i < m_Rects.Count; ++i)
            {
                //与图片显示区域重叠了，返回图片右侧的坐标
                if (m_Rects[i].Overlaps(r))
                {
                    ox = (m_Rects[i].xMax + 5f);
                    return false;
                }
            }

            ox = 0f;
            return true;
        }
    }
}