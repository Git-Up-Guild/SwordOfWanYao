/*****************************************************************
** 文件名:	TextSettingConfig.cs
** 版  权:	(C) 冰川网络网络科技
** 创建人:  郑秀程
** 日  期:	2021/5
** 版  本:	1.0
** 描  述:	文本设置配置
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using XGame.Utils;

namespace XGame.TextSetting
{
    [System.Serializable]
    public class TextSettingConfig : SingletonScriptObject
    {
        public List<PSDTMPTextStyleSettings> tmpTextStyles;
        public TextColorLibrary textColorLibrary;

        public PSDTMPTextStyleSettings GetTextStyleSettings(TMPro.TMP_FontAsset font)
        {
            foreach(var s in tmpTextStyles)
            {
                if(s.sdfFont == font)
                {
                    return s;
                }
            }
            return null;
        }
    }
}