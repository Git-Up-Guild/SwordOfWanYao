/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIMain.cs <FileHead_AutoGenerate>
Author：mcaswen
Date：2025.06.28
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using GameScripts.TestDemo.UI.Bag;
using GameScripts.TestDemo.UI.BagEx;

namespace GameScripts.TestDemo.UI.Main
{
    public partial class UIMain : UIWindowEx
    {
        protected override void OnUpdateUI()
        {
        }

        //@<<< ExecuteEventHandlerGenerator >>>
        //@<<< ButtonFuncGenerator >>>
        private void OnBtn_Bag2Clicked() //@Window 
        {
            
            UIWindowManager.Instance.ShowWindow<UIBagEx>();

		}

        private void OnBtn_Bag3Clicked() //@Window 
        {
            
            UIWindowManager.Instance.ShowWindow<UIBagEx>();

		}

        private void OnBtn_BagExClicked() //@Window 
        {
            
            UIWindowManager.Instance.ShowWindow<UIBagEx>();

		}

        private void OnBtn_BagClicked() //@Window 
        {

            UIWindowManager.Instance.ShowWindow<UIBag>();

		}

		
    }
	
	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>
	
	
}
