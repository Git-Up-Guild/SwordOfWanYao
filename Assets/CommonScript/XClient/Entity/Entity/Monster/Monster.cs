/*******************************************************************
** 文件名:	Monster.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物实体类

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using XClient.Common;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    public class Monster : VisibleEntity, IMonster
    {
        public static readonly Vector3 s_vLeftScale = new Vector3(-1, 1, 1);

        public static readonly Vector3 s_vRightScale = Vector3.one;

        //资源路径
        private string m_resPath;

        //移动速度
        private float m_speed;

        public int ceatureId;
        //友方列表
        private List<ulong> m_listFriendCamps;

        //敌方列表
        private List<ulong> m_listEnemyCamps;

        //是否本地对象
        private bool m_bLocalObject = false;
        private bool m_bFaceLeft = false;
        private Quaternion m_rotate = Quaternion.identity;
        private Transform m_parent;

        //怪物数据同步部件
        private MonsterDataPart m_dataPart;

        //预制体部件
        private PrefabPart m_prefabPart;

        //预制体部件
       // private SpineSkinPart m_skinPart;

        //属性修改器
        private IATTRModifier m_attrModifier;

        //int属性列表
        private Dictionary<int, int> m_dicProp = new Dictionary<int, int>();

        public override Vector3 position => GetPos();

        public ulong GetCamp()
        {
            return (ulong)m_dataPart.m_camp.Value;
        }

        public Vector3 GetForward()
        {
            if (m_prefabPart.transform == null)
            {
                return m_dataPart.m_forward.Value;
            }

            return m_prefabPart.transform.forward;
        }

        public Vector3 GetPos()
        {
            if (m_prefabPart.transform == null)
            {
                return m_dataPart.m_pos.Value;
            }

            return m_prefabPart.transform.position;
        }

        public Vector3 GetLocalPos()
        {
            if (m_prefabPart.transform == null)
            {
                return m_dataPart.m_localPos.Value;
            }

            return m_prefabPart.transform.localPosition;
        }

        public bool GetFaceLeft()
        {
            return m_bFaceLeft;
        }

        public void SetForward(ref Vector3 forward)
        {
            if (m_prefabPart.transform == null)
            {
                m_dataPart.m_forward.Value = forward;
                return;
            }

            m_prefabPart.transform.forward = forward;
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

        public void SetPos(ref Vector3 pos)
        {
            if (m_prefabPart.transform == null)
            {
                m_dataPart.m_pos.Value = pos;
                m_dataPart.m_isLocalPos.Value = false;
                return;
            }

            m_prefabPart.transform.position = pos;
        }

        public void SetLocalPos(ref Vector3 localPos)
        {
            if (m_prefabPart.transform == null)
            {
                m_dataPart.m_localPos.Value = localPos;
                m_dataPart.m_isLocalPos.Value = true;
                return;
            }

            m_prefabPart.transform.localPosition = localPos;
        }


        public override string GetResPath()
        {
            return m_resPath;
        }

        protected override void OnInit(object context)
        {
            cfg_Monster cfg = GameGlobal.GameScheme.Monster_0((uint)configId);

            if (cfg == null)
            {
                Debug.LogError("不存在的CreatureView配置 configId=" + configId);
            }
            else
            {
                config = cfg;
                var ctx = context as NetEntityShareInitContext;
                CreateMonsterContext createMonsterContext = (CreateMonsterContext)(ctx.localInitContext);
                if (!string.IsNullOrEmpty(createMonsterContext.resPath))
                {
                    m_resPath = createMonsterContext.resPath;
                }
                else
                {
                    m_resPath = cfg.szResPath;
                }
                ceatureId = createMonsterContext.creatureID;


                if(string.IsNullOrEmpty(m_resPath))
                {
                    Debug.LogError("怪物的资源路径为空 configId=" + configId);
                }


                m_speed = 0;
            }

            //获取修改器
            m_attrModifier = MonsterSystem.Instance.GetAttrModified();

            SetSpeed((int)cfg.fbaseSpeed);

            base.OnInit(context);
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);

            //数据原型
            m_dataPart = GetPart<MonsterDataPart>();
            m_prefabPart = GetPart<PrefabPart>();
           // m_skinPart = GetPart<SpineSkinPart>();
            var ctx = context as NetEntityShareInitContext;
            m_bLocalObject = (ctx.isInitFromNet == false);
            if (m_bLocalObject)
            {
                CreateMonsterContext createMonsterContext = (CreateMonsterContext)(ctx.localInitContext);
                m_dataPart.m_camp.Value = (long)createMonsterContext.camp;
                m_listFriendCamps = createMonsterContext.listFriendCamps;
                m_listEnemyCamps = createMonsterContext.listEnemyCamps;

                m_parent = MonsterSystem.Instance.GetEntityRoot();
                m_bFaceLeft = createMonsterContext.faceLeft;
                m_rotate = Quaternion.identity;
                m_dataPart.m_hp.Value = createMonsterContext.baseHP;
                m_dataPart.m_maxHp.Value = createMonsterContext.maxHP;
                m_dataPart.m_isLocalPos.Value = createMonsterContext.isLocalPos;
                if (createMonsterContext.isLocalPos)
                {
                    m_dataPart.m_localPos.Value = createMonsterContext.pos;
                }
                else
                {
                    m_dataPart.m_pos.Value = createMonsterContext.pos;
                }

                m_dataPart.m_forward.Value = createMonsterContext.forward;
                m_dataPart.m_visibleType.Value = (int)createMonsterContext.visibleType;
            }
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_attrModifier = null;
            m_parent = null;
            m_bFaceLeft = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdatePart();
        }

        public bool IsDie()
        {
            //return false;
            return m_dataPart.m_hp.Value <= 0;
        }

        public int GetHP()
        {
            if (m_attrModifier != null)
            {
                return m_attrModifier.OnModifiedHP(this, (int)m_dataPart.m_hp.Value);
            }

            return (int)m_dataPart.m_hp.Value;
        }

        public int GetMaxHP()
        {
            return (int)m_dataPart.m_maxHp.Value;
        }

        public void SetMaxHP(int maxHp)
        {
            m_dataPart.m_maxHp.Value = maxHp;
        }

        public void SetHPDelta(int hp)
        {
            //if (m_resPath.IndexOf("Wall") >= 0)
            //{
            //    m_dataPart.m_hp.IsDebug = true;
            //    m_dataPart.m_hp.Name = "WallHp";
            //}

            m_dataPart.m_hp.RemoteValueDelta += hp;
            m_dataPart.m_hp.Value += hp;
        }

        public float GetSpeed()
        {
            if (m_attrModifier != null)
            {
                return m_attrModifier.OnModifiedSpeed(this, m_speed);
            }

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
                m_prefabPart.gameObject.BetterSetActive(true);
                Vector3 pos = m_dataPart.m_pos.Value;

                if (m_bLocalObject == false)
                {
                    pos = m_attrModifier.WorldPositionToBattlePosition(pos, false);
                }

                if (m_dataPart.m_isLocalPos.Value)
                {
                    pos = m_dataPart.m_localPos.Value;
                }

                SetParent(m_parent);

                SetFace(m_bFaceLeft);
                Vector3 forward = m_dataPart.m_forward.Value;
                if (m_dataPart.m_isLocalPos.Value)
                {
                    SetLocalPos(ref pos);
                }
                else
                {
                    SetPos(ref pos);
                }

                SetForward(ref forward);
               // SetRotation(m_rotate);

               // m_prefabPart.ResetColor();
                IDReco reco = m_prefabPart.gameObject.GetComponent<IDReco>();
                if (null == reco)
                {
                    reco = m_prefabPart.gameObject.AddComponent<IDReco>();
                }

                reco.camp = (ulong)m_dataPart.m_camp.Value;
                reco.entID = base.id;
                reco.hitTrans = m_prefabPart.gameObject.GetComponent<SkillCompontBase>()
                    .GetSkillEffNode(ECreaturePos.Center).transform;
                reco.entType = base.type;

                //本地对象可以被打，也能攻击
                if (m_bLocalObject)
                {
                    reco.canAttack = true;
                    reco.beAttack = true;
                    reco.listFriendCamps = m_listFriendCamps;
                    reco.listEnemyCamps = m_listEnemyCamps;
                }
                else
                {
                    //远程对象不能被攻击,但是能挨打
                    reco.canAttack = false;
                    reco.beAttack = true;
                }

                InnerStateTrans();
                /*
                BuffBaseComponent buffComponent = m_prefabPart.gameObject.GetComponent<BuffBaseComponent>();
                if (null != buffComponent)
                {
                     buffComponent.Awake();
                    // buffComponent.effectActionCreate = KingBuffEffectCreator.Instance;
                    buffComponent.AddBuff(10004);
                }
                */
            }
        }

        public int GetIntAttr(int propID)
        {
            int val = 0;
            m_dicProp.TryGetValue(propID, out val);

            return val;
        }

        public void SetIntAttr(int propID, int val)
        {
            if (m_dicProp.ContainsKey(propID) == false)
            {
                m_dicProp.Add(propID, val);
            }
            else
            {
                m_dicProp[propID] = val;
            }
        }

        public void SetATTRModifier(IATTRModifier modifier)
        {
            m_attrModifier = modifier;
        }

        public CREATURE_VISIBLE_TYPE GetVisibleType()
        {
            return (CREATURE_VISIBLE_TYPE)m_dataPart.m_visibleType.Value;
        }

        public bool IsLoaded()
        {
            return m_prefabPart.gameObject!=null;
        }

        public Transform GetTransform()
        {
            return m_prefabPart.transform;
        }

        public SkeletonGraphic GetSkeltonGraphic()
        {
            return GetComponent<SpineComponent>().skeGra;
        }

        public SkeletonAnimation GetSkeletonAnimation()
        {
            return GetComponent<SpineComponent>().skeAni;
        }

        private Transform m_stateTrans;

        public void SetStateTrans(Transform hpTrans)
        {
            m_stateTrans = hpTrans;
            InnerStateTrans();
        }

        private void InnerStateTrans()
        {
            if (null != m_stateTrans && m_prefabPart.transform != null)
            {
                m_stateTrans.BetterSetParent(m_prefabPart.transform);
                m_stateTrans.localPosition = new Vector3(0, 20);
                m_stateTrans.localScale = m_bFaceLeft ? s_vLeftScale : s_vRightScale;
            }
        }
    }
}