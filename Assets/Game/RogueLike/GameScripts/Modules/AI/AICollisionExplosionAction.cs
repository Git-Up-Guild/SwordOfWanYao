/*******************************************************************
** 文件名:	AICollisionExplosionAction.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.26
** 版  本:	1.0
** 描  述:	
** 应  用:  碰到敌方自爆行为

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;


namespace XClient.Entity
{
    public class AICollisionExplosionAction : IAIAction
    {
        //行为的优先级
        private int m_nPriority = 0;

        //这个AI的拥有者
        private ICreatureEntity m_Master;

        //是否运行
        private bool m_bRun = false;

        public bool Create()
        {
           return true;
        }

        public int GetPriority()
        {
            return m_nPriority;
        }

        public void Init(object context = null)
        {
            cfg_AI cfg = context as cfg_AI;
            m_nPriority = cfg.iPriority;
        }

        public bool OnExeUpdate()
        {
            if (m_bRun == false)
                return false;

            return false;
        }

        public void OnReceiveEntityMessage(uint id, object data = null)
        {
            
        }

        public void Release()
        {
            
        }

        public void Reset()
        {
            m_Master = null;
        }

        public void SetMaster(ICreatureEntity master)
        {
            m_Master = master;
        }

        public void Start()
        {
            m_bRun = true;
        }

        public void Stop()
        {
          
        }

        /*
        AI_ACTION_TYPE IAIAction.GetType()
        {
            return AI_ACTION_TYPE.AI_ACTION_COLLISION_EXPLOSION;
        }
        */
    }
}

