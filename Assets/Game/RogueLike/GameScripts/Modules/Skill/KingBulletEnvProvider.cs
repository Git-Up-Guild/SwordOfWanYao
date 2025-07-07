/*******************************************************************
** 文件名:	KingBulletEnvProvider.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.08
** 版  本:	1.0
** 描  述:	
** 应  用:  王国demo子弹环境提供者

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XGame.Entity;
using XGame;
using XGame.Poolable;
using XGame.Utils;
using XClient.Common;
using XGame.FlowText;
using XClient.Network;
using static UnityEngine.EventSystems.EventTrigger;
using System.Security.Principal;
using XGame.UI;
using static XClient.Entity.BroadcastFlowText;
using GameScripts.Common;

namespace GameScripts.KingHero
{

    //技能命令现场
    public class SkillCmdContext
    {
        public string cmd;
        public List<int> param = new List<int>();
    }

    public class KingBulletEnvProvider : Singleton<KingBulletEnvProvider>, IBulletEnvProvider
    {

        //技能命令列表(减少GC)
        private Dictionary<int, List<SkillCmdContext>> m_dicSkillCmdList = new Dictionary<int, List<SkillCmdContext>>();

        //本地临时目标
        private List<IDReco> m_listTarget = new List<IDReco>();
        private List<IDReco> m_listTempTarget = new List<IDReco>();
        private List<IDReco> m_listFilter = new List<IDReco>();


        //默认参数
        private float[] m_defaultTargetParams = new float[1] { 0 };

        //本地临时阵营
        private List<ulong> m_listCamp = new List<ulong>();

        private GameObject m_entityRoot = null;


        //获取技能的命令列表
        public List<SkillCmdContext> GetSkillCmdContextList(int skillID,string commandSkillList)
        {
            List<SkillCmdContext> listContext= null;
            if(m_dicSkillCmdList.TryGetValue(skillID,out listContext)==false)
            {
                //添加新的技能项
                listContext = new List<SkillCmdContext>();
                m_dicSkillCmdList.Add(skillID,listContext); 

                string[] aryContext = commandSkillList.Split(";");
                string[] item;
                SkillCmdContext context = null;
                int nLen = aryContext.Length;   
                for(int i=0;i<nLen;++i)
                {
                    item = aryContext[i].Split("#");
                    if(item.Length<2)
                    {
                        continue;
                    }

                    context = new SkillCmdContext();
                    context.cmd = item[0];
                    int nCount = item.Length;
                    for(int j=1;j<nCount;++j)
                    {
                        context.param.Add(int.Parse(item[j]));
                    }

                    listContext.Add(context);
                }
            }

            return listContext;
        }

        public void Clear()
        {
           // broadcastFlowText = null;
        }

        public BulletFireContext BuildFireContext(ulong srcID, int bulletID, Transform root, int userData)
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            BulletFireContext bulletFireContext = itemPoolMgr.PopObjectItem<BulletFireContext>();
            bulletFireContext.src = srcID;
            bulletFireContext.target = 0;
            bulletFireContext.srcPos = root.position;
            bulletFireContext.userData = userData;
            // bulletFireContext.srcPos.y += 0.5f;

            cfg_Skill cfgSkill = GameGlobal.GameScheme.Skill_0(userData);

            if(cfgSkill!=null)
            {
                //创建一个默认参数
                if(cfgSkill.targetParams.Length==0)
                {
                    cfgSkill.targetParams = new float[1] { 0};
                }


                if (cfgSkill.castPos.Length>=3)
                {
                    bulletFireContext.srcPos = new Vector3(cfgSkill.castPos[0], cfgSkill.castPos[1], cfgSkill.castPos[2]);
                    //Debug.LogError("选取目标参数少于3个 skillID=" + userData);
                }
              

                bulletFireContext.visibleType = (CREATURE_VISIBLE_TYPE)cfgSkill.iVisibleType;
            }



            bulletFireContext.targetPos = bulletFireContext.srcPos + root.forward * 5;

           

            bulletFireContext.bulletEnvProvider = this;
           

            return bulletFireContext;
        }

        public bool CanFire(ulong srcID, int bulletID, int userData)
        {
            return true;
        }

        public Vector3 GetPos(ulong entID, BulletFireContext context, IBullet bullet)
        {
            IEntityManager manager = GameGlobal.EntityWorld.Local;
            IEntity entity = manager.GetEntity(entID);
            if (entity != null)
            {
                ICreatureEntity ce = entity as ICreatureEntity;
                if (ce != null)
                {
                    return ce.GetPos();
                }
            }
            return context.targetPos;
        }

        public bool IsNeedForwardTarget(ulong srcID, int bulletID, int userData)
        {
            return false;
            return IsNeedTarget(srcID, bulletID, userData);
        }

        public bool IsNeedTarget(ulong srcID, int bulletID, int userData)
        {
            cfg_Skill cfgSkill = GameGlobal.GameScheme.Skill_0(userData);

            return cfgSkill.isNeedCastTarget>0;
        }

        public bool OnCollisionDamage(BulletFireContext context, IBullet bullet, IDReco target)
        {
            return __OnHit(context, bullet, target);
        }

        public void OnDestroy(BulletFireContext context, IBullet bullet)
        {

        }

        public bool OnExploreDamage(BulletFireContext context, IBullet bullet, IDReco target)
        {
            return __OnHit(context, bullet, target);
        }

        public bool OnTimedDamage(BulletFireContext context, IBullet bullet, IDReco target)
        {
            return __OnHit(context, bullet, target);
        }

        public IDReco SelectTarget(ulong srcID, int bulletID, IDReco IDReco, int userData, List<IDReco> filters = null)
        {
            cfg_Bullet cfg = GameGlobal.GameScheme.Bullet_0(bulletID);
            if (cfg == null)
            {
                return null;
            }

            //从范围内选择一个
            float damangeDistance = cfg.fDamageDistance;
            cfg_Skill cfgSkill = GameGlobal.GameScheme.Skill_0(userData);
            if (cfgSkill != null)//&& cfgSkill.skillMonRange< damangeDistance)
            {
                damangeDistance = cfgSkill.skillMonRange;
            }


            List<IDReco> listReco = SelectTargets(IDReco, cfgSkill.targetRule, cfgSkill.targetParams, filters);

            //忽略死亡的
            m_listTempTarget.Clear();
            IDReco reco = null;
            IEntityManager manager = GameGlobal.EntityWorld.Local;
            int nCount = listReco.Count;
            for (int i = 0; i < nCount; ++i)
            {
                reco = listReco[i];
                IEntity entity = manager.GetEntity(reco.entID);
                if (entity != null)
                {
                    ICreatureEntity creatureTarget = entity as ICreatureEntity;
                    if (creatureTarget != null&& creatureTarget.IsDie()==false)
                    {
                        m_listTempTarget.Add(reco);
                    }
                }
            }

            //随机选一个
            if (listReco != null && m_listTempTarget.Count > 0)
            {
                int nIndx = Random.Range(0, m_listTempTarget.Count);
                return m_listTempTarget[nIndx % m_listTempTarget.Count];
            }

            return null;
        }



        


        private bool __OnHit(BulletFireContext context, IBullet bullet, IDReco target)
        {
            ulong entID = target.entID;
            if (entID != 0)
            {
                IEntityManager manager = GameGlobal.EntityWorld.Local;
                IEntity entity = manager.GetEntity(entID);
                if (entity != null)
                {
                    ICreatureEntity creatureTarget = entity as ICreatureEntity;
                    if (creatureTarget != null)
                    {
                        NetID.Temp.Set(target.entID);

                        IEntity srcEntity = manager.GetEntity(context.src);
                        ICreatureEntity creatureSrc = srcEntity as ICreatureEntity;


                        float damageRate = 1;
                        cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(context.userData);
                        if(cfg!=null)
                        {
                            //是否造成伤害
                            if (cfg.isHurt > 0)
                            {

                                /*
                                SEVENT_DAMAGE damageContext = SEVENT_DAMAGE.Instance;

                                //发送消息，出发外部处理和外部伤害减免
                                damageContext.srcID = context.src;
                                damageContext.targetID = target.entID;
                                damageContext.skillID = context.userData;

                                damageContext.attackBase = 0;
                                damageContext.powerBaseAttackRate = 0;
                                damageContext.phyBaseDefense = creatureTarget.GetIntAttr(CreatureAttributeDef.PHY_DEFENSE);
                                damageContext.powerBaseAttackCoff = 0;
                                damageContext.addtionAttackCoff = 0; //
                                damageContext.addtionPowerAttackRate = 0; //
                                damageRate = cfg.fDamageRate;
                                damageContext.addtionSecKillRate = 0;   //秒杀
                                damageContext.addtionHurtPert = 0;   //受伤增幅
                                damageContext.addtionAttackBoomCoff = 0; //攻击伤害增加  
                                */

                                int textID = 106;

                                //读取数据源
                                if (null != creatureSrc)
                                {
                                  //  damageContext.powerBaseAttackCoff = creatureSrc.GetIntAttr(CreatureAttributeDef.POWER_ATTACK_COFF);
                                  //  damageContext.powerBaseAttackRate = creatureSrc.GetIntAttr(CreatureAttributeDef.POWER_ATTACK_RATE);
                                  //  damageContext.attackBase = creatureSrc.GetIntAttr(CreatureAttributeDef.ATTACK);
                                    //damageContext.phyBaseDefense = creatureSrc.GetIntAttr(CreatureAttributeDef.PHY_DEFENSE);
                                }

                                // GameGlobalEx.EventEgnine.FireExecute(DGlobalEventEx.EVENT_DAMAGE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, damageContext);


                                //计算一下是否发生了暴击
                                /*
                                int powerHit = 0;
                                int powerHitRate = Random.Range(1, 1001);
                                if (powerHitRate < damageContext.powerBaseAttackRate*(1 + (float)damageContext.addtionPowerAttackRate/1000.0f))
                                {
                                    textID = 103;
                                    powerHit = 1;
                                }

                                //伤害=攻击力*子弹伤害系数*（1+是否暴击*暴击伤害）*（攻击加成1+攻击加成2）*伤害加成*（1-防御/（防御+常量））
                                //伤害=iAttackPower*skill.DamageRate*（1+是否暴击*暴击伤害）*（攻击加成1+攻击加成2）*1*（1-iPhyDefense/（iPhyDefense+50））


                                double damageResult = damageContext.attackBase * damageRate * (1 + powerHit * damageContext.powerBaseAttackCoff/1000.0) * (1+damageContext.addtionAttackCoff/1000.0) * 1 * (1 - (float)damageContext.phyBaseDefense / (damageContext.phyBaseDefense + 50.0f));

                                //计算秒杀概率
                                int secKillRate = Random.Range(1, 1001);
                                if(secKillRate< damageContext.addtionSecKillRate)
                                {
                                    damageResult += creatureTarget.GetHP()*2;
                                }else
                                {
                                    int exraDamageCoff = damageContext.addtionHurtPert + damageContext.addtionAttackBoomCoff;
                                    damageResult *= (1 + (float)exraDamageCoff / 1000.0f);

                                }

                                */

                                int damageResult = 100;

                                creatureTarget.SetHPDelta(-(int)damageResult);

                                //textID = 103;
                                //飘子


                               // BroadcastFlowTextData data = GameGlobalEx.Battle.SyncManager.SyncFlowTextContext(textID, creatureTarget.GetPos(), -(int)damageResult);
                               // OnFlowText(data,true);



                                // broadcastFlowText?.FlowDamageText(textID, creatureTarget.GetPos(), -(int)damageResult);

                            }

                            //添加Buff
                            AddBuff(target, context.userData, cfg.commandSkillList);

                           
                        }

                     

                        return true;
                    }
                }
            }

            return true;
        }

        //对某个对象添加buff列表
        public void AddBuff(IDReco target,int skillID,string commandSkillList)
        {
            if(target == null)
            {
                Debug.LogError("KingBulletEnvProvider.AddBuffe invoke fail. target is null.");
                return;
            }

            BuffBaseComponent buffComponent = target.transform.GetComponent<BuffBaseComponent>();
            if (null != buffComponent)
            {
                List<SkillCmdContext> ListCmdContext = KingBulletEnvProvider.Instance.GetSkillCmdContextList(skillID, commandSkillList);
                SkillCmdContext cmdContext;
                int nCount = ListCmdContext.Count;
                int nLen = 0;
                for (int i = 0; i < nCount; i++)
                {
                    cmdContext = ListCmdContext[i];
                    nLen = cmdContext.param.Count;
                    for (int j = 0; j < nLen; ++j)
                    {
                        buffComponent.AddBuff(cmdContext.param[j]);

                        //if(false)
                        //{
                        //    IEntityManager manager = XGameComs.Get<IEntityManager>();
                        //    ICreatureEntity entity = manager.GetEntity(target.entID) as ICreatureEntity;
                        //    int monsterID = entity.configId;
                        //    Debug.LogWarning(" monsterID = " + monsterID + ",被技能 "+ skillID + " 打中, 添加buff =" + cmdContext.param[j]);
                        //}
                    }
                }
            }
        }

        /*
        private FlowTextContext m_flowContext = new FlowTextContext();
        private Camera mainCamera = null;

        public void FlowDamageText(int textID,Vector3 pos, int hp)
        {
            if (null == mainCamera)
            {
                Camera[] allCameras = Camera.allCameras;
                int nLen = allCameras.Length;
                for (int i = 0; i < nLen; ++i)
                {
                    if (allCameras[i].tag == "MainCamera")
                    {
                        mainCamera = allCameras[i];
                        break;
                    }
                }

            }

            IFlowTextManager flowTextMgr = XGame.XGameComs.Get<IFlowTextManager>();
            Vector3 screenPos = mainCamera.WorldToScreenPoint(pos);
            m_flowContext.content = hp.ToString();
            m_flowContext.startPosition = flowTextMgr.ScreenPositionToLayerLocalPosition(textID, screenPos, mainCamera);
            flowTextMgr.AddFlowText(textID, m_flowContext);
        }
        */


        public bool IsExternalSupportCamp()
        {
            return true;
        }

        public List<ulong> GetExternalHitCamp(BulletFireContext context, IBullet bullet)
        {
            return GetTargetCamp(context.userData);
        }

        public bool CanHitTarget(IDReco target, BulletFireContext context, IBullet bullet)
        {
            List<ulong> listCamp = GetTargetCamp(context.userData);
            return listCamp.IndexOf(target.camp)>=0;
        }


        //选目标
        public List<IDReco> SelectTargets(IDReco src,string targetRule, float[] param, List<IDReco> filters = null)
        {
            //过滤
            if(null== filters)
            {
                m_listFilter.Clear();
                filters = m_listFilter;
            }

            //默认参数
            if(param==null||param.Length==0)
            {
                param = m_defaultTargetParams;
            }
           

            //targetRule = CommandDef.selfRoundMonster;
             //param = 100;

            m_listTarget.Clear();
            switch (targetRule)
            {
                case CommandDef.targetSelf:
                    m_listTarget.Add(src);
                    break;
                case CommandDef.targetsAllTeam:
                    {
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE);
                        __GetTargetsByCamp(camp, m_listTarget, filters); 
                    }
                    break;
                case CommandDef.selfRoundMonster:
                    {
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        Vector3 pos = src.transform.position;
                        Vector3 forward = src.transform.forward;
                        IDRecoEntityMgr.Instance.GetIDRecoByCamp(m_listTarget, camp, EntityType.monsterType, ref pos, ref forward, REGION_TYPE.REGION_SHAPE_CIRCLE, param[0], param[0]);
                        __FilterTargets(m_listTarget, filters);
                    }
                    break;
                case CommandDef.randomEnemy: //敌方棋子
                    {
                        ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE);
                        __GetTargetsByCamp(camp, m_listTarget, filters, (int)param[0]);
                    }
                    break;
                case CommandDef.randomTeam: //我方棋子
                    {
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE);
                        __GetTargetsByCamp(camp, m_listTarget, filters, (int)param[0]);
                    }
                    break;
                case CommandDef.targetsNearMonster:
                    {
                        Vector3 pos = src.transform.position;
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        IDReco target = GetNearMonster(camp, pos, filters);
                        m_listTarget.Clear();
                        if(target!=null)
                        {

                            m_listTarget.Add(target);

                        }
                    }
                    break;
                case CommandDef.targetsPointNearMonster:
                    {
                        Vector3 pos = src.transform.position;
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        IDReco target = GetNearMonster(camp, pos, filters);
                        m_listTarget.Clear();
                        if (target != null)
                        {
                            if (Vector3.Distance(pos, target.transform.position) <= param[0])
                            {
                                m_listTarget.Add(target);
                            }
                        }
                    }
                    break;
                case CommandDef.nearRoundMonster:
                    {
                        Vector3 pos = src.transform.position;
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        IDReco target = GetNearMonster(camp, pos, filters);
                        m_listTarget.Clear();
                        if (target != null)
                        {
                            pos = target.transform.position;
                            Vector3 forward = target.transform.forward;
                            IDRecoEntityMgr.Instance.GetIDRecoByCamp(m_listTarget, camp, EntityType.monsterType, ref pos, ref forward, REGION_TYPE.REGION_SHAPE_CIRCLE, param[0], param[0]);
                            __FilterTargets(m_listTarget, filters);
                        }
                    }
                    break;
                case CommandDef.targetsNearMonsterEnemy: //敌方最近的怪物
                    {
                        Vector3 pos = src.transform.position;
                        ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        IDReco target = GetNearMonster(camp, pos, filters);
                        m_listTarget.Clear();
                        if (target != null)
                        {
                            m_listTarget.Add(target);

                        }
                    }
                    break;
                case CommandDef.nearRoundMonsterEnemy:
                    {
                        Vector3 pos = src.transform.position;
                        ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        IDReco target = GetNearMonster(camp, pos, filters);
                        m_listTarget.Clear();
                        if (target != null)
                        {
                            pos = target.transform.position;
                            Vector3 forward = target.transform.forward;
                            IDRecoEntityMgr.Instance.GetIDRecoByCamp(m_listTarget, camp, EntityType.monsterType, ref pos, ref forward, REGION_TYPE.REGION_SHAPE_CIRCLE, param[0], param[0]);
                            __FilterTargets(m_listTarget, filters);
                        }
                    }
                   
                    break;
                case CommandDef.targetsAllMonster: //敌方最近的怪物
                    {
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        __GetTargetsByCamp(camp, m_listTarget, filters);
                    }
                    break;

                case CommandDef.randomAreaMonster:
                    {
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        GetRandomScreenInsideMonster(camp, param[0]);
                       
                    }
                    break;
                case CommandDef.areaPointEnemy:
                    {

                    }
                    break;
                case CommandDef.targetsAllEnemy:
                    {
                        ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE);
                        __GetTargetsByCamp(camp, m_listTarget, filters, (int)param[0]);
                    }
                    break;
                case CommandDef.targetsAllMonsterEnemy:
                    {
                        ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                        __GetTargetsByCamp(camp, m_listTarget, filters, (int)param[0]);
                    }
                    break;
                case CommandDef.selectArea:
                    {

                    }
                    break;
                case CommandDef.selectRoleCard:
                    {
                        /*
                        IEntityManager manager = XGameComs.Get<IEntityManager>();
                        ICreatureEntity entity = manager.GetEntity(SkillCardUseContext.Instance.EntityID) as ICreatureEntity;
                        if (null != entity)
                        {
                            IDReco reco = entity.GetComponent<IDReco>();
                            if(null!= reco)
                            {
                                m_listTarget.Add(reco);
                            }
                        }
                        */
                            
                    }
                    break;
                case CommandDef.targetWall:
                    {
                        ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_WALL);
                        __GetTargetsByCamp(camp, m_listTarget, filters);
                    }
                    break;
                case CommandDef.targetWallEnemy:
                    {
                        ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_WALL);
                        __GetTargetsByCamp(camp, m_listTarget, filters);
                    }
                    break;
                default:
                    break;
            }

            return m_listTarget;
        }


        //通过阵营获取目标
        private void __GetTargetsByCamp(ulong camp, List<IDReco> targets, List<IDReco> filters, int nCount =-1)
        {
            //通过阵营获取目标
            IDRecoEntityMgr.Instance.GetAllIDRecoByCamp(targets, camp, EntityType.monsterType);
            //过滤目标
            __FilterTargets(targets,filters);
            //过滤个数
            if(nCount>0)
            {
                while (m_listTarget.Count > nCount)
                {
                    int nIndex = Random.Range(0, m_listTarget.Count);
                    m_listTarget.RemoveAt(nIndex);
                }
            }
        }

        //获取最近的monster
        private IDReco GetNearMonster(ulong camp,Vector3 pos, List<IDReco> filters)
        {

            IDReco target = null;
            IDReco src = null;
            IDRecoEntityMgr.Instance.GetAllIDRecoByCamp(m_listTarget, camp, EntityType.monsterType);

            //剔除
            __FilterTargets(m_listTarget, filters);

            int nCount = m_listTarget.Count;
            float distance = 9999999;
            float curDistance = 0;
            for (int i=0;i<nCount;++i)
            {
                src = m_listTarget[i];
                curDistance = Vector3.Distance(src.transform.position, pos);
                if(curDistance< distance)
                {
                    distance = curDistance;
                    target = m_listTarget[i];
                }
            }


            return target;
        }

        private void GetRandomScreenInsideMonster(ulong camp,float r)
        {

            IDReco target = null;
            IDReco src = null;
            IDRecoEntityMgr.Instance.GetAllIDRecoByCamp(m_listTarget, camp, EntityType.monsterType);

            if(m_listTarget.Count>0)
            {
                int nIndex = Random.Range(0, m_listTarget.Count);
                target = m_listTarget[nIndex];

                Vector3 pos = target.transform.position;
                Vector3 forward = target.transform.forward;
                IDRecoEntityMgr.Instance.GetIDRecoByCamp(m_listTarget, camp, EntityType.monsterType, ref pos, ref forward, REGION_TYPE.REGION_SHAPE_CIRCLE, r, r);

            }

        }

        private List<ulong> GetTargetCamp(int skillID)
        {
            m_listCamp.Clear();
            cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillID);
            if(cfg!=null)
            {
                switch (cfg.targetRule)
                {
                    case CommandDef.targetSelf:
                        break;
                    case CommandDef.targetsAllTeam:
                    case CommandDef.randomTeam:
                    case CommandDef.selectRoleCard:
                        {
                            ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE);
                            m_listCamp.Add(camp);
                        }
                        break;
                    case CommandDef.selfRoundMonster:
                    case CommandDef.targetsNearMonster:
                    case CommandDef.targetsPointNearMonster:
                    case CommandDef.nearRoundMonster:
                    case CommandDef.randomAreaMonster:
                    case CommandDef.targetsAllMonster:
                    case CommandDef.nearRoundMonsterEnemy:
                    case CommandDef.areaPointEnemy:
                    case CommandDef.selectArea:
                        {
                            ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                            m_listCamp.Add(camp);
                        }
                        break;

                    case CommandDef.targetsAllEnemy:
                    case CommandDef.randomEnemy: //敌方棋子
                        {
                            ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE);
                            m_listCamp.Add(camp);
                        }
                        break;
                    case CommandDef.targetsNearMonsterEnemy:
                    case CommandDef.targetsAllMonsterEnemy:
                        {
                            ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER);
                            m_listCamp.Add(camp);

                        }
                        break;
                    case CommandDef.targetWall:
                        {
                            ulong camp = CampDef.GetRemoteCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_WALL);
                            m_listCamp.Add(camp);

                        }
                        break;
                    case CommandDef.targetWallEnemy:
                        {
                            ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_WALL);
                            m_listCamp.Add(camp);

                        }
                        break;
                    default:
                        break;
                }
            }

            return m_listCamp;


        }

        private void __FilterTargets(List<IDReco> targets,List<IDReco> filters)
        {
            //判空
            if(null== filters|| filters.Count==0)
            {
                return;
            }

            //删除过滤对象
            int nCount = filters.Count;
            for(int i=0;i<nCount;++i)
            {
                targets.Remove(filters[i]); 
            }
        }



        //飘子现场
        static private FlowTextContext m_flowContext = new FlowTextContext();
        static private Camera mainCamera = null;

        //飘字
        public void OnFlowText(BroadcastFlowTextData data,bool isMySelf)
        {
            if (null == mainCamera)
            {
                Camera[] allCameras = Camera.allCameras;
                int nLen = allCameras.Length;
                for (int i = 0; i < nLen; ++i)
                {
                    if (allCameras[i].tag == "MainCamera")
                    {
                        mainCamera = allCameras[i];
                        break;
                    }
                }

            }

           // int textID = data.textID.Value;
           // int hp = data.hp.Value;
           // Vector3 pos = data.pos.Value;

            //远程过来的对象
            if(isMySelf==false)
            {
               // pos = MonsterSystem.Instance.WorldPositionToBattlePosition(pos, false);
            }

            IFlowTextManager flowTextMgr = XGame.XGameComs.Get<IFlowTextManager>();
          //  Vector3 screenPos = mainCamera.WorldToScreenPoint(pos);
           // m_flowContext.content = hp.ToString();
           // m_flowContext.startPosition = flowTextMgr.ScreenPositionToLayerLocalPosition(textID, screenPos, mainCamera);
          //  flowTextMgr.AddFlowText(textID, m_flowContext);
        }

        public EffectContext GetBuffEffectContext(string szBuffEffect)
        {
            throw new System.NotImplementedException();
        }

        public Transform GetEntityRoot()
        {
            if(null== m_entityRoot)
            {
                m_entityRoot = new GameObject("EntityRoot");
                GameObject.DontDestroyOnLoad(m_entityRoot); 
            }

            return m_entityRoot.transform;
        }

        /*
        private void __InitBroadcastFlowText()
        {
            ulong camp = CampDef.GetLocalCamp(BATTLE_CAMP_DEF.BATTLE_CAMP_WALL);
            IDRecoEntityMgr.Instance.GetAllIDRecoByCamp(m_listTarget, camp, EntityType.monsterType);
            if(m_listTarget.Count>0)
            {
                IDReco reco = m_listTarget[0];  
                broadcastFlowText = reco.GetComponent<BroadcastFlowText>();
                if(null== broadcastFlowText)
                {
                    broadcastFlowText = reco.gameObject.AddComponent<BroadcastFlowText>();
                    INetObject netObject = broadcastFlowText.GetNetObject();
                    netObject.SetupNetID(GameGlobal.Role.entityIDGenerator.Next());
                    netObject.Start();
                }
            }
        }
        */

    }

}
