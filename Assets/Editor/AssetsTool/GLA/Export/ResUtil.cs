//using UnityEngine;
//using System.Collections;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace rkt
//{
//    public class ResUtil
//    {
//        public enum Location
//        {
//            External,   //外部存放目录，Application.persistentDataPath
//            Internal,   //内部部存放目录，Application.streamingAssetsPath
//            Remote,     //远程存放目录（服务器上）
//        }
//        public const int ABDataABID = 0;
//        public const int ABDataAssetID = 0;        
//        public const string AssetBundleSuffix = ".data";              //所有的AssetBunde的通用后缀,防止AssetBundle目录和AssetBundle重名       
//        public const string ABDataFileName = "ABdata.asset";
//        public const string ConfigFileName = "Config.csv";
//        public const string ObfuscatedABFolderName = "/AB/";

//        public const string PathCombineFormat = "{0}/{1}";
//        public const string TmpFilePathCombinleFormat = "{0}/_{1}";       

//        protected const string AssetBundleRootPathFormatString = "{0}/{1}/Data";
//        protected const string ExtractPackageDirString = "{0}/{1}";

//        public struct AssetData
//        {
//            public int assetBundleName;
//            public string assetName;

//            public AssetData(int assetBundleName, string assetName)
//            {
//                this.assetBundleName = assetBundleName;
//                this.assetName = assetName;
//            }
//        }


//#if UNITY_EDITOR

//        public const string ResourceFolderName = "GResources";        //动态加载资源文件夹名字（可以有多个，类似系统的Resource）
//        public const string ResourceFolderDirName = "GResources/";
//        public const string AssetMapFileName = "AssetMap.csv";
//        public const string AssetBundleListFileName = "AssetbundleList.csv";
//        public const string AssetBundleManifestName = "AssetBundleManifest";

//        //平台统一名字，用于编辑器显示和目录命名
//        protected const string PlatformNameAndroid = "Android";
//        protected const string PlatformNameIOS = "IOS";
//        protected const string PlatformNameWindows = "Windows";
//        //其他平台暂用Others表示,当打包选择当前平台的时候可能会使用到
//        private const string PlatformNameOther = "Other";

//        public struct OriAssetData
//        {
//            public string assetBundleName;
//            public string assetName;

//            public OriAssetData(string assetBundleName, string assetName)
//            {
//                this.assetBundleName = assetBundleName;
//                this.assetName = assetName;
//            }
//        }

//        public static string GetPlatformNameForBuildTarget(BuildTarget target)
//        {
//            switch (target) {
//                case BuildTarget.Android:
//                    return PlatformNameAndroid;
//                case BuildTarget.iOS:
//                    return PlatformNameIOS;
//                case BuildTarget.StandaloneWindows:
//                case BuildTarget.StandaloneWindows64:
//                    return PlatformNameWindows;
//                default:
//                    return PlatformNameOther;
//            }
//        }

//        //根据资源的GUID获取资源的加载路径，用于后面的资源加载，不管是AssetBundle还是Resource的加载方式，这里返回的路径应该是一致的，资源加载的时候，分别对路径进行识别
//        public static string GUIDToAssetPath(string guid)
//        {
//            //对于NestPrefab，PrefabUtility.GetPrefabParent会找到RootPrefab，所以这里使用另外的方式处理
//            //tmpPrefabPath = AssetDatabase.GetAssetOrScenePath(PrefabUtility.GetPrefabParent(adornings[i].gameObject));
//            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
//            return GetAssetLoadPath(assetPath);
//        }

//        //根据AssetDataBase的路径获取资源的加载路径，用于后面的资源加载
//        public static string GetAssetLoadPath(string assetPath)
//        {
//            if (!string.IsNullOrEmpty(assetPath)) {
//                int pathStartIndex = assetPath.IndexOf(ResourceFolderDirName);
//                if (pathStartIndex != -1) {
//                    int skipLength = ResourceFolderDirName.Length;
//                    pathStartIndex = pathStartIndex + skipLength;
//                    int pathEndIndex = assetPath.LastIndexOf('.');
//                    if (pathEndIndex != -1) {
//                        assetPath = assetPath.Substring(pathStartIndex, pathEndIndex - pathStartIndex);
//                    } else {
//                        assetPath = assetPath.Substring(pathStartIndex);
//                    }
//                }
//            }
//            return assetPath;
//        }
//#endif


//        public static string GetCurrentPlatformName()
//        {
//#if UNITY_EDITOR
//            //return GetPlatformNameForBuildTarget(EditorUserBuildSettings.activeBuildTarget);
//            return PlatformNameWindows;
//#else
//            return GetPlatformNameForRuntimePlatform(Application.platform);
//#endif
//        }

//        public static string GetPlatformNameForRuntimePlatform(RuntimePlatform platform)
//        {
//            switch (platform) {
//                case RuntimePlatform.Android:
//                    return PlatformNameAndroid;
//                case RuntimePlatform.IPhonePlayer:
//                    return PlatformNameIOS;
//                case RuntimePlatform.WindowsPlayer:
//                case RuntimePlatform.WindowsEditor:
//                    return PlatformNameWindows;
//                default:
//                    return PlatformNameOther;
//            }
//        }

//        public static string GetServerUrl()
//        {
//            return "http://172.16.0.32:80/AssetBundles";
//        }

//        public static string GetAssetBundleRootPathExternal()
//        {
//            return string.Format(AssetBundleRootPathFormatString, Application.persistentDataPath, ResUtil.GetCurrentPlatformName());
//        }

//        public static string GetAssetBundleRootPathInternal()
//        {
//            return string.Format(AssetBundleRootPathFormatString, Application.streamingAssetsPath, ResUtil.GetCurrentPlatformName());
//        }

//        public static string GetExtractPackageDir()
//        {
//            return string.Format(ExtractPackageDirString, Application.persistentDataPath, ResUtil.GetCurrentPlatformName());
//        }


//        /// <summary>
//        /// 用户包目录
//        /// </summary>
//        public static string OuterPackageDirectory
//        {
//            get {
//                return UserPath + "Package/";
//            }
//        }

//        public static string UserPath
//        {
//            get {
//                string directory = "";
//                // 初始化用户根目录
//                if (Application.platform == RuntimePlatform.Android) {
//                    //安卓平台
//                    directory = Application.persistentDataPath + "/User/";
//                } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
//                    //IPhone平台
//                    string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.Length - 5);
//                    directory = path.Substring(0, path.LastIndexOf('/')) + "/Documents/";
//                } else {
//                    //PC平台
//                    directory = Application.dataPath + "/User/";
//                }
//                return directory;
//            }
//        }

//        /// <summary>
//        /// 内部包目录
//        /// </summary>
//        public static string InterPackageDirectory
//        {
//            get {
//                string directory = "";
//                if (Application.platform == RuntimePlatform.Android) {
//                    directory = Application.streamingAssetsPath + "/Android/Data";
//                } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
//                    directory = "file:///" + Application.streamingAssetsPath + "/IOS/Data";
//                } else {
//                    directory = "file:///" + Application.streamingAssetsPath + "/Windows/Data";
//                }

//                return directory;
//            }
//        }

//        /// <summary>
//        /// 内部包目录
//        /// </summary>
//        public static string InterPackageDirectoryEx
//        {
//            get {
//                string directory = "";
//                if (Application.platform == RuntimePlatform.Android) {
//                    directory = Application.streamingAssetsPath + "/Android/";
//                } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
//                    directory = Application.streamingAssetsPath + "/Iphone/";
//                } else {
//                    directory = Application.streamingAssetsPath + "/Web/";
//                }

//                return directory;
//            }
//        }

//        public static void UnloadAsset(UnityEngine.Object asset)
//        {
//            if (asset == null) {
//                return;
//            }

//            System.Type assettype = asset.GetType();

//            bool handled = false;
//            for (int k = 0; k < 10; k++) {
//                if (assettype == null) {
//                    break;
//                }

//                if (assettype == typeof(AnimationClip) ||
//                    assettype == typeof(Texture2D) ||
//                    assettype == typeof(Texture3D) ||
//                    assettype == typeof(AnimationState) ||
//                    assettype == typeof(Material) ||
//                    assettype == typeof(Shader) ||
//                    assettype == typeof(Avatar) ||
//                    assettype == typeof(ScriptableObject) ||
//                    assettype == typeof(Mesh)) {
//                    Resources.UnloadAsset(asset);
//                    handled = true;
//                    break;
//                } else if (assettype == typeof(GameObject) ||
//                        assettype == typeof(Transform) ||
//                        assettype == typeof(AssetBundle) ||
//                        assettype == typeof(Component)) {
//                    handled = true;
//                    break;
//                } else {
//                }

//                assettype = assettype.BaseType;
//            }

//            if (handled == false) {
//                XGame.Trace.TRACE.TraceLn("UnloadSingleAssetImp:" + asset);
//                Resources.UnloadAsset(asset);
//            }
//        }

//        private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
//        public static byte[] EncryptDES(byte[] inputdata, string encryptKey)
//        {
//            try {
//                byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
//                byte[] rgbIV = Keys;
//                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
//                MemoryStream mStream = new MemoryStream();
//                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
//                cStream.Write(inputdata, 0, inputdata.Length);
//                cStream.FlushFinalBlock();
//                cStream.Close();
//                return mStream.ToArray();
//            } catch {
//                return null;
//            }
//        }

//        public static byte[] DecryptDES(byte[] inputdata, string decryptKey)
//        {
//            try {
//                byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
//                byte[] rgbIV = Keys;
//                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
//                MemoryStream mStream = new MemoryStream();
//                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
//                cStream.Write(inputdata, 0, inputdata.Length);
//                cStream.FlushFinalBlock();
//                cStream.Close();
//                return mStream.ToArray();
//            } catch {
//                return null;
//            }
//        }
//    }

//}