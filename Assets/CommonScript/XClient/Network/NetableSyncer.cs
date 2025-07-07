/*******************************************************************
** 文件名:	NetableSyncer.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2024/5/21 15:35:30
** 版  本:	1.0
** 描  述:	
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using gamepol;
using System.Collections.Generic;
using UnityEngine.Networking.Types;
using XClient.Common;
using XClient.Entity;
using XClient.Entity.Net;
using XGame.Entity;
using XGame.Poolable;
using XGame.Utils;

namespace XClient.Network
{
    public class NetableSyncer
    {
        private static IDebugEx Debug=>NetworkManager.Debug;

        protected CommonNetMessageRegister NetMessageRegister { get; set; }

        public void Start()
        {
            if(NetMessageRegister == null)
            {
                var desc = GetType().Name;

                NetMessageRegister = LitePoolableObject.Instantiate<CommonNetMessageRegister>();

                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_PROP_UPDATED_NTF, ON_MSG_NET_OBJ_PROP_UPDATED_NTF, desc);
                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_PROP_CHANGE_REQ, ON_MSG_NET_OBJ_PROP_CHANGE_REQ, desc);
                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_CREATE_NTF, ON_MSG_NET_OBJ_CREATE_NTF, desc);
                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_DESTROY_NTF, ON_MSG_NET_OBJ_DESTROY_NTF, desc);
                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_DESTROY_BY_ROLE_NTF, ON_MSG_NET_OBJ_DESTROY_BY_ROLE_NTF, desc);
                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_SYNC_NTF, ON_TMSG_NET_OBJ_SYNC_NTF, desc);
                NetMessageRegister.AddHandler(TCSMessage.MSG_NET_OBJ_SYNC_ELEMENT_NTF, ON_MSG_NET_OBJ_SYNC_ELEMENT_NTF, desc);
            }
        }

        public void Stop()
        {
            if (NetMessageRegister != null)
            {
                LitePoolableObject.Recycle(NetMessageRegister);
                NetMessageRegister = null;
            }
        }

        #region 消息处理器
        private void ON_MSG_NET_OBJ_PROP_CHANGE_REQ(TCSMessage message)
        {
            var msg = message.stTMSG_NET_OBJ_PROP_CHANGE_REQ;

            ulong netID = msg.get_iNetID();

            //黑名中的对象不更新
            if (NetObjectManager.Instance.IsClientWhiteListEnabled && NetObjectManager.Instance.IsObjectInBlackList(netID))
                return;

            long srcRoleID = msg.get_iSrcRoleID();
            long destRoleID = msg.get_iTagetRoleID();

            if (NetworkManager.Instance.LocalClientID != destRoleID)
            {
                Debug.Error("不是自己的消息！TMSG_NET_OBJ_PROP_CHANGE_REQ");
                return;
            }

            var obj = NetObjectManager.Instance.GetObject(netID);
            if (obj != null)
            {
                ReceiveFromOthers(obj, msg);
            }
            else
            {
                VirtualNetObjectManager.Intance.UpdateObj(netID, msg.get_stPropertySet(), true);
            }
        }

        private void ON_MSG_NET_OBJ_PROP_UPDATED_NTF(TCSMessage message)
        {
            TMSG_NET_OBJ_PROP_UPDATED_NTF msg = message.stTMSG_NET_OBJ_PROP_UPDATED_NTF;

            ulong netID = msg.get_iNetID();

            //黑名中的对象不更新
            if(NetObjectManager.Instance.IsClientWhiteListEnabled && NetObjectManager.Instance.IsObjectInBlackList(netID))
                return;

            var obj = NetObjectManager.Instance.GetObject(netID);
            if (obj != null)
            {
                ReceiveFromOthers(obj, msg);
            }
            else
            {
                VirtualNetObjectManager.Intance.UpdateObj(netID, msg.get_stPropertySet(), false);
            }
        }

        private void ON_MSG_NET_OBJ_CREATE_NTF(TCSMessage message)
        {
            var msgBody = message.stTMSG_NET_OBJ_CREATE_NTF;

            var netID = msgBody.get_iNetID();
            var ctx = msgBody.get_stEntityContext();

            var entID = ctx.get_iEntID();
            var type = ctx.get_iType();

            //网络实体对象
            if (entID > 0)
            {
                //不在白名单中的对象，放入到黑名单
                if (!NetObjectManager.Instance.IsValidClient(netID))
                {
                    NetID.Temp.Set(netID);
                    NetObjectManager.Instance.AddObjectToBlackList(netID);
                    return;
                }

                //创建实体
                OnCreateNetEntityFromMessage(message);
            }
            //普通的网络对象
            else
            {
                //启用白名单后，只有白名单中的网络对象才能被创建
                if (NetObjectManager.Instance.IsClientWhiteListEnabled 
                    && (type == NetObjectType.EntityData || type == NetObjectType.Mono))
                {
                    //不在白名单中的对象，放入到黑名单
                    if (!NetObjectManager.Instance.IsValidClient(netID))
                    {
                        NetObjectManager.Instance.AddObjectToBlackList(netID);
                        return;
                    }
                }

                //网络通信数据，这种对象都是传输数据的，不需要生成一个NetObject对象保存起来
                if (type == NetObjectType.ComData)
                {
                    //Debug.Log("收到网络通信数据消息！");
                    NetCommDataManager.Instance.OnReceive(ctx.get_szResPath(), netID, msgBody);
                    return;
                }

                var obj = NetObjectManager.Instance.GetObject(netID);
                if (obj == null)
                {
                    //这种对象为纯数据对象，不依赖与其它的对象来创建，因此自动创建
                    if (type == NetObjectType.Element)
                    {
                        string typePath = ctx.get_szResPath();
                        obj = ReflectionUtils.CreateByTypePath(typePath) as NetObject;
                        obj.Create();
                        obj.SetupNetID(netID);
                        obj.Start();
                    }
                    else
                    {
                        //网络对象通常是通过实体自主创建的，但是由于资源加载等问题，变成异步创建。
                        //因此异步导致找不到网络对象的时候就先创建一个虚拟的网络对象，用来缓存网络数据
                        VirtualNetObjectManager.Intance.CreateObj(netID);
                    }
                }

                if (obj != null)
                {
                    ReceiveFromOthers(obj, msgBody);

                    var monoNetObj = (obj as MonoNetObject);
                    monoNetObj?.SetConnectComplete();
                }
                else
                {
                    VirtualNetObjectManager.Intance.UpdateObj(netID, msgBody.get_stPropertySet(), false);
                }
            }
        }

        private void ON_MSG_NET_OBJ_DESTROY_NTF(TCSMessage message)
        {
            var msgBody = message.stTMSG_NET_OBJ_DESTROY_NTF;

            var netID = msgBody.get_iNetID();
            var entID = msgBody.get_iEntID();

            //可能在黑名单中
            NetObjectManager.Instance.RemoveObjectFromBlackList(netID);

            //这是一个实体
            if (entID > 0)
            {
                OnDestroyNetEntityFromMessage(message);
            }
            //普通的NetObject对象
            else
            {
                //正常的移除
                var obj = NetObjectManager.Instance.GetObject(netID);
                if (obj != null)
                {
                    //if (Debug.isDebug)
                    //    Debug.Log($"销毁网络实体! {netID}");

                    NetObjectManager.Instance.RemoveObject(obj);
                }
                else
                {
                    //if (Debug.isDebug)
                    //    Debug.Log($"销毁虚拟网络实体! {netID}");

                    VirtualNetObjectManager.Intance.DestroyObj(netID);
                }
            }
        }

        private void ON_MSG_NET_OBJ_DESTROY_BY_ROLE_NTF(TCSMessage message)
        {
            Debug.Log("ON_MSG_NET_OBJ_DESTROY_BY_ROLE_NTF 通知");

            var msgBody = message.stTMSG_NET_OBJ_DESTROY_BY_ROLE_NTF;
            var roleID = msgBody.get_iRoleID();

            //将黑名单中的对象移除
            NetObjectManager.Instance.RemoveObjectFromBlackListByClient(roleID);

            List<IEntity> rets = new List<IEntity>();
            GameGlobal.EntityWorld.Local.FilterEntities((entity) =>
            {
                NetID.Temp.Set(entity.id);
                return NetID.Temp.ClientID == roleID;
            }, rets);

            foreach (var entity in rets)
            {
                GameGlobal.EntityWorld.Local.DestroyEntity(entity);
            }
        }

        private void ON_TMSG_NET_OBJ_SYNC_NTF(TCSMessage message)
        {
            var bStart = message.stTMSG_NET_OBJ_SYNC_NTF.get_bStart();
            if (bStart == 1)
                Debug.Log("开始同步其它玩家数据！");
            else
                Debug.Log("同步其它玩家数据完成！");
        }

        private void ON_MSG_NET_OBJ_SYNC_ELEMENT_NTF(TCSMessage message)
        {
            var msgBody = message.stTMSG_NET_OBJ_SYNC_ELEMENT_NTF;
            var roleID = msgBody.get_iOwnerRoleID();
            var isRoomNetObject = msgBody.get_bIsRoomNetObj() == 1;
            var bStart = msgBody.get_bStart();
            if (bStart == 1)
                Debug.Log($"开始同步 NetObject 数据！roleID={roleID}, isRoomNetObject={isRoomNetObject}");
            else
                Debug.Log($"同步 NetObject 数据完成！roleID={roleID}, isRoomNetObject={isRoomNetObject}");
        }

        private void OnDestroyNetEntityFromMessage(TCSMessage msg)
        {
            var msgBody = msg.stTMSG_NET_OBJ_DESTROY_NTF;
            var entID = msgBody.get_iNetID();
            // Debug.Log($"收到 网络实体销毁通知！{entID}");

            GameGlobal.EntityWorld.Local.DestroyEntity(entID);
        }


        private void OnCreateNetEntityFromMessage(TCSMessage msg)
        {
            NetEntityShareInitContext.instance.Reset();

            var msgBody = msg.stTMSG_NET_OBJ_CREATE_NTF;

            var context = msgBody.get_stEntityContext();
            var propSet = msgBody.get_stPropertySet();

            var entID = context.get_iEntID();
            var type = context.get_iType();
            var configID = context.get_iConfigID();
            var resPath = context.get_szResPath();

            //携带了网络属性的，需要构建一个序列化器
            NetableSerializerForReceiveCSMessage.Instance.Clear();
            NetableSerializerForReceiveCSMessage.Instance.Setup(propSet);

            //设置网络数据的序列化器
            NetEntityShareInitContext.instance.netInitContext.netDataSerializer = NetableSerializerForReceiveCSMessage.Instance;

            //资源路径是NetGameObject实体的专属属性
            NetEntityShareInitContext.instance.netInitContext.resPath = resPath;

            //网络初始化标记
            NetEntityShareInitContext.instance.isInitFromNet = true;

            //创建网络实体
            GameGlobal.EntityWorld.Local.CreateEntity(type, (ulong)entID, configID, NetEntityShareInitContext.instance);

            //清理
            NetEntityShareInitContext.instance.Reset();
            NetableSerializerForReceiveCSMessage.Instance.Clear();
        }

        #endregion

        #region 网络实体创建销毁同步
        public void CreateNetEntityToOthers(IEntity entity)
        {
            // Debug.Log($"[EntityModule] 发送 网络实体创建通知！{entity.id}");

            if(entity is INetEntity)
            {
                var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_NET_OBJ_CREATE_NTF);
                var msgBody = msg.stTMSG_NET_OBJ_CREATE_NTF;

                //实体数据
                var ctx = msgBody.set_stEntityContext();
                ctx.set_iType(entity.type);
                ctx.set_iEntID(entity.id);
                ctx.set_iConfigID(entity.configId);

                msgBody.set_iNetID(entity.id);

                msgBody.set_bIsRoomNetObj(0);

                //资源实体，则还需要带上路径
                if (entity is INetEntity)
                    ctx.set_szResPath(entity.GetResPath());

                //如果有网络数据部件，那么将网络数据部件的数据也一起打包到创建现场
                NetObject netObj = null;
                var dataPart = entity.GetPart(EntityPartType.Data);
                if (dataPart is NetDataPart)
                {
                    netObj = dataPart as NetDataPart;
                }

                if (netObj != null)
                {
                    NetableSerializerForSendCSMessage.Instance.Setup(msgBody.set_stPropertySet());
                    NetableSerializerForSendCSMessage.Instance.Serializer(netObj);
                    NetableSerializerForSendCSMessage.Instance.Clear();
                }

                GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
            }
        }

        public void DestroyNetEntityToOthers(IEntity entity)
        {
            // Debug.Log($"[EntityModule] 发送 网络实体销毁通知！{entID}");

            if (entity is INetEntity)
            {
                var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_NET_OBJ_DESTROY_NTF);
                var msgBody = msg.stTMSG_NET_OBJ_DESTROY_NTF;
                msgBody.set_iNetID(entity.id);
                msgBody.set_iEntID(entity.id);

                GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
            }
        }

        #endregion

        public void CreateToOthers(INetable obj)
        {
            var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_NET_OBJ_CREATE_NTF);
            var msgBody = msg.stTMSG_NET_OBJ_CREATE_NTF;
            msgBody.set_iNetID(obj.NetID);

            //网络通信数据
            if(obj is NetCommData || obj is NetObjectElement || obj is MonoNetObject)
            {
                var stProperty = msgBody.set_stPropertySet();
                NetableSerializerForSendCSMessage.Instance.Setup(stProperty);
                NetableSerializerForSendCSMessage.Instance.Serializer(obj, false);

                //设置专门的类型和对象路径
                var ctx = msgBody.set_stEntityContext();
                ctx.set_szResPath(obj.GetType().FullName);

                if(obj is NetCommData)
                {
                    ctx.set_iType(NetObjectType.ComData);
                }
                else if (obj is NetObjectElement)
                {
                    ctx.set_iType(NetObjectType.Element);
                }
                else if (obj is MonoNetObject)
                {
                    ctx.set_iType(NetObjectType.Mono);
                }
            }

            if (obj.IsDebug)
            {
                var stProperty = msgBody.get_stPropertySet();
                var detail = NetworkUtility.GetPropertySetDetail(stProperty, obj);
                NetworkManager.Debug.Log($"Send#MSG_NET_OBJ_CREATE_NTF：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
            }

            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
            NetableSerializerForSendCSMessage.Instance.Clear();
        }

        public void DestroyToOthers(INetable obj)
        {
            if (obj.IsDebug)
                NetworkManager.Debug.Log($"Send#MSG_NET_OBJ_DESTROY_NTF：{obj.NetID}, {obj.GetType().FullName}");

            if (GameGlobal.NetModule == null)
                return;

            var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_NET_OBJ_DESTROY_NTF);
            msg.stTMSG_NET_OBJ_DESTROY_NTF.set_iNetID(obj.NetID);
            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
        }

        /// <summary>
        /// 发送属性修改请求给其它玩家
        /// </summary>
        /// <param name="obj"></param>
        public void SendPropChangeReqToTarget(INetable obj)
        {
            NetID.Temp.Set(obj.NetID);

            //本地序号为0，说明这个是玩家或者是玩家代理
            if (NetID.Temp.SerialNo == 0)
            {
                return;
            }
            else
            {
                if (NetableSerializerForSendCSMessage.Instance.IsWorking)
                {
                    NetworkManager.Debug.Error($"同步网络对象失败！当前正在同步另外一个对象！{obj.NetID}, {obj.GetType().FullName}");
                    return;
                }

                var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_NET_OBJ_PROP_CHANGE_REQ);
                var msgBody = msg.stTMSG_NET_OBJ_PROP_CHANGE_REQ;

                msgBody.set_iSrcRoleID(NetworkManager.Instance.LocalClientID);
                msgBody.set_iTagetRoleID(NetID.Temp.ClientID);

                msgBody.set_iNetID(obj.NetID);

                var stProperty = msgBody.set_stPropertySet();

                NetableSerializerForSendCSMessage.Instance.Setup(stProperty);
                bool bRet = NetableSerializerForSendCSMessage.Instance.RemoteValueDeltaSerializer(obj);

                if (!bRet)
                {
                    NetableSerializerForSendCSMessage.Instance.Clear();
                    return;
                }

                if (obj.IsDebug)
                {
                    var detail = NetworkUtility.GetPropertySetDetail(stProperty, obj);
                    NetworkManager.Debug.Log($"Send#MSG_NET_OBJ_PROP_CHANGE_REQ：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
                }

                GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
                NetableSerializerForSendCSMessage.Instance.Clear();
            }
        }

        public void UpdateToOthers(INetable obj, bool isDirtyOnly)
        {
            NetID.Temp.Set(obj.NetID);

            //本地序号为0，说明这个是玩家或者是玩家代理
            if (NetID.Temp.SerialNo == 0)
            {
                var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_MINIGAME_SET_PROP_REQ);
                UpdateRoleToOthers(obj, isDirtyOnly, msg);
            }
            else
            {
                if (GameGlobal.NetModule != null)
                {
                    var msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_NET_OBJ_PROP_UPDATED_NTF);
                    UpdateNetObjectToOthers(obj, isDirtyOnly, msg);
                }
            }
        }

        private void UpdateNetObjectToOthers(INetable obj, bool isDirtyOnly, TCSMessage msg)
        {
            var msgBody = msg.stTMSG_NET_OBJ_PROP_UPDATED_NTF;

            msgBody.set_iNetID(obj.NetID);

            var stProperty = msgBody.set_stPropertySet();

            NetableSerializerForSendCSMessage.Instance.Setup(stProperty);
            bool bRet = NetableSerializerForSendCSMessage.Instance.Serializer(obj, isDirtyOnly);

            if (!bRet)
            {
                NetableSerializerForSendCSMessage.Instance.Clear();
                return;
            }

            if (obj.IsDebug)
            {
                var detail = NetworkUtility.GetPropertySetDetail(stProperty, obj);
                NetworkManager.Debug.Log($"Send#MSG_NET_OBJ_PROP_UPDATED_NTF：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
            }

            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
            NetableSerializerForSendCSMessage.Instance.Clear();
        }


        private void UpdateRoleToOthers(INetable obj, bool isDirtyOnly, TCSMessage msg)
        {
            NetID.Temp.Set(obj.NetID);

            var msgBody = msg.stTMSG_MINIGAME_SET_PROP_REQ;

            //在生成NetID的时候就是使用RoleID生成的，因此可以这样子获取
            msgBody.set_i64RoleID(NetID.Temp.ClientID);

            msgBody.set_bCheckVer(0);
            msgBody.set_iType((int)EnPropType.enPT_Role);

            var stProperty = msgBody.set_stProperty();

            NetableSerializerForSendCSMessage.Instance.Setup(stProperty);
            bool bRet = NetableSerializerForSendCSMessage.Instance.Serializer(obj, isDirtyOnly);
            if (!bRet)
            {
                NetableSerializerForSendCSMessage.Instance.Clear();
                return;
            }

            if (obj.IsDebug)
            {
                var detail = NetworkUtility.GetPropertySetDetail(stProperty, obj);
                NetworkManager.Debug.Log($"Send#MSG_MINIGAME_SET_PROP_REQ：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
            }

            GameGlobal.NetModule?.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);

            NetableSerializerForSendCSMessage.Instance.Clear();
        }

        public static void ReceiveFromOthers(NetObject obj, TMSG_NET_OBJ_PROP_CHANGE_REQ msg)
        {
            if (obj.IsDebug)
            {
                var detail = NetworkUtility.GetPropertySetDetail(msg.get_stPropertySet(), obj);
                NetworkManager.Debug.Log($"Receive#MSG_NET_OBJ_PROP_CHANGE_REQ：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
            }

            NetableSerializerForReceiveCSMessage.Instance.Setup(msg.get_stPropertySet());
            NetableSerializerForReceiveCSMessage.Instance.RemoteValueDeltaUnserializer(obj);
            NetableSerializerForReceiveCSMessage.Instance.Clear();
        }

        public void ReceiveFromOthers(NetObject obj, TMSG_NET_OBJ_PROP_UPDATED_NTF msg)
        {
            if (obj.IsDebug)
            {
                var detail = NetworkUtility.GetPropertySetDetail(msg.get_stPropertySet(), obj);
                NetworkManager.Debug.Log($"Receive#MSG_NET_OBJ_PROP_UPDATED_NTF：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
            }

            var netID = msg.get_iNetID();
            NetID.Temp.Set(netID);
            if (NetworkManager.Instance.IsLocalClient(NetID.Temp) && !obj.IsPublic)
            {
                Debug.Error($"NetObject更新消息发送给了自己！{NetID.Temp}, type={obj.GetType().FullName}");
                //return;
            }

            NetableSerializerForReceiveCSMessage.Instance.Setup(msg.get_stPropertySet());
            NetableSerializerForReceiveCSMessage.Instance.Unserializer(obj);
            NetableSerializerForReceiveCSMessage.Instance.Clear();
        }

        public void ReceiveFromOthers(INetable obj, TMSG_NET_OBJ_CREATE_NTF msg)
        {
            if (obj.IsDebug)
            {
                var detail = NetworkUtility.GetPropertySetDetail(msg.get_stPropertySet(), obj);
                NetworkManager.Debug.Log($"Receive#TMSG_NET_OBJ_CREATE_NTF：{obj.NetID}, {obj.GetType().FullName}, Detail:{detail}");
            }

            var netID = msg.get_iNetID();
            NetID.Temp.Set(netID);

            if(NetworkManager.Instance.IsLocalClient(NetID.Temp))
            {
                Debug.Error($"NetObject创建消息发送给了自己！{NetID.Temp}");
                return;
            }

            NetableSerializerForReceiveCSMessage.Instance.Setup(msg.get_stPropertySet());
            NetableSerializerForReceiveCSMessage.Instance.Unserializer(obj);
            NetableSerializerForReceiveCSMessage.Instance.Clear();
        }
    }
}
