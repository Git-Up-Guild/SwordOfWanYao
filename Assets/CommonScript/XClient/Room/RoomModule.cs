using gamepol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Game;
using XClient.Network;
using XGame;
using XGame.Poolable;

namespace XClient.Client
{
    //房间实现模块
    public class RoomModule : IRoomModule, OnMessageSink
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }


        //房间相关信息
        private int m_roomID = 0;
        private long m_roleID = 0;
        private int m_playerIndex = 0;
        private HashSet<long> m_hashPlayers = new HashSet<long>();

        //   bookid -> propertySet<int, long>
        private Dictionary<int, RoomPropertySet> m_IntPropBook = new Dictionary<int, RoomPropertySet>();
        
        private Dictionary<int ,int> m_mapRoomInfo = new Dictionary<int, int>();


        public bool Create(object context, object config = null)
        {
            return true;
        }

        public void EnterRoom(int RoomID)
        {
            // 发送进入房间请求
            TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ROOM_USER_ENTER_REQ);
            tcsMessage.stTMSG_ROOM_USER_ENTER_REQ.set_iRoomID(RoomID);
            GameHelp.SendMessage_CS(tcsMessage);
        }

        public void LeaveRoom()
        {
            // 发送进入房间请求
            TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ROOM_USER_LEAVE_REQ);
            //tcsMessage.stTMSG_ROOM_USER_LEAVE_REQ.(m_roomID);
            GameHelp.SendMessage_CS(tcsMessage);
        }


        public void FixedUpdate()
        {

        }

        public void LateUpdate()
        {

        }

        public void OnMessage(TCSMessage msg)
        {
            uint msgID = msg.stHead.get_iMsgID();
            switch (msgID)
            {
                case TCSMessage.MSG_ROOM_USER_ENTER_RSP:
                    {
                        TMSG_ROOM_USER_ENTER_RSP context = msg.stTMSG_ROOM_USER_ENTER_RSP;
                        __OnMSG_ROOM_USER_ENTER_RSP(context);
                    }
                    break;
                case TCSMessage.MSG_ROOM_USER_ENTER_NTF:
                    {
                        TMSG_ROOM_USER_ENTER_NTF context = msg.stTMSG_ROOM_USER_ENTER_NTF;
                        __OnMSG_ROOM_USER_ENTER_NTF(context);
                    }


                    break;
                case TCSMessage.MSG_ROOM_USER_LEAVE_NTF:
                    {
                        TMSG_ROOM_USER_LEAVE_NTF context = msg.stTMSG_ROOM_USER_LEAVE_NTF;
                        __OnMSG_ROOM_USER_LEAVE_NTF(context);
                    }


                    break;
                case TCSMessage.MSG_ROOM_USER_LEAVE_RSP:
                    {
                        TMSG_ROOM_USER_LEAVE_RSP context = msg.stTMSG_ROOM_USER_LEAVE_RSP;
                        __OnMSG_ROOM_USER_LEAVE_RSP(context);
                    }
                    break;

                case TCSMessage.MSG_MINIGAME_PROP_UPDATED_NTF:
                    {
                        TMSG_MINIGAME_PROP_UPDATED_NTF context = msg.stTMSG_MINIGAME_PROP_UPDATED_NTF;
                        
                        __UpdateLocalProperty(context.get_iType(), context.get_stProperty().get_iBookID(), context.get_iVer(), context.get_iModifier()
                           , context.get_stProperty(), context.get_szPassback());
                    }
                    break;

                case TCSMessage.MSG_MINIGAME_SET_PROP_RSP:
                    {
                        TMSG_MINIGAME_SET_PROP_RSP context = msg.stTMSG_MINIGAME_SET_PROP_RSP;
                        if (context.get_iError() != 0 )
                        {
                            Debug.LogWarningFormat("TCSMessage.MSG_MINIGAME_SET_PROP_RSP has recvived error {0}.  ", context.get_iError());
                            return;
                        }


                        __UpdateLocalProperty(context.get_iType(), context.get_stProperty().get_iBookID(), context.get_iVer(),m_roleID
                            , context.get_stProperty(), context.get_szPassback());
                    }
                    break;

                case TCSMessage.MSG_ROOM_USER_ENTER_COMPLETE_NTF:
                    {
                        __OnMSG_ROOM_USER_ENTER_COMPLETE_NTF(msg.stTMSG_ROOM_USER_ENTER_COMPLETE_NTF);
                    }
                    break;
                case TCSMessage.MSG_ROOM_GET_ROOMDESC_RSP:
                {
                    TMSG_ROOM_GET_ROOMDESC_RSP context = msg.stTMSG_ROOM_GET_ROOMDESC_RSP;
                    On_MSG_ROOM_GET_ROOMDESC_RSP(context);
                }
                    break;
                default:
                    break;
            }
        }

        private void __OnMSG_ROOM_USER_ENTER_COMPLETE_NTF(TMSG_ROOM_USER_ENTER_COMPLETE_NTF context)
        {
            //发送进入房间消息
            IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            SEVENT_ROOM_ENTER_COMPLETE data = poolManger.PopObjectItem<SEVENT_ROOM_ENTER_COMPLETE>();
            data.roleID = m_roleID;
            data.roomID = m_roomID;
            data.iPlayerIdx = m_playerIndex;
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_ENTER_COMPLETE, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, data);
            poolManger.PushObjectItem(data);
        }

        public void Release()
        {

        }

        public bool Start()
        {
            INetModule netModule = CGame.Instance.NetModule;
            netModule.AddMessageHandler(TCSMessage.MSG_ROOM_USER_ENTER_RSP, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_ROOM_USER_ENTER_NTF, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_ROOM_USER_LEAVE_NTF, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_ROOM_USER_LEAVE_RSP, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_PROP_UPDATED_NTF, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_SET_PROP_RSP, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_ROOM_USER_ENTER_COMPLETE_NTF, this, "RoomModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_ROOM_GET_ROOMDESC_RSP, this, "RoomModule:Start");
            
            return true;
        }

        public void Stop()
        {
            INetModule netModule = CGame.Instance.NetModule;
            netModule.RemoveMessageHandler(TCSMessage.MSG_ROOM_USER_ENTER_RSP, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_ROOM_USER_ENTER_NTF, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_ROOM_USER_LEAVE_NTF, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_ROOM_USER_LEAVE_RSP, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_PROP_UPDATED_NTF, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_SET_PROP_RSP, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_ROOM_USER_ENTER_COMPLETE_NTF, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_ROOM_GET_ROOMDESC_RSP, this);
            

        }

        public void Update()
        {

        }
		
		//断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param)
        {
            m_roomID = 0;
            m_roleID = 0;
            m_playerIndex = 0;
            m_hashPlayers.Clear();
            m_IntPropBook.Clear();
            m_mapRoomInfo.Clear();
        }

    private void OnDestroyRoom()
        {
            //通知 agent 模块删除 所有玩家
            IAgentModule agentModule = CGame.Instance.GetModule(DModuleID.MODULE_ID_AGENT) as IAgentModule;
            agentModule.DestroyAllAgents();

            //发送离开房间消息
            IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            SRoomCreateDestroy data = poolManger.PopObjectItem<SRoomCreateDestroy>();
            data.roleID = m_roleID;
            data.roomID = m_roomID;
            data.iPlayerIdx = m_playerIndex;
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_LEAVE, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, data);
            poolManger.PushObjectItem(data);

            m_roomID = 0;
            m_roleID = 0;
            m_hashPlayers.Clear();
        }

        //本玩家进入房间
        private void __OnMSG_ROOM_USER_ENTER_RSP(TMSG_ROOM_USER_ENTER_RSP context)
        {
            int iError = context.get_iError();
            if (0 != iError)
            {
                Debug.LogError("__OnMSG_ROOM_USER_ENTER_RSP error = " + iError);
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_ENTER_FAIL, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, null);
                return;
            }

            m_roleID = context.get_i64SelfRoleID();

            Debug.Log($"[Room] 本玩家进入房间：RoleID = {m_roleID}");

            //设置本地实体ID生成器主ID
            GameGlobal.LocalEntityIDGenerator.SetMasterID((ulong)m_roleID);

            //网络对象管理器
            NetID.Temp.Set((ulong)m_roleID);
            NetworkManager.Instance.LocalClientID = NetID.Temp.ClientID;
            NetworkManager.Instance.Start();

            m_hashPlayers.Clear();
            TGameRoom stRoomID = context.get_stRoomInfo();
            m_roomID = stRoomID.get_iRoomID();
            m_playerIndex = context.get_iPlayerIdx();
            
            int nCount = stRoomID.get_iRoleNum();
            UpdateRoomRoleNum(m_roomID, nCount);

            //发送进入房间消息
            IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            SRoomCreateDestroy data = poolManger.PopObjectItem<SRoomCreateDestroy>();
            data.roleID = m_roleID;
            data.roomID = m_roomID;
            data.iPlayerIdx = m_playerIndex;
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_ENTER, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, data);
            poolManger.PushObjectItem(data);

           
            long[] arrRoleID  = stRoomID.get_arrRoleID();
            int[] arrPlayerIdx = stRoomID.get_arrPlayerIdx();
            for (int i=0;i<nCount;++i)
            {
                m_hashPlayers.Add(arrRoleID[i]);
                //通知 agent 模块 创建某个玩家
                IAgentModule agentModule = CGame.Instance.GetModule(DModuleID.MODULE_ID_AGENT) as IAgentModule;
                agentModule.CreateAgent(arrRoleID[i], arrPlayerIdx[i]);

            }

        }

        //某个玩家进入房间
        private void __OnMSG_ROOM_USER_ENTER_NTF(TMSG_ROOM_USER_ENTER_NTF context)
        {
            long i64RoleID = context.get_i64RoleID();
            if(m_hashPlayers.Contains(i64RoleID)==false)
            {
                m_hashPlayers.Add(i64RoleID);

                Debug.Log($"[Room] 其它玩家进入房间：RoleID = {i64RoleID}");

                //通知 agent 模块 创建某个玩家
                IAgentModule agentModule = CGame.Instance.GetModule(DModuleID.MODULE_ID_AGENT) as IAgentModule;
                agentModule.CreateAgent(i64RoleID, context.get_iPlayerIdx());

                //发送其他玩家进入房间的消息
                IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
                SRoomCreateDestroy data = poolManger.PopObjectItem<SRoomCreateDestroy>();
                data.roleID = i64RoleID;
                data.roomID = context.get_iRoomID();
                data.iPlayerIdx = context.get_iPlayerIdx();
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_USER_ENTER, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, data);
                poolManger.PushObjectItem(data);

            }
        }

        //角色离开通知
        private void __OnMSG_ROOM_USER_LEAVE_NTF(TMSG_ROOM_USER_LEAVE_NTF context)
        {
            int roomID = context.get_iRoomID();
            if (m_roomID != context.get_iRoomID())
            {
                return;
            }

            long roleID = context.get_i64RoleID();
            if(roleID == m_roleID)
            {
                OnDestroyRoom();
                return;
            }

            Debug.Log($"[Room] 其它玩家起开房间：RoleID = {roleID}");

            //通知 agent 模块删除 某个玩家
            IAgentModule agentModule = CGame.Instance.GetModule(DModuleID.MODULE_ID_AGENT) as IAgentModule;
            agentModule.DestroyAgent(roleID);

            //删除某个玩家
            m_hashPlayers.Remove(roleID);

        }

        //离开房间
        private void __OnMSG_ROOM_USER_LEAVE_RSP(TMSG_ROOM_USER_LEAVE_RSP context)
        {
            int iError = context.get_iError();
            if (0 != iError)
            {
                Debug.LogError("__OnMSG_ROOM_USER_LEAVE_RSP error = " + iError);

                return;
            }

            if (m_roomID== context.get_iRoomID())
            {
                //离开房间停止网络管理器
                NetworkManager.Instance.Stop();

                OnDestroyRoom();
            }
        }

        private RoomPropertySet __GetPropertySet(int iBook, bool bCreateIfNotExists)
        {
            RoomPropertySet propSet;
            if (m_IntPropBook.TryGetValue(iBook, out propSet))
            {
                return propSet;
            }

            if (!bCreateIfNotExists)
            {
                return null;
            }

            propSet = new RoomPropertySet();
            m_IntPropBook.Add(iBook, propSet);
            return propSet;
        }


        private void __UpdateLocalProperty(int iType, int iBook, long iVer, long iModifier, TPropertySet stPropSet, string sPassback )
        {
            if (iType != (int)EnPropType.enPT_Room)
            {
                return;
            }

            //  
            RoomPropertySet propSet = __GetPropertySet(iBook, true);
            propSet.Version = iVer;
            propSet.BatchUpdate(stPropSet);

            //  发布事件
            IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            SEVENT_ROOM_PROPERTY_UPDATED eventContext = poolManger.PopObjectItem<SEVENT_ROOM_PROPERTY_UPDATED>();
            eventContext.stProperty = stPropSet;
            eventContext.iVer = iVer;
            eventContext.iModifier = iModifier;
            eventContext.szPassback = sPassback;
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_PROPERTY_UPDATED, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, eventContext);
            poolManger.PushObjectItem(eventContext);

        }


        //获取本玩家的ID
        public long GetLocalRoleID()
        {
            return m_roleID;
        }

        public Dictionary<int, long> GetAllProperty(int iBook)
        {
            RoomPropertySet propSet = __GetPropertySet(iBook, false); ;
            if (propSet == null)
            {
                return null;
            }
            return propSet.GetAllProperty();
        }

        public long GetIntProperty(int iBook, int PropID)
        {
            RoomPropertySet propSet = __GetPropertySet(iBook, false); ;
            if (propSet == null)
            {
                return 0;
            }

            return propSet.GetProperty(PropID);
        }

        
        public void SetIntProperty(int iBook, int PropID, long Val, bool bCheckVer, string sPassback)
        {
            int[] arrPropID = new int[] { PropID };
            long[] arrVal = new long[] { Val};
            SetIntProperty(iBook, arrPropID, arrVal, 1, bCheckVer, sPassback);
        }

        public void SetIntProperty(int iBook, int[] arrPropID, long[] arrVal, int iLen, bool bCheckVer, string sPassback)
        {
            RoomPropertySet propSet = __GetPropertySet(iBook, true);

            //  无竞争, 直接更新
            if (!bCheckVer)
            {
                propSet.SetProperty(arrPropID, arrVal, iLen);
            }

            //  修改服务器
            TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_MINIGAME_SET_PROP_REQ);
            tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_bCheckVer((sbyte)(bCheckVer ? 1 : 0));
            tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_iType((int)EnPropType.enPT_Room);
            tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_iClientVer(propSet.Version);
            tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_szPassback(sPassback);
            TPropertySet stProperty = tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_stProperty();
            stProperty.set_iBookID(iBook);
            stProperty.set_iNum(iLen);
            for (int i = 0; i < iLen; ++i )
            {
                stProperty.set_arrID()[i] = arrPropID[i];
                stProperty.set_arrVal()[i].set_iVal(arrVal[i]);
            }
            
            GameHelp.SendMessage_CS(tcsMessage);
        }

        //请求房间信息
        public void ReqRoomInfo(int RoomID)
        {
            TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ROOM_GET_ROOMDESC_REQ);
            tcsMessage.stTMSG_ROOM_GET_ROOMDESC_REQ.set_iRoomID(RoomID);
            GameHelp.SendMessage_CS(tcsMessage);
        }

        private void On_MSG_ROOM_GET_ROOMDESC_RSP(TMSG_ROOM_GET_ROOMDESC_RSP context)
        {
            int iError = context.get_iError();
            if (0 != iError)
            {
                Debug.LogError("__On_MSG_ROOM_GET_ROOMDESC_RSP error = " + iError);
                return;
            }
            //存储房间信息
            TGameRoom stRoomInfo  = context.get_stRoomInfo();
            if (stRoomInfo!=null)
            {
                var roomId= stRoomInfo.get_iRoomID();
                var roleNum= stRoomInfo.get_iRoleNum();
                UpdateRoomRoleNum(roomId, roleNum);
            }
        }

        private void UpdateRoomRoleNum(int roomId, int roleNum)
        {
            if (m_mapRoomInfo.ContainsKey(roomId))
            { 
                m_mapRoomInfo[roomId] = roleNum;
            }
            else
            {
                m_mapRoomInfo.Add(roomId, roleNum);
            }
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_ROOM_INFO_UPDATE, (byte)DEventSourceType.SOURCE_TYPE_ROOM, 0, roomId);
        }
        
        public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }

        public int GetPlayerIndex()
        {
            return m_playerIndex; 
        }

        public int GetRoomRoleNum(int RoomID)
        { 
            if (m_mapRoomInfo.ContainsKey(RoomID))
            {
                return m_mapRoomInfo[RoomID];
            }
            return 0;
        }

        public void OnRoleDataReady()
        {
        }
    }
}
