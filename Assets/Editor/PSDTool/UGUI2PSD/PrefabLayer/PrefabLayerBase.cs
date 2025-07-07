using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUI2PSD
{
    //预制体层类型
    public enum EPrefabLayerType
    {
        GroupStart, //组开始
        Image,      //图片
        Text,       //文本
        GroupEnd,        //组结束
        Extend,     //扩展层（额外的脚本信息记录层）
    }

    //预制体层基类
    public abstract class PrefabLayerBase
    {
        protected enum ELogType
        {
            Log,
            Warning,
            Error
        }

        public string LayerName;     //名字
        public byte Opacity = 255;    //透明度
        public bool Visible = true;    //显隐
        public Rect PsRect = Rect.zero;   //位置信息(ps的位置信息是左，下，宽，高)
        public bool IsResolveSuccess = true;    //是否解析成功
        public Transform trans;

        protected bool openDebug = true;    //是否输出log

        /// <summary>
        /// 层类型
        /// </summary>
        public abstract EPrefabLayerType LayerType { get; }

        public PrefabLayerBase(Transform tran)
        {
            trans = tran;
            LayerName = tran.name;
            Visible = tran.gameObject.activeSelf;

            RectTransform rectTrans = tran as RectTransform;
            if (rectTrans)
            {
                PsRect = rectTrans.rect;

                /*Rect rect = rectTrans.rect;
                float ps_x = rectTrans.position.x - rect.width * 0.5f;
                float ps_y = rectTrans.position.y - rect.height * 0.5f;
                PsRect = new Rect(rect.x, rect.y, rect.width, rect.height);
                Debug.Log($"x:{PsRect.x}, y:{PsRect.y}, width:{PsRect.width}, height:{PsRect.height}");
                Debug.Log($"yMin:{PsRect.yMin}, xMin:{PsRect.xMin}, yMax:{PsRect.yMax}, xMax:{PsRect.xMax}");
                */
            }
        }

        /// <summary>
        /// 解析层级信息
        /// </summary>
        public abstract void ResolveLayerInfo();

        protected void Print(string message, ELogType logType = ELogType.Log)
        {
            if (openDebug)
            {
                switch (logType)
                {
                    case ELogType.Log:
                        Debug.Log(message);
                        break;
                    case ELogType.Warning:
                        Debug.LogWarning(message);
                        break;
                    case ELogType.Error:
                        Debug.LogError(message);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
