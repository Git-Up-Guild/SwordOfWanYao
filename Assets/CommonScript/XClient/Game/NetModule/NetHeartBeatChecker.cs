using cgpol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XClient.Common;
using XGame;
using XGame.FrameUpdate;
using XGame.Timer;

namespace XClient.Client
{
    /// <summary>
    /// 网络心跳检查
    /// </summary>
    internal class NetHeartBeatChecker : ITimerHandler
    {
        private NetModule _net;

        private float _lastCheckTick;
        private float _overTime = 10f;  //超时时间
        private bool _isOverTime = false;

        public bool Create(NetModule net)
        {
            net.AddGatewayMessageHandler(TCGMessage.MSG_GATEWAY_CONNECT_NTF, ON_MSG_GATEWAY_CONNECT_NTF, GetType().Name);
            net.AddGatewayMessageHandler(TCGMessage.MSG_GATEWAY_HEART_CHECK_REQ, ON_MSG_GATEWAY_HEART_CHECK_REQ, GetType().Name);

            _net = net;

            return true;
        }

        private void ON_MSG_GATEWAY_CONNECT_NTF(TCGMessage msg)
        {
            DisableCheckTimer();
            EnableCheckTimer();
        }

        private void ON_MSG_GATEWAY_HEART_CHECK_REQ(TCGMessage checkReq)
        {
            _lastCheckTick = Time.realtimeSinceStartup;
            _isOverTime = false;

            var checkRsp = _net.GetGatewayMsg(true);
            checkRsp.Init(TCGMessage.MSG_GATEWAY_HEART_CHECK_RSP);
            _net.SendMessage(checkRsp);
        }

        private void EnableCheckTimer()
        {
            _lastCheckTick = Time.realtimeSinceStartup;
            XGameComs.Get<ITimerManager>().AddTimer(this, 88, 2000, GetType().Name);
        }

        private void DisableCheckTimer()
        {
            XGameComs.Get<ITimerManager>().RemoveTimer(this);
        }

        public void Release()
        {
            _net.RemoveGatewayMessageHandler(TCGMessage.MSG_GATEWAY_CONNECT_NTF, ON_MSG_GATEWAY_CONNECT_NTF);
            _net.RemoveGatewayMessageHandler(TCGMessage.MSG_GATEWAY_HEART_CHECK_REQ, ON_MSG_GATEWAY_HEART_CHECK_REQ);
            _net = null;
        }

        public void OnTimer(TimerInfo ti)
        {
            if(Time.realtimeSinceStartup - _lastCheckTick > _overTime)
            {
                if(!_isOverTime && GameGlobal.Instance.GameStateManager.GetState() == GameState.Game)
                {
                    _isOverTime = true;
                    Debug.LogError($"客户端超过{_overTime}s没有收到心跳消息！");
                    //DisableCheckTimer();

                    //这里做进一步的处理
                }
            }
        }
    }
}
