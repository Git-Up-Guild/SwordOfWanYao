/*******************************************************************
** 文件名:	TrackMovement.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.01
** 版  本:	1.0
** 描  述:	
** 应  用:  追踪目标

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Network;
using XGame;
using XGame.Entity;
using static XClient.Entity.TrackMovement;

namespace XClient.Entity
{
    public class TrackMovement :  NetObjectBehaviour<TrackMovementData>
    {

        public class TrackMovementData : MonoNetObject
        {
            //是否在移动
            public NetVarBool m_bMoving ;

            //移动的目标点
            public NetVarFloat m_duration;

            //移动的目标点
            public NetVarLong m_target;

            //开始追踪时间
            //public NetVarFloat m_startTrackTime;

            //开始追踪位置
            public NetVarVector3 m_startPos;

            //子弹偏移高度
            public NetVarFloat m_fireOffsetHeight;



            protected override void OnSetupVars()
            {
                //IsDebug = true;
                m_bMoving = SetupVar<NetVarBool>("m_bMoving");
                m_duration = SetupVar<NetVarFloat>("m_duration");
                m_target = SetupVar<NetVarLong>("m_target");
                //m_startTrackTime = SetupVar<NetVarFloat>("m_startTrackTime");
                m_startPos = SetupVar<NetVarVector3>("m_startPos");
                m_fireOffsetHeight = SetupVar<NetVarFloat>("m_fireOffsetHeight");

                m_bMoving.Value = false;

            }
        }


        //追踪位置
        Vector3 m_targetPos;

        //开始时间
        private float m_startTrackTime;


        protected override void OnNetObjectCreate()
        {
            NetObj.m_bMoving.OnChange.AddListener((o, v) =>
            {
                //transform.position = NetObj.m_startPos.Value;
                m_startTrackTime = 0;
            });
        }




        public void StartTrack(ulong target, float duration, float speed,float fireOffsetHeight)
        {
            m_startTrackTime = 0;
            NetObj.m_bMoving.Value = true;
            NetObj.m_target.Value = (long)target;
      
            NetObj.m_fireOffsetHeight.Value = fireOffsetHeight;
            NetObj.m_startPos.Value = transform.position;
            m_targetPos = NetObj.m_startPos.Value;
            __GetTargetPos(ref m_targetPos);

            float distance = Vector3.Distance(m_targetPos, NetObj.m_startPos.Value);
            float time = distance / speed;
            NetObj.m_duration.Value = duration< time? duration:time;

        }

        //是否在移动
        public bool IsMoving()
        {
            return NetObj.m_bMoving.Value;
        }

        // Start is called before the first frame update
        void Start()
        {
           
        }

        private void OnDisable()
        {
            
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {

            if (!NetObj.m_bMoving.Value)
                return;


            __GetTargetPos(ref m_targetPos);

            Vector3 startPos = NetObj.m_startPos.Value;
            if(NetObj.IsOwner)
            {
                startPos  = MonsterSystem.Instance.WorldPositionToBattlePosition(startPos, false);
            }
            m_startTrackTime += Time.deltaTime;

            float t = Mathf.Clamp(m_startTrackTime / NetObj.m_duration.Value, 0, 1);
            Vector3 curPos = Vector3.Lerp(startPos, m_targetPos, t);
            Vector3 bulletPos = transform.position;



            float x = 0, y = 0, z = 0;
            //ScriptableObjectAssets.Instance.ballisticAssets.CalcPosition(m_trackCurveID, t, ref x, ref y, ref z);

            curPos.y += y + NetObj.m_fireOffsetHeight.Value ;
            curPos.x += x;
            curPos.z += z;

            transform.position = curPos;
            //m_master.SetPos(ref curPos);

            //设置方向
            Vector3 forward = (curPos - bulletPos).normalized;
            transform.forward = forward;
            //m_master.SetForward(ref forward);

            if (t >= 1.0)
            {
                transform.position = curPos;
                //m_master.SetPos(ref curPos);
                NetObj.m_bMoving.Value = false;
            }
        }

        private  void __GetTargetPos(ref Vector3 target)
        {
            //获取目标actor
            ICreatureEntity actor = GameGlobal.EntityWorld.Local.GetEntity((ulong)NetObj.m_target.Value) as ICreatureEntity;
            if (actor != null)
            {
                target = actor.GetPos();
            }
        }
    }

}

