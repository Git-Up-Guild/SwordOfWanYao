using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ContentSizeFitterEx), true)]
    [CanEditMultipleObjects]

    public class ContentSizeFitterExEditor : ContentSizeFitterEditor
    {
        SerializedProperty m_MinWidth;
        SerializedProperty m_MaxWidth;
        SerializedProperty m_MinHeight;
        SerializedProperty m_MaxHeight;

        ContentSizeFitterEx m_Target;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Target = target as ContentSizeFitterEx;

            m_MinWidth = serializedObject.FindProperty("m_MinWidth");
            m_MaxWidth = serializedObject.FindProperty("m_MaxWidth");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
            m_MaxHeight = serializedObject.FindProperty("m_MaxHeight");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            if(m_Target.horizontalFit != ContentSizeFitter.FitMode.Unconstrained)
            {
                EditorGUILayout.PropertyField(m_MinWidth, true);
                EditorGUILayout.PropertyField(m_MaxWidth, true);
            }

            if (m_Target.verticalFit != ContentSizeFitter.FitMode.Unconstrained)
            {
                EditorGUILayout.PropertyField(m_MinHeight, true);
                EditorGUILayout.PropertyField(m_MaxHeight, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
