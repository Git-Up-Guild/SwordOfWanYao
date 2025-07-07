using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XGame.Reddot;
using XGameEditor.Reddot;
using XGameEditor.Scheme;

namespace XGameEditor.FunctionOpen
{
    [CustomEditor(typeof(ReddotConfigs))]
    public class ReddotConfigsEditor : Editor
    {
        private ReddotConfigs _configs;

        private void OnEnable()
        {
            _configs = target as ReddotConfigs;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("刷新列表"))
            {
                RefreshConfigs(_configs);
            }
        }

        public static void RefreshConfigs(ReddotConfigs configs)
        {
            configs.Items.Clear();

            var num = EditorGameSchame.Instance.Scheme.Reddot_nums();
            for (var i = 0; i < num; i++)
            {
                var config = EditorGameSchame.Instance.Scheme.Reddot(i);

             //   var item = new ReddotItem(config.iID, config.szName);
              //  configs.Items.Add(item);
            }

            RefreshElementsData(configs);

            configs.Save();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RefreshElementsData(ReddotConfigs configs)
        {
            foreach (var item in configs.Items)
                item.resRefs.Clear();

            List<GameObject> files = new List<GameObject>();
            var absolutePaths = Directory.GetFiles(configs.PrefabPath, "*.prefab", SearchOption.AllDirectories);

            for (int i = 0; i < absolutePaths.Length; i++)
            {
                var path = XGameEditorUtility.ConvertToBaseOnAssetsPath(absolutePaths[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    var eles = prefab.GetComponentsInChildren<UIReddotIcon>(true);
                    foreach (var e in eles)
                    {
                        var comp = e as Component;
                        foreach(var s in e.settings)
                        {
                            if(s.ID > 0)
                                ReddotConfigs.Instance.AddRef(s.ID, path, XGame.Utils.GameObjectUtility.GetGameObjectFullPath(comp.gameObject));
                        }
                    }
                }
            }

            ReddotConfigs.Instance.RefreshDesc();
        }
    }
}
