/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：Guide.cs 
Author：郑秀程
Date：2024.06.19
Description：引导模块
***************************************************************************/

using XClient.Common;

namespace XClient.Reddot
{
    public class ReddotModuleMessageHandler :  ModuleMessageHandler<ReddotModule>
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
