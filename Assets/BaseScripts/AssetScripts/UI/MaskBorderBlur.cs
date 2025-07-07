using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskBorderBlur : MonoBehaviour
{
    //mask区域
    public RectTransform m_maskRect;

    //背景图对象
    public GameObject m_BG;

    public bool m_enableTop = true;

    public bool m_enableBottom = true;

    private Image m_selfImg = null;
    private Material m_matMaskBlur = null;

    //强制重新计算
    public bool m_forceCalcTrigger;

    //private Rect m_BgWorldRect;
    //private Rect m_MaskWorldRect;
    //private Rect m_SelfWorldRect;

    private static Vector3[] m_cornorsTemp = new Vector3[4];

    //材质缓存
    private static Stack<Material> m_stackMaterials = new Stack<Material>();

    //锁住不改动材质
    private bool m_bLockUpdate = false;

    private void Awake()
    {
        m_selfImg = GetComponent<Image>();
        if (m_selfImg&&m_BG)
        {
            Image bgImg = m_BG.GetComponent<Image>();

            if(null==bgImg)
            {
                bgImg = m_BG.GetComponentInChildren<Image>();
            }

            if (bgImg)
            {
                m_selfImg.sprite = bgImg.sprite;
            }
        }

    }

    private void OnDestroy()
    {
        if (m_selfImg != null)
        {
            m_selfImg.sprite = null;
        }
            

        if(null!= m_matMaskBlur)
        {
            RecycleMat(m_matMaskBlur);
            m_matMaskBlur = null;
        }
        
    }

    private void OnEnable()
    {
        //gameObject.BetterSetActive(false);
        //return;

        //无论如何要更新一次
        m_forceCalcTrigger = true;

        //设置贴图
        if (m_bLockUpdate == false && m_selfImg != null)
        {
            if (null == m_matMaskBlur)
            {
                m_matMaskBlur = AllocMat(m_selfImg.material);
                m_selfImg.material = m_matMaskBlur;
               
            }

            m_selfImg.enabled = false;
            //推动一次update 合并rebuild
            //Update();
        }

    }

    private void OnDisable()
    {
        if (m_bLockUpdate==false&&null != m_matMaskBlur)
        {
            RecycleMat(m_matMaskBlur);
            m_matMaskBlur = null;
        }
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

    private Vector4 GetTexOffsetAndScale()
    {
        Rect bgWorldRect = GetWorldCornerRect(m_BG.transform as RectTransform);
        Rect selfWorldRect = GetWorldCornerRect(transform as RectTransform);

        //计算偏移
        Vector4 offset = new Vector4();

        //计算缩放
        offset.x = selfWorldRect.width / bgWorldRect.width;
        offset.y = selfWorldRect.height / bgWorldRect.height;

        //计算偏移
        offset.z = selfWorldRect.xMin - bgWorldRect.xMin;
        offset.w = selfWorldRect.yMin - bgWorldRect.yMin;
        offset.z = offset.z / bgWorldRect.width;
        offset.w = offset.w / bgWorldRect.height;

        return offset;
    }

    private Vector3 lastBgPos = new Vector3();
    private Vector3 lastMaskPos = new Vector3();
    private Rect lastRect = new Rect();

    // Update is called once per frame
    void LateUpdate()
    {
        m_bLockUpdate = true;

        if (m_selfImg && m_maskRect && m_BG != null)
        {

            if(lastBgPos!= m_BG.transform.position||
               lastMaskPos!= transform.position||
               lastRect!= m_maskRect.rect|| m_forceCalcTrigger)
            {

              

                RectTransform selfRect = transform as RectTransform;
                RectTransform bgRect = m_BG.transform as RectTransform;

                Rect rc = bgRect.rect;
                bgRect.GetWorldCorners(m_cornorsTemp);
                //强行改成左下角对齐
                selfRect.anchorMin = new Vector2();
                selfRect.anchorMax = new Vector2();
                selfRect.pivot = new Vector2();
                //同步宽高
                selfRect.sizeDelta = new Vector2(rc.width, rc.height);
                //同步位置
                selfRect.position = m_cornorsTemp[0];
  


                lastRect = m_maskRect.rect;
                lastBgPos = m_BG.transform.position;
                lastMaskPos = transform.position;


               if (m_matMaskBlur)
               {
                        Rect worldMaskRect = GetWorldCornerRect(m_maskRect);

                        Vector4 bound = new Vector4(worldMaskRect.yMin, worldMaskRect.yMax, worldMaskRect.width, worldMaskRect.height);
                        m_matMaskBlur.SetVector("_Bound", bound);
                        m_matMaskBlur.SetTextureOffset("_MainTex", new Vector2(0, 0));
                        m_matMaskBlur.SetTextureScale("_MainTex", new Vector2(1, 1));
                        m_matMaskBlur.SetVector("_EnableBorder", new Vector4(m_enableTop ? 1 : 0, m_enableBottom ? 1 : 0, 0, 0));

                    //更新一下材质
                    m_selfImg.enabled = false;
                        //m_selfImg.SetMaterialDirty();
                    //m_selfImg.material = m_matMaskBlur;
                    //刷新一下mask材质
                    //gameObject.BetterSetActive(false);
                    //gameObject.BetterSetActive(true);

                    if (m_selfImg.enabled == false)
                    {
                        m_selfImg.enabled = true;
                    }

                    m_forceCalcTrigger = false;
                }

               
                
            }
        }

        m_bLockUpdate = false;


    }

    //分配一个材质
    private Material AllocMat(Material src)
    {
        if(m_stackMaterials.Count>0)
        {
            return m_stackMaterials.Pop();
        }

        return new Material(src);
    }

    private void RecycleMat(Material mat)
    {
        m_stackMaterials.Push(mat);
    }
}
