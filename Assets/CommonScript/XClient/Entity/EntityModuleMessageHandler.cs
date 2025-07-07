/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：EntityModuleMessageHandler.cs 
Author：郑秀程
Date：2024.06.17
Description：实体模块消息处理器
***************************************************************************/
using System.Collections.Generic;
using gamepol;
using XClient.Common;
using XGame.EventEngine;
using XGame;
using UnityEngine;
using XGame.Server;
using XGame.Poolable;
using XGame.Entity;
using XClient.Game;

namespace XClient.Entity
{

    public class EntityModuleMessageHandler : ModuleMessageHandler<EntityModule>
    {
        protected override void OnSetupHandlers()
        {
            var desc = GetType().Name;
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_CREATE_ROLE_NTF, ON_MSG_ENTITY_CREATE_ROLE_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_CREATE_LEECHDOM_NTF, ON_RECEIVE_MSG_ENTITY_CREATE_LEECHDOM_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_CREATE_ROLE_PART_NTF, ON_MSG_ENTITY_CREATE_ROLE_PART_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_DESTROY_NTF, ON_MSG_MSG_ENTITY_DESTROY_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_CREATE_PERSON_NTF, ON_MSG_ENTITY_CREATE_PERSON_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_UPDATE_PROP_NTF, ON_MSG_ENTITY_UPDATE_PROP_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_RENAME_RSP, ON_MSG_ENTITY_RENAME_RSP, desc);
        }

        private void ON_MSG_ENTITY_CREATE_PERSON_NTF(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_ENTITY_CREATE_PERSON_NTF;
            var num = msgBody.get_iNums();
            var arrSid = msgBody.get_arrSID();
            var arrPublicContext = msgBody.get_arrPublicData();
            var arrPartData = msgBody.get_arrPartData();
            for (var i = 0; i < num; i++)
            {
                var sid = arrSid[i];
                PersonCreateShareContext.instance.Reset();
                PersonCreateShareContext.instance.publicContext = arrPublicContext[i];
                PersonCreateShareContext.instance.partContext = arrPartData[i];
                GameGlobal.EntityWorld.Role.CreateEntity(EntityInnerType.Person, sid, 0, PersonCreateShareContext.instance);
                Debug.Log("[Login] 玩家被创建！");
            }
        }

        private void ON_MSG_ENTITY_CREATE_ROLE_NTF(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_ENTITY_CREATE_ROLE_NTF;
            var sid = msgBody.get_sid();
            GameServer.Instance.UpdateServerTime((ulong) msgBody.get_i64Time());
            GameServer.Instance.UpdateServerTickTime((ulong)msgBody.get_iTickTime());
            if (GameGlobal.Role != null)
            {
                GameGlobal.EntityWorld.Role.DestroyEntity(GameGlobal.Role);
            }
            GameGlobal.EntityWorld.Role.CreateEntity(EntityInnerType.Role, sid, 0, msg.stTMSG_ENTITY_CREATE_ROLE_NTF);
            Debug.Log("[Login] 玩家被创建！");


        }

        List<EntityCreateNtfContext> list = new List<EntityCreateNtfContext>();

        private void ON_RECEIVE_MSG_ENTITY_CREATE_LEECHDOM_NTF(gamepol.TCSMessage msg) //@ReceivHandler
        {
            var msgBody = msg.stTMSG_ENTITY_CREATE_LEECHDOM_NTF;

            //Write your code here.
            var goods = msgBody.get_arrPubContext();
            var nums = msgBody.get_iNums();
            IItemPoolManager itemPool = XGameComs.Get<IItemPoolManager>();
            itemPool.PushListObjectItem(list);
            EntityCreateNtfContext context = null;
            for (int i = 0; i < nums; i++)
            {
                var itemID = (int) goods[i].get_iGoodsID();
                var num = (int) goods[i].get_iNums();
                var sid = (ulong) goods[i].get_sid();
                GoodsEntityShareUpdateContext.instance.Reset();
                GoodsEntityShareUpdateContext.instance.num = num;

                //优先进行更新操作
                if (!GameGlobal.EntityWorld.Default.UpdateEntity(sid, GoodsEntityShareUpdateContext.instance))
                {
                    //更新失败则创建
                    GoodsEntityShareCreateContext.instance.Reset();
                    GoodsEntityShareCreateContext.instance.num = num;
                    GameGlobal.EntityWorld.Default.CreateEntity(EntityInnerType.Goods, sid, itemID, GoodsEntityShareCreateContext.instance);
                }
                context = itemPool.PopObjectItem<EntityCreateNtfContext>();
                context.configID = itemID;
                context.num = num;
                context.sid = sid;
                list.Add(context);
            }
            XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_GOOD_ENTITY_CHANGE, 0, 0, list);
        }

        private void ON_MSG_ENTITY_CREATE_ROLE_PART_NTF(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_ENTITY_CREATE_ROLE_PART_NTF;
            var sid = msgBody.get_sid();

            //保存玩家的篮子ID
            //GameGlobalEx.Bag.SetSkepID(msgBody.get_stPartContext().get_stPackagePart().get_dwSkepID());
            var entity = GameGlobal.EntityWorld.Role.GetEntity(sid);
            entity?.OnReceiveServerMessage(TCSMessage.MSG_ENTITY_CREATE_ROLE_PART_NTF, msgBody);
            Debug.Log("[Login] 玩家数据初始化完成！");

            //数据初始化完成事件
            XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_DATA_READY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, sid);
            CGame.Instance.OnRoleDataReady();


        }

        private void ON_MSG_MSG_ENTITY_DESTROY_NTF(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_ENTITY_DESTROY_NTF;
            var num = msgBody.get_iNums();
            var arrSid = msgBody.get_arrSID();
            var entityManager = GameGlobal.EntityWorld.Default;
            for (var i = 0; i < num; i++)
            {
#if UNITY_EDITOR
                var ent = entityManager.GetEntity(arrSid[i]);
                if (ent == null)
                    Debug.LogError($"实体不存在！sid={arrSid[i]}");
#endif
                entityManager.DestroyEntity(arrSid[i]);
            }
        }

        //收到Entity属性更新通知
        private void ON_MSG_ENTITY_UPDATE_PROP_NTF(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_ENTITY_UPDATE_PROP_NTF;
            var entityId = msgBody.get_sid();
            var entity = GameGlobal.EntityWorld.Role.GetEntity(entityId);
            if (entity == null)
            {
                return;
            }

            bool isRoleExpUpd = false;
            int roleExpPropID = (int) EnRoleProp.ROLE_PROP_EXP;
            int roleMoneyID = (int) EnRoleProp64.ROLE_PROP64_MONEY;
            int roleGoldID = (int) EnRoleProp64.ROLE_PROP64_BIND_MONEY;
            int roleStaminaID = (int) EnRoleProp.ROLE_PROP_STAMINA;

            EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT.Instance.Reset();

            //更新玩家属性
            if (entity.type == EntityInnerType.Role)
            {
                RoleEntity role = entity as RoleEntity;

                //32位属性
                var arrProps = msgBody.get_arrProperty();
                var num = msgBody.get_iPropNums();
                for (var i = 0; i < num; i++)
                {
                    var prop = arrProps[i];
                    var id = prop.get_nID();
                    var val = prop.get_nVal();
                    role?.SetIntProp(id, val);

                    if (id == roleExpPropID)
                        isRoleExpUpd = true;
                   
                        
                }

                //64位属性
                var arrProps64 = msgBody.get_arrProp64();
                num = msgBody.get_u8Prop64Nums();
                for (var i = 0; i < num; i++)
                {
                    var prop = arrProps64[i];
                    var id = prop.get_nID();
                    var val = prop.get_nVal();
                    role?.SetIntProp64(id, val);


                }

                //UI注册的事件没支持来源
                XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_DATA_UPDATE, 0, 0, null);

                //经验更新
                if (isRoleExpUpd)
                    XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_EXP_UPDATE, 0, 0, null);

                //数值物品更新
                if (EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT.Instance.lstGoodsID.Count > 0)
                {
                    XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE, 0, 0, EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT.Instance);
                }
            }
        }

        private void ON_MSG_ENTITY_RENAME_RSP(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_ENTITY_RENAME_RSP;
            var errorCode = msgBody.get_iError();
            if (errorCode != 0)
            {
                Debug.LogError($"修改玩家名称失败！(ENTITY_RENAME_RSP), errorCode={errorCode}");
               // GameUtility.ShowErrorCodeMessage((uint)errorCode);
                return;
            }
            var name = msgBody.get_szName();
            if (name != null)
            {
                GameGlobal.Role?.SetName(name);
            }
            XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_NAME_CHANGE, 0, 0, null);
        }
    }

}