/*******************************************************************
** 文件名: VariantOptimizerEditor.cs
** 版  权:    (C) 
** 创建人:    
** 日  期:    2021/07/02
** 版  本:    1.0
** 描  述:    Variant优化工具
** 应  用:    

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.ShaderVariantCollection;
using Object = UnityEngine.Object;
using System.Linq;

public class VariantOptimizerEditor : EditorWindow
{
    public Dictionary<Shader, List<ShaderVariant>> SVCResolver(ShaderVariantCollection shadervariants)
    {
        Dictionary<Shader, List<ShaderVariant>> shaderVariants = new Dictionary<Shader, List<ShaderVariant>>();
        using (var so = new SerializedObject(shadervariants))
        {
            var array = so.FindProperty("m_Shaders.Array");
            if (array != null && array.isArray)
            {
                var arraySize = array.arraySize;
                for (int i = 0; i < arraySize; ++i)
                {
                    var shaderRef = array.FindPropertyRelative(String.Format("data[{0}].first", i));
                    var shaderShaderVariants = array.FindPropertyRelative(String.Format("data[{0}].second.variants", i));
                    if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference &&
                        shaderShaderVariants != null && shaderShaderVariants.isArray)
                    {
                        var shader = shaderRef.objectReferenceValue as Shader;
                        if (shader == null)
                        {
                            continue;
                        }
                        var shaderAssetPath = AssetDatabase.GetAssetPath(shader);
                        List<ShaderVariant> variants = null;
                        if (!shaderVariants.TryGetValue(shader, out variants))
                        {
                            variants = new List<ShaderVariant>();
                            shaderVariants.Add(shader, variants);
                        }
                        var variantCount = shaderShaderVariants.arraySize;
                        for (int j = 0; j < variantCount; ++j)
                        {
                            ShowProgress("Resolver " + shader.name + " SVC Variants ...", j, variantCount);

                            var prop_keywords = shaderShaderVariants.FindPropertyRelative(String.Format("Array.data[{0}].keywords", j));
                            var prop_passType = shaderShaderVariants.FindPropertyRelative(String.Format("Array.data[{0}].passType", j));
                            if (prop_keywords != null && prop_passType != null && prop_keywords.propertyType == SerializedPropertyType.String)
                            {
                                var srcKeywords = prop_keywords.stringValue;
                                var keywords = srcKeywords.Split(' ');
                                var pathType = (UnityEngine.Rendering.PassType)prop_passType.intValue;
                                ShaderVariantCollection.ShaderVariant sv = new ShaderVariantCollection.ShaderVariant();
                                sv.shader = shader;
                                sv.passType = pathType;
                                sv.keywords = keywords;
                                variants.Add(sv);
                            }
                        }
                    }
                }
            }
        }

        return shaderVariants;
    }

    private List<ShaderVariantCollection> objList = new List<ShaderVariantCollection>();

    [MenuItem("XGame/其它/GLA/Res Tools/Material/Open VariantOptimizerEditor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(VariantOptimizerEditor));
    }
     
    void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Search Directory");
        for (int i = objList.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal("Box");
            objList[i] = EditorGUILayout.ObjectField(objList[i], typeof(ShaderVariantCollection), false) as ShaderVariantCollection;
            if (GUILayout.Button("Remove"))
            {
                objList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add"))
        {
            objList.Add(null);
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Variant Optimizer"))
        {
            UpdateMaterials();
        }
    }

    private static Dictionary<Shader, List<ShaderVariant>> defaultShadervariantsDic;
    private void UpdateMaterials()
    {
        if (defaultShadervariantsDic == null) {
            string configPath = Path.Combine("Assets/BigWorld/Artist/Shader/DefaultShaderVariants.shadervariants");
           // LighadEngine.ToolkitsEditor.EditorHelper.MakeSureDirectoryExist(Path.GetDirectoryName(configPath));
            ShaderVariantCollection allSVC = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(configPath);
            defaultShadervariantsDic = SVCResolver(allSVC);
        }

        foreach (var svc in objList) {
            VariantOptimizer(svc as ShaderVariantCollection);
        }
    }

    public bool VariantOptimizer(ShaderVariantCollection svc)
    {
        Dictionary<Shader, List<ShaderVariant>> shadervariantsDic = SVCResolver(svc);
        svc.Clear();
        foreach (var key in shadervariantsDic.Keys)
        {
            List<ShaderVariant> shaderVariants = shadervariantsDic[key];
            if (defaultShadervariantsDic.ContainsKey(key))
            {
                List<ShaderVariant> defaultVariants = defaultShadervariantsDic[key];
                foreach (var shaderVariant in shaderVariants)
                {
                    bool isNot = true;
                    foreach (ShaderVariant defaultVariant in defaultVariants){
                        if (Enumerable.SequenceEqual(defaultVariant.keywords, shaderVariant.keywords))
                        {
                            isNot = false;
                            break;
                        }
                    }

                    //if (isNot)
                    //{
                    //    svc.Remove(shaderVariant);
                    //}
                    if (!isNot) {
                        svc.Add(shaderVariant);
                    }
                }
            }
            //else {
            //    foreach (var shaderVariant in shaderVariants)
            //    {
            //        svc.Remove(shaderVariant);
            //    }
            //}
        }

        EditorUtility.SetDirty(svc);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        return false;
    }

    private void ShowProgress(string msg, int progress, int total)
    {
        EditorUtility.DisplayProgressBar("VariantCollectionsEditor", string.Format("{0}...{1}/{2}", msg, progress, total), progress * 1.0f / total);
    }
}

