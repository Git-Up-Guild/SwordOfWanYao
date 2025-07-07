/*******************************************************************
** 文件名:	IDReco.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.27
** 版  本:	1.0
** 描  述:	
** 应  用:  身份识别组件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace XClient.Entity
{
    public class IDReco : MonoBehaviour
    {
        //实体身份ID
        public ulong entID = 0;

        public Transform hitTrans;

        //实体类型
        public int entType;

        //阵营组件
        public ulong camp = 0;

        //是否能被攻击
        public bool beAttack = true;

        //是否能攻击
        public bool canAttack = true;

        //友方阵营
        public List<ulong> listFriendCamps;

        //敌方阵营
        public List<ulong> listEnemyCamps;

        //上一次的阵营
        private ulong m_lastCamp = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            m_lastCamp = camp;
            if(camp>0)
            {
                IDRecoEntityMgr.Instance.AddIDRecoEntity(this);
            }
            
        }

        private void OnDisable()
        {
            IDRecoEntityMgr.Instance.RemoveIDRecoEntity(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (m_lastCamp!= camp)
            {
                m_lastCamp = camp;
                IDRecoEntityMgr.Instance.ChangeComp(this);
            }
        }

        private void OnDestroy()
        {
            IDRecoEntityMgr.Instance.RemoveIDRecoEntity(this);
        }
    }
}

