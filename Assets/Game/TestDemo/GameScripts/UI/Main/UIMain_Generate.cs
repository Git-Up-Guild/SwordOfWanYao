/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIMain.cs <FileHead_AutoGenerate>
Author：mcaswen
Date：2025.06.28
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.TestDemo.UI.Main
{

    public partial class UIMain : UIWindowEx
    {
		public static UIMain Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Button btn_Bag = null;
		private Button btn_BagEx = null;
		private Button btn_Bag2 = null;
		private Button btn_Bag3 = null;
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
            ResPath = "Game/TestDemo/GameResources/Prefabs/UI/UIMain.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			btn_Bag = Meta.Widgets.GetWidgetComponent<Button>(0);
			btn_BagEx = Meta.Widgets.GetWidgetComponent<Button>(1);
			btn_Bag2 = Meta.Widgets.GetWidgetComponent<Button>(2);
			btn_Bag3 = Meta.Widgets.GetWidgetComponent<Button>(3);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			btn_Bag = null;
			btn_BagEx = null;
			btn_Bag2 = null;
			btn_Bag3 = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_Bag?.onClick.AddListener(OnBtn_BagClicked);
			btn_BagEx?.onClick.AddListener(OnBtn_BagExClicked);
			btn_Bag2?.onClick.AddListener(OnBtn_Bag2Clicked);
			btn_Bag3?.onClick.AddListener(OnBtn_Bag3Clicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_Bag?.onClick.RemoveAllListeners();
			btn_BagEx?.onClick.RemoveAllListeners();
			btn_Bag2?.onClick.RemoveAllListeners();
			btn_Bag3?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
