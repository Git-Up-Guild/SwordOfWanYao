using subjectnerdagreement.psdexport;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.PSDTool
{
    public class PSDTools : ScriptableObject
    {
        [MenuItem("XGame/PSD工具/导出公共资源")]
        static void ExportCommRes()
        {
            string dir = EditorUtility.SaveFolderPanel("选择导出资源的存放目录",  PSDSetting.Instance.exportCommResDir, null);
            if (string.IsNullOrEmpty(dir))
                return;

            if(PSDSetting.Instance.exportCommResDir != dir)
            {
                PSDSetting.Instance.exportCommResDir = dir;
                EditorUtility.SetDirty(PSDSetting.Instance);
                AssetDatabase.SaveAssets();
            }

            List<string> lsPaths = new List<string>();
            string fullDirPath = XGameEditorUtilityEx.AssetPathInApplication(PSDSetting.Instance.importBaseDir + "/Common");
            XGameEditorUtilityEx.GetFilesPath(fullDirPath, true, lsPaths, new string[] { ".png" });

            int nProgress = 0;
            foreach(var path in lsPaths)
            {
                EditorUtility.DisplayProgressBar("正在导出", path, (float)(nProgress + 1) / lsPaths.Count);
                string relativePath = path.Substring(fullDirPath.Length + 1);
                int idx = relativePath.IndexOf("\\");
                if(idx != -1)
                {
                    string lastFileName = Path.GetFileName(path);
                    string subDirName = relativePath.Substring(0, idx);
                    foreach(var m in PSDSetting.Instance.pathMap)
                    {
                        if(m.DirectoryName == subDirName)
                        {
                            lastFileName = m.matchName + "_" + lastFileName;
                            break;
                        }
                    }

                    string exportFilePath = PSDSetting.Instance.exportCommResDir + "/" + lastFileName;
                    if(File.Exists(exportFilePath))
                        File.Delete(exportFilePath);
                    FileUtil.CopyFileOrDirectory(path, exportFilePath);
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}