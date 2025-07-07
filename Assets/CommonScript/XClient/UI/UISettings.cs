///*******************************************************************
//** 文件名:    UISettings.cs
//** 版  权:    (C) 冰川网络网络科技
//** 创建人:    郑秀程
//** 日  期:    2016/3/16
//** 版  本:    1.0
//** 描  述:    UI设置脚本

//**************************** 修改记录 ******************************
//** 修改人:    
//** 日  期:    
//** 描  述:    
//********************************************************************/
//using XGame.Utils;
//using System;
//using UnityEngine;
//using UnityEngine.UI;
//using XGame.LOP;

//namespace XClient.UI
//{
//    public class UISettings : MonoBehaviourEX<UISettings>
//    {
//        //画布
//        public Canvas canvas;

//        //主相机
//        public Camera mainCamera;

//        //UI相机
//        public Camera uiCamera;

//        //UI灰化现实
//        public Material uigrayMat;

//        public override void Awake()
//        {
//            base.Awake();

//            mainCamera = Camera.main;
//            uiCamera = XGame.UI.UICamera.uiCamera;
//            canvas = GetComponent<Canvas>();
//        }

//        public float GetWScale()
//        {
//            CanvasScaler scaler = GetComponent<CanvasScaler>();
//            return (transform as RectTransform).rect.width / scaler.referenceResolution.x;
//        }

//        public float GetHScale()
//        {
//            CanvasScaler scaler = GetComponent<CanvasScaler>();
//            return (transform as RectTransform).rect.height / scaler.referenceResolution.y;
//        }

//    }
//}
