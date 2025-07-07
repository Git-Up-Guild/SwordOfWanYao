///*******************************************************************
//** 文件名:	GameStateMachine.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2020-20-28
//** 版  本:	1.0
//** 描  述:	游戏客户端状态机
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

using XClient.Common;
using XGame.State;
using XGame.Poolable;

namespace XClient.Game
{
	public class GameStateMachine : BaseStateMachine
	{ 
		/// <summary>
		/// 控制器
		/// </summary>
		private GameStateSwitchValidator stateSwitchColtroller => validator as GameStateSwitchValidator;

		/// <summary>
		/// 获取当前状态
		/// </summary>
		/// <returns></returns>
		public int GetState()
		{
			IState state = GetCurrentState();
			return state == null ? GameState.None : state.ID;
		}

		public override void OnCreate<T>(object context)
		{
			base.OnCreate<T>(context);

			//None状态自动跳转到Create状态
			IState state = CreateState<GameStateNone>((int)GameState.None);
			state.enableAutoSwitch = false;
			state.autoSwitchTargetStateID = (int)GameState.Login;

			//Create状态自动跳转到Init状态
			state = CreateState<GameStateLogin>((int)GameState.Login);
            state.enableAutoSwitch = true;
            state.autoSwitchTargetStateID = (int)GameState.Game;

			//Init状态自动跳转到CheckVer状态
			state = CreateState<GameStateBattlle>((int)GameState.Battle);
            state.enableAutoSwitch = false;
            state.autoSwitchTargetStateID = (int)GameState.Game;

			//Game 游戏状态
			CreateState<GameStateGame>((int)GameState.Game);
		}

		public override void OnPreChangeState(int nNewState, int nOldState)
		{
            //状态切换事件
            IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            GameStateChangeEventContext data = poolManger.PopObjectItem<GameStateChangeEventContext>();
            data.nOldState = (int)nOldState;        // 老状态
            data.nNewState = (int)nNewState;           // 新状态
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE, DEventSourceType.SOURCE_TYPE_SYSTEM, 0, data);
            poolManger.PushObjectItem(data);
        }

        public override void OnAfterChangeState(int nNewState, int nOldState)
        {
            //状态切换事件
            IItemPoolManager poolManger = XGame.XGameComs.Get<IItemPoolManager>();
            GameStateChangeEventContext data = poolManger.PopObjectItem<GameStateChangeEventContext>();
            data.nOldState = (int)nOldState;        // 老状态
            data.nNewState = (int)nNewState;           // 新状态
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE, DEventSourceType.SOURCE_TYPE_SYSTEM, 0, data);
            poolManger.PushObjectItem(data);
        }

		/// <summary>
		/// 进入登录状态
		/// </summary>
		/// <returns></returns>
		public bool EnterLogin()
		{
			SwitchTo((int)GameState.Login);
			return true;
		}

		/// <summary>
		/// /进入游戏状态
		/// </summary>
		/// <returns></returns>
		public bool EnterGame()
		{
            SwitchTo((int)GameState.Game);
            return true;
		}

        /// <summary>
        /// 进入战斗状态
        /// </summary>
        /// <returns></returns>
        public bool EnterBattle()
        {
            SwitchTo((int)GameState.Battle);
            return true;
        }

	}
}


