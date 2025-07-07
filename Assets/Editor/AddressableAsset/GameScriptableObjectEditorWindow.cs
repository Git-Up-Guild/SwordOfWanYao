/**************************************************************************    
文　　件：AddressableAssetCollectionsEditorWindow
作　　者：郑秀程
创建时间：2021/1/17 15:37:37
描　　述：
***************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.AddressableAsset
{
    class GameScriptableObjectEditorWindow : EditorWindow
    {
        [MenuItem("XGame/配置资源/创建配置资源")]
        public static void CreateAssetCollections()
        {
            GameScriptableObjectEditorWindow window = GetWindow<GameScriptableObjectEditorWindow>();
            window.maxSize = new Vector2(550, 200);
            window.maxSize = new Vector2(550, 400);
            window.Show();
        }

        private List<System.Type> m_lsTypes;
        private int m_selectType = 0;

        private void OnEnable()
        {
            m_lsTypes = new List<Type>();
            XGameEditorUtilityEx.FindAllSubClassTyps(typeof(XGame.Utils.GameScriptableObject), m_lsTypes);
        }

        private void OnGUI()
        {
            string[] names = new string[m_lsTypes.Count];
            for(var i = 0; i < m_lsTypes.Count; ++i)
            {
                names[i] = m_lsTypes[i].FullName;
            }

            m_selectType = EditorGUILayout.Popup("选择要创建的类型", m_selectType, names);

            if(GUILayout.Button("创建"))
            {
                var asset = ScriptableObject.CreateInstance(m_lsTypes[m_selectType].FullName);
                string path = EditorUtility.SaveFilePanelInProject("保存资源", m_lsTypes[m_selectType].Name, "asset", "选择要保存的文件！");
                AssetDatabase.CreateAsset(asset, path);
                EditorGUIUtility.PingObject(asset);
            }
        }
    }
}
