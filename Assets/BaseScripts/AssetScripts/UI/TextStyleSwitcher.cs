/*******************************************************************
** 文件名:	TextStyleSwitcher.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2019/10/23 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Attr;
using XGame.MonoState;

namespace XGame.AssetScript.UI
{
    public class TextStyleSwitcher : BaseMonoStateSwitcher
    {
        [AutoBind(typeof(TextMeshProUGUI))]
        public TextMeshProUGUI tmpText;

        [AutoBind(typeof(Text))]
        public Text text;

        //颜色代码
        public string[] colorNames;

        //颜色
        public Color[] colors;

        //材质风格列表
        public string[] matStyles;

        //材质列表
        public Material[] tmpMats;

        public override int stateCount => Mathf.Max(colorNames.Length, matStyles.Length, tmpMats.Length);

        protected override void OnStateChange()
        {
            if (tmpText == null && text == null)
                return;

            //颜色代码
            bool isColorSetted = false;
            if (stateIndex < colorNames.Length)
            {
                var code = colorNames[stateIndex];
                Color c = ColorNameToColor(code);

                isColorSetted = true;

                if (text != null)
                {
                    text.color = c;
                }

                if (tmpText != null)
                {
                    tmpText.color = c;
                }
            }

            //切换颜色
            if (!isColorSetted && stateIndex < colors.Length)
            {
                var c = colors[stateIndex];
                if(text != null)
                {
                    text.color = c;
                }

                if(tmpText != null)
                {
                    tmpText.color = c;
                }
            }

            //风格名称
            bool isMaterialSetted = false;
            if (tmpText != null && stateIndex < matStyles.Length)
            {
                var styleName = matStyles[stateIndex];
                if(!string.IsNullOrEmpty(styleName))
                {
                    isMaterialSetted = SetTMPTextMaterialFormStyleName(tmpText, styleName);
                }
            }

            //切换材质
            if (!isMaterialSetted && tmpText != null && stateIndex < tmpMats.Length)
            {
                tmpText.fontMaterial = tmpMats[stateIndex];
            }
        }

        public static bool SetTMPTextMaterialFormStyleName(TextMeshProUGUI tmpText, string styleName)
        {
            /*
            ScriptableObjectAssets scriptableObjectAssets = ScriptableObjectAssets.Instance;
            if (scriptableObjectAssets == null || scriptableObjectAssets.textSettingConfig == null)
                return false;

            TextSettingConfig textSettingConfig = scriptableObjectAssets.textSettingConfig;
            foreach (var style in textSettingConfig.tmpTextStyles)
            {
                if (style.sdfFont == tmpText.font)
                {
                    var setting = style.GetStyle(styleName);
                    if (setting != null)
                    {
                        tmpText.fontMaterial = setting.fontMat;
                        if (setting.colorGrandient != null)
                        {
                            tmpText.enableVertexGradient = true;
                            tmpText.colorGradientPreset = setting.colorGrandient;
                        }
                        return true;
                    }
                }
            }
            */
            return false;
        }

        public static Color ColorNameToColor(string colorName)
        {
            /*
            ScriptableObjectAssets scriptableObjectAssets = ScriptableObjectAssets.Instance;
            if (scriptableObjectAssets == null || scriptableObjectAssets.textSettingConfig == null)
                return Color.white;

            TextSettingConfig textSettingConfig = scriptableObjectAssets.textSettingConfig;
            TextColorLibrary colorLibrary = textSettingConfig.textColorLibrary;
            if (colorLibrary == null)
                return Color.white;

            return colorLibrary.GetColorByName(colorName);
            */

            return new Color();

        }
    }
}