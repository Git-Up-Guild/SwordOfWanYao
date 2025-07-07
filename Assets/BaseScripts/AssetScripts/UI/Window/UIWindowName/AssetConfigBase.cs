#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.CodeGenerator
{
    public class AssetConfigBase <T> where T : ScriptableObject
    {
        public static string assetConfigPath = string.Empty;
        public static string assetPathHead = string.Empty;
        public static void CreateAssetConfig()
        {
            if (!Directory.Exists(assetConfigPath))
            {
                Directory.CreateDirectory(assetConfigPath);
            }
            string path = assetPathHead + "/" + typeof(T) + ".asset";
            ScriptableObject asset = ScriptableObject.CreateInstance(typeof(T));
            if(asset == null)
            {
                Debug.LogError("create error file:" + typeof(T));
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
                Debug.Log(" creat succeed path:" + path);
            }
        }
    }
}
#endif