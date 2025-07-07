/*******************************************************************
** 文件名:	HybridCLREditor.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	李美
** 日  期:   $DATE$
** 版  本:	1.0
** 描  述:	
** 应  用:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XGameEditor;

public class HybridCLREditor : IPostprocessBuildWithReport
{

    public int callbackOrder => 999; // 运行顺序，越大越晚执行

    public static class AOTDllBackupMenu
    {
        [MenuItem("HybridCLR/备份AOT DLL")]
        public static void ManualBackupAOTDlls()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            BackupAOTDlls(target);
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (BuildAPPWindow.IsBackupAOTDll)
        {
            BackupAOTDlls(report.summary.platform);
            
        }
    }

    private static void BackupAOTDlls(BuildTarget buildTarget)
    {
        /*
        // 获取 HybridCLR 自动生成的裁剪后 AOT DLL 目录
        string sourceDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);

        //备份目标目录
        string targetDir = GetBackupAOTDllDir(buildTarget);
        if (!Directory.Exists(sourceDir))
        {
            Debug.LogError($"[备份AOT DLL] 剥离目录不存在：{sourceDir}");
            return;
        }
        if (Directory.Exists(targetDir))
        {
            Directory.Delete(targetDir, true);
        }
        Directory.CreateDirectory(targetDir);
        foreach (var file in Directory.GetFiles(sourceDir, "*.dll"))
        {
            string fileName = Path.GetFileName(file);
            string destPath = Path.Combine(targetDir, fileName);
            File.Copy(file, destPath, overwrite: true);
            Debug.Log($"[备份AOT DLL] 复制: {fileName} -> {destPath}");
        }
        AssetDatabase.Refresh();
        Debug.Log($"已成功备份 AOT DLL 到：{targetDir}");
        */
    }

    public static string GetBackupAOTDllDir(BuildTarget target)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string targetDir = Path.Combine(projectRoot, "HybridCLRData/AOTBackup", target.ToString());
        return targetDir;
    }
}