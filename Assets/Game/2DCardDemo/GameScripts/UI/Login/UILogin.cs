/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UILogin.cs <FileHead_AutoGenerate>
Author：许德纪
Date：2025.05.09
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;

namespace GameScripts.CardDemo.UI.Login
{
    public partial class UILogin : UIWindowEx
    {
        protected override void OnUpdateUI()
        {
        }
		
		//@<<< ExecuteEventHandlerGenerator >>>
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_SelectRoomClicked() //@Window 
		{
		}

		private void OnBtn_NoticeClicked() //@Window 
		{
		}

		private void OnBtn_ServerClicked() //@Window 
		{
		}

		private void OnBtn_LoginClicked() //@Window 
		{

			GameGlobalEx.LoginModule.Login();
		}

		private void OnBtn_AccountClicked() //@Window 
		{
		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>
	
	
}
