/*******************************************************************
** 文件名:	BulletSystem.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  子弹系统(负责子弹的创建，销毁和逻辑推动)

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.Entity.Part;
using XGame.Entity;
using XGame;
using XGame.EventEngine;
using XGame.Utils;
using System.Threading;
using XGame.Poolable;
using System.Linq;
using XClient.FlowText;
using XGame.FlowText;
using XGame.UI;

namespace XClient.Entity
{
    public class BulletEnvProviderDefault : Singleton<BulletEnvProviderDefault>, IBulletEnvProvider
    {
        public BulletFireContext BuildFireContext(ulong srcID, int bulletID, Transform root, int userData)
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            BulletFireContext bulletFireContext = itemPoolMgr.PopObjectItem<BulletFireContext>();

            // IBulletEnvProvider bulletEnvProvider = EnvProviderMgr.Instance.GetBulletEnvProvider();

            Transform bulletRoot = root;// bulletEnvProvider.GetEntityRoot();
            bulletFireContext.src = srcID;
            bulletFireContext.target = 0;
            bulletFireContext.srcPos = root.position;
            bulletFireContext.srcLocalPos = bulletRoot.InverseTransformPoint(bulletFireContext.srcPos);
            // bulletFireContext.srcPos.y += 0.5f;
            bulletFireContext.targetPos = bulletFireContext.srcPos + root.forward * 5;
            bulletFireContext.targetLocalPos = bulletFireContext.srcLocalPos;//+ root.forward * 5;
            bulletFireContext.bulletEnvProvider = this;
            bulletFireContext.visibleType = CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_ALL;


            //test
            //FlowDamageText(root.position, -200);


            return bulletFireContext;
        }

        public bool CanFire(ulong srcID, int bulletID, int userData)
        {
            return true;
        }

        //是否需要目标
        public bool IsNeedTarget(ulong srcID, int bulletID, int userData)
        {
            return true;
        }

        //是否需要朝向目标
        public bool IsNeedForwardTarget(ulong srcID, int bulletID, int userData)
        {
            return true;
        }

        public Vector3 GetPos(ulong entID, BulletFireContext context, IBullet bullet)
        {
            return context.targetPos;
        }

        public bool OnCollisionDamage(BulletFireContext context, IBullet bullet, IDReco target)
        {
            ulong entID = target.entID;
            if (entID != 0)
            {
                IEntity entity = GameGlobal.EntityWorld.Local?.GetEntity(entID);
                if (entity != null)
                {
                    ICreatureEntity creatureEntity = entity as ICreatureEntity;
                    if (creatureEntity != null)
                    {
                        //目前测试就伤10点血
                        int hp = creatureEntity.GetHP();
                        int damage = Random.Range(0, 200);
                        creatureEntity.SetHPDelta(-damage);

                        //飘子
                        FlowDamageText(creatureEntity.GetPos(), -damage);

                        TankHealth th = creatureEntity.GetComponent<TankHealth>();
                        if(th!=null)
                        {
                            hp = creatureEntity.GetHP();
                            th.SetHP((float)hp);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void OnDestroy(BulletFireContext context, IBullet bullet)
        {
        }

        public bool OnExploreDamage(BulletFireContext context, IBullet bullet, IDReco target)
        {
            return OnCollisionDamage(context, bullet, target);
        }

        public bool OnTimedDamage(BulletFireContext context, IBullet bullet, IDReco target)
        {
            return OnCollisionDamage(context, bullet, target);
        }

        private FlowTextContext m_flowContext = new FlowTextContext();
        private Camera mainCamera = null;

        public void FlowDamageText(Vector3 pos, int hp)
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


            Vector3 scenePos = mainCamera.WorldToScreenPoint(pos);
            scenePos.y -= Screen.height / 2;
            scenePos.x -= Screen.width / 2;
            m_flowContext.content = hp.ToString();
            m_flowContext.startPosition.x = scenePos.x;
            m_flowContext.startPosition.y = scenePos.y;

            IFlowTextManager flowTextMgr = XGame.XGameComs.Get<IFlowTextManager>();
            flowTextMgr.AddFlowText((int)EFlowTextType.BattleDmgNormal, m_flowContext);
        }

        private List<int> listCamp = new List<int>();

        public IDReco SelectTarget(ulong srcID, int bulletID, IDReco IDReco, int userData, List<IDReco> filters = null)
        {
            cfg_Bullet cfg = GameGlobal.GameScheme.Bullet_0(bulletID);
            if (cfg == null)
            {
                return null;
            }


            List<IDReco> listReco = null;
            Vector3 pos = IDReco.transform.position;
            Vector3 forward = IDReco.transform.forward;

            //配置特定的敌方阵营列表
            List<ulong> listEnemyCamps =
                IDRecoEntityMgr.Instance.GetEnemyCamp(IDReco.camp, IDReco.listFriendCamps, IDReco.listEnemyCamps);
            listReco = IDRecoEntityMgr.Instance.GetIDRecoByCamp(listEnemyCamps, EntityType.monsterType, ref pos,
                ref forward, REGION_TYPE.REGION_SHAPE_CIRCLE, 9999, 0);

            /*
            if (IDReco.listEnemyCamps!=null&& IDReco.listEnemyCamps.Count>0)
            {
                listReco = IDRecoEntityMgr.Instance.GetIDRecoByCamp(IDReco.listEnemyCamps, EntityType.monsterType,ref pos, false,cfg.iDamageDistance,0);
                
            }else //没有敌方列表的
            {
                int camp = IDReco.camp;
                listCamp.Clear();
                listCamp.Add(camp);
                //有友方列表
                if (IDReco.listFriendCamps!=null&& IDReco.listFriendCamps.Count>0)
                {
                    listCamp.AddRange(IDReco.listFriendCamps);
                }

                //获取敌方阵营
                List<int> listEnemyCamps = IDRecoEntityMgr.Instance.GetCamps(listCamp);
                listReco = IDRecoEntityMgr.Instance.GetIDRecoByCamp(listEnemyCamps, EntityType.monsterType, ref pos, false, cfg.iDamageDistance, 0);

            }
            */


            if (listReco != null && listReco.Count > 0)
            {
                int nIndx = Random.RandomRange(0, listReco.Count);
                return listReco[nIndx % listReco.Count];
            }

            return null;
        }

        public bool IsExternalSupportCamp()
        {
            return false;
        }

        public List<ulong> GetExternalHitCamp(BulletFireContext context, IBullet bullet)
        {
            return null;
        }

        public bool CanHitTarget(IDReco reco, BulletFireContext context, IBullet bullet)
        {
            return true;
        }

        public EffectContext GetBuffEffectContext(string szBuffEffect)
        {
            return null;
        }

        public Transform GetEntityRoot()
        {
            return null;
        }
    }


    //子弹创建现场
    /*
    public class CreateBulletContext
    {
        public Vector3 pos;
        public Vector3 foprward;
    }*/

    //子弹的类型
    public partial class EntityType
    {
        public readonly static int NormalBulletType = 200; //子弹类型
        public readonly static int TrackBulletType = 201; //追踪子弹类型
        public readonly static int RangeBulletType = 202; //范围型子弹类型
        public readonly static int SurroundBulletType = 203; //环绕类型子弹
    }

    //部件类型
    public partial class EntityPartType
    {
        public readonly static int bulletMovePartType = 200; //移动部件
        public readonly static int bulletDamagePartType = 201; //伤害部件
    }

    public class BulletSystem : Singleton<BulletSystem>, IEventExecuteSink
    {
        //公共的创建对象
        // private CreateBulletContext m_CreateContext = new CreateBulletContext();

        //所有需要控制逻辑的对象
        private Dictionary<ulong, IBullet> m_dicBullet = new Dictionary<ulong, IBullet>();

        //删除列表
        private HashSet<ulong> m_hashWaitDel = new HashSet<ulong>();


        //创建系统
        public void Create()
        {
            //普通类型子弹
            GameGlobal.EntityWorld.RegisterEntityType<Bullet>(EntityType.NormalBulletType);
            GameGlobal.EntityWorld.RegisterEntityPartType<BulletDataPart>(EntityType.NormalBulletType, EntityPartInnerType.Data);
            GameGlobal.EntityWorld.RegisterEntityPartType<PrefabPart>(EntityType.NormalBulletType, EntityPartType.Prefab);

            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.Subscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0,
                "BulletSystem:Create"); // FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }

        public void Release()
        {
            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.UnSubscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY,
                0); // FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了

        public void Clear()
        {
            foreach (var kv in m_dicBullet)
            {
                DestroyBullet(kv.Key);
            }
            m_dicBullet.Clear();
        }


        static int index = 0;

        //分配
        public IBullet CreateBullet(int bulletID, BulletFireContext fireContext)
        {
            NetEntityShareInitContext.instance.Reset();
            NetEntityShareInitContext.instance.localInitContext = fireContext;

            //分配一个唯一ID
            ulong entId = GameGlobal.Role.entityIDGenerator.Next();
            IEntityManager manager = GameGlobal.EntityWorld.Local;
            IBullet bullet =
                manager.CreateEntity(EntityType.NormalBulletType, entId, bulletID,
                    NetEntityShareInitContext.instance) as IBullet;

            m_dicBullet.Add(entId, bullet);

            //Debug.LogError("发生子弹" + bulletID+",index="+(index++));

            return bullet;
        }

        //销毁
        public void DestroyBullet(ulong entId)
        {
            GameGlobal.EntityWorld.Local.DestroyEntity(entId);
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        public void Update()
        {
            //删除子弹
            int nCount = m_hashWaitDel.Count;
            if (nCount > 0)
            {
                foreach (ulong id in m_hashWaitDel)
                {
                    m_dicBullet.Remove(id);
                }

                m_hashWaitDel.Clear();
            }

            //推动子弹逻辑
            foreach (IBullet bullet in m_dicBullet.Values)
            {
                if (null != bullet)
                {
                    //没有被删除
                    if (m_hashWaitDel.Contains(bullet.id) == false)
                    {
                        bullet.OnUpdate();
                    }
                }
            }
        }


        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            IBullet bullet = pContext as IBullet;
            if (null != bullet)
            {
                ulong entID = bullet.id;
                if (m_dicBullet.ContainsKey(entID))
                {
                    // m_dicBullet[entID] = null;
                    m_hashWaitDel.Add(entID);
                }
            }
        }
    }
}