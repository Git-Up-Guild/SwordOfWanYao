/*******************************************************************
** 文件名:	ForwardMovement.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.01
** 版  本:	1.0
** 描  述:	
** 应  用:  向前移动效果

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Network;
using static XClient.Entity.ForwardMovement;

namespace XClient.Entity
{
    public class ForwardMovement :  NetObjectBehaviour<ForwardMovementData>
    {
        public class ForwardMovementData : MonoNetObject
        {
            //是否在移动
            public NetVarBool m_bMoving;

            //移动的目标点
            public NetVarVector3 m_startPos;

            //移动的目标点
            public NetVarVector3 m_targetPos;

            //移动速度
            public NetVarFloat m_speed;


            protected override void OnSetupVars()
            {
  
                //IsDebug = true;
                m_bMoving = SetupVar<NetVarBool>("m_bMoving");
                m_startPos = SetupVar<NetVarVector3>("m_startPos");
                m_targetPos = SetupVar<NetVarVector3>("m_targetPos");
                m_speed = SetupVar<NetVarFloat>("m_speed");
                m_bMoving.Value = false;

                
            }
        }

        //子弹拥有者
         private ICreatureEntity m_master;

        public void StartMove( float speed, Vector3 start,Vector3 target)
        {
            /*
            m_bMoving = true;
            m_targetPos = target;
            m_master = master;
            m_speed = speed;
            */

            NetObj.m_speed.Value = speed;
            NetObj.m_targetPos.Value = target;
            NetObj.m_bMoving.Value = true;
            NetObj.m_startPos.Value = start;

           // Debug.LogError("target="+ target);
      

        }

        public void EnableSync(bool enableSync)
        {
            NetObj.IsEnableSync = enableSync;   
        }

        /*
        public void StartMove(float speed, Vector3 target)
        {
            m_bMoving = true;
            m_targetPos = target;
            m_speed = speed;    
        }
        */

        //是否在移动
        public bool IsMoving()
        {
            return NetObj.m_bMoving.Value;
        }

        public void StopMove()
        {
             NetObj.m_bMoving.Value = false;
        }

        void Start()
        {
            //NetObj.m_bMoving.Value = false;
        }

        protected override void OnNetObjectCreate()
        {
            base.OnNetObjectCreate();

            NetObj.m_startPos.OnChange.AddListener((o, v) =>
            {
                if (NetObj.IsOwner==false)
                {
                    transform.position = MonsterSystem.Instance.WorldPositionToBattlePosition(NetObj.m_startPos.Value, false);
                   
                }else
                {
                    transform.position = NetObj.m_startPos.Value;
                }
               

                //if (!IsOwner)
                //{
                //    Debug.Log($"{NetObj.NetID} 开始移动！");
                //}
            });
        }


        // Start is called before the first frame update
        private void OnDisable()
        {
            NetObj.m_bMoving.Value = false;
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
            if (NetObj.m_bMoving.Value==false) 
                return;


            /*
            if (NetObj.IsOwner)
            {
                this.gameObject.BetterSetActive(false);
                return;
            }*/
            


            //判断是否已经到达目的地了
            Vector3 curPos = transform.position;
            Vector3 targetPos = NetObj.m_targetPos.Value;
            //if (NetObj.IsOwner==false)
            //{
            //  //  Debug.LogError("转换前 targetPos" + targetPos);
            //    targetPos = MonsterSystem.Instance.WorldPositionToBattlePosition(targetPos, false);
            //  //  Debug.LogError("转换后 targetPos" + targetPos);
            //}


          

            float distance = Vector3.Distance(curPos, targetPos);
            float detal = Time.deltaTime;
            float move_detal = NetObj.m_speed.Value * detal;

            if (distance <= 0.01f || distance <= move_detal)
            {
                //m_master.SetPos(ref m_targetPos);
                transform.position = targetPos; 
                NetObj.m_bMoving.Value = false;
                return;
            }


            Vector3 forward = (targetPos - curPos).normalized;
            curPos += forward * move_detal;

            transform.position = curPos;

            //NetID.Temp.Set(NetObj.NetID);
            //Debug.Log($"{NetID.Temp} 更新坐标！{curPos}");

            //name = $"{NetID.Temp}";

            transform.forward = forward;
            //m_master.SetPos(ref curPos);
            //m_master.SetForward(ref forward);


        }
    }
}

