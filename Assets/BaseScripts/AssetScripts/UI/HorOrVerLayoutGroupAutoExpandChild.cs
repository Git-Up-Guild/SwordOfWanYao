/*******************************************************************
** 文件名:	HorOrVerLayoutGroupAutoExpandChild.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2019/10/23 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  布局组件中自动扩展的尺寸的子对象

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGame.UI;

namespace XGame.AssetScript.UI
{
    //布局组件中自动扩展的尺寸的子对象
    public class HorOrVerLayoutGroupAutoExpandChild : MonoBehaviour
    {
        [SerializeField]
        private HorizontalOrVerticalLayoutGroup m_LayoutGroup;

        [SerializeField]
        private List<RectTransformDimensionsMonitor> m_Monitors;

        [SerializeField]
        private float m_Min = 0.0f;

        [SerializeField]
        private float m_Max = 0.0f;

        private bool m_IsDirty = false;

        private void OnEnable()
        {
            if (m_Monitors != null)
            {
                foreach (var m in m_Monitors)
                {
                    m.onDimensionsChange.AddListener(OnMonitorSizeChange);
                }
            }
            m_IsDirty = true;
        }

        private void OnDisable()
        {
            if (m_Monitors != null)
            {
                foreach (var m in m_Monitors)
                {
                    if(m != null)
                    {
                        m.onDimensionsChange.RemoveListener(OnMonitorSizeChange);
                    }
                }
            }
        }

        private void Update()
        {
            if (!m_IsDirty)
                return;

            m_IsDirty = false;

            var rectTransOfLayoutGroup = (m_LayoutGroup.transform as RectTransform);
            var rectLayoutGroup = rectTransOfLayoutGroup.rect;
            float childTotalW = 0f, childTotalH = 0f;
            int visibleCount = 0;
            for (var i = 0; i < rectTransOfLayoutGroup.childCount; i++)
            {
                var child = rectTransOfLayoutGroup.GetChild(i);
                if (child == transform)
                {
                    visibleCount++;
                    continue;
                }
                  
                if(child.gameObject.activeSelf)
                {
                    visibleCount++;
                    var rectChildRect = (child as RectTransform).rect;

                    childTotalW += rectChildRect.width;
                    childTotalH += rectChildRect.height;
                }
            }

            var totalSpacing = (visibleCount - 1) * m_LayoutGroup.spacing;

            childTotalW += totalSpacing;
            childTotalH += totalSpacing;

            float expandW = rectLayoutGroup.width - childTotalW;
            float expandH = rectLayoutGroup.height - childTotalH;

            if (m_LayoutGroup is HorizontalLayoutGroup)
            {
                if (m_Min > 0 && expandW < m_Min)
                    expandW = m_Min;

                if (m_Max > 0 && expandW > m_Max)
                    expandW = m_Max;

                if (expandW < 0)
                    expandW = 0;

                var rectTrans = transform as RectTransform;
                rectTrans.sizeDelta = new Vector2(expandW, rectTrans.rect.height);
            }
            else if (m_LayoutGroup is VerticalLayoutGroup)
            {
                if (m_Min > 0 && expandH < m_Min)
                    expandH = m_Min;

                if (m_Max > 0 && expandH > m_Max)
                    expandH = m_Max;

                if (expandH < 0)
                    expandH = 0;

                var rectTrans = transform as RectTransform;
                rectTrans.sizeDelta = new Vector2(rectTrans.rect.width, expandH);
            }

            //var r = transform as RectTransform;
            //Debug.Log("尺寸自动扩展，size=" + r.sizeDelta);
        }

        private void OnMonitorSizeChange(float w, float h)
        {
            m_IsDirty = true;
        }
    }
}
