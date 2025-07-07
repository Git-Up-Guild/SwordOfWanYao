using System;
using gamepol;
using UnityEngine;
namespace XClient.Common.VirtualServer
{
    public class GMVirMsgHandler : IMessageHandler
    {
        private VirtualNet netServ;

        public void OnVirtualServerStarted(VirtualNet net)
        {
            netServ = net;
            net.RegisterMsgHandler(TCSMessage.MSG_ACTION_LUA_REQUEST_REQ, _MSG_ACTION_LUA_REQUEST_REQ);
        }

        public void OnVirtualServerStoped(VirtualNet net)
        {
            net.UnregisterMsgHandler(TCSMessage.MSG_ACTION_LUA_REQUEST_REQ, _MSG_ACTION_LUA_REQUEST_REQ);
        }
        void _MSG_ACTION_LUA_REQUEST_REQ(TCSMessage obj)
        {
            string str = obj.stTMSG_ACTION_LUA_REQUEST_REQ.get_szFuncName();
            switch (str)
            {
                case "Req_T1002_SuperStoneMain": {
                    TCSMessage sendObj = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_ACTION_LUA_SCRIPT_NTF);
                    var data_str = "Request_T1002_Rsp({ bEndFlag=true,tbScript={\n        "
                        + "[1]={ callback=\"Req_T1002_SubMenu?1=1\",name=\"个人物资\",},\n        "
                        + "[2]={ callback=\"Req_T1002_SubMenu?1=2\",name=\"聊天事件\",},\n        "
                        + "[3]={ callback=\"Req_T1002_SubMenu?1=3\",name=\"0920版本5章事件\",},\n        "
                        + "[4]={ callback=\"Req_T1002_SubMenu?1=4\",name=\"故事大纲自动生成\",},\n        "
                        + "[5]={ callback=\"Req_T1002_SubMenu?1=5\",name=\"用户标签提取\",},\n"
                        + "    },\n})";
                    sendObj.stTMSG_ACTION_LUA_SCRIPT_NTF.set_szScript(data_str);

                    Debug.LogError("发起命令！！！！！");

                    netServ.SendMessageToClient(sendObj.stHead.get_iMsgID(), sendObj);
                }
                    break;
                default: {
                    Debug.LogError("没有对应的GM命令，请检查一波" + str);
                }
                    break;
            }
        }

        public void ON_EVENT_ROLE_PART_CREATED(VirtualNet virtualNet) { }

        public void ON_EVENT_ROLE_CREATED(VirtualNet virtualNet) { }
    }

}
