///*******************************************************************
//** 文件名:	GameStateGame.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2020-20-28
//** 版  本:	1.0
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

using Game;

namespace XClient.Game
{
    // 游戏客户端状态-主城游戏 Game
    public class GameStateGame : GameStateBase
    {
        private bool m_IsSystemSettingApplied = false;

        public GameStateGame()
        {
            isAsyncSwitch = true;
            isSupportReadyStateValidator = true;
        }

        private void ApplySystemSetting()
        {
            if(!m_IsSystemSettingApplied)
            {
                m_IsSystemSettingApplied = true;
            }
        }

        public override void OnPreEnter(object context = null)
        {
            base.OnPreEnter(context);

            isReady = false;

            GameRoots.Instance.gameSceneRoot?.SafeSetActive(true);
        }

        public override bool CheckReady()
		{
            return base.CheckReady();
		}

		public override void OnEnter()
		{
            base.OnEnter();

            GameRoots.Instance.battleSceneRoot?.SafeSetActive(false);

            //应用系统设置
            ApplySystemSetting();
        }

        public override void OnLeave()
		{
            base.OnLeave();
		}
	}
}
