/*****************************************************************
** 文件名:	TextColoryLibrary.cs
** 版  权:	(C) 冰川网络网络科技
** 创建人:  郑秀程
** 日  期:	2021/5
** 版  本:	1.0
** 描  述:	文本颜色库
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using XGame.Utils;

namespace XGame.TextSetting
{
    [System.Serializable]
    public class TextColorItem : AddressableAsset
    {
        public Color color = Color.white;
        public string desc;
    }

    public class TextColorLibrary : AddressableAssetCollections<TextColorItem> 
    {
        public Color GetColorByName(string name)
        {
            foreach(var item in assets)
            {
                if(item.name == name)
                {
                    return item.color;
                }
            }
            return Color.white;
        }

        public string GetHexColorStringByName(string name)
        {
            var color = GetColorByName(name);
            return ColorUtility.ToHtmlStringRGBA(color);
        }

#if UNITY_EDITOR
        public override void RefreshName()
        {
            foreach (var asset in assets)
            {
                asset.name = "C" + asset.ID.ToString();
            }
        }
#endif

    }

}