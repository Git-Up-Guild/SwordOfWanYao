/*******************************************************************
** 文件名:	MonsterSystem.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物系统(负责怪物的创建，销毁和逻辑推动)

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using GameScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;
using XGame.Poolable;
using XGame.Utils;


namespace XClient.Entity
{
    //怪物创建现场
    public class CreateMonsterContext
    {
        public int creatureID;
        public int displayID;
        public Vector3 pos;
        public bool isLocalPos;
        public bool faceLeft;
        public Vector3 forward;
        public ulong camp;
        public int baseHP = 1000;
        public int maxHP = 1000;

        //显示方式 
        public CREATURE_VISIBLE_TYPE visibleType = CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_LOCAL;

        //友方阵营
        public List<ulong> listFriendCamps;

        //敌方阵营
        public List<ulong> listEnemyCamps;

        //外观数据
        public Dictionary<int, int> dicSkinData;

        //资源路径
        public string resPath;

        public bool isPlayer;

        public void Reset()
        {
            displayID = 0;
            pos = Vector3.zero;
            isLocalPos = false;
            faceLeft = false;
            forward = Vector3.zero;
            camp = (ulong)0;
            baseHP = 1000;
            maxHP = 1000;
            visibleType = CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_LOCAL;

            listFriendCamps = null;
            listEnemyCamps = null;

            dicSkinData = null;
            resPath = "";
            isPlayer = true;
        }
    }

//怪物的类型
    public partial class EntityType
    {
        public readonly static int monsterType = 300; //代理实体
    }

//部件类型
    public partial class EntityPartType
    {
        public readonly static int entityMovePartType = 300; //移动部件
        public readonly static int entityAIPartType = 301; //AI部件
        public readonly static int entityDamagePartType = 302; //伤害部件
        public readonly static int entitySkillPartType = 303; //技能部件
        public readonly static int entityMaterialSwitchType = 304; //材质切换部件
    }

    public class MonsterSystem : Singleton<MonsterSystem>, IEventExecuteSink
    {
        //公共的创建对象
        public static CreateMonsterContext s_CreateContext = new CreateMonsterContext();

        //所有需要控制逻辑的对象
        private Dictionary<ulong, IMonster> m_dicMonster = new Dictionary<ulong, IMonster>();

        //删除列表
        private HashSet<ulong> m_hashWaitDel = new HashSet<ulong>();

        //AI创建器
        private IAIActionCreator m_AICreator = null;

        //属性修改器
        private IATTRModifier m_attrModifier = null;

        //实体根节点
        private Transform entityRoot;

        public void RegisterEntityRoot(Transform root)
        {
            entityRoot = root;
        }

        public void DeregisterEntityRoot()
        {
            entityRoot = null;
        }

        public Transform GetEntityRoot()
        {
            return entityRoot;
        }


        //创建系统
        public void Create()
        {
            GameGlobal.EntityWorld.RegisterEntityType<Monster>(EntityType.monsterType);

            GameGlobal.EntityWorld.RegisterEntityPartType<AIPart>(EntityType.monsterType, EntityPartType.entityAIPartType);
            GameGlobal.EntityWorld.RegisterEntityPartType<LightnEffectPart>(EntityType.monsterType, EntityPartType.Lightn);
            GameGlobal.EntityWorld.RegisterEntityPartType<LightnEffectPart>(EntityType.monsterType, EntityPartType.Lightn);
            GameGlobal.EntityWorld.RegisterEntityPartType<SpriteRendererMaterialSwitchPart>(EntityType.monsterType, EntityPartType.entityMaterialSwitchType);

            GameGlobal.EntityWorld.RegisterEntityPartType<MonsterDataPart>(EntityType.monsterType, EntityPartInnerType.Data);
            GameGlobal.EntityWorld.RegisterEntityPartType<EntityMovePart>(EntityType.monsterType, EntityPartType.entityMovePartType);
            GameGlobal.EntityWorld.RegisterEntityPartType<SkillPart>(EntityType.monsterType, EntityPartType.entitySkillPartType);
            GameGlobal.EntityWorld.RegisterEntityPartType<PrefabPart>(EntityType.monsterType, EntityPartType.Prefab);
            //GameGlobal.EntityWorld.RegisterEntityPartType<SpineSkinPart>(EntityType.monsterType, EntityPartType.Skin);
            //注册AI行为对象
            
            
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            itemPoolMgr.Register<AIMoveAction>();
            //itemPoolMgr.Register<AISkillAction>();
            //itemPoolMgr.Register<AICollisionExplosionAction>();
            

            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.Subscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0,
                "MonsterSystem:Create"); // FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }

        public void Release()
        {
            /*
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            itemPoolMgr.Unregister<AIMoveAction>();
            itemPoolMgr.Unregister<AISkillAction>();
            itemPoolMgr.Unregister<AICollisionExplosionAction>();
            */

            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.UnSubscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY,
                0); // FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了

        public void Clear()
        {
            foreach (var kv in m_dicMonster)
            {
                DestroyMonster(kv.Key);
            }
            m_dicMonster.Clear();
        }

        //获得刷怪的个数
        public int GetMonsterCount()
        {
            return m_dicMonster.Count;
        }

        //分配
        public IMonster CreateMonster(CreateMonsterContext createContext)
        {
            NetEntityShareInitContext.instance.Reset();
            NetEntityShareInitContext.instance.localInitContext = createContext;

            //分配一个唯一ID
            ulong entId = GameGlobal.Role.entityIDGenerator.Next();
            IMonster monster =
                GameGlobal.EntityWorld.Local.CreateEntity(EntityType.monsterType, entId, createContext.displayID,
                        NetEntityShareInitContext.instance) as
                    IMonster;

            m_dicMonster.Add(entId, monster);
            return monster;
        }

        //销毁
        public void DestroyMonster(ulong entId)
        {
            //Debug.LogError("DestroyMonster:" + entId);
            GameGlobal.EntityWorld.Local.DestroyEntity(entId);
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        public void Update()
        {
            //删除怪物
            int nCount = m_hashWaitDel.Count;
            if (nCount > 0)
            {
                foreach (ulong id in m_hashWaitDel)
                {
                    m_dicMonster.Remove(id);
                }

                m_hashWaitDel.Clear();
            }

            //推动逻辑进行
            
            foreach (IMonster monster in m_dicMonster.Values)
            {
                if (null != monster)
                {
                    monster.OnUpdate();

                   if (monster.IsDie())
                    {
                        DestroyMonster(monster.id);
                        m_hashWaitDel.Add(monster.id);
                        //MonsterDieEffectMgr.Instance.AddMonster(monster.id);
                        continue;
                    }
                }
            }
            


            //清怪测试
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    foreach (IMonster monster in m_dicMonster.Values)
            //    {
            //        DestroyMonster(monster.id);
            //    }
            //
            //    m_dicMonster.Clear();
            //    Debug.Log("空格键被按下");
            //}
        }


        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            IMonster monster = pContext as IMonster;
            if (null != monster)
            {
                ulong entID = monster.id;
                if (m_dicMonster.ContainsKey(entID))
                {
                    //m_dicMonster[entID] = null;
                    m_hashWaitDel.Add(entID);
                }
            }
        }

        //获取AI创建器
        public IAIActionCreator GetAICreator()
        {
            return m_AICreator;
        }

        //设置AI创建器
        public void SetAICreator(IAIActionCreator creator)
        {
            m_AICreator = creator;
        }

        //获取属性修改器
        public IATTRModifier GetAttrModified()
        {
            return m_attrModifier;
        }

        //设置属性修改器
        public void SetAttrModifier(IATTRModifier modifier)
        {
            m_attrModifier = modifier;
        }

        //坐标转换
        public Vector3 WorldPositionToBattlePosition(Vector3 worldPosition, bool isMyBattle)
        {

            return worldPosition;

            //return m_attrModifier.WorldPositionToBattlePosition(worldPosition, isMyBattle);
        }
    }
}