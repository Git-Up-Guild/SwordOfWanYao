/*******************************************************************
** 文件名:	AIMoveAction.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.26
** 版  本:	1.0
** 描  述:	
** 应  用:  控制实体的移动

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;
using XClient.Entity;
using GameScripts.KingHero;

namespace GameScripts
{

    public class AIMoveAction : IAIAction, IEventExecuteSink
    {
        //行为的优先级
        private int m_nPriority = 0;

        //这个AI的拥有者
        private ICreatureEntity m_Master;

        //初始化位置
        private Vector3 m_startPos;

        //目标位置
        private Vector3 m_targetPos;

        //移动的距离
        private float m_fDistance;

        //是否运行
        private bool m_bRun = false;

        //前向移动组件
        private ForwardMovement m_forwardMovement;


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
            m_fDistance = cfg.param[0];

        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {

            /*
            if(wEventID== DGlobalEventEx.EVENT_STOP_MOVE)
            {
                SEVENT_STOP_MOVE context = (SEVENT_STOP_MOVE)pContext;
                if (m_Master != null && m_Master.id == context.srcID)
                {
                    //停止移动
                    if (m_forwardMovement != null && m_forwardMovement.IsMoving())
                    {
                        m_forwardMovement.StopMove();
                    }
                }
            }else
            {
                SEVENTP_MOVE_SPEED_UPDATE context = (SEVENTP_MOVE_SPEED_UPDATE)pContext;
                if (m_Master != null && m_Master.id == context.srcID)
                {
                    //停止移动
                    if (m_forwardMovement != null && m_forwardMovement.IsMoving())
                    {
                        m_forwardMovement.StopMove();
                    }
                }
            }
            */
           
           
        }

        public bool OnExeUpdate()
        {

            if (m_bRun == false|| m_forwardMovement==null)
                return false;



            //假如已经到达目标点了
            PrefabPart visiblePart = m_Master.GetPart<PrefabPart>();
            Vector3 curPos = visiblePart.transform.position;// m_Master.GetPos();
            if (Vector3.Distance(curPos, m_targetPos) <= 0.01f)
            {
                m_bRun = false;
                return false;
            }

            //没有在移动或者目标点大于0.1的，重新移动
            if(m_forwardMovement.IsMoving()==false)
            {

                //发送投票消息，询问是否能移动
               // SEVENT_CAN_MOVE.Instance.srcID = m_Master.id;
               // bool ret = GameGlobalEx.EventEgnine.FireVote(DGlobalEventEx.EVENT_CAN_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, SEVENT_CAN_MOVE.Instance);
                //if (ret)
                {
                    m_forwardMovement.StartMove(m_Master.GetSpeed(), curPos, m_targetPos);
                }
            }


            return true; 
        }

        public void OnReceiveEntityMessage(uint id, object data = null)
        {
            if (id == EntityMessageID.ResLoaded)
            {
                m_startPos = m_Master.GetPos();
                Vector3 forward = m_Master.GetForward();
                m_targetPos = m_startPos + forward.normalized * m_fDistance;
                m_bRun = true;

                PrefabPart visiblePart = m_Master.GetPart<PrefabPart>();
                if(null!= visiblePart&& visiblePart.transform)
                {
                    m_forwardMovement = visiblePart.transform.GetComponent<ForwardMovement>();
                    if(m_forwardMovement)
                    {
                        m_forwardMovement.StartMove(m_Master.GetSpeed(), m_startPos, m_targetPos);
                    }
                    //m_forwardMovement.StopMove();
                }
                 
            }
        }

        public void Release()
        {
            Reset();
        }

        public void Reset()
        {
            Stop();
            if (m_forwardMovement)
            {
                m_forwardMovement.StopMove();
            }

            m_Master = null;
            m_forwardMovement = null;
            m_bRun = false;

        }

        public void SetMaster(ICreatureEntity master)
        {
            m_Master = master;
  
        }

        public void Start()
        {
            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
           // eventEngine.Subscibe(this, DGlobalEventEx.EVENT_STOP_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "AIMoveAction:Start");// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)
           // eventEngine.Subscibe(this, DGlobalEventEx.EVENT_MOVE_SPEED_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "AIMoveAction:Start");// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)

        }

        public void Stop()
        {
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
           // eventEngine.UnSubscibe(this, DGlobalEventEx.EVENT_STOP_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)
           // eventEngine.UnSubscibe(this, DGlobalEventEx.EVENT_MOVE_SPEED_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }


    }

}

