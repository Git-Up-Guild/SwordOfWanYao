/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UILogin.cs <FileHead_AutoGenerate>
Author：LIN
Date：2025.07.14
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.IR.UI.Login
{

    public partial class UILogin : UIWindowEx
    {
		public static UILogin Instance { get; private set; }
	    
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
            ResPath = "Game/ImmortalRecord/GameResources/Prefabs/UI/UILogin.prefab";
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
