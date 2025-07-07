using UnityEngine;
using XClient.Client;
using XClient.Common;
using XGame;
using XGame.State;

namespace XClient.Login.State
{
    internal class StateGateway : StateBase
    {
        StateGatewayContext stateContext => context as StateGatewayContext;

        public override void OnCreate(IStateMachine m)
        {
            base.OnCreate(m);
            context = new StateGatewayContext();
        }

        protected override void OnParseContext(object c)
        {
            (c as StateGatewayShareContext).CopyTo(stateContext);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log($"[Login] 请求连接网关！{stateContext.serverAddr}:{stateContext.serverPort}");
            netModule.Disconnect();
            netModule.Connect(stateContext.serverAddr, stateContext.serverPort, 1);

            var desc = GetType().Name;

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_LOGIN_GATEWAY_SUC, 
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_GATEWAY_SUC, desc);

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_LOGIN_GATEWAY_FAIL,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_GATEWAY_FAIL, desc);
            
            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_NET_CONNECT_ERROR,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_GATEWAY_ERR, desc);
            
        }

        public override void OnLeave()
        {
            base.OnLeave();
            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_LOGIN_GATEWAY_SUC,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_GATEWAY_SUC);

            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_LOGIN_GATEWAY_FAIL,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_GATEWAY_FAIL);
            
            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_NET_CONNECT_ERROR,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_GATEWAY_ERR);
        }

        private void ON_EVENT_LOGIN_GATEWAY_SUC(ushort eventID, object context)
        {
            SwitchTo(LoginStateID.Login);
            //failCount = 0;
        }

        private void ON_EVENT_LOGIN_GATEWAY_FAIL(ushort eventID, object context)
        {
            SwitchTo(LoginStateID.None);
            failCount++;
            if (NeedConfirm())
            {
                netModule.ShowReconnectMessageBox();
            }
            else
            {
                netModule.OnLoginFail(EnNetConnFailType.ServerConnectFailed, EnNetLoginFailType.Gateway_Fail, 0);
            }
        }
        private void ON_EVENT_LOGIN_GATEWAY_ERR(ushort eventID, object context)
        {
            Debug.LogError("链接错误");
            SwitchTo(LoginStateID.None);
            failCount++;
            if (NeedConfirm())
            {
                netModule.ShowReconnectMessageBox();
            }
            else
            {
                netModule.OnLoginFail(EnNetConnFailType.ServerConnectFailed, EnNetLoginFailType.Gateway_Error, 0);
            }
        }
    }
}
