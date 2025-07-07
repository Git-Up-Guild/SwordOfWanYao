/*******************************************************************
** 文件名:	EntityMovePart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  普通实体移动部件(非主角)

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XGame.Entity.Part;


namespace XClient.Entity
{
    public class EntityMovePart : BasePart, IEntityMovePart
    {
        //是否在移动
        private bool m_bMoving = false;

        //移动的目标点
        private Vector3 m_targetPos = Vector3.zero;

        //移动部件的拥有者
        private new ICreatureEntity master;

        protected override void OnInit(object context)
        {
            master = base.master as ICreatureEntity;
        }

        public Vector3 GetTargetPos()
        {
            return m_targetPos;
        }

        public bool IsMoving()
        {
            return m_bMoving;
        }

        public void MoveTo(Vector3 target)
        {
            m_bMoving = true;
            m_targetPos = target;
        }

        public override void OnUpdate()
        {
            if (!m_bMoving)
                return;

            //判断是否已经到达目的地了
            Vector3 curPos = master.GetPos();
            float distance = Vector3.Distance(curPos, m_targetPos);
            float detal = Time.deltaTime;
            float move_detal = master.GetSpeed() * detal;
            if (distance <= 0.01f || distance <= move_detal)
            {
                master.SetPos(ref m_targetPos);
                m_bMoving = false;
                return;
            }


            Vector3 forward = (m_targetPos - curPos).normalized;
            curPos += forward * move_detal;
            master.SetPos(ref curPos);
            master.SetForward(ref forward);
        }
    }
}