using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 网格重建分析
/// </summary>


namespace Analyze
{
    public class UGUIRebuild : MonoBehaviour
    {

        IList<ICanvasElement> m_LayoutRebuildQueue;
        IList<ICanvasElement> m_GraphicRebuildQueue;
        private int rebuildIdx; //重建次数
        private List<Canvas> canvasLst;
        private void Awake()
        {
            System.Type type = typeof(CanvasUpdateRegistry);
            FieldInfo field = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            m_LayoutRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
            field = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            m_GraphicRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
            rebuildIdx = 0;
            canvasLst = new List<Canvas>();
            transform.GetComponentsInChildren<Canvas>(canvasLst);
           
        }

        private void Update()
        {
            for (int j = 0; j < m_LayoutRebuildQueue.Count; j++)
            {
                var rebuild = m_LayoutRebuildQueue[j];
                if (ObjectValidForUpdate(rebuild))
                {
                    Graphic graphic = rebuild.transform.GetComponent<Graphic>();
                    if(graphic!=null)
                    {
                       if(canvasLst.Contains(graphic.canvas))
                        {
                            ++rebuildIdx;
                            Debug.LogWarningFormat("{0}引起{1}网格重建", rebuild.transform.name, graphic.canvas.name);
                        }
                    }
                }
            }

            for (int j = 0; j < m_GraphicRebuildQueue.Count; j++)
            {
                var element = m_GraphicRebuildQueue[j];
                if (ObjectValidForUpdate(element))
                {
                    Graphic graphic = element.transform.GetComponent<Graphic>();
                    if (graphic != null)
                    {
                        if(canvasLst.Contains(graphic.canvas))
                        {
                            ++rebuildIdx;
                            Debug.LogWarningFormat("{0}引起{1}网格重建", element.transform.name, graphic.canvas.name);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 当前重建次数
        /// </summary>
        /// <returns></returns>
        public int NowRebuildCount()
        {
            return rebuildIdx;
        }
        private bool ObjectValidForUpdate(ICanvasElement element)
        {
            var valid = element != null;

            var isUnityObject = element is Object;
            if (isUnityObject)
                valid = (element as Object) != null; //Here we make use of the overloaded UnityEngine.Object == null, that checks if the native object is alive.

            return valid;
        }
    }
}
