
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace XGame.UI
{
    [CustomEditor(typeof(ButtonEx))]
    public class ButtonExEditor : UnityEditor.UI.ButtonEditor
    {
        private ButtonEx Target;
        private ReorderableList reorderableList;

        protected override void OnEnable()
        {
            base.OnEnable();
            Target = target as ButtonEx;
            reorderableList = new ReorderableList(Target.eventContexList, Target.eventContexList.GetType(), true, false, true, true);
            reorderableList.drawElementCallback += new ReorderableList.ElementCallbackDelegate(DrawEventContextItem);
            reorderableList.drawHeaderCallback += new ReorderableList.HeaderCallbackDelegate((rect) => { EditorGUI.LabelField(rect, "特殊事件列表"); });
            reorderableList.elementHeightCallback += new ReorderableList.ElementHeightCallbackDelegate(DrawEventContextHigh);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

        }

        private void DrawEventContextItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            GUIStyle style = EditorStyles.label;
            style.richText = true;

            var data = Target.eventContexList[index];
            if (data != null)
            {
                data.id = index;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "ID", data.id.ToString());
                rect.y += EditorGUIUtility.singleLineHeight;
                data.eventType = (EnBtnExEventType)EditorGUI.EnumPopup(rect, "事件类型", data.eventType);
                rect.y += EditorGUIUtility.singleLineHeight;

                switch (data.eventType)
                {
                    case EnBtnExEventType.LongPress:
                        {
                            data.maxDuration = EditorGUI.FloatField(rect, "最长持续时间", data.maxDuration);
                        }
                        break;
                    case EnBtnExEventType.MorePress:
                        {
                            data.totalClickCount = EditorGUI.IntField(rect, "点击次数", data.totalClickCount);
                            rect.y += EditorGUIUtility.singleLineHeight;
                            data.interval = EditorGUI.FloatField(rect, "最长间隔时间", data.interval);
                            rect.y += EditorGUIUtility.singleLineHeight;
                            data.maxDuration = EditorGUI.FloatField(rect, "最长持续时间", data.maxDuration);
                            rect.y += EditorGUIUtility.singleLineHeight;
                            EditorGUI.LabelField(rect, "当前点击次数", data.curClickCount.ToString());
                            rect.y += EditorGUIUtility.singleLineHeight;

                        }
                        break;
                }
            }
        }

        private float DrawEventContextHigh(int index)
        {
            var data = Target.eventContexList[index];
            if (data == null)
                return 0;
            float high = EditorGUIUtility.singleLineHeight;
            switch (data.eventType)
            {
                case EnBtnExEventType.LongPress:
                    high = EditorGUIUtility.singleLineHeight * 3;
                    break;
                case EnBtnExEventType.MorePress:
                    high = EditorGUIUtility.singleLineHeight * 6;
                    break;
                default:
                    break;
            }
            return high;
        }
    }


}
