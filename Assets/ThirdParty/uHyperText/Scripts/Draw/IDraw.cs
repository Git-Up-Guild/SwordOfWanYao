﻿using UnityEngine;
using System;

namespace WXB
{
    public enum DrawType
    {
        Default, // 默认
        Alpha, // 透明
        Offset, // 位置偏移

        Outline, // 画线
        OffsetAndAlpha, // 透明+位置
        Cartoon, // 动画
    }

    /// <summary>
    /// 绘制接口
    /// </summary>
    public interface IDraw
    {
        // 类型
        DrawType type { get; }

        // 名称
        long key { get; set; }

        // 源材质
        Material srcMat { get; set; }

        // 源贴图
        Texture texture { get; set; } 

        // 画布
        CanvasRenderer canvasRenderer { get; }

        // 目标
        RectTransform rectTransform { get; }

        void UpdateSelf(float deltaTime);

        void FillMesh(Mesh workerMesh);

        void UpdateMaterial(Material mat);

        void Release();

        void DestroySelf();

        void OnInit();
    }
}