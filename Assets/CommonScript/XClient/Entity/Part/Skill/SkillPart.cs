/*******************************************************************
** 文件名:	SkillPart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  技能部件实现

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    public class SkillPart : BasePart, ISkillPart
    {
        //还未初始化的时候,攻击列表
        private List<int> m_listWaitAttack = new List<int>();

        //等待添加的技能列表
        private List<int> m_listWaitAddSkills = new List<int>();

        //技能逻辑实现组件
        private SkillCompontBase m_skillComponent = null;

        //清除预先设置的技能
        private bool m_ClearPreConfig = true;

        protected override void OnInit(object context)
        {
            m_ClearPreConfig = false;
            base.OnInit(context);
        }

        //攻击
        public void DoAttack(int skillID)
        {
            if (m_skillComponent != null)
            {
                m_skillComponent.DoAttack(skillID);
            }
            else
            {
                m_listWaitAttack.Add(skillID);
            }
        }

        protected override void OnReset()
        {
            if(null!= m_skillComponent)
            {
                m_skillComponent.Reset();
                m_skillComponent = null;
            }

           
            m_listWaitAttack.Clear();
            m_listWaitAddSkills.Clear();
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            if (id == EntityMessageID.ResLoaded)
            {
                ICreatureEntity ce = master as ICreatureEntity;
                m_skillComponent = ce.GetComponent<SkillCompontBase>();
                if (null != m_skillComponent)
                {
                    //清除预制体配置的
                    if(m_ClearPreConfig)
                    {
                        m_skillComponent.skillIDs.Clear();
                    }

                    int nCount = m_listWaitAttack.Count;
                    for (int i = 0; i < nCount; ++i)
                    {
                        DoAttack(m_listWaitAttack[i]);
                    }
                    m_listWaitAttack.Clear();

                    if(m_listWaitAddSkills.Count > 0)
                        m_skillComponent.skillIDs.AddRange(m_listWaitAddSkills);

                }
            }
        }

        public void AddSkill(int skillID)
        {
            m_listWaitAddSkills.Add(skillID);
        }

        public void ClearPreConfig(bool clear)
        {
            if (m_skillComponent != null&& clear)
            {
                m_skillComponent.skillIDs.Clear();
            }

            m_ClearPreConfig = clear;
        }

        public int GetSkillCount()
        {
            if (m_listWaitAddSkills == null)
                return m_listWaitAddSkills.Count;
            return m_skillComponent.skillIDs.Count;
        }

        public int GetSkillID(int skillIndex)
        {
            if(m_skillComponent == null)
            {
                if (skillIndex < 0 || skillIndex >= m_listWaitAddSkills.Count)
                    return 0;
                return m_listWaitAddSkills[skillIndex];
            }

            if (skillIndex < 0 || skillIndex >= m_skillComponent.skillIDs.Count)
                return 0;
            return m_skillComponent.skillIDs[skillIndex];
        }
    }
}
