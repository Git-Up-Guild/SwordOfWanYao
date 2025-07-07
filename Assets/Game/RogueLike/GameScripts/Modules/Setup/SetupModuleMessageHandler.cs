/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：Setup.cs 
Author：郑秀程
Date：2025.05.13
Description：#Desc#
***************************************************************************/

using minigame;
using gamepol;
using XClient.Common;
using XClient.Net;
using XClient.RPC;
using XGame.LitJson;

namespace GameScripts.RogueLike.Setup
{
    public class SetupModuleMessageHandler :  ModuleMessageHandler<SetupModule>
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
