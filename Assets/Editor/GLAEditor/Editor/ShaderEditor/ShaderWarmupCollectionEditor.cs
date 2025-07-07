using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using XClient.Game;

namespace XGameEditor
{
    [CustomEditor(typeof(ShaderCollectionWarmUp))]
    public class ShaderWarmupCollectionEditor : Editor
    {
        protected ShaderCollectionWarmUp targetObject;

        [MenuItem("XGame/Shader/CreateShaderWarmupCollection")]
        public static void CreateShaderWarmupCollection()
        {
            ShaderCollectionWarmUp inst = ScriptableObject.CreateInstance<ShaderCollectionWarmUp>();

            string[] strs = Selection.assetGUIDs;
            if(strs.Length  == 0)
                return;
            string path = AssetDatabase.GUIDToAssetPath(strs[0]);
            if (string.IsNullOrEmpty(path))
                return;

            AssetDatabase.CreateAsset(inst, path + "/NewShaderCollectionWarmUp.asset");
        }

        public void OnEnable()
        {
            targetObject = target as ShaderCollectionWarmUp;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Refresh Shaders", GUILayout.MinWidth(100f)))
            {
                targetObject.shaders.Clear();

                if (targetObject.shaderVariantCollections == null)
                {
                    return;
                }

                var svcSize = targetObject.shaderVariantCollections.Length;
                for (int i = 0; i < svcSize; ++i)
                {
                    var shaderVariantCollection = targetObject.shaderVariantCollections[i];
                    SerializedProperty shadersProperty = new SerializedObject(shaderVariantCollection).FindProperty("m_Shaders");

                    List<Shader> shaders = GetShaders(shadersProperty);
                    targetObject.shaders.AddRange(shaders);

                    //获取每一个变体收集器对应的Shader
                    //var shaderNames = GetShaderNames(shadersProperty);

                    ////变体收集器的路径
                    //var assetPath = AssetDatabase.GetAssetPath(shaderVariantCollection);
                    //AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
                    //var assetBundleName = assetImporter.assetBundleName;

                    ////获取变体收集器对应的Shader
                    //var shaderObjs = LoadAllAssetsByBundleName(assetBundleName);
                    //if (shaderObjs != null)
                    //{
                    //    foreach (var shaderObj in shaderObjs)
                    //    {
                    //        var shader = shaderObj as Shader;
                    //        if (shader != null)
                    //        {
                    //            var shaderName = shader.name;
                    //            if (shaderNames.Contains(shaderName) || extraShaderNames.Contains(shaderName))
                    //            {
                    //                //在Shader列表的最后插入一个元素
                    //                m_ShadersProperty.InsertArrayElementAtIndex(m_ShadersProperty.arraySize);

                    //                //设置列表最后一个元素的值
                    //                var property = m_ShadersProperty.GetArrayElementAtIndex(m_ShadersProperty.arraySize - 1);
                    //                property.objectReferenceValue = shader;
                    //            }
                    //        }
                    //    }
                    //}
                }

                EditorUtility.SetDirty(targetObject);
                AssetDatabase.SaveAssets();
            }

            DrawDefaultInspector();
        }

        private List<string> GetStringsFromArrayProperty(SerializedProperty arrayProperty)
        {
            var result = new List<string>();
            for (int i = 0; i < arrayProperty.arraySize; ++i)
            {
                result.Add(arrayProperty.GetArrayElementAtIndex(i).stringValue);
            }
            return result;
        }

        private List<Shader> GetShaders(SerializedProperty shadersProperty)
        {
            int shadersSize = shadersProperty.arraySize;
            List<Shader> result = new List<Shader>();

            for (int i = 0; i < shadersSize; ++i)
            {
                var shaderProperty = shadersProperty.GetArrayElementAtIndex(i);
                Shader objectReferenceValue = (Shader)shaderProperty.FindPropertyRelative("first").objectReferenceValue;
                if(objectReferenceValue != null)
                    result.Add(objectReferenceValue);
            }

            return result;
        }

        private List<string> GetShaderNames(SerializedProperty shadersProperty)
        {
            int shadersSize = shadersProperty.arraySize;
            List<string> result = new List<string>();

            for (int i = 0; i < shadersSize; ++i)
            {
                var shaderProperty = shadersProperty.GetArrayElementAtIndex(i);
                Shader objectReferenceValue = (Shader)shaderProperty.FindPropertyRelative("first").objectReferenceValue;
                result.Add(objectReferenceValue.name);
            }

            return result;
        }

        private Object[] LoadAllAssetsByBundleName(string assetBundleName)
        {
            string[] assetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            if (assetBundlePaths.Length == 0)
            {
                Debug.LogError("There is no asset bundle with name \"" + assetBundleName + "\"");
                return null;
            }

            Object[] assets = new Object[assetBundlePaths.Length];

            for (int i = 0, count = assetBundlePaths.Length; i < count; ++i)
            {
                string assetPath = assetBundlePaths[i];
                assets[i] = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            }

            return assets;
        }
    }
}