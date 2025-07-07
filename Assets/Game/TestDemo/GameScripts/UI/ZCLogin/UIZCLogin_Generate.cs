/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIZCLogin.cs <FileHead_AutoGenerate>
Author：mcaswen
Date：2025.06.28
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.TestDemo.UI.ZCLogin
{

    public partial class UIZCLogin : UIWindowEx
    {
		public static UIZCLogin Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Button btn_Login = null;
		//@End_Widget_Variables
		
		protected override void OnSetupOrClearWndInstance(bool isCreate)
		{
			if (isCreate)
				Instance = this;
			else
				Instance = null;
        }
		
        protected override void OnSetupParams()
        {
            ResPath = "Game/TestDemo/GameResources/Prefabs/UI/UIZCLogin.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			btn_Login = Meta.Widgets.GetWidgetComponent<Button>(0);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			btn_Login = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_Login?.onClick.AddListener(OnBtn_LoginClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_Login?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
