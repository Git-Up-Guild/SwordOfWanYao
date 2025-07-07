/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIBag.cs <FileHead_AutoGenerate>
Author：mcaswen
Date：2025.06.28
Description：笑哭笑哭笑哭
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.EffList;
using TMPro;
//@End_NameSpace

namespace GameScripts.TestDemo.UI.Bag
{

    public partial class UIBag : UIWindowEx
    {
		public static UIBag Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Button btn_Close = null;
		private RectTransform tran_ListRoot = null;
		private EffectiveListView effList_BagList = null;
		private EffectiveList effList_BagListInst = null;
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
            ResPath = "Game/TestDemo/GameResources/Prefabs/UI/UIBag.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			btn_Close = Meta.Widgets.GetWidgetComponent<Button>(0);
			tran_ListRoot = Meta.Widgets.GetWidgetComponent<RectTransform>(1);
			effList_BagList = Meta.Widgets.GetWidgetComponent<EffectiveListView>(2);
			effList_BagListInst = UIFrameworkFactory.Instance.CreateEffectiveList<EffList_BagListItem>(effList_BagList);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			UIFrameworkFactory.Instance.ReleaseEffectiveList(effList_BagListInst);
			btn_Close = null;
			tran_ListRoot = null;
			effList_BagListInst = null;
			effList_BagList = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_Close?.onClick.AddListener(OnBtn_CloseClicked);
			EventSubscriber?.AddHandler(DGlobalEventEx.EVENT_CARD_DATA_UPDATE, ON_EVENT_CARD_DATA_UPDATE, "UIBag");
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_Close?.onClick.RemoveAllListeners();
			EventSubscriber?.RemoveHandler(DGlobalEventEx.EVENT_CARD_DATA_UPDATE, ON_EVENT_CARD_DATA_UPDATE);
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	partial class EffList_BagListItem : EffectiveListItem
	{
		//@Begin_EffList_BagList_Widget_Variables
		private Image img_Frame = null;
		private TextMeshProUGUI text_Name = null;
		private Image img_Ele1 = null;
		private Image img_Ele2 = null;
		private Image img_Description = null;
		private TextMeshProUGUI text_Time = null;
		private TextMeshProUGUI text_TeacherName = null;
		private TextMeshProUGUI text_Location = null;
		private TextMeshProUGUI text_ExamMethod = null;
		//@End_EffList_BagList_Widget_Variables
		
		protected override void InitWidgets() //@EffList_BagList 
		{
			img_Frame = Meta.Widgets.GetWidgetComponent<Image>(0);
			text_Name = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(1);
			img_Ele1 = Meta.Widgets.GetWidgetComponent<Image>(2);
			img_Ele2 = Meta.Widgets.GetWidgetComponent<Image>(3);
			img_Description = Meta.Widgets.GetWidgetComponent<Image>(4);
			text_Time = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(5);
			text_TeacherName = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(6);
			text_Location = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(7);
			text_ExamMethod = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(8);
			OnInitWidgets();
		} //@End_EffList_BagList_InitWidgets

		protected override void ClearWidgets() //@EffList_BagList 
		{
			OnClearWidgets();
			img_Frame = null;
			text_Name = null;
			img_Ele1 = null;
			img_Ele2 = null;
			img_Description = null;
			text_Time = null;
			text_TeacherName = null;
			text_Location = null;
			text_ExamMethod = null;
		} //@End_EffList_BagList_ClearWidgets
					
		protected override void SubscribeEvents() //@EffList_BagList 
		{
			UnsubscribeEvents();
			OnSubscribeEvents();
		} //@End_EffList_BagList_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@EffList_BagList 
		{
			OnUnsubscribeEvents();
		} //@End_EffList_BagList_UnsubscribeEvents
		
	}
	
	
}
