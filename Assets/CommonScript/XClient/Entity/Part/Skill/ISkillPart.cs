/*******************************************************************
** 文件名:	ISkillPart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  技能部件接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity.Part;


namespace XClient.Entity
{
    public interface  ISkillPart : IEntityPart
    {
        /// <summary>
        /// 初始化出生技能
        /// </summary>
        /// <param name="skillID"></param>
        void DoAttack(int skillID);

        /// <summary>
        /// 添加一个技能
        /// </summary>
        /// <param name="skillID"></param>
        void AddSkill(int skillID);

        /// <summary>
        /// 是否清楚预先设置的技能
        /// </summary>
        /// <param name="clear"></param>
        void ClearPreConfig(bool clear);

        /// <summary>
        /// 获取技能数量
        /// </summary>
        /// <returns></returns>
        int GetSkillCount();

        /// <summary>
        /// 获取技能ID
        /// </summary>
        /// <param name="skillIndex"></param>
        /// <returns></returns>
        int GetSkillID(int skillIndex);
    }

}

