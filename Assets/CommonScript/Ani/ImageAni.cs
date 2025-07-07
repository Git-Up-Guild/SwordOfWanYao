/*******************************************************************
** 文件名:	ImageAni.cs
** 版  权:	(C) 冰川网络
** 创建人:	甘炳钧
** 日  期:	2025.1.17
** 版  本:	1.0
** 描  述:	
** 应  用:  配置2D动画Image版

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace GameScripts.ImageAni
{
    //动作配置项
    [Serializable]
    public class ActionItem
    {

        //动作名称
        public string name;

        //动作间隔时长
        public float interval;

        //动作列表
        public List<Sprite> listSprite;
    }

    public class ImageAni : MonoBehaviour
    {
        //更改sprite动画控制器
        public Image image;

        //动画切换间隔
        public float defaulkInterval = 0.03f;

        //动作配置
        public List<ActionItem> actionItems;

        //当前播放的动作名称
        public string actionName = "";

        //默认的动作
        public string defaultActionName = "";

        //当前播放的动作
        private ActionItem m_curItem;

        //上次切换动作的时间
        private float m_lastChangeFrameTime = 0;

        //帧下标
        private int m_frameIndex = 0;

        //当前是否循环播放
        private bool m_playLoop = false;

        // Start is called before the first frame update
        void Awake()
        {
            if(null== image)
            {
                image = gameObject.GetComponent<Image>(); 
            }


            //默认是非空配
            if(string.IsNullOrEmpty(defaultActionName)==false)
            {
                m_curItem = FindAcionItem(defaultActionName);
                actionName = m_curItem.name;
            }

            if (m_curItem!=null&&actionItems.Count>0)
            {
                m_curItem = actionItems[0];
                actionName = m_curItem.name;
                defaultActionName = actionName;
            }
        }

        private void OnEnable()
        {
            DoAction(actionName);
        }

        // Update is called once per frame
        public void Update()
        {

//#if UNITY_EDITOR
            if (m_curItem==null||actionName != m_curItem.name)
            {
                DoAction(actionName);
            }

            if (null == image)
            {
                image = gameObject.GetComponent<Image>();
            }



//#endif


            if (image != null && m_curItem != null && m_curItem.listSprite.Count > 0)
            {
                float curTime = Time.time;
                float curActionInterval = m_curItem.interval > 0 ? m_curItem.interval : defaulkInterval;
                if (curTime - m_lastChangeFrameTime>= curActionInterval)
                {
                    m_lastChangeFrameTime = curTime;

                    m_frameIndex = (m_frameIndex + 1);

                    if(m_frameIndex>= m_curItem.listSprite.Count)
                    {
                        if(m_playLoop==false)
                        {
                            if (string.IsNullOrEmpty(defaultActionName) == false)
                                DoAction(defaultActionName, true);
                           return;
                        }
                        
                    }

                    //更换图片
                    if(null!= m_curItem)
                    {
                        m_frameIndex %= m_curItem.listSprite.Count;
                        image.sprite = m_curItem.listSprite[m_frameIndex];
                    }
                   

                }
                
            }
        }

        public void DoAction(string name,bool loop = false)
        {
            m_playLoop = loop;  
            actionName = name;
            m_lastChangeFrameTime = Time.time;
            m_curItem = FindAcionItem(name);
            m_frameIndex = 0;

            if (image!=null&&m_curItem!=null&& m_curItem.listSprite.Count>0)
            {
                image.sprite = m_curItem.listSprite[m_frameIndex];
            }
        }

        ActionItem FindAcionItem(string name)
        {
            int nCount = actionItems.Count;
            for(int i=0;i<nCount;++i)
            {
                if(name== actionItems[i].name)
                {
                    return actionItems[i];  
                }
            }

            return null;
        }
    }

}

