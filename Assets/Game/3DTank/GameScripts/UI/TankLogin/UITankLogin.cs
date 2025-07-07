/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UITankLogin.cs <FileHead_AutoGenerate>
Author：许德纪
Date：2025.05.09
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;

namespace GameScripts.Tank.UI.TankLogin
{
    public partial class UITankLogin : UIWindowEx
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
