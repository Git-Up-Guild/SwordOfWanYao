/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIMain.cs <FileHead_AutoGenerate>
Author：甘炳钧
Date：2025.05.09
Description：主界面
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using TMPro;
//@End_NameSpace

namespace GameScripts.CardDemo.UI.Main
{

    public partial class UIMain : UIWindowEx
    {
		public static UIMain Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Image img_PannelMask = null;
		private Image img_Pannel = null;
		private Button btn_MailBtn = null;
		private Button btn_TaskBtn = null;
		private Button btn_SettingBtn = null;
		private Button btn_ShowItem = null;
		private Image img_Figure = null;
		private RectTransform tran_MainTaskEntry = null;
		private Image img_Photo = null;
		private TextMeshProUGUI text_Name = null;
		private TextMeshProUGUI text_LevelTxt = null;
		private TextMeshProUGUI text_Power = null;
		private Button btn_NameEdit = null;
		private Button btn_Market = null;
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
            ResPath = "Game/2DCardDemo/GameResources/Prefabs/UI/UIMain.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			img_PannelMask = Meta.Widgets.GetWidgetComponent<Image>(0);
			img_Pannel = Meta.Widgets.GetWidgetComponent<Image>(1);
			btn_MailBtn = Meta.Widgets.GetWidgetComponent<Button>(2);
			btn_TaskBtn = Meta.Widgets.GetWidgetComponent<Button>(3);
			btn_SettingBtn = Meta.Widgets.GetWidgetComponent<Button>(4);
			btn_ShowItem = Meta.Widgets.GetWidgetComponent<Button>(5);
			img_Figure = Meta.Widgets.GetWidgetComponent<Image>(6);
			tran_MainTaskEntry = Meta.Widgets.GetWidgetComponent<RectTransform>(7);
			img_Photo = Meta.Widgets.GetWidgetComponent<Image>(8);
			text_Name = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(9);
			text_LevelTxt = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(10);
			text_Power = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(11);
			btn_NameEdit = Meta.Widgets.GetWidgetComponent<Button>(12);
			btn_Market = Meta.Widgets.GetWidgetComponent<Button>(13);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			img_PannelMask = null;
			img_Pannel = null;
			btn_MailBtn = null;
			btn_TaskBtn = null;
			btn_SettingBtn = null;
			btn_ShowItem = null;
			img_Figure = null;
			tran_MainTaskEntry = null;
			img_Photo = null;
			text_Name = null;
			text_LevelTxt = null;
			text_Power = null;
			btn_NameEdit = null;
			btn_Market = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_MailBtn?.onClick.AddListener(OnBtn_MailBtnClicked);
			btn_TaskBtn?.onClick.AddListener(OnBtn_TaskBtnClicked);
			btn_SettingBtn?.onClick.AddListener(OnBtn_SettingBtnClicked);
			btn_ShowItem?.onClick.AddListener(OnBtn_ShowItemClicked);
			btn_NameEdit?.onClick.AddListener(OnBtn_NameEditClicked);
			btn_Market?.onClick.AddListener(OnBtn_MarketClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_MailBtn?.onClick.RemoveAllListeners();
			btn_TaskBtn?.onClick.RemoveAllListeners();
			btn_SettingBtn?.onClick.RemoveAllListeners();
			btn_ShowItem?.onClick.RemoveAllListeners();
			btn_NameEdit?.onClick.RemoveAllListeners();
			btn_Market?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
