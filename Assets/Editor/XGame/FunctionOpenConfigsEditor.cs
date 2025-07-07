using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using XClient.Common;
using XGame.FunctionOpen;
using XGameEditor.Scheme;

namespace XGameEditor.FunctionOpen
{
    [CustomEditor(typeof(FunctionOpenConfigs))]
    public class FunctionOpenConfigsEditor : Editor
    {

        private FunctionOpenConfigs _configs;

        private void OnEnable()
        {
            _configs = target as FunctionOpenConfigs;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("刷新功能列表"))
            {
                _configs.Items.Clear();

                /*
                var num = EditorGameSchame.Instance.Scheme.FunctionOpen_nums();
                for (var i = 0; i < num; i++)
                {
                    var config = EditorGameSchame.Instance.Scheme.FunctionOpen(i);

                    var item = new FunctionItem(config.iID, config.szName);
                    _configs.Items.Add(item);
                }
                */
                RefreshElementsData();

                _configs.Save();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void RefreshElementsData()
        {
            foreach (var item in _configs.Items)
                item.resRefs.Clear();

            List<GameObject> files = new List<GameObject>();
            var absolutePaths = Directory.GetFiles(_configs.PrefabPath, "*.prefab", SearchOption.AllDirectories);

            for (int i = 0; i < absolutePaths.Length; i++)
            {
                var path = XGameEditorUtility.ConvertToBaseOnAssetsPath(absolutePaths[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    var eles = prefab.GetComponentsInChildren<IFunctionElement>(true);
                    foreach (var e in eles)
                    {
                        var comp = e as Component;
                        FunctionOpenConfigs.Instance.AddRef(e.FuncID, path, XGame.Utils.GameObjectUtility.GetGameObjectFullPath(comp.gameObject));
                    }
                }
            }

            FunctionOpenConfigs.Instance.RefreshDesc();
        }
    }
}
