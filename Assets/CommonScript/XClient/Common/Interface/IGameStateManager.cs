///*******************************************************************
//** 文件名:	IGameStateManager.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2021-01-28
//** 版  本:	1.0
//** 描  述:	
//** 应  用:   游戏状态管理
//**			
//**			
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/


// 游戏客户端状态-游戏状态管理接口 IGameClient
namespace XClient.Common
{
    /// <summary>
    /// 游戏状态
    /// </summary>
    public static class GameState
	{
        /// <summary>
        /// 初始化前
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// 登录状态
        /// </summary>
        public const int Login = 1;

        //游戏状态
        public const int Game = 2;

        //战斗状态
        public const int Battle = 3;        
	}

	public interface IGameStateManager
	{
		/// <summary>
		/// 获取当前状态
		/// </summary>
		/// <returns></returns>
		int GetState();

		/// <summary>
		/// 获取当前切换的目标状态
		/// </summary>
		/// <returns></returns>
		int GetTargetState();

		/// <summary>
		/// 设置目标状态准备好了。调用切换状态接口后状态并不会马上切换完成，
		/// 必须目标状态准备好了，才能最终切换成功
		/// </summary>
		void SetTargetStateReady(bool isReady);

        /// <summary>
        /// 进入登录状态
        /// </summary>
        /// <returns></returns>
        bool EnterLogin();

		/// <summary>
		/// /进入游戏状态
		/// </summary>
		/// <returns></returns>
		bool EnterGame();

		/// <summary>
		/// 进入快速重连
		/// </summary>
		/// <returns></returns>
		bool EnterBattle();

		/// <summary>
		/// 切换到某个状态
		/// </summary>
		/// <param name="state"></param>
		/// <param name="context"></param>
		void EnterState(int state, object context);

    }
}
