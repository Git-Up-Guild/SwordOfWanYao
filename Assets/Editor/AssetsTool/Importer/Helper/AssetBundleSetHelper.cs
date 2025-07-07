using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Globalization;

namespace XGameEditor.AssetImportTool
{
    public class AssetBundleSetHelper
    {
        /// <summary>
        /// 导入资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        public static void OnPostprocessAsset(string assetPath)    //自动获取资源路径
        {
            if (Path.GetDirectoryName(assetPath).ToLower().StartsWith("assets/ui/"))
            {
                if (Path.GetDirectoryName(assetPath).ToLower().StartsWith("assets/ui/minimap"))
                {
                }
                else
                {
                    TextureImporter texImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (texImporter != null && string.IsNullOrEmpty(texImporter.spritePackingTag))
                    {
                        texImporter.textureType = TextureImporterType.Sprite;
                        texImporter.spritePackingTag = Path.GetDirectoryName(assetPath).Replace("Assets/", "").Replace("/", ".").ToLower();
                        texImporter.mipmapEnabled = false;
                    }
                }
            }
            else
            {
            }

            if (!bInit)
            {
                LoadXMLConfig();
                bInit = true;
            }

            if (assetPath.EndsWith(".dds", System.StringComparison.OrdinalIgnoreCase))
            {
                AssetDatabase.DeleteAsset(assetPath);
                EditorUtility.DisplayDialog("警告！", "不支持dds格式的图片,请转成“png”或者“tga”格式", "确定", "");
                Debug.LogError("不能使用*.dds文件");
            }

            foreach (var config in configList)
            {
                bool bStartWith = assetPath.StartsWith(config.path, System.StringComparison.OrdinalIgnoreCase);
                bool bEndWith = false;
                foreach (var sub in config.postfix.Split(new char[] { ',' }))
                {
                    bEndWith |= assetPath.EndsWith(sub, System.StringComparison.OrdinalIgnoreCase);
                    if (bEndWith)
                        break;
                }
                if (bStartWith && bEndWith)
                {
                    SetUpAssetBundleName(assetPath, config.strategy, config.name);
                }
            }
        }

        /// <summary>
        /// 配置所有资源ab（按配置表 AssetBundleConfig.xml）
        /// </summary>
        public static void DepolyAssetBundleConfig()
        {
            LoadXMLConfig();
            bInit = true;

            bool bCancel = false;
            int count = configList.Count;
            int index = 0;
            foreach (var config in configList)
            {
                if (bCancel) break;
                ++index;
                string[] assets = AssetDatabase.FindAssets("", new string[] { config.path });
                float childIndex = 0;
                int childCount = assets.Length;
                foreach (var guid in assets)
                {
                    string assetName = AssetDatabase.GUIDToAssetPath(guid);

                    ++childIndex;
                    bCancel = EditorUtility.DisplayCancelableProgressBar($"配置所有资源ab路径: {config.path}: {index}/{count}",
                    $"file:{assetName}", childIndex / childCount);
                    if (bCancel) break;

                    bool bEndWith = false;
                    foreach (var sub in config.postfix.Split(new char[] { ',' }))
                    {
                        bEndWith |= assetName.EndsWith(sub, System.StringComparison.OrdinalIgnoreCase);
                        if (bEndWith)
                            break;
                    }
                    if (bEndWith)
                    {
                        SetUpAssetBundleName(assetName, config.strategy, config.name);
                    }
                }
                AssetDatabase.SaveAssets();
            }
            EditorUtility.ClearProgressBar();
        }

        private static void SetUpAssetBundleName(string path, STRATEGY strategy, string name)
        {
            var assetImporter = AssetImporter.GetAtPath(path);
            switch (strategy)
            {
                case STRATEGY.PATH:
                    assetImporter.assetBundleName = path.Substring(path.IndexOf("/") + 1);
                    break;
                case STRATEGY.DIR:
                    assetImporter.assetBundleName = System.IO.Path.GetDirectoryName(path).Substring(path.IndexOf("/") + 1);
                    break;
                case STRATEGY.TAG:
                    var textureImporter = assetImporter as TextureImporter;
                    if (textureImporter != null)
                    {
                        if (!string.IsNullOrEmpty(textureImporter.spritePackingTag))
                        {
                            textureImporter.assetBundleName = uiTextureBundlePrefix + textureImporter.spritePackingTag;
                        }
                        else
                        {
                            string assetBundleName = path.Substring(path.IndexOf("/") + 1);
                            textureImporter.assetBundleName = assetBundleName;
                        }
                    }
                    break;
                case STRATEGY.CUSTOMER:
                    assetImporter.assetBundleName = name;
                    Debug.LogFormat("new asset as {0}: {1}", name, path);
                    break;
                default:
                    break;
            }
        }

        #region XML配置解析相关
        private enum STRATEGY
        {
            PATH = 0,
            DIR,
            TAG,
            CUSTOMER,
        }
        private struct AssetBundleConfig
        {
            public string path;
            public string postfix;
            public STRATEGY strategy;
            public string name;
        }
        private static List<AssetBundleConfig> configList = new List<AssetBundleConfig>();
        private static string xmlpath = "Assets/XGameEditor/Editor/AssetsTool/AssetImporter/AssetBundleSetting.xml";
        private static bool bInit = false;
        private const string uiTextureBundlePrefix = "ui/spritepacking/";
        /*
		 *  1. LUA 脚本						.txt
		 *  2. 策划配置的数据文件			.bytes
		 *  3. 美术资源
		 * 			场景					.unity 怎么切分的
		 * 			
		 * 			人物
		 * 			
		 * 			模型
		 * 			
		 * 			特效
		 * 	
		 * 			UI
		 * 			美术资源就涉及 Shader, Material, Mesh, Texture
		 * 
		 *  门槛，依赖关系的解决, 小包和大包的综合
		 *  ASSETBUNDLE 的打包策略，文件？路径？文件夹？ 特殊的TAG？
		 *
		 */
        private static void LoadXMLConfig()
        {
            configList.Clear();
            #region XMLCONFIG
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" + xmlpath;
                xmlDoc.Load(fullpath);
                XmlNodeList abList = xmlDoc.SelectSingleNode("AssetBundleConfig").SelectNodes("AssetBundle");
                for (int i = 0; i < abList.Count; ++i)
                {
                    XmlNode ab = abList.Item(i);
                    string name = ab.Attributes["name"].Value;
                    XmlNode path = ab.SelectSingleNode("AssetPath");
                    XmlNode postfix = ab.SelectSingleNode("PostFix");
                    XmlNode strategy = ab.SelectSingleNode("Strategy");

                    AssetBundleConfig abc = new AssetBundleConfig();
                    abc.path = path.InnerText;
                    abc.postfix = postfix.InnerText;
                    switch (strategy.InnerText)
                    {
                        case "path":
                            abc.strategy = STRATEGY.PATH;
                            break;
                        case "dir":
                            abc.strategy = STRATEGY.DIR;
                            break;
                        case "tag":
                            abc.strategy = STRATEGY.TAG;
                            break;
                        case "customer":
                            abc.strategy = STRATEGY.CUSTOMER;
                            break;
                    }
                    abc.name = name;
                    configList.Add(abc);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("导入资源时，发现配置错误，请联系程序员！");
                Debug.LogError(e);
            }
            #endregion
        }
        #endregion
    }
}