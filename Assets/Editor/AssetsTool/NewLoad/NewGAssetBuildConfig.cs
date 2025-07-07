using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using XGame.Utils;

namespace XGameEditor
{
    /// <summary>
    /// GAssetBuildConfig, 资源打包配置类
    /// </summary>
    public class NewGAssetBuildConfig : ScriptableObject
    {

        /// <summary>
        /// 资源打包类型
        /// </summary>
        public enum PackType
        {
            [Tooltip("按文件夹打包")]
            PerFloder = 0,
            [Tooltip("目录下的文件按单个文件打包")]
            Alone = 1,
            [Tooltip("目录下的所有文件打包到一起")]
            AllInOne = 2,
            [Tooltip("按一级子目录打包")]
            FirstLevelSubdirectory = 3,
        }

        /// <summary>
        /// 打包模式
        /// </summary>
        public enum BuildMode
        {
            //增量打包
            Default,
            //全部重新打包
            ForceRebuild,
            //不进行实际打包，只生成相关数据文件 
            DataOnly,
        }

        /// <summary>
        /// 打包类型配置
        /// </summary>
        [System.Serializable]
        public struct PackTypeConfig
        {
            // 路径
            public string path;
            // 打包类型
            public PackType packMode;

            public PackTypeConfig(string path, PackType packMode)
            {
                this.path = path;
                this.packMode = packMode;
            }
        }

        /// <summary>
        /// 打包目录组
        /// </summary>
        [System.Serializable]
        public struct PackGroup
        {
            [Header("子目录名称(#分隔)")]
            public string childDirName;

            [Header("需要动态加载的目录")]
            public List<string> mainResDirList;
        }


        /// <summary>
        /// 打包目录组
        /// </summary>
        [System.Serializable]
        public struct DynamicMainResGroup
        {
            [Header("后缀名")]
            public string suffixName;

            [Header("需要变成主资源的目录")]
            public string mainResDir;

            [Header("是否按目录独立生成AB名称")]
            public bool aloneDirAB;

            [Header("是否能作为依赖资源加载")]
            public bool beDependent;
        }

        //需要动态加载的目录(会生成AB包的目录)
        [Header("需要动态加载的目录(会生成AB包的目录)")]
        public List<string> mainResDirList;

        //需要动态加载的目录(会生成AB包的目录)
        [Header("需要动态加载的目录组")]
        public List<PackGroup> packGroupList;

        //需要动态加载的目录(会生成AB包的目录)
        [Header("动态资源变成主资源的目录组")]
        public List<DynamicMainResGroup> dynamicGroupList;

        //指定了打包类型目录，按目录深度降序
        [Header("指定了打包类型目录")]
        public List<PackTypeConfig> packTypeConfigList;

        // 动态加载的目录中 要过滤的目录
        [Header("动态加载的目录中 要过滤的目录")]
        public List<string> mainResFilterDirList;

        //特殊资源放到一个包里面
        public List<string> resInOnePackList;

        // 图集目录
        [Header("递归打包图集的目录")]
        public List<string> spriteAltasDirList;

        /// <summary>
        /// 这个列表支持填写文件夹中的子目录单独打包图集
        /// </summary>
        [Header("所有图片打包一个图集的目录")]
        public List<string> spriteAltasSubDirList;

        // 不导出映射表的目录
        [Header("不导出映射表的目录")]
        public List<string> dontExportDirList;

        // 要导出映射表的资源
        [Header("要导出映射表的资源")]
        public List<string> needExportDirList;

        // 要导出映射表的资源
        [Header("不能压缩的列表")]
        public List<string> uncompresseedDirList;


        private static NewGAssetBuildConfig _instance;

        //全局实例
        public static NewGAssetBuildConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<NewGAssetBuildConfig>(NewAssetDataDef.AssetBuildConfigFilePath);
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<NewGAssetBuildConfig>();
                        _instance.mainResDirList = new List<string>();
                        _instance.packTypeConfigList = new List<PackTypeConfig>();
                        NewAssetDataDef.CheckCreateDirectory(NewAssetDataDef.AssetBuildConfigFilePath);
                        AssetDatabase.CreateAsset(_instance, NewAssetDataDef.AssetBuildConfigFilePath);
                    }
                    _instance.ForceRefreshMainResPathList();
                    _instance.SortPackTypeConfigList();
                }
                return _instance;
            }
        }

        public void SortPackTypeConfigList()
        {
            packTypeConfigList.Sort((l, r) => { return -string.Compare(l.path, r.path); });
        }

        /// <summary>
        /// 获取ab包名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetAssetBundleName(string path)
        {
            string assetBundleName = string.Empty;
            //动态资源作为主资源的ab名字转换
            foreach (DynamicMainResGroup dynamicGroup in dynamicGroupList)
            {
                if (dynamicGroup.aloneDirAB == false)
                {
                    continue;
                }

                if (path.StartsWithEx(dynamicGroup.mainResDir) && path.EndsWithEx(dynamicGroup.suffixName))
                {
                    assetBundleName = Path.GetDirectoryName(path) + "_Dynamic";
                    return assetBundleName.ToLower();
                }
            }

            //指定了目录的打包配置的
            foreach (var packModeConfig in packTypeConfigList)
            {
                if (path.StartsWith(packModeConfig.path))
                {
                    switch (packModeConfig.packMode)
                    {
                        case PackType.Alone:
                            assetBundleName = path;
                            break;
                        case PackType.AllInOne:
                            assetBundleName = packModeConfig.path;
                            break;
                        case PackType.FirstLevelSubdirectory:
                            string noParentPath = path.Replace(packModeConfig.path, "").Replace("\\", "/");
                            if (noParentPath.StartsWith("/"))
                                noParentPath = noParentPath.Substring(1, noParentPath.Length - 1);
                            string firstLevel = noParentPath.Split('/')[0];
                            assetBundleName = Path.Combine(packModeConfig.path, firstLevel).Replace("\\", "/");
                            break;
                        default:
                            assetBundleName = Path.GetDirectoryName(path);
                            break;
                    }

                    if (path.EndsWith(".unity"))
                    {
                        assetBundleName = $"{assetBundleName}_scene";
                    }
                    return assetBundleName.ToLower();
                }
            }//foreach end

            //没有指定的就是获取目录名称作为包名
            //assetBundleName = Path.GetDirectoryName(path);
            assetBundleName = path;

            if (path.EndsWith(".unity"))
            {
                assetBundleName = $"{assetBundleName}/{Path.GetFileNameWithoutExtension(path)}_scene";
            }

 

            return assetBundleName.ToLower();
        }

        /// <summary>
        /// 是否有打包规则
        /// </summary>
        /// <returns></returns>
        public bool IsHasPackMode(string path)
        {
            foreach (var packModeConfig in packTypeConfigList)
            {
                if (path.StartsWith(packModeConfig.path))
                {
                    return true;
                }
            }
            return false;
        }

        //使用前需强制刷新一次
        [NonSerialized]
        public List<string> curMainResPath;
        public List<string> ForceRefreshMainResPathList()
        {
            curMainResPath = new List<string>(mainResDirList);
            foreach (var group in packGroupList)
            {
                string[] childs = group.childDirName.Split('#');
                foreach (var dir in group.mainResDirList)
                {
                    foreach (var child in childs)
                    {
                        string[] results = Directory.GetDirectories(dir, child, SearchOption.AllDirectories);
                        foreach (var result in results)
                        {
                            var r = result.Replace("\\", "/");
                            curMainResPath.Add(r);

                        }
                    }
                }
            }
            return curMainResPath;
        }
        //是否被过滤的资源
        public bool IsInFilterFolder(string path)
        {
            int count = mainResFilterDirList.Count;
            for (int i = 0; i < count; i++)
            {
                if (path.StartsWith(mainResFilterDirList[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInMainAsset(string asset)
        {
            var searchFolder = curMainResPath;
            if (searchFolder != null)
            {
                var seachFolderCount = searchFolder.Count;
                for (int i = 0; i < seachFolderCount; i++)
                {
                    if (asset.StartsWithEx(searchFolder[i]))
                    {
                        return true;
                    }
                }
            }

            //特殊的动态资源，转换成主资源
            foreach(DynamicMainResGroup dynamicGroup in dynamicGroupList)
            {
                if (asset.StartsWithEx(dynamicGroup.mainResDir)&& asset.EndsWithEx(dynamicGroup.suffixName))
                {
                    return true;
                }
            }

            return false;

        }

        //是否能被依赖
        public bool CanBeDependent(string asset)
        {
          
            //特殊动态资源，删除依赖
            foreach (DynamicMainResGroup dynamicGroup in dynamicGroupList)
            {
                if (asset.StartsWithEx(dynamicGroup.mainResDir) && asset.EndsWithEx(dynamicGroup.suffixName))
                {
                    return dynamicGroup.beDependent;
                }
            }

            return true;

        }

        //是否能被压缩
        public bool CanCompresseed(string asset)
        {

            int nCount = uncompresseedDirList.Count;
            for(int i=0;i<nCount;++i)
            {
                if (asset.IndexOf(uncompresseedDirList[i])>=0)
                {
                    return false;
                }
            }

            return true;

        }

    }
}