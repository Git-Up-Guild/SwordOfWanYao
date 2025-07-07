/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: HeroCardModule.cs 
Module: HeroCard
Author: 许德纪
Date: 2025.05.09
Description: 影响卡模块
***************************************************************************/

using System.Collections.Generic;
using XClient.Common;

namespace GameScripts.CardDemo.HeroCard
{
    public class HeroCardModule : IHeroCardModule
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private HeroCardModuleMessageHandler m_MessageHandler;


        private List<HeroCardData> m_listData = new List<HeroCardData>();

        public HeroCardModule()
        {
			ModuleName = "HeroCard";
            ID = DModuleIDEx.MODULE_ID_HEROCARD;
        }

        public bool Create(object context, object config = null)
        {
            m_MessageHandler = new HeroCardModuleMessageHandler();
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

        List<HeroCardData> IHeroCardModule.GetHeroCardData()
        {
            if(m_listData.Count==0)
            {
                for (int i = 0; i < 100; ++i)
                {
                    HeroCardData data = new HeroCardData();
                    data.name = "弟子" + i;
                    data.state = "修练中";
                    data.power = i;

                    m_listData.Add(data);

                }
            }

            return m_listData;



        }
    }
}
