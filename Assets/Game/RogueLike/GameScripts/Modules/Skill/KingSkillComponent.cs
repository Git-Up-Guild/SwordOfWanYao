/*******************************************************************
** 文件名:	KingSkillComponent.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.08
** 版  本:	1.0
** 描  述:	
** 应  用:  王国demo技能释放组件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using GameScripts;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.Entity;

namespace GameScripts.KingHero
{
    public class KingSkillComponent : SkillCompontBase
    {

        [Header("动作组件")]
        public SpriteRenderAni spriteRenderAni;

        [Header("Spine动画控制组")]
        public SpineAni spineAni;

        [Header("Animator动画组")]
        public Animator animator;



        //记录出发次数
        private Dictionary<int, int> m_dicTriggerCount = new Dictionary<int, int>();


        //临时选目标

        // Start is called before the first frame update
        void Start()
        {
            if (bulletLauncher != null)
            {
                //设置本demo的全局环境对象提供者,支撑子弹回调
                bulletLauncher.bulletEnvProvider = KingBulletEnvProvider.Instance;
                bulletLauncher.bulletItems.Clear();
            }

            if (null == spriteRenderAni)
            {
                spriteRenderAni = GetComponentInChildren<SpriteRenderAni>();
            }

            if (null == spineAni)
            {
                spineAni = GetComponentInChildren<SpineAni>();
            }

            if (null == animator)
            {
                animator = GetComponent<Animator>();
            }


        }


        // Update is called once per frame
        new void Update()
        {
            base.Update();
        }


        //添加技能
        public void InitSkills(List<int> listSkillID)
        {
            skillIDs.Clear();
            skillIDs.AddRange(listSkillID);
            __InitCooldingTime();
        }

        public void InitSkills(int[] listSkillID)
        {
            skillIDs.Clear();
            skillIDs.AddRange(listSkillID);
            __InitCooldingTime();
        }

        private void __InitCooldingTime()
        {
            float curTime = Time.realtimeSinceStartup;
            skillCooldingTime.Clear();
            listLastCastTime.Clear();
            int count = skillIDs.Count;
            for (int i = 0; i < count; ++i)
            {
                cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillIDs[i]);
                if (cfg != null)
                {
                    skillCooldingTime.Add(cfg.innerCD);
                    listLastCastTime.Add(curTime);
                }
            }
        }






        //判断技能是否能释放
        public override bool CanAttack(int skillID)
        {

            //判断是否到了最大的触发次数
            if (m_dicTriggerCount.ContainsKey(skillID))
            {
                cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillID);
                if (cfg == null)
                {
                    Debug.LogError("不存在的技能配置 ID=" + skillID);
                    return false;
                }

                int triggerCount = m_dicTriggerCount[skillID];
                if (cfg.triggerCount <= triggerCount)
                {
                    return false;

                }

            }

            /*
            SEVENT_CAN_USE_SKILL context = SEVENT_CAN_USE_SKILL.Instance;
            context.srcID = reco.entID;
            context.skillID = skillID;

            bool ret = GameGlobalEx.EventEgnine.FireVote(DGlobalEventEx.EVENT_CAN_USE_SKILL, DEventSourceType.SOURCE_TYPE_ENTITY, 0, context);
            */
            return true;
        }

        public override void OnAttack(int skillID, bool needAct = true)
        {



            cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillID);
            if (cfg == null)
            {
                Debug.LogError("不存在的技能配置 ID=" + skillID);
                return;
            }

            //音效
            if(cfg.iAudioID > 0)
                NetEntityEffectSyncManager.Instance.PlayAudio(cfg.iAudioID);

            //做动作
            if (string.IsNullOrEmpty(cfg.szCastAction) == false)
            {
                spriteRenderAni?.DoAction(cfg.szCastAction);
                spineAni?.DoAction(cfg.szCastAction);

                if (animator != null)
                    animator.Play(cfg.szCastAction);
            }

            //非子弹类型的
            if (cfg.iBulletGroupID == 0)
            {

                List<IDReco> listReco = KingBulletEnvProvider.Instance.SelectTargets(reco, cfg.targetRule, cfg.targetParams);
                int nTargetCount = listReco.Count;
                IDReco target = null;
                for (int k = 0; k < nTargetCount; ++k)
                {
                    target = listReco[k];
                    KingBulletEnvProvider.Instance.AddBuff(target, skillID, cfg.commandSkillList);

                }
            }

            //记录触发次数
            if (cfg.triggerCount > 0)
            {
                if (m_dicTriggerCount.ContainsKey(skillID) == false)
                {
                    m_dicTriggerCount.Add(skillID, 1);
                }
                else
                {
                    m_dicTriggerCount[skillID] = m_dicTriggerCount[skillID] + 1;
                }
            }


            //发送攻击事件
            /*
            SEVENT_USE_SKILL context = SEVENT_USE_SKILL.Instance;
            context.srcID = reco.entID;
            context.skillID = skillID;

            GameGlobalEx.EventEgnine.FireExecute(DGlobalEventEx.EVENT_USE_SKILL, DEventSourceType.SOURCE_TYPE_ENTITY, 0, context);
            */

            //1
            if (true && reco != null)
            {

                IEntityManager manager = GameGlobal.EntityWorld.Local;
                ICreatureEntity entity = manager.GetEntity(reco.entID) as ICreatureEntity;

                //int attack = entity.GetIntAttr(CreatureAttributeDef.ATTACK);
                //int monsterID = entity.configId;
                //Debug.LogWarning(" monsterID = "+ monsterID+",攻击力 = "+ attack+ "释放 技能 =" + skillID);
            }
        }

        public override IBulletEnvProvider GetBulletEnvProvider()
        {
            return KingBulletEnvProvider.Instance;
        }

        public override void Reset()
        {
            m_dicTriggerCount.Clear();
            base.Reset();

            //buff的清理暂时放这里，稍后考虑清理时机点
            if (null != buffComponent)
            {
                buffComponent.Reset();
            }

        }

        public override bool IsCooling(float curTime, float lastCastTime, float cooldingTime, int skillID)
        {
            cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillID);
            if (cfg == null)
            {
                Debug.LogError("不存在的技能配置 ID=" + skillID);
                return false;
            }

            /*
            SEVENT_SKILL_COLDING context = SEVENT_SKILL_COLDING.Instance;
            context.srcID = reco.entID;
            context.lastCastTime = lastCastTime;
            context.baseCoolingTime = cfg.skillCD;
            context.addtionCoolingCoff = 0;
            context.skillID = skillID;


            GameGlobalEx.EventEgnine.FireExecute(DGlobalEventEx.EVENT_SKILL_COOLING, DEventSourceType.SOURCE_TYPE_ENTITY, 0, context);
             */

           // float finalCooldingTime = cfg.skillCD * (1 + (float)context.addtionCoolingCoff / 1000.0f);
           

            return false;
        }
    }
}

