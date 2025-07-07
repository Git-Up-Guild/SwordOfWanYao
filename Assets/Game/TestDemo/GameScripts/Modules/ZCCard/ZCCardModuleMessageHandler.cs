/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：ZCCard.cs 
Author：mcaswen
Date：2025.06.28
Description：#Desc#
***************************************************************************/

using minigame;
using gamepol;
using XClient.Common;
using XClient.Net;
using XClient.RPC;
using XGame.LitJson;

namespace GameScripts.TestDemo.ZCCard
{
    public class ZCCardModuleMessageHandler :  ModuleMessageHandler<ZCCardModule>
    {
        protected override void OnSetupHandlers()
        {
			var desc = GetType().Name;
			//@SetupMessageHandlerGenerator
        }
		
        //@ReceiveMessageHandlerGenerator

        //@SendMessageHandlerGenerator
    }
}
