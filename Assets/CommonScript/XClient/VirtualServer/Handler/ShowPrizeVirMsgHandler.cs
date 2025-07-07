using System;
using gamepol;
using UnityEngine;
namespace XClient.Common.VirtualServer
{
    public class ShowPrizeVirMsgHandler : IMessageHandler
    {
        private VirtualNet netServ;

        public void OnVirtualServerStarted(VirtualNet net)
        {
            netServ = net;
        }

        public void OnVirtualServerStoped(VirtualNet net)
        {
        }
        public void ON_EVENT_ROLE_PART_CREATED(VirtualNet virtualNet)
        {
            TCSMessage sendObj = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ACTION_SHOW_PRIZE_NTF);
            sendObj.stTMSG_ACTION_SHOW_PRIZE_NTF.set_iWanderGoodsNum(2);
            var numInfo = sendObj.stTMSG_ACTION_SHOW_PRIZE_NTF.set_arrWanderGoodsNum();
            numInfo[0] = 11;
            numInfo[1] = 21;
            var idInfo = sendObj.stTMSG_ACTION_SHOW_PRIZE_NTF.set_arrWanderGoodsID();
            idInfo[0] = 100001;
            idInfo[1] = 1;
            
            netServ.SendMessageToClient(sendObj.stHead.get_iMsgID(), sendObj);
        }

        public void ON_EVENT_ROLE_CREATED(VirtualNet virtualNet) { }
    }

}
