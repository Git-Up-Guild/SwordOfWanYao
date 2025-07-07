///*******************************************************************
//** 文件名:	GameStateBase.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2020-20-28
//** 版  本:	1.0
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

using XGame.State;

namespace XClient.Game
{
    /// <summary>
    /// 游戏状态基类
    /// </summary>
    public class GameStateBase : BaseState
    {
        /// <summary>
        /// 游戏状态机
        /// </summary>
        protected GameStateMachine stateMachine => machine as GameStateMachine;

        /// <summary>
        /// 状态前切换器
        /// </summary>
        protected GameStateSwitchValidator stateSwitchColtroller => machine.validator as GameStateSwitchValidator;

    }
}
