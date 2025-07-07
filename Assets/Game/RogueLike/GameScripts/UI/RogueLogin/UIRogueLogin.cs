/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIRogueLogin.cs <FileHead_AutoGenerate>
Author：许德纪
Date：2025.05.13
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;

namespace GameScripts.RogueLike.UI.RogueLogin
{
    public partial class UIRogueLogin : UIWindowEx
    {
        protected override void OnUpdateUI()
        {
        }
		
		//@<<< ExecuteEventHandlerGenerator >>>
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_LoginClicked() //@Window 
		{
            GameGlobalEx.LoginModule.Login();
        }

		
    }
	
	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>
	
	
}
