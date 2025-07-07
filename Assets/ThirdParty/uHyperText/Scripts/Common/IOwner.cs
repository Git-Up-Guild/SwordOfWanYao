using UnityEngine;
using System;

namespace WXB
{
    public interface IOwner
    {
        // 最小行高
        int minLineHeight { get; set; }

        Around around { get; }

        RenderCache renderCache { get; }

        Anchor anchor { get; }

        float vLineSpace { get; }

        void SetRenderDirty();

        void SetRenderNodeDirty();

        // 元素分割
        ElementSegment elementSegment { get; }

        // 通过纹理获取渲染对象,会考虑合并的情况
        IDraw GetDraw(DrawType type, long key, Action<IDraw, object> onCreate, object para = null);

        //更新渲染的元素
        void UpdateRenderElements();

        Material material { get; }

        LineAlignment lineAlignment { get; }
    }
}