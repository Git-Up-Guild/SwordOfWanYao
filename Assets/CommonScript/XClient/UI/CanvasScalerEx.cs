/*******************************************************************
** 文件名:  CanvasScalerEx.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	谌安
** 日  期:	2017/12/11
** 版  本:	1.0 
** 描  述:  整个UI 基础适配类，通过宽高比调整 matchWidthOrHeight及背景适配调整
** 应  用: 

**************************** 修改记录 ******************************
** 修改人:    
** 日  期:    
** 描  述:    
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace XClient.UI
{
    public class CanvasScalerEx : CanvasScaler
    {
        private float aspectRatio;           //屏幕高宽比
        public float maxAspectRatio = 3.0f / 4.0f;        //设置最小宽高比
        public float minAspectRatio = 9.0f / 16.0f;        //设置最大宽高比             

        #region 背景适配
        private RectTransform m_trans;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
            {
                aspectRatio = (float)Screen.height / (float)Screen.width;
                MatchHeightOrWidth();
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        public void MatchHeightOrWidth()
        {
            if (aspectRatio > maxAspectRatio)
            {
                matchWidthOrHeight = 0f;
            }
            else if (aspectRatio < minAspectRatio)
            {
                matchWidthOrHeight = 1f;
            }
        }
    }
}
