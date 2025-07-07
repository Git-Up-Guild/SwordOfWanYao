/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIZCLogin.cs <FileHead_AutoGenerate>
Author：mcaswen
Date：2025.06.28
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;

namespace GameScripts.TestDemo.UI.ZCLogin
{
    public partial class UIZCLogin : UIWindowEx
    {
		protected override void OnUpdateUI()
		{
			
			//Debug.LogError("123");

        }

		//@<<< ExecuteEventHandlerGenerator >>>
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_LoginClicked() //@Window 
		{

			//Debug.LogError("123");
			GameGlobalEx.LoginModule.Login();

		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>
	
	
}
