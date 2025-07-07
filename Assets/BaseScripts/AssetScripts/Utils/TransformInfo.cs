/*******************************************************************
** 文件名:	TransformInfo.cs 
** 版  权:	(C) 深圳冰川网络技术有限公司 2008 - All Rights Reserved
** 创建人:	郑秀程
** 日  期:	2020-04-16
** 版  本:	1.0
** 描  述:	Transform信息
********************************************************************/

using UnityEngine;
using XGame.Attr;

namespace XGameEditor.Util
{
    class TransformInfo : MonoBehaviour
    {
        public Vector3 worldPosition;
        public Vector3 eulerAngles;
        public Transform target;
        public float distance;

        [HideInInspector]
        public bool isRectTransform = false;
        public Vector3 localPosition;

        [AChecker("isRectTransform", true, CheckerIntentionType.Visible)]
        [CustomDisplay]
        public Vector2 anchoredPosition;

        [AChecker("isRectTransform", true, CheckerIntentionType.Visible)]
        [CustomDisplay]
        public Vector2 offsetMin;

        [AChecker("isRectTransform", true, CheckerIntentionType.Visible)]
        [CustomDisplay]
        public Vector2 offsetMax;

        [AChecker("isRectTransform", true, CheckerIntentionType.Visible)]
        [CustomDisplay]
        public Vector2 sizeDelta;

        public Vector3 newWorldPosition;
        public bool newWorldPositionTrigger;

        public bool isMonitorPosChange;

        public bool isMonitorSizeChange;

        private void Awake()
        {
            if(transform is RectTransform)
            {
                isRectTransform = true;
            }
        }

        private void Update()
        {
            if(isMonitorPosChange)
            {
                if (transform.localPosition != localPosition)
                    Debug.Log($"{name} 位置变化：{transform.localPosition}");
            }

            if (isMonitorSizeChange && isRectTransform)
            {
                if(sizeDelta != (transform as RectTransform).sizeDelta)
                    Debug.Log($"{name} 尺寸变化：{sizeDelta}");
            }

            worldPosition = transform.position;
            localPosition = transform.localPosition;

            if(isRectTransform)
            {
                var rectTrans = (transform as RectTransform);
                anchoredPosition = rectTrans.anchoredPosition;
                offsetMin = rectTrans.offsetMin;
                offsetMax = rectTrans.offsetMax;
                sizeDelta = rectTrans.sizeDelta;
            }

            eulerAngles = transform.eulerAngles;

            if(newWorldPositionTrigger)
            {
                newWorldPositionTrigger = false;
                transform.position = newWorldPosition;
            }

            if (target != null)
            {
                distance = Vector3.Distance(target.position, transform.position);
            }
        }
    }
}
