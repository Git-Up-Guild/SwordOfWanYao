using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Object = UnityEngine.Object;

namespace XGameEditor
{
    public class AssetbundlesMenuItems
    {
        //private const string kSimulateAssetBundlesMenu = "AssetBundles/Simulate AssetBundles";
        //private const string kSimulateAssetBundleUpdateMenu = "AssetBundles/Simulate AssetBundles Update";
        //private const string uiTextureBundlePrefix = "ui/spritepacking/";

        //protected static Regex ms_RemoveCommentRegex = new Regex(@"--\[\[(\s|.)*?\]\]");
        //protected static Regex ms_CommentLogRegex = new Regex(@"(\n[\t ]*?)(print|log.Log|log.LogFormat|log.Warning|log.WarningFormat)( *?)((?<open>\()[^\(\)]*(?<-open>\)))");

        //      [MenuItem(kSimulateAssetBundlesMenu)]
        //public static void ToggleSimulateAssetBundle()
        //{
        //          // AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
        //          PropertyInfo property = typeof(AssetBundleManager).GetProperty("SimulateAssetBundleInEditor",
        //                                                                  BindingFlags.Public | BindingFlags.Static);
        //          if (property != null)
        //          {
        //              bool simulate = (bool)property.GetValue(null, null);
        //              property.SetValue(null, !simulate, null);
        //          }
        //      }

        //[MenuItem(kSimulateAssetBundlesMenu, true)]
        //public static bool ToggleSimulateAssetBundleValidate()
        //{
        //	// Menu.SetChecked(kSimulateAssetBundlesMenu, AssetBundleManager.SimulateAssetBundleInEditor);
        //	PropertyInfo property = typeof(AssetBundleManager).GetProperty("SimulateAssetBundleInEditor",
        //															BindingFlags.Public | BindingFlags.Static);
        //	if (property != null)
        //	{
        //		bool simulate = (bool)property.GetValue(null, null);

        //		Menu.SetChecked(kSimulateAssetBundlesMenu, simulate);
        //	}
        //	return true;
        //}

        //[MenuItem("AssetBundles/Build AssetBundles")]
        //static public void BuildAssetBundles()
        //{
        //	BuildScript.BuildAssetBundles();
        //}

        //[MenuItem("AssetBundles/Build Player")]
        //static void BuildPlayer()
        //{
        //	BuildScript.BuildPlayer();
        //}


        //      [MenuItem("AssetBundles/Decrypt Asset Bundles")]
        //      static void DecryptAssetBundles()
        //      {
        //          BuildScript.DecryptAssetBundles();
        //      }

        //[MenuItem("Assets/AssetBundle/Copy AssetBundle Path")]
        //private static void CopyAssetBundles()
        //{
        //    Object[] objs = Selection.objects;
        //    if (objs != null && objs.Length > 0)
        //    {
        //        var assetPath = AssetDatabase.GetAssetPath(objs[0]);
        //        var assetImporter = AssetImporter.GetAtPath(assetPath);
        //        if (assetImporter != null)
        //        {
        //            GUIUtility.systemCopyBuffer = assetImporter.assetBundleName;
        //        }
        //    }
        //}
        //public static void SetAssetBundleByPath(string assetFilePath)
        //{
        //    var assetImporter = AssetImporter.GetAtPath(assetFilePath);

        //    string assetBundleName = assetFilePath.Substring(assetFilePath.IndexOf("/") + 1);
        //    assetImporter.assetBundleName = assetBundleName;
        //    AssetDatabase.SaveAssets();
        //}

        //[MenuItem("Assets/AssetBundle/Set AssetBundle By Path")]
        //public static void SetAssetBundleByPath()
        //{
        //    try
        //    {
        //        foreach (var obj in Selection.objects)
        //        {
        //            var assetPath = AssetDatabase.GetAssetPath(obj);
        //            var assetImporter = AssetImporter.GetAtPath(assetPath);

        //            string assetBundleName = assetPath.Substring(assetPath.IndexOf("/") + 1);
        //            assetImporter.assetBundleName = assetBundleName;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Error: " + e.Message);
        //    }
        //    finally
        //    {
        //        EditorUtility.ClearProgressBar();
        //        AssetDatabase.SaveAssets();
        //        AssetDatabase.Refresh();
        //    }
        //}

        //private static bool isCancelSetAB = false;
        //[MenuItem("Assets/AssetBundle/Set AssetBundle Folder By Path")]
        //public static void SetAssetBundleFolderByPath()
        //{
        //    isCancelSetAB = false;
        //    foreach (var obj in Selection.objects)
        //    {
        //        var assetPath = AssetDatabase.GetAssetPath(obj);
        //        SetABPath(assetPath);
        //    }
        //    EditorUtility.ClearProgressBar();
        //}
        //private static void SetABPath(string folder)
        //{
        //    if (isCancelSetAB || !Directory.Exists(folder))
        //    {
        //        EditorUtility.ClearProgressBar();
        //        return;
        //    }
        //    DirectoryInfo dirInfo = new DirectoryInfo(folder);
        //    var allFile = dirInfo.GetFiles("", SearchOption.AllDirectories);
        //    foreach (FileInfo file in dirInfo.GetFiles())
        //    {
        //        var assetPath = file.FullName;
        //        var fromAsset = assetPath.Substring(assetPath.IndexOf("Assets\\"));
        //        var assetImporter = AssetImporter.GetAtPath(fromAsset);
        //        string assetBundleName = fromAsset.Substring(fromAsset.IndexOf("\\") + 1);
        //        if (assetImporter == null)
        //            continue;

        //        assetImporter.assetBundleName = assetBundleName;
        //        isCancelSetAB = EditorUtility.DisplayCancelableProgressBar("设置AB", $"{assetBundleName}", 0f);
        //        //Debug.Log($"设置AB路径:  {fromAsset}  {assetBundleName}");
        //        if (isCancelSetAB)
        //        {
        //            EditorUtility.ClearProgressBar();
        //            return;
        //        }
        //    }
        //    //foreach (DirectoryInfo dir in dirInfo.GetDirectories())
        //    //{
        //    //    SetABPath(dir.FullName);
        //    //}
        //}

        //[MenuItem("Assets/AssetBundle/Set AssetBundle Folder By Path(直接改meta)")]
        //public static void SetAssetBundleMetaFolderByPath()
        //{
        //    isCancelSetAB = false;
        //    try
        //    {
        //        foreach (var obj in Selection.objects)
        //        {
        //            var assetPath = AssetDatabase.GetAssetPath(obj);
        //            SetABPathByMeta(assetPath);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Error: " + e.Message);
        //    }
        //    finally
        //    {
        //        EditorUtility.ClearProgressBar();
        //        AssetDatabase.SaveAssets();
        //        AssetDatabase.Refresh();
        //    }
        //}

        //[MenuItem("Assets/AssetBundle/Set AssetBundle Folder By Path(直接改meta，跳过已有AB)")]
        //public static void SetAssetBundleMetaFolderByPathSkipExist()
        //{
        //    isCancelSetAB = false;
        //    try
        //    {
        //        foreach (var obj in Selection.objects)
        //        {
        //            var assetPath = AssetDatabase.GetAssetPath(obj);
        //            SetABPathByMeta(assetPath, null, true);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError("Error: " + e.Message);
        //    }
        //    finally
        //    {
        //        EditorUtility.ClearProgressBar();
        //        AssetDatabase.SaveAssets();
        //        AssetDatabase.Refresh();
        //    }
        //}

        //private static string _abNameKey = "assetBundleName:";
        //private static string _assetPathName = "assets/";
        //public static void SetABPathByMeta(string folder, string assetName = null, bool isSkipExist = false)
        //{
        //    if (isCancelSetAB || !Directory.Exists(folder))
        //    {
        //        EditorUtility.ClearProgressBar();
        //        return;
        //    }
        //    DirectoryInfo dirInfo = new DirectoryInfo(folder);
        //    var allFile = dirInfo.GetFiles("*.meta", SearchOption.AllDirectories);
        //    int count = allFile.Length;
        //    for (int i = 0; i < count; ++i)
        //    {
        //        var assetPath = allFile[i].FullName;
        //        assetPath = assetPath.Replace("\\", "/").ToLower();
        //        string name = "";
        //        if (assetName != null)
        //        {
        //            name = assetName;
        //        }
        //        else
        //        {
        //            name = assetPath.Substring(assetPath.IndexOf(_assetPathName) + _assetPathName.Length).Replace(".meta", "");
        //        }

        //        isCancelSetAB = EditorUtility.DisplayCancelableProgressBar("设置AB", $"{name}", (float)i / count);
        //        if (isCancelSetAB)
        //        {
        //            EditorUtility.ClearProgressBar();
        //            return;
        //        }

        //        if (File.ReadAllText(assetPath).Contains("folderAsset: yes"))   // 文件夹跳过
        //        {
        //            continue;
        //        }


        //        var allLine = File.ReadAllLines(assetPath);
        //        string curLine;
        //        bool isFind = false;
        //        for (int j = allLine.Length - 1; j >= 0; --j)
        //        {
        //            curLine = allLine[j];
        //            if (curLine.Contains(_abNameKey))
        //            {
        //                if (isSkipExist && curLine.Trim() != _abNameKey)
        //                {
        //                    //Debug.Log($"已存在AB名字 {_abNameKey}");
        //                    continue;
        //                }

        //                var endIndex = curLine.IndexOf(_abNameKey) + _abNameKey.Length;
        //                curLine = curLine.Substring(0, endIndex);
        //                allLine[j] = $"{curLine} {name}";
        //                isFind = true;
        //                break;
        //            }
        //        }
        //        if (isFind)
        //        {
        //            File.WriteAllLines(assetPath, allLine);
        //        }
        //    }
        //    EditorUtility.ClearProgressBar();
        //}

        //[MenuItem("Assets/AssetBundle/Set Lua AssetBundle Folder By Path(直接改meta)")]
        //public static void SetLuaAssetBundleMetaFolderByPath()
        //{
        //    isCancelSetAB = false;
        //    foreach (var obj in Selection.objects)
        //    {
        //        var assetPath = AssetDatabase.GetAssetPath(obj);
        //        SetABPathByMeta(assetPath, "luascript");
        //    }
        //    EditorUtility.ClearProgressBar();
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();
        //}

        //[MenuItem("Assets/AssetBundle/Set AssetBundle By Directory")]
        //public static void SetAssetBundleByDir()
        //{
        //    foreach (var obj in Selection.objects)
        //    {
        //        var assetPath = AssetDatabase.GetAssetPath(obj);
        //        var assetImporter = AssetImporter.GetAtPath(assetPath);

        //        var dirName = Path.GetDirectoryName(assetPath);
        //        string assetBundleName = dirName.Substring(dirName.IndexOf("/") + 1);
        //        assetImporter.assetBundleName = assetBundleName;
        //    }
        //}

        //[MenuItem("Assets/AssetBundle/Set AssetBundle By PackingTag")]
        //public static void SetAssetBundleByPackingTag()
        //{
        //    foreach (var obj in Selection.objects)
        //    {
        //        var assetPath = AssetDatabase.GetAssetPath(obj);
        //        var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        //        if (textureImporter != null)
        //        {
        //            if (!string.IsNullOrEmpty(textureImporter.spritePackingTag))
        //            {
        //                textureImporter.assetBundleName = uiTextureBundlePrefix + textureImporter.spritePackingTag;
        //            }
        //            else
        //            {
        //                string assetBundleName = assetPath.Substring(assetPath.IndexOf("/") + 1);
        //                textureImporter.assetBundleName = assetBundleName;
        //            }
        //        }
        //    }
        //}

        //[MenuItem("Assets/Shader/Replace Shader By Name")]
        //public static void ReplaceShaderByName()
        //{
        //    foreach (var obj in Selection.objects)
        //    {
        //        var mat = obj as Material;
        //        if (mat != null)
        //        {
        //            mat.shader = Shader.Find(mat.shader.name);
        //            EditorUtility.SetDirty(obj);
        //        }
        //    }
        //}

        //[MenuItem("Assets/Shader/Find Standard Shader In Use")]
        //public static void FindStandardShaderInUse()
        //{
        //    //Object[] selected = 
        //    List<Object> selects = new List<Object>();
        //    foreach (var obj in Selection.objects)
        //    {
        //        var mat = obj as Material;
        //        if (mat != null)
        //        {
        //            if (mat.shader.name.Contains("Standard"))
        //            {
        //                selects.Add(obj);
        //                //Debug.LogFormat("use standard material: {0}", AssetDatabase.GetAssetPath(mat));
        //            }
        //        }
        //    }

        //    Selection.objects = selects.ToArray();
        //}

        //private static readonly Dictionary<string, string> MobileShaderMap = new Dictionary<string, string>()
        //{
        //    /*{"Particles/Additive", "Mobile/Particles/Additive"},
        //    {"Particles/Alpha Blended", "Mobile/Particles/Alpha Blended"},
        //    {"Particles/Multiply", "Mobile/Particles/Multiply"},*/
        //    {"Legacy Shaders/Bumped Specular","War/Character/BumpedSpecular"},
        //    //{"Skybox/6 Sided","Mobile/Skybox" },
        //    {"Legacy Shaders/Diffuse", "War/Mobile/Diffuse"},
        //    {"Mobile/Diffuse", "War/Mobile/Diffuse" },
        //    {"Effects/FPS_Pack/AlphaBlended","Particles/Alpha Blended"},
        //    {"FORGE3D/Additive","Particles/Additive" }
        //};

        //[MenuItem("Assets/Shader/Replace Shader To Mobile")]
        //public static void ReplaceShaderToMobile()
        //{
        //    //Object[] selected = 
        //    List<Object> selects = new List<Object>();
        //    foreach (var obj in Selection.objects)
        //    {
        //        var mat = obj as Material;
        //        if (mat != null)
        //        {
        //            string mobileShaderName;
        //            MobileShaderMap.TryGetValue(mat.shader.name, out mobileShaderName);
        //            if (!string.IsNullOrEmpty(mobileShaderName))
        //            {
        //                mat.shader = Shader.Find(mobileShaderName);
        //                EditorUtility.SetDirty(obj);
        //                selects.Add(obj);
        //            }
        //        }
        //    }

        //    Selection.objects = selects.ToArray();
        //}

        //[MenuItem("Assets/Shader/Find Material Use Shader")]
        //public static void FindMaterialUseShader()
        //{
        //    List<Object> selects = new List<Object>();
        //    foreach (var obj in Selection.objects)
        //    {
        //        var mat = obj as Material;
        //        if (mat != null)
        //        {
        //            if (mat.shader.name == "Mobile/Particles/Alpha Blended")
        //            {
        //                selects.Add(obj);
        //            }
        //        }
        //    }

        //    Selection.objects = selects.ToArray();
        //}

        //[MenuItem("AssetBundles/Comment Lua Print")]
        //public static Dictionary<string, byte[]> CommentLuaPrint()
        //{
        //    Dictionary<string, byte[]> originLuaContents = new Dictionary<string, byte[]>();
        //    string[] luaFiles = AssetDatabase.GetAssetPathsFromAssetBundle("luascript");
        //    foreach (var luaFile in luaFiles)
        //    {
        //        var bytes = File.ReadAllBytes(luaFile);
        //        originLuaContents.Add(luaFile, bytes);

        //        string content = Encoding.UTF8.GetString(bytes);
        //        content = ms_RemoveCommentRegex.Replace(content, "");
        //        content = ms_CommentLogRegex.Replace(content, "");
        //        File.WriteAllBytes(luaFile, Encoding.UTF8.GetBytes(content));
        //    }

        //    return originLuaContents;
        //}

        ////[MenuItem("XGame/2020.3打包工具/打ab到StreamingAssets")]
        //        public static void BuildAB()
        //        {
        //            AssetDatabase.RemoveUnusedAssetBundleNames();

        //            string platformFolder = GetPlatformFolderForAssetBundles();
        //            string basePath = Application.dataPath + "/../";
        //            string baseDir = basePath + "/ab_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "/" + platformFolder + "/";

        //            // Choose the output path according to the build target.
        //            string outputPath = baseDir + "assetbundles/" + platformFolder;
        //            Debug.Log("outputPath = " + outputPath);
        //            if (!Directory.Exists(outputPath))
        //                Directory.CreateDirectory(outputPath);

        //            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        //            //清理多余manifest
        //            DelManifest(ref outputPath);
        //        }

        //        //(8)拷贝文件到streaming文件夹
        //        private static void CopyPackingRes(ref string dir, bool copyOrg = false)
        //        {
        //            string platformFolder = GetPlatformFolderForAssetBundles();
        //            string basePath = Application.dataPath + "/../";
        //            string baseDir = basePath + "/" + dir + "/" + platformFolder + "/";
        //            string appResDir = baseDir + "appasssetbundles";

        //            string destination = Application.streamingAssetsPath + "/" + "assetbundles";
        //            if (Directory.Exists(destination))
        //                FileUtil.DeleteFileOrDirectory(destination);

        //            //string settingFilePath = baseDir + buildsettingfile;

        //            if (copyOrg)
        //            {
        //                appResDir = baseDir + "assetbundles";
        //            }

        //            //存入设置到streaming
        //            //string appBuildsettingPath = appResDir + "/" + platformFolder + "/" + buildsettingfile;
        //            //if (File.Exists(appBuildsettingPath))
        //            //    File.Delete(appBuildsettingPath);
        //            //FileUtil.CopyFileOrDirectory(settingFilePath, appBuildsettingPath);
        //            FileUtil.CopyFileOrDirectory(appResDir, destination);
        //            //Debug.LogErrorFormat("位置：{0}  {1}  {2}  {3}  {4}  {5}  {6}",m_basePath, dir, platformFolder, baseDir, appResDir, settingFilePath, destination);
        //        }

        //        public static string GetPlatformFolderForAssetBundles()
        //        {
        //#if UNITY_EDITOR
        //            return GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        //#else
        //		return GetPlatformFolderForAssetBundles(Application.platform);
        //#endif
        //        }

        //        private static string GetPlatformFolderForAssetBundles(BuildTarget target)
        //        {
        //            switch (target)
        //            {
        //                case BuildTarget.Android:
        //                    return "android";
        //                case BuildTarget.iOS:
        //                    return "ios";
        //                case BuildTarget.StandaloneWindows:
        //                case BuildTarget.StandaloneWindows64:
        //                    return "windows";
        //                case BuildTarget.StandaloneOSXIntel:
        //                case BuildTarget.StandaloneOSXIntel64:
        //                    //case BuildTarget.StandaloneOSX:
        //                    return "osx";
        //                // Add more build targets for your own.
        //                // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
        //                default:
        //                    return null;
        //            }
        //        }

        //        //删除Manifest文件
        //        private static void DelManifest(ref string outputPath)
        //        {
        //            string platformFolder = GetPlatformFolderForAssetBundles().ToLower();


        //            // Choose the output path according to the build target.
        //            //string outputPath = Application.streamingAssetsPath + "/" + assetBundlesDir;

        //            List<string> listPath = new List<string>();

        //            Dictionary<string, bool> dicfilter = new Dictionary<string, bool>();
        //            dicfilter.Add(".manifest", true);
        //            //获取路径
        //            GetFilePaths(ref outputPath, listPath, dicfilter);

        //            string abManifest = platformFolder + ".manifest";

        //            int nLen = listPath.Count;
        //            for (int i = 0; i < nLen; ++i)
        //            {
        //                if (listPath[i].IndexOf(abManifest) >= 0)
        //                {
        //                    continue;
        //                }
        //                if (File.Exists(listPath[i]))
        //                {
        //                    File.Delete(listPath[i]);
        //                }
        //            }
        //        }

        //        //获取某个目录的特定文件
        //        private static void GetFilePaths(ref string dir, List<string> listPath, Dictionary<string, bool> dicfilter)
        //        {
        //            DirectoryInfo dire = new DirectoryInfo(dir);
        //            if (null == dire || !dire.Exists)
        //            {
        //                if (dire != null)
        //                {

        //                    FileSystemInfo file = dire as FileSystemInfo;
        //                    string extension = file.Extension.ToLower();
        //                    if (dicfilter.ContainsKey(extension))
        //                    {
        //                        listPath.Add(file.FullName);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                string fullName = null;
        //                foreach (FileSystemInfo info in dire.GetFileSystemInfos())
        //                {
        //                    fullName = info.FullName;
        //                    GetFilePaths(ref fullName, listPath, dicfilter);
        //                }
        //            }
        //        }
    }
}