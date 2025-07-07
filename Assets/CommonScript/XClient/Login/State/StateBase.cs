using XClient.Common;
using XClient.Game;
using XGame.EventEngine;
using XGame.Poolable;
using XGame.State;

namespace XClient.Login.State
{
    internal class StateBase : BaseState
    {
        protected int failCount = 0;
        protected LoginStateMachine loginStateMachine => machine as LoginStateMachine;

        protected INetModule netModule => CGame.Instance.NetModule;

        protected ExecuteEventSubscriber executeEventSubscriber { get; set; }

        protected LoginModule loginModule => loginStateMachine.loginModule;

        public override void OnCreate(IStateMachine m)
        {
            executeEventSubscriber = LitePoolableObject.Instantiate<ExecuteEventSubscriber>();
            base.OnCreate(m);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            LitePoolableObject.Recycle(executeEventSubscriber);
            executeEventSubscriber = null;
        }

        public override bool CheckReady()
        {
            return true;
        }

        protected bool NeedConfirm()
        {
            return failCount % 5 == 0;
        }

        protected void SwitchTo(int stateID)
        {
            loginStateMachine.SwitchTo(stateID);
        }
    }
}
