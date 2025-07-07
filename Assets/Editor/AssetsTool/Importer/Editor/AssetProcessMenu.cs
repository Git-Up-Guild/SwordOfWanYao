using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Xml;
using System.IO;
using Object = UnityEngine.Object;

namespace XGameEditor.AssetImportTool
{
    //资源处理菜单
    public class AssetProcessMenu
    {
        [MenuItem("Assets/XGame/资源工具/更新所有资源(ab路径和导入设置)")]
        public static void UpdateAssetsSetting()
        {
            UpdateAssetsBySetting(true, true, "更新所有资源(ab路径和导入设置)");
        }

        [MenuItem("Assets/XGame/资源工具/更新所有资源ab路径")]
        public static void UpdateAssetsAB()
        {
            UpdateAssetsBySetting(false, true, "更新资源Ab路径");
        }

        [MenuItem("Assets/XGame/资源工具/更新所有资源导入设置")]
        public static void UpdateAssetsImporter()
        {
            UpdateAssetsBySetting(true, false, "更新资源导入设置");
        }

        [MenuItem("Assets/XGame/资源工具/检测选中资源导入配置")]
        public static void CheckAssetImporter()
        {
            Object[] selectObjs = GetSelectAsset(SelectionMode.DeepAssets);
            foreach (Object item in selectObjs)
            {
                string path = AssetDatabase.GetAssetPath(item);
                bool bRes = AssetImporterChecker.CheckAssetImporterSetting(path, true);
                Debug.Log($"资源导入配置检测是否通过：[{bRes}] - [{path}]");
            }
        }

        [MenuItem("Assets/XGame/资源工具/更新选中资源导入配置")]
        public static void UpdateSelectAssetImporter()
        {
            XmlNode xmlImportSettingRoot = XmlHelper.GetXmlNode(ImportDefine.ImportSettingXmlPath, ImportDefine.Root);
            Object[] selectObjs = GetSelectAsset(SelectionMode.DeepAssets);
            foreach (Object item in selectObjs)
            {
                string path = AssetDatabase.GetAssetPath(item);
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                if (assetImporter && xmlImportSettingRoot != null)
                    AssetImportHelper.UpdateAssetImporterByXmlNode(assetImporter, xmlImportSettingRoot);
            }
        }

        [MenuItem("Assets/XGame/资源工具/更新选中资源AB路径配置")]
        public static void UpdateSelectAssetAbPath()
        {
            Object[] selectObjs = GetSelectAsset(SelectionMode.DeepAssets);
            foreach (Object item in selectObjs)
            {
                string path = AssetDatabase.GetAssetPath(item);
                AssetBundleSetHelper.OnPostprocessAsset(path);
            }
        }

        private static Object[] GetSelectAsset(SelectionMode selectionMode)
        {
            Object[] selectObjs = Selection.GetFiltered<Object>(selectionMode);
            return selectObjs;
        }

        //按配置更新资源
        private static void UpdateAssetsBySetting(bool bCheckImportSetting, bool bCheckAbSetting, string Desc)
        {
            try
            {
                //XmlNode xmlImportSettingRoot = null;
                //XmlNode xmlAbSettingRoot = null;
                if (bCheckImportSetting)
                {
                    XmlNode xmlImportSettingRoot = XmlHelper.GetXmlNode(ImportDefine.ImportSettingXmlPath, ImportDefine.Root);

                    //if (bCheckAbSetting)
                    //    xmlAbSettingRoot = XmlHelper.GetXmlNode(AssetBundleDefine.AssetBundleSettingXmlPath, AssetBundleDefine.Root);

                    bool bCancel = false;
                    string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
                    int count = allAssetPaths.Length;
                    float index = 0f;
                    foreach (string path in allAssetPaths)
                    {
                        ++index;
                        bCancel = EditorUtility.DisplayCancelableProgressBar($"{Desc}: {index}/{count}",
                    $"file:{path}", index / count);
                        if (bCancel) break;

                        if (string.IsNullOrEmpty(Path.GetExtension(path))) continue;
                        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                        if (!assetImporter) continue;

                        //AssetImportHelper.ProcessSingleAsset(assetImporter, bCheckImportSetting, bCheckAbSetting);
                        //设置导入信息
                        if (bCheckImportSetting && xmlImportSettingRoot != null)
                            AssetImportHelper.UpdateAssetImporterByXmlNode(assetImporter, xmlImportSettingRoot);

                        //设置ab
                        //if (bCheckAbSetting && xmlAbSettingRoot != null)
                        //AssetImportHelper.SetAssetBundleByXmlNode(assetImporter, xmlAbSettingRoot);
                        if (bCheckAbSetting)
                            AssetBundleSetHelper.OnPostprocessAsset(path);
                    }
                }
                else
                {
                    if (bCheckAbSetting)
                        AssetBundleSetHelper.DepolyAssetBundleConfig(); //如果只检查ab，那么就按ab配置表路径检测即可，更快
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"资源导入异常：{e.Message} \n堆栈: {e.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
