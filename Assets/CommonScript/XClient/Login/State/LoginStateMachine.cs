using XGame.State;

namespace XClient.Login.State
{
    internal class LoginStateMachine : BaseStateMachine
    {
        public LoginModule loginModule { get; private set; }

        public override void OnCreate<T>(object context)
        {
            base.OnCreate<T>(context);

            loginModule = context as LoginModule;

            CreateState<StateNone>(LoginStateID.None);
            CreateState<StateGateway>(LoginStateID.Gateway);
            CreateState<StateLogin>(LoginStateID.Login);
            CreateState<StateEnterRoom>(LoginStateID.EnterRoom);
            CreateState<StateGame>(LoginStateID.Game);

            SwitchTo(LoginStateID.None);
        }
    }
}
