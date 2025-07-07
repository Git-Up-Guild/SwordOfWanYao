using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEditor;
using XGame.Utils;

namespace XGameEditor
{
    static public class NewAssetDataDef
    {
        // 打包配置文件路径
        static public string ConfigBasePath { get => "Assets/Editor/AssetsTool/NewLoad"; }
        // 配置路径
        static public string AssetBuildConfigFilePath { get => $"{ConfigBasePath}/NewAssetBuildConfig.asset"; }
        // 缓存路径
        static public string AssetDataCacheFilePath { get => $"{ConfigBasePath}/NewAssetDataCache.asset"; }
        // 输出映射关系调试信息路径
        static public string Asset2BundlePath { get => $"{ConfigBasePath}/NewAsset2Bundle.asset"; }
        // 输出最后简化版映射关系文件
        static public string FinalAsset2BundlePath { get => $"{ConfigBasePath}/NewAsset2BundleFinal.asset"; }
        // 打包用的映射关系文件
        static public string BuildABAsset2BundlePath { get => $"{ConfigBasePath}/NewAsset2BundleBuildAB.asset"; }

        static public string DataWindowFolderPath { get => $"{Application.dataPath}/../BuildAPP/StreamingAssets/Windows/Data/"; }
        static public string DataAndroidFolderPath { get => $"{Application.dataPath}/../BuildAPP/StreamingAssets/Android/Data/"; }

        // 输出映射关系路径
        static public string Asset2BundleDataWindowPath { get => $"{DataWindowFolderPath}Asset2BundleRecords"; }
        static public string Asset2BundleDataAndroidPath { get => $"{DataAndroidFolderPath}Asset2BundleRecords"; }
        static public string Asset2BundleDataAndroidPathEX { get => $"{DataAndroidFolderPath}Asset2BundleRecordsEx"; }

        //AB包名反向索引表路径
        static public string AssetBundleNameWindowPath { get => $"{DataWindowFolderPath}AssetBundleNameRecords"; }
        static public string AssetBundleNameAndroidPath { get => $"{DataAndroidFolderPath}AssetBundleNameRecords"; }



        static public int outOfAssetsCount = 0;
        /// <summary>
        /// 检查创建文件夹
        /// </summary>
        /// <param name="filePath"></param>
        static public void CheckCreateDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }


        public static List<string> StartWithList = new List<string>()
        {
            "Assets/Editor",
        };

        public static List<string> IncludeWithList = new List<string>()
        {
            "/Editor/",
        };

        public static List<string> EndWithList = new List<string>()
        {
            ".cs",
            ".js",
            ".lua",
            ".dll",
            ".ini",
            ".bat",

            //排除LightmapData，因为LightmapData是Editor Only Object,排除后光照贴图将直接被场景依赖，保证依赖的正确性
            //为了保证依赖的正确性，对于其他Editor Only Object，也需要排除
            "LightingData.asset",
            ".mask",
            ".giparams" //,

            //图集打图片自动将打进AB
           // ".spriteatlas"
        };

        private const string AssetsDataRoot = "Assets/";
        /// <summary>
        /// 检测资源是否为有效资源（需要打包的）
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>有效返回True，无效返回False</returns>
        public static bool IsValidAsset(string path)
        {

            if (Directory.Exists(path))
            {
                return false;
            }

            foreach (var startStr in StartWithList)
            {
                if (path.StartsWithEx(startStr))
                    return false;
            }

            //包含文件
            foreach (var include in IncludeWithList)
            {
                if (path.IndexOf(include)>=0)
                    return false;
            }

          

            foreach(var endStr in EndWithList)
            {
                if (path.EndsWithEx(endStr))
                    return false;
            }

            if (!path.StartsWithEx(AssetsDataRoot))
            {
                ++outOfAssetsCount;
                Debug.LogWarning("asset 之外的资源 path=" + path+ "outOfAssetsCount="+(outOfAssetsCount));
                //return false;
               
            }

            return true;
        }
    }
}
