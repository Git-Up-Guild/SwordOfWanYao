/*******************************************************************
** 文件名: ShaderCollector.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:    黄二军
** 日  期:    2020/11/11
** 版  本:    1.0
** 描  述:   
** 应  用:   

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;


public class ShaderCollector : EditorWindow
{

    private List<Object> searchDirs = new List<Object>();
    private Dictionary<string, string> m_UpdateDataDic = new Dictionary<string, string>();

    [MenuItem("XGame/其它/GLA/Res Tools/Material/Open ShaderCollector")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ShaderCollector));
    }

    void OnGUI()
    {
        searchDirs = CustomEditorGUILayout.ObjectListField("Search Directory", searchDirs, typeof(Object));

        if (GUILayout.Button("Update Materials"))
        {
            UpdateMaterials();
            SaveUsageDataToCsv();
        }
    }

    private void UpdateMaterials()
    {
        m_UpdateDataDic.Clear();
        CustomEditorUtility.ProcessingFiles<Shader>(searchDirs, "*.shader", ProcessingMaterial);
    }

    private bool ProcessingMaterial(Shader tmpShader)
    {
        m_UpdateDataDic.Add(AssetDatabase.GetAssetPath(tmpShader), tmpShader.name);
        return false;
    }

    private string GetCurrentDirectory()
    {
        MonoScript ms = MonoScript.FromScriptableObject(this);
        string path = AssetDatabase.GetAssetPath(ms);
        return path.Substring(0, path.LastIndexOf('/') + 1);
    }

    List<string> shaderPaths;
    List<string> shaderNames;
    private void SaveUsageDataToCsv()
    {
        CustomEditorUtility.ShowProgress("Executing", "Save Batching Data To Csv...", 0, 1);
        string csvMapFilePath = Path.Combine(GetCurrentDirectory(), "ShaderCollector.csv");
        Debug.Log(csvMapFilePath);
        using (StreamWriter sw = new StreamWriter(csvMapFilePath))
        {
            string tmpStr;
            int tmpIndex = 0;
            shaderPaths = m_UpdateDataDic.Keys.ToList<string>();
            shaderNames = m_UpdateDataDic.Values.ToList<string>();

            string shaderPath;
            string shaderName;
            for (int i = 0; i < shaderPaths.Count; i++)
            {
                shaderPath = Path.GetFileNameWithoutExtension(shaderPaths[i]);
                shaderName = shaderNames[i];
                tmpStr = string.Format("{0},{1},{2}", tmpIndex, shaderPath, shaderName);
                sw.WriteLine(tmpStr);
                tmpIndex++;
            } 
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        EditorUtility.RevealInFinder(csvMapFilePath);
    }
}

