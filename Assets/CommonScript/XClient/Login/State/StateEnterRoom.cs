
using UnityEngine;
using XClient.Common;

namespace XClient.Login.State
{
    internal class StateEnterRoom : StateBase
    {
        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log($"[Login] 请求进入房间！roomID={LoginDataManager.instance.current.roomID}");

            (GameGlobal.ComAndModule.GetModule(DModuleID.MODULE_ID_ROOM) as IRoomModule).EnterRoom(LoginDataManager.instance.current.roomID);

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_ROOM_ENTER_COMPLETE, DEventSourceType.SOURCE_TYPE_ROOM,
                ON_EVENT_ROOM_ENTER_COMPLETE, GetType().Name);

            executeEventSubscriber.AddHandler(DGlobalEvent.EVENT_ROOM_ENTER_FAIL, DEventSourceType.SOURCE_TYPE_ROOM,
                ON_EVENT_ROOM_ENTER_FAIL, GetType().Name);
        }

        private void ON_EVENT_ROOM_ENTER_FAIL(ushort eventID, object data)
        {
            Debug.LogError("[Login] 进入房间失败！");
            SwitchTo(LoginStateID.None);
        }

        private void ON_EVENT_ROOM_ENTER_COMPLETE(ushort eventID, object data)
        {
            Debug.Log("[Login] 进入房间完成！");
            SwitchTo(LoginStateID.Game);
        }
    }
}
