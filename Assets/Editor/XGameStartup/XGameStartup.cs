using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XGameEditor
{
    [InitializeOnLoad]
    public class XGameStartup
    {
        static XGameStartup()
        {
            CheckScriptsDll.StartCheck();
            //MiscToolsMenu.CheckUnityVersion();
        }
    }
}
