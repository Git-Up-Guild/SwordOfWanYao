/*******************************************************************
** 文件名:	RectTransformSetter.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2022/10/23 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  设置坐标

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;

namespace XGame.AssetScript
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSetter : MonoBehaviour
    {
        public enum PositionType
        {
            AnchoredPosition,
            LocalPosition,
            Position,
        }

        public enum Mode
        {
            OnAwake,
            OnEnable,
        }

        public Mode mode = Mode.OnAwake;

        public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        public Vector2 anchorMax = new Vector2(0.5f, 0.5f);
        public Vector2 pivot = new Vector2(0.5f, 0.5f);

        public PositionType positionType = PositionType.LocalPosition;
        public Vector3 position;

        private void Awake()
        {
            if(mode == Mode.OnAwake)
            {
                UpdateParams();
            }
        }

        private void OnEnable()
        {
            if(mode == Mode.OnEnable)
            {
                UpdateParams();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateParams();
        }
#endif

        public void UpdateParams()
        {
            RectTransform rectTransform = transform as RectTransform;

            switch (positionType)
            {
                case PositionType.AnchoredPosition:
                    {
                        rectTransform.anchoredPosition = position;
                    }
                    break;
                case PositionType.LocalPosition:
                    transform.localPosition = position;
                    break;
                case PositionType.Position:
                    transform.position = position;
                    break;
                default:
                    break;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
        }

    }
}
