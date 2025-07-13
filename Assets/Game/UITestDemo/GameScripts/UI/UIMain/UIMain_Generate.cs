/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIMain.cs <FileHead_AutoGenerate>
Author：LIN
Date：2025.07.12
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.UITsetDemo.UI.Main
{

    public partial class UIMain : UIWindowEx
    {
		public static UIMain Instance { get; private set; }
	    
		//@Begin_Widget_Variables
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
            ResPath = "Game/UITestDemo/GameResources/Prefabs/UI/Image.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
