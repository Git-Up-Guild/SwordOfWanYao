/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: EntityModule.cs 
Module: Entity
Author: 郑秀程
Date: 2024.06.17
Description: 实体模块
***************************************************************************/

using gamepol;
using UnityEngine;
using XClient.Common;
using XClient.Entity.Net;
using XClient.LightEffect;
using XClient.Network;
using XGame;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;
using XGame.FrameUpdate;
using XGameEngine.Player;

namespace XClient.Entity
{
    public class EntityModule : IEntityModule
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private EntityModuleMessageHandler m_MessageHandler;

        public IEntityWorld World => GameGlobal.EntityWorld;

        public IEntity role { get; private set; }

        public EntityModule()
        {
			ModuleName = "Entity";
            ID = DModuleID.MODULE_ID_ENTITY;
        }

        public bool Create(object context, object config = null)
        {
            m_MessageHandler = new EntityModuleMessageHandler();
            m_MessageHandler.Create(this);
            Progress = 1f;
            return true;
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }
		
        public void Release()
        {
            m_MessageHandler?.Release();
            m_MessageHandler = null;
        }

        public bool Start()
        {
			m_MessageHandler?.Start();
            State = ModuleState.Success;
            OnRegisterEntityInfo();

            World.AddEntityCreateOrDestroyGlobalCallback(OnEntityCreateOrDestroy);

            //暂时先放这里初始化两个全局系统,稍后再想地方
            new MonsterSystem();
            new BulletSystem();
            MonsterSystem.Instance.Create();
            BulletSystem.Instance.Create();
            EffectMgr.Instance();

            NetEntityEffectSyncManager.Instance.Initialize();

            //订阅帧更新
            XGameComs.Get<IFrameUpdateManager>()?.RegUpdateCallback(this.Update, "EntityModule:Start");

            return true;
        }

        public void Stop()
        {
            //退订帧更新
            XGameComs.Get<IFrameUpdateManager>()?.UnregUpdateCallback(this.Update);

            NetEntityEffectSyncManager.Instance.Reset();

            m_MessageHandler?.Stop();

            World.RemoveEntityCreateOrDestroyGlobalCallback(OnEntityCreateOrDestroy);

            //暂时先放这里初始化两个全局系统,稍后再想地方
            MonsterSystem.Instance.Release();
            BulletSystem.Instance.Release();
            EffectMgr.Instance().Release();
        }

        private void OnEntityCreateOrDestroy(IEntity entity, EntityCallbackReason reason, IEntityManager manager)
        {
            //网络实体的创建和销毁
            var netEntity = entity as INetEntity;
            if(netEntity != null)
            {
                //是本地客户端创建的实体
                if (NetworkManager.Instance.IsLocalClient(entity.id))
                {
                    if (reason == EntityCallbackReason.ForCreate)
                    {
                        NetworkManager.Instance.Syncer.CreateNetEntityToOthers(entity);
                    }
                    else
                    {
                        NetworkManager.Instance.Syncer.DestroyNetEntityToOthers(entity);
                    }
                }
            }

            //玩家自身事件
            if(entity.type == EntityInnerType.Role)
            {
                var roleEntity = entity as RoleEntity;
                if(roleEntity.isSelf)
                {
                    if (reason == EntityCallbackReason.ForCreate)
                    {
                        role = roleEntity;
                        PlayerData.Instance.SetScope(roleEntity.GetRoleID().ToString());
                        XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_CREATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, roleEntity);
                    }
                    else
                    {
                        XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, roleEntity);
                        role = null;
                    }
                }
            }

            //实体创建
            if (reason == EntityCallbackReason.ForCreate)
            {
                XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_CREATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
            }
            //实体更新
            else if (reason == EntityCallbackReason.ForUpdate)
            {
                XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
            }
            //实体销毁
            else
            {
                XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
            }
        }

        public void Update()
        {
            MonsterSystem.Instance.Update();
            BulletSystem.Instance.Update();
            EffectMgr.Instance().Update();
        }

        protected virtual void OnRegisterEntityInfo()
        {
            //主角
            GameGlobal.EntityWorld.RegisterEntityType<RoleEntity>(EntityInnerType.Role);
            GameGlobal.EntityWorld.RegisterEntityPartType<RoleDataPart>(EntityInnerType.Role, EntityPartInnerType.Data);

            //非房间模式下才有这些
            if (GameGlobal.Instance.GameInitConfig&&!GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
            {
                //主角其它部件
                if(GameGlobal.Instance.GameInitConfig.enablePlayerControl)
                {
                    GameGlobal.EntityWorld.RegisterEntityPartType<RoleVisiblePart>(EntityInnerType.Role, EntityPartInnerType.Visible);
                    GameGlobal.EntityWorld.RegisterEntityPartType<MovePart>(EntityInnerType.Role, EntityPartInnerType.Move);
                }

                //其他玩家
                GameGlobal.EntityWorld.RegisterEntityType<PersonEntity>(EntityInnerType.Person);
                GameGlobal.EntityWorld.RegisterEntityPartType<VisiblePart>(EntityInnerType.Person, EntityPartInnerType.Visible);
            }

            GameGlobal.EntityWorld.RegisterEntityType<GoodsEntity>(EntityInnerType.Goods);

            //测试网络实体
            GameGlobal.EntityWorld.RegisterEntityType<TestNetEntity>(EntityType.TestNetEntity);
            GameGlobal.EntityWorld.RegisterEntityPartType<TestNetEntityDataPart>(EntityType.TestNetEntity, EntityPartInnerType.Data);

            //NetGameObject对象
            GameGlobal.EntityWorld.RegisterEntityType<NetGameObjectEntity>(EntityType.NetGameObject);
            GameGlobal.EntityWorld.RegisterEntityPartType<PrefabPart>(EntityType.NetGameObject, EntityPartInnerType.Prefab);

        }

		public void OnGameStateChange(int newStateID, int oldStateID)
        {
            if(newStateID == (int)GameState.Login)
            {
                World.DestroyAllEntities();
            }
        }

        public void SetRoleIntProp(int id, int value)
        {
           if(null!=role)
            {
                IRoleEntity roleEntity = role as IRoleEntity;   
                if(roleEntity!=null)
                {
                    roleEntity.SetIntProp(id, value);

                    //发送同步消息给服务器
                    TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ENTITY_SET_PROP_REQ);
                    TMSG_ENTITY_SET_PROP_REQ stTMSG_ENTITY_SET_PROP_REQ = tcsMessage.stTMSG_ENTITY_SET_PROP_REQ;
                    stTMSG_ENTITY_SET_PROP_REQ.set_iRoleID(roleEntity.GetRoleID());
                    stTMSG_ENTITY_SET_PROP_REQ.set_iPropNums(1);
                    stTMSG_ENTITY_SET_PROP_REQ.set_iProp64Nums(0);
                    TEntityProp[] entityProp = stTMSG_ENTITY_SET_PROP_REQ.set_arrProperty();
                    entityProp[0].set_nID((ushort)id);
                    entityProp[0].set_nVal(value);

                    GameHelp.SendMessage_CS(tcsMessage);
                }
            }
        }

        public int GetRoleIntProp(int id)
        {
            if (null != role)
            {
                IRoleEntity roleEntity = role as IRoleEntity;
                if (roleEntity != null)
                {
                    return roleEntity.GetIntProp(id);
                }
            }

            return 0;
        }
		
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param)
        {
            //if (role != null)
            //{
            //    manager.DestroyEntity(role);
            //    role = null;
            //}
            MonsterSystem.Instance.Clear();
            BulletSystem.Instance.Clear();
            EffectMgr.Instance().Clear();
        }

        public void OnRoleDataReady()
        {
        }
        
        public void SEND_MSG_ENTITY_RENAME_REQ(string name)
        {
            TCSMessage msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ENTITY_RENAME_REQ);
            TMSG_ENTITY_RENAME_REQ msgBody = msg.stTMSG_ENTITY_RENAME_REQ;
            msgBody.set_szName(name);
            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
        }
    }
}
