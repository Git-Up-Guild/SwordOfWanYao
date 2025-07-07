/*******************************************************************
** 文件名:    UIJoystickCommon.cs
** 版  权:    (C) 深圳冰川网络网络科技有限公司 2020 - Speed
** 创建人:    李世柳
** 日  期:    2020/7/21
** 版  本:    1.0
** 描  述:    UI摇杆
** 应  用:    通用摇杆控制（不依赖游戏）

**************************** 修改记录 ******************************
** 修改人:    
** 日  期:    
** 描  述:    
********************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections.Generic;
using JoystickDelegate = System.Action;
namespace XClient.UI
{
    public enum PointerDownEventType
    {
        BeginDrag = 1,
        Drag,
        EndDrag,
        PointerDown,
        PointerUp,
    }

    //public delegate void JoystickDelegate();

    public class UIJoystickCommon : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler
    {
        public GameObject holder;//持有者（根节节点）
        public GameObject thumb;//触点

        public bool alwaysShow = true;
        public float alphaDefault = 0.5f;

        public float maxRadius = 60;
        public bool m_lockRadius;//是否锁定最大半径
        private bool mDisable = false;//是否隐藏

       // private Vector2 mOriginHolderPosition;//记录原始位置
        private RectTransform holderRect;
        private RectTransform thumbRect;
        private RectTransform containerRect;

        private Vector2 vec = Vector3.zero;
        private Vector3 tmpPosition;

        private bool mInitFinish = false;


        public RectTransform arrow;

        Dictionary<PointerDownEventType, JoystickDelegate> m_allJoystickDelegate;

        void Awake()
        {
            holderRect = holder.GetComponent<RectTransform>();
            thumbRect = thumb.GetComponent<RectTransform>();
            containerRect = this.GetComponent<RectTransform>();

        }

        public void OnDestroy()
        {
            if (m_allJoystickDelegate != null)
            {
                m_allJoystickDelegate.Clear();
                m_allJoystickDelegate = null;
            }
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
            #region 被隐藏时，重置
            if (mInitFinish)
            {
                Reset();
            }
            #endregion
        }

        void Start()
        {
            //gameObject.transform.SetAsFirstSibling();
            //设置摇杆触控区大小
            //RectTransform bg = gameObject.GetComponent<RectTransform>();

           // mOriginHolderPosition = holderRect.position;
            mInitFinish = true;
            Reset();
        }

        public void AddListener(PointerDownEventType eventType, JoystickDelegate callBack)
        {
            if (m_allJoystickDelegate == null)
            {
                m_allJoystickDelegate = new Dictionary<PointerDownEventType, JoystickDelegate>();
            }
            JoystickDelegate mCallBack = null;
            if (!m_allJoystickDelegate.TryGetValue(eventType, out mCallBack))
            {
                m_allJoystickDelegate.Add(eventType, callBack);
            }
            else
            {
                mCallBack += callBack;
            }
        }
        public void RemoveListener(PointerDownEventType eventType, JoystickDelegate callBack)
        {
            if (m_allJoystickDelegate == null)
            {
                m_allJoystickDelegate = new Dictionary<PointerDownEventType, JoystickDelegate>();
            }
            JoystickDelegate mCallBack = null;
            if (m_allJoystickDelegate.TryGetValue(eventType, out mCallBack))
            {
                mCallBack -= callBack;
            }


        }

        public void ClearListener()
        {
            if (m_allJoystickDelegate != null)
            {
                m_allJoystickDelegate.Clear();
            }
        }


        public Vector2 getDirection()
        {

            tmpPosition = thumbRect.localPosition;
            vec.x = tmpPosition.x;
            vec.y = tmpPosition.y;
            return vec.normalized;
        }

        float MaxRadius
        { get
            {

                float tempRadius = maxRadius;
                if (m_lockRadius)
                {
                    Vector2 sizeDelta = holderRect.sizeDelta;
                    float radius = Mathf.Min(sizeDelta.x, sizeDelta.y) * 0.5f;

                    if (radius < tempRadius)
                    {
                        tempRadius = radius;
                    }
                }
                if (tempRadius <= 0.0001f)
                {
                    tempRadius = 0.1f;
                }
                return tempRadius;
            } }

        public float GetDistPercentage()
        {
            float radius = MaxRadius;
            float dis = Vector3.Magnitude(thumbRect.localPosition);

            float v = dis / radius;
            v = Mathf.Min(v, 1);
            return v;
            //Vector3.ClampMagnitude(, radius)
        }

        public void SetEnable(bool enabled)
        {
            mDisable = !enabled;
            if (mDisable)
            {
                Reset();
            }
            holder.BetterSetActive(enabled);
        }

        public void Reset()
        {
            if (holderRect && thumbRect)
            {
                //thumbArrow.BetterSetActive(false);
                holderRect.LocalPositionEx(Vector3.zero); 
            }

            if (thumbRect)
            {
                thumbRect.LocalPositionEx(Vector3.zero);

            }
            if (!alwaysShow)
            {
                holder.BetterSetActive(false);
            }
            else
            {
                holder.GetComponent<CanvasRenderer>().SetAlpha(alphaDefault);
                 
            }
            if (arrow.gameObject.activeSelf != false)
            {
                arrow.gameObject.BetterSetActive(false);
            }
        }

        Vector3 m_dir = Vector3.zero;
        void UpdatePosition(PointerEventData eventData)
        {
            

            if (mDisable)
            {
                return;
            }
            Vector3 position;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out position);
            thumbRect.PositionEx(position);

            float tempRadius = MaxRadius;

           // float dis = Vector3.Magnitude(thumbRect.localPosition);
           // Debug.LogError("Vector3.Magnitude(thumbRect.localPosition)>>" + dis +
           //     "," + tempRadius + "," + (dis / tempRadius));

            thumbRect.LocalPositionEx( Vector3.ClampMagnitude(thumbRect.localPosition, tempRadius));
     
            Vector2 dir = getDirection();
            m_dir.x = dir.x;
            m_dir.z = dir.y;
            if (dir.y >= 0)
            {
                arrow.localRotation = Quaternion.Euler(arrow.localRotation.x, arrow.localRotation.y, Vector3.Angle(Vector3.right, m_dir));
            }
            else
            {
                arrow.localRotation = Quaternion.Euler(arrow.localRotation.x, arrow.localRotation.y, -Vector3.Angle(Vector3.right, m_dir));
            }
        }

        void RunJoystickDelegate(PointerDownEventType eventType)
        {
            if (m_allJoystickDelegate == null) return;
            JoystickDelegate mCallBack = null;
            if (m_allJoystickDelegate.TryGetValue(eventType, out mCallBack))
            {
                mCallBack();
            }


        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData);
            RunJoystickDelegate(PointerDownEventType.Drag);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (!alwaysShow)
            {
                if (holder.activeSelf != true)
                {
                    holder.BetterSetActive(true);
                }
            }
            else
            {
                holder.GetComponent<CanvasRenderer>().SetAlpha(1);
            }

            if (arrow.gameObject.activeSelf != true)
            {
                arrow.gameObject.BetterSetActive(true);
            }
            UpdatePosition(eventData);
            RunJoystickDelegate(PointerDownEventType.PointerDown);

           
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Reset();
            RunJoystickDelegate(PointerDownEventType.PointerUp);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RunJoystickDelegate(PointerDownEventType.BeginDrag);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            RunJoystickDelegate(PointerDownEventType.EndDrag);
        }

    }
}