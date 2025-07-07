/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UILogin.cs <FileHead_AutoGenerate>
Author：许德纪
Date：2025.05.09
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using TMPro;
//@End_NameSpace

namespace GameScripts.CardDemo.UI.Login
{

    public partial class UILogin : UIWindowEx
    {
		public static UILogin Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private TextMeshProUGUI text_ServerInfo = null;
		private Button btn_SelectRoom = null;
		private Button btn_Notice = null;
		private Button btn_Server = null;
		private Button btn_Login = null;
		private RectTransform tran_LoginRoot = null;
		private Button btn_Account = null;
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
            ResPath = "Game/2DCardDemo/GameResources/Prefabs/UI/UILogin.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			text_ServerInfo = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(0);
			btn_SelectRoom = Meta.Widgets.GetWidgetComponent<Button>(1);
			btn_Notice = Meta.Widgets.GetWidgetComponent<Button>(2);
			btn_Server = Meta.Widgets.GetWidgetComponent<Button>(3);
			btn_Login = Meta.Widgets.GetWidgetComponent<Button>(4);
			tran_LoginRoot = Meta.Widgets.GetWidgetComponent<RectTransform>(5);
			btn_Account = Meta.Widgets.GetWidgetComponent<Button>(6);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			text_ServerInfo = null;
			btn_SelectRoom = null;
			btn_Notice = null;
			btn_Server = null;
			btn_Login = null;
			tran_LoginRoot = null;
			btn_Account = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_SelectRoom?.onClick.AddListener(OnBtn_SelectRoomClicked);
			btn_Notice?.onClick.AddListener(OnBtn_NoticeClicked);
			btn_Server?.onClick.AddListener(OnBtn_ServerClicked);
			btn_Login?.onClick.AddListener(OnBtn_LoginClicked);
			btn_Account?.onClick.AddListener(OnBtn_AccountClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_SelectRoom?.onClick.RemoveAllListeners();
			btn_Notice?.onClick.RemoveAllListeners();
			btn_Server?.onClick.RemoveAllListeners();
			btn_Login?.onClick.RemoveAllListeners();
			btn_Account?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
