/*****************************************************
** 文 件 名：Asset2BundleRecords
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/7/13 12:08:47
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.NewLoad
{
    public class Asset2BundleRecordsUtils : Editor
    {
        // 屏了旧的
        //[MenuItem("XGame/打包工具/生成资源与Bundle映射文件")]
        //public static void GenerateAsset2BundleRecords()
        //{
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();

        //    Asset2BundleRecords records = new Asset2BundleRecords();

        //    string[] bundles = AssetDatabase.GetAllAssetBundleNames();
        //    int nCur = 0;
        //    int nTotal = bundles.Length;
        //    bool bCancel = false;
        //    foreach(var bd in bundles)
        //    {
        //        nCur++;

        //        if(nCur % 312 == 0)
        //        {
        //            bCancel = EditorUtility.DisplayCancelableProgressBar("正在生成", $"进度 ({nCur}/{nTotal})", (float)nCur / nTotal);
        //        }

        //        if(bCancel)
        //        {
        //            break;
        //        }

        //        string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bd);
        //        foreach (var asName in assetPaths)
        //        {
        //            var assetName = Path.GetFileNameWithoutExtension(bd).ToLowerEx();
        //            records.records.Add(new Asset2BundleRecords.Record(XGameEditorUtilityEx.RelativeToAssetsPath(asName), bd, assetName));
        //        }

        //    }

        //    //已经取消
        //    if(bCancel)
        //    {
        //        EditorUtility.ClearProgressBar();
        //        return;
        //    }

        //    //保存数据
        //    string filePath = Application.dataPath + "/StreamingAssets/Windows/Data/Asset2BundleRecords";
        //    BinaryFormatter binFormat = new BinaryFormatter();
        //    Stream fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        //    binFormat.Serialize(fStream, records);
        //    fStream.Close();

        //    filePath = Application.dataPath + "/StreamingAssets/Android/Data/Asset2BundleRecords";
        //    fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        //    binFormat.Serialize(fStream, records);
        //    fStream.Close();

        //    EditorUtility.ClearProgressBar();

        //}

        [MenuItem("Assets/XGame/AssetBundle/复制AB路径")]
        private static void CopyAssetBundles()
        {
            var objs = Selection.objects;
            if (objs != null && objs.Length > 0)
            {
                var assetPath = AssetDatabase.GetAssetPath(objs[0]);
                var assetImporter = AssetImporter.GetAtPath(assetPath);
                if (assetImporter != null)
                {
                    GUIUtility.systemCopyBuffer = assetImporter.assetBundleName;
                }
            }
        }

        [MenuItem("Assets/XGame/AssetBundle/设置文件AB名(直接改meta)")]
        public static void SetAssetBundleMetaByPath()
        {
            try
            {
                foreach (var obj in Selection.objects)
                {
                    var assetPath = AssetDatabase.GetAssetPath(obj);
                    SetABPathByMeta(assetPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static string GetAssetBundleName(string bundleAlias)
        {
            //构建AB名称的转换关系
            if (dicAssetbundleNames == null)
            {
                dicAssetbundleNames = new Dictionary<string, string>();
                var recrods = NewGAsset2BundleRecords.Instance.allRecord;
                var recrodsForBuildAB = NewGAsset2BundleRecords.BuildABInstance.allRecord;
                for (var i = 0; i < recrods.Count; i++)
                {
                    var r = recrods.RecordList[i];
                    var r2 = NewGAsset2BundleRecords.BuildABInstance.allRecord.GetRecord(r.assetPath);
                    if(!dicAssetbundleNames.ContainsKey(r2.bundleName))
                    {
                        dicAssetbundleNames.Add(r2.bundleName, r.bundleName);
                    }
                }
            }

            if (dicAssetbundleNames.ContainsKey(bundleAlias))
                return dicAssetbundleNames[bundleAlias];

            return string.Empty;
        }

        public static AssetBundleManifest manifest = null;
        public static Dictionary<string, string> dicAssetbundleNames = null;
        [MenuItem("XGame/打包工具/查找依赖关系")]
        public static void ShowDependens()
        {
            PromptDialog.Item[] items = new PromptDialog.Item[1];
            items[0] = new PromptDialog.Item();
            items[0].itemType = PromptDialog.EnItemType.String;
            items[0].label = "输入资源的路径";
            PromptDialog.Display("输入", items, "确定", "取消", (result, objs) => {
                if (manifest == null)
                {
                    var manifestBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/data/data.bin");
                    if (manifestBundle == null)
                    {
                        Debug.LogError("manifestBundle is null.");
                        return;
                    }

                    manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    if (manifest == null)
                    {
                        Debug.LogError("manifest is null.");
                        return;
                    }
                }

                string assetName = objs[0].ToString();
                string bundleName = NewGAsset2BundleRecords.FinalInstance.GetBundleName(assetName);
                string[] depends = manifest.GetAllDependencies(bundleName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(objs[0] + " 的依赖Assetbundle列表：");
                foreach (var d in depends)
                {
                    string orginName = GetAssetBundleName(d);
                    sb.AppendLine(".........->" + orginName + "("+ d +")");
                    GetAssetbundleDepends(manifest, d, ".........", sb);
                }
                Debug.Log(sb.ToString());
            });
        }

        private static void GetAssetbundleDepends(AssetBundleManifest manifest, string assetBundleName, string pre, StringBuilder sb)
        {
            string newPre = pre + ".........";
            string[] depends = manifest.GetAllDependencies(assetBundleName);
            foreach(var n in depends)
            {
                string orginName = GetAssetBundleName(n);
                sb.AppendLine(newPre + "->" + orginName + "(" + n + ")");
                GetAssetbundleDepends(manifest, n, newPre, sb);
            }
        }

        private static bool isCancelSetAB = false;
        [MenuItem("Assets/XGame/AssetBundle/设置目录AB名(直接改meta)")]
        public static void SetAssetBundleMetaFolderByPath()
        {
            isCancelSetAB = false;
            try
            {
                foreach (var obj in Selection.objects)
                {
                    var assetPath = AssetDatabase.GetAssetPath(obj);
                    SetFolderABPathByMeta(assetPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 格式化ab资源名或包名
        /// </summary>
        /// <param name="assetPath"></param>
        private static string GetRelativeABName(string name)
        {
            return XGameEditorUtilityEx.RelativeToAssetsPath(name);
        }

        private static string _abNameKey = "assetBundleName:";
        private static string _assetPathName = "Assets/";
        /// <summary>
        /// 设置目录下所有AB路径
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="assetName"></param>
        public static void SetFolderABPathByMeta(string folder, string assetName = null)
        {
            if (isCancelSetAB || !Directory.Exists(folder))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            var allFile = dirInfo.GetFiles("*.meta", SearchOption.AllDirectories);
            int count = allFile.Length;
            for (int i = 0; i < count; ++i)
            {
                var assetPath = allFile[i].FullName;
                assetPath = assetPath.Replace("\\", "/");
                string name = "";
                if (assetName != null)
                {
                    name = assetName;
                }
                else
                {
                    var asset = assetPath.Substring(assetPath.IndexOf(_assetPathName)).Replace(".meta", "");
                    if (!NewAssetDataDef.IsValidAsset(asset))
                    {
                        continue;
                    }
                    name = NewGAsset2BundleRecords.Instance.GetBundleName(GetRelativeABName(asset));
                }
                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogError($"找不到AB包名配置：assetPath:{assetPath}");
                    continue;
                }

                isCancelSetAB = EditorUtility.DisplayCancelableProgressBar("设置AB", $"{name}", (float)i / count);
                if (isCancelSetAB)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                if (File.ReadAllText(assetPath).Contains("folderAsset: yes"))   // 文件夹跳过
                {
                    continue;
                }

                SetMetaFileABName(assetPath, name);
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 设置AB路径
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="assetName"></param>
        /// <param name="isSkipExist"></param>
        public static void SetABPathByMeta(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"设置AB失败！不存在文件：{filePath}");
                return;
            }
            var metaFile = $"{filePath}.meta";
            var assetPath = filePath;
            assetPath = assetPath.Replace("\\", "/");
            var asset = assetPath.Substring(assetPath.IndexOf(_assetPathName));
            if (!NewAssetDataDef.IsValidAsset(asset))
            {
                Debug.LogError($"设置AB失败！不需要设置AB的文件：{filePath}");
                return;
            }
            var key = GetRelativeABName(asset);
            string bundleName = NewGAsset2BundleRecords.Instance.GetBundleName(key);
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError($"找不到AB包名配置：assetPath:{key}");
                return;
            }

            SetMetaFileABName(metaFile, bundleName);
        }

        /// <summary>
        /// 设置meta文件中的ab名
        /// </summary>
        /// <param name="metaFile"></param>
        /// <param name="bundle"></param>
        static private void SetMetaFileABName(string metaFile, string bundleName)
        {
            if (!File.Exists(metaFile))
            {
                Debug.LogError($"设置AB失败！不存在meta文件：{metaFile}");
                return;
            }

            var allLine = File.ReadAllLines(metaFile);
            string curLine;
            bool isFind = false;
            for (int j = allLine.Length - 1; j >= 0; --j)
            {
                curLine = allLine[j];
                if (curLine.Contains(_abNameKey))
                {
                    var endIndex = curLine.IndexOf(_abNameKey) + _abNameKey.Length;
                    var newCurLine = curLine.Substring(0, endIndex);
                    allLine[j] = $"{newCurLine} {bundleName}";
                    if (curLine == allLine[j])
                    {
                        //Debug.Log($"已存在AB相同名字 {curLine}");
                        break;
                    }
                    isFind = true;
                    break;
                }
            }
            if (isFind)
            {
                Debug.Log($"设置AB file:{metaFile}, bundleName:{bundleName}");
                File.WriteAllLines(metaFile, allLine);
            }
        }
    }
}
