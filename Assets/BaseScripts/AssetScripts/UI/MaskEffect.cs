using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskEffect : MonoBehaviour
{
    //最大查找次数
    public int nMaxLayer = 5;

    //强制重新计算
    private bool m_forceCalcTrigger;
    private Vector3 lastMaskPos = new Vector3();
    private Rect lastRect = new Rect();

    private static Vector3[] m_cornorsTemp = new Vector3[4];

    //材质缓存
    // private static Stack<Material> m_stackMaterials = new Stack<Material>();

    //锁住不改动材质
    private bool m_bLockUpdate = false;

    private List<Renderer> m_aryRenders = new List<Renderer>();
    private List<Material> m_listMat = new List<Material>();

    //mask区域
    private RectTransform m_maskRect;

    //外部设置mask，不自动查找
    private bool m_bCustomMask = false;

    private void Awake()
    {
        this.GetComponentsInChildren<Renderer>(m_aryRenders);
        for (int i = 0; i < m_aryRenders.Count; ++i)
        {
            Material m = AllocMat(m_aryRenders[i].material);
            m_aryRenders[i].material = m;
            m_listMat.Add(m);
        }
    }

    private void OnDestroy()
    {
        //Debug.Log("[MaskEffect] OnDestroy, name=" + name + ", enable=" + enabled, this);

        for (int i = 0; i < m_listMat.Count; ++i)
        {
            RecycleMat(m_listMat[i]);
        }

        m_listMat.Clear();

        m_bCustomMask = false;
    }

    private void OnEnable()
    {
        //无论如何要更新一次
        m_forceCalcTrigger = true;

        //设置贴图
        /*
        if (m_bLockUpdate == false )
        {
            for (int i = 0; i < m_aryRenders.Length; ++i)
            {
                Material m = AllocMat(m_aryRenders[i].material);
                m_aryRenders[i].material = m;
                m_listMat.Add(m);
            }
        }
        */

        FindMaskRect();

        //Debug.Log("[MaskEffect] OnEnable, name=" + name + ", enable=" + enabled, this);
    }

    private void OnDisable()
    {
        m_bCustomMask = false;
        /*
        for (int i = 0; i < m_listMat.Count; ++i)
        {
            RecycleMat(m_listMat[i]);
        }

        m_listMat.Clear();
        m_maskRect = null;
        */

        //Debug.Log("[MaskEffect] OnDisable, name=" + name + ", enable=" + enabled, this);
    }

    private Rect GetWorldCornerRect(RectTransform rectTrans)
    {
        rectTrans.GetWorldCorners(m_cornorsTemp);
        Rect worldRect = new Rect();
        worldRect.xMin = m_cornorsTemp[0].x;
        worldRect.xMax = m_cornorsTemp[2].x;
        worldRect.yMin = m_cornorsTemp[0].y;
        worldRect.yMax = m_cornorsTemp[2].y;
        return worldRect;
    }

    // Update is called once per frame
    void Update()
    {
        m_bLockUpdate = true;
        if (m_maskRect)
        {
            if (lastMaskPos != m_maskRect.position ||
               lastRect != m_maskRect.rect || m_forceCalcTrigger)
            {
                lastRect = m_maskRect.rect;
                lastMaskPos = m_maskRect.position;

                Rect worldMaskRect = GetWorldCornerRect(m_maskRect);
                Vector4 clipRect = new Vector4(worldMaskRect.xMin, worldMaskRect.xMax, worldMaskRect.yMin, worldMaskRect.yMax);
                for (int i = 0; i < m_aryRenders.Count; ++i)
                {
                    Material m = m_listMat[i];
                    m.SetVector("_ClipRect", clipRect);
                    m.EnableKeyword("_ENABLECLIPRECT_ON");
                    m_aryRenders[i].material = m;
                }
                m_forceCalcTrigger = false;

            }
        }

        m_bLockUpdate = false;
    }

    private void FindMaskRect()
    {
        if(m_bCustomMask)
        {
            return;
        }

        m_maskRect = null;
        Transform parent = this.transform.parent;
        for (int i = 0; i < nMaxLayer && parent != null; ++i)
        {
            Mask m = parent.GetComponent<Mask>();
            if(m != null)
            {
                m_maskRect = parent.transform as RectTransform;
                break;
            }

            RectMask2D m2 = parent.GetComponent<RectMask2D>();
            if(m2 != null)
            {
                m_maskRect = parent.transform as RectTransform;
                break;
            }

            //再上一层父亲
            parent = parent.parent;
        }
    }

    private void OnTransformParentChanged()
    {
        m_bCustomMask = false;
        FindMaskRect();
        //Debug.LogError("OnTransformParentChanged");
    }

    //分配一个材质
    private Material AllocMat(Material src)
    {
        /*
        if(m_stackMaterials.Count>0)
        {
            return m_stackMaterials.Pop();
        }
        */

        return new Material(src);
    }

    private void RecycleMat(Material mat)
    {
        Object.DestroyImmediate(mat);
        // m_stackMaterials.Push(mat);
    }

    public void SetMaskTransform(RectTransform transform)
    {
        if(null== transform)
        {
            return;
        }

        Mask m = transform.GetComponent<Mask>();
        if (null != m)
        {
            m_maskRect = transform;
           
        }

        m_bCustomMask = true;
    }
}
