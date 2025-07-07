/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：HeroCard.cs 
Author：许德纪
Date：2025.05.09
Description：#Desc#
***************************************************************************/

using minigame;
using gamepol;
using XClient.Common;
using XClient.Net;
using XClient.RPC;
using XGame.LitJson;

namespace GameScripts.CardDemo.HeroCard
{
    public class HeroCardModuleMessageHandler :  ModuleMessageHandler<HeroCardModule>
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
