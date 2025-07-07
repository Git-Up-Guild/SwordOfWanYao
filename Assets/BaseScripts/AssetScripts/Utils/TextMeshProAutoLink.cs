/*******************************************************************
** 文件名: TextMeshProAutoLink.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 代文鹏 
** 日  期: 2020/xx/xx
** 版  本: 1.0
** 描  述: 
** 应  用: 
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/

using XClient.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using XGame.UI;

public class TextMeshProAutoLink : MonoBehaviour, IPointerClickHandler
{
    //链接格式 <color=blue><u><link="https://www.baidu.com">百度</link></u></color>
    private TextMeshProUGUI text;

    private void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (text)
        {
            Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, 0);
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, pos, UICamera.uiCamera); //UI相机
            if (linkIndex > -1)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
                string url = linkInfo.GetLinkID();
                if (url != null)
                {
                    url = url.Replace("\'", "").Replace("/'", "").Trim();
                }
                Debug.Log("打开了超链接：" + url);
                Application.OpenURL(url);
            }
        }
    }


}
