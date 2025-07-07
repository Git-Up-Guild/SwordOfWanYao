/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIAllDisciple.cs <FileHead_AutoGenerate>
Author：李美
Date：2025.05.09
Description：弟子列表展示
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.EffList;
using XGame.UI;
using TMPro;
//@End_NameSpace

namespace GameScripts.CardDemo.UI.AllDisciple
{

    public partial class UIAllDisciple : UIWindowEx
    {
		public static UIAllDisciple Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private RectTransform tran_ListRoot = null;
		private EffectiveListView effList_Disciple = null;
		private EffectiveList effList_DiscipleInst = null;
		private Button btn_Return = null;
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
            ResPath = "Game/2DCardDemo/GameResources/Prefabs/UI/UIAllDisciple.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			tran_ListRoot = Meta.Widgets.GetWidgetComponent<RectTransform>(0);
			effList_Disciple = Meta.Widgets.GetWidgetComponent<EffectiveListView>(1);
			btn_Return = Meta.Widgets.GetWidgetComponent<Button>(2);
			effList_DiscipleInst = UIFrameworkFactory.Instance.CreateEffectiveList<EffList_DiscipleItem>(effList_Disciple);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			UIFrameworkFactory.Instance.ReleaseEffectiveList(effList_DiscipleInst);
			tran_ListRoot = null;
			effList_DiscipleInst = null;
			effList_Disciple = null;
			btn_Return = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_Return?.onClick.AddListener(OnBtn_ReturnClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_Return?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	partial class EffList_DiscipleItem : EffectiveListItem
	{
		//@Begin_EffList_Disciple_Widget_Variables
		private SpriteSwitcher imgSwt_Icon = null;
		private Button btn_Icon = null;
		private Image img_Icon = null;
		private SpriteSwitcher imgSwt_Prop = null;
		private TextMeshProUGUI text_Name = null;
		private TextMeshProUGUI text_Power = null;
		private TextMeshProUGUI text_ExamLvl = null;
		private Image img_WanderState = null;
		//@End_EffList_Disciple_Widget_Variables
		
		protected override void InitWidgets() //@EffList_Disciple 
		{
			imgSwt_Icon = Meta.Widgets.GetWidgetComponent<SpriteSwitcher>(0);
			btn_Icon = Meta.Widgets.GetWidgetComponent<Button>(1);
			img_Icon = Meta.Widgets.GetWidgetComponent<Image>(2);
			imgSwt_Prop = Meta.Widgets.GetWidgetComponent<SpriteSwitcher>(3);
			text_Name = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(4);
			text_Power = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(5);
			text_ExamLvl = Meta.Widgets.GetWidgetComponent<TextMeshProUGUI>(6);
			img_WanderState = Meta.Widgets.GetWidgetComponent<Image>(7);
			OnInitWidgets();
		} //@End_EffList_Disciple_InitWidgets

		protected override void ClearWidgets() //@EffList_Disciple 
		{
			OnClearWidgets();
			imgSwt_Icon = null;
			btn_Icon = null;
			img_Icon = null;
			imgSwt_Prop = null;
			text_Name = null;
			text_Power = null;
			text_ExamLvl = null;
			img_WanderState = null;
		} //@End_EffList_Disciple_ClearWidgets
					
		protected override void SubscribeEvents() //@EffList_Disciple 
		{
			UnsubscribeEvents();
			btn_Icon?.onClick.AddListener(OnBtn_IconClicked);
			OnSubscribeEvents();
		} //@End_EffList_Disciple_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@EffList_Disciple 
		{
			OnUnsubscribeEvents();
			btn_Icon?.onClick.RemoveAllListeners();
		} //@End_EffList_Disciple_UnsubscribeEvents
		
	}
	
	
}
