///*******************************************************************
//** 文件名:	GameStateLogin.cs
//** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
//** 创建人:	郑秀程
//** 日  期:	2020-20-28
//** 版  本:	1.0
//**************************** 修改记录 ******************************
//** 修改人: 
//** 日  期: 
//** 描  述: 
//********************************************************************/

using XClient.Common;
using UnityEngine;
using XGame.State;

namespace XClient.Game
{
	// 游戏客户端状态-登录 GameStateLogin
	public class GameStateLogin : GameStateBase
	{
        //是否登陆成功
        private bool m_isLoginSuccess;

        public GameStateLogin()
        {
            isAsyncSwitch = false;
        }

        public void SetLoginResult(bool isSuccess)
        {
            m_isLoginSuccess = isSuccess;
        }

		public override  void OnEnter()
		{
            base.OnEnter();
            m_isLoginSuccess = false;

            //清除所有的命令处理
            GameGlobal.SystemCommandQueue.ClearAll();
        }

		public override void OnLeave()
        {
            base.OnLeave();
        }

        public override bool CheckReady()
        {
            return true;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            //已经完成的不处理
            if(isEnd)
            {
                return;
            }

            //是否登陆完成
            isEnd = m_isLoginSuccess;
        }
	}
}



