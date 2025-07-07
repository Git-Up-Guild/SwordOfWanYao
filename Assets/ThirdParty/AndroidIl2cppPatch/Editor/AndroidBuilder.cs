//using Google.Android.AppBundle.Editor.Internal;
//using Google.Android.AppBundle.Editor.Internal.AssetPacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using XGame;
using XGame.Update;

public class AndroidBuilder : MonoBehaviour
{

    //-----------------------------------------------------------------------------------
    public static readonly string PROJECT_DIR = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
    public static readonly string ANDROID_EXPORT_PATH = PROJECT_DIR + "/AndroidGradleProject_v1.0";
    public static readonly string IOS_EXPORT_PATH = PROJECT_DIR + "/IOSProject_v1.0";

    public static string ANDROID_PROJECT_PATH { get { return ANDROID_EXPORT_PATH; } }
    public static string ANDROID_MANIFEST_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/";
    public static string JAVA_SRC_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/java/";
    public static string JAR_LIB_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/libs/";
    public static string SO_DIR_NAME = "jniLibs";
    public static string SO_LIB_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/jniLibs/";
    public static string EXPORTED_ASSETS_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/assets";
    public static string R_JAVA_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/gen/";
    public static string LAUNCHER_RES_PATH = ANDROID_PROJECT_PATH + "/launcher/src/main/res";
    public static string LAUNCHER_MANIFEST_XML_PATH = ANDROID_PROJECT_PATH + "/launcher/src/main/AndroidManifest.xml";
    public static string RES_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/res";
    public static string MANIFEST_XML_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/AndroidManifest.xml";
    public static string JAVA_OBJ_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/objs/";
    public static string BUILD_SCRIPTS_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/";
    public static string ZIP_PATH = PROJECT_DIR + "/Assets/AndroidIl2cppPatch/Editor/Exe/zip.exe";

    DingTalkHelper dingtalkhelper;

    static bool Exec(string filename, string args)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = filename;
        process.StartInfo.Arguments = args;

        int exit_code = -1;

        try
        {
            process.Start();
            if (process.StartInfo.RedirectStandardOutput && process.StartInfo.RedirectStandardError)
            {
                process.BeginOutputReadLine();
                Debug.LogError(process.StandardError.ReadToEnd());
            }
            else if (process.StartInfo.RedirectStandardOutput)
            {
                string data = process.StandardOutput.ReadToEnd();
                Debug.Log(data);
            }
            else if (process.StartInfo.RedirectStandardError)
            {
                string data = process.StandardError.ReadToEnd();
                Debug.LogError(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
        process.WaitForExit();
        exit_code = process.ExitCode;
        process.Close();
        return exit_code == 0;
    }

    public static bool ValidateConfig()
    {
        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot", "");
        if (string.IsNullOrEmpty(sdkPath))
        {
            Debug.LogError("sdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        string jdkPath = EditorPrefs.GetString("JdkPath", "");
        if (string.IsNullOrEmpty(jdkPath))
        {
            Debug.LogError("jdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        /*
        string ndkPath = EditorPrefs.GetString("AndroidNdkRootR16b", "");    //
		if (string.IsNullOrEmpty(ndkPath))
        {
            ndkPath = EditorPrefs.GetString("AndroidNdkRoot", "");
            if (string.IsNullOrEmpty(ndkPath))
            {
                Debug.LogError("ndk path is empty! please config via menu path:Edit/Preference->External tools.");
                return false;
            }
        }
		*/

        Debug.Log("Build Env is ready!");
        Debug.Log("Build Options:");
        Debug.Log("SDK PATH=" + sdkPath);
        Debug.Log("JDK PATH=" + jdkPath);
        return true;
    }



    //配置工程设置
    static private void InitProjectSetting(string appVer = null, string bundleCodeVer = null, bool sab = false)
    {
        string keystoreDir = PROJECT_DIR + "AndroidKeystore";

        if (!Directory.Exists(keystoreDir)) { Directory.CreateDirectory(keystoreDir); }

        /*
        PlayerSettings.applicationIdentifier = "ImmortalFamily.q1.com";
        PlayerSettings.companyName = "Q1";
        PlayerSettings.productName = "ImmortalFamily";
        */

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android )
        {
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
            PlayerSettings.Android.useAPKExpansionFiles = false;
            EditorUserBuildSettings.buildAppBundle = sab;

            if (string.IsNullOrEmpty(bundleCodeVer) == false)
            {
                PlayerSettings.Android.bundleVersionCode = int.Parse(bundleCodeVer);
            }
        }

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; //AndroidArchitecture.ARMv7 | 

        if (string.IsNullOrEmpty(appVer) == false)
        {
            PlayerSettings.bundleVersion = appVer;
        }

        PlayerSettings.Android.keystoreName = keystoreDir + "/imm.keystore";
        PlayerSettings.Android.keystorePass = "22222";
        PlayerSettings.Android.keyaliasName = PlayerSettings.applicationIdentifier;
        PlayerSettings.Android.keyaliasPass = "34444";

        //PlayerSettings.Android.bundleVersionCode = (PlayerSettings.Android.bundleVersionCode + 1) % 100000;
        //PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();

        //删除res目录
        string resPath = Application.dataPath + "/Plugins/Android/res";
        if (Directory.Exists(resPath))
        {
            Directory.Delete(resPath);
        }

    }

    //生成完整的apk，跳过热更新
    static public void BuildNoCodeHotUpdateAPK(string resVer, string appVer, string bundleCodeVer, string outDir = null, bool sab = false, bool development = false)
    {
        InitProjectSetting(appVer, bundleCodeVer, sab);


        string targetDir = outDir + "/publish";

        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        string apkPath = targetDir + "/" + PlayerSettings.applicationIdentifier + ".apk";

        BuildOptions options = BuildOptions.None;
        if (EditorUserBuildSettings.development || development)
        {
            options = BuildOptions.Development;
        }

        string projectPath = ANDROID_EXPORT_PATH;
        string error_msg = string.Empty;
        if (Directory.Exists(projectPath)) { FileUtil.DeleteFileOrDirectory(projectPath); }
        Directory.CreateDirectory(projectPath);
        try
        {

            if (sab)
            {

                int bundleCode = PlayerSettings.Android.bundleVersionCode;

                string version = appVer;

                if (string.IsNullOrEmpty(version))
                {
                    version = PlayerSettings.bundleVersion;
                }
                version += "." + resVer;

                /*
                //启用分包模式
                AssetDeliveryConfig assetDeliveryConfig = AssetDeliveryConfigSerializer.LoadConfig();
                assetDeliveryConfig.SplitBaseModuleAssets = true;
                AssetDeliveryConfigSerializer.SaveConfig(assetDeliveryConfig);

                apkPath = targetDir + "/" + PlayerSettings.applicationIdentifier +"."+ bundleCode + "."+ version + ".aab";
                AppBundlePublisher.Build(apkPath);
                */
            }
            else
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
                {

                    string exeDir = targetDir + "/exe";
                    if (Directory.Exists(exeDir))
                    {
                        Directory.Delete(exeDir, true);
                    }

                    Directory.CreateDirectory(exeDir);
                    apkPath = exeDir + "/" + PlayerSettings.applicationIdentifier + ".exe";
                }
                else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
                {
                    string exeDir = targetDir + "/WebGL";
                    if (Directory.Exists(exeDir))
                    {
                        Directory.Delete(exeDir, true);
                    }

                    Directory.CreateDirectory(exeDir);
                    apkPath = exeDir ;
                }

                error_msg = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, apkPath, EditorUserBuildSettings.activeBuildTarget, options).summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? string.Empty : "Failed to export project!";

                if (string.IsNullOrEmpty(error_msg) == false)
                    Debug.LogError("error_msg=" + error_msg);


                //删除临时文件夹
                string burstDebugInformationDir = targetDir + "/" + PlayerSettings.productName + "_BurstDebugInformation_DoNotShip";
                if(Directory.Exists(burstDebugInformationDir))
                {
                    Directory.Delete(burstDebugInformationDir,true);
                }
                
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }

        //CopyTargetFiles(outDir,false,sab);

        //保存符号表
        PrepareUpload_Symbol(outDir, resVer);

    }

    [MenuItem("AndroidBuilder/Step 1: Export Gradle Project", false, 109)]
    public static bool ExportGradleProject(string appVer, string bundleCodeVer, bool sab = false, string outDir = null)
    {
        //build settings
        if (!ValidateConfig()) { return false; }





        BuildOptions options = BuildOptions.AcceptExternalModificationsToPlayer;
        string projectPath = ANDROID_EXPORT_PATH;
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            options = BuildOptions.None;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            projectPath = IOS_EXPORT_PATH;
        }
        else
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        }

        InitProjectSetting(appVer, bundleCodeVer, sab);

        //export project
        string error_msg = string.Empty;
        string[] levels = new string[] { "Assets/G_Artist/Scene/Update.unity" };//, "Assets/G_Resources/Game/Scene/Login.unity" };//, "Assets/AndroidIl2cppPatch/Scene/0.unity" };

        if (Directory.Exists(projectPath)) { FileUtil.DeleteFileOrDirectory(projectPath); }
        Directory.CreateDirectory(projectPath);
        try
        {
            /*
            BuildPlayerOptions bp = new BuildPlayerOptions();
            bp.locationPathName = projectPath;
            bp.scenes = levels;
            bp.targetGroup = BuildTargetGroup.iOS;
            bp.target = BuildTarget.iOS;
            bp.options = BuildOptions.None;
            BuildReport rp = BuildPipeline.BuildPlayer(bp);
            */

            error_msg = BuildPipeline.BuildPlayer(levels, projectPath, EditorUserBuildSettings.activeBuildTarget, options).summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? string.Empty : "Failed to export project!";


        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }

        if (!string.IsNullOrEmpty(error_msg))
        {
            Debug.LogError(error_msg);
            return false;
        }


        //保存符号表
        PrepareUpload_Symbol(outDir, appVer);

        //copy the prebuild patch to the assets directory instead of downloading.
        // FileUtil.CopyFileOrDirectory(PROJECT_DIR + "/Assets/AndroidIl2cppPatch/PrebuiltPatches/AllAndroidPatchFiles_Version1.zip", EXPORTED_ASSETS_PATH + "/AllAndroidPatchFiles_Version1.zip");
        // FileUtil.CopyFileOrDirectory(PROJECT_DIR + "/Assets//AndroidIl2cppPatch/PrebuiltPatches/AllAndroidPatchFiles_Version2.zip", EXPORTED_ASSETS_PATH + "/AllAndroidPatchFiles_Version2.zip");
        return true;
    }

    [MenuItem("AndroidBuilder/Step 2: Patch Gradle Project", false, 102)]
    public static bool PatchAndroidProject()
    {

        //1. patch java file
        string[] javaEntranceFiles = Directory.GetFiles(JAVA_SRC_PATH, "UnityPlayerActivity.java", SearchOption.AllDirectories);
        if (javaEntranceFiles.Length != 1)
        {
            Debug.LogError("UnityPlayerActivity.java not found or more than one.");
            return false;
        }
        string javaEntranceFile = javaEntranceFiles[0];
        string allJavaText = File.ReadAllText(javaEntranceFile);
        if (allJavaText.IndexOf("noodle1983") > 0)
        {
            Debug.Log("UnityPlayerActivity.java already patched.");
            return true;
        }
        allJavaText = allJavaText.Replace("import android.view.WindowManager;",
            @"import android.view.WindowManager;
import io.github.noodle1983.Boostrap;");


        allJavaText = allJavaText.Replace("mUnityPlayer = new UnityPlayer(this, this);",
            @"Boostrap.InitNativeLibBeforeUnityPlay(getApplication().getApplicationContext().getFilesDir().getPath(),getApplication().getApplicationContext().getExternalFilesDir(null).getPath());
        mUnityPlayer = new UnityPlayer(this, this);");
        File.WriteAllText(javaEntranceFile, allJavaText);
        return true;
    }


    [MenuItem("AndroidBuilder/Step 3: Pick Hot SO", false, 103)]
    public static bool PickHotSO()
    {

        string srcPath;
        string targetPath;
        string targetStreamPath;


        string baseAPKPath = EXPORTED_ASSETS_PATH + "/" + UpdateConfig.DATA_DIR;

        //仅仅是做测试时候拷贝一份
        string baseStreamPath = Application.streamingAssetsPath + "/" + UpdateConfig.DATA_DIR;


        /*
        if (Directory.Exists(baseStreamPath))
        {
            Directory.Delete(baseStreamPath, true);
        }
        */

        string[] s_soHotList = UpdateSetup.s_soHotList;
        int nCount = s_soHotList.Length;
        for (int i = 0; i < nCount; ++i)
        {
            srcPath = BUILD_SCRIPTS_PATH + s_soHotList[i];

            if (File.Exists(srcPath) == false)
            {
                Debug.LogError("不存在:" + srcPath);
                continue;
            }

            targetPath = baseAPKPath + "/" + s_soHotList[i];
            targetStreamPath = baseStreamPath + "/" + s_soHotList[i];


            string dirPath = targetPath.Substring(0, targetPath.LastIndexOf("/"));

            // 判断下目标目录是否存在，不存在就创建
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            dirPath = targetStreamPath.Substring(0, targetStreamPath.LastIndexOf("/"));

            // 判断下目标目录是否存在，不存在就创建
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            File.Copy(srcPath, targetPath);
            File.Copy(srcPath, targetStreamPath);
            File.Delete(srcPath);

        }



        return true;
    }

    [MenuItem("AndroidBuilder/Step 4: Generate Build Scripts", false, 104)]
    public static bool GenerateBuildScripts()
    {
        string jdkPath = EditorPrefs.GetString("JdkPath", "");

        //jdkPath = "F://2019.3.2//UnitySetup64(2019.3.2f1)//Editor//Data//PlaybackEngines//AndroidPlayer";

        if (string.IsNullOrEmpty(jdkPath))
        {
            Debug.LogError("jdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        //must use the jdk in Unity
        //string gradlePath = jdkPath + "/../Tools/Gradle";

        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        string Editordir = Path.GetDirectoryName(exePath);


        string gradlePath = Editordir + "/Data/PlaybackEngines/AndroidPlayer/Tools/gradle";

        string[] gradleMainJarFiles = Directory.GetFiles(gradlePath + "/lib", "gradle-launcher*.jar", SearchOption.TopDirectoryOnly);


        //gradleMainJarFiles[0] = 

        if (gradleMainJarFiles.Length == 0)
        {
            Debug.LogError("gradle-launcher jar file not found in " + gradlePath + "/lib");
            return false;
        }
        string gradleMainJarFile = gradleMainJarFiles[0];



        //sign
        string keystoreDir = PROJECT_DIR + "/AndroidKeystore";
        if (!Directory.Exists(keystoreDir)) { Directory.CreateDirectory(keystoreDir); }
        string keystoreFile = keystoreDir + "/imm.keystore";
        if (!File.Exists(keystoreFile))
        {
            string keytoolPath = jdkPath + "/bin/keytool.exe";
            string genKeyParam = "-genkey -alias civre -validity 1000 -keyalg RSA -keystore " + keystoreFile + " -dname \"CN = Test, OU = Test, O = Test, L = Test, S = Test, C = Test\" -keysize 4096 -storepass civre -keypass civre";
            if (!Exec(keytoolPath, genKeyParam))
            {
                Debug.LogError("exec failed:" + keytoolPath + " " + genKeyParam);
                return false;
            }
        }

        StringBuilder allCmd = new StringBuilder();
        allCmd.AppendFormat("cd \"{0}\"\n\n", ANDROID_EXPORT_PATH);
        allCmd.AppendFormat("call \"{0}\" "
            + " -classpath \"{1}\" org.gradle.launcher.GradleMain \"-Dorg.gradle.jvmargs=-Xmx4096m\" \"assembleRelease\""
            + " -Pandroid.injected.signing.store.file=\"{2}\""
            + " -Pandroid.injected.signing.store.password=au6Dsxd3#%! "
            + " -Pandroid.injected.signing.key.alias=civre "
            + " -Pandroid.injected.signing.key.password=aSo9Qd2%$&"
            + " \n\n",
            jdkPath + "/bin/java.exe",
            gradleMainJarFile,
            keystoreFile);

        allCmd.AppendFormat("copy /Y \"{0}\\launcher\\build\\outputs\\apk\\release\\launcher-release.apk\"  \"{0}\\{1}.apk\" \n\n",
            ANDROID_EXPORT_PATH.Replace("//", "/").Replace("/", "\\"),
            Application.identifier);

        //allCmd.AppendFormat("explorer.exe {0} \n\n", ANDROID_EXPORT_PATH.Replace("//", "/").Replace("/", "\\"));
        allCmd.AppendFormat("@echo on\n\n"); //explorer as the last line wont return success, so...
        File.WriteAllText(ANDROID_EXPORT_PATH + "/build_apk.bat", allCmd.ToString());


        //导出工程后，改变grade

        // 修改unityLibrary/build.gradle
        /*
        string path = ANDROID_EXPORT_PATH + "/unityLibrary/build.gradle";
        string str = File.ReadAllText(path);
        // BuildIl2CppTask任务模板文件
        string path2 = Application.dataPath + "/AndroidIl2cppPatch/Editor/il2cpp.gradle";
        string task = File.ReadAllText(path2);
        str += task;
        File.WriteAllText(path, str);
        */

        return true;
    }


    [MenuItem("AndroidBuilder/Step 5: Build Apk File", false, 105)]
    public static bool BuildApk()
    {
        string buildApkPath = ANDROID_EXPORT_PATH + "/build_apk.bat";
        string alignedApkName = Application.identifier + ".apk";
        string alignedApkPath = ANDROID_EXPORT_PATH + "/" + alignedApkName;

        if (!Exec(buildApkPath, ""))
        {
            Debug.LogError("exec failed:" + buildApkPath);
            return false;
        }

        if (!File.Exists(alignedApkPath))
        {
            Debug.LogError("apk not found:" + alignedApkPath + ", exec failed:" + buildApkPath);
            return false;
        }
        return true;
    }

    [MenuItem("AndroidBuilder/Run Step 1-5", false, 1)]
    public static void BuildAll(string appVer, string bundleCodeVer, string outDir = null, bool bsa = false)
    {

        //Step 1
        if (!ExportGradleProject(appVer, bundleCodeVer, bsa))
        {
            Debug.LogError("failed to ExportGradleProject");
            return;
        }

        //Step 2
        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        //Step 3
        if (!PickHotSO())
        {
            Debug.LogError("failed to GenerateBinPatches");
            return;
        }


        //Step 4
        if (!GenerateBuildScripts())
        {
            Debug.LogError("failed to GenerateBuildScripts");
            return;
        }

        //Step 5
        if (!BuildApk())
        {
            Debug.LogError("failed to BuildApk");
            return;
        }

        //step 6
        if (!CopyTargetFiles(outDir))
        {
            Debug.LogError("failed to CopyTargetFile");
            return;
        }

        Debug.Log("Done!");
    }

    [MenuItem("AndroidBuilder/Run Step 1, 2, 4, 5 for base version", false, 2)]
    public static void BuildWithoutPatch()
    {
        //Step 1
        if (!ExportGradleProject(null, null))
        {
            Debug.LogError("failed to ExportGradleProject");
            return;
        }

        //Step 2
        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        //Step 4
        if (!GenerateBuildScripts())
        {
            Debug.LogError("failed to GenerateBuildScripts");
            return;
        }

        //Step 5
        if (!BuildApk())
        {
            Debug.LogError("failed to BuildApk");
            return;
        }
        Debug.Log("Done!");
    }

    [MenuItem("AndroidBuilder/Run Step 1-4 for Patch Version", false, 3)]
    public static void BuildPatch()
    {
        //Step 1
        if (!ExportGradleProject(null, null))
        {
            Debug.LogError("failed to ExportGradleProject");
            return;
        }

        //Step 2
        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        //Step 3
        if (!PickHotSO())
        {
            Debug.LogError("failed to GenerateBinPatches");
            return;
        }

        //Step 4
        if (!GenerateBuildScripts())
        {
            Debug.LogError("failed to GenerateBuildScripts");
            return;
        }

        Debug.Log("Done!");
    }

    //拷贝二进制文件
    public static void GenerateCodeBinPatches(string outDir)
    {
        if (null == outDir)
        {
            return;
        }

        string bin_data_path = "/bin/Data";


        string assetBinDataPath = EXPORTED_ASSETS_PATH + bin_data_path;
        string soBinDataPath = BUILD_SCRIPTS_PATH + "/" + SO_DIR_NAME;
        string targetFolder = outDir + "/android/hotbin";

        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        Directory.CreateDirectory(targetFolder);

        //拷贝 asset 资源
        string targetAsset = targetFolder + "/assets/bin/Data";
        Directory.CreateDirectory(targetFolder + "/assets/bin");
        FileUtil.CopyFileOrDirectory(assetBinDataPath, targetAsset);

        //拷贝 so 
        string targetso = targetFolder + "/" + SO_DIR_NAME;
        //Directory.CreateDirectory(targetso);
        FileUtil.CopyFileOrDirectory(soBinDataPath, targetso);

        //拷贝热更的so目录
        string hotsoDir = EXPORTED_ASSETS_PATH + "/" + UpdateConfig.DATA_DIR + "/" + SO_DIR_NAME;

        OverrideCopyDirectory(hotsoDir, targetso);
        //FileUtil.CopyFileOrDirectory(hotsoDir, targetso);

        //测试version

        //File.WriteAllText(targetFolder + "/version.txt" , "101201"+DateTime.Now);
    }

    public static bool CopyTargetFiles(string outDir, bool bSuportCSHotCode = true, bool sab = false)
    {
        if (null == outDir)
        {
            return true;
        }

        string suffix = ".apk";
        if (sab)
        {
            suffix = ".aab";
        }

        //copy apk
        string apkPath = ANDROID_EXPORT_PATH + "/" + PlayerSettings.applicationIdentifier + suffix;
        if (File.Exists(apkPath) == false)
        {
            return false;
        }
        string targetDir = outDir + "/publish";
        if (Directory.Exists(targetDir) == false)
        {
            Directory.CreateDirectory(targetDir);
        }
        string targetapk = targetDir + "/" + PlayerSettings.applicationIdentifier + suffix;

        if (File.Exists(targetapk))
        {
            File.Delete(targetapk);
        }

        FileUtil.CopyFileOrDirectory(apkPath, targetapk);


        //copy obb
        bool bIsSAB = false;
        string obbDir = ANDROID_EXPORT_PATH;// + "/unityLibrary";
        DirectoryInfo diInfo = new DirectoryInfo(obbDir);
        if (diInfo.Exists)
        {
            string appid = PlayerSettings.applicationIdentifier;
            int bundleCodeVersion = PlayerSettings.Android.bundleVersionCode;

            string dstName = null;
            FileInfo[] fileInfos = diInfo.GetFiles("*.obb", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
                var file = fileInfos[i];
                dstName = targetDir + "/" + "main." + bundleCodeVersion + "." + appid + ".obb";
                File.Copy(file.FullName, dstName);
                bIsSAB = true;
                Debug.Log("拷贝obb 到: " + dstName);
                break;
            }
        }

        if (false == bIsSAB && bSuportCSHotCode)
        {
            GenerateCodeBinPatches(outDir);
            /*
            //copy so res
            string soDir = outDir + "/il2cpp";
            if (Directory.Exists(soDir))
            {
                Directory.Delete(soDir, true);
            }
            Directory.CreateDirectory(soDir);


            string arm64v8aPath = PROJECT_DIR + "/AllAndroidPatchFiles_arm64-v8a.zip";
            if(File.Exists(arm64v8aPath))
            {
                FileUtil.CopyFileOrDirectory(arm64v8aPath, soDir + "/AllAndroidPatchFiles_arm64-v8a.zip");
            }

            string v7aaPath = PROJECT_DIR + "/AllAndroidPatchFiles_armeabi-v7a.zip";
            if (File.Exists(v7aaPath))
            {
                FileUtil.CopyFileOrDirectory(v7aaPath, soDir + "/AllAndroidPatchFiles_armeabi-v7a.zip");
            }

            */
        }
        return true;
    }

    public static void OverrideCopyDirectory(string srcPath, string destPath)
    {
        if (Directory.Exists(srcPath) == false)
        {
            return;
        }

        DirectoryInfo dir = new DirectoryInfo(srcPath);
        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
        foreach (FileSystemInfo i in fileinfo)
        {
            string curDstPath = destPath + "/" + i.Name;
            if (i is DirectoryInfo)
            {     //判断是否文件夹
                if (!Directory.Exists(curDstPath))
                {
                    Directory.CreateDirectory(curDstPath);   //目标目录下不存在此文件夹即创建子文件夹
                }
                OverrideCopyDirectory(i.FullName, curDstPath);    //递归调用复制子文件夹
            }
            else
            {
                File.Copy(i.FullName, curDstPath, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
            }
        }
    }


    //准备buggly的符号表
    public static void PrepareUpload_Symbol(string srcDir, string resVer)
    {
        if (string.IsNullOrEmpty(srcDir))
        {
            Debug.LogWarning("PrepareUpload_Symbol srcDir==null");
            return;
        }

        string dstSymbolDir = srcDir + "/symbol";

        //创建符号表目录
        if (Directory.Exists(dstSymbolDir))
        {
            Directory.Delete(dstSymbolDir, true);
        }

        if (Directory.Exists(dstSymbolDir) == false)
        {
            Directory.CreateDirectory(dstSymbolDir);
        }

        //拷贝buggly原始对象
        string bugglyPath = srcDir + "/../buglyqq-upload-symbol";
        OverrideCopyDirectory(bugglyPath, srcDir);

        //buggly 批处理路径
        string bugglyBatPath = srcDir + "/upload-symbolexe.bat";


        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            //写批处理
            string content = "java -jar buglyqq-upload-symbol.jar -appid 0cffa34cbd -appkey 5f8370a9-5e8c-43ef-9565-ae64cf86ca2f";// -bundleid 1 -version 1.0 -platform Android -inputSymbol symbol";
            content += " -bundleid " + PlayerSettings.applicationIdentifier;
            content += " -version " + PlayerSettings.Android.bundleVersionCode;
            content += " -platform IOS -inputSymbol symbol";
            content += "\n\r pause";

            File.WriteAllText(bugglyBatPath, content);
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            //拷贝符号表
            string srcSymbolDir = Application.dataPath + "/../Library/Bee/artifacts/Android/il2cppOutput/build";

            OverrideCopyDirectory(srcSymbolDir, dstSymbolDir);

            //写批处理
            string content = "java -jar buglyqq-upload-symbol.jar -appid 0412c8889c -appkey d8092085-19f1-4dba-8a6c-22f2a690f1fd";// -bundleid 1 -version 1.0 -platform Android -inputSymbol symbol";
            content += " -bundleid " + PlayerSettings.applicationIdentifier;
            content += " -version " + PlayerSettings.bundleVersion;
            content += " -platform Android -inputSymbol symbol";

            content += "\n\r pause";

            File.WriteAllText(bugglyBatPath, content);

        }
    }
}
