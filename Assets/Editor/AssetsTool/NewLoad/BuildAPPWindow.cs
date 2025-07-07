using XGame.Asset.Manager;
using XGame.I18N;
using XGameEditor.AssetImportTool;
using XGameEditor.NewLoad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XGame;
using XGame.Asset.Load;

#if SUPPORT_I18N
using XGameEditor.I18N;
#endif

using System.Runtime.Serialization.Formatters.Binary;
using XGame.Utils;
using System.Text;
using static XGameEditor.NewGAssetDataCache;
using ReferenceFinder;
using UnityEngine.Build.Pipeline;
using UnityEditor.Build.Pipeline;
using Debug = UnityEngine.Debug;
using TMPro;
using static XGame.UI.Grey.GraphicGreyManager;

//using Zip;

namespace XGameEditor
{
    public class BuildAPPWindow : EditorWindow
    {

        //启动热更前需要加载的Assembly文件目录配置(热更需要重启的目录)
        public static List<string> s_StartupAssemblyFolders { get; } = new List<string>()
        {
                "BaseScripts/Update",
               // "srcClient/XGame/XGameBase",
               // "srcClient/XGame/XGameEngine", //连重启都不需要了
        };

        //需要重装的目录
        public static List<string> s_ReinstallAssemblyFoldres { get; } = new List<string>()
    {
            // "BaseScripts/Update", //暂时留这里稍后移动到重启目录
            "srcClient/XGame/XGameBase",
            //"srcClient/XGame/XGameEngine",
            //上面这一批没有改代码前都当作需要重装
            "BaseScripts/HybridCLR",
            "Plugins/",
    };

        /// <summary>
        /// 构建平台
        /// </summary>
        public enum BuildPlatform
        {
            Android = 0,
            IOS,
            Windows,
            WebGL,
        }

        public class PosInfo
        {
            public AB_RES_POS abResPos;
            public string path;
        }

        /// <summary>
        /// 平台对应名字
        /// </summary>
        static public readonly Dictionary<BuildPlatform, string> _platformNameDic = new Dictionary<BuildPlatform, string>()
        {
            { BuildPlatform.Android, "android"},
            { BuildPlatform.IOS, "IOS"},
            { BuildPlatform.Windows, "Windows"},
            { BuildPlatform.WebGL, "WebGL"},
        };

        static public string CurPlatformName { get => _platformNameDic[_curBuildPlatform]; }
        static public string ABFolderName { get => $"{UpdateConfig.DATA_DIR}"; }
        static public string StreamingABFolderPath { get => $"{Application.streamingAssetsPath}/{UpdateConfig.DATA_DIR}"; }
        static public string GenABBaseFolder = "";//{ get => $"{Application.dataPath}/../BuildAPP";}
        static public string GenVersionPath { get => $"{GenABBaseFolder}/pkg_{_versionStr}"; }
        static public string GenABPath { get => $"{GenVersionPath}/{CurPlatformName}"; }
        static public string GenVersionLogPath { get => $"{GenVersionPath}/log_{_versionStr}.txt"; }
        static public string ABOutputPath { get => $"{GenABPath}/{ABFolderName}"; }
        static public string ABManifestPath { get => $"{ABOutputPath}/android"; }
        //static public string MainManifestName { get => "data.manifest"; }

        static public string BuildConfigPath { get => $"{Application.dataPath}/../BuildAPP"; }
        static public string BuildApkConfigPath { get => $"{BuildConfigPath}/BuildApkFromCmdConfig.json"; }
        static public string BuildStateTextPath { get => $"{BuildConfigPath}/BuildState.txt"; }
        static public string BuildSendDingDingBatPath { get => $"{BuildConfigPath}/SendBuildApkOKToDingDing.bat"; }

        //拆分资源相关路径
        static public string InnerPath { get => $"{GenVersionPath}/apk/{ABFolderName}"; }
        static public string FixPath { get => $"{GenVersionPath}/fix/{ABFolderName}"; }
        static public string WebPath { get => $"{GenVersionPath}/publish/web_{_versionStr}"; }

        static public string BuildDllDevEnvKey = "devenv";
        static public string BuildDllConfigurationKey = "configuration";
        static public string BuildDllSlnPahtKey = "slnPath";
        static public string BuildApkNameKey = "apkName";
        static public string BuildUploadURLKey = "uploadURL";
        static public string BuildUploadUserNameKey = "uploadUserName";
        static public string BuildUploadPasswordKey = "uploadPassword";

        static public string SelectABResListFilePath { get => $"{GenABBaseFolder}/SelectList.txt"; }
        static public string SelectABOutputPath { get => $"{GenABPath}"; }
        static public string SelectABOutputABFolderPath { get => $"{SelectABOutputPath}/{ABFolderName}"; }
        static public string SelectABDebugGenInfoPath { get => $"{SelectABOutputPath}/DebugSelectOutputInfo.txt"; }
        static public string LuaFolderPath { get => $"{Application.dataPath}/G_Resources/Game/Lua"; }
        static public string LuaGenPath { get => $"{LuaFolderPath}/{LuaGenerateName}"; }
        static public string LuaSourceName = "Sources";
        static public string LuaGenerateName = "Generate";

        static public string MicroABInputPath { get => $"{_microAllABPath}/{ABFolderName}"; }
        static public string MicroABInputManifestPath { get => $"{MicroABInputPath}/android"; }
        static public string MicroGenInnerABFolderPath { get => $"{_microAllABPath}_micro"; }
        static public string MicroGenInnerOutputABFolderPath { get => $"{MicroGenInnerABFolderPath}/{ABFolderName}"; }
        static public string MicroBackDownloadBinPath { get => $"{MicroGenInnerABFolderPath}/{ResourceConfigManager.BackDownloadFileName}"; }
        static public string MicroStreamingAndroidDataPath { get => $"{Application.streamingAssetsPath}/Android/Data/{ResourceConfigManager.BackDownloadFileName}"; }
        static public string MicroDebugGenInnerInfoPath { get => $"{MicroGenInnerABFolderPath}/MicroInnerInfo.txt"; }
        static public string MicroDebugGenOuterInfoPath { get => $"{MicroGenInnerABFolderPath}/MicroOuterInfo.txt"; }

        static public string NewABDebugGenInfoPath { get => $"{GenVersionPath}/DebugOutputInfo_New.txt"; }

        static public string AssetCacheChildName { get => $"NewAssetDataCache"; }
        static public string AssetPathABMapFileName { get => $"Path2AB.txt"; }

        static public string RedundantResFileName { get => $"RedundantRes.txt"; }
        static public string SelectABOutputAssetCacheChildPath { get => $"{GenVersionPath}/{AssetCacheChildName}"; }

        static public bool IsBackupAOTDll => _isBackupAOTDll;

        private Vector2 _guiScrollPos = new Vector2();
        private Dictionary<string, bool> _showBoxDic = new Dictionary<string, bool>();

#if UNITY_IOS
        static private BuildPlatform _curBuildPlatform = BuildPlatform.IOS;
#elif UNITY_ANDROID
        static private BuildPlatform _curBuildPlatform = BuildPlatform.Android;
#elif UNITY_WEBGL
         static private BuildPlatform _curBuildPlatform = BuildPlatform.WebGL;
#else
        static private BuildPlatform _curBuildPlatform = BuildPlatform.Windows;
#endif
        static private string _versionStr = string.Empty;
        static private bool _isAutoCopyToPulishFolder = true;
        //static private bool _isAutoSetABPath = true;

        static private string _hotUpdateABOldPath = string.Empty;
        static private string _hotUpdateABNewPath = string.Empty;
        static private string _hotUpdateOutputPath = string.Empty;

        static private string _hotUpdateStartVersion = string.Empty;
        static private string _hotUpdateEndVersion = string.Empty;

        static private string _microAllABPath = string.Empty;
        static private string _microRecordPath = string.Empty;

        //打包配置x相关
        static private bool _isCompatibleType = true; //AB资源兼容模式
        static private bool _isDiscardLuaLog = true; //AB资源兼容模式
        static private bool _isFullPack = true; //是否完整包
        static private bool _isEnableHybridCLR = false;//是否使用华佗热更
        static private bool _isBackupAOTDll = false; //是否备份AOT DLL
        static private bool _isSplitInner = true;//是否分包
        static private bool _isSplitFix = true; //是否拆分固件包
        static private bool _isSplitWeb = true; //是否拆分Web包
        static private bool _isIncrementABPack = true; //是否使用增量AB打包
        static private bool _isSurportObb = false; //是否打obb google包
        static private bool _isNoCSCodeUpdate = true; //是否允许代码热更
        static private bool _isDevelopment = false; //是否打开发版本的包，非代码热更有效
        static private bool _isResHotupdateVer = false; //生成纯资源热更版本
        static private bool _isUpLoad = false; //上传到下载路径
        static private bool _isSkipI18N = false; //是否跳过国际化
        

        //分包列表
        static List<Dictionary<string, bool>> _listSplitRes = new List<Dictionary<string, bool>>();

      

        //包体位置索引表
        static Dictionary<string, PosInfo> _dicBundlePos = new Dictionary<string, PosInfo>();

        //监测的资源目录表格
        static string[] _SpiltCheckList = { "BuildCfg/inner_cfg", "BuildCfg/fix_cfg", "BuildCfg/web_cfg" };

        static string _publishTargetDir = "D:/httpd-2.4.46-o111i-x64-vc15/Apache24/htdocs/download";

#region 资源检测相关
        static private bool _isCheckCodeModified = true; //代码是否有变更

        static private string ResCheckFilePath { get => $"{SelectABOutputPath}/ResCheckResult.txt"; }
        static private List<string> _resCheckNoPassList = new List<string>();
#endregion

        [MenuItem("XGame/打包工具/打包窗口")]
        public static void ShowWindow()
        {
            _curBuildPlatform = GetCurPlatform();
            EditorWindow window = GetWindow<BuildAPPWindow>("XGame打包工具");
            window.Show();
        }

        private void OnGUI()
        {
            using (var srocllArea = new EditorGUILayout.ScrollViewScope(_guiScrollPos))
            {
                _guiScrollPos = srocllArea.scrollPosition;

                _curBuildPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("打包平台", _curBuildPlatform);
                using (var versionArea = new EditorGUILayout.HorizontalScope())
                {
                    if (string.IsNullOrEmpty(_versionStr))
                        _versionStr = System.DateTime.Now.ToString("yyMMddHHmm");
                    _versionStr = EditorGUILayout.TextField("版本号：", _versionStr);
                    var path = Application.streamingAssetsPath + "/" + UpdateConfig.UPDATE_CONFIG;
                    if (GUILayout.Button($"刷新版本配置:{path}"))
                    {
                        SetVersionStr();
                        EditorUtility.RevealInFinder(path);
                    }
                }

                // DrawBox("拷贝文件", DrawCopyFiles);
                DrawBox("Lua打包加密", DrawEncryLua);
                DrawBox("删除模型的默认材质", DrawDeleteModelDefualtMatrial);
                DrawBox("国际化配置", DrawI18NConfig);
                DrawBox("shader变体收集", DrawCollectVariantApart);
                DrawBox("新版AB路径设置", DrawNewABPath, true);
                //DrawBox("打AB包", DrawBuidlAB);
                DrawBox("打热更包", DrawHotUpdate);
                // DrawBox("打微端包", DrawMicroClient);
                DrawBox("测试", DrawTest);
                //转换录制资源
                DrawBox("录制资源", DrawResRecord);
                DrawBox("AB资源解密", DrawABDecode);
                DrawBox("重新生成版本号文件(手动更改后)", DrawEncryVerInfo);


            }
        }

#region 通用绘制界面
        private void DrawBox(string label, Action customDraw, bool isDefualtShow = false)
        {
            if (!_showBoxDic.ContainsKey(label))
            {
                _showBoxDic.Add(label, isDefualtShow);
            }
            using (var area = new GUILayout.VerticalScope("box"))
            {
                _showBoxDic[label] = EditorGUILayout.Foldout(_showBoxDic[label], label);
                if (_showBoxDic[label])
                {
                    customDraw.Invoke();
                }
            }
        }


        /// <summary>
        /// 绘制文件夹浏览配置
        /// </summary>
        /// <param name="label"></param>
        /// <param name="result"></param>
        /// <param name="isFolder"></param>
        private void DrawFolderEdit(string label, ref string result, bool isFolder = true, bool isCheckExists = true)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                result = EditorGUILayout.TextField(label, result);
                if (GUILayout.Button("浏览"))
                {
                    result = isFolder ? EditorUtility.OpenFolderPanel(label, result, "") :
                        EditorUtility.OpenFilePanel(label, result, "");
                }
                if (isCheckExists && ((isFolder && !Directory.Exists(result)) || (!isFolder && !File.Exists(result))))
                {
                    GUILayout.Box("", "Wizard Error", GUILayout.Width(32), GUILayout.Height(16));
                }
            }

        }



        // 绘制选中文件
        private void DrawPingObj(string desc, UnityEngine.Object obj)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(desc);
                if (GUILayout.Button("选中文件"))
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
        }



#endregion

#region 输出日志
        static private string DebugLog(string str, int level = 1)
        {
            string prefix = string.Empty;
            if (level <= 1)
            {
                prefix = "Normal";
                //Debug.Log(str);
            }
            else if (level <= 2)
            {
                prefix = "Warning";
                //Debug.LogWarning(str);
            }
            else if (level <= 3)
            {
                prefix = "Error";
                //Debug.LogError(str);
            }
            var result = $"【【{DateTime.Now.ToString("[HH:mm:ss]")}[{prefix}]{str}】】\n";
            if (!string.IsNullOrEmpty(_versionStr))
            {
                Debug.Log(result);
                try
                {
                    var dir = Path.GetDirectoryName(GenVersionLogPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.AppendAllText(GenVersionLogPath, result);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            return result;
        }
#endregion

#region 拷贝文件



#endregion

#region 设置版本号
        // 设置版本号
        public static bool SetVersionStr()
        {
            //删除外包路径的更新文件
            if (File.Exists(UpdateConfig.updateConfigPath))
            {
                File.Delete(UpdateConfig.updateConfigPath);
            }
            var path = Application.streamingAssetsPath + "/" + UpdateConfig.UPDATE_CONFIG;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                try
                {
                    var xmlVersionInfo = new XMLVersionInfo();
                    var info = xmlVersionInfo.GetVersionInfo(sr.BaseStream);
                    sr.Close();
                    sr.Dispose();
                    if (info != null)
                    {
                        ulong verValue;
                        if (!ulong.TryParse(_versionStr, out verValue))
                        {
                            Debug.LogError($"设置版本号失败！字符串转换整形失败！_versionStr:{_versionStr}");
                            return false;
                        }

                        Debug.LogWarning($"设置版本号:{info.localVerNo} -> {verValue}\n路径：{path}");
                        info.localVerNo = verValue;

                        //有固件包
                        info.isNeedDownloadFixedItem = false;

                        //设置是否有固件
                        if (Directory.Exists(FixPath))
                        {
                            string[] files = Directory.GetFiles(FixPath, "*.*", SearchOption.AllDirectories);
                            if (files.Length > 0)
                            {
                                //有固件包
                                info.isNeedDownloadFixedItem = true;
                            }
                        }
                        
                        info.isEnableHybridCLR = _isEnableHybridCLR;

                        return xmlVersionInfo.SetVersionInfo(info, path);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return false;
                }
                finally
                {
                    sr.Close();
                    sr.Dispose();
                }

            }
            return false;
        }

#endregion

#region 刷新必要资源MD5
        // 刷新资源MD5
        public static bool UpdateCheckMD5()
        {
            //删除外包路径的更新文件
            if (File.Exists(UpdateConfig.updateConfigPath))
            {
                File.Delete(UpdateConfig.updateConfigPath);
            }
            var path = Application.streamingAssetsPath + "/" + UpdateConfig.UPDATE_CONFIG;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                try
                {
                    var xmlVersionInfo = new XMLVersionInfo();
                    var info = xmlVersionInfo.GetVersionInfo(sr.BaseStream);
                    sr.Close();
                    sr.Dispose();
                    if (info != null)
                    {
                        var newDic = new Dictionary<string, string>();
                        foreach (var item in info.assetMD5Dic)
                        {
                            var assetPath = item.Key;
                            if (string.IsNullOrEmpty(assetPath))
                                continue;
                            assetPath = $"{StreamingABFolderPath}/{assetPath}";
                            if (!File.Exists(assetPath))
                            {
                                Debug.Log($"刷新检查MD5失败！ 找不到资源:{assetPath}");
                                continue;
                            }
                            newDic.Add(item.Key, MD5.GetMD5(assetPath));
                        }

                        info.assetMD5Dic = newDic;
                        return xmlVersionInfo.SetVersionInfo(info, path);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return false;
                }
                finally
                {
                    sr.Close();
                    sr.Dispose();
                }

            }
            return false;
        }

#endregion

#region 新版打AB

        [MenuItem("XGame/打包工具/刷新缓存并导出AB映射文件")]
        public static void RefreshCacheRecordAndExport()
        {
            ReferenceFinderData.CollectDependenciesInfo();
            NewGAssetDataCache.Instance.RefreshCache();
            NewGAsset2BundleRecords.Instance.RefreshRecord();

            //导出资源索引表
            string path = UpdateConfig.appInnerDir + UpdateConfig.ASSET_BUNDLE_RECORD;
            NewGAsset2BundleRecords.Instance.Export(path);

            //设置资源位置
            __BuildSplitBundlePosCfg(_listSplitRes, _dicBundlePos);


        }

        private string _findAssetPathName;
        public string[] _bundleNameList = new string[2];
        private Dictionary<string, string> _assetABName = new Dictionary<string, string>();
        private void DrawNewABPath()
        {

            if (string.IsNullOrEmpty(_publishTargetDir))
            {
                _publishTargetDir = $"{Application.dataPath}/../WebPublish";
            }

            if (Directory.Exists(_publishTargetDir) == false)
            {
                _publishTargetDir = _publishTargetDir.Replace("D:", "C:");
            }


            DrawFolderEdit("发布的http目录", ref _publishTargetDir);



            if (string.IsNullOrEmpty(GenABBaseFolder))
            {
                GenABBaseFolder = $"{Application.dataPath}/../BuildAPP";
            }
            DrawFolderEdit("生成AB路径", ref GenABBaseFolder);
            EditorGUILayout.LabelField($"生成AB路径：{SelectABOutputPath}");
            EditorGUILayout.LabelField($"实际输出路径：{SelectABOutputABFolderPath}");
            DrawPingObj($"配置文件路径：{NewAssetDataDef.AssetBuildConfigFilePath}", NewGAssetBuildConfig.Instance);
            DrawPingObj($"缓存文件路径：{NewAssetDataDef.AssetDataCacheFilePath}", NewGAssetDataCache.Instance);
            DrawPingObj($"缓存生成资源对应AB包名文件路径：{NewAssetDataDef.Asset2BundlePath}", NewGAsset2BundleRecords.Instance);
            _isCompatibleType = EditorGUILayout.Toggle("AB资源是否兼容模式：", _isCompatibleType);
            _isDiscardLuaLog = EditorGUILayout.Toggle("是否屏蔽lua日志：", _isDiscardLuaLog);

            _isCheckCodeModified = EditorGUILayout.Toggle("是否检查代码：", _isCheckCodeModified);
            _isIncrementABPack = EditorGUILayout.Toggle("是否增量打AB资源：", _isIncrementABPack);
            _isAutoCopyToPulishFolder = EditorGUILayout.Toggle("是否自动拷贝到发布资源路径：", _isAutoCopyToPulishFolder);
            _isSplitFix = EditorGUILayout.Toggle("是否拆分固件资源：", _isSplitFix);
            _isSplitWeb = EditorGUILayout.Toggle("是否拆分We资源：", _isSplitWeb);
            _isFullPack = EditorGUILayout.Toggle("是否完整包：", _isFullPack);
            _isNoCSCodeUpdate = EditorGUILayout.Toggle("是否允许C#代码热更：", _isNoCSCodeUpdate);
            _isEnableHybridCLR = EditorGUILayout.Toggle("是否使用华佗热更：", _isEnableHybridCLR);
            _isSplitInner = EditorGUILayout.Toggle("是否进行分包：", _isSplitInner);
            _isBackupAOTDll = EditorGUILayout.Toggle("是否备份AOT DLL：", _isBackupAOTDll);
            
            //SettingsUtil.Enable = _isEnableHybridCLR;
            _isSurportObb = EditorGUILayout.Toggle("是否打obb包(google)：", _isSurportObb);
            _isDevelopment = EditorGUILayout.Toggle("是否开发版本：", _isDevelopment);
            _isResHotupdateVer = EditorGUILayout.Toggle("是否纯热更资源版本：", _isResHotupdateVer);
            _isUpLoad = EditorGUILayout.Toggle("是否上传到下载路径：", _isUpLoad);
            _isSkipI18N = EditorGUILayout.Toggle("是否跳过国际化处理：", _isSkipI18N);


            if (GUILayout.Button("编译发布版本dll"))
            {
                CompilerDLL();
            }


            if (GUILayout.Button("刷新缓存"))
            {
                NewGAssetDataCache.Instance.RefreshCache();
            }
            if (GUILayout.Button("生成资源对应AB包名文件"))
            {
                NewGAsset2BundleRecords.Instance.RefreshRecord();
            }

            if (GUILayout.Button("设置图集标签"))
            {
                //SpriteSetting.SetupBuildSpriteAltasCfg();
                SpriteSetting.DoSetSpriteAltas();
            }

            if (GUILayout.Button("刷新缓存 -> 生成资源对应AB包名文件 -> 导出"))
            {
                RefreshCacheRecordAndExport();
            }

            if (GUILayout.Button("生成AB包"))
            {
                BuildNewABRes();
            }

            if (GUILayout.Button("拆分资源(APK,固件，web)"))
            {
                SpiltABRes(SelectABOutputABFolderPath);
            }

            if (GUILayout.Button($"拷贝到Streaming下： {StreamingABFolderPath}"))
            {
                _CopyResToStreaming();

            }
            if (GUILayout.Button("刷新检查MD5文件"))
            {
                UpdateCheckMD5();
            }

            if (GUILayout.Button("打IL2CPP包"))
            {
             
                AndroidBuilder.BuildAll(CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, GenVersionPath, _isSurportObb);
            }

            if (GUILayout.Button("打非CS代码热更包"))
            {
                AndroidBuilder.BuildNoCodeHotUpdateAPK(_versionStr, CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, GenVersionPath, _isSurportObb);
            }

            if (GUILayout.Button("一键生成APK"))
            {
                BuildAPK();
            }

            if (GUILayout.Button("保存文件缓存信息(增量打包需要)"))
            {
                SerAssetDataCache(NewGAssetDataCache.Instance.assetDataList, SelectABOutputAssetCacheChildPath);
            }

            if (GUILayout.Button("生成增量AB"))
            {
                DoBuildInrementAB();
            }

            if (GUILayout.Button("生成发布资源(APK,Web,热更包，固件)"))
            {
                BuildPublish();
            }
        }

        static public BuildPlatform GetCurPlatform()
        {
            if (EditorUserBuildSettings.activeBuildTarget== BuildTarget.iOS)
            {
               return BuildPlatform.IOS;
            }else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                return BuildPlatform.Android;
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                return BuildPlatform.WebGL;
            }

            return BuildPlatform.Windows;
        }

        //生成一个APK
        static public bool BuildAPK()
        {
            HybridCLRGenerateAll();
            
            //复制热更新Dll到资源目录下
            CopyHotUpdateAssembliesToABPath();
            
            //清除字体的依赖
            ClearDepencyFont();

            //替换默认材质
            DeleleModelDefualtMatrial();

            //执行变体收集
            VariantCollectionsEditor.CollectVariantApart();

            //国际化处理
            if (!ProcessI18N())
            {
                if(_isSkipI18N==false)
                {
                    Debug.LogError("国际化处理失败！");

                    return false;
                }
            }

            //加一个资源更新
            AssetDatabase.Refresh();

            InitConfigs();
            // CompilerDLL();

            //同步一下spine的加载路径
            SpineAssets.SynSpineLoadPath();

            if (false == _isIncrementABPack)
            {
                GenEncryLua();
                SpriteSetting.DoSetSpriteAltas();
                RefreshCacheRecordAndExport();
                /*
                //保存一下路径到ab的索引
                SavePath2ABMap(GenVersionPath + "/" + AssetPathABMapFileName);
                BuildNewABRes();
                */
            }
            else //增量打AB方式
            {
                ulong NO;
                GetLastVersion(out NO, ulong.Parse(_versionStr));
                ulong luaLastWriteTime = GetLuaLastWriteTime();
                //if (luaLastWriteTime >= NO)
                {
                    GenEncryLua();
                }

               // ReferenceFinderData.CollectDependenciesInfo();
                
                HashSet<string> targetABSet = null;// new HashSet<string>();//实际需要打的AB
                HashSet<string> curNewABSet = null;
                HashSet<string> modifiedSet = null;
                string oldABAssetDir = GetLastAssetCacheDir();
                GetModifiedResInfo(oldABAssetDir, out targetABSet, out curNewABSet, out modifiedSet);
                SpriteSetting.DoSetSpriteAltas(modifiedSet);
                RefreshCacheRecordAndExport();

                //保存一下路径到ab的索引
                // SavePath2ABMap(GenVersionPath + "/" + AssetPathABMapFileName);

                // DoBuildInrementAB();
            }

            //保存一下路径到ab的索引
            SavePath2ABMap(GenVersionPath + "/" + AssetPathABMapFileName);
            BuildNewABRes();

            //拷贝配置到data目录
            __CopyConfig();

            SpiltABRes(SelectABOutputABFolderPath);
            _CopyResToStreaming();
            UpdateCheckMD5();
           

            if (_isResHotupdateVer == false)
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.Android:

                       // if (_isNoCSCodeUpdate)
                        {
                            AndroidBuilder.BuildNoCodeHotUpdateAPK(_versionStr,CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, GenVersionPath, _isSurportObb, _isDevelopment);
                        }
                       // else
                        {
                         //   AndroidBuilder.BuildAll(CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, GenVersionPath, _isSurportObb);
                        }

                        break;
                    case BuildTarget.iOS:
                        AndroidBuilder.ExportGradleProject(CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, _isSurportObb,GenVersionPath);
                        break;
                    case BuildTarget.StandaloneWindows64:
                        AndroidBuilder.BuildNoCodeHotUpdateAPK(_versionStr, CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, GenVersionPath, _isSurportObb, _isDevelopment);
                        break;
                    case BuildTarget.WebGL:
                        AndroidBuilder.BuildNoCodeHotUpdateAPK(_versionStr, CommandBuildBoot._appVer, CommandBuildBoot._bundleCodeVer, GenVersionPath, _isSurportObb, _isDevelopment);
                        break;
                    default:
                        Debug.LogError("当前平台不能打包: " + EditorUserBuildSettings.activeBuildTarget.ToString());
                        break;

                }
            }

            //保存当前版本号
            SaveAppVersion();

            return true;
        }

        //发布资源
        static public void BuildPublish()
        {
            if(!BuildAPK())
            {
                DingTalkHelper.Notify_fail();
                Debug.LogError("打包失败");
                return;
            }

            BuildPublishPackage();

            //拷贝资源记录缓存
            SerAssetDataCache(NewGAssetDataCache.Instance.assetDataList, SelectABOutputAssetCacheChildPath);

            if (_isUpLoad)
            {
                string path_apk = $"{BuildAPPWindow.GenVersionPath}/publish/com.hyl.civre.apk";
                string path_IosProject = $"{BuildAPPWindow.BuildConfigPath}/../IOSProject_v1.0/Info.plist"; 
                if (File.Exists(path_apk)||File.Exists(path_IosProject))
                {
                    FTPNetwork.FTP.Uploads();
                    Debug.LogError("上传完毕");
                    DingTalkHelper.Notify();

                }
                else
                {
                    DingTalkHelper.Notify_fail();
                    Debug.LogError("打包失败");
                }
            }
            
            //分包
            SplitInnerPackage();
        }

        static public void CompilerDLL()
        {
            AutoBuildDLL.BuildDLL();
        }

        static public void ClearDepencyFont()
        {
            //清除国际化的字体
            /*
            foreach (var item in I18NEditorConfig.Instance.FontConfig.items)
            {
                item.baseTmpFont?.fallbackFontAssetTable.Clear();
            }
            AssetDatabase.SaveAssets();
            */

            //设置TMP Setting的默认依赖，减少一份字体依赖
            TMP_Settings textSettings = AssetDatabase.LoadAssetAtPath("Assets/ThirdParty/TextMesh Pro/Resources/TMP Settings.asset", typeof(TMP_Settings)) as TMP_Settings; // 

            TMP_FontAsset defaultFontAsset = AssetDatabase.LoadAssetAtPath("Assets/ThirdParty/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset", typeof(TMP_FontAsset)) as TMP_FontAsset; // 
            if(null!= textSettings&&null!= defaultFontAsset)
            {
                textSettings.m_defaultFontAsset = defaultFontAsset;
                EditorUtility.SetDirty(textSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            

           

        }

        /// <summary>
        /// 国际化处理
        /// </summary>
        static public bool ProcessI18N()
        {

         

            //检查是否存在字符丢失
            /*
            var langs = new List<ELanguage>();
            I18NEditorUtility.FindI18NCharsChagned(langs);
            if (langs.Count > 0)
                return false;

            AssetDatabase.SaveAssets();
            */
            return true;
        }

        // 打新版规则AB
        static public bool BuildNewABRes(HashSet<string> targetABSet = null)
        {
            bool isSuccess = true;

            try
            {
                isSuccess = SetVersionStr();
                if (!isSuccess)
                {
                    Debug.LogError($"设置版本号失败！!");
                    return false;
                }

                // 创建所选资源目录
                if (Directory.Exists(SelectABOutputABFolderPath))
                    Directory.Delete(SelectABOutputABFolderPath, true);
                Directory.CreateDirectory(SelectABOutputABFolderPath);

                Dictionary<string, List<EditorA2BRecord>> assetBundleBulidDic = new Dictionary<string, List<EditorA2BRecord>>();
                //普通格式队列
                List<AssetBundleBuild> assetBundleBulidList = new List<AssetBundleBuild>();

                //不能压缩的队列
                List<AssetBundleBuild> assetBundleUncompressBulidList = new List<AssetBundleBuild>();

                Dictionary<string, string> resultRecord = new Dictionary<string, string>();

                List<string> assetNames = new List<string>();
                List<string> assetAddressNames = new List<string>();
                List<string> listDebugInfo = new List<string>();


                // 构建ab包
                var recordList = NewGAsset2BundleRecords.BuildABInstance.allRecord.RecordList;
                int count = recordList.Count;
                bool isCancel = false;
                for (int i = 0; i < count; i++)
                {

                    EditorA2BRecord record = recordList[i];
                    string abName = record.bundleName;


                    //假如打指定的ab包，存在才进行打包
                    if (targetABSet != null && !targetABSet.Contains(abName))
                    {
                        continue;
                    }

                    //Debug.LogError($"{i}  {record.assetPath}");
                    if (!assetBundleBulidDic.ContainsKey(abName))
                    {
                        assetBundleBulidDic.Add(abName, new List<EditorA2BRecord>());
                    }
                    var a2bList = assetBundleBulidDic[abName];
                    if (a2bList.Contains(record))
                    {
                        Debug.LogError($"添加AB包异常，映射表居然存在两个引用相同的记录对象！！！！assetName:{record.assetName}");
                        continue;
                    }
                    bool isFaild = false;
                    foreach (var item in a2bList)
                    {
                        if (item.assetName == record.assetName)
                        {
                            isFaild = true;
                            Debug.LogError($"同一个包中存在多个同名资源，改一下名字或者打包规则：bundle:{abName}, assets:{record.assetName}");
                            break;
                        }
                        if (item.assetPath.EndsWith(".unity"))
                        {
                            isSuccess = false;
                            Debug.LogError($"存在和场景文件同一个AB包的资源，bundle:{abName}, asset:{record.assetPath}");
                            //return isSuccess;
                            continue;
                        }
                    }
                    if (isFaild)
                    {
                        continue;
                    }


                    resultRecord.Add(record.assetPath, record.bundleName);
                    assetBundleBulidDic[abName].Add(record);
                    isCancel = EditorUtility.DisplayCancelableProgressBar($"收集映射资源: ({i} / {count})", $"{record.assetPath}", (float)i / count);
                    if (isCancel)
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                }
                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                    return false;
                }

                bool bCanCompress = true;
                int index = 0;
                int totalABCount = assetBundleBulidDic.Count;
                foreach (var item in assetBundleBulidDic)
                {

                    ++index;
                    string abName = item.Key;
                    var records = item.Value;
                    var recordCount = records.Count;


                    assetNames.Clear();
                    assetAddressNames.Clear();

                    bCanCompress = true;
                    for (int i = 0; i < recordCount; i++)
                    {
                        string path = records[i].assetPath;

                        if(path.StartsWith("Packages/")==false)
                        {
                            path = $"Assets/{records[i].assetPath}";
                        }else
                        {
                            //Debug.Log("打包pacjages 資源 path= "+ path);
                        }
                        
                        if(EditorUserBuildSettings.activeBuildTarget== BuildTarget.Android&& NewGAssetBuildConfig.Instance.CanCompresseed(path)==false)
                        {
                            bCanCompress = false;
                        }
                        assetNames.Add(path);
                        assetAddressNames.Add(records[i].assetName);
                        Debug.Log($"{index} 资源：{abName}， Assets/{records[i].assetPath}， {records[i].assetPath}");
                    }

                    AssetBundleBuild abBuild = new AssetBundleBuild();
                    abBuild.assetBundleName = abName;
                    abBuild.assetNames = assetNames.ToArray();
                    abBuild.addressableNames = assetAddressNames.ToArray();

                    if(bCanCompress)
                    {
                        assetBundleBulidList.Add(abBuild);
                    }else
                    {
                        //暂时安卓版本，MP4
                        assetBundleUncompressBulidList.Add(abBuild);
                    }
                    
                    isCancel = EditorUtility.DisplayCancelableProgressBar($"收集AB包: ({index} / {totalABCount})", $"{abName}", (float)index / totalABCount);
                    if (isCancel)
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                }
                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                    return false;
                }
                //return false;

                
                listDebugInfo.Clear();
                foreach (var key in resultRecord.Keys)
                {
                    listDebugInfo.Add(key + " => " + resultRecord[key]);
                }

                // 所有ab资源写到记录里面
                File.WriteAllLines(NewABDebugGenInfoPath, listDebugInfo);

                // 只检查依赖资源有没有设ab，不打AB包
                EditorUtility.DisplayCancelableProgressBar("准备打AB资源（这里会卡一会，资源越多越久）", $"AB资源数量：{assetBundleBulidList.Count}", 1);

                var option = (BuildAssetBundleOptions.ChunkBasedCompression & ~BuildAssetBundleOptions.StrictMode) | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension | BuildAssetBundleOptions.DisableWriteTypeTree;
                //if (_isCompatibleType)  //开了不兼容模式，编辑器可能启动不了AB
                {
                    option = BuildAssetBundleOptions.ChunkBasedCompression;
                }

                if (_isIncrementABPack == false)
                {
                   // option |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                }

                //没有新的资源就不打AB了
                if(assetBundleBulidList.Count>0)
                {
                    // 打AB
                    option |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
                                                               
                    CompatibilityAssetBundleManifest manifest =  CompatibilityBuildPipeline.BuildAssetBundles(SelectABOutputABFolderPath, assetBundleBulidList.ToArray(), option, EditorUserBuildSettings.activeBuildTarget);
                    if (manifest == null)
                        throw (new Exception("生成AB包失败！"));
                }

                if(assetBundleUncompressBulidList.Count>0)
                {
                    //删除压缩打包的文件夹
                    string unCompressedDir = SelectABOutputABFolderPath + "_unCompress";
                    if(Directory.Exists(unCompressedDir))
                    {
                        Directory.Delete(unCompressedDir, true);
                    }
                    option = BuildAssetBundleOptions.UncompressedAssetBundle|BuildAssetBundleOptions.ForceRebuildAssetBundle;
                    CompatibilityAssetBundleManifest manifest = CompatibilityBuildPipeline.BuildAssetBundles(unCompressedDir, assetBundleUncompressBulidList.ToArray(), option, EditorUserBuildSettings.activeBuildTarget);
                    if (manifest == null)
                        throw (new Exception("生成AB包失败！"));


                    //删除冗余文件
                    DelFiles(unCompressedDir, "*.manifest");
                    DelFiles(unCompressedDir, "*.json");

                    //合并资源到普通文件夹
                    __Copy2Directory(unCompressedDir, SelectABOutputABFolderPath);
                }

                //删除冗余文件
                DelFiles(SelectABOutputABFolderPath,"*.manifest");
                DelFiles(SelectABOutputABFolderPath, "*.json");


                //检测冗余资源
                //CheckRedundantRes(SelectABOutputABFolderPath, resultRecord);

                isSuccess = true;
            }
            catch (Exception e)
            {
                DebugLog($"打AB包异常： {e}", 3);
                isSuccess = false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }


      
            if(UpdateConfig.isEncryAssetBundle)
            {
                EncryptRes(SelectABOutputABFolderPath);
            }



            //导出一份资源索引目录
            // NewGAsset2BundleRecords.Instance.Export(SelectABOutputABFolderPath+ "/"+UpdateConfig.ASSET_BUNDLE_RECORD);



            return isSuccess;
        }


#endregion

        static private void  __CopyConfig()
        {
            //拷贝资源索引表bin
            string src = UpdateConfig.appInnerDir + UpdateConfig.ASSET_BUNDLE_RECORD;
            string dst = SelectABOutputABFolderPath + "/" + UpdateConfig.ASSET_BUNDLE_RECORD;
            File.Copy(src, dst, true);

            //拷贝 gameconfig.ini
            //同步一次Game.ini
            src = Application.dataPath + "/" + UpdateConfig.GAME_INI_CFG;
            dst = UpdateConfig.appInnerDir + UpdateConfig.GAME_INI_CFG;
            File.Copy(src, dst,true);
            dst = ABOutputPath + "/" + UpdateConfig.GAME_INI_CFG;
            File.Copy(src, dst, true);

            //拷贝Service文件
            _CopySingleFile("/Service/service.xml", "service.xml");
            _CopySingleFile("/Service/serverlist.xml", "serverlist.xml");
        }

        static private void _CopySingleFile(string srcFile, string dstFile)
        {
            string src = Application.dataPath + "/" + srcFile;
            string dst = UpdateConfig.appInnerDir + dstFile;
            File.Copy(src, dst, true);
            dst = ABOutputPath + "/" + dstFile;
            File.Copy(src, dst, true);
        }

#region 转Lua到Bytes
        static private void DrawEncryLua()
        {
            EditorGUILayout.LabelField($"Lua文件夹路径：{LuaFolderPath}");
            if (GUILayout.Button("转换Lua"))
            {
                GenEncryLua();
            }
        }

        static private ulong GetLuaLastWriteTime()
        {
            ulong lastWriteTime = 0;
            DirectoryInfo diInfo = new DirectoryInfo(LuaFolderPath);
            if (!diInfo.Exists)
            {
                DebugLog($"不存在Lua文件夹：{LuaFolderPath}", 3);
                return 0;
            }
            FileInfo[] fileInfos = diInfo.GetFiles("*.lua", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
                var file = fileInfos[i];
                ulong lastAceessTime = ulong.Parse(file.LastWriteTime.ToString("yyMMddHHmm"));
                if (lastWriteTime < lastAceessTime)
                {
                    lastWriteTime = lastAceessTime;
                }

            }

            return lastWriteTime;
        }


        static private void GenEncryLua()
        {
            if (Directory.Exists(LuaGenPath))
            {
                Directory.Delete(LuaGenPath, true);
            }
            if (EncryptLua())
            {
                //NewGAsset2BundleRecords.Instance.AddRecord("luascript", "luascript", string assetName)
                // NewGAssetDataCache.Instance.RefreshCache();
                // NewGAsset2BundleRecords.Instance.RefreshRecord();
                //Asset2BundleRecordsUtils.SetFolderABPathByMeta(LuaGenPath);
            }

            // AssetDatabase.Refresh();
            //AssetbundlesMenuItems.SetABPathByMeta(LuaGenPath, "luascript");      // 重置AB路径为"luascript"
            //Asset2BundleRecordsUtils.SetFolderABPathByMeta(LuaGenPath);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        //[MenuItem("XGame/其它/临时/测试Lua脚本代码优化")]
        //static void TestLuaCutDownCode()
        //{
        //    var path = "F:/Projects/Project1002/bin/client/Game/Assets/G_Resources/Game/Lua/Sources/hotrefresh.lua";
        //    string content = File.ReadAllText(path);
        //    content = CutDownCode(content);
        //    File.WriteAllText(path, content);
        //}

#region 删除代码里的注释和打印
        static string CutDownCode(string content)
        {
            //先把content当中块注释的部分剔除
            void cut_block_comment(string s_str, string e_str)
            {
                int cur_index = 0;
                while (true)
                {
                    int start_index = content.IndexOf(s_str, cur_index);
                    if (start_index < 0)
                        break;
                    if (start_index > 0)
                    {
                        char prev_char = content[start_index - 1];
                        if (prev_char == '-')
                        {
                            cur_index = start_index + s_str.Length;
                            if (cur_index < content.Length)
                                continue;
                            else
                                break;
                        }
                    }
                    int end_index = content.IndexOf(e_str, start_index);
                    if (end_index < 0)
                    {
                        //content = content.Substring(0, start_index - 1);
                        break;
                    }
                    else
                    {
                        string head = content.Substring(0, start_index);
                        string rear = content.Substring(end_index + e_str.Length);
                        content = head + rear;
                    }
                }
            }
            cut_block_comment("--[[", "]]");
            cut_block_comment("--[=[", "]=]");
            cut_block_comment("--[==[", "]==]");

            //引号数量是否是奇数个
            bool is_odd(string total_str, string find_str)
            {
                int len = total_str.Length - total_str.Replace(find_str, "").Length;
                int count = len / find_str.Length;
                return count % 2 == 1;
            }

            //是否有成对括号
            bool not_pair_bracket(string total_str)
            {
                int left = total_str.Length - total_str.Replace("(", "").Length;
                int right = total_str.Length - total_str.Replace(")", "").Length;
                return left != right;
            }

            void cut_str(string s_str, string e_str)
            {
                int cur_index = 0;
                while (true)
                {
                    if (cur_index > content.Length)
                        break;
                    int start_index = content.IndexOf(s_str, cur_index);
                    cur_index = start_index + s_str.Length;
                    if (start_index < 0)
                        break;
                    if (start_index > 0)
                    {
                        while(start_index > 0)
                        {
                            char prev_char = content[start_index - 1];
                            if (prev_char != ' ' &&
                                prev_char != '\n' &&
                                prev_char != '\t')
                            {
                                --start_index;
                            }else
                            {
                                break;
                            }
                        }
                      
                    }

                    int find_end_index = start_index;
                    int end_index = find_end_index;
                    while (true)
                    {
                        end_index = content.IndexOf(e_str, find_end_index);
                        if (end_index > cur_index)
                        {
                            string checkStr = content.Substring(cur_index, end_index - cur_index);
                            if (not_pair_bracket(checkStr) || (is_odd(checkStr, "'") || is_odd(checkStr, "\"")))
                            {
                                find_end_index = end_index + 1;
                                continue;
                            }
                        }
                        break;
                    }

                    if (end_index < 0)
                    {
                        //content = content.Substring(0, start_index - 1);
                        break;
                    }
                    else
                    {
                        string head = content.Substring(0, start_index);
                        string rear = content.Substring(end_index + e_str.Length);

                        string logContent = content.Substring(start_index, (end_index + e_str.Length) - start_index);
                        Debug.Log(logContent);

                        string newLogContent = $"\nif gEnableDebug then\n  {logContent} \nend\n";

                        content = head + newLogContent + rear;
                        cur_index = start_index + newLogContent.Length;
                    }
                }
            }

            ////查找content当中的log.Log()注释
            cut_str(".Log(", ")");
            cut_str("self:Log(", ")");

            ////查找content当中的print()注释
            cut_str("print(", ")");

            //replace
            /**这个已经受Ini中的EnableDebug配置项控制，不需要处理**/
            //content = content.Replace(".isDebug = true", ".isDebug = false");  

            //行注释
            string[] text = content.Split('\r', '\n');
            List<string> str_list = new List<string>();
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(text[i]))
                {
                    string str = text[i];
                    int start_index = 0;
                    while (true)
                    {
                        if (start_index > str.Length)
                            break;
                        int index = str.IndexOf("--", start_index);
                        if (index > 0 && (is_odd(str.Substring(0, index), "'") || is_odd(str.Substring(0, index), "\"")))
                        {
                            start_index = index + 2;
                            continue;
                        }
                        if (index < 0)
                        {
                            break;
                        }
                        else
                        {
                            str = str.Substring(0, index).TrimEnd();
                        }
                    }
                    if (str != "")
                    {
                        str_list.Add(str);
                    }
                }
            }
            for (int i = 0; i < str_list.Count; i++)
            {
                output.Append(str_list[i]);
                if (i != str_list.Count - 1)
                    output.Append("\n");
            }
            return output.ToString();
        }
#endregion

        /// <summary>
        /// 加密Lua，从AssetBundleBuilder.RunXXTea 抄过来的
        /// </summary>
        static private bool EncryptLua()
        {
            bool isSuccess = true;
            DirectoryInfo diInfo = new DirectoryInfo(LuaFolderPath);
            if (!diInfo.Exists)
            {
                DebugLog($"不存在Lua文件夹：{LuaFolderPath}", 3);
                return false;
            }
            try
            {

                FileInfo[] fileInfos = diInfo.GetFiles("*.lua", SearchOption.AllDirectories);
                for (int i = 0; i < fileInfos.Length; i++)
                {
                    var file = fileInfos[i];
                    string szBuff = string.Empty;
                    byte[] byteBuff = null;

                    //byteBuff = File.ReadAllBytes(file.FullName);

                    using (StreamReader read = file.OpenText())
                    {
                        szBuff = read.ReadToEnd();

                       // if (file.Name.IndexOf("log.lua") < 0)
                        {
                            if (_isDiscardLuaLog)
                            {
                                szBuff = CutDownCode(szBuff);
                            }
                        }

                        //szBuff = XGame.Utils.XXTea.Encrypt(szBuff, XGame.Utils.GameConfig.PW);
                        byteBuff = System.Text.Encoding.UTF8.GetBytes(szBuff);
                    }


                    string filePath = file.FullName.Replace(".lua", ".bytes").Replace(LuaSourceName, LuaGenerateName);
                    var dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    FileStream write = null;
                    if (!File.Exists(filePath))
                        write = File.Create(filePath);

                    if (write == null)
                        write = File.OpenWrite(filePath);

                    write.Seek(0, SeekOrigin.Begin);
                    write.SetLength(0);


                    //DebugLog("" + fi.FullName + "    " + XXTea.Decrypt(szBuff, GameConfig.PW),1);
                    write.Write(byteBuff, 0, byteBuff.Length);

                    write.Dispose();
                }
            }
            catch (Exception e)
            {
                DebugLog($"选资源打包异常，加密Lua失败： {e}", 3);
                isSuccess = false;
            }
            return isSuccess;
        }

#endregion

#region 删除模型的默认材质
        private void DrawDeleteModelDefualtMatrial()
        {
            if (GUILayout.Button("删吧"))
            {
                try
                {
                    DeleleModelDefualtMatrial();
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// 删除模型默认材质
        /// </summary>
        /// <returns></returns>
        static private bool DeleleModelDefualtMatrial()
        {



            var materialPath = "Assets/G_Artist/Effect/Materials/Default_Material.mat";
            Material defaultMat = null;
            if (AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)))
            {
                defaultMat =  AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            }



            string[] allFiles = AssetDatabase.FindAssets("t:Model", new string[] { "Assets" });
            bool isCancel = false;
            int length = allFiles.Length;
            for (int i = 0; i < length; i++)
            {
                string name = AssetDatabase.GUIDToAssetPath(allFiles[i]);
                isCancel = EditorUtility.DisplayCancelableProgressBar($"处理模型{i}/{length}", $"{name}", (float)i / length);
                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }

                var model = AssetDatabase.LoadAssetAtPath<GameObject>(name);
                if (model != null)
                {
                    //DebugLog($"[[[[[{model}",1);
                    Renderer[] renderComs = model.GetComponentsInChildren<Renderer>();
                    for (int j = 0; j < renderComs.Length; j++)
                    {
                        if(renderComs[j].sharedMaterial!= defaultMat)
                            renderComs[j].sharedMaterial = defaultMat;
                       // if (renderComs[j].sharedMaterials != null)
                       //     renderComs[j].sharedMaterials = defaultMat;

                      
                    }
                    EditorUtility.SetDirty(model);
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
#endregion


        
        static private void DelFiles(string outputPath,string pattern)
        {

            string[] paths = Directory.GetFiles(outputPath, pattern, SearchOption.AllDirectories);
            for(int i=0;i<paths.Length;++i)
            {
                if(File.Exists(paths[i]))
                {
                    File.Delete(paths[i]);
                }
            }
        }

        //删除Manifest文件
        static private void DeleteManifest(string outputPath)
        {
            DirectoryInfo diInfo = new DirectoryInfo(outputPath);
            if (!diInfo.Exists)
            {
                DebugLog($"不存在文件夹：{outputPath}", 3);
                return;
            }
            FileInfo[] fileInfos = diInfo.GetFiles("*.manifest", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
                var file = fileInfos[i];
                /*
                if (file.Name.Contains(UpdateConfig.MANIFEST_NAME))
                    continue;
                */

                if (fileInfos[i].Exists)
                {
                    File.Delete(fileInfos[i].FullName);
                }

            }

            //将全局依赖表重命名
            FileInfo info = new FileInfo(outputPath + "/" + UpdateConfig.DATA_DIR);
            info.MoveTo(outputPath + "/" + UpdateConfig.MANIFEST_NAME_DAT);

        }

        static void _CopyResToStreaming()
        {
            if (_isFullPack)
            {
                CopyABToStreaming(SelectABOutputABFolderPath);
            }
            else
            {
                CopyABToStreaming(InnerPath);


            }

            //同步一次版本号
            SetVersionStr();



        }

        // 拷贝AB资源到Streaming底下
        static private void CopyABToStreaming(string path, bool deleteOld = true)
        {
            TryCopyDirectory(path, StreamingABFolderPath, deleteOld);
        }



        // 拷贝AB资源到Streaming底下
        static private void TryCopyDirectory(string src, string des, bool deleteOld = true)
        {
            if (!Directory.Exists(src))
            {
                DebugLog($"拷贝失败！不存在：{src}", 3);
                return;
            }
            if (Directory.Exists(des))
            {
                if (deleteOld)//|| EditorUtility.DisplayDialog("文件已存在，是否删除后覆盖", $"{des}", "确定"))
                {
                    Directory.Delete(des, true);

                }

                FileUtil.ReplaceDirectory(src, des);
            }
            else
            {
                string parent = Path.GetDirectoryName(des);
                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }
                FileUtil.CopyFileOrDirectory(src, des);
            }
        }


        private void DrawHotUpdate()
        {
            if (string.IsNullOrEmpty(_hotUpdateABOldPath))
            {
                _hotUpdateABOldPath = GenABPath + "_old";
            }

            if (string.IsNullOrEmpty(_hotUpdateABNewPath))
            {
                _hotUpdateABNewPath = GenABPath;
                VersionInfoWindow.fileInfoPath = @"D:\HotUpdate\data\ver_" + _versionStr;
            }
            if (string.IsNullOrEmpty(_hotUpdateOutputPath))
            {
                _hotUpdateOutputPath = GenVersionPath + "/"+UpdateConfig.EDITOR_HOT_UPDATE_DIR;
                VersionInfoWindow.zipInfoPath = _hotUpdateOutputPath;
            }

            if (string.IsNullOrEmpty(_hotUpdateEndVersion))
            {
                _hotUpdateEndVersion =_versionStr;
            }


            DrawFolderEdit("旧AB路径：", ref _hotUpdateABOldPath);
            DrawFolderEdit("新AB路径：", ref _hotUpdateABNewPath);
            DrawFolderEdit("生成热更包路径：", ref _hotUpdateOutputPath, true, false);
            DrawFolderEdit("合并的起始版本：", ref _hotUpdateStartVersion);
            DrawFolderEdit("合并的结束版本：", ref _hotUpdateEndVersion);



            if (GUILayout.Button($"生成热更文件"))
            {
                if (Directory.Exists(_hotUpdateOutputPath))
                {
                    if (EditorUtility.DisplayDialog("生成热文件目录已存在，是否删除后覆盖", $"{_hotUpdateOutputPath}", "确定"))
                    {
                        Directory.Delete(_hotUpdateOutputPath, true);
                        BuildHotUpdatePackage();
                    }
                }
                else
                {
                    BuildHotUpdatePackage();
                }
            }
            if (GUILayout.Button($"生成热更新Web文件"))
            {
                BuildPublishPackage();
            }

            if (GUILayout.Button($"合并热更包资源"))
            {
                MergeHotPackage();
            }

            DrawBox("热更版本工具", VersionInfoWindow.Draw, true);
        }

        static private void BuildHotUpdatePackage()
        {
            try
            {

                _hotUpdateOutputPath = _hotUpdateOutputPath.Replace("\\", "/");
                _hotUpdateABNewPath = _hotUpdateABNewPath.Replace("\\", "/");
                _hotUpdateABOldPath = _hotUpdateABOldPath.Replace("\\", "/");


                if (!Directory.Exists(_hotUpdateOutputPath))
                {
                    Directory.CreateDirectory(_hotUpdateOutputPath);
                }

             
                DirectoryInfo diInfo = new DirectoryInfo(_hotUpdateABNewPath);
                FileInfo[] fileInfos = diInfo.GetFiles("*", SearchOption.AllDirectories);
                bool isCancel = false;
                int length = fileInfos.Length;
                for (int i = 0; i < fileInfos.Length; i++)
                {
                    isCancel = EditorUtility.DisplayCancelableProgressBar("查找热更资源", $"{fileInfos[i]}", (float)i / length);
                    if (isCancel)
                        break;

                    HotUpdateGetDiff(fileInfos[i].FullName.Replace("\\", "/"));

                   
                }

                string prject_Res_dir = _hotUpdateOutputPath + "/" + UpdateConfig.HOT_CODE_GLOBAL_RES_DIR;
                if(Directory.Exists(prject_Res_dir))
                {
                    // check global res 
                    string[] filePaths = Directory.GetFiles(prject_Res_dir, "*.*", SearchOption.TopDirectoryOnly);
                    //工程的资源不热更新
                    if (filePaths.Length > 0)
                    {
                        for (int i = 0; i < filePaths.Length; ++i)
                        {
                            File.Delete(filePaths[i]);
                        }
                    }
                }

                //web的配置资源，不做热更新，过滤掉
                string webCfgPath = _hotUpdateOutputPath + "/" + UpdateConfig.HOTUPDATE_REAL_DIR+"/"+ UpdateConfig.WEB_INFO_CFG;
                if(File.Exists(webCfgPath))
                {
                    File.Delete(webCfgPath);
                }

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static private void HotUpdateGetDiff(string filePath)
        {
            string lastPath = filePath.Replace(_hotUpdateABNewPath, _hotUpdateABOldPath);
            string newMD5 = MD5.GetMD5(filePath);
            if (newMD5 == null) return;

            string oldMD5 = null;

            if (File.Exists(lastPath))
            {
                oldMD5 = MD5.GetMD5(lastPath);
            }


            //文件不一致，拷贝文件
            if (newMD5 != oldMD5)
            {
                string updatePath = filePath.Replace(_hotUpdateABNewPath, _hotUpdateOutputPath);

                DebugLog($"更新  {updatePath}", 1);

                //删除老的文件
                if (File.Exists(updatePath))
                {
                    File.Delete(updatePath);
                }

                //创建文件夹
                Directory.CreateDirectory(updatePath);
                Directory.Delete(updatePath);



                //拷贝文件
                FileUtil.CopyFileOrDirectory(filePath, updatePath);

            }
        }


        static void MergeHotPackage()
        {
            VersionInfoWindow.MergeHotPackage(GenABBaseFolder, _hotUpdateStartVersion, _hotUpdateEndVersion, _isAutoCopyToPulishFolder, _publishTargetDir);
        }

       static void BuildPublishPackage()
        {

            //代码是否有变更
            bool isCodeModified = true;
            string lastVerDir = "";

            //版本文件
            string vesionInfoPath = BuildConfigPath + "/" + UpdateConfig.UPDATE_VERSION;

            if (File.Exists(vesionInfoPath))
            {

                //输出目录
                _hotUpdateOutputPath = GenVersionPath + "/" + UpdateConfig.EDITOR_HOT_UPDATE_DIR;
                VersionInfoWindow.zipInfoPath = _hotUpdateOutputPath;

                //当前热更新资源
                _hotUpdateABNewPath = GenABPath;

                //上一个版本的热更新资源
                ulong NO = 0;
                if (GetLastVersion(out NO, ulong.Parse(_versionStr)))
                {
                    lastVerDir = BuildConfigPath + "/pkg_" + NO.ToString();
                    _hotUpdateABOldPath = BuildConfigPath + "/pkg_" + NO.ToString() + "/" + CurPlatformName;

                    //消除../路径
                    _hotUpdateABNewPath = Directory.GetParent(_hotUpdateABNewPath).FullName + "/" + CurPlatformName;
                    _hotUpdateABOldPath = Directory.GetParent(_hotUpdateABOldPath).FullName + "/" + CurPlatformName;
                    _hotUpdateOutputPath = Directory.GetParent(_hotUpdateOutputPath).FullName + "/" + UpdateConfig.EDITOR_HOT_UPDATE_DIR;

                    //查找默认上一版本资源
                    BuildHotUpdatePackage();
                }

            }


            bool isNeedRestart = false;
            bool isNeedReinstall = false;
            if (_isCheckCodeModified)
            {
                List<string> listModifiedCodes;
                ModifyCodeCheck.GenAndCheckCode(lastVerDir + "/" + "/codemd5.csv", GenVersionPath + "/codemd5.csv", out isCodeModified,out listModifiedCodes);
                isNeedRestart = IsNeedRestart(listModifiedCodes);
                isNeedReinstall = IsNeedReinstall(listModifiedCodes);
            }


            //发布资源包
            VersionInfoWindow.BuildPublishPackage(GenVersionPath, _versionStr, _isAutoCopyToPulishFolder, _publishTargetDir, isCodeModified, isNeedReinstall == false,_isFullPack==false&&_isSplitFix, isNeedRestart);

        }

        //查询一下热更的资源是否需要重启
        static bool IsNeedRestart(List<string> listModifiedCodes)
        {

            List<string> StartupAssemblyFolders = s_StartupAssemblyFolders;
            int nCount = StartupAssemblyFolders.Count;
            int nLen = listModifiedCodes.Count;
            for (int i = 0; i < nCount; ++i)
            {
                for (int j = 0; j < nLen; ++j)
                {
                    if (listModifiedCodes[j].LastIndexOf(StartupAssemblyFolders[i]) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //查询一下热更的资源是否需要重装
        static bool IsNeedReinstall(List<string> listModifiedCodes)
        {

            List<string> ReinstallAssemblyFoldres = s_ReinstallAssemblyFoldres;
            int nCount = ReinstallAssemblyFoldres.Count;
            int nLen = listModifiedCodes.Count;
            for (int i = 0; i < nCount; ++i)
            {
                for (int j = 0; j < nLen; ++j)
                {
                    if (listModifiedCodes[j].LastIndexOf(ReinstallAssemblyFoldres[i]) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //获取上一次最新的版本号
        private static bool GetLastVersion(out ulong lastVersion, ulong curVersion)
        {
            string vesionInfoPath = BuildConfigPath + "/" + UpdateConfig.UPDATE_VERSION;

            if (File.Exists(vesionInfoPath) == false)
            {
                lastVersion = 0;
                return false;
            }

            //上一个版本的热更新资源
            CSVVersionInfoHelper versoninfoHelper = new CSVVersionInfoHelper();

            List<CSVVersionInfo> listVersionInfo = versoninfoHelper.LoadFile(File.ReadAllBytes(vesionInfoPath));

            lastVersion = curVersion;//
            int nCount = listVersionInfo.Count;
            bool bFindLastVersion = false;
            for (int i = nCount - 1; i >= 0; --i)
            {
                if (lastVersion > listVersionInfo[i].m_NO)
                {
                    lastVersion = listVersionInfo[i].m_NO;
                    bFindLastVersion = true;
                    break;
                }
            }

            return bFindLastVersion;
        }

        private static void InitConfigs()
        {
            //生成版本号
            if (string.IsNullOrEmpty(_versionStr))
                _versionStr = System.DateTime.Now.ToString("yyMMddHHmm");

            if (string.IsNullOrEmpty(GenABBaseFolder))
            {
                GenABBaseFolder = $"{Application.dataPath}/../BuildAPP";
            }

            if (string.IsNullOrEmpty(_hotUpdateABOldPath))
            {
                _hotUpdateABOldPath = GenABPath + "_old";
            }

            if (string.IsNullOrEmpty(_hotUpdateABNewPath))
            {
                _hotUpdateABNewPath = GenABPath;
                VersionInfoWindow.fileInfoPath = @"D:\HotUpdate\data\ver_" + _versionStr;
            }
            if (string.IsNullOrEmpty(_hotUpdateOutputPath))
            {
                _hotUpdateOutputPath = GenVersionPath + UpdateConfig.EDITOR_HOT_UPDATE_DIR;
                VersionInfoWindow.zipInfoPath = _hotUpdateOutputPath;
            }

            if (Directory.Exists(_publishTargetDir) == false)
            {
                _publishTargetDir = _publishTargetDir.Replace("D:", "C:");
            }
        }


        static private void BuildZip(string srcDir, string zipPath)
        {

            //压缩更新包
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

#if UNITY_IPHONE
     ZipHelper.ZipFileDirectory(srcDir, zipPath);
#else
    ZipHelper.ZipDirectory(srcDir, zipPath);
#endif
        }
        
        //拷贝热更DLL到AB目录
        static void CopyHotUpdateAssembliesToABPath()
        {
            /*
            if (!_isEnableHybridCLR)
            {
                return;
            }
            var    target                 = EditorUserBuildSettings.activeBuildTarget;
            string hotfixDllSrcDir        = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string externalPath = HybridCLRSettings.Instance.externalHotUpdateAssembliyDirs[0];
            string hotfixAssembliesDstDir = $"{Application.dataPath}/G_Resources/CodeUpdate";
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath      = $"{hotfixDllSrcDir}/{dll}";
                //外部热更新dll不会拷贝到hotfixDllSrcDir 需要从外部热更dll的路径拷贝
                if (!File.Exists(dllPath))
                {
                    string parentPath = Path.GetDirectoryName(Application.dataPath);
                    // 将反斜杠替换为正斜杠
                    string normalizedPath = parentPath.Replace("\\", "/");

                    dllPath = $"{normalizedPath}/{externalPath}/{dll}";
                }
                string name         = Path.GetFileNameWithoutExtension(dll);
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{name}.bytes";
            
                string targetDirectory = Path.GetDirectoryName(dllBytesPath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
            
                File.Copy(dllPath, dllBytesPath, true);
            
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
            
            CopyAOTAssembliesToABPath();
            */
        }
        
        //拷贝AOT DLL到AB目录
        static void CopyAOTAssembliesToABPath()
        {
            /*
            var    target              = EditorUserBuildSettings.activeBuildTarget;
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string aotAssembliesDstDir = $"{Application.dataPath}/G_Resources/CodeUpdate";
            
            foreach (var dll in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                string dllPath      = $"{aotAssembliesSrcDir}/{dll}";
                string name = dll.Replace(".dll", "");  // 替换掉 .dll 扩展名
                string dllBytesPath = $"{aotAssembliesDstDir}/{name}.bytes";
            
                string targetDirectory = Path.GetDirectoryName(dllBytesPath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
            
                File.Copy(dllPath, dllBytesPath, true);
            
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
            */
        }

        static void HybridCLRGenerateAll()
        {
            /*
            if (_isEnableHybridCLR)
            {
                PrebuildCommand.GenerateAll();
            }
            */
        }
        
        //进行分包
        static void SplitInnerPackage()
        {
            if (_isSplitInner)
            {
                BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
                string apkName = PlayerSettings.GetApplicationIdentifier(group);
                //拷贝apk
                string path_apk = $"{BuildConfigPath}/pkg_{_versionStr}/publish/{apkName}";
                if (!File.Exists(path_apk))
                {
                    Debug.LogError("apk文件不存在：" + path_apk);
                    return;
                }
                string targetDir = $"{Application.dataPath}/../BuildAPP/APKTools";
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                string targetPath = $"{targetDir}/{apkName}";
                File.Copy(path_apk, targetPath,true);
                Debug.Log("拷贝apk成功：" + targetPath);
                
                //执行批处理
                string batPath  = Application.dataPath + "/../BuildApp/APKTools/ConvertAPK.bat";
                if (!File.Exists(batPath))
                {
                    Debug.LogError("批处理文件不存在：" + batPath);
                    return;
                }
                ProcessStartInfo psi = new ProcessStartInfo
                {
                        FileName = "cmd.exe",
                        Arguments = "/c \"" + batPath + "\"", // /c 执行命令后关闭窗口
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(batPath) // 设置当前目录，否则可能找不到相对路径
                };
                Process process = new Process
                {
                        StartInfo = psi
                };
                process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
                process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                
            }
        }


        /// <summary>
        /// 获取所有依赖
        /// </summary>
        /// <param name="allDepends"></param>
        /// <param name="assetName"></param>
        static private void GetAllDepencies(AssetBundleManifest abManifest, ref List<string> allDepends, string assetName)
        {
            if (abManifest)
            {
                string[] dependencies = abManifest.GetAllDependencies(assetName);
                allDepends.AddRange(dependencies);
                int length = dependencies.Length;
                for (int i = 0; i < length; i++)
                {
                    GetAllDepencies(abManifest, ref allDepends, dependencies[i]);
                }
            }
        }


        /// <summary>
        /// //获取相对路径
        /// </summary>
        static private string GetRelativePath(string originalPath, string basePath = null)
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



        /// <summary>
        /// 获取所有录制资源以及依赖资源
        /// </summary>
        static private List<string> GetSelectResList(AssetBundleManifest abManifest, string parentFolder, string[] recordList, out string errMsg)
        {
            errMsg = string.Empty;
            List<string> result = new List<string>();
            int count = recordList.Length;
            for (int i = 0; i < count; i++)
            {
                string curLine = recordList[i];
                string relativePath = curLine.Replace(@"\", "/");
                string fullPath = $"{parentFolder}/{relativePath}";
                if (File.Exists(fullPath))
                {
                    if (!result.Contains(relativePath))
                        result.Add(relativePath);

                    //如果有依赖的，把依赖文件也加入
                    List<string> allDepends = new List<string>();
                    GetAllDepencies(abManifest, ref allDepends, relativePath);
                    if (allDepends.Count > 0)
                    {
                        result.AddRange(allDepends);
                    }
                }
                else if (Directory.Exists(fullPath))
                {
                    List<string> Files = new List<string>();
                    GetFiles(fullPath, out Files, parentFolder);
                    result.AddRange(Files);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有文件
        /// </summary>
        static private void GetFiles(string path, out List<string> pathArr, string relativeBasePath, bool recursive = true)
        {
            pathArr = Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Select(p => p = GetRelativePath(p, relativeBasePath)).ToList<string>();
        }


       

        static private Dictionary<string, string> _buildApkConfig;

        [Flags]
        public enum EnBuildApkCmdArgs
        {
            None = 0,
            //IsBuildDll = 0x0001,
            //IsGenXLua = 0x0002,
            //IsEncryptLua = 0x0004,
            //IsDeleteModelMaterial = 0x0008,
            //IsBuildAB = 0x0010,
            //IsAutoCopyABToProject = 0x020,
            //IsBuildApk = 0x0040,
            IsUpdateSvn = 0x0001,
            IsUpload = 0x0002,
            IsHotUpdate = 0x0004,
            IsMicroClient = 0x0008,

            IsBuildAB = 0x0010,  //是否生成AB包
            IsBuildApk = 0x0020, //是否生成APK
            IsCompatibleType = 0x0040,  //AB资源是否兼容模式
            IsIncrementABPack = 0x0080,  //是否增量打AB资源
            IsDiscardLuaLog = 0x0100, //是否打印log
            IsSplitFix = 0x0200, //是否拆分固件资源
            IsSplitWeb = 0x0400, //是否拆分We资源
            IsFullPack = 0x0800, //是否完整包
            IsSurportObb = 0x1000, //是否打obb包(google)
            IsNoCSCodeUpdate = 0x2000, //是否打非CS代码热更包
            IsDevelopment = 0x4000, //是否开发版本，非代码热更有效
            IsUpLoad = 0x8000, //是否上传发布资源
            IsBuglySymbol = 0x10000, //是否上传bugly符号表
            IsXlua = 0x20000, //是否生成xlua
            IsResHotupdateVer = 0x40000, //是否纯热更资源版本_isResHotupdateVer
            IsCheckCodeModified = 0x80000, //是否检查代码
            IsAutoCopyToPulishFolder = 0x100000, //是否自动拷贝到发布资源路径
            IsSkipI18N = 0x200000, //是否跳过国际化
     

        }
        static readonly public EnBuildApkCmdArgs BuildApkCmdArgsAllTrue = (EnBuildApkCmdArgs)(-1);
        static readonly public EnBuildApkCmdArgs DefualtBuildApkCmdArg = BuildApkCmdArgsAllTrue
            & (~EnBuildApkCmdArgs.IsUpdateSvn) & (~EnBuildApkCmdArgs.IsHotUpdate) & (~EnBuildApkCmdArgs.IsMicroClient);
        static private Dictionary<string, EnBuildApkCmdArgs> _argsNameDic = new Dictionary<string, EnBuildApkCmdArgs>()
        {
            //{ "-buildDll", EnBuildApkCmdArgs.IsBuildDll},
            //{ "-genXLua", EnBuildApkCmdArgs.IsGenXLua},
            //{ "-encryptLua", EnBuildApkCmdArgs.IsEncryptLua},
            //{ "-deleteMat", EnBuildApkCmdArgs.IsDeleteModelMaterial},
            //{ "-buildAB", EnBuildApkCmdArgs.IsBuildAB},
            //{ "-autoCopyAB", EnBuildApkCmdArgs.IsAutoCopyABToProject},
            //{ "-buildApk", EnBuildApkCmdArgs.IsBuildApk},
            //{ "-updateSvn", EnBuildApkCmdArgs.IsUpdateSvn},
            //{ "-upload", EnBuildApkCmdArgs.IsUpload},
            //{ "-hotUpdate", EnBuildApkCmdArgs.IsHotUpdate},
            //{ "-microClient", EnBuildApkCmdArgs.IsMicroClient},
        };

        /// <summary>
        /// 检查参数
        /// </summary>
        /// <param name="errInfo"></param>
        /// <param name="argsFlags"></param>
        /// <returns></returns>
        /// 
        /*
        static public bool CheckArgsFromCmd(out string errInfo, out EnBuildApkCmdArgs argsFlags)
        {
            errInfo = string.Empty;
            var args = System.Environment.GetCommandLineArgs();
            argsFlags = EnBuildApkCmdArgs.None;
            _versionStr = string.Empty;
            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                DebugLog($"打包参数：{i}:{arg}");
                if (arg == "-ver")
                {
                    //DebugLog($"设置版本号111：{args[i + 1]}");
                    //int temInt;
                    if (i == arg.Length - 1)
                    {
                        errInfo = "无效版本号";
                        return false;
                    }
                    _versionStr = args[i + 1];
                    DebugLog($"设置版本号：{_versionStr}");
                    //++i;
                    continue;
                }

                if (_argsNameDic.ContainsKey(arg))
                {
                    argsFlags |= _argsNameDic[arg];
                }
            }
            if (argsFlags == EnBuildApkCmdArgs.None)
            {
                argsFlags = DefualtBuildApkCmdArg;
            }
            if (string.IsNullOrEmpty(_versionStr))
            {
                _versionStr = System.DateTime.Now.ToString("yyyyMMddHHmm");
                DebugLog($"设置版本号：{_versionStr}");
            }
            DebugLog($"参数配置：{argsFlags.ToString()}", 2);
            return true;
        }
        */
        /// <summary>
        /// 是否包含参数
        /// </summary>
        /// <param name="argsFlags"></param>
        /// <param name="targetArg"></param>
        /// <returns></returns>
        /// 
        /*
        static public bool IsContainsArg(EnBuildApkCmdArgs argsFlags, EnBuildApkCmdArgs targetArg)
        {
            return (argsFlags & targetArg) != 0;
        }
        */
        /// <summary>
        /// 打apk，失败返回错误信息
        /// </summary>
        /// <returns></returns>
        /// 
        /*
        static public void BuildApkFromCmd()
        {
            string errInfo = string.Empty;
            try
            {
                StartReceivedErrorLog();
                File.WriteAllText(BuildStateTextPath, "0\n打包中");
                DoSendDingDing();
                EnBuildApkCmdArgs argsFlags;
                if (CheckArgsFromCmd(out errInfo, out argsFlags)) // 检查参数合法性
                    errInfo = BuildApkFromCmd(argsFlags);
                else
                    errInfo = DebugLog(errInfo, 3);

            }
            catch (Exception e)
            {
                errInfo = DebugLog($"打包异常：{e.ToString()}");
            }
            finally
            {
                var isSuccess = string.IsNullOrEmpty(errInfo);
                var logErrorInfo = EndReceivedErrorLog();
                errInfo += $"\n\n\n{logErrorInfo}";
                File.WriteAllText(BuildStateTextPath, isSuccess ? $"1\n执行打包批处理成功，目录{GenABBaseFolder}\n{errInfo}" : $"-1\n{errInfo}");
                DoSendDingDing();
            }
        }


        /// <summary>
        /// 根据参数打包
        /// </summary>
        /// <param name="argsFlags"></param>
        /// <returns></returns>
        static public string BuildApkFromCmd(EnBuildApkCmdArgs argsFlags)
        {
            string errInfo;

            // 1.加载配置
            if (!LoadBuildApkConfig(out errInfo))
                return DebugLog(errInfo, 3);

            // 2.编DLL
            bool isBuildDLL = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsBuildDll);
            DebugLog($"【是否编DLL: {isBuildDLL}】");
            if (isBuildDLL)
            {
                if (!BuildDll(out errInfo))
                    return DebugLog(errInfo, 3);
                if (!GenXLua(out errInfo))          // 3.生成Xlua 编完DLL自动生成Xlua
                    return DebugLog(errInfo, 3);
            }


            // 拷贝引擎DLL
            DebugLog($"【拷贝引擎DLL [{EngineCopySrcPath}] -> [{EngineCopyDesPath}]");
            CopyFolder(EngineCopySrcPath, EngineCopyDesPath);

            //// 3.生成Xlua
            //bool isGenXLua = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsGenXLua);
            //DebugLog($"【生成XLua：{isGenXLua}】");
            //if (isGenXLua && !GenXLua(out errInfo))
            //    return DebugLog(errInfo, 3);

            //// 4.Lua代码重新生成
            //bool isEncrpytLua = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsEncryptLua);
            //DebugLog($"【加密Lua：{isEncrpytLua}】");
            //if (isEncrpytLua && !EncryptLua())
            //    return DebugLog("加密Lua失败", 3);

            //// 5.删除默认材质
            //bool isDeleteModelMaterial = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsDeleteModelMaterial);
            //DebugLog($"【删除模型默认材质：{isDeleteModelMaterial}】");
            //if (isDeleteModelMaterial && !DeleleModelDefualtMatrial())
            //    return DebugLog("删除模型默认材质失败", 3);

            // 6.打AB
            bool isBuildAB = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsBuildAB);
            DebugLog($"【是否打AB：{isBuildAB}】");
            if (isBuildAB)
            {
                if (!EncryptLua())
                    return DebugLog("加密Lua失败", 3);           // 4.Lua代码重新生成
                if (!DeleleModelDefualtMatrial())
                    return DebugLog("删除模型默认材质失败", 3);   // 5.删除默认材质

                RefreshCacheRecordAndExport();  // 刷新映射文件

                if (!BuildNewABRes())
                    return DebugLog("打AB失败", 3);            // 6.打AB
            }

            // 微端分包
            /*
            bool isMicroClient = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsMicroClient);
            DebugLog($"【是不是微端：{isMicroClient}】");
            if (isMicroClient && !BuildMicroAB())
                return DebugLog("打微端AB失败", 3);
                */

        // 7.打完AB是否要拷到StreamingAssets
        /*
        bool isCopyToStreaming = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsAutoCopyABToProject);
        DebugLog($"【是否拷贝AB：{isCopyToStreaming}】");
        if (isCopyToStreaming)
        {
            if (isMicroClient)
            {
                CopyABToStreaming(MicroGenInnerOutputABFolderPath, true);
                if (!ForceCopyFile(MicroBackDownloadBinPath, MicroStreamingAndroidDataPath))
                {
                    return DebugLog("微端拷贝资源失败", 3);
                }
            }
            else
                CopyABToStreaming(SelectABOutputABFolderPath, true);
            if (!UpdateCheckMD5())
            {
                DebugLog("刷新检查MD5文件失败！！", 3);
            }
        }
        */
        // 8.打APK

        /*
        bool isBuildAPK = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsBuildApk);
        DebugLog($"【是否打APK：{isBuildAPK}】");
        string finalApkPath = string.Empty;
        if (isBuildAPK && !BuildApk(out errInfo, out finalApkPath, IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsHotUpdate), isMicroClient))
            return DebugLog($"打APK失败：{errInfo}", 3);

        // 9.上传备份
        bool isUploadBackup = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsUpload);
        DebugLog($"【是否上传备份：{isUploadBackup}】");
        if (isUploadBackup && !UploadFile($"{_buildApkConfig[BuildUploadURLKey]}", _buildApkConfig[BuildUploadUserNameKey], _buildApkConfig[BuildUploadPasswordKey], finalApkPath))
            return DebugLog($"上传备份失败：{finalApkPath}", 3);

        DebugLog($"打包完成：{finalApkPath}");

        return string.Empty;
    }

    /// <summary>
    /// 发消息给钉钉
    /// </summary>
    static private void DoSendDingDing()
    {
        if (File.Exists(BuildSendDingDingBatPath))
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = BuildSendDingDingBatPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            DebugLog($"发消息给钉钉：【{BuildSendDingDingBatPath}】");
            p.Close();

        }
    }

    /// <summary>
    /// 加载APK配置
    /// </summary>
    /// <param name="errInfo"></param>
    /// <returns></returns>
    static private bool LoadBuildApkConfig(out string errInfo)
    {
        errInfo = string.Empty;
        if (!File.Exists(BuildApkConfigPath))
        {
            errInfo = $"找不到构建APK配置文件：{BuildApkConfigPath}";
            return false;
        }

        string textAsset = File.ReadAllText(BuildApkConfigPath);
        _buildApkConfig = JsonMapper.ToObject<Dictionary<string, string>>(textAsset);
        if (_buildApkConfig == null)
        {
            errInfo = $"读取APK配置文件：{BuildApkConfigPath}";
            return false;
        }

        if (!CheckBuildConfig(out errInfo, BuildDllDevEnvKey, true))
            return false;
        else if (!CheckBuildConfig(out errInfo, BuildDllConfigurationKey, false))
            return false;
        else if (!CheckBuildConfig(out errInfo, BuildDllSlnPahtKey, true))
            return false;
        else if (!CheckBuildConfig(out errInfo, BuildApkNameKey, false))
            return false;

        DebugLog("读取打包配置成功");
        return true;
    }

    /// <summary>
    /// 检查配置
    /// </summary>
    /// <param name="errInfo"></param>
    /// <param name="key"></param>
    /// <param name="isFile"></param>
    /// <returns></returns>
    static private bool CheckBuildConfig(out string errInfo, string key, bool isFile = false, bool isFolder = false)
    {
        errInfo = string.Empty;
        if (!_buildApkConfig.ContainsKey(key))
        {
            errInfo = $"APK配置文件中编译DLL Key读取失败：isFile:{isFile}, {BuildApkConfigPath} -> {key}";
            return false;
        }
        var path = _buildApkConfig[key];
        if (isFile && !File.Exists(path))
        {
            errInfo = $"APK配置文件中路径无效：{BuildApkConfigPath} -> {key} : {path}";
            return false;
        }
        if (isFolder && !Directory.Exists(path))
        {
            errInfo = $"APK配置文件夹中路径无效：{BuildApkConfigPath} -> {key} : {path}";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 编译DLL
    /// </summary>
    /// <param name="errInfo"></param>
    /// <returns></returns>
    static private bool BuildDll(out string errInfo)
    {
        errInfo = string.Empty;
        string devenvPath = _buildApkConfig[BuildDllDevEnvKey];
        string slnPath = _buildApkConfig[BuildDllSlnPahtKey];
        string configuration = _buildApkConfig[BuildDllConfigurationKey];

        string slnConfiguration = _buildApkConfig[BuildDllConfigurationKey];
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = devenvPath;
        p.StartInfo.Arguments = $"{slnPath} /Rebuild {configuration}";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        DebugLog($"开始编译DLL：【{p.StartInfo.Arguments}】");
        p.Start();

        string outlog = p.StandardOutput.ReadToEnd();
        bool isSuccess = false;
        int startIndex = outlog.LastIndexOf("成功") - 1;
        if (startIndex > 0)
        {
            ////编译成功
            //int count = outlog.LastIndexOf(" ") - startIndex;
            //outlog = outlog.Substring(startIndex, count);
            //isSuccess = true;
        }
        else
        {
            //errInfo = "编译失败！";
        }

        DebugLog($"Dll编译输出：{outlog}");
        p.Close();

        DebugLog($"Dll编译完成");
        return true;
    }

    /// <summary>
    /// 生成Xlua
    /// </summary>
    static private bool GenXLua(out string errInfo)
    {




        bool isSuccess = true;
        errInfo = string.Empty;
        try
        {
           // CSObjectWrapEditor.Generator.ClearAll();
          //  CSObjectWrapEditor.Generator.GenAll();
        }
        catch (Exception e)
        {
            errInfo = $"生成Xlua抛异常：{e.ToString()}";
            isSuccess = false;
        }
        return isSuccess;
    }

    /// <summary>
    /// 打APK
    /// </summary>
    /// <param name="errInfo"></param>
    /// <returns></returns>
    static private bool BuildApk(out string errInfo, out string finalApkPath, bool isInclueUpdate, bool isMicroClient)
    {
        errInfo = string.Empty;
        var levels = GetLevelsFromBuildSettings(isInclueUpdate);
        BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
        string apkName = _buildApkConfig[BuildApkNameKey];

        finalApkPath = $"{GenVersionPath}/{apkName}_{_versionStr}{(isMicroClient ? "_micro" : string.Empty)}.apk";
        DebugLog($"开始打APK：{finalApkPath}");
        var result = BuildPipeline.BuildPlayer(levels, finalApkPath, EditorUserBuildSettings.activeBuildTarget, option);
        var isSuccess = result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        errInfo = $"打APK完成：{finalApkPath}  成功：{isSuccess}  错误数量：{result.summary.totalErrors}";
        for (int i = 0; i < result.steps.Length; i++)
        {
            var msg = result.steps[i].messages;
            for (int j = 0; j < msg.Length; j++)
            {
                var type = msg[j].type;
                if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
                {
                    errInfo = $"{errInfo}\n[{i} - {j}][{type}] {msg[j].content}";
                }
            }
        }
        DebugLog(errInfo);
        if (isSuccess)
        {
            EditorUtility.RevealInFinder(finalApkPath);
        }
        return isSuccess;
    }

    /// <summary>
    /// 获取构建场景配置
    /// </summary>
    /// <returns></returns>
    static private string[] GetLevelsFromBuildSettings(bool isInclueUpdate)
    {
        List<string> levels = new List<string>()
        {
            @"Assets\G_Resources\Scene\Update.unity",
            @"Assets\G_Resources\Scene\Login.unity",
        };

        //for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
        //{
        //    if (EditorBuildSettings.scenes[i] != null && EditorBuildSettings.scenes[i].enabled)
        //    {
        //        var path = EditorBuildSettings.scenes[i].path;
        //        if (!isInclueUpdate && path == @"Assets\G_Resources\Scene\Update.unity")
        //        {
        //            continue;
        //        }
        //        levels.Add(path);
        //        DebugLog($"添加场景：{path}");
        //    }
        //}

        return levels.ToArray();
    }

#endregion

#region 上传文件相关
    static private string _waitForDeleteFile;

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// 
    /*
    static private bool UploadFile(string url, string userName, string password, string filePath)
    {
        DebugLog($"上传文件：url:{url}, userName:{userName}, password:{password}, filePath:{filePath}");
        bool isSuccess = true;

        if (!File.Exists(filePath))
        {
            DebugLog($"不存在文件：{filePath}");
            return false;
        }
        try
        {
            _waitForDeleteFile = filePath;
            WebClient client = new System.Net.WebClient();
            var finalURL = $"{url}/{Path.GetFileName(filePath)}";
            DebugLog($"上传文件：{finalURL}, file:{filePath}");
            Uri uri = new Uri(finalURL);
            client.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
            client.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
            client.Credentials = new System.Net.NetworkCredential(userName, password);
            client.UploadFileAsync(uri, filePath);

        }
        catch (Exception e)
        {
            DebugLog(e.ToString(), 3);
            isSuccess = false;
            _waitForDeleteFile = string.Empty;
        }
        return isSuccess;
    }

    static private DateTime _startUploadTime;
    const float MaxUploadTime = 600f;
    static private WebClient _upLoadClient;
    /// <summary>
    /// 上传文件
    /// </summary>
    static private bool UploadFolder(string url, string userName, string password, string folderPath)
    {
        DebugLog($"上传文件目录：url:{url}, userName:{userName}, password:{password}, filePath:{folderPath}");
        bool isSuccess = true;
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        if (!dirInfo.Exists)
        {
            DebugLog($"不存在目录：{folderPath}");
            return false;
        }
        try
        {
            var targetZIP = $"{dirInfo.Parent}/{dirInfo.Name}.zip";
            DebugLog($"压缩目录：filePath:{folderPath}, zip:{targetZIP}");
            if (File.Exists(targetZIP))
            {
                File.Delete(targetZIP);
            }

            if (!ZipHelper.ZipDirectory($"{folderPath}", $"{targetZIP}"))
            {
                DebugLog($"压缩目录失败：filePath:{folderPath}, zip:{targetZIP}");
                return false;
            }
            _waitForDeleteFile = targetZIP;

            _upLoadClient = new System.Net.WebClient();
            var finalURL = $"{url}/{dirInfo.Name}.zip";
            DebugLog($"上传文件：{finalURL}, file:{targetZIP}");
            _startUploadTime = DateTime.Now;
            Uri uri = new Uri(finalURL);
            _upLoadClient.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
            _upLoadClient.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
            _upLoadClient.Credentials = new System.Net.NetworkCredential(userName, password);
            _upLoadClient.UploadFileAsync(uri, targetZIP);

        }
        catch (Exception e)
        {
            DebugLog(e.ToString(), 3);
            isSuccess = false;
            _waitForDeleteFile = string.Empty;
        }
        return isSuccess;
    }

    static private string _lastChangeString = string.Empty;
    static private void OnFileUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
    {
        var str = $"上传进度变更: {_waitForDeleteFile}, {e.ProgressPercentage}";
        if (str != _lastChangeString)
        {
            DebugLog(str);
            _lastChangeString = str;
        }
        var duration = DateTime.Now - _startUploadTime;
        if (duration.TotalSeconds > MaxUploadTime)
        {
            DebugLog($"上传超时: {_waitForDeleteFile}, {e.ProgressPercentage}  。开始时间：{_startUploadTime}", 3);
            _upLoadClient.CancelAsync();
        }
    }

    static private void OnFileUploadCompleted(object sender, UploadFileCompletedEventArgs e)
    {
        DebugLog($"上传【{_waitForDeleteFile}】完成，结果：{(e.Error == null ? "成功" : e.Error.ToString())}");
        if (!string.IsNullOrEmpty(_waitForDeleteFile) && File.Exists(_waitForDeleteFile))
        {
            File.Delete(_waitForDeleteFile);
        }
        _waitForDeleteFile = string.Empty;
    }

    // 接收错误log
    static private StringBuilder _receivedLogStr = new StringBuilder();
    static private void StartReceivedErrorLog()
    {
        _receivedLogStr.Clear();
        Application.logMessageReceived += ReceivedErrorLog;
    }

    static private void ReceivedErrorLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            _receivedLogStr.AppendLine($"【[{type}] {condition}\n{stackTrace}】");
        }
    }

    static private string EndReceivedErrorLog()
    {
        Application.logMessageReceived -= ReceivedErrorLog;
        var result = _receivedLogStr.ToString();
        _receivedLogStr.Clear();
        return result;
    }

#endregion
 */

#region 国际化
        private void DrawI18NConfig()
        {            
#if SUPPORT_I18N
            if (I18NEditorConfig.Instance.Config == null)
            {
                EditorGUILayout.HelpBox($"找不到国际化配置！", MessageType.Error);
                return;
            }

            DrawPingObj($"国际化配置：", I18NEditorConfig.Instance.Config);

            if (GUILayout.Button("国际化窗口"))
            {
                // I18NWindow.ShowWindow();
            }
#endif

        }
#endregion

        private void DrawCollectVariantApart()
        {
            if (GUILayout.Button("开始收集变体"))
            {
                string err;
                VariantCollectionsEditor.CollectVariantApart();
            }
        }


        //拆分资源
        static private void SpiltABRes(string SelectABOutputABFolderPath)
        {

            if (_isFullPack)
            {
                Debug.Log("当前打的是全量包，不需要拆分");
                return;
            }

            if (_dicBundlePos.Count == 0)
            {
                Debug.LogError("资源拆分列表为0，请执行缓存导出");
                return;
            }


            if (Directory.Exists(SelectABOutputABFolderPath) == false)
            {
                Debug.LogError("请先打AB，再拆分资源");
                return;
            }

            //删除三个目录
            if (Directory.Exists(InnerPath))
            {
                Directory.Delete(InnerPath, true);
            }

            if (Directory.Exists(FixPath))
            {
                Directory.Delete(FixPath, true);
            }

            if (Directory.Exists(WebPath))
            {
                Directory.Delete(WebPath, true);
            }

            string[] abPaths = Directory.GetFiles(SelectABOutputABFolderPath, "*.*", SearchOption.AllDirectories);
            int nCount = abPaths.Length;
            int nPose = 0;
            string dataDir = UpdateConfig.DATA_DIR + "\\";
            int nLen = dataDir.Length;
            AB_RES_POS posType;
            for (int i = 0; i < nCount; ++i)
            {
                nPose = abPaths[i].LastIndexOf(dataDir);
                string realPath = abPaths[i].Substring(nPose + nLen);
                realPath = realPath.Replace('\\', '/');

            
                if (_dicBundlePos.ContainsKey(realPath) == true)
                {
                    PosInfo pi = _dicBundlePos[realPath];
                    posType = pi.abResPos;

                    //不打固件资源的，固件资源放入apk内
                    if(!_isSplitFix&&AB_RES_POS.RES_POS_FIX== posType)
                    {
                        posType = AB_RES_POS.RES_POS_INNER;
                    }
                }
                else
                {
                    posType = AB_RES_POS.RES_POS_INNER;// 无主的，放内包，通常是动态生成的配置
                }

                string targetPath = null;

                switch (posType)
                {
                    case AB_RES_POS.RES_POS_INNER:
                        targetPath = InnerPath + "/" + realPath;
                        break;
                    case AB_RES_POS.RES_POS_FIX:
                        targetPath = FixPath + "/" + realPath;
                        break;
                    case AB_RES_POS.RES_POS_WEB:
                        targetPath = WebPath + "/" + realPath;
                        break;
                    case AB_RES_POS.RES_POS_DEFAULT:
                        targetPath = InnerPath + "/" + realPath;
                        break;
                    default:
                        targetPath = InnerPath + "/" + realPath;
                        break;
                }

                //复制文件
                __CopyFile(abPaths[i], targetPath);

            }

            Debug.Log("拆分完成");

            //输出web配置到streaming目录下
            ExportWebCfg();

            //压缩web资源
            if (UpdateConfig.isZipWebData)
            {
                ZipRes(WebPath);
            }
           





        }

        static void EncryptRes(string dirPath)
        {
            if (Directory.Exists(dirPath) == false)
            {
                return;
            }

            byte[] buffer = new byte[5];
            string[] filePaths = Directory.GetFiles(dirPath, "*.bin", SearchOption.AllDirectories);
            int nCount = filePaths.Length;
            for (int i = 0; i < nCount; ++i)
            {
                //string str = filePaths[i].Replace("\\", "/");
                byte[] data = File.ReadAllBytes(filePaths[i]); 
               
                int size = data.Length + UpdateConfig.EncryData.Length;
                if (buffer.Length!= size)
                {
                    buffer = new byte[size];
                }
                Array.Copy(UpdateConfig.EncryData, buffer, UpdateConfig.EncryData.Length);
                Array.Copy(data, 0,buffer, UpdateConfig.EncryData.Length, data.Length);
                File.Delete(filePaths[i]);
                File.WriteAllBytes(filePaths[i], buffer);
            }
        }

        static void ZipRes(string dirPath)
        {
            if (Directory.Exists(dirPath) == false)
            {
                return;
            }
            string[] filePaths = Directory.GetFiles(dirPath, "*.bin", SearchOption.AllDirectories);
            int nCount = filePaths.Length;
            for (int i = 0; i < nCount; ++i)
            {
                string str = filePaths[i].Replace("\\","/");
                string dst = str + ".zip";
                ZipHelper.ZipFile(str, dst);
                File.Delete(str);
                File.Move(dst, str);
     
            }
        }



        //列表排序比较类
        public class CompareFunc : IComparer<ResItem>
        {
            //优先级列表
            public List<string> sortList;

            public Action<string> pathConvert;
            public int Compare(ResItem a, ResItem b)
            {

                string pa = null;
                string pb = null;
                PosInfo pi = null;
                if(_dicBundlePos.TryGetValue(a.path, out pi)==false)
                {
                    Debug.LogError("资源不存在： "+ a.path);
                    return -1;
                }

                pa = pi.path;

                if (_dicBundlePos.TryGetValue(b.path, out pi) == false)
                {
                    Debug.LogError("资源不存在： " + b.path);
                    return -1;
                }

                pb = pi.path;

                int idx1 = GetSortIndex(pa);
                int idx2 = GetSortIndex(pb);
                if (idx1 == idx2)
                {
                    if (pa.Length == pb.Length)
                    {
                        return 0;
                    }

                    return pa.Length > pb.Length ? 1 : -1;
                }

                return idx1 > idx2 ? 1 : -1;
            }

            private int GetSortIndex(string str)
            {
                int nPos = 0;
                int nCount = sortList.Count;
                for (int i = 0; i < nCount; ++i)
                {
                    nPos = str.IndexOf(sortList[i]);
                    if (nPos >= 0)
                    {
                        return i;
                    }
                }
                return nCount;
            }
        }


        //导出web资源配置
        static void ExportWebCfg()
        {
            if (Directory.Exists(WebPath) == false)
            {
                return;
            }
            WebLoadInfoConfig webCfg = new WebLoadInfoConfig();
            webCfg.m_webDir = UpdateConfig.WEB_REAL_DIR + "_" + _versionStr;

            string[] filePaths = Directory.GetFiles(WebPath, "*.*", SearchOption.AllDirectories);
            string webdir = WebPath + "\\";
            int nCount = filePaths.Length;
            for (int i = 0; i < nCount; ++i)
            {
               
                string strfile = filePaths[i].Replace(webdir, "");
                FileInfo fileInfo = new FileInfo(filePaths[i]);
                string md5 = MD5.GetMD5(filePaths[i]);
                int hashKey = A2BRecord.GetHashCode(md5); 
                strfile = strfile.Replace('\\', '/');
                webCfg.AddItem(strfile, (int)fileInfo.Length, (uint)(hashKey));
            }


            List<string> listSorts = new List<string>();
            string dir = Application.dataPath + "//..//" + _SpiltCheckList[(int)AB_RES_POS.RES_POS_WEB];
            __GetSortList(listSorts, dir);


            CompareFunc cf = new CompareFunc();
            cf.sortList = listSorts;
            webCfg.m_resList.Sort(cf);

           // webCfg.SortItem(listSorts);

            //导入到apk 内包
            if (Directory.Exists(InnerPath))
            {
                webCfg.Save(InnerPath + "/" + UpdateConfig.WEB_INFO_CFG);
            }

            //导入到安卓ab生成资源目录，生成热更新包
            webCfg.Save(SelectABOutputABFolderPath + "/" + UpdateConfig.WEB_INFO_CFG);

            //游戏内,先读取热更包资源，再读取内包资源

        }

        static void __GetSortList(List<string> listSorts, string dir)
        {
            if (Directory.Exists(dir) == false)
            {
                return;
            }

            string[] resFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            int nCount = resFiles.Length;
            for (int i = 0; i < nCount; ++i)
            {
                string[] lines = File.ReadAllLines(resFiles[i]);
                int nLen = lines.Length;
                for (int n = 0; n < nLen; ++n)
                {
                    string[] contens = lines[n].Split(',');
                    for (int k = 0; k < contens.Length; ++k)
                    {
                        if (string.IsNullOrEmpty(contens[k]))
                        {
                            continue;
                        }

                        listSorts.Add(contens[k].Replace('\\', '/').ToLower());
                    }

                }
            }


        }

        static void __CopyFile(string strPath, string strTarget)
        {
            int nPose = strTarget.LastIndexOf('/');
            if (nPose < 0)
            {
                Debug.LogError("拷贝文件夹，没有拆分出目标目录");
                return;
            }

            string dir = strTarget.Substring(0, nPose);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            FileUtil.ReplaceFile(strPath, strTarget);


        }

        static void __Copy2Directory(string src, string dst)
        {

            if (Directory.Exists(src) == false)
            {
                Debug.LogError(src + "   不存在");
                return;
            }


            src = src.Replace("\\", "/");
            dst = dst.Replace("\\", "/");

            string targetFile;
            string srcFile;
            string[] paths = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
            int nCount = paths.Length;
            for (int i = 0; i < nCount; ++i)
            {
                srcFile = paths[i].Replace("\\", "/");
                targetFile = srcFile.Replace(src, dst);
                __CopyFile(srcFile, targetFile);
            }

        }

        //单个文件解析
        static void __PareSplitCfgFile(string path, Dictionary<string, bool> dicResPaths)
        {
            string[] fileContens = File.ReadAllLines(path);
            int nCount = fileContens.Length;
            for (int i = 0; i < nCount; ++i)
            {
                string[] filePaths = fileContens[i].Split(',');
                for (int j = 0; j < filePaths.Length; ++j)
                {
                    string filepath = filePaths[j];
                    if (string.IsNullOrEmpty(filepath))
                    {
                        continue;
                    }

                    //修正成绝对路径
                    string Absolutepath = Application.dataPath + "/" + filepath;

                    //存在这个目录
                    if (Directory.Exists(Absolutepath))
                    {
                        string[] resFiles = Directory.GetFiles(Absolutepath, "*.*", SearchOption.AllDirectories);
                        int nResCount = resFiles.Length;
                        for (int n = 0; n < nResCount; ++n)
                        {
                            int nPos = resFiles[n].LastIndexOf(".meta");
                            if (nPos >= 0)
                            {
                                continue;
                            }

                            nPos = resFiles[n].LastIndexOf("Assets/");
                            if (nPos <= 7)
                            {
                                continue;
                            }

                            filepath = resFiles[n].Substring(nPos + 7); //跳过Assets/

                            filepath = filepath.Replace('\\', '/');
                            if (dicResPaths.ContainsKey(filepath) == false)
                            {
                                dicResPaths.Add(filepath, true);
                            }
                        }
                    }
                    else if (File.Exists(Absolutepath))   //存在这个文件
                    {
                        filepath = filepath.Replace('\\', '/');
                        if (dicResPaths.ContainsKey(filepath) == false)
                        {
                            dicResPaths.Add(filepath, true);
                        }
                    }

                }
            }
        }

        //查找某个目录下的配置，建立路径索引表
        static void __BuildSplitResPathCfg(string path, Dictionary<string, bool> dicResPaths)
        {

            dicResPaths.Clear();
            string[] filePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            int nCount = filePaths.Length;
            for (int i = 0; i < nCount; ++i)
            {
                __PareSplitCfgFile(filePaths[i], dicResPaths);
            }

        }


        //资源包位置构建
        static void __BuildSplitBundlePosCfg(List<Dictionary<string, bool>> listPaths, Dictionary<string, PosInfo> dicResPoss)
        {
            bool[] checkAuthority = { !_isFullPack, true, _isSplitWeb };
            //完整包屏蔽固件和web
            if (_isFullPack)
            {
                checkAuthority[(int)AB_RES_POS.RES_POS_FIX] = false;
                checkAuthority[(int)AB_RES_POS.RES_POS_WEB] = false;
            }

            //建立包体索引
            listPaths.Clear();
            int nLen = _SpiltCheckList.Length;
            for (int i = 0; i < nLen; ++i)
            {
                Dictionary<string, bool> dicPathsCfg = new Dictionary<string, bool>();

                //打开了权限才做处理
                if (checkAuthority[i])
                {
                    string path = Application.dataPath + "//..//" + _SpiltCheckList[i];
                    __BuildSplitResPathCfg(path, dicPathsCfg);
                }

                listPaths.Add(dicPathsCfg);
            }

            //构建AB位置表
            dicResPoss.Clear();

            //设置资源位置
            List<EditorA2BRecord> listRecord = NewGAsset2BundleRecords.Instance.allRecord.RecordList;
            int nCount = listRecord.Count;
            int nPosType = 0;
            for (int i = 0; i < nCount; ++i)
            {
                EditorA2BRecord record = listRecord[i];

              
                //插入到原始表
                if (dicResPoss.ContainsKey(record.bundleName) == false)
                {
                    PosInfo pi = new PosInfo();
                    pi.abResPos = AB_RES_POS.RES_POS_DEFAULT;
                    pi.path = record.assetPath.ToLower();
                    dicResPoss.Add(record.bundleName, pi);
                    nPosType = (int)AB_RES_POS.RES_POS_DEFAULT;
                }
                else
                {
                    PosInfo pi = dicResPoss[record.bundleName];
                    nPosType = (int)pi.abResPos;
                }

                if(nPosType> (int)AB_RES_POS.RES_POS_INNER)
                {

                    for (int j = 0; j < nLen; ++j)
                    {
                        Dictionary<string, bool> dicPathsCfg = listPaths[j];
                        if (dicPathsCfg.ContainsKey(record.assetPath))
                        {
                            if (nPosType > j)
                            {
                                nPosType = j;
                                PosInfo pi = dicResPoss[record.bundleName];
                                pi.abResPos = (AB_RES_POS)j;
                                pi.path = record.assetPath.ToLower();
                            }

                            break;
                        }
                    }
                }



            }


            //PosInfo pi2 = dicResPoss["qbihni.bin"];
            //Debug.LogError(pi2.path);

            //同步存放位置信息到A2BRecord
            /*
            for (int i = 0; i < nCount; ++i)
            {
                A2BRecord record = listRecord[i];
                if (dicResPoss.ContainsKey(record.bundleName))
                {
                    record.byResPosType = (byte)dicResPoss[record.bundleName];
                }else
                {
                    record.byResPosType = (int)AB_RES_POS.RES_POS_DEFAULT;
                }
            }
            */
        }












        private string _printAllBundleAssetsPath;
        private string _testLoopAsset;
        private string _testHash;
        private bool m_isShowBundleEx = false;
        private string m_targetBundleFolder;
        private string m_targetBundleNameRecordPath;

        private void DrawTest()
        {
            if (GUILayout.Button("自动增量打AB"))
            {
                //AutoBuildInermentAB();
            }
            if (GUILayout.Button("刷新缓存"))
            {
                NewGAssetDataCache.Instance.RefreshCache();
            }
            if (GUILayout.Button("生成资源对应AB包名文件"))
            {
                NewGAsset2BundleRecords.Instance.RefreshRecord();
            }
            if (GUILayout.Button("导出资源映射文件到 StreamingAssets"))
            {
                NewGAsset2BundleRecords.Instance.Export();
            }
            //if (GUILayout.Button("导出新索引表"))
            //{
            //    NewGAsset2BundleRecords.Instance.NewExport();
            //}
            //if (GUILayout.Button("设置GameConfigIni"))
            //{
            //    m_GameConfigIni.WriteValue(GameIni.GAME_INI_SECTION, GameIni.GAME_INI_UWA, true);
            //    m_GameConfigIni.WriteValue(GameIni.GAME_INI_SECTION, GameIni.GAME_INI_PUBLISH, true);
            //    SaveGameConfig();
            //}

            if (GUILayout.Button("拷贝资源记录缓存"))
            {
                //SerAssetDataCache(NewGAssetDataCache.Instance.assetDataList, SelectABOutputAssetCacheChildPath);
            }

            EditorGUI.indentLevel++;
            m_isShowBundleEx = EditorGUILayout.Foldout(m_isShowBundleEx, "AssetBundle EX");
            if (m_isShowBundleEx)
            {
                EditorGUILayout.BeginVertical("Box");
                {
                    DrawFolderEdit("AB包存放目录", ref m_targetBundleFolder);
                    if (string.IsNullOrEmpty(m_targetBundleNameRecordPath))
                        m_targetBundleNameRecordPath = NewAssetDataDef.AssetBundleNameWindowPath;
                    DrawFolderEdit("指定AB包名索引表", ref m_targetBundleNameRecordPath, false);

                    if (GUILayout.Button("恢复路径"))
                    {
                        RecoverAssetBundlePath(m_targetBundleNameRecordPath, m_targetBundleFolder);
                    }

                    if (GUILayout.Button("缩短路径"))
                    {
                        SimplifyAssetBundlePath(m_targetBundleNameRecordPath, m_targetBundleFolder);
                    }

                    if (GUILayout.Button("打印AB包名反向索引表"))
                    {
                        var dic = BinaryFormatterUtil.LoadBinaryFormatterFile<Dictionary<string, string>>(m_targetBundleNameRecordPath);
                        if (dic != null)
                        {
                            foreach (var pairs in dic)
                            {
                                Debug.Log($"{pairs.Key} -> {pairs.Value}");
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;
            _testHash = EditorGUILayout.TextField("A2BRecord哈希值 测试", _testHash);
            if (GUILayout.Button("A2BRecord哈希值 GetHashCode"))
            {
                Debug.Log($"Hash:[{A2BRecord.GetHashCode(_testHash)}], Path:[{_testHash}]");
            }
            if (GUILayout.Button("A2BRecord哈希值 GetHashCodeString"))
            {
                Debug.Log($"Hash:[{A2BRecord.GetHashCodeString(_testHash)}], Path:[{_testHash}]");
            }
            if (GUILayout.Button("A2BRecord哈希值 翻译AB包名编码"))
            {
                var dic = BinaryFormatterUtil.LoadBinaryFormatterFile<Dictionary<string, string>>(m_targetBundleNameRecordPath);
                if (dic != null)
                {
                    foreach (var pairs in dic)
                    {
                        if (_testHash == pairs.Key)
                        {
                            Debug.Log($"Hash:[{A2BRecord.GetHashCodeString(_testHash)}], ABName:[{pairs.Value}]");
                            break;
                        }
                    }
                }
            }

            /*
            _deleteFile = EditorGUILayout.TextField("删文件", _deleteFile);
            if (GUILayout.Button("删文件"))
            {
                File.Delete(_deleteFile);
            }
            if (GUILayout.Button("删除Streaming目录下多余Mainfest"))
            {
                DeleteManifest(StreamingABFolderPath);
            }

            DrawTestGetDependencies();
            DrawTestOutputCSVAsset();

            DrawTestNewAB();
            _printAllBundleAssetsPath = EditorGUILayout.TextField("打印bundle下所有资源", _printAllBundleAssetsPath);
            if (GUILayout.Button("打印bundle下所有资源"))
            {
                var allAsset = AssetDatabase.GetAssetPathsFromAssetBundle(_printAllBundleAssetsPath);
                for (int i = 0; i < allAsset.Length; i++)
                {
                    Debug.Log($"{i}: {allAsset[i]}");
                }
            }
    
            if (GUILayout.Button("钉钉消息发送"))
            {
                DoSendDingDing();
            }
            if (GUILayout.Button("开始接收错误log"))
            {
                StartReceivedErrorLog();
            }
            if (GUILayout.Button("停止接收错误log"))
            {
                Debug.Log(EndReceivedErrorLog());
            }
            if (GUILayout.Button("测试错误log"))
            {
                Debug.LogError("测试错误log啊啊啊");
            }
            */


            _testLoopAsset = EditorGUILayout.TextField("测是否循环依赖：", _testLoopAsset);
            if (GUILayout.Button("测是否循环依赖"))
            {
                HashSet<string> tem = new HashSet<string>();
                bool isFind;
                var link = NewGAssetDataCache.Instance.CheckLoopSame(_testLoopAsset, _testLoopAsset, ref tem, out isFind);
                Debug.Log($"isFind:{isFind}, link:{link}, mapCount:{tem.Count}");
            }


            if (GUILayout.Button("测所有是否循环依赖"))
            {
                var result = NewGAssetDataCache.Instance.CheckLoopDepend();
                StringBuilder str = new StringBuilder();
                str.AppendLine("测所有是否循环依赖");
                for (int i = 0; i < result.Count; i++)
                {
                    str.AppendLine($"[{i}] = {result[i]}");
                }
                Debug.LogError(str);
            }
        }


        string m_recordPath;
        private void DrawResRecord()
        {
            DrawFolderEdit("转换资源路径", ref m_recordPath);
            if (string.IsNullOrEmpty(m_recordPath))
                m_recordPath = Application.dataPath+ "/../RecordRes.txt";

            if (GUILayout.Button("转换所有依赖"))
            {
               if(File.Exists(m_recordPath))
                {
                    ReferenceFinderData.ConvertDependcyRecords(m_recordPath, Application.dataPath + "/../RecordRes_Finnal.txt");
                   
                }
            }

        }
        string m_abPath;
        private void DrawABDecode()
        {
            DrawFolderEdit("解密AB资源路径",ref m_abPath);
            //if (string.IsNullOrEmpty(m_abPath))
            //m_abPath = Application.dataPath + "/../test/1a2ydvr.bin";
            m_abPath = Application.dataPath + "/../test/";


            if (GUILayout.Button("ab资源解密"))
            {
                if (Directory.Exists(m_abPath))
                {
                    Debug.LogError(m_abPath);
                    string m_abPaths = Application.dataPath + "/../test/";
                    string[] abPaths= Directory.GetFiles(m_abPaths, "*.bin", SearchOption.AllDirectories);
                    Debug.LogError(abPaths.Length);
                    for (int i = 0; i < abPaths.Length; i++)
                    {
                        string filename = Path.GetFileName(abPaths[i]);
                        byte[] content = File.ReadAllBytes(abPaths[i]);
                        byte[] src = content;
                        byte[] dst = new byte[content.Length - UpdateConfig.EncryData.Length];
                        Array.Copy(src, UpdateConfig.EncryData.Length, dst, 0, dst.Length);
                        if (!Directory.Exists(Application.dataPath + "/abdecodePath/"))
                        {
                            Directory.CreateDirectory(Application.dataPath + "/abdecodePath/");
                        }
                        File.WriteAllBytes(Application.dataPath + "/abdecodePath/" + filename, dst);

                    } Debug.LogError("AB资源解密结束");
                }
            }
        }


        string m_verInfoPath;
        private void DrawEncryVerInfo()
        {

            m_verInfoPath = Application.dataPath + "/../BuildApp/versioninfo.bin";
            m_verInfoPath = EditorGUILayout.TextField("版本号文件：", m_verInfoPath);

            if (GUILayout.Button("重新生成检验"))
            {
                if (File.Exists(m_verInfoPath))
                {

                    UpdateFileUtil.RewriteLenChcekFile(m_verInfoPath);
                    Debug.LogError("重新生成完毕 " + m_verInfoPath);

                }
            }
            if (GUILayout.Button("生成强更版本号文件"))
            {
                if (File.Exists(m_verInfoPath))
                {

                    UpdateFileUtil.RewriteForeceUpdateVersionInfo(m_verInfoPath);
                    Debug.LogError("重新生成完毕 " + m_verInfoPath);

                }
            }

            if (GUILayout.Button("改成热更版本号文件"))
            {
                if (File.Exists(m_verInfoPath))
                {

                    ulong lastVersion = UpdateFileUtil.RewriteHotUpdateVersionInfo(m_verInfoPath);

                    string lastUpdateDir = Application.dataPath + "/../BuildApp/pkg_" + lastVersion + "/publish/ver_" + lastVersion;
                    UpdateFileUtil.CreateFileInfo(lastUpdateDir,"",true,false);
                    Debug.LogError("重新生成完毕 " + m_verInfoPath);

                }
            }
        }

        //恢复已编码路径AB
        private static void RecoverAssetBundlePath(string assetBundleNamePath, string folder)
        {
            var assetBundleNameDic = BinaryFormatterUtil.LoadBinaryFormatterFile<Dictionary<string, string>>(assetBundleNamePath);
            if (assetBundleNameDic != null)
                ConversionBundlePath(folder, assetBundleNameDic);
        }

        //编码路径AB
        private static void SimplifyAssetBundlePath(string assetBundleNamePath, string folder)
        {
            var assetBundleNameDic = BinaryFormatterUtil.LoadBinaryFormatterFile<Dictionary<string, string>>(assetBundleNamePath);
            if (assetBundleNameDic != null)
            {
                assetBundleNameDic = assetBundleNameDic.ToDictionary(x => x.Value, y => y.Key);
                ConversionBundlePath(folder, assetBundleNameDic);
            }
        }

        //转换AB路径
        private static void ConversionBundlePath(string folder, Dictionary<string, string> assetBundleNameDic)
        {
            var dir = new DirectoryInfo(folder);
            string[] oldAssetFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            bool isCancel = false;
            string partten1 = "\\";
            string partten2 = "/";
            string partten3 = folder + "\\";
            for (int i = 0; i < oldAssetFiles.Length; i++)
            {
                string oldPath = oldAssetFiles[i];
                string oldName = oldPath.Replace(partten3, string.Empty).Replace(partten1, partten2);
                isCancel = EditorUtility.DisplayCancelableProgressBar($"转换AB路径: ({i} / {oldAssetFiles.Length})", $"{oldName}", (float)i / oldAssetFiles.Length);
                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
                if (assetBundleNameDic.ContainsKey(oldName))
                {
                    string newPath = folder + "/" + assetBundleNameDic[oldName];
                    //Debug.Log($"oldName = {oldName}, oldPath = {oldPath}, newPath = {newPath}");
                    (new FileInfo(newPath)).Directory.Create();
                    File.Copy(oldPath, newPath);
                    File.Delete(oldPath);
                }
                else
                {
                    Debug.LogError($"移动失败，索引表中找不到： {oldName}");
                }
            }
            EditorUtility.DisplayProgressBar($"清除空文件夹", string.Empty, 100);
            DeleteEmptyDirectory(dir);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        // 删除空文件夹
        public static void DeleteEmptyDirectory(DirectoryInfo dir)
        {
            DirectoryInfo[] subdirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in subdirs)
            {
                DeleteEmptyDirectory(subdir);
            }
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            if (files.Length == 0)
                dir.Delete();
        }

        //序列化资源记录缓存，用与增量打AB
        static private void SerAssetDataCache(List<NewAssetData> src, string filePath)
        {
            BinaryFormatter binFormat = new BinaryFormatter();
            new FileInfo(filePath).Directory.Create();
            //保存数据
            Stream fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            binFormat.Serialize(fStream, src);
            fStream.Close();
            Debug.Log($"导出资源记录缓存文件:{filePath}");
        }

        //获取上一个版本的缓存目录
        private static string GetLastAssetCacheDir()
        {



            string vesionInfoPath = BuildConfigPath + "/" + UpdateConfig.UPDATE_VERSION;
            ulong NO = 0;
            ulong lastVer = ulong.Parse(_versionStr);
            if (File.Exists(vesionInfoPath))
            {
                while(GetLastVersion(out NO, lastVer))
                {
                    string oldABAssetCacheDir = BuildConfigPath + "/pkg_" + NO.ToString();
                    string oldcachePath = oldABAssetCacheDir + "/" + AssetCacheChildName;
                    if(File.Exists(oldcachePath))
                    {
                        return oldABAssetCacheDir;
                    }

                    lastVer = NO;
                }
               

            }
            return "";
        }

        //获取变更资源列表
        static bool GetModifiedResInfo(string oldABAssetDir, out HashSet<string> targetABSet, out HashSet<string> curNewABSet, out HashSet<string> modifiedSet)
        {
            targetABSet = new HashSet<string>();
            curNewABSet = new HashSet<string>();
            modifiedSet = new HashSet<string>();



            if (DrawProgressBar($"拷贝旧的缓存信息", "NewGAssetDataCache", 0)) return false;

            var oldCache = ScriptableObject.CreateInstance<NewGAssetDataCache>();
            string oldABAssetCachePath = oldABAssetDir + "/" + AssetCacheChildName;
            List<NewAssetData> oldCacheList = DeserAssetDataCache(oldABAssetCachePath);
            if (oldCacheList == null)
            {
                oldCache.assetDataList = new List<NewAssetData>();
            }
            else
            {
                oldCache.assetDataList = oldCacheList;
            }
            oldCache.InitDic();
            var oldCacheCount = oldCache.assetDataList.Count;

            if (DrawProgressBar($"拷贝旧的缓存信息", "NewGAsset2BundleRecords", 0)) return false;

            var oldRecords = ScriptableObject.CreateInstance<NewGAsset2BundleRecords>();
            oldRecords.allRecord = new EditorAsset2BundleRecords();
            oldRecords.allRecord.SetRecordList(new List<EditorA2BRecord>(NewGAsset2BundleRecords.Instance.allRecord.RecordList));

            if (DrawProgressBar($"刷新缓存信息", "刷新缓存信息", 0)) return false;




            var newCache = NewGAssetDataCache.Instance;
            var newRecord = NewGAsset2BundleRecords.Instance;


            if (EditorUtility.DisplayCancelableProgressBar($"对比修改信息", "对比修改信息", 0)) return false;

            Debug.LogError($"【【【对比新旧AB数据：\nCache Count：{oldCache.assetDataList.Count} -> {newCache.assetDataList.Count}\n" +
                $"Record Count: {oldRecords.allRecord.Count} -> {newRecord.allRecord.Count}");

            List<NewAssetData> modifyAssetList = new List<NewAssetData>();
            List<NewAssetData> deleteAssetList = new List<NewAssetData>();
            var newCacheCount = newCache.assetDataList.Count;
            for (int i = 0; i < newCacheCount; i++)
            {
                var newData = newCache.assetDataList[i];
                if (!oldCache.Contains(newData.assetPath))
                {
                    modifyAssetList.Add(newData);
                }
                else
                {
                    var oldData = oldCache.GetAssetData(newData.assetPath);
                    if (oldData.hash != newData.hash)
                    {
                        modifyAssetList.Add(newData);
                    }
                }
            }
            var deleteAssetNeedUpdateList = new List<EditorA2BRecord>();
            for (int i = 0; i < oldCacheCount; i++)
            {
                var oldData = oldCache.assetDataList[i];
                if (!newCache.Contains(oldData.assetPath))
                {
                    deleteAssetList.Add(oldData);
                    var oldRecordList = oldRecords.allRecord._recordList;
                    for (int j = 0; j < oldRecordList.Count; j++)
                    {
                        var dependArr = oldRecordList[j].dependArr;
                        for (int k = 0; k < dependArr.Length; k++)
                        {
                            if (dependArr[k] == oldData.assetPath)
                            {
                                Debug.Log($"删除资源 影响依赖AB包 [{j} - {oldData.assetPath}] -> {oldRecordList[j]}");
                                deleteAssetNeedUpdateList.Add(oldRecordList[j]);
                                break;
                            }
                        }
                    }
                }
            }


            if (EditorUtility.DisplayCancelableProgressBar($"对比修改信息", $"发现资源新增、修改数量：{modifyAssetList.Count}，删除数量：{deleteAssetList.Count}", 0)) return false;
            Debug.LogError($"【【【发现资源新增、修改数量：{modifyAssetList.Count}，删除数量：{deleteAssetList.Count}");

            List<EditorA2BRecord> modifyRecordList = new List<EditorA2BRecord>();
            List<EditorA2BRecord> deleteRecordList = new List<EditorA2BRecord>();
            for (int i = 0; i < modifyAssetList.Count; i++)
            {
                //modifiedSet.Add(modifyAssetList[i].assetPath);
                var record = newRecord.allRecord.GetRecord(XGameEditorUtilityEx.RelativeToAssetsPath(modifyAssetList[i].assetPath));
                if (record != null && !modifyRecordList.Contains(record))
                {
                    modifyRecordList.Add(record);



                    if (targetABSet.Contains(record.bundleName) == false)
                    {
                        targetABSet.Add(record.bundleName);
                    }

                    if(modifiedSet.Contains(record.assetPath)==false)
                    {
                        modifiedSet.Add(record.assetPath);
                    }
                    

                    Debug.Log($"新增AB：{record.bundleName}");
                }
            }
            for (int i = 0; i < deleteAssetList.Count; i++)
            {
                var record = oldRecords.allRecord.GetRecord(XGameEditorUtilityEx.RelativeToAssetsPath(deleteAssetList[i].assetPath));
                if (record != null && !deleteRecordList.Contains(record))
                {
                    deleteRecordList.Add(record);
                    if (targetABSet.Contains(record.bundleName) == false)
                    {
                        targetABSet.Add(record.bundleName);
                    }

                    if (modifiedSet.Contains(record.assetPath) == false)
                    {
                        modifiedSet.Add(record.assetPath);
                    }

                    Debug.Log($"删除AB：{record.bundleName}");
                }
            }


            if (EditorUtility.DisplayCancelableProgressBar($"对比修改信息", $"发现资源新增、修改Bundle数量：{modifyRecordList.Count}，删除Bundle数量：{deleteRecordList.Count}", 0)) return false;
            Debug.LogError($"【【【发现资源新增、修改Bundle数量：{modifyRecordList.Count}，删除Bundle数量：{deleteRecordList.Count}");


            //挑选ab列表
            //SelectModifyAB2(newRecord.allRecord, targetABSet, modifiedSet,out curNewABSet);



            return true;

        }

        static private void SelectFileDepencys(EditorAsset2BundleRecords allRecord, HashSet<string> selectABSet, HashSet<string> selectPathSet,string path)
        {
            //已经处理过了的，直接返回
            if(selectPathSet.Contains(path))
            {
                return ;
            }

            selectPathSet.Add(path);

            EditorA2BRecord record = allRecord.GetRecord(path);
            if (record==null)
            {
                return;
            }


            //保存增量 ab的名字
            if(selectABSet.Contains(record.bundleName)==false)
            {
                selectABSet.Add(record.bundleName);
            }

            //处理依赖部分
            for(int i=0;i< record.dependArr.Length;++i)
            {
                SelectFileDepencys( allRecord, selectABSet, selectPathSet, record.dependArr[i]);
            }

        }

        static private void SelectFileDepencys2(EditorAsset2BundleRecords allRecord, Dictionary<string, List<EditorA2BRecord>> dicParentDependcy, HashSet<string> selectABSet, HashSet<string> selectPathSet, string path,bool bCheckDependcy,bool bCheckBeDependcy)
        {
            //已经处理过了的，直接返回
            if (selectPathSet.Contains(path))
            {
                return;
            }

            selectPathSet.Add(path);

            EditorA2BRecord record = allRecord.GetRecord(path);
            if (record == null)
            {
                return;
            }


            //保存增量 ab的名字(自身AB)
            if (selectABSet.Contains(record.bundleName) == false)
            {
                selectABSet.Add(record.bundleName);
            }

            //是否檢測依賴
            if(bCheckDependcy)
            {
                //处理依赖部分(当前资源，依赖的资源)
                for (int i = 0; i < record.dependArr.Length; ++i)
                {
                    SelectFileDepencys2(allRecord, dicParentDependcy, selectABSet, selectPathSet, record.dependArr[i],true,false);
                }
            }
          

            if(bCheckBeDependcy)
            {
                //处理被依赖部分(当前资源被依赖的部分)
                List<EditorA2BRecord> listEB = null;
                if (dicParentDependcy.TryGetValue(path, out listEB))
                {
                    for (int i = 0; i < listEB.Count; ++i)
                    {
                        //不再檢查子以來，不然就是全量包了
                        SelectFileDepencys2(allRecord, dicParentDependcy, selectABSet, selectPathSet, listEB[i].assetPath, false,true);
                    }
                }
            }
           


        }


        private static void SelectModifyAB2(EditorAsset2BundleRecords allRecord, HashSet<string> targetABSet, HashSet<string> modifiedSet, out HashSet<string> curNewABSet,string lastDir,string curDir)
        {
            curNewABSet = new HashSet<string>();

         
            //建立依赖索引 path ->parent depend EditorRecord
            Dictionary<string, List<EditorA2BRecord>> dicParentDependcy = new Dictionary<string, List<EditorA2BRecord>>();

            List<EditorA2BRecord> listEB = null;
            EditorA2BRecord record = null;
            for (int i = 0; i < allRecord.Count; ++i)
            {
                record = allRecord._recordList[i];

                //记录所有ab
                if (curNewABSet.Contains(record.bundleName) == false)
                {
                    curNewABSet.Add(record.bundleName);
                }


                //建立依賴索引
                if (record.dependArr.Length>0)
                {
                    for (int j=0;j< record.dependArr.Length;++j)
                    {
                        if(dicParentDependcy.TryGetValue(record.dependArr[j], out listEB)==false)
                        {
                            listEB = new List<EditorA2BRecord>();
                            //创建记录
                            dicParentDependcy.Add(record.dependArr[j],listEB);
                        }

                        EditorA2BRecord parentRecord = allRecord.GetRecord(record.assetPath);
                        if (listEB.IndexOf(parentRecord) <0)
                        {
                            listEB.Add(parentRecord);
                        }
                    }
                }
            }


            //檢測是否已經有被刪除了的資源
            HashSet<string> selectPaths = new HashSet<string>();
            Dictionary<string, List<string>> oldBeDepency = null;
            List<string> beDepencys = null;
            foreach (string path in modifiedSet)
            {
                //資源存在的，跳過
                EditorA2BRecord parentRecord = allRecord.GetRecord(path);
                if(null!= parentRecord)
                {
                    continue;
                }

                if(null== oldBeDepency)
                {
                    oldBeDepency = LoadBedepency(lastDir+"/Bedepency.txt");
                }

                if(oldBeDepency.TryGetValue(path,out beDepencys))
                {
                    for(int i=0;i< beDepencys.Count;++i)
                    {
                        if (selectPaths.Contains(beDepencys[i]) == false)
                        {
                            selectPaths.Add(beDepencys[i]);
                        }
                    }
                }

            }

            //同步到更改集合
            foreach (string path in selectPaths)
            {
                if(modifiedSet.Contains(path)==false)
                {
                    modifiedSet.Add(path);
                }
              
            }


            //处理启动部分的文件资源
            selectPaths.Clear();
            foreach (string path in modifiedSet)
            {
                SelectFileDepencys2(allRecord, dicParentDependcy,targetABSet, selectPaths, path,true,true);
            }

            //同步更改目录
            modifiedSet.Clear();
            foreach (string path in selectPaths)
            {
                modifiedSet.Add(path);
            }

            //保持當前集合
            SaveBedepency(curDir + "/Bedepency.txt", dicParentDependcy);

        }

        //保存被引用關係
        private static void SaveBedepency(string filePath, Dictionary<string, List<EditorA2BRecord>> dicParentDependcy)
        {
            List<string> listPathDependcyMap = new List<string>();
            string Item = null;
            List<EditorA2BRecord> listER = null;
            foreach(var path in dicParentDependcy.Keys)
            {
                Item = path+"=";
                listER = dicParentDependcy[path];
                for(int i=0;i<listER.Count;++i)
                {
                    Item += listER[i].assetPath + ",";
                }
                listPathDependcyMap.Add(Item);
            }

            File.WriteAllLines(filePath,listPathDependcyMap);
        }

        private static Dictionary<string, List<string>>  LoadBedepency(string filePath)
        {
            Dictionary<string, List<string>> dicParentDependcy = new Dictionary<string, List<string>>();
            if (File.Exists(filePath))
            {
                string[] allItems = File.ReadAllLines(filePath);
                for (int i = 0; i < allItems.Length; ++i)
                {
                    string[] keyValues = allItems[i].Split('=');
                    if (keyValues.Length < 2)
                    {
                        Debug.LogError("LoadBedepency: 格式錯誤" + allItems[i]);
                        continue;
                    }

                    string[] Items = keyValues[1].Split(',');
                    List<string> listItem = new List<string>();
                    dicParentDependcy.Add(keyValues[0], listItem);
                    for (int j=0;j< Items.Length;++j)
                    {
                        listItem.Add(Items[j]);
                    }
                }
            }

            

            return dicParentDependcy;
        }


        private static void SelectModifyAB(EditorAsset2BundleRecords allRecord,  HashSet<string> targetABSet, HashSet<string> modifiedSet, out HashSet<string> curNewABSet)
        {
            curNewABSet = new HashSet<string>();
            //建立树状索引
            Dictionary<string, List<EditorA2BRecord>> dicParentDependcy = new Dictionary<string, List<EditorA2BRecord>>();
            for(int i=0;i< allRecord.Count;++i)
            {
                EditorA2BRecord record = allRecord._recordList[i];

                //记录所有ab
                if(curNewABSet.Contains(record.bundleName)==false)
                {
                    curNewABSet.Add(record.bundleName);
                }

                if(targetABSet.Contains(record.assetName)&&!modifiedSet.Contains(record.assetPath))
                {
                    modifiedSet.Add(record.assetPath);
                }
            }


            //处理启动部分的文件资源
            HashSet<string> selectPaths = new HashSet<string>();
            foreach (string path in modifiedSet)
            {
                SelectFileDepencys(allRecord, targetABSet, selectPaths, path);
            }

            //同步更改目录
            modifiedSet.Clear();
            foreach (string path in selectPaths)
            {
                modifiedSet.Add(path);
            }

             
        }


        private static void DoBuildInrementAB()
        {
            //版本文件
            string oldABAssetDir = GetLastAssetCacheDir();
            if (string.IsNullOrEmpty(oldABAssetDir)==false&&DoBuildInrementABImp(oldABAssetDir))
            {
                return;
            }

            BuildNewABRes();

        }

        // 执行增量打包
        private static bool DoBuildInrementABImp(string oldABAssetDir = default, string outputABPath = default)
        {
            bool isCancel = false;
            try
            {

               
                HashSet<string> targetABSet = null;// new HashSet<string>();//实际需要打的AB
                HashSet<string> curNewABSet = null;
                HashSet<string> modifiedSet = new HashSet<string>(); 
                GetModifiedResInfo(oldABAssetDir, out targetABSet, out curNewABSet, out modifiedSet);



                //加载老的ab映射表
                string oldMapPath = oldABAssetDir+"/" + AssetPathABMapFileName; 
                Dictionary<string, string> dicOldMap = new Dictionary<string, string>();
                LoadPath2ABMap(oldMapPath, dicOldMap);

                //加载新的ab映射表
                Dictionary<string, string> dicNewMap = new Dictionary<string, string>();
                LoadPath2ABMap(GenVersionPath+ "/" + AssetPathABMapFileName, dicNewMap);

                //计算old->new 的差异
                //modifiedSet = new HashSet<string>();
                CalcModilefiedABs(dicOldMap, dicNewMap, targetABSet,modifiedSet);
                //new ->old 的差异
                CalcModilefiedABs(dicNewMap,dicOldMap, targetABSet, modifiedSet);


                var newRecord = NewGAsset2BundleRecords.Instance;
                SelectModifyAB2(newRecord.allRecord, targetABSet, modifiedSet, out curNewABSet, oldABAssetDir, GenVersionPath);

              

                //在原来的ab文件中拷贝资源
                string oldDataDir = oldABAssetDir + "/" + CurPlatformName + "/" + UpdateConfig.DATA_DIR;

                //当前AB文件的路径
                string curDataDir = SelectABOutputABFolderPath;


                //出来丢失的资源
                HashSet<string> lostABSet = new HashSet<string>();
                CalcLostABs(dicNewMap, oldDataDir, lostABSet);
                foreach (string ab in lostABSet)
                {
                    if (targetABSet.Contains(ab) == false)
                    {
                        targetABSet.Add(ab);
                    }
                }


                //打AB包，假如列表为空，则相当于全打更新包
                BuildNewABRes(targetABSet);

               
                //同步老的资源
                DirectoryInfo diInfo = new DirectoryInfo(oldDataDir);
                if (!diInfo.Exists)
                {
                    DebugLog($"不存在文件夹：{outputABPath}", 3);
                }
                else
                {
                    string dstName = null;
                    FileInfo[] fileInfos = diInfo.GetFiles("*", SearchOption.AllDirectories);
                    for (int i = 0; i < fileInfos.Length; i++)
                    {
                        var file = fileInfos[i];
                        dstName = curDataDir + "/" + file.Name;
                        if (File.Exists(dstName) == false)
                        {
                            //curNewABSet 是当前新的所有ab名字集合
                            if (curNewABSet.Contains(file.Name) == true)
                            {
                                File.Copy(file.FullName, dstName);
                            }

                        }

                    }
                }

                //保存当前信息
                SerAssetDataCache(NewGAssetDataCache.Instance.assetDataList, SelectABOutputAssetCacheChildPath);

            }
            catch (Exception e)
            {
                Debug.LogError($"增量打AB异常：{e}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return true;
        }

        /// <summary>
        /// 选择依赖的AB
        /// </summary>
        /// <param name="src">索引记录</param>
        /// <param name="editorRecord">查找的资源索引</param>
        /// <param name="selectedABList">最终加进的列表</param>
        /// <param name="newRecordDependList">用于防止死循环</param>
        private static void SelectDependAB(EditorAsset2BundleRecords src, EditorA2BRecord editorRecord, HashSet<string> selectedABList, HashSet<string> newRecordDependList)
        {
            if (newRecordDependList.Contains(editorRecord.assetPath))
                return;

            newRecordDependList.Add(editorRecord.assetPath);
            var dependList = editorRecord.dependArr;
            for (int j = 0; j < dependList.Length; j++)
            {
                var assetPath = dependList[j];
                var dependRecord = src.GetRecord(assetPath);
                selectedABList.Add(dependRecord.bundleName);
                //这个资源依赖的AB也要打，不然它的依赖会单独打进这个包
                SelectDependAB(src, dependRecord, selectedABList, newRecordDependList);
            }
        }

        // 画进度条
        private static bool DrawProgressBar(string title, string content, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar(title, content, progress))
            {
                EditorUtility.ClearProgressBar();
                return true;
            }
            return false;
        }

        //反序列化资源记录缓存
        static private List<NewAssetData> DeserAssetDataCache(string filePath)
        {
            List<NewAssetData> cache = null;
            if (!File.Exists(filePath))
            {
                Debug.LogError($"该路径找不到： {filePath}");
                return cache;
            }
            BinaryFormatter binFormat = new BinaryFormatter();
            Stream fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            try
            {
                cache = binFormat.Deserialize(fStream) as List<NewAssetData>;
            }
            catch (Exception e)
            {
                Debug.LogError($"资源记录缓存反序列化失败： {filePath}, {e}");
            }
            finally
            {
                fStream.Close();
            }
            return cache;
        }

        public static void SavePath2ABMap(string path)
        {
            List<string> listPathABMap = new List<string>();
            var recordList = NewGAsset2BundleRecords.BuildABInstance.allRecord.RecordList;
            int count = recordList.Count;
            for (int i = 0; i < count; i++)
            {
                EditorA2BRecord record = recordList[i];
                listPathABMap.Add(record.assetPath);
                listPathABMap.Add(record.bundleName);
            }

            string dirPath = Path.GetDirectoryName(path);
            if(Directory.Exists(dirPath)==false)
            {
                Directory.CreateDirectory(dirPath);
            }

            //string strPath = GenABPath + "/" + AssetPathABMapFileName;
            File.WriteAllLines(path, listPathABMap);
        }

        public static void LoadPath2ABMap(string path,Dictionary<string,string> dicPath2AB)
        {
            dicPath2AB.Clear();
            if(File.Exists(path))
            {
                string[] path2abmaps = File.ReadAllLines(path);
                int maxIndex = path2abmaps.Length - 1;
                for (int i=0;i< maxIndex; i+=2)
                {
                    if(dicPath2AB.ContainsKey(path2abmaps[i])==false)
                    {
                        dicPath2AB[path2abmaps[i]] = path2abmaps[i + 1];
                    }
                }
            }
        }

        public static void CalcModilefiedABs(Dictionary<string, string> dicOldPath2AB, Dictionary<string, string> dicNewPath2AB, HashSet<string> targetABSet,HashSet<string> setNewAB)
        {
            //setNewAB.Clear();
            //先处理已经删除或者调整了打包策略的
            foreach(var path in dicOldPath2AB.Keys)
            {
                if (dicNewPath2AB.ContainsKey(path) ==false)
                {
                    if (setNewAB.Contains(path) == false)
                    {
                        setNewAB.Add(path);

                        if (targetABSet.Contains(dicOldPath2AB[path]) == false)
                        {
                            targetABSet.Add(dicOldPath2AB[path]);
                        }
                    }
                }
                else if(dicOldPath2AB[path]!= dicNewPath2AB[path]) //ab不一致
                {
                    if (setNewAB.Contains(path) == false)
                    {
                        setNewAB.Add(path);
                        if (targetABSet.Contains(dicNewPath2AB[path]) == false)
                        {
                            targetABSet.Add(dicNewPath2AB[path]);
                        }
                    }
                    if (setNewAB.Contains(path) == false)
                    {
                        setNewAB.Add(path);
                        if (targetABSet.Contains(dicNewPath2AB[path]) == false)
                        {
                            targetABSet.Add(dicNewPath2AB[path]);
                        }
                    }
                }
            }
        }


        //检测丢失的AB
        public static void CalcLostABs(Dictionary<string, string> dicNewPath2AB, string lastResDir,HashSet<string> setNewAB)
        {
            string path = null;
            setNewAB.Clear();
            //先处理已经删除或者调整了打包策略的
            foreach (var abName in dicNewPath2AB.Values)
            {
                path = lastResDir + "/" + abName;
                if(File.Exists(path)==false)
                {
                    if (setNewAB.Contains(abName) == false)
                    {
                        setNewAB.Add(abName);
                    }
                }
            }
        }




        public static string SynBuildAPKParam(EnBuildApkCmdArgs argsFlags)
        {
            

            _isCompatibleType = (argsFlags & EnBuildApkCmdArgs.IsCompatibleType) > 0;
            _isDiscardLuaLog = (argsFlags & EnBuildApkCmdArgs.IsDiscardLuaLog) > 0;
            _isFullPack = (argsFlags & EnBuildApkCmdArgs.IsFullPack) > 0;
            _isSplitFix = (argsFlags & EnBuildApkCmdArgs.IsSplitFix) > 0;
            _isSplitWeb = (argsFlags & EnBuildApkCmdArgs.IsSplitWeb) > 0;

            _isIncrementABPack = (argsFlags & EnBuildApkCmdArgs.IsIncrementABPack) > 0;
            _isSurportObb = (argsFlags & EnBuildApkCmdArgs.IsSurportObb) > 0;
            _isNoCSCodeUpdate = (argsFlags & EnBuildApkCmdArgs.IsNoCSCodeUpdate) > 0;
            _isDevelopment = (argsFlags & EnBuildApkCmdArgs.IsDevelopment) > 0;
            _isResHotupdateVer = (argsFlags & EnBuildApkCmdArgs.IsResHotupdateVer) > 0;
            _isCheckCodeModified = (argsFlags & EnBuildApkCmdArgs.IsCheckCodeModified) > 0;
            _isUpLoad = (argsFlags & EnBuildApkCmdArgs.IsUpLoad) > 0;
            _isAutoCopyToPulishFolder = (argsFlags & EnBuildApkCmdArgs.IsAutoCopyToPulishFolder) > 0;
            _isSkipI18N = (argsFlags & EnBuildApkCmdArgs.IsSkipI18N) > 0;
            return string.Empty;
        }

        static public void SaveAppVersion()
        {
            string verPath = GenVersionPath + "/version.txt";

            string content = "appVersion = " + PlayerSettings.bundleVersion+"\r\n";

             content += "bundleVersionCode = " + PlayerSettings.Android.bundleVersionCode + "\r\n";

            content += "resVersion = " + _versionStr + "\r\n";

            File.WriteAllText(verPath, content);
          
        }



        static public void CheckRedundantRes(string SelectABOutputABFolderPath,Dictionary<string,string>  path2AB)
        {
            List<string> listDebugInfo = new List<string>();

            //按AB索引生成文件索引
            string abName = null;
            HashSet<string> hashPaths = null;
            Dictionary<string, HashSet<string>> dicAB2Res = new Dictionary<string, HashSet<string>>();
            foreach(var path in path2AB.Keys)
            {
                abName = path2AB[path];
                if(dicAB2Res.TryGetValue(abName, out hashPaths)==false)
                {
                    hashPaths = new HashSet<string>();
                    dicAB2Res.Add(abName, hashPaths);
                }
                hashPaths.Add(path.ToLower().Replace("/", "_"));
            }

            //获取所有AB
            string[] ABPaths = Directory.GetFiles(SelectABOutputABFolderPath, "*.bin", SearchOption.AllDirectories);
            int nLen = ABPaths.Length;
            for(int i=0;i<nLen;++i)
            {
                ABPaths[i] = ABPaths[i].Replace("\\", "/");

                try
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(ABPaths[i]);
                    if (null != ab)
                    {
                        string bunldeName = ABPaths[i].Substring(ABPaths[i].LastIndexOf("/") + 1);

                        if (dicAB2Res.TryGetValue(bunldeName, out hashPaths) == false)
                        {
                            ab.Unload(true);
                            Debug.LogError("没有找到索引 ABPath=" + ABPaths[i]);
                            continue;
                        }

                        string[] names = ab.GetAllAssetNames();
                        UnityEngine.Object[] objs = ab.LoadAllAssets();

                        if (names.Length != objs.Length)
                        {
                            Debug.LogWarning("资源个数不匹配 bunldeName = " + bunldeName);
                            foreach (var obj in objs)
                            {
                                bool bFind = false;
                                foreach (var name in names)
                                {
                                    if(name.IndexOf(obj.name)>=0)
                                    {
                                        bFind = false;
                                        break;
                                    }
                                        
                                }

                                if(bFind==false)
                                {
                                    string item = "冗余资源 bunldeName= " + bunldeName + "-> res assetName= " + obj.name;
                                    Debug.LogWarning(item);
                                }
                            }

                        }

                        foreach (var name in names)
                        {
                            string assetName = name.ToLower().Replace("/", "_");

                            bool bFind = false;
                            foreach (var path in hashPaths)
                            {
                                if (path.IndexOf(assetName) >= 0)
                                {
                                    bFind = true;
                                    break;
                                }
                            }

                            //没有找到资源的，打印一下冗余资源
                            if (bFind == false)
                            {
                                string item = "冗余资源 bunldeName= " + bunldeName + "-> res assetName= " + assetName;
                                listDebugInfo.Add(item);
                                Debug.LogError(item);
                            }
                        }

                        ab.Unload(true);
                    }

                }
                catch(Exception e)
                {
                    Debug.LogError("加载异常 path = "+ ABPaths[i]);
                }

            }

            File.WriteAllLines(GenVersionPath + "/" + RedundantResFileName, listDebugInfo);
        }




    }
}
