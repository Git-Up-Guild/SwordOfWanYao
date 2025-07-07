using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XClient.Game;
using XGame.State;

namespace XClient.Login.State
{
    /// <summary>
    /// 登录状态ID定义
    /// </summary>
    public class LoginStateID
    {
        public static readonly int None = 1;                   //无效状态
        public static readonly int Gateway = 2;              //网关握手状态
        public static readonly int Login = 3;                  //登录状态
        public static readonly int EnterRoom = 4;         //进入房间状态
        public static readonly int Game = 5;                  //游戏状态
    }

    public class LoginStateManager
    {
        //获取到状态机
        private LoginStateMachine m_StateMachine;

        public bool Create(LoginModule loginModule)
        {
            //创建状态机
            IStateMachineManager machineManager = XGame.XGameComs.Get<IStateMachineManager>();
            m_StateMachine = machineManager.CreateMachine<LoginStateMachine, StateSwitchValidator>(loginModule);

            return true;
        }

        public void Release()
        {
            IStateMachineManager machineManager = XGame.XGameComs.Get<IStateMachineManager>();
            machineManager?.ReleaseMachine(m_StateMachine);
            m_StateMachine = null;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateID"></param>
        /// <param name="context"></param>
        public void SwitchTo(int stateID, object context)
        {
            m_StateMachine.SwitchTo(stateID, context);
        }
    }
}
