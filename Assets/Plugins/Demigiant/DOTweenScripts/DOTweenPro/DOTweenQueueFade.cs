/*******************************************************************
** 文件名:	DOTweenQueueFade.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2022.02.11
** 版  本:	1.0
** 描  述:	
** 应  用:  处理队列方式的渐变

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/



using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTweenQueueFade : MonoBehaviour
{
    //延迟时间
    public float delay = 0.0f;

    //运动时间
    public float duration = 0.2f;

    //运动方式
    public Ease easeType = Ease.OutQuad;

    //初始化的alpha值
    public float startAlpha = 0.0f;

    //目标的alpha值
    public float endAlpha = 1.0f;

    //延后显示列表
    public List<CanvasGroup> listCanvasGroup = new List<CanvasGroup>();


    //运动序列
    private Sequence m_se;

    //每一步骤变化值
    private float m_step = 0.1f;

    //启动时间
    private float m_startTime = 0.0f;

    //是否正在启动
    private bool m_bStartCheck = false;

    // Start is called before the first frame update
    void Start()
    {
        m_se = DOTween.Sequence();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_bStartCheck)
        {
            if(Time.realtimeSinceStartup-m_startTime>= delay)
            {
                m_bStartCheck = false;
                m_step = duration / listCanvasGroup.Count;
                Tweener tweener = DOTween.To(OnProcess, 0, duration, duration);
                tweener.SetEase(easeType);
                m_se.Append(tweener);
            }
        }
    }

    public void OnProcess(float value)
    {
        for (int i = 0; i < listCanvasGroup.Count; ++i)
        {
            listCanvasGroup[i].alpha = Mathf.Lerp(startAlpha, endAlpha, (value-i*m_step)/ m_step);
        }
    }

    private void OnEnable()
    {
        if(listCanvasGroup.Count==0)
        {
            return;
        }

        for(int i=0;i< listCanvasGroup.Count;++i)
        {
            listCanvasGroup[i].alpha = startAlpha;
        }

        m_startTime = Time.realtimeSinceStartup;
        m_bStartCheck = true;

      
    }

    private void OnDisable()
    {
        m_bStartCheck = false;
        if(m_se!=null)
        {
            m_se.Kill(true);
        }
    }

    private void OnDestroy()
    {
        m_se = null;
    }
}
