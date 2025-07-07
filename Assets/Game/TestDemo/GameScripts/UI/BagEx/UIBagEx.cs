/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIBagEx.cs <FileHead_AutoGenerate>
Author：李美
Date：2025.06.28
Description：弟子列表展示
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using GameScripts.TestDemo.ZCCard;

namespace GameScripts.TestDemo.UI.BagEx
{
    public partial class UIBagEx : UIWindowEx
    {
		protected override void OnUpdateUI()
		{

			effList_BagExInst.SetData(GameGlobalEx.ZCCard.GetAllCards());

        }

		//@<<< ExecuteEventHandlerGenerator >>>
		private void ON_EVENT_CARD_DATA_UPDATE(ushort eventID, object context) //@Window
		{

			OnUpdateUI();

		}
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_ReturnClicked() //@Window 
		{

			this.Close();

		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	
	partial class EffList_BagExItem : EffectiveListItem
	{
		
		protected override void OnClear()
		{
		}
		
		protected override void OnUpdateUI()
		{
		}
		
		//@<<< EffList_BagEx_ButtonFuncGenerator >>>
		private void OnBtn_IconClicked() //@EffList_BagEx 
		{
		}

	}
	
	partial class EffList_DiscipleItem : EffectiveListItem
	{
		
		protected override void OnClear()
		{
		}

		protected override void OnUpdateUI()
		{
			Card cardData = this.ItemData as Card;
            text_Name.text = cardData.Name;
            imgSwt_Icon.Switch(cardData.ID % 6);

		}
		
		//@<<< EffList_Disciple_ButtonFuncGenerator >>>
		private void OnBtn_IconClicked() //@EffList_Disciple 
		{
		}

	}
	//@<<< FlexItemGenerator >>>
	
	
}
