///*******************************************************************
//** 文件名:	GameStateBattlle.cs
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
    /// <summary>
    /// 战斗状态
    /// </summary>
    public class GameStateBattlle : GameStateBase
    {
        public GameStateBattlle()
        {
            isAsyncSwitch = false;
        }

        public override bool CheckReady()
        {
            return true;
        }

        public override void OnPreEnter(object context = null)
        {
            base.OnPreEnter(context);
            GameRoots.Instance.battleSceneRoot?.SafeSetActive(true);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            GameRoots.Instance.gameSceneRoot?.SafeSetActive(false);
        }
    }
}
