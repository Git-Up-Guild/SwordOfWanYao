using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XGame.Attr;
using XGame.UI.Framework;
using XGame.Utils;

namespace XGame.UI.Framework
{
    [CreateAssetMenu(menuName = "XGame/UIWindowExSetting")]
    public class UIWindowExSetting : ScriptableObject
    {
        //public int a;
        //常驻窗口
        [Tooltip("常驻窗口 刷新")]
        public List<string> residentWindows = new List<string>();
        //特殊窗口
        [Tooltip("特殊窗口 自己处理")]
        public List<string> specWindows = new List<string>();

        private bool IsExistInResi(UIWindow window)
        {
            for (int i = 0; i < residentWindows.Count; i++)
            {
                if (residentWindows[i] == window.Settings.Name)
                    return true;
            }
            return false;

        }
    }
}
