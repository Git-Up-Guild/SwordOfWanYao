using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace XGameEditor.ResTools
{
    public class SkinPrefabOptimizer : MonoBehaviour
    {

        [MenuItem("XGame/Res Tools/Skin/Optimize Select Skin Prefabs")]
        public static void OptimizeSelectSkinPrefabs()
        {
            Object[] prefabs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            foreach (var item in prefabs) {
                if (item is GameObject) {
                    GameObject prefab = item as GameObject;
                    Renderer smr = prefab.GetComponentInChildren<Renderer>();
                    GameObject newSkin = GameObject.Instantiate(smr.gameObject);
                    newSkin.transform.localPosition = Vector3.zero;
                    newSkin.transform.LocalRotationEx(Quaternion.identity);
                    PrefabUtility.ReplacePrefab(newSkin, prefab);
                    DestroyImmediate(newSkin);
                }
            }//foreach end

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}