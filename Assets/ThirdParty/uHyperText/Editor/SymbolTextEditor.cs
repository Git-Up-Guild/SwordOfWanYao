using UnityEditor;
using UnityEditor.UI;
using System.Collections.Generic;

namespace WXB
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SymbolText), true)]
    public class SymbolTextEditor : TextEditor
    {
        protected SerializedProperty m_Text;
        protected SerializedProperty m_FontData;
        protected SerializedProperty m_ElementSegment;
        protected SerializedProperty m_MinLineHeight;
        protected SerializedProperty m_isCheckFontY;
        protected SerializedProperty m_LineAlignment;
        protected SerializedProperty m_Background;
        protected SerializedProperty m_BackgroundSizeExtend;
        protected SerializedProperty m_CartonScale;
        protected SerializedProperty m_SpriteScale;
        protected SerializedProperty m_VLineSpace;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            m_ElementSegment = serializedObject.FindProperty("m_ElementSegment");
            m_MinLineHeight = serializedObject.FindProperty("m_MinLineHeight");
            m_isCheckFontY = serializedObject.FindProperty("m_isCheckFontY");
            m_LineAlignment = serializedObject.FindProperty("m_LineAlignment");
            m_Background = serializedObject.FindProperty("m_Background");
            m_BackgroundSizeExtend = serializedObject.FindProperty("m_BackgroundSizeExtend");
            m_CartonScale = serializedObject.FindProperty("m_CartonScale");
            m_SpriteScale = serializedObject.FindProperty("m_SpriteScale");
            m_VLineSpace = serializedObject.FindProperty("m_VLineSpace");
        }

        protected virtual void OnGUIFontData()
        {
            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
        }

        protected virtual void OnGUIOther()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            OnGUIFontData();
            AppearanceControlsGUI();
            RaycastControlsGUI();

            EditorGUILayout.PropertyField(m_LineAlignment);

            // 元素分割类型
            {
                List<string> alles = new List<string>();
                alles.Add("Empty");
                alles.AddRange(ESFactory.GetAllName());
                int current = alles.IndexOf(m_ElementSegment.stringValue);
                if (current == -1)
                    current = 0;

                int[] optionValues = new int[alles.Count];
                for (int i = 0; i < optionValues.Length; ++i)
                    optionValues[i] = i;

                current = EditorGUILayout.IntPopup("Element Segment", current, alles.ToArray(), optionValues);
                if (current <= 0)
                    m_ElementSegment.stringValue = null;
                else
                    m_ElementSegment.stringValue = alles[current];
            }

            // V标记行距
            EditorGUILayout.PropertyField(m_VLineSpace);

            // 最小行高
            EditorGUILayout.PropertyField(m_MinLineHeight);

            // 是否开启字体高度修正
            EditorGUILayout.PropertyField(m_isCheckFontY);

            // 背景对象
            EditorGUILayout.PropertyField(m_Background);

            // 背景尺寸扩展
            EditorGUILayout.PropertyField(m_BackgroundSizeExtend);

            // 表情缩放
            EditorGUILayout.PropertyField(m_CartonScale);

            // 精灵缩放
            EditorGUILayout.PropertyField(m_SpriteScale);

            OnGUIOther();

            if (serializedObject.ApplyModifiedProperties())
            {
                SymbolText st = target as SymbolText;
                st.SetAllDirty();
            }
        }
    }
}