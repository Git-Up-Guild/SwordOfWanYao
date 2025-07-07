/*******************************************************************
** 文件名:	TextMeshProPreferreWidthSyncer.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2019/10/23 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  TextMeshProUGUI 的宽度同步

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using TMPro;
using UnityEngine;
using XGame.Attr;

namespace XGame.AssetScript.UI
{
    public class TextMeshProPreferreWidthSyncer : MonoBehaviour
    {
        [AutoBind(typeof(TextMeshProUGUI))]
        public TextMeshProUGUI text;

        //最大宽度
        public float maxWidth;

        //最小宽度
        public float minWidth;

        private RectTransform rectTransform { get; set; }

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        private void Update()
        {
            if(maxWidth > 0 || minWidth > 0)
            {
                var newWidth = text.preferredWidth;
                newWidth = Mathf.Clamp(newWidth, minWidth, maxWidth);

                var sizeDelta = rectTransform.sizeDelta;
                if (newWidth != sizeDelta.x)
                    rectTransform.sizeDelta = new Vector2(newWidth, sizeDelta.y);
            }
        }
    }
}