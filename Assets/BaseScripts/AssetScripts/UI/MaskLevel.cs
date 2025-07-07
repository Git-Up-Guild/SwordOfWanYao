using System;
using System.Collections.Generic;
using UnityEngine;
using XGame.Utils;

public class MaskLevel : MonoBehaviour
{
    public Material matMaskLevel;

    //已经打过的层
    public int openLevelMask;

    //当前攻击的层
    public int attackLevelMask;

    //是否开始播放开放过渡动画
    public bool bPlayOpenAni = false;

    //当前过渡的范围
    public float fFadeRadius;

    public float speed = 10;

    public float fAttackAphlaRadius = 1;

    private float m_fMaxRadius;

    //攻击的size
    private Vector4 m_vAttackSize;

    //攻击列表
    public List<Transform> m_listAttacks = new List<Transform>();

    //上一个关卡对象
    public Transform m_lastLevel;

    //当前的关卡对象
    public Transform m_attackLevel;

    private Action<int> m_callback = null;

    //开放动画控制器
    private Vector4 m_posAndRadius = new Vector4();

    //开放动画控制器
    private Vector4 m_attackCenter = new Vector4();

    [SerializeField]
    private Camera m_camera;

    private static Vector3[] s_tempVector3 = new Vector3[4];

    private bool m_enableRenderPass;

    // Start is called before the first frame update
    void Start()
    {
        if(m_camera == null)
            m_camera = GetComponent<Camera>();
        EnableRenderPass();
    }

    public void SetCamera(Camera camera)
    {
        m_camera = camera;
       // LevelMaskRenderPass.UpdateContext(matMaskLevel,m_camera, ref m_posAndRadius, ref m_attackCenter, ref m_vAttackSize, openLevelMask, attackLevelMask);
    }

    public void EnableRenderPass()
    {
        m_enableRenderPass = true;
      //  LevelMaskRenderPass.enableRenderPass = true;
    }

    public void DisableRenderPass()
    {
        m_enableRenderPass = false;
       // LevelMaskRenderPass.enableRenderPass = false;
    }

    // Update is called once per frame
    void Update()
    {
        //没有当前的关卡，返回
        if (m_attackLevel == null)
        {
            return;
        }

        //没有上一关的，取当前关
        if (m_lastLevel==null)
        {
            m_lastLevel = m_attackLevel;
        }
        
        //播放开放动画
        if (bPlayOpenAni)
        {

#if UNITY_EDITOR
            CalcMaxRadius();
#endif

            Vector3 lastLvlPos = m_lastLevel.transform.position;
            
            fFadeRadius += Time.deltaTime * speed;
            m_posAndRadius = new Vector4(lastLvlPos.x, lastLvlPos.y, lastLvlPos.z, fFadeRadius);
           
            //已经播放完成，回调给外部
            if (fFadeRadius>=m_fMaxRadius)
            {
                bPlayOpenAni = false;
                m_vAttackSize = new Vector4();
                fFadeRadius = 0;
                m_callback?.Invoke(0);
                m_callback = null;

            }
        }
        else
        {
            //位置和半径，半径置为空
            m_posAndRadius.w = 0;
        }

        Vector3 attackCenter = m_attackLevel.transform.position;
        m_attackCenter = new Vector4(attackCenter.x, attackCenter.y, attackCenter.z, fAttackAphlaRadius);

        //同步渲染现场
       // LevelMaskRenderPass.UpdateContext(matMaskLevel,m_camera, ref m_posAndRadius, ref m_attackCenter,ref m_vAttackSize,openLevelMask, attackLevelMask);
    }

    //设置关卡
    public void SetLevel(Transform attackLevel, Transform lastLevel)
    {
        m_listAttacks.Clear();
        m_lastLevel = lastLevel;
        m_attackLevel = attackLevel;
        if(null == m_lastLevel)
        {
            m_lastLevel = attackLevel;
        }
        CalcMaxRadius();
    }


    public void SetLevels(List<Transform> listAttacks, Transform lastLevel)
    {
        if(listAttacks.Count<1)
        {
            return;
        }

        m_listAttacks.Clear();
        m_listAttacks.AddRange(listAttacks);

        m_lastLevel = lastLevel;
        m_attackLevel = m_listAttacks[0];
        if (null == m_lastLevel)
        {
            m_lastLevel = m_attackLevel;
        }
        CalcMaxRadius();
    }

    //停止动画
    public void StopConquerAni()
    {
        fFadeRadius += 999999;
        Update();
        /*
        bPlayOpenAni = false;
        m_callback?.Invoke(1);
        m_listAttacks.Clear();
        m_lastLevel = null;
        m_attackLevel = null;
        */
    }

    //播放过渡动画
    public void PlayConquerAni(Action<int> callback)
    {
        if(null == m_lastLevel)
        {
            callback(1);
            return;
        }

        m_callback = callback;
        bPlayOpenAni = true;
        fFadeRadius = 0;
        CalcMaxRadius();
    }

    //计算当前攻打关卡的范围
    private void CalcMaxRadius()
    {
        Vector3 pos = m_lastLevel.transform.position;
        //Bounds bounds = m_attackLevel.bounds;
        (m_attackLevel as RectTransform).GetWorldCorners( s_tempVector3);

        Bounds bounds = new Bounds();
        bounds.min = s_tempVector3[0];
        bounds.max = s_tempVector3[2];

        Vector3 vMin, vMax;

        int nCount = m_listAttacks.Count;
        for (int i=1;i< nCount; ++i)
        {
            vMin = s_tempVector3[0];
            vMax = s_tempVector3[2];
            (m_listAttacks[i] as RectTransform).GetWorldCorners(s_tempVector3);
            if(vMin.x> s_tempVector3[0].x)
            {
                vMin.x = s_tempVector3[0].x;
            }
            if (vMin.y > s_tempVector3[0].y)
            {
                vMin.y = s_tempVector3[0].y;
            }
            if (vMin.z > s_tempVector3[0].z)
            {
                vMin.z = s_tempVector3[0].z;
            }

            if (vMax.x < s_tempVector3[2].x)
            {
                vMax.x = s_tempVector3[2].x;
            }
            if (vMax.y < s_tempVector3[2].y)
            {
                vMax.y = s_tempVector3[2].y;
            }
            if (vMax.z < s_tempVector3[2].z)
            {
                vMax.z = s_tempVector3[2].z;
            }
        }


        Vector3 center = bounds.center;
        m_vAttackSize = new Vector4(bounds.size.x, bounds.size.y, bounds.size.z);
        m_fMaxRadius = Vector3.Distance(pos, center) + bounds.size.magnitude*2;

    }

}
