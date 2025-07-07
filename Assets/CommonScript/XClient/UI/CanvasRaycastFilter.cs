/*******************************************************************
** 文件名:    CanvasRaycastFilter.cs
** 版  权:    (C) 深圳冰川网络网络科技有限公司 2016 - Speed
** 创建人:    杜家武
** 日  期:    2017/7/4
** 版  本:    1.0
** 描  述:    
** 应  用:   判断区域是否可以响应事件
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace XClient.UI
{
	public class RectangleRaycast : MaskableGraphic
    {
        protected override void Awake()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}
