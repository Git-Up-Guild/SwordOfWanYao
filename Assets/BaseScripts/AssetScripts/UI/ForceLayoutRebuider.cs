/*******************************************************************
** 文件名:	ForceLayoutRebuider.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2019/10/23 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  滚动偏移层处理

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace XGame.AssetScript.UI
{
    public class ForceLayoutRebuider : MonoBehaviour
    {
        public float delay;

        private void OnEnable()
        {
            if(delay > 0)
            {
                Invoke("Layout", delay);
            }
            else
            {
                Layout();
            }
        }

        private void Layout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}