using System.Collections.Generic;
using UnityEngine;


namespace XClient.Entity
{
    public class BulletHitMgr : ICollisionSink
    {
        //拥有的者
        private IBullet m_masterBullet;

        //子弹现场
        private BulletFireContext m_bulletFireContext;

        //碰撞脚本
        private CollisionItem m_collisionItem;

        //敌方列表
        private List<ulong> m_listEnemyCamps = new List<ulong>();

        //子弹配置
        private cfg_Bullet m_bulletCfg;

        //是否区域伤害子弹
        private bool m_bRectDamage;

        //是否已经检测过延迟时间
        private bool m_bHadCheckDelay;

        //上次检测时间
        private float m_lastCheckTime;

        //弹射次数
        private int m_nJumpCount = 0;

        //伤害时间间隔
        private Dictionary<ulong, float> m_dicDamageTime = new Dictionary<ulong, float>();

        //弹射列表
        private List<IDReco> m_listJumpCountReco = new List<IDReco>();

        //获取目标的临时变量
        static private List<IDReco> m_listExploreReco = new List<IDReco>();
        static private List<IDReco> m_listCheckDamageReco = new List<IDReco>();


        //创建
        public bool Create(IBullet masterBullet, BulletFireContext bulletFireContext)
        {
            m_masterBullet = masterBullet;
            m_bulletFireContext = bulletFireContext;
            m_bulletCfg = m_masterBullet.config as cfg_Bullet;
            m_dicDamageTime.Clear();
            m_listJumpCountReco.Clear();

            //m_nJumpCount = m_bulletCfg.iJumCount;
            //m_bRectDamage = m_bulletCfg.iDamageRegionType != REGION_TYPE.REGION_SHAPE_SINGLE;

            return true;
        }

        //重置
        public void Reset()
        {
            m_listEnemyCamps.Clear();
            m_masterBullet = null;
            m_bulletCfg = null;
            if (m_collisionItem != null)
            {
                m_collisionItem.collisionSink = null;
                m_collisionItem = null;
            }

            m_listJumpCountReco.Clear();
            m_dicDamageTime.Clear();
        }

        //销毁
        public void Release()
        {
            Reset();
            m_dicDamageTime = null;
        }

        //资源加载完成
        public void OnResLoaded(GameObject gameObject)
        {
            //获取碰撞脚本
            //if (m_bulletCfg.iType == BulletType.NoShowBullet || m_bulletCfg.iType == BulletType.NoTrajectoryBullet)
            {
                m_collisionItem = gameObject.GetComponent<CollisionItem>();
                if (m_collisionItem != null)
                {
                    m_collisionItem.collisionSink = this;
                }

            }
        }

        //推动更新
        public void OnUpdate()
        {
            
            //是不区域范围伤害不用检测
            if (m_bRectDamage == false)
            {
                return;
            }

            float curTime = Time.realtimeSinceStartup;

            //是否已经检测
            if (m_bHadCheckDelay == false)
            {
                if (curTime - m_lastCheckTime < m_bulletCfg.fDamageDelay)
                {
                    return;
                }

                m_bHadCheckDelay = true;
            }

            //间隔伤害
            if (curTime - m_lastCheckTime > m_bulletCfg.fDamageInterval)
            {
                OnCheckDamage();
                m_lastCheckTime = curTime;
            }
        }

        //碰撞到对象
        public void OnCollision(IDReco reco)
        {
            //现场非法了
            if (m_bulletFireContext == null || m_collisionItem == null ||
                m_bulletFireContext.bulletEnvProvider == null || reco.beAttack == false)
            {
                return;
            }

            int bulletTyp = m_bulletCfg.iType;

            ulong bulletCamp = m_masterBullet.GetCamp();
            ulong camp = reco.camp;
            if (bulletTyp == BulletType.NoShowBullet || bulletTyp == BulletType.TrajectoryBullet)
            {
                bool bHitDamage = false;

                if (m_bulletFireContext.bulletEnvProvider.IsExternalSupportCamp())
                {
                    bHitDamage = m_bulletFireContext.bulletEnvProvider.CanHitTarget(reco, m_bulletFireContext, m_masterBullet);
                }
                else
                {

                    //获取打击的阵营
                    if (m_listEnemyCamps.Count == 0)
                    {
                        List<ulong> listEnemyCamps = IDRecoEntityMgr.Instance.GetEnemyCamp(bulletCamp, m_bulletFireContext.listFriendCamps, m_bulletFireContext.listFriendCamps);
                        m_listEnemyCamps.AddRange(listEnemyCamps);
                    }

                    //有配置敌方阵营的
                    if (m_listEnemyCamps.IndexOf(camp) >= 0)
                    {
                        bHitDamage = true;
                    }
                }

                if (bHitDamage)
                {
                    __OnHitReco(reco);
                }
            }
        }


        //定时检测
        public void OnCheckDamage()
        {
            
            int nRegionType = m_bulletCfg.iDamageRegionType;

            List<ulong> listEnemyCamps = null;

            //是否由应用提供目标
            if (m_bulletFireContext.bulletEnvProvider.IsExternalSupportCamp())
            {
                listEnemyCamps =
                    m_bulletFireContext.bulletEnvProvider.GetExternalHitCamp(m_bulletFireContext, m_masterBullet);
            }
            else
            {
                //配置特定的敌方阵营列表
                listEnemyCamps = __GetEnemyCamps();
            }

            Vector3 pos = m_masterBullet.GetPos();
            Vector3 forward = m_masterBullet.GetForward();

            //获取简单目标对象
            IDRecoEntityMgr.Instance.GetIDRecoByCamp(m_listCheckDamageReco, listEnemyCamps, -1, ref pos, ref forward,
                nRegionType, m_bulletCfg.fDamageDistance, m_bulletCfg.fAngeAndYdis);


            //处理伤害目标
            int nCount = m_listCheckDamageReco.Count;
            IDReco reco = null;
            for (int i = 0; i < nCount; ++i)
            {
                reco = m_listCheckDamageReco[i];
                __OnHitReco(reco);
            }
        }

        //子弹消失时候爆炸处理
        public void OnExplosion()
        {
            if (m_bulletCfg != null && m_bulletCfg.iBoomRadus > 0)
            {
                //播放爆炸特效
                __PlayOnHitEffect();

                List<ulong> listEnemyCamps = null;

                //获取伤害对象
                if (m_bulletFireContext.bulletEnvProvider.IsExternalSupportCamp())
                {
                    listEnemyCamps =
                        m_bulletFireContext.bulletEnvProvider.GetExternalHitCamp(m_bulletFireContext, m_masterBullet);
                }
                else
                {
                    listEnemyCamps = __GetEnemyCamps();
                }

                Vector3 pos = m_masterBullet.GetPos();
                Vector3 forward = m_masterBullet.GetForward();
                IDRecoEntityMgr.Instance.GetIDRecoByCamp(m_listExploreReco, listEnemyCamps, -1, ref pos, ref forward,
                    REGION_TYPE.REGION_SHAPE_CIRCLE, m_bulletCfg.iBoomRadus, m_bulletCfg.fAngeAndYdis);

                int nCount = m_listExploreReco.Count;
                IDReco reco = null;
                for (int i = 0; i < nCount; ++i)
                {
                    reco = m_listExploreReco[i];
                    __OnDamgeReco(reco);
                }
            }
        }

        //击中某个实体
        private void __OnHitReco(IDReco reco)
        {
            //伤害
            __OnDamgeReco(reco);

            //子弹是不是没血了
            int hp = m_masterBullet.GetHP();

            //范围子弹不伤子弹的血
            if (m_bRectDamage == false)
            {
                m_masterBullet.SetHPDelta(-1);
            }

            if (m_masterBullet.IsDie())
            {
                //子弹死亡前先爆炸
                OnExplosion();

                //子弹销毁让外部销毁，避免时序问题
                //BulletSystem.Instance.DestroyBullet(m_masterBullet.id);
            }
            else
            {
                //需要弹射的，重新选择目标
                if (--m_nJumpCount > 0)
                {
                    if (m_listJumpCountReco.IndexOf(reco) == -1)
                    {
                        m_listJumpCountReco.Add(reco);
                    }

                    //弹射失败,子弹死亡
                    if (m_masterBullet.Jump2NextTarget(m_listJumpCountReco) == false)
                    {
                        m_masterBullet.SetHPDelta(-hp);
                        //子弹死亡前先爆炸
                        OnExplosion();
                        return;
                    }

                    __PlayOnHitEffect();

                    //重新选一个目标，飞过去
                    //IDReco target = m_bulletFireContext.bulletEnvProvider.SelectTarget(m_bulletFireContext.src, m_masterBullet.configId,null, m_bulletFireContext.userData);
                }
            }
        }

        private bool __OnDamgeReco(IDReco reco)
        {
      
            float curTime = Time.realtimeSinceStartup;
            float lastDamageTime = -1000;
            if (m_dicDamageTime.TryGetValue(reco.entID, out lastDamageTime) == false)
            {
                lastDamageTime = -1000;
                m_dicDamageTime.Add(reco.entID, curTime);
            }

            //不满足伤害间隔的，每次都伤害
            if (curTime - lastDamageTime > m_bulletCfg.fDamageInterval)
            {
                m_dicDamageTime[reco.entID] = curTime;
                //计算伤害
                m_bulletFireContext.bulletEnvProvider.OnCollisionDamage(m_bulletFireContext, m_masterBullet, reco);
                return true;
            }

            return false;
        }

        private void __PlayOnHitEffect()
        {
            //受击音效
            NetEntityEffectSyncManager.Instance.PlayAudio(NetEntityEffectSyncManager.Instance.defaultHitAudioID);

            cfg_Bullet cfg = m_masterBullet.config as cfg_Bullet;
            if (string.IsNullOrEmpty(cfg.szHitEffect))
            {
                return;
            }

            NetEffectPlayer netEP = m_masterBullet.GetComponent<NetEffectPlayer>();

            if (null != netEP)
            {
                //获取爆炸点
                Vector3 pos = m_masterBullet.GetPos();
                netEP.PlayEffect(cfg.szHitEffect, ref pos, 3, 0);
            }

            

        }


        //获取地方阵营
        private List<ulong> __GetEnemyCamps()
        {
            ulong camp = m_masterBullet.GetCamp();
            //if (m_listEnemyCamps.Count == 0)
            {
                List<ulong> listEnemyCamps = IDRecoEntityMgr.Instance.GetEnemyCamp(camp,
                    m_bulletFireContext.listFriendCamps, m_bulletFireContext.listEnemyCamps);
                m_listEnemyCamps.Clear();
                m_listEnemyCamps.AddRange(listEnemyCamps);
            }

            return m_listEnemyCamps;
        }
    }
}