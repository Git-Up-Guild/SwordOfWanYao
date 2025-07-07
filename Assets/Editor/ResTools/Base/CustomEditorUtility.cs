using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CustomEditorUtility
{
    public static List<string> SearchFiles(List<Object> dirs, string searchPattern)
    {
        List<string> filePaths = new List<string>();
        foreach (var dir in dirs)
        {

            if (dir != null)
            {
                filePaths.AddRange(Directory.GetFiles(AssetDatabase.GetAssetPath(dir), searchPattern, SearchOption.AllDirectories));
            }
        }
        return filePaths;
    }

    public static void ProcessingFiles<T>(List<Object> dirs, string searchPattern, System.Func<T, bool> processingFunc, bool willChangeFile = true) where T : Object
    {
        ShowProgress("Executing", "Collecting files...", 0, 1);

        T tmpObj;
        int processedSum = 0;
        int ignoredSum = 0;
        List<string> filePaths = SearchFiles(dirs, searchPattern);

        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = filePaths[i];
            tmpObj = AssetDatabase.LoadAssetAtPath<T>(path);
            if (tmpObj != null)
            {
                if (processingFunc(tmpObj))
                {
                    if (willChangeFile)
                    {
                        EditorUtility.SetDirty(tmpObj);
                    }                   
                    processedSum++;
                }
                else
                {
                    ignoredSum++;
                }
                ShowProgress("Executing", "Processing file:" + tmpObj.name, i, filePaths.Count);
            }//tmpObj null check if end
        }//outer for end       

        if (processedSum > 0 && willChangeFile)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("Processing completed! Processed count:" + processedSum + "; Ignored count:" + ignoredSum);
    }

    public static void ShowProgress(string title, string msg, int progress, int total)
    {
        EditorUtility.DisplayProgressBar(title, string.Format("{0}...{1}/{2}", msg, progress, total), progress * 1.0f / total);
    }

}
