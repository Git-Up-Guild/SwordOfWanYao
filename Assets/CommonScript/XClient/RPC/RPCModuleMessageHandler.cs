/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：DataDispatcher.cs 
Author：曾嘉喜
Date：2024.12.25
Description：#Desc#
***************************************************************************/

using minigame;
using XClient.Common;
using XClient.Net;
using XGame.FlowText;
using XGame.LitJson;

namespace XClient.RPC
{
    public class RPCModuleMessageHandler :  ModuleMessageHandler<RPCModule>
    {
        protected override void OnSetupHandlers()
        {
			var desc = GetType().Name;
            netMessageRegister.AddHandler(gamepol.TCSMessage.MSG_ACTION_LUALIKE_SCRIPT_NTF, ON_MSG_ACTION_LUALIKE_SCRIPT_NTF, desc);
            //this.module.Register(RPCNames.Client_RPC_AddFlowText, ON_RPC_Client_RPC_AddFlowText);
			//@SetupMessageHandlerGenerator
        }

        void ON_RPC_Client_RPC_AddFlowText(string obj)
        {
            var jsonData = JsonMapper.ToObject(obj);
            SystemFlowText.Show(jsonData["text"].ToString());
        }

        void ON_MSG_ACTION_LUALIKE_SCRIPT_NTF(gamepol.TCSMessage msg)
        {
            var ntfData = msg.stTMSG_ACTION_LUALIKE_SCRIPT_NTF;
            module.OnGetNetData(ntfData.get_szFunction(), ntfData.get_szJsonParam());
        }

        //@ReceiveMessageHandlerGenerator

        //@SendMessageHandlerGenerator
    }
}
