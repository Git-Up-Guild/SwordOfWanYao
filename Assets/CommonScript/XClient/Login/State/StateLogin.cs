
using System.Security.Cryptography;
using XClient.Common;

namespace XClient.Login.State
{
    internal class StateLogin : StateBase
    {
        public override void OnEnter()
        {
            base.OnEnter();

            //发送握手消息
            loginModule.messageHandler.SEND_MSG_LOGIN_HANDSHAKE_REQ();

            var desc = GetType().Name;

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_LOGIN_HANDSHAKE_SUC,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_HANDSHAKE_SUC, desc);

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_LOGIN_HANDSHAKE_FAIL,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_HANDSHAKE_FAIL, desc);

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_LOGIN_FAIL,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_FAIL, desc);

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_ENTITY_ROLE_DATA_READY,
                DEventSourceType.SOURCE_TYPE_ENTITY, ON_EVENT_ENTITY_ROLE_DATA_READY, desc);

        }

        public override void OnLeave()
        {
            base.OnLeave();

            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_LOGIN_HANDSHAKE_SUC,
               DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_HANDSHAKE_SUC);

            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_LOGIN_HANDSHAKE_FAIL,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_HANDSHAKE_FAIL);

            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_LOGIN_FAIL,
                DEventSourceType.SOURCE_TYPE_LOGIN, ON_EVENT_LOGIN_FAIL);

            executeEventSubscriber.RemoveHandler(DGlobalEvent.EVENT_ENTITY_ROLE_DATA_READY,
               DEventSourceType.SOURCE_TYPE_ENTITY, ON_EVENT_ENTITY_ROLE_DATA_READY);
        }

        private void ON_EVENT_ENTITY_ROLE_DATA_READY(ushort eventID, object context)
        {
            SwitchTo(LoginStateID.Game);
        }

        private void ON_EVENT_LOGIN_HANDSHAKE_SUC(ushort eventID, object context)
        {
            loginModule.messageHandler.SEND_MSG_LOGIN_LOGIN_REQ();
        }
        private void ON_EVENT_LOGIN_HANDSHAKE_FAIL(ushort eventID, object context)
        {
            SwitchTo(LoginStateID.None);
            failCount++;
            if (NeedConfirm())
            {
                netModule.ShowReconnectMessageBox();
            }
            else
            {
                netModule.OnLoginFail(EnNetConnFailType.ServerConnectFailed, EnNetLoginFailType.HandleShake_Fail, 0);
            }

        }
        private void ON_EVENT_LOGIN_FAIL(ushort eventID, object context)
        {
            SwitchTo(LoginStateID.None);
            failCount++;
            if (NeedConfirm())
            {
                netModule.ShowReconnectMessageBox();
            }
            else
            {
                netModule.OnLoginFail(EnNetConnFailType.ServerConnectFailed, EnNetLoginFailType.Login_Fail, 0);
            }
        }
    }
}
