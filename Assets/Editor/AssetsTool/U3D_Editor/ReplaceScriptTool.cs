using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;
using System.IO;
using System.Linq;
using System.Text;

public class ReplaceScriptTool
{
    /*
    private static readonly string srcDir = "Audio";
    private static readonly string destDir = "Sound";
    [MenuItem("Assets/Replace/AudioAssetObject")]
    public static void ReplaceAudioAssetObject()
    {
        Object[] selectObjs = Selection.GetFiltered(typeof(rkt.Sound.AudioAssetObject), SelectionMode.DeepAssets);
        foreach (Object item in selectObjs)
        {
            rkt.Sound.AudioAssetObject srcAssetObj = item as rkt.Sound.AudioAssetObject;
            //创建新类型，赋值
            XGame.Audio.AudioAssetObject destAssetObj = ScriptableObject.CreateInstance<XGame.Audio.AudioAssetObject>();
            destAssetObj.name = srcAssetObj.name;
            destAssetObj.ID = srcAssetObj.ID;
            destAssetObj.audioClip = srcAssetObj.audioClip;
            destAssetObj.Desc = srcAssetObj.Desc;
            destAssetObj.priority = srcAssetObj.priority;
            destAssetObj.pitch = srcAssetObj.pitch;

            //保存
            string path = AssetDatabase.GetAssetPath(srcAssetObj);
            string srcDirPath = Path.GetDirectoryName(path);
            string destDirPath = srcDirPath.Replace(srcDir, destDir);
            if (!Directory.Exists(destDirPath))
                Directory.CreateDirectory(destDirPath);
            string destPath = path.Replace(srcDir, destDir);
            FileUtil.DeleteFileOrDirectory(destPath);
            AssetDatabase.CreateAsset(destAssetObj, destPath);
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Assets/Replace/AssetRecordsData")]
    public static void ReplaceAssetRecordsData()
    {
        Object[] selectObjs = Selection.GetFiltered(typeof(AssetRecordsData), SelectionMode.DeepAssets);
        foreach (Object item in selectObjs)
        {
            AssetRecordsData srcdata = item as AssetRecordsData;
            //创建新类型，赋值
            XGame.Audio.AssetRecordsData destData = ScriptableObject.CreateInstance<XGame.Audio.AssetRecordsData>();
            destData.assetRecords = new List<XGame.Audio.AssetRecord>();
            foreach (AssetRecord srcRecord in srcdata.assetRecords)
            {
                string newPath = srcRecord.path.Replace(srcDir, destDir);
                XGame.Audio.AssetRecord destRecord = new XGame.Audio.AssetRecord(srcRecord.id, newPath);
                destData.assetRecords.Add(destRecord);
            }

            //保存
            string path = AssetDatabase.GetAssetPath(srcdata);
            string srcDirPath = Path.GetDirectoryName(path);
            string destDirPath = srcDirPath.Replace(srcDir, destDir);
            if (!Directory.Exists(destDirPath))
                Directory.CreateDirectory(destDirPath);
            string destPath = path.Replace(srcDir, destDir);
            FileUtil.DeleteFileOrDirectory(destPath);
            AssetDatabase.CreateAsset(destData, destPath);
            AssetDatabase.SaveAssets();
        }
    }
    [MenuItem("Assets/Replace/UIPlaySound")]
    public static void ReplaceUIPlaySound()
    {
        string checkDirPath = Application.dataPath;
        string pattern = "*.prefab";
        //获取选择的文件
        Object selectObj = Selection.activeObject;
        string objpath = AssetDatabase.GetAssetPath(selectObj);
        string matchGUID = AssetDatabase.AssetPathToGUID(objpath);
        Debug.Log("匹配的GUID: " + matchGUID);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("被替换的脚本: " + objpath);
        sb.AppendLine("替换结果如下：");

        //获取所有文件
        List<string> filePath = new List<string>();
        GetFiles(checkDirPath, pattern, out filePath);
        foreach (var path in filePath)
        {
            bool isMatch = false;
            string[] otherDependsPaths = AssetDatabase.GetDependencies(path);
            foreach (var item2 in otherDependsPaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(item2);
                if (guid == matchGUID)
                {
                    isMatch = true;
                    break;
                }
            }

            if (isMatch)
            {
                GameObject tempPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (tempPrefab)
                {
                    try
                    {
                        bool isChange = false;
                        GameObject tempPrefabInstant = PrefabUtility.InstantiatePrefab(tempPrefab) as GameObject;

                        rkt.Sound.UIPlaySound[] srcSound1Arr = tempPrefabInstant.GetComponentsInChildren<rkt.Sound.UIPlaySound>(true);
                        if (srcSound1Arr.Length > 0)
                        {
                            isChange = true;
                            foreach (var oldSound in srcSound1Arr)
                            {
                                //添加新的组件
                                XGame.Audio.UIPlaySound newSound = oldSound.gameObject.AddComponent<XGame.Audio.UIPlaySound>();
                                newSound.audioClip = oldSound.audioClip;
                                newSound.audioAssetId = oldSound.audioAssetId;
                                int oldEnumValue = (int)oldSound.trigger;
                                newSound.trigger = (XGame.Audio.UIPlaySound.Trigger)oldEnumValue;
                                newSound.volume = oldSound.volume;
                                newSound.pitch = oldSound.pitch;
                                newSound.coldTime = oldSound.coldTime;

                                //删除旧的组件
                                GameObject.DestroyImmediate(oldSound);
                            }
                        }

                        XGame.SoundModule.UIPlaySound2[] srcPlaySoundArr = tempPrefabInstant.GetComponentsInChildren<XGame.SoundModule.UIPlaySound2>(true);
                        if (srcPlaySoundArr.Length > 0)
                        {
                            isChange = true;
                            foreach (var oldSound in srcPlaySoundArr)
                            {
                                //添加新的组件
                                XGame.Audio.UIPlaySound newSound = oldSound.gameObject.AddComponent<XGame.Audio.UIPlaySound>();
                                newSound.audioClip = oldSound.audioClip;
                                newSound.audioAssetId = oldSound.audioAssetId;
                                int oldEnumValue = (int)oldSound.trigger;
                                newSound.trigger = (XGame.Audio.UIPlaySound.Trigger)oldEnumValue;
                                newSound.volume = oldSound.volume;
                                newSound.pitch = oldSound.pitch;
                                newSound.coldTime = oldSound.coldTime;

                                //删除旧的组件
                                GameObject.DestroyImmediate(oldSound);
                            }
                        }

                        if (isChange)
                        {
                            //覆盖旧的预置体
                            PrefabUtility.SaveAsPrefabAsset(tempPrefabInstant, path);
                            Debug.Log("更新" + path + "对UIPlaySound声音组件的引用！");
                            sb.AppendLine(path);
                        }
                        GameObject.DestroyImmediate(tempPrefabInstant);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("替换异常：" + e.Message);
                        sb.AppendLine(path);
                    }
                }
            }
        }

        //写入记录文件
        File.WriteAllText(Application.dataPath + "/ReplaceResult.txt", sb.ToString());
    }
    private static void GetFiles(string path, string pattern, out List<string> pathArr, bool recursive = true)
    {
        pathArr = Directory.GetFiles(path, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Select(p => p = GetRelativePath(p)).ToList<string>();
    }

    /// <summary>
    /// 获取相对路径（不填基础路径的话，默认就是相对工程的路径Assets上一级）
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="basePath"></param>
    /// <returns></returns>
    private static string GetRelativePath(string originalPath, string basePath = null)
    {
        originalPath = originalPath.Replace("\\", "/");
        if (string.IsNullOrEmpty(basePath))
        {
            originalPath = FileUtil.GetProjectRelativePath(originalPath);
        }
        else
        {
            originalPath = originalPath.Replace(basePath, "");
        }
        return originalPath;
    }
    */
}
