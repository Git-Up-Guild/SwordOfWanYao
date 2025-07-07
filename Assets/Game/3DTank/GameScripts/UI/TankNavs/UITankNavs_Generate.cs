/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UITankNavs.cs <FileHead_AutoGenerate>
Author：许德纪
Date：2025.05.09
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.TglBtn;
//@End_NameSpace

namespace GameScripts.Tank.UI.TankNavs
{

    public partial class UITankNavs : UIWindowEx
    {
		public static UITankNavs Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private ToggleButtonGroup tglBtnGroup_NavButtons = null;
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
            ResPath = "Game/3DTank/GameResources/Prefabs/UI/UITankNavs.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			tglBtnGroup_NavButtons = Meta.Widgets.GetWidgetComponent<ToggleButtonGroup>(0);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			tglBtnGroup_NavButtons = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			tglBtnGroup_NavButtons?.AddChangeListener(OnTglBtnGroup_NavButtonsChanged);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			tglBtnGroup_NavButtons.RemoveChangeListener(OnTglBtnGroup_NavButtonsChanged);
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
