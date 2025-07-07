/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIMainNavs.cs <FileHead_AutoGenerate>
Author：甘炳钧
Date：2025.05.09
Description：主界面导航
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using GameScripts.CardDemo.UI.AllDisciple;

namespace GameScripts.CardDemo.UI.MainNavs
{
    public partial class UIMainNavs : UIWindowEx
    {
        protected override void OnUpdateUI()
        {
        }
		
		//@<<< ExecuteEventHandlerGenerator >>>
		//@<<< ButtonFuncGenerator >>>
		private void OnTglBtnGroup_NavButtonsChanged(int idx, int val) //@Window 
		{

			if(idx==0)
			{
				UIWindowManager.Instance.ShowWindow<UIAllDisciple>();
			}

		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>
	
	
}
