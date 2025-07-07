/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: Login.cs
Module: Login
Author: 郑秀程
Date: 2024.06.17
Description: 登录模块
***************************************************************************/

using I18N.Common;
using System;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XClient.Game;
using XClient.Login.State;
using XClient.Net;
using XGame;
using XGame.EventEngine;
using XGame.Ini;
using XGame.Poolable;
using XGame.Server;
using XGame.Timer;
using XGame.Utils;
using static XGame.Ini.GameIni;

namespace XClient.Login
{
    public class LoginModule : ILoginModule, ITimerHandler
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private LoginModuleMessageHandler m_MessageHandler;

        public LoginModuleMessageHandler messageHandler => m_MessageHandler;

        public LoginStateManager stateManager { get; private set; }

        private ServerSelectManager serverSelectManager;

        //登录处理方式
        private LOGOUT_ACTION m_LoginAction = LOGOUT_ACTION.NON;

        //整点时间检测ID
        private int m_checkTimeID = 200;
        //上一次检测时间
        private int m_lastHour = -1;

        public ServerSelectManager GetServerSelectManager() { return serverSelectManager; }
        private ExecuteEventSubscriber m_EventSubscriber;
        public LoginModule()
        {
            ModuleName = "Login";
            ID = DModuleID.MODULE_ID_LOGIN;
        }

        public bool Create(object context, object config = null)
        {
            //加载登录数据
            LoginDataManager.instance.Load();

            m_MessageHandler = new LoginModuleMessageHandler();
            m_MessageHandler.Create(this);

            stateManager = new LoginStateManager();
            stateManager.Create(this);

            Progress = 1f;

            serverSelectManager = new ServerSelectManager();
            serverSelectManager.Init();

            m_EventSubscriber = LitePoolableObject.Instantiate<ExecuteEventSubscriber>();
            m_EventSubscriber.AddHandler(DGlobalEvent.EVENT_NET_RETURN_LOGIN, DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_RETURN_LOGIN, GetType().Name);
            m_EventSubscriber.AddHandler(DGlobalEvent.EVENT_NET_CLEAR, DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_NET_CLEAR, GetType().Name);
            m_EventSubscriber.AddHandler(DGlobalEvent.EVENT_NET_RECONNECT, DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_NET_RECONNECT, GetType().Name);
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
            m_EventSubscriber.RemoveHandler(DGlobalEvent.EVENT_NET_RETURN_LOGIN, DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_RETURN_LOGIN);
            m_EventSubscriber.RemoveHandler(DGlobalEvent.EVENT_NET_CLEAR, DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_NET_CLEAR);
            m_EventSubscriber.RemoveHandler(DGlobalEvent.EVENT_NET_RECONNECT, DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_NET_RECONNECT);
            m_EventSubscriber = null;
            stateManager?.Release();
            stateManager = null;

            m_MessageHandler?.Release();
            m_MessageHandler = null;
        }

        public bool Start()
        {
			m_MessageHandler?.Start();
		 

            //假如是发布版本,默认自动登录
            if(GameIni.Instance.releaseTarget!= ReleaseTarget.DEVELOPMENT)
            {
                m_LoginAction = LOGOUT_ACTION.LOGIN;
            }

            State = ModuleState.Success;


            

            return true;
        }

        public void Stop()
        {
            //删除定时器
            XGame.XGameComs.Get<ITimerManager>().RemoveTimer(this, m_checkTimeID);
            m_MessageHandler?.Stop();
        }

        public void Update()
        {
            
        }
		
		public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }

        private void ON_EVENT_RETURN_LOGIN(ushort eventID, object context)
        {
            //stateManager.SwitchTo(LoginStateID.None, null);


            Logout(LOGOUT_ACTION.SWITCH_SERVER);
        }
        private void ON_EVENT_NET_CLEAR(ushort eventID, object context)
        {
            if (GameStateManager.Instance.loginState != null)
            {
                
            }
        }
        private void ON_EVENT_NET_RECONNECT(ushort eventID, object context)
        {
#if  !UNITY_EDITOR
            if (!LoginDataManager.instance.current.IsValid())
            {
                GameGlobal.LoginModule.Logout(LOGOUT_ACTION.LOGIN);
                return;
            }
#endif
            //先切换空状态，然后切换登录
            stateManager.SwitchTo(LoginStateID.None, null);
            Login();
        }
        /// <summary>
        /// 单机登录
        /// </summary>
        private void LoginWithStandalone()
        {
            //创建玩家角色
            GameGlobal.EntityWorld.Role.DestroyEntityByType(EntityInnerType.Role);
            GameGlobal.EntityWorld.Role.CreateEntity(EntityInnerType.Role, 1, 0, null);

            //派发事件
            GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_ENTITY_ROLE_DATA_READY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);
            CGame.Instance.OnRoleDataReady();

            //进入游戏状态
            stateManager.SwitchTo(LoginStateID.Game, null);
        }

        private int m_roomId = 0;
        public void Login(int roomID = 0)
        {
            m_lastHour = -1;

            //单机模式的时候，直接
            if (GameGlobal.Instance.GameInitConfig.isStandaloneMode||true)
            {
                LoginWithStandalone();
                return;
            }
            m_roomId = roomID;
            // var serverInfo = GameGlobal.Instance.GameInitConfig.GetCurrentServerInfo();
            var serverInfo = serverSelectManager.GetCurrentServerInfo();
            if (serverInfo == null)
            {
                Debug.LogError("[Login] 请先设置服务器信息！");
                return;
            }

            StateGatewayShareContext.instance.Reset();
            StateGatewayShareContext.instance.serverAddr = serverInfo.ip;
            StateGatewayShareContext.instance.serverPort = serverInfo.GetRandomPort();

            if (!LoginDataManager.instance.current.IsValid())
            {
#if UNITY_EDITOR
                //LoginDataManager.instance.current.userName = $"XGame123";
                LoginDataManager.instance.current.userName = $"XGame{System.DateTime.Now.Ticks}" + SystemInfo.deviceUniqueIdentifier;
                Debug.Log("生成账户，账户名为" + LoginDataManager.instance.current.userName);
                LoginDataManager.instance.current.password = "123456";
#endif
            }

            LoginDataManager.instance.current.serverAddr = serverInfo.ip;
            LoginDataManager.instance.current.serverPort = serverInfo.GetRandomPort();
            LoginDataManager.instance.current.serverID = serverInfo.zid;
            LoginDataManager.instance.current.serverSID = serverInfo.sid;

            if (roomID > 0)
                GameGlobal.Instance.GameInitConfig.serverConfig.roomID = roomID;

            LoginDataManager.instance.current.roomID = serverSelectManager.GetDefaultRoomId();
            //保存一下登录的服务器ZID
            Debug.LogError("登录的服务器 ZID=" + serverInfo.zid);
            serverSelectManager.SetSelectId(serverInfo.zid);    

            LoginDataManager.instance.SyncCurrentToSave();

            stateManager.SwitchTo(LoginStateID.Gateway, StateGatewayShareContext.instance);

            //先删除定时器
            XGame.XGameComs.Get<ITimerManager>().RemoveTimer(this, m_checkTimeID);
            long interaval = 300000;// (3600 + (integralHour.Ticks - curDate.Ticks) / 10000000) * 1000;
            XGame.XGameComs.Get<ITimerManager>().AddTimer(this, m_checkTimeID, (int)interaval, "CheckIntegralHourPoint");
        }

        public void Logout(LOGOUT_ACTION action)
        {
      

            m_LoginAction = action;
            stateManager.SwitchTo(LoginStateID.None, null);
            GameStateManager.Instance.EnterLogin();
            GameGlobal.Instance.NetModule.Disconnect();

            //假如已经在登录状态了，先关窗口？？？登录窗口很特殊，为了不闪烁
            if(LOGOUT_ACTION.SWITCH_SERVER== action)
            {
                GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_SHOW_SELECT_SERVE, 0, 0, null);
            }


        }

        //获取登出行为
        public LOGOUT_ACTION GetLogoutAction()
        {
            return m_LoginAction;
        }

        public void StartEnterRoom(int roomID)
        {
            LoginDataManager.instance.current.roomID = roomID;
            stateManager.SwitchTo(LoginStateID.EnterRoom, null);
        }

        public void ExitRoom()
        {
            stateManager.SwitchTo(LoginStateID.Game, null);
        }
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了

        public void Clear(int param)
        {
   
        }

        public void OnTimer(TimerInfo ti)
        {
            CheckIntegralHourPoint();
        }

        public void CheckIntegralHourPoint()
        {


            //获取当前时间
            DateTime curDate = DateTimeUtil.GetDataTime2((uint)GameServer.Instance.ServerTime);
            if(m_lastHour!= curDate.Hour)
            {
             
                m_lastHour = curDate.Hour;

                //发送事件
               // XGameComs.Get<IEventEngine>()?.FireExecute(DGlobalEvent.EVENT_LOGIN_HOUR_CHANGE, 0, 0, TimeHourChangeContext.Instance);
            }

          

        }

        public void OnRoleDataReady()
        {
        }
    }
}