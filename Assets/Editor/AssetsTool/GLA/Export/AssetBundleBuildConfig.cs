/*******************************************************************
** 文件名: AssetBundleBuildConfig.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:     郑秀程
** 日  期:    2016/3/29
** 版  本:    1.0
** 描  述:    AssetBundle打包配置类
** 应  用:    用来记录AssetBundle打包的相关配置

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleBuildConfig : ScriptableObject
{
    public enum BuildTargetMode
    {
        CurrentPlatform,
        SpecifiedPlatforms,
    }

    public enum BuildPlatform
    {
        Android = BuildTarget.Android,
        IOS = BuildTarget.iOS,
        Window = BuildTarget.StandaloneWindows,
    }

    public enum CompressionType
    {
        UnCompressed = BuildAssetBundleOptions.UncompressedAssetBundle,
        ChunkCompressed_LZ4 = BuildAssetBundleOptions.ChunkBasedCompression,    //Chunk-Based,支持分块读取，性能最优，但压缩比率较低（相对于LZMA）
        StreamCompressed_LZMA = BuildAssetBundleOptions.None,                   //压缩比率最高，性能较低（相对于LZ4）
    }


    [System.Serializable]
    public class PathConfig
    {
        public Object dir;
        public string pathString;
    }

    [System.Serializable]
    public class PackCongfig:PathConfig
    {
        //是否将目录下的所有资源打个一个AssetBundle中，默认是每一个子目录打到一个AssetBundle
        public bool allInOne = false;
    }

    public BuildTargetMode buildPlatformMode = BuildTargetMode.CurrentPlatform;
    public CompressionType compressionType = CompressionType.ChunkCompressed_LZ4;
    public string outputRootDir;
    public bool dryRunBuild = false;                //是否不进行实际的资源打包，只生成对应的配置文件
    public List<BuildPlatform> buildPlatforms = new List<BuildPlatform>();

    public bool enableEncypt;
    public string encyptKey;
    public List<PathConfig> encryptDirs = new List<PathConfig>();          //需加密的目录

    public bool includDlls;
    public List<PathConfig> includedDllDirs = new List<PathConfig>();          //需加密的目录

    //指定的目录按目录打包，其他的目录每个文件一个AssetBundle
    public List<PackCongfig> packDirs = new List<PackCongfig>();

    //指定的目录每个文件单独打包，用于设置按目录打包的目录里面的部分子目录
    public List<PathConfig> packAloneDirs = new List<PathConfig>();

    //指定的目录将按照依赖打包(里面的资源在打包的时候会把所依赖的资源也打进来)
    public List<PathConfig> packByDependenciesDirs = new List<PathConfig>();

    //忽略依赖关系的目录，里面的资源当被按照依赖打包的资源依赖，也不会跟依赖它的资源打包到一起
    public List<PathConfig> ignoreDependencyDirs = new List<PathConfig>();

    //需要打图集的UI资源的目录（只需配置根目录，通过配置可以省去逐一判断是否为需要打包成图集的资源的消耗）
    public List<PathConfig> spriteDirs = new List<PathConfig>();

	public bool needPostProgressWithOutPutFiles = false;             //是否对所有输出文件做加工（如加后缀，删除.Mainifest文件）
	public string outputFileSuffix = "dat";                          //输出文件统一后缀，needPostProgressWithOutPutFiles为True时生效
	public bool delManifestFiles = false;							 //是否删除.Manifest文件(实际发布不需要相关的.Manifest文件)
    public bool delCsvFiles = false;							 //是否删除.csv文件(实际发布不需要相关的.csv文件)
}
