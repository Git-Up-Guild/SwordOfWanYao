/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: SetupModule.cs 
Module: Setup
Author: 郑秀程
Date: 2025.05.13
Description: demo的启动引导初始化模块
***************************************************************************/

using GameScripts.KingHero;
using XClient.Common;
using XClient.Entity;
using XGame.Entity;

namespace GameScripts.RogueLike.Setup
{
    public class SetupModule : ISetupModule
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private SetupModuleMessageHandler m_MessageHandler;

        public SetupModule()
        {
			ModuleName = "Setup";
            ID = DModuleIDEx.MODULE_ID_SETUP;
        }

        public bool Create(object context, object config = null)
        {
            m_MessageHandler = new SetupModuleMessageHandler();
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




            //初始化子弹提供者
            EnvProviderMgr.Instance.SetBulletEnvProvider(KingBulletEnvProvider.Instance);

            //初始化AI 创建者
            MonsterSystem.Instance.SetAICreator(KingAICreator.Instance);    


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
        }

        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后创建的数据,Create中创建的不要清理了
        public void Clear(int param)
        {
        }
		
		//玩家数据准备好后回调
        public void OnRoleDataReady()
        {
        }
    }
}
