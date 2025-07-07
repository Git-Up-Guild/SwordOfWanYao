///*******************************************************************
//** 文件名:	GameStateNoneNew.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2020-20-28
//** 版  本:	1.0
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

#define OpenDebugInfo_GameStateLogin
using XClient.Common;
using UnityEngine;
using XGame.State;

namespace XClient.Game
{
	// 游戏客户端状态-原始状态
	public class GameStateNone : GameStateBase
	{

		public override void OnCreate(IStateMachine m)
		{
			isSupportReadyStateValidator = false;

            base.OnCreate(m);
		}

		public override bool CheckReady()
		{
			return true;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			isEnd = true;
		}
	}
}



