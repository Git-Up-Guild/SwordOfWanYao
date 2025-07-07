/*******************************************************************
** 文件名:	MonsterDieEffectMgr.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.9.10
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物延迟死亡系统

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity;
using XGame;
using XGame.Utils;
using XClient.Entity;
using XClient.Common;

namespace XClient.Entity
{
    public class MonsterDieEffectMgr : MonoBehaviourEX<MonsterDieEffectMgr>
    {

        //延后死亡
        public float delayDie = 2;

        //等待死亡队列
        private Dictionary<ulong, float> m_dicWaitDie = new Dictionary<ulong, float>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float curTime = Time.realtimeSinceStartup;
            foreach (ulong entID in m_dicWaitDie.Keys)
            {
                if (curTime - m_dicWaitDie[entID] > delayDie)
                {
                    GameGlobal.EntityWorld.Local.DestroyEntity(entID);
                    m_dicWaitDie.Remove(entID);
                    break;
                }
            }
        }

        public void AddMonster(ulong id)
        {
            float curTime = Time.realtimeSinceStartup;
            if (m_dicWaitDie.ContainsKey(id) == false)
            {

                IEntity entity = GameGlobal.EntityWorld.Local.GetEntity(id);
                if (entity != null)
                {
                    ICreatureEntity ce = entity as ICreatureEntity;
                    if (ce != null)
                    {
                        /*
                        SpineAni pa = ce.GetComponent<SpineAni>();
                        if (pa != null)
                        {
                            pa.DoAction("die");
                        }
                        */
                    }

                    m_dicWaitDie.Add(id, curTime);
                }


            }
        }
    }

}
