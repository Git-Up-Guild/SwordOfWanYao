/*******************************************************************
** 文件名: MaterialOptimizer.cs
** 版  权:  
** 创建人:  
** 日  期:    2017/12/26
** 版  本:    1.0
** 描  述:    材质文件优化
** 应  用:    1、清除材质无用的属性

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public static class MaterialOptimizer
{
    [MenuItem("XGame/其它/GLA/Res Tools/Material/Optimize Select Materials(Assets)")]
    public static void OptimizeSelectMaterials()
    {
        ShowProgress("Collect Materials...", 0, 1);
        Object[] objs = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);
        Material tmpMat;
        for (int i = 0; i < objs.Length; i++)
        {
            tmpMat = objs[i] as Material;
            if (tmpMat != null)
            {
                ShowProgress("Optimize Material:" + tmpMat.name, i, objs.Length);
                //MaterialOptions.OptimizeKeywords(tmpMat);
               // MaterialOptions.OptimizePropertys(tmpMat);
            }
        }
        ShowProgress("Refresh Materials...", 1, 1);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Debug.Log("Optimize Successed!");
    }

    #region 手动优化Material
    private static string[] s_MatSearchPaths = new string[] {
        "Artist",
        "BigWorld",
        "G_Artist",
        "G_Resources",
    };

    private static List<Material> s_allMaterial = new List<Material>();
    public static void OptimizeMaterials()
    {
        ShowProgress("Collect Materials...", 0, 1);
        FindAllMaterial();
        ShowProgress("Collect Materials Finish! Materials count:" + s_allMaterial.Count, 1, 1);

        GenShaderVariantByList();

        ShowProgress("Refresh Materials...", 1, 1);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Debug.Log("Optimize Successed!");
    }
     
    private static void FindAllMaterial()
    {
        s_allMaterial.Clear();
        List<string> filePaths = new List<string>();
        for (int i = 0; i < s_MatSearchPaths.Length; i++)
        {
            filePaths.AddRange(Directory.GetFiles(Path.Combine("Assets", s_MatSearchPaths[i]), "*.mat", SearchOption.AllDirectories));
        }

        Material tmpMat;
        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = filePaths[i];
            tmpMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (tmpMat != null)
            {
                s_allMaterial.Add(tmpMat);
            }
        }
    }

    private static void GenShaderVariantByList()
    {
        Debug.Log(s_allMaterial.Count);
        for (int i = 0; i < s_allMaterial.Count; i++)
        {
            ShowProgress("Handle Material...", i, s_allMaterial.Count);

            Material mat = s_allMaterial[i];

            if (mat != null)
            {
                if (mat.shader != null)
                {
                    Debug.Log("Optimize Material:" + mat.name+i);
                    ShowProgress("Optimize Material:" + mat.name, i, s_allMaterial.Count);
                  //  MaterialOptions.OptimizeKeywords(mat);
                   // MaterialOptions.OptimizePropertys(mat);
                }
            }
        }

        Debug.Log(s_allMaterial.Count + "  finish");
    }

    #endregion
    private static void ShowProgress(string msg, int progress, int total)
    {
        EditorUtility.DisplayProgressBar("MaterialOptimizer", string.Format("{0}...{1}/{2}", msg, progress, total), progress * 1.0f / total);
    }
}
