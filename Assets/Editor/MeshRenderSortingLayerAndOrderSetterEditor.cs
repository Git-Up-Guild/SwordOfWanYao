using UnityEditor;
using UnityEngine;
using XGame.Utils;

namespace XGameEditor
{
    [CustomEditor(typeof(MeshRenderSortingLayerAndOrderSetter))]
    public class MeshRenderSortingLayerAndOrderSetterEditor : Editor
    {
        SerializedProperty m_PropLayerIndex;
        SerializedProperty m_PropSortingOrder;
        SerializedProperty m_PropAutoDestory;
        string[] m_LayerNames;

        private void OnEnable()
        {
            m_PropLayerIndex = serializedObject.FindProperty("m_LayerIndex");
            m_PropSortingOrder = serializedObject.FindProperty("m_SortingOrder");
            m_PropAutoDestory = serializedObject.FindProperty("m_AutoDestory");

            m_LayerNames = new string[SortingLayer.layers.Length];
            for (int i = 0; i < SortingLayer.layers.Length; i++)
                m_LayerNames[i] = SortingLayer.layers[i].name;

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if(m_PropLayerIndex.intValue < 0 || m_PropLayerIndex.intValue >= m_LayerNames.Length)
            {
                m_PropLayerIndex.intValue = 0;
            }

            m_PropLayerIndex.intValue = EditorGUILayout.Popup("Sorting Layer", m_PropLayerIndex.intValue, m_LayerNames);
            EditorGUILayout.PropertyField(m_PropSortingOrder);
            EditorGUILayout.PropertyField(m_PropAutoDestory);

            serializedObject.ApplyModifiedProperties();
        }
    }
}