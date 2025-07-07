
using UnityEditor;
using UnityEngine;
using XGameEditor.Scheme;

namespace XGameEditor
{
#if UNITY_EDITOR1
    [CustomEditor(typeof(HelpTips))]
    internal class HelpTipsEditor : UnityEditor.Editor
    {
        HelpTips _helpTip;
        private void OnEnable()
        {
            _helpTip = (HelpTips)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_helpTip.HelpID <= 0)
            {
                EditorGUILayout.HelpBox("错误的ID", MessageType.Error);
                return;
            }

            var config = EditorGameSchame.Instance.Scheme.HelpTips_0(_helpTip.HelpID);
            if (config == null)
            {
                EditorGUILayout.HelpBox("错误的ID", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("标题", config.szTitle);
            EditorGUILayout.LabelField("内容", config.szContent);
        }
    }

#endif
}
