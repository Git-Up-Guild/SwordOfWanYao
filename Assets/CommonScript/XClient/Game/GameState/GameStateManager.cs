///*******************************************************************
//** 文件名:	GameClientNew.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2020-20-28
//** 版  本:	1.0
//** 描  述:   游戏状态机
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

using XClient.Common;
using UnityEngine;
using System;
using XGame.Buffer;
using XGame.Utils;
using XGame.State;
using XGame.FrameUpdate;
using XGame;

namespace XClient.Game
{
	public class GameStateManager : IGameStateManager
	{
		//单实例
		private static GameStateManager m_instance = null;

		//状态机
		private GameStateMachine m_gameStateMachine;

		// 模块加载器流程状态
		public int m_nState = GameState.None;

		// 单实例
		public static GameStateManager Instance { get { return m_instance; } }

		//登录
		public GameStateLogin loginState { get { return State<GameStateLogin>(GameState.Login); } }

		//主城游戏
		public GameStateGame gameState { get { return State<GameStateGame>(GameState.Game); } }

		//获取状态
		private T State<T>(int nStateID) where T: class
		{
			return m_gameStateMachine.GetState((int)nStateID) as T;
		}

        //帧回调定时器
        private IFrameUpdateManager frameUpdator => XGame.XGameComs.Get<XGame.FrameUpdate.IFrameUpdateManager>();

		//状态切换器
		private GameStateSwitchValidator stateSwitchColtroller => m_gameStateMachine.validator as GameStateSwitchValidator;

		///<summary>
		/// 构造函数
		/// </summary>
		public GameStateManager()
		{
			m_instance = this;
		}

		/// <summary>
		/// 获取当前状态
		/// </summary>
		/// <returns></returns>
		public int GetState()
		{
			IState state = m_gameStateMachine.GetCurrentState();
			return state == null ? GameState.None : state.ID;
		}

		/// <summary>
		/// 获取当前要切换到的状态
		/// </summary>
		/// <returns></returns>
		public int GetTargetState()
        {
            IState state = m_gameStateMachine.GetTargetState();
            return state == null ? GameState.None : state.ID;
        }

		public bool Create()
		{
			//创建状态机
			IStateMachineManager machineManager = XGame.XGameComs.Get<IStateMachineManager>();
			m_gameStateMachine = machineManager.CreateMachine<GameStateMachine, GameStateSwitchValidator>();
			return true;
		}

		public void Release()
		{
			if(m_gameStateMachine != null)
			{
                IStateMachineManager machineManager = XGame.XGameComs.Get<IStateMachineManager>();
                machineManager.ReleaseMachine(m_gameStateMachine);
				m_gameStateMachine = null;
			}
		}

		public bool Start()
		{
			m_gameStateMachine.SwitchTo((int)GameState.None, null);
			return true;
		}

		/// <summary>
		/// 模块释放
		/// </summary>
		public void Stop()
		{
			//停止销毁状态机
            if (m_gameStateMachine != null)
            {
                IStateMachineManager machineManager = XGame.XGameComs.Get<IStateMachineManager>();
                machineManager.ReleaseMachine(m_gameStateMachine);
                m_gameStateMachine = null;
            }
        }

		/// <summary>
		/// 进入登录状态
		/// </summary>
		/// <returns></returns>
		public bool EnterLogin()
		{
			//Debug.Log("EnterLogin");
			m_gameStateMachine.SwitchTo((int)GameState.Login);
			return true;
		}

		/// <summary>
		/// /进入游戏状态
		/// </summary>
		/// <returns></returns>
		public bool EnterGame()
        {
            //Debug.Log("EnterGame");
            GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_ENTERGAME, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            m_gameStateMachine.SwitchTo((int)GameState.Game);
            return true;
		}

		public void StartGameState()
		{
		}

        public void EnterState(int state, object context)
        {
            switch (state)
            {
                case GameState.Login:
                    EnterLogin();
                    break;
                case GameState.Game:
                    EnterGame();
                    break;
                case GameState.Battle:
                    EnterBattle();
                    break;
                default:
                    Debug.LogError("尝试进入一个非法的状态！！！");
                    break;
            }
        }

		public bool EnterBattle()
		{
			m_gameStateMachine.SwitchTo(GameState.Battle);
			return true;
		}

		public void SetTargetStateReady(bool isReady)
		{
            IState state = m_gameStateMachine.GetTargetState();
			state?.SetReady(isReady);
        }
	}
}


