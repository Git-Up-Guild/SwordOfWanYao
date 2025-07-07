/*****************************************************
** 文 件 名：SelectOneDataSourceSelector
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2024/6/15 15:34:50
** 内容简述：
** 修改记录： 
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using XGame.Utils;

namespace XClient.SelectOne
{
    public class SelectOneDataSourceSelector : MonoBehaviour
    {
        /// <summary>
        /// 选择事件
        /// </summary>
        [Serializable]
        public class SelectEvent : UnityEvent<object, int> { }

        [SerializeField]
        private string m_DataSourceTypePath;

        [SerializeField]
        private int m_OptionCount = 3;

        [SerializeField]
        private SelectEvent m_OnSelect;

        public string DataSourceTypePath => m_DataSourceTypePath;

        public SelectEvent OnSelect => m_OnSelect;

        public int OptionCount => m_OptionCount;

        /// <summary>
        /// 数据源实例
        /// </summary>
        private ISelectOneDataSource m_DataSourceInstance;

        private void OnDestroy()
        {
            ReleaseDataSourceInstance();
            OnSelect.RemoveAllListeners();
        }

        /// <summary>
        /// 销毁转换器
        /// </summary>
        public void ReleaseDataSourceInstance()
        {
            if (m_DataSourceInstance != null)
            {
                m_DataSourceInstance.Release();
                m_DataSourceInstance = null;
            }
        }

        /// <summary>
        /// 创建数据源实例
        /// </summary>
        /// <returns></returns>
        public ISelectOneDataSource GetDataSourceInstance()
        {
            if (m_DataSourceInstance != null)
                return m_DataSourceInstance;

            if (string.IsNullOrEmpty(m_DataSourceTypePath))
            {
                Debug.LogError("未选择数据源！");
                return null;
            }

            var type = ReflectionUtils.GetTypeByTypePath(m_DataSourceTypePath);
            if (type == null)
            {
                Debug.LogError("查找数据源Type失败！");
                return null;
            }

            m_DataSourceInstance = Activator.CreateInstance(type) as ISelectOneDataSource;
            if (m_DataSourceInstance == null)
            {
                Debug.LogError("创建数据源实例失败！CreateInstance Error.");
                return null;
            }

            if (!m_DataSourceInstance.Create())
            {
                Debug.LogError("创建数据源实例失败！Create Error.");
                ReleaseDataSourceInstance();
                return null;
            }

            return m_DataSourceInstance;

        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SelectOneDataSourceSelector))]
    class SelectOneDataSourceSelectorEditor : Editor
    {
        private List<string> m_AllImplementTypes;
        private SerializedProperty prop_DataSourceTypePath;
        private SerializedProperty prop_OnSelect;
        private SerializedProperty prop_OptionCount;

        private void OnEnable()
        {
            prop_DataSourceTypePath = serializedObject.FindProperty("m_DataSourceTypePath");
            prop_OnSelect = serializedObject.FindProperty("m_OnSelect");
            prop_OptionCount = serializedObject.FindProperty("m_OptionCount");

            m_AllImplementTypes = new List<string>();

            var lsType = new List<Type>();
            ReflectionUtils.FindAllImplementsOfType(typeof(ISelectOneDataSource), lsType, null);

            for (var i = 0; i < lsType.Count; i++)
            {
                m_AllImplementTypes.Add(lsType[i].FullName);
            }
        }

        private void OnDisable()
        {
            prop_DataSourceTypePath?.Dispose();
            prop_DataSourceTypePath = null;

            prop_OnSelect?.Dispose();
            prop_OnSelect = null;

            prop_OptionCount?.Dispose();
            prop_OptionCount = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int selectIndex = -1;
            for (var i = 0; i < m_AllImplementTypes.Count; i++)
            {
                if (m_AllImplementTypes[i] == prop_DataSourceTypePath.stringValue)
                {
                    selectIndex = i;
                }
            }

            if (selectIndex == -1)
                selectIndex = 0;

            selectIndex = EditorGUILayout.Popup("选择数据源", selectIndex, m_AllImplementTypes.ToArray());
            if (selectIndex >= 0)
            {
                prop_DataSourceTypePath.stringValue = m_AllImplementTypes[selectIndex];
            }

            EditorGUILayout.PropertyField(prop_OptionCount, new GUIContent("选项数量"));

            EditorGUILayout.PropertyField(prop_OnSelect, new GUIContent("选择回调"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}