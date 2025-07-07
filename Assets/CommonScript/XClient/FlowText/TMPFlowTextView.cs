/*******************************************************************
** 文件名:	TMPFlowTextView.CS
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
using XGame.FlowText;

namespace XClient.FlowText
{
    public class TMPFlowTextView : BaseFlowTextView
    {
        public TextMeshProUGUI textMeshPro;

        protected override void OnInitView()
        {
            base.OnInitView();

            if(textMeshPro != null)
                orginColor = textMeshPro.color;
        }

        protected override void OnSetColor()
        {
            base.OnSetColor();
            if (textMeshPro != null)
                textMeshPro.color = color;
        }

        protected override void OnSetAlpha()
        {
            base.OnSetAlpha();

            if (!isSupportCanvasGroupAlpha && textMeshPro != null)
                textMeshPro.color = color;
        }

        protected override void OnUpdateContent()
        {
            if (string.IsNullOrEmpty(content.text))
                return;

            if (textMeshPro != null)
                textMeshPro.text = content.text;
        }

        protected override void OnResetView()
        {
            base.OnResetView();

            if (textMeshPro != null)
                textMeshPro.color = orginColor;
        }
    }
}