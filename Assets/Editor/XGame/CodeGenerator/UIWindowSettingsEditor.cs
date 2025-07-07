#if SUPPORT_LOP

using UnityEditor;

using UnityEngine;
using XGame.AssetScript.UI;
using XGameEditor.I18N;
using XGameEditor.TextSetting;

namespace XGameEditor.CodeGenerator
{
    [CustomEditor(typeof(LOPUIWindowMetaEx))]
    public class LOPUIWindowMetaExEditor : LOPUIWindowMetaEditor
    {
        protected override void OnI18NPrefabCheckerWndShow(GameObject go)
        {
#if SUPPORT_I18N
            I18NPrefabCheckerWindow.ShowWindow(go);
#endif
        }

        protected override void OnTextStyleBatchSettingEditorWndShow(GameObject go)
        {
            TextStyleBatchSettingEditorWnd.ShowWindow(go.transform);
        }
    }
}

#endif
