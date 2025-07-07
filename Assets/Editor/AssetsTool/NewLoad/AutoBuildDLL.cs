using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

public class AutoBuildDLL : EditorWindow
{
    enum DLL_BUILD_OPTIONS
    {
        Debug = 0,
        Release = 1,
        ReleaseAndroid = 2,
        ReleaseAndroidProfiler = 3,
        ReleaseIos = 4,
        ReleasePc = 5
    }

    static DLL_BUILD_OPTIONS op = DLL_BUILD_OPTIONS.ReleaseAndroid;

    static string ms_DevenvPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\IDE\\devenv.com";
    static string ms_ProjectPath = "";// Application.dataPath + "/../../../../Client";
    static string ms_CSProjectPathBuffer = ""; //Application.dataPath+"/../../../../Client";
    static string ms_CSharpPathWarning = "";
    static string ms_DevenvPathWarning = "";
    static string denenvPath_Buffer = "";
    static string projectPath_Buffer = "";
    static string cachepath = "";
    static bool inited = false;
    static XmlDocument cachefile = null;
    private static string msLogF = "";
    private static string msLogS = "";
    private static string ToolVersion = "1.0";

    AutoBuildDLL()
    {
        titleContent = new GUIContent("动态库编译工具");
    }

    private void OnEnable()
    {
        cachepath = Application.dataPath + "/dllBuildEnvCache.xml";
        //msLogS = "";
        //msLogF = "";
    }

    private static void Init()
    {
        if(!inited)
        {

            LoadCache();
            CheckDefaultCSharpPath();
            ms_CSProjectPathBuffer = ms_ProjectPath;

            FileInfo myfir = new FileInfo(ms_DevenvPath);
            if (myfir.Exists)
            {
                ms_DevenvPathWarning = "已找到devenv.com!";
                SaveDenenvCache(ms_DevenvPath);
            }
            else
            {
                //Debug.Log("未找到devenv.com!");
                ms_DevenvPathWarning = "未找到devenv.com!";
            }
            op = LoadSetting();
            msLogF = LoadLog(true);
            msLogS = LoadLog(false);
            inited = true;
        }
    }

    [MenuItem("Tool/DLL Builder")]
    static void ShowWindow()
    {
        //Debug.Log("Window Show!!");
        EditorWindow.GetWindow(typeof(AutoBuildDLL));
        msLogF = "";
        msLogS = "";
        RefreshCacheFile();
        SaveLog(" ", true);
        SaveLog(" ",false);
        Init();
    }

    void OnGUI()
    {
        Init();
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 24;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("动态库编译工具");
        GUI.skin.label.fontSize = 12;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUILayoutOption[] opt = { GUILayout.ExpandWidth(true), GUILayout.Width(600.0f) };

        ms_CSProjectPathBuffer = EditorGUILayout.TextField("CSharp Path", ms_CSProjectPathBuffer, opt);
        if(IsCSharpPath(ms_CSProjectPathBuffer))
        {
            ms_CSharpPathWarning = "已找到CSharp项目!";
            SaveCSharpPathCache(ms_CSProjectPathBuffer);
        }
        else
        {
            ms_CSharpPathWarning = "未找到CSharp项目!";
        }
        GUILayout.Space(10);
        GUILayout.Label(ms_CSharpPathWarning);

        if (!ms_DevenvPath.Equals(""))
        {
            FileInfo myfir = new FileInfo(ms_DevenvPath);
            if (myfir.Exists)
            {
                ms_DevenvPathWarning = "已找到devenv.com!";
                SaveDenenvCache(ms_DevenvPath);
            }
            else
            {
                ms_DevenvPathWarning = "未找到devenv.com!";
            }
        }
        else
        {
            ms_DevenvPathWarning = "未找到devenv.com!";
        }
        GUILayout.Space(10);

        ms_DevenvPath = EditorGUILayout.TextField("denenv.com", ms_DevenvPath, opt);
        GUILayout.Label(ms_DevenvPathWarning);

        op = (DLL_BUILD_OPTIONS)EditorGUILayout.EnumPopup("Build Option:",op);

        GUILayout.Space(10);

        if (GUILayout.Button("Build"))
        {
            BuildDLL();
        }
        GUILayout.Space(10);
       // string outputtest = msLogF;
        GUILayout.Label(msLogF);
        GUILayout.Space(5);
        //outputtest = msLogS;
        GUILayout.Label(msLogS);
    }

    public static void BuildDLL()
    {
        //检查devenv.com
        FileInfo myfir = new FileInfo(ms_DevenvPath);
        if(myfir.Exists)
        {
            Debug.Log("exist: "+ ms_DevenvPath);
        }
        else
        {
            Debug.Log("未找到devenv.com!");
            return;
        }
        //检查CSharp的.dll编译项目

        ms_CSProjectPathBuffer = Application.dataPath + "/../../../../Client";

        if (SetCSharpPath(ms_CSProjectPathBuffer))
        {
            ms_CSProjectPathBuffer = ms_ProjectPath;
        }
        else
        {
            //Debug.Log("未找到CSharp项目!");
            return;
        }
        //获取.cs输出路径,script路径
        string scripts_path = Application.dataPath + "/Scripts/";
        //检查编译类型
        string build_opt = "ReleaseAndroid";
        switch (op)
        {
            case DLL_BUILD_OPTIONS.Debug:
                build_opt = "Debug";
                break;
            case DLL_BUILD_OPTIONS.Release:
                build_opt = "Release";
                break;
            case DLL_BUILD_OPTIONS.ReleaseAndroid:
                build_opt = "ReleaseAndroid";
                break;
            case DLL_BUILD_OPTIONS.ReleaseAndroidProfiler:
                build_opt = "ReleaseAndroidProfiler";
                break;
            case DLL_BUILD_OPTIONS.ReleaseIos:
                build_opt = "ReleaseIos";
                break;
            case DLL_BUILD_OPTIONS.ReleasePc:
                build_opt = "ReleasePc";
                break;
            default:
                break;
        }
        SaveSetting(op);

        /*
        //修改Script.csproj，删除Gen目录下的项
        XmlDocument csproject_document = new XmlDocument();
        csproject_document.Load(ms_ProjectPath + "\\Script\\Script.csproj");
        XmlElement root = csproject_document.DocumentElement;
        XmlNodeList xmlNodeList = root.GetElementsByTagName("Compile");
        XmlNode parentNode = xmlNodeList[0].ParentNode;
        //获取dll文件输出路径
        string dll_path = (ms_ProjectPath + "\\Script\\" + root.GetElementsByTagName("OutputPath")[0].InnerText).Replace("\\","/");

        //修改FaceBook.Unity.dll路径
        XmlNodeList RefNodeList = root.GetElementsByTagName("Reference");
        foreach (XmlElement xl1 in RefNodeList)
        {
            if (xl1.GetAttribute("Include").Equals("Facebook.Unity"))
            {
                XmlNode FBElement = xl1.GetElementsByTagName("HintPath")[0];
                string fb_path = ms_ProjectPath + "\\Script\\" + FBElement.InnerText.Substring(0, FBElement.InnerText.IndexOf("\\Facebook.Unity.dll"));
                string local_fb_path = Application.dataPath + "/FacebookSDK/Plugins";
                if(isEqualPath(fb_path, local_fb_path))
                {
                }
                else
                {
                    FBElement.InnerText = local_fb_path.Replace("/", "\\") + "\\Facebook.Unity.dll";
                }
                //Debug.Log("FACEBOOK PATH IS: " + fb_path);
                break;
            }
        }

        foreach (XmlElement xl1 in xmlNodeList)
        {
            if (xl1.GetAttribute("Include").Contains("XLua\\Gen\\"))
            {
                parentNode.RemoveChild(xl1);
            }
        }
        File.Delete(ms_ProjectPath + "\\Script\\Script.csproj");
        csproject_document.Save(ms_ProjectPath + "\\Script\\Script.csproj");
        DirectoryInfo mydir = new DirectoryInfo(ms_ProjectPath + "\\Script\\XLua\\Gen");
        if (mydir.Exists)
        {
            FileInfo[] files = mydir.GetFiles("*.*");
            for (int i = 0; i < files.Length; i++)
            {
                files[i].Delete();
            }
        }

        */

        //启动devenv.com，编译.dll文件
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = @ms_DevenvPath;
        p.StartInfo.Arguments = @ms_ProjectPath + "\\Client.sln /rebuild " + build_opt;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true; 
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        System.Console.InputEncoding = System.Text.Encoding.UTF8;
        p.Start();
        p.WaitForExit(60);
        string outlog = p.StandardOutput.ReadToEnd();
        int length = outlog.LastIndexOf("成功") - 1;
        //Regex.Match(outlog, "(.-)=+$");

        if(length>0)
        {
            msLogF = "第一次编译: " + outlog.Substring(length, outlog.LastIndexOf(" ") - length);
            SaveLog(msLogF, true);
        }
       
        //outlog = UTF8Convertion(outlog);
        Debug.Log("第一次编译输出：\n" + outlog);
        p.Close();

        //刷新导入
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


        /*
        if(isEqualPath(dll_path,scripts_path))
        {
            //Debug.Log("DLL PATH AND SCRIPTS PATH ARE SAME!");
        }
        else
        {
            //Debug.Log("DLL PATH AND SCRIPTS PATH ARE NOT SAME!");
            MoveAllFiles(dll_path, scripts_path,"*.dll");
        }


        //刷新导入
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //清理XLua生成的接口
        CSObjectWrapEditor.Generator.ClearAll();
        if (op != DLL_BUILD_OPTIONS.Debug)
        {
            //生成Gen下的.cs文件
            CSObjectWrapEditor.Generator.GenAll();

            if (isEqualPath(luaGeneratePath, csharp_gen_path))
            {
                //Debug.Log("LUA PATH AND GEN PATH ARE SAME!");
            }
            else
            {
                //Debug.Log("LUA PATH AND GEN PATH ARE NOT SAME!");
                MoveAllFiles(luaGeneratePath, csharp_gen_path,"*.cs");
            }

            //修改Script.csproj，添加Gen目录下的项
            if (mydir.Exists)
            {
                FileInfo[] files = mydir.GetFiles("*.cs");
                for (int i = 0; i < files.Length; i++)
                {
                    //Debug.Log(files[i].Name);
                    XmlElement newelement = csproject_document.CreateElement("Compile", parentNode.NamespaceURI);
                    newelement.SetAttribute("Include", "XLua\\Gen\\" + files[i].Name);
                    parentNode.AppendChild(newelement);
                }
            }
            csproject_document.Save(ms_ProjectPath + "\\Script\\Script.csproj");

            //再次启动denenv.com，编译.dll文件
            p.Start();
            p.WaitForExit();
            outlog = p.StandardOutput.ReadToEnd();
            length = outlog.LastIndexOf("成功") - 1;
            msLogS = "第二次编译: " + outlog.Substring(length, outlog.LastIndexOf(" ") - length);
            SaveLog(msLogS, false);
            Debug.Log("第二次编译输出：\n" + outlog);
            p.Close();

            if (isEqualPath(dll_path, scripts_path))
            {
                //Debug.Log("DLL PATH AND SCRIPTS PATH ARE SAME!");
            }
            else
            {
                //Debug.Log("DLL PATH AND SCRIPTS PATH ARE NOT SAME!");
                MoveAllFiles(dll_path, scripts_path, "*.dll");
            }
           
            //刷新导入
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

     */


    }

    private static string UTF8Convertion(string input)
    {
        //System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        byte[] buffer = System.Text.Encoding.GetEncoding("GBK").GetBytes(input);
        return System.Text.Encoding.GetEncoding("utf-8").GetString(buffer);
    }

    private static bool SetCSharpPath(string newpath)
    {
        if(string.IsNullOrEmpty(newpath))
        {
            return false;
        }

        if(IsCSharpPath(newpath))
        {
            ms_CSharpPathWarning = "已找到CSharp项目!";
            ms_ProjectPath = newpath;
            SaveCSharpPathCache(newpath);
            return true;
        }
        else
        {
            return CheckCurrentCSharpPath();
        }
    }

    private static bool CheckCurrentCSharpPath()
    {
        if (IsCSharpPath(ms_ProjectPath))
        {
            ms_CSharpPathWarning = "已找到CSharp项目!";
            return true;
        }
        else
        {
            return CheckDefaultCSharpPath();
        }
    }


    private static bool CheckDefaultCSharpPath()
    {
        if(IsCSharpPath(ms_ProjectPath))
        {
            ms_CSharpPathWarning = "已找到CSharp项目!";
            return true;
        }
        string default_path = Application.dataPath;
        default_path += "/../../../Client/CSharp";
        DirectoryInfo tmpinfo = new DirectoryInfo(default_path);
        Debug.Log(tmpinfo.FullName);
        FileInfo myfile = new FileInfo(default_path + "/Client.sln");
        if (myfile.Exists)
        {
            ms_CSharpPathWarning = "已找到CSharp项目!";
            ms_ProjectPath = default_path;
            return true;
        }
        else
        {
            ms_CSharpPathWarning = "未找到CSharp项目!";
            return false;
        }
    }

    private static bool IsCSharpPath(string path)
    {
        if(path.Equals(""))
        {
            return false;
        }
        else
        {
            FileInfo myfile = new FileInfo(path + "\\Client.sln");
            if (myfile.Exists )//&& Directory.Exists(path + "\\Script"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private static bool isEqualPath(string pathA, string pathB)
    {
        pathA.Replace("\\", "/");
        pathB.Replace("\\", "/");
        DirectoryInfo DirA = new DirectoryInfo(pathA);
        DirectoryInfo DirB = new DirectoryInfo(pathB);
        if(DirA.Exists&&DirB.Exists&&DirA.FullName.Equals(DirB.FullName))
        {
           // Debug.Log("Path A is: " + DirA.FullName + "\nPath B is: " + DirB.FullName);
            return true;
        }
        else
        {
           // Debug.Log("Path A is: " + DirA.FullName + "\nPath B is: " + DirB.FullName);
            return false;
        }
    }

    public static void MoveAllFiles(string src, string dst,string searchPattern = "*.*")
    {
        DirectoryInfo srcinfo = new DirectoryInfo(src);
        if(srcinfo.Exists)
        {
            dst = dst.Replace("\\", "/");
            FileInfo[] files = srcinfo.GetFiles(searchPattern);
            for(int i = 0; i<files.Length;i++)
            {
  
                if (File.Exists(dst + "/" + files[i].Name))
                {
                    File.Delete(dst + "/" + files[i].Name);
                }
                File.Move(files[i].FullName, dst + "/" + files[i].Name);
            }
        }
    }  

    private static void LoadCache()
    {
        RefreshCacheFile();
        denenvPath_Buffer = cachefile.SelectSingleNode("paths").SelectSingleNode("denenv").InnerText;
        if(!denenvPath_Buffer.Equals("") && denenvPath_Buffer.Contains("devenv.com") && File.Exists(denenvPath_Buffer))
        {
            ms_DevenvPath = denenvPath_Buffer;
        }
        projectPath_Buffer = cachefile.SelectSingleNode("paths").SelectSingleNode("csharp").InnerText;
        if(IsCSharpPath(projectPath_Buffer))
        {
            ms_ProjectPath = projectPath_Buffer;
        }
    }

    private static void SaveDenenvCache(string newstr)
    {
        RefreshCacheFile();
        if (! newstr.Equals(denenvPath_Buffer))
        {
            denenvPath_Buffer = newstr;
            cachefile.SelectSingleNode("paths").SelectSingleNode("denenv").InnerText = newstr;
            cachefile.Save(cachepath);
            //save
        }
    }

    private static void SaveCSharpPathCache(string newstr)
    {
        if(string.IsNullOrEmpty(cachepath))
        {
            return;
        }

        RefreshCacheFile();
        if (!newstr.Equals(projectPath_Buffer))
        {
            projectPath_Buffer = newstr;
            //RefreshCacheFile();
            cachefile.SelectSingleNode("paths").SelectSingleNode("csharp").InnerText = newstr;
            cachefile.Save(cachepath);
            //save
        }
    }

    private static void SaveLog(string newstr, bool first)
    {
        RefreshCacheFile();
        if (first)
        {
            cachefile.SelectSingleNode("paths").SelectSingleNode("first").InnerText = newstr;
            cachefile.Save(cachepath);
        }
        else
        {
            cachefile.SelectSingleNode("paths").SelectSingleNode("second").InnerText = newstr;
            cachefile.Save(cachepath);
        }
    }

    private static string LoadLog(bool first)
    {
        RefreshCacheFile();
        if(first)
        {
            return cachefile.SelectSingleNode("paths").SelectSingleNode("first").InnerText;
        }
        else
        {
            return cachefile.SelectSingleNode("paths").SelectSingleNode("second").InnerText;
        }
    }

    private static DLL_BUILD_OPTIONS LoadSetting()
    {
        RefreshCacheFile();
        string token = cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText;

        if (token.Equals("D"))
        {
            return DLL_BUILD_OPTIONS.Debug;
        }
        else if (token.Equals("R"))
        {
            return DLL_BUILD_OPTIONS.Release;
        }
        else if (token.Equals("RPAndroid"))
        {
            return DLL_BUILD_OPTIONS.ReleaseAndroidProfiler;
        }
        else if (token.Equals("RIos"))
        {
            return DLL_BUILD_OPTIONS.ReleaseIos;
        }
        else if (token.Equals("RPc"))
        {
            return DLL_BUILD_OPTIONS.ReleasePc;
        }
        else
        {
            return DLL_BUILD_OPTIONS.ReleaseAndroid;
        }
    }

    private static void SaveSetting(DLL_BUILD_OPTIONS option)
    {
        RefreshCacheFile();
        switch (option)
        {
            case DLL_BUILD_OPTIONS.Debug:
                cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText = "D";
                break;
            case DLL_BUILD_OPTIONS.Release:
                cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText = "R";
                break;
            case DLL_BUILD_OPTIONS.ReleaseAndroid:
                cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText = "RAndroid";
                break;
            case DLL_BUILD_OPTIONS.ReleaseAndroidProfiler:
                cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText = "RPAndroid";
                break;
            case DLL_BUILD_OPTIONS.ReleaseIos:
                cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText = "RIos";
                break;
            case DLL_BUILD_OPTIONS.ReleasePc:
                cachefile.SelectSingleNode("paths").SelectSingleNode("set").InnerText = "RPc";
                break;
            default:
                break;
        }

        if(string.IsNullOrEmpty(cachepath))
        {
            cachepath = Application.dataPath + "/dllBuildEnvCache.xml";
        }

        cachefile.Save(cachepath);
    }

    private static void CreateDefaultCacheFile()
    {
        cachefile = new XmlDocument();
        XmlElement root = cachefile.CreateElement("paths");
        XmlElement devenvelement = cachefile.CreateElement("denenv");
        devenvelement.InnerText = " ";
        root.AppendChild(devenvelement);

        XmlElement csharpelement = cachefile.CreateElement("csharp");
        csharpelement.InnerText = " ";
        root.AppendChild(csharpelement);

        XmlElement log1 = cachefile.CreateElement("first");
        log1.InnerText = " ";
        root.AppendChild(log1);

        XmlElement log2 = cachefile.CreateElement("second");
        log2.InnerText = " ";
        root.AppendChild(log2);

        XmlElement Setting = cachefile.CreateElement("set");
        Setting.InnerText = "RAndroid";
        root.AppendChild(Setting);

        XmlElement Version = cachefile.CreateElement("version");
        Version.InnerText = ToolVersion;
        root.AppendChild(Version);

        cachefile.AppendChild(root);

        if(string.IsNullOrEmpty(cachepath))
        {
            return;
        }

        cachefile.Save(cachepath);
        //Debug.Log(xml.SelectSingleNode("paths").SelectSingleNode("denenv").InnerText);
    }

    private static void RefreshCacheFile()
    {

        if(cachefile == null)
        {
            if (!File.Exists(cachepath))
            {
                CreateDefaultCacheFile();
            }
            else
            {
                cachefile = new XmlDocument();
                cachefile.Load(cachepath);
                RefreshCacheFile();
            }
        }
        else if(cachefile.SelectSingleNode("paths").SelectSingleNode("version") == null || !cachefile.SelectSingleNode("paths").SelectSingleNode("version").InnerText.Equals(ToolVersion))
        {
            CreateDefaultCacheFile();
        }

    }

    public static void iosInnerBuild()
    {
        //移动.cs文件，重新生成ios动态库并导入至Unity项目
    }

    public static void BatchGenerateXLua()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
