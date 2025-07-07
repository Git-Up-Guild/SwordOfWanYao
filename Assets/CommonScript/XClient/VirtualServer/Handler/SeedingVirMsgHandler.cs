using System;
using gamepol;
using UnityEngine;
using Random = UnityEngine.Random;

namespace XClient.Common.VirtualServer
{
    public class SeedingVirMsgHandler : IMessageHandler
    {
        private VirtualNet netServ;

        public void OnVirtualServerStarted(VirtualNet net)
        {
            netServ = net;
        }

        public void OnVirtualServerStoped(VirtualNet net)
        {
        }

        private void SendInitSeedingMsg(int iSeedingId)
        {
            TCSMessage sendObj = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_SEEDING_CREATE_NTF);
            var seedingInfo = sendObj.stTMSG_SEEDING_CREATE_NTF.set_stSeedingInfo();

            seedingInfo.set_iSeedingId(iSeedingId);
            seedingInfo.set_iLocalId(iSeedingId);

            Debug.LogError("[VirtualServer]发送创建胚子！！！！！");
            netServ.SendMessageToClient(sendObj.stHead.get_iMsgID(), sendObj);
        }

        private void SendInitDiscipleMsg()
        {
            TCSMessage sendObj = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_DISCIPLE_CREATE_NTF);
            var discipleInfo = sendObj.stTMSG_DISCIPLE_CREATE_NTF.set_stDiscipleInfo();

            discipleInfo.set_iSeedingId(101);
            discipleInfo.set_iLocalId(1);
            
            var arrProp = discipleInfo.set_arrProp();
            arrProp[(int)EnDiscipleProp.DISCIPLE_PROP_QUALIFICATION_EFF] = Random.Range(1,255);
            arrProp[(int)EnDiscipleProp.DISCIPLE_PROP_ROOT_BONE_EFF] = Random.Range(1,255);
            arrProp[(int)EnDiscipleProp.DISCIPLE_PROP_INSIGHT_EFF] = Random.Range(1,255);
            arrProp[(int)EnDiscipleProp.DISCIPLE_PROP_QI_SENSE_EFF] = Random.Range(1,255);
            arrProp[(int)EnDiscipleProp.DISCIPLE_PROP_POTENTIAL_EFF] = Random.Range(1,255);
            arrProp[(int)EnDiscipleProp.DISCIPLE_PROP_OPPORTUNITY_EFF] = Random.Range(1,255);
            var equip = discipleInfo.set_stFullEquipInfo();

            Debug.LogError("[VirtualServer]发送创建弟子！！！！！");
            netServ.SendMessageToClient(sendObj.stHead.get_iMsgID(), sendObj);
        }
        
        private void SendResultNtf()
        {
            TCSMessage sendObj = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_DISCIPLE_WANDERING_WILL_FINISH_NTF);
            sendObj.stTMSG_DISCIPLE_WANDERING_WILL_FINISH_NTF.set_iDiscipleLocalID(101);
            
            Debug.LogError("[VirtualServer]结算数据处理！！！！！");
            netServ.SendMessageToClient(sendObj.stHead.get_iMsgID(), sendObj);
        }

        private void SendResultRsp()
        {
            TCSMessage sendObj = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_DISCIPLE_WANDERING_FINISH_RSP);
            sendObj.stTMSG_DISCIPLE_WANDERING_FINISH_RSP.set_iDiscipleLocalID(101);
            
            netServ.SendMessageToClient(sendObj.stHead.get_iMsgID(), sendObj);
        }
        private void _MSG_DISCIPLE_WANDERING_FINISH_REQ(TCSMessage obj)
        {
            SendResultRsp();
        }

        public void ON_EVENT_ROLE_PART_CREATED(VirtualNet virtualNet)
        {
            this.SendInitSeedingMsg(101);
            this.SendInitSeedingMsg(102);
            this.SendInitDiscipleMsg();
            
        }

        public void ON_EVENT_ROLE_CREATED(VirtualNet virtualNet)
        {
        }
    }

}