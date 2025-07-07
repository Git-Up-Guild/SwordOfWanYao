/**************************************************************************    
文　　件：EditorWindowWithTips
作　　者：郑秀程
创建时间：2020/1/30 12:29:59
描　　述：
***************************************************************************/
using UnityEditor;
using UnityEngine;

namespace XGameEditor.EditorUI
{
    public class EditorWindowWithTips : EditorWindow
    {
        protected OperateTips operateTip;

        public EditorWindowWithTips()
        {
            operateTip = new OperateTips();
        }

        virtual public void ShowTips(string tips)
        {
            operateTip.ShowTips(tips, new Rect(0, 0, position.width, position.height));
        }

        virtual public void OnGUI()
        {
            operateTip.Update();
        }

    }
}
