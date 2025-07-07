using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using XGameEditor.NewLoad;

namespace XGameEditor.AssetImportTool
{
    #if !UNITY_PACKING

    /// <summary>
    /// 资源导入设置工具
    /// </summary>
    public class AssetImportSetting : AssetPostprocessor
    {
        private enum EPostType
        {
            Import,
            Delete,
            Move
        }

        //是否开启导入设置自动检查
        public static bool isAutoCheckImportSetting = false;

        //是否开启设置ab检查
        public static bool isAutoCheckAbSetting = false; 


        //所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!isAutoCheckImportSetting && !isAutoCheckAbSetting) return;
            HashSet<string> needSetABMap = new HashSet<string>();

            //Debug.Log("资源导入：");
            foreach (string path in importedAsset)
            {
                //Debug.Log("资源导入：" + path);
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                if (assetImporter)
                {
                    ProcessAssetImporter(assetImporter);
                }
                else
                {
                    Debug.LogError("找不到资源导入设置：" + path);
                }

                if (ProcessAssetAbSetting(EPostType.Import, path) && !needSetABMap.Contains(path))
                {
                    needSetABMap.Add(path);
                }
            }

            foreach (string path in deletedAssets)
            {
                //Debug.Log("资源删除：" + path);
                ProcessAssetAbSetting(EPostType.Delete, path);
            }

            int length = movedAssets.Length;
            string _path = null;
            for (int i = 0; i < length; i++)
            {
                _path = movedAssets[i];
                //Debug.Log($"资源移动：从[{movedFromAssetPaths[i]}]移动到[{_path}]");
                AssetImporter assetImporter = AssetImporter.GetAtPath(_path);
                if (assetImporter)
                {
                    ProcessAssetImporter(assetImporter);
                }
                else
                {
                    Debug.LogError("找不到资源导入设置：" + _path);
                }

                if (ProcessAssetAbSetting(EPostType.Move, _path, movedFromAssetPaths[i]) && !needSetABMap.Contains(_path))
                {
                    needSetABMap.Add(_path);
                }
            }
            int count = needSetABMap.Count; // 有新的才刷新AB
            if (count > 0)
            {
                NewGAsset2BundleRecords.Instance.RefreshRecord();
                foreach (var item in needSetABMap)
                {
                    Asset2BundleRecordsUtils.SetABPathByMeta(item);
                }
                NewGAsset2BundleRecords.Instance.Export();
                ////AssetDatabase.SaveAssets();
                ////AssetDatabase.Refresh();
            }
        }

        //处理资源导入设置
        private static void ProcessAssetImporter(AssetImporter importer)
        {
            //简单过滤非文件资源
            //if (!importer.assetPath.Contains(".")) return;

            if (isAutoCheckImportSetting)
            {
                //获取后缀对应的node节点
                XmlNode RootNode = XmlHelper.GetXmlNode(ImportDefine.ImportSettingXmlPath, ImportDefine.Root);
                if (RootNode != null)
                {
                    bool isSuc = AssetImportHelper.UpdateAssetImporterByXmlNode(importer, RootNode);
                    Debug.Log($"资源：{importer.assetPath} >> 自动配置结果：{isSuc}");
                }
                else
                {
                    Debug.LogError(ImportDefine.ImportSettingXmlPath + "找不到节点：" + ImportDefine.Root);
                }
            }

            //if (isAutoCheckAbSetting)
            //{
            //    //继续采用旧版设置ab路径
            //    AssetBundleSetHelper.OnPostprocessAsset(importer.assetPath);

            //    //设置ab路径 - 新版
            //    /*XmlNode AbRootNode = XmlHelper.GetXmlNode(AssetBundleDefine.AssetBundleSettingXmlPath, AssetBundleDefine.Root);
            //    if (AbRootNode != null)
            //    {
            //        AssetImportHelper.SetAssetBundleByXmlNode(importer, AbRootNode);
            //    }
            //    else
            //    {
            //        Debug.LogError(AssetBundleDefine.AssetBundleSettingXmlPath + "找不到节点：" + AssetBundleDefine.Root);
            //    }*/

            //}
        }

        /// <summary>
        /// 处理资源ab设置 返回是否需要设置AB
        /// </summary>
        /// <param name="ePostType"></param>
        /// <param name="curPath"></param>
        /// <param name="previousPath"></param>
        /// <returns></returns>
        private static bool ProcessAssetAbSetting(EPostType ePostType, string curPath, string previousPath = null)
        {
            bool isNeedSetAB = false;
            if (isAutoCheckAbSetting)
            {
                if (IsUsefulAsset(curPath) && NewAssetDataDef.IsValidAsset(curPath))
                {
                    switch (ePostType)
                    {
                        case EPostType.Import:
                            //更新资源缓存
                            isNeedSetAB = NewGAssetDataCache.Instance.Add(curPath);
                            isNeedSetAB = true;
                            break;
                        case EPostType.Delete:
                            //更新资源缓存
                            NewGAssetDataCache.Instance.Remove(curPath);
                            break;
                        case EPostType.Move:
                            if (string.IsNullOrEmpty(previousPath))
                            {
                                Debug.LogError("AssetImportSetting >> 参数有误！");
                                return false;
                            }
                            NewGAssetDataCache.Instance.Remove(previousPath);
                            isNeedSetAB = NewGAssetDataCache.Instance.Add(curPath);
                            isNeedSetAB = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //无需设置ab的资源，或者无用资源，那么清除ab
                    ClearAbPath(curPath);
                }
            }
            return isNeedSetAB;
        }


        private static readonly string whitePath = "BuildAB/WhiteList.txt"; //白名单
        private static readonly string blackPath = "BuildAB/BlackList.txt";

        private static string prefix = "assets";
        private static StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

        //是否有用的资源
        private static bool IsUsefulAsset(string filePath)
        {
            string baseDir = Application.dataPath + "/../";
            if (!File.Exists(baseDir + whitePath) || !File.Exists(baseDir + blackPath))
            {
                Debug.LogWarning("未找到相关配置：" + baseDir + whitePath + " 或 " + baseDir + blackPath);
                return false;
            }

            string[] whiteContents = File.ReadAllLines(baseDir + whitePath);
            string[] blackContents = File.ReadAllLines(baseDir + blackPath);

            filePath = filePath.ToLower().Replace('\\', '/');

            string tempStr = null;
            //先看是否处于白名单的文件夹
            if (whiteContents != null && whiteContents.Length > 0)
            {
                foreach (var wItem in whiteContents)
                {
                    if (string.IsNullOrEmpty(wItem)) continue;

                    tempStr = wItem.ToLower().Replace('\\', '/');
                    if (!tempStr.StartsWith(prefix))
                    {
                        tempStr = prefix + "/" + tempStr;
                    }

                    //表示处于白名单，然后在检测是否处于黑名单
                    if (filePath.StartsWith(tempStr))
                    {
                        if (blackContents != null && blackContents.Length > 0)
                        {
                            foreach (var bItem in blackContents)
                            {
                                if (string.IsNullOrEmpty(bItem)) continue;

                                tempStr = bItem.ToLower().Replace('\\', '/');
                                if (tempStr.StartsWith("."))
                                {
                                    //后缀过滤
                                    if (filePath.EndsWith(tempStr))
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    //文件过滤
                                    if (tempStr.Contains("."))
                                    {
                                        if (filePath == tempStr)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        //文件夹过滤
                                        if (!tempStr.StartsWith(prefix))
                                        {
                                            tempStr = prefix + "/" + tempStr;
                                        }

                                        if (filePath.StartsWith(tempStr))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }


        //清除ab设置
        private static void ClearAbPath(string path)
        {
            if (path.EndsWith(".cs")) return;

            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter)
            {
                assetImporter.assetBundleName = null;
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}