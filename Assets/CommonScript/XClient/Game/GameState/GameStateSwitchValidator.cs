///*******************************************************************
//** 文件名:	GameStateSwitchValidator.cs
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
using XClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Game
{
    /// <summary>
    /// 游戏状态切换控制器
    /// </summary>
    public class GameStateSwitchValidator : IStateSwitchValidator
    {
        private GameStateMachine m_machine;

        public void OnCreate(IStateMachine machine)
        {
            m_machine = machine as GameStateMachine;
        }

        public void OnRelease()
        {
            m_machine = null;
        }

        public bool IsTargetValid(int newStateID, object stateContext = null)
        {
            return true;
        }
    }

}
