/*******************************************************************
** 文件名:	Bullet.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  子弹基础类的实现

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity.Net;
using XClient.LightEffect;
using XClient.Network;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.Poolable;

namespace XClient.Entity
{
    public class Bullet : VisibleEntity, IBullet
    {
        public static readonly Vector3 s_vLeftScale = Vector3.one;

        public static readonly Vector3 s_vRightScale = new Vector3(-1, 1, 1);
        //是否播放远程特效
        //static bool playRemoteEffect = false;

        //资源路径
        private string m_resPath;

        //移动速度
        private float m_speed;

        //子弹现场
        private BulletFireContext m_bulletFireContext;

        //子弹创建时间
        private float m_createTime = 0;

        //处理碰撞伤害管理器
        BulletHitMgr m_BulletHitMgr = new BulletHitMgr();

        //子弹类型
        private int m_bulletType = 0;

        //移动方式组件
        private MonoBehaviour m_movement;

        //需要同步的数据
        private BulletDataPart m_bulletData;

        //是否本地对象
        private bool m_bLocalObject = false;

        //预制体部件
        private PrefabPart m_prefabPart;

        private Transform m_parent;
        private bool m_bFaceLeft = false;
        private Quaternion m_rotate = Quaternion.identity;

        //子弹枪口特效
        private uint m_nLaunchEffectHandle = 0;

        //子弹挂接句柄
        private uint m_nContinueEffectHandle = 0;

        //是否永生子弹
        private bool m_bAliveForerver = false;

        //是否延迟初始化
        private bool m_bDelayInit = false;

        public override string GetResPath()
        {
            return m_resPath;
        }

        protected override void OnInit(object context)
        {
            //初始化配置表格相关
            cfg_Bullet cfg = GameGlobal.GameScheme.Bullet_0(configId);
            config = cfg;
            m_resPath = cfg.szResPath;
            m_speed = cfg.fSpeed;
            m_bulletType = cfg.iType;

            base.OnInit(context);
        }

        protected override void OnBuildComplete(EntityBuildShareContext buildContext)
        {
            base.OnBuildComplete(buildContext);

            //数据原型
            m_bulletData = GetPart<BulletDataPart>();
            m_prefabPart = GetPart<PrefabPart>();
            //m_prefabPart.m_doAction = false;

            cfg_Bullet cfg = config as cfg_Bullet;
            //本地创建的初始化赋值
            NetEntityShareInitContext ctx = ((NetEntityShareInitContext)buildContext.context);
            m_bLocalObject = (ctx.isInitFromNet == false);
            if (!ctx.isInitFromNet)
            {
                m_bulletFireContext = ctx.localInitContext as BulletFireContext;
                m_createTime = Time.realtimeSinceStartup;
                m_BulletHitMgr.Create(this, m_bulletFireContext);


                //无轨道子弹不会飞行，有目标的情况直接在目标点中释放
                if (m_bulletType != BulletType.TrajectoryBullet && m_bulletFireContext.target > 0)
                {
                    m_bulletFireContext.srcPos = m_bulletFireContext.targetPos;
                    m_bulletFireContext.srcLocalPos = m_bulletFireContext.targetLocalPos;
                }

                IBulletEnvProvider bulletEnvProvider = EnvProviderMgr.Instance.GetBulletEnvProvider();
                m_parent = bulletEnvProvider.GetEntityRoot();
                // 赋值到 part
                m_bulletData.m_camp.Value = (long)m_bulletFireContext.camp;
                m_bulletData.m_hp.Value = 1;
                //m_bulletData.m_hp.Value = cfg.iJumCount == 0 ? 1 : cfg.iJumCount;

                m_bulletData.m_pos.Value = m_bulletFireContext.srcPos;
                m_bulletData.m_forward.Value = new Vector3(0, 0, 1);
                m_bulletData.m_netLauncherObjectID.Value = (long)m_bulletFireContext.netLauncherObjectID;
                m_bulletData.m_visibleType.Value = (int)m_bulletFireContext.visibleType;

                m_bDelayInit = m_bulletFireContext.delayduration > 0;
            }

            m_bAliveForerver = false;
            //m_bAliveForerver = (cfg.iJumCount < 0);

            //设置初始化位置
            // SetPos(m_bulletData.m_pos.Value);
            // SetForward(m_bulletData.m_forward.Value);
        }

        protected override void OnReset()
        {
            if (null != m_bulletFireContext)
            {
                IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
                itemPoolMgr.PushObjectItem(m_bulletFireContext);
                m_bulletFireContext = null;
            }

            m_BulletHitMgr.Reset();

            if (m_movement != null)
            {
                m_movement.enabled = false;
                m_movement = null;
            }

            if (m_nLaunchEffectHandle > 0)
            {
                EffectMgr.Instance().StopEffect(m_nLaunchEffectHandle);
                m_nLaunchEffectHandle = 0;
            }

            if (m_nContinueEffectHandle > 0)
            {
                EffectMgr.Instance().StopEffect(m_nContinueEffectHandle);
                m_nContinueEffectHandle = 0;
            }
        }


        public ulong GetCamp()
        {
            return (ulong)m_bulletData.m_camp.Value;
        }

        public Vector3 GetForward()
        {
            if (m_prefabPart.transform == null)
            {
                return m_bulletData.m_forward.Value;
            }

            return m_prefabPart.transform.forward;
        }

        public Vector3 GetPos()
        {
            if (m_prefabPart.transform == null)
            {
                return m_bulletData.m_pos.Value;
            }

            return m_prefabPart.transform.position;
        }

        public Vector3 GetLocalPos()
        {
            if (m_prefabPart.transform == null)
            {
                return m_bulletData.m_localPos.Value;
            }

            return m_prefabPart.transform.localPosition;
        }

        public void SetFace(bool faceLeft)
        {
            if (m_prefabPart.transform == null)
            {
                m_bFaceLeft = faceLeft;
                return;
            }

            m_prefabPart.transform.localScale = m_bFaceLeft ? s_vLeftScale : s_vRightScale;
        }

        public void SetRotation(Quaternion rotate)
        {
            if (m_prefabPart.transform == null)
            {
                m_rotate = rotate;
                return;
            }

            m_prefabPart.transform.rotation = rotate;
        }

        public void SetParent(Transform parent)
        {
            if (m_prefabPart.transform == null)
            {
                m_parent = parent;
                return;
            }

            m_prefabPart.transform.BetterSetParent(parent);
        }

        public void SetForward(ref Vector3 forward)
        {
            if (m_prefabPart.transform == null)
            {
                m_bulletData.m_forward.Value = forward;
                return;
            }


            m_prefabPart.transform.forward = forward;
        }


        public void SetPos(ref Vector3 pos)
        {
            if (m_prefabPart.transform == null)
            {
                m_bulletData.m_pos.Value = pos;
                m_bulletData.m_isLocalPos.Value = false;
                return;
            }

            m_prefabPart.transform.position = pos;
        }

        public void SetLocalPos(ref Vector3 pos)
        {
            if (m_prefabPart.transform == null)
            {
                m_bulletData.m_localPos.Value = pos;
                m_bulletData.m_isLocalPos.Value = true;
                return;
            }

            m_prefabPart.transform.localPosition = pos;
        }

        public bool IsDie()
        {
            if (m_bAliveForerver)
            {
                return false;
            }

            return m_bulletData.m_hp.Value <= 0;
        }

        public int GetHP()
        {
            return (int)m_bulletData.m_hp.Value;
        }

        public int GetMaxHP()
        {
            return (int)m_bulletData.m_maxHp.Value;
        }

        public void SetMaxHP(int maxHp)
        {
            m_bulletData.m_maxHp.Value = maxHp;
        }

        public void SetHPDelta(int hp)
        {
            m_bulletData.m_hp.Value += hp;
            //m_bulletData.m_hp.RemoteValueDelta += hp;
        }

        public float GetSpeed()
        {
            return m_speed;
        }

        public void SetSpeed(float speed)
        {
            m_speed = speed;
        }


        public override T GetComponent<T>() where T : class
        {
            if (m_prefabPart != null && m_prefabPart.gameObject != null)
            {
                return m_prefabPart.gameObject.GetComponent<T>();
            }

            return null;
        }

        public void GetComponents<T>(List<T> list) where T : class
        {
            if (m_prefabPart != null && m_prefabPart.gameObject != null)
            {
                m_prefabPart.gameObject.GetComponents<T>(list);
            }

            list.Clear();
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            if (id == EntityMessageID.ResLoaded)
            {
                if (m_bDelayInit)
                {
                    m_prefabPart.gameObject.BetterSetActive(false);
                }

                //初始化
                __InitView();
            }
        }

        public override void OnUpdate()
        {
            //需要延迟初始化
            if (m_bDelayInit)
            {
                float curTime = Time.realtimeSinceStartup;
                if (curTime - m_createTime >= m_bulletFireContext.delayduration)
                {
                    m_bDelayInit = false;
                    if (m_prefabPart.transform != null)
                    {
                        __InitView();
                    }
                }
            }

            //超过存活时间了,销毁对象
            if (isAlive() == false)
            {
                //时间到了，要死亡率，出发爆炸
                if (m_BulletHitMgr != null)
                {
                    m_BulletHitMgr.OnExplosion();
                }

                BulletSystem.Instance.DestroyBullet(this.id);
                return;
            }

            //范围型伤害计算
            if (m_BulletHitMgr != null)
            {
                m_BulletHitMgr.OnUpdate();
            }


            base.OnUpdatePart();
        }


        //是否还存活
        public bool isAlive()
        {
           // return true;

            float curTime = Time.realtimeSinceStartup;
            cfg_Bullet cfg = config as cfg_Bullet;
            //float finalTime = cfg.iBulletTime * 0.001f * GameGlobal.invTimeScale;
            float finalTime = cfg.fMaxSurvivalTime + m_bulletFireContext.delayduration;
            if (curTime - m_createTime >= finalTime || IsDie())
            {
                return false;
            }


            switch (m_bulletType)
            {
                //前向运动子弹
                case BulletType.TrajectoryBullet:
                {
                    ForwardMovement fm = m_movement as ForwardMovement;
                    if (fm != null && fm.IsMoving() == false)
                    {
                        return false;
                    }
                }
                    break;
                /*case BulletType.TrackBullet:
                {
                    TrackMovement tm = m_movement as TrackMovement;
                    if (tm != null && tm.IsMoving() == false)
                    {
                        return false;
                    }
                }
                    break;
                case BulletType.SurroundBullet:
                    break;*/
                default:
                    break;
            }

            return true;
        }

        //初始化移动方式
        private void InitMoveBehaviour()
        {
            m_movement = null;
            //启用运动方式
            switch (m_bulletType)
            {
                //前向运动子弹
                case BulletType.TrajectoryBullet:
                {
                    ForwardMovement fm = m_prefabPart.gameObject.GetComponent<ForwardMovement>();
                    m_movement = fm;
                }
                    break;
                /*case BulletType.TrackBullet:
                {
                    TrackMovement tm = m_prefabPart.gameObject.GetComponent<TrackMovement>();
                    m_movement = tm;
                }
                    break;
                case BulletType.SurroundBullet:
                {
                    m_movement = m_prefabPart.gameObject.GetComponent<CircleMovement>();
                }
                    break;*/
                default:
                    break;
            }

            //启动运动脚本
            if (m_movement)
            {
                m_movement.enabled = true;
            }
        }

        public int GetIntAttr(int propID)
        {
            return 0;
        }

        public void SetIntAttr(int propID, int val)
        {
        }

        public void SetATTRModifier(IATTRModifier modifier)
        {
        }


        //弹射到另外一个目标
        public bool Jump2NextTarget(List<IDReco> filters)
        {
            IDReco reco = m_prefabPart.gameObject.GetComponent<IDReco>();
            IDReco target = m_bulletFireContext.bulletEnvProvider.SelectTarget(m_bulletFireContext.src, this.configId,
                reco, m_bulletFireContext.userData, filters);
            if (null != target)
            {
                IBulletEnvProvider bulletEnvProvider = EnvProviderMgr.Instance.GetBulletEnvProvider();
                var bulletRoot = bulletEnvProvider.GetEntityRoot();
                m_bulletFireContext.target = target.entID;
                m_bulletFireContext.targetPos = target.transform.position;
                m_bulletFireContext.targetLocalPos = bulletRoot.InverseTransformPoint(m_bulletFireContext.targetPos);

                Vector3 pos = GetPos();
                m_bulletFireContext.srcPos = pos;
                m_bulletFireContext.srcLocalPos = bulletRoot.InverseTransformPoint(pos);
                Vector3 forward = m_bulletFireContext.targetLocalPos - m_bulletFireContext.srcLocalPos;
                forward.y = 0;
                forward = forward.normalized;
                cfg_Bullet cfg = this.config as cfg_Bullet;
                //开始运动
                __StartMove(cfg, forward);

                return true;
            }

            return false;
        }

        //初始化子弹
        private void __InitView()
        {
            m_prefabPart.gameObject.BetterSetActive(true);


            cfg_Bullet cfg = config as cfg_Bullet;
            Vector3 pos = m_bulletData.m_pos.Value;


            //转换战场坐标
            if (m_bLocalObject == false)
            {
                IATTRModifier attrModifier = MonsterSystem.Instance.GetAttrModified();
                pos = attrModifier.WorldPositionToBattlePosition(m_bulletData.m_pos.Value, false);
            }

            SetParent(m_parent);

            Vector3 forward = m_bulletData.m_forward.Value;
            SetFace(true);
            SetPos(ref pos);
            SetForward(ref forward);
            SetRotation(Quaternion.identity);


            //设置标识数据
            IDReco reco = m_prefabPart.gameObject.GetComponent<IDReco>();
            if (null == reco)
            {
                reco = m_prefabPart.gameObject.AddComponent<IDReco>();
            }

            reco.camp = (ulong)m_bulletData.m_camp.Value;
            reco.entID = base.id;
            reco.entType = base.type;
            reco.beAttack = false;


            //设置缩放比例
            m_prefabPart.gameObject.transform.localScale = new Vector3(1, 1, 1);


            //发射器根部
            Transform launchRoot = null;
            NetObject netObject = NetObjectManager.Instance.GetObject((ulong)m_bulletData.m_netLauncherObjectID.Value);
            if (null != netObject)
            {
                MonoNetObject mo = netObject as MonoNetObject;
                if (mo != null)
                {
                    launchRoot = mo.Mono.transform;
                }
            }

            //追随发射器
            //if (cfg.iFollowLuancher > 0)
            //{
            //    m_prefabPart.gameObject.transform.BetterSetParent(launchRoot);
            //}


            //不是本地创建的
            if (m_bLocalObject)
            {
                reco.listFriendCamps = m_bulletFireContext.listFriendCamps;
                reco.listEnemyCamps = m_bulletFireContext.listEnemyCamps;
                reco.canAttack = true;

                //通知伤害管理器加载完成
                m_BulletHitMgr.OnResLoaded(m_prefabPart.gameObject);


                //设置位置
                SetPos(ref m_bulletFireContext.srcPos);
                m_prefabPart.gameObject.transform.position = m_bulletFireContext.srcPos;

                forward = new Vector3(0, 0, 1);
                //设置朝向
                Vector3 targetPos = m_bulletFireContext.targetPos;
                targetPos.y = m_bulletFireContext.srcPos.y;
                forward = (m_bulletFireContext.targetPos - m_bulletFireContext.srcPos).normalized;
                SetForward(ref forward);
                //if (cfg.forwardType == BulletForwardType.forwardTarget)
                //{
                //    Vector3 targetPos = m_bulletFireContext.targetPos;
                //    targetPos.y = m_bulletFireContext.srcPos.y;
                //    forward = (m_bulletFireContext.targetPos - m_bulletFireContext.srcPos).normalized;
                //    SetForward(ref forward);
                //}
                //else
                //{
                //    //有父亲的，就设置父亲的朝向
                //    if (launchRoot)
                //    {
                //        forward = launchRoot.forward;
                //        SetForward(ref forward);
                //    }
                //}

                //初始化移动方式
                InitMoveBehaviour();

                //开始运动
                __StartMove(cfg, forward);
            }
            else
            {
                //远程创建的子弹不能主动攻击
                reco.canAttack = false;
                InitMoveBehaviour();
            }

            //持续特效
            if (m_bulletData.m_visibleType.Value == (int)(CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_ALL) ||
                m_bLocalObject && m_bulletData.m_visibleType.Value == (int)(CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_LOCAL) ||
                m_bLocalObject == false &&
                m_bulletData.m_visibleType.Value == (int)(CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_REMOTE))
            {
                //if (string.IsNullOrEmpty(cfg.szLaunchEffect) == false)
                //{
                //    //持续特效
                //    m_nLaunchEffectHandle = EffectMgr.Instance()
                //        .PlayEffect(cfg.szLaunchEffect, ref pos, cfg.fLaunchEffectTime, launchRoot);
                //}
//
                //if (string.IsNullOrEmpty(cfg.szContinuousEffect) == false)
                //{
                //    //持续特效
                //    m_nContinueEffectHandle = EffectMgr.Instance().PlayEffect(cfg.szContinuousEffect, ref pos,
                //        cfg.fMaxSurvivalTime > 0 ? cfg.fMaxSurvivalTime : 20, m_prefabPart.gameObject.transform);
                //}
            }
        }

        //开始移动
        private void __StartMove(cfg_Bullet cfg, Vector3 forward)
        {
            //启用运动方式
            switch (m_bulletType)
            {
                //前向运动子弹
                case BulletType.TrajectoryBullet:
                {
                        Vector3 targetPos;
                        if (cfg.fMaxSurvivalDistance > 0)
                        {
                            targetPos = m_bulletFireContext.srcPos + forward * cfg.fMaxSurvivalDistance;
                        }
                        else
                        {
                            targetPos = m_bulletFireContext.srcPos + forward * cfg.fMaxSurvivalTime * cfg.fSpeed;
                        }

                        ForwardMovement fm = m_movement as ForwardMovement;
                        if (fm != null)
                        {
                            m_movement.enabled = true;
                            fm.StartMove(cfg.fSpeed, m_bulletFireContext.srcPos, targetPos);
                        }
                    }
                    break;
                /*case BulletType.TrackBullet:
                {
                    TrackMovement tm = m_movement as TrackMovement;
                    if (tm)
                    {
                        //cfg.param[0] 填偏移高度
                        tm.StartTrack(m_bulletFireContext.target, cfg.fMaxSurvivalTime, cfg.fSpeed, cfg.param[0]);
                    }
                }
                    break;

                case BulletType.SurroundBullet:
                {
                    CircleMovement circleMovement = m_movement as CircleMovement;
                    //m_movement = m_prefabPart.gameObject.GetComponent<CircleMovement>();
                }
                    break;*/
                default:
                    break;
            }
        }

        //获取显示类型
        public CREATURE_VISIBLE_TYPE GetVisibleType()
        {
            return (CREATURE_VISIBLE_TYPE)m_bulletData.m_visibleType.Value;
        }
    }
}