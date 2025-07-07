/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIAllDisciple.cs <FileHead_AutoGenerate>
Author：李美
Date：2025.05.09
Description：弟子列表展示
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using System.Collections.Generic;
using GameScripts.CardDemo.HeroCard;

namespace GameScripts.CardDemo.UI.AllDisciple
{



    public partial class UIAllDisciple : UIWindowEx
    {

	



		protected override void OnInitialize()
		{

		

			base.OnInitialize();


		}

		protected override void OnUpdateUI()
        {
			List<HeroCardData> listData = GameGlobalEx.HeroCard.GetHeroCardData();

            effList_DiscipleInst.SetData(listData);
        }
		
		//@<<< ExecuteEventHandlerGenerator >>>
		private void ON_EVENT_DISCIPLE_NAME_CHANGE(ushort eventID, object context) //@Window
		{
		}
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_ReturnClicked() //@Window 
		{
			base.Close();
		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	
	partial class EffList_DiscipleItem : EffectiveListItem
	{
		
		protected override void OnClear()
		{
		}
		
		protected override void OnUpdateUI()
		{
            HeroCardData data = ItemData as HeroCardData;
			text_Name.text = data.name;
            text_Power.text = "战斗力 "+data.power;

			//显示下标为1的 icon
			imgSwt_Icon.Switch(1);

        }
		
		//@<<< EffList_Disciple_ButtonFuncGenerator >>>
		private void OnBtn_IconClicked() //@EffList_Disciple 
		{
		}

	}
	//@<<< FlexItemGenerator >>>
	
	
}
