
using XGame.Asset.Manager;
using XGame.I18N;
using XGameEditor.AssetImportTool;
using XGameEditor.NewLoad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XGame;
using XGame.Asset.Load;
#if SUPPORT_I18N
using XGameEditor.I18N;
#endif
using XGameEditor;
using System.Text;
using static XGameEditor.NewGAssetDataCache;
using EnBuildApkCmdArgs = XGameEditor.BuildAPPWindow.EnBuildApkCmdArgs;

namespace XGameEditor
{
    public class CommandBuildBoot
    {
        static private Dictionary<string, string> _buildApkConfig;
       

        static public EnBuildApkCmdArgs BuildApkCmdArgsAllTrue = (EnBuildApkCmdArgs)(-1);
        static public EnBuildApkCmdArgs DefualtBuildApkCmdArg = BuildApkCmdArgsAllTrue;

        static private Dictionary<string, EnBuildApkCmdArgs> _argsNameDic = new Dictionary<string, EnBuildApkCmdArgs>()
        {

            { "-buildAB", EnBuildApkCmdArgs.IsBuildAB},
            { "-buildApk", EnBuildApkCmdArgs.IsBuildApk},
            { "-CompatibleType", EnBuildApkCmdArgs.IsCompatibleType},
            { "-IncrementABPack", EnBuildApkCmdArgs.IsIncrementABPack},
            { "-LuaLog", EnBuildApkCmdArgs.IsDiscardLuaLog},
            { "-SplitFix", EnBuildApkCmdArgs.IsSplitFix},
            { "-SplitWeb", EnBuildApkCmdArgs.IsSplitWeb},
            { "-FullPack", EnBuildApkCmdArgs.IsFullPack},
            { "-Obb", EnBuildApkCmdArgs.IsSurportObb},
            { "-CSCodeUpdate", EnBuildApkCmdArgs.IsNoCSCodeUpdate},
            { "-debug", EnBuildApkCmdArgs.IsDevelopment},
            { "-upload", EnBuildApkCmdArgs.IsUpLoad },
            { "-buglysymbol", EnBuildApkCmdArgs.IsBuglySymbol },
            { "-Xlua", EnBuildApkCmdArgs.IsXlua },
            { "-ResHotupdateVer", EnBuildApkCmdArgs.IsResHotupdateVer },
            { "-CheckCodeModified", EnBuildApkCmdArgs.IsCheckCodeModified},
            { "-AutoCopyToPulishFolder", EnBuildApkCmdArgs.IsAutoCopyToPulishFolder},
            { "-SkipI18N", EnBuildApkCmdArgs.IsSkipI18N },

        };

        //app的主版本号
        static public string _appVer = string.Empty;
        static public string _bundleCodeVer = string.Empty;

        static private string _versionStr = string.Empty;
        //检查批处理参数
        static public bool CheckArgsFromCmd(out string errInfo, out XGameEditor.BuildAPPWindow.EnBuildApkCmdArgs argsFlags)
        {
            //int param = Convert.ToInt32(p);

            //string[] arrParam = System.Environment.GetCommandLineArgs();
            //int index = 0;
            //foreach (var param in args)
            //{
            //    Debug.Log($"【BuildApkFromCmd 被调用,参数为{index++}:{param}】");
            //}

            errInfo = string.Empty;
            var args = System.Environment.GetCommandLineArgs();
            argsFlags = EnBuildApkCmdArgs.None;
            _versionStr = string.Empty;
            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                Debug.Log($"打包参数：{i}:{arg}");
                if (arg == "-AppVer")
                {
                    //DebugLog($"设置版本号111：{args[i + 1]}");
                    //int temInt;
                    if (i == arg.Length - 1)
                    {
                        errInfo = "无效版本号";
                        return false;
                    }


                   
                    _appVer = args[i + 1];
                    DebugLog($"设置版本号：{_appVer}");
                    ++i;
                    continue;
                }

                if (arg == "-BundleCodeVer")
                {
                    //DebugLog($"设置版本号111：{args[i + 1]}");
                    //int temInt;
                    if (i == arg.Length - 1)
                    {
                        errInfo = "无效版本号";
                        return false;
                    }



                    _bundleCodeVer = args[i + 1];
                    DebugLog($"代码版本号：{_bundleCodeVer}");
                    ++i;
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
        static public string BuildConfigPath { get => $"{Application.dataPath}/../BuildAPP"; }
        static public string BuildStateTextPath { get => $"{BuildConfigPath}/BuildState.txt"; }
        //检查是否包含参数
        static public bool IsContainsArg(EnBuildApkCmdArgs argsFlags, EnBuildApkCmdArgs targetArg)
        {
            return (argsFlags & targetArg) != 0;
        }
        static public void BuildApkFromCmd()
        {
            string errInfo = string.Empty;
            try
            {
                StartReceivedErrorLog();
                File.WriteAllText(BuildStateTextPath, "0\n打包中");
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
            }
        }


        //根据参数的打包
        static public string BuildApkFromCmd(EnBuildApkCmdArgs argsFlags)
        {
            string errInfo;

            //int param = Convert.ToInt32(p);
            //1.加载配置
            //if (!LoadBuildApkConfig(out errInfo))
            //    return Debug.Log(errInfo, 3);

            //string[] arrParam = System.Environment.GetCommandLineArgs();
            //int index = 0;
            //foreach (var param in arrParam)
            //{
            //    Debug.Log($"【BuildApkFromCmd 被调用,参数为{index++}:{param}】");
            //}


            //打APK(什么都不用勾选，默认选项打包)
            bool isBuildAPK = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsBuildApk);
            Debug.Log($"【是否打APK：{isBuildAPK}】");
            string finalApkPath = string.Empty;
            if (isBuildAPK)
            {
                Debug.Log($"【正在打APK：{isBuildAPK}】");
                BuildAPPWindow.SynBuildAPKParam(argsFlags);
                BuildAPPWindow.BuildPublish();
                bool isUpload =  IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsUpLoad);
                bool isBuglySymbol = IsContainsArg(argsFlags, EnBuildApkCmdArgs.IsBuglySymbol);
              
                if (isBuglySymbol)
                {
                    FTPNetwork.FTP.cpyfiles();
                }
            }
            return string.Empty;
            //

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
        static public string GenABBaseFolder = "";//{ get => $"{Application.dataPath}/../BuildAPP";}
        static public string GenVersionPath { get => $"{GenABBaseFolder}/pkg_{_versionStr}"; }
        static public string GenVersionLogPath { get => $"{GenVersionPath}/log_{_versionStr}.txt"; }
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
    }
}