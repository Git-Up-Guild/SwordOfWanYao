/**************************************************************************    
文　　件：SingletonScriptableObjectEditorWindow
作　　者：郑秀程
创建时间：2021/1/17 15:37:37
描　　述：
***************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XGame.Utils;
using XGameEditor;
using XGameEditor.Config;

namespace XGameEditor.AddressableAsset
{
    class SingletonScriptableObjectEditorWindow : EditorWindow
    {
        [MenuItem("XGame/配置资源/SingletonScriptableObject")]
        public static void ShowScriptableObject()
        {
            var window = GetWindow<SingletonScriptableObjectEditorWindow>("SingletonScriptableObject");
            window.maxSize = new Vector2(550, 200);
            window.maxSize = new Vector2(550, 400);
            window.Show();
        }

        private List<System.Type> m_lsTypes;
        private int m_selectType = 0;
        private int m_selectDir = 0;
        private List<string> m_lsFindPath = new List<string>();

        private void OnEnable()
        {
            m_lsTypes = new List<Type>();
            XGameEditorUtilityEx.FindAllSubClassTyps(typeof(SingletonScriptObject), m_lsTypes);

            m_lsFindPath.Clear();
            m_lsFindPath.Add(XGameEditorConfig.editorSettingsSaveDir.Replace("/", "\\"));
            m_lsFindPath.Add(XGameEditorConfig.Instance.gameSettingsSaveDir.Replace("/", "\\"));
            m_lsFindPath.Add("和脚本同目录");
        }

        private void OnGUI()
        {
            string[] names = new string[m_lsTypes.Count];
            for(var i = 0; i < m_lsTypes.Count; ++i)
            {
                names[i] = m_lsTypes[i].FullName;
            }

            string tips = $"只支持在以下目录下查找或者创建：\n1. {XGameEditorConfig.editorSettingsSaveDir};\n1. {XGameEditorConfig.Instance.gameSettingsSaveDir};\n3. 和脚本同目录;";
            EditorGUILayout.HelpBox(tips, MessageType.Info);

            m_selectType = EditorGUILayout.Popup("请先选择类型", m_selectType, names);

            ScriptableObject obj = XGameEditorUtilityEx.GetEditorScriptableObject(m_lsTypes[m_selectType], false);
            if(obj == null)
                obj = XGameEditorUtilityEx.GetEditorScriptableObject(m_lsTypes[m_selectType], false, m_lsFindPath[0]);
            if(obj == null)
                obj = XGameEditorUtilityEx.GetEditorScriptableObject(m_lsTypes[m_selectType], false, m_lsFindPath[1]);

            if(obj != null)
            {
                EditorGUILayout.ObjectField("找到目标对象", obj, obj.GetType(), false);

                if (GUILayout.Button("查找"))
                {
                    XGameEditorUtilityEx.PingObject(obj);
                }
                return;
            }

            //先检查是否已经存在
            m_lsFindPath[2] = XGameEditorUtilityEx.GetCSharpScriptDir(m_lsTypes[m_selectType]).Replace("/","\\");

            //请选中要创建的目录
            m_selectDir = EditorGUILayout.Popup("选择存放目录", m_selectDir, m_lsFindPath.ToArray());

            if (GUILayout.Button("创建"))
            {
                obj = XGameEditorUtilityEx.GetEditorScriptableObject(m_lsTypes[m_selectType], true, m_lsFindPath[m_selectDir]);
                XGameEditorUtilityEx.PingObject(obj);
            }
        }
    }
}
