/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIBag.cs <FileHead_AutoGenerate>
Author：mcaswen
Date：2025.06.28
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using System.Collections.Generic;
using GameScripts.TestDemo.ZCCard;

namespace GameScripts.TestDemo.UI.Bag
{
    public partial class UIBag : UIWindowEx
    {
        protected override void OnUpdateUI()
        {

            effList_BagListInst.SetData(GameGlobalEx.ZCCard.GetAllExams());

        }

		//@<<< ExecuteEventHandlerGenerator >>>
		private void EVENT_CARD_DATA_UPDATE(ushort eventID, object context) //@Window
		{
		}
        private void ON_EVENT_CARD_DATA_UPDATE(ushort eventID, object context) //@Window
        {

            OnUpdateUI();

		}
		//@<<< ButtonFuncGenerator >>>
        private void OnBtn_CloseClicked() //@Window 
        {

            this.Close();

		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	
	partial class EffList_BagListItem : EffectiveListItem
	{
		
		protected override void OnClear()
		{
		}

		protected override void OnUpdateUI()
		{
			Exam data = ItemData as Exam;

			text_Name.text = data.CourseName;
			text_Time.text = data.Time;
			text_TeacherName.text = data.TeacherName;
			text_Location.text = data.Location;

		}
		
		//@<<< EffList_BagList_ButtonFuncGenerator >>>
	}
	//@<<< FlexItemGenerator >>>
	
	
}
