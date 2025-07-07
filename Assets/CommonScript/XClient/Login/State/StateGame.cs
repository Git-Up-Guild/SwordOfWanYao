using UnityEngine;
using XClient.Common;
using XClient.Game;

namespace XClient.Login.State
{
    internal class StateGame : StateBase
    {
        public override void OnEnter()
        {
            base.OnEnter();

            //进入游戏状态
            Debug.Log("进入游戏！");
            GameStateManager.Instance.EnterGame();
        }

    }
}
