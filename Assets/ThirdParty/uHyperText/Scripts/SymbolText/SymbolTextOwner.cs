﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WXB
{
    public partial class SymbolText : Text, IOwner
    {
        //设置#v行距
        [SerializeField]
        private float m_VLineSpace = 0f;

        public float vLineSpace => m_VLineSpace;

        //当前绘制列表
        private List<IDraw> m_UsedDraws = new List<IDraw>();

        //释放所有的绘制对象
        protected void FreeDraws()
        {
            m_UsedDraws.ForEach((IDraw d) => 
            {
                if (d != null)
                {
                    DrawFactory.Free(d);
                }
            });

            m_UsedDraws.Clear();
        }

        // 通过纹理获取渲染对象
        public IDraw GetDraw(DrawType type, long key, Action<IDraw, object> oncreate, object p = null)
        {
            for (int i = 0; i < m_UsedDraws.Count; ++i)
            {
                IDraw draw = m_UsedDraws[i];
                if (draw.type == type && draw.key == key)
                    return m_UsedDraws[i];
            }

            IDraw dro = DrawFactory.Create(gameObject, type);
            dro.key = key;
            m_UsedDraws.Add(dro);

            oncreate(dro, p);

            return dro;
        }
    }
}
