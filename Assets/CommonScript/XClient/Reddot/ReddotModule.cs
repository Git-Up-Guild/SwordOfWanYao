/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: ReddotModule.cs 
Module: Reddot
Author: 郑秀程
Date: 2024.06.19
Description: 红点模块
***************************************************************************/

using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.EventEngine;
using XGame.Poolable;
using XGame.Reddot;

namespace XClient.Reddot
{
    public class ReddotModule : IReddotModule
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private ReddotModuleMessageHandler m_MessageHandler;

        public IReddotManager Manager => XGameComs.Get<IReddotManager>();

        private ExecuteEventSubscriber m_EventSubscriber;

        public ReddotModule()
        {
			ModuleName = "Reddot";
            ID = DModuleID.MODULE_ID_REDDOT;
        }

        public bool Create(object context, object config = null)
        {
            m_MessageHandler = new ReddotModuleMessageHandler();
            m_MessageHandler.Create(this);
            Progress = 1f;

            m_EventSubscriber = LitePoolableObject.Instantiate<ExecuteEventSubscriber>();
            m_EventSubscriber.AddHandler(DGlobalEvent.EVENT_ENTITY_ROLE_CREATE, DEventSourceType.SOURCE_TYPE_ENTITY, ON_EVENT_ENTITY_ROLE_CREATE, GetType().Name);

            return true;
        }

        private void ON_EVENT_ENTITY_ROLE_CREATE(ushort eventID, object context)
        {
            var role = context as RoleEntity;
            Manager.StartWork(role.GetRoleID().ToString());
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

            m_EventSubscriber?.Release();
            m_EventSubscriber = null;
        }

        public bool Start()
        {
			m_MessageHandler?.Start();
            State = ModuleState.Success;
            return true;
        }

        public void Stop()
        {
			m_MessageHandler?.Stop();
        }

        public void Update()
        {
        }
		
		public void OnGameStateChange(int newStateID, int oldStateID)
        {
            if(newStateID == GameState.Login)
            {
                Manager.Clear();
            }
            else if(newStateID == GameState.Game)
            {
            }
        }
		
		//断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param)
        {
            //红点不清,不然要全部重新注册
            //Manager.Clear();
        }

        public void OnRoleDataReady()
        {
        }
    }
}
