using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using XGame.UI;

namespace XGameEditor.UI
{
    [CustomEditor(typeof(VerticalLayoutGroupEx))]
    public class VerticalLayoutGroupExEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        SerializedProperty m_ContentSizeFiltter;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ContentSizeFiltter = serializedObject.FindProperty("m_ContentSizeFiltter");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_ContentSizeFiltter, true);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(HorizontalLayoutGroupEx))]
    public class HorizontalLayoutGroupExEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        SerializedProperty m_ContentSizeFiltter;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ContentSizeFiltter = serializedObject.FindProperty("m_ContentSizeFiltter");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_ContentSizeFiltter, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}