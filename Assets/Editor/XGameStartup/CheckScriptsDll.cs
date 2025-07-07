using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using XClient.Common;

namespace XGameEditor
{
    public class CheckScriptsDll
    {
        const string PDB_RECORD_FILE = "dll_record_file.txt";//记录当前游戏进行数据

        static string Record_FilePath
        {
            get
            {
                string filePath = Application.dataPath.Replace("Assets", PDB_RECORD_FILE);
                return filePath;
            }
        }

        public static void StartCheck()
        {
            //Debug.LogError("启动了游戏 ");

            string str = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;//当前当前unity安装目录

            if (CheckDLL_Change())//dll有变更
            {
                StartCreateMDB();

            }
            else
            {
                Debug.Log("dll没有变更，不用重新生成");
            }
        }


        static string DllKey(FileSystemInfo file)
        {
            return file.Name + "|" + file.LastWriteTime.ToString();
        }
        static bool CheckDLL_Change()
        {
            bool dllIsChange = false;


            List<string> list = new List<string>();
            string filePath = Record_FilePath;
            if (!File.Exists(filePath))
            {
                dllIsChange = true;
            }
            else
            {
                string[] allLine = File.ReadAllLines(filePath);
                foreach (var line in allLine)
                {
                    list.Add(line);
                }
            }

            if (dllIsChange == false)
            {
                FileInquireUtil fileInquireUtil = new FileInquireUtil();
                fileInquireUtil.StartInquire(Application.dataPath + "/Scripts/Dll", (file) =>
                {
                    if (dllIsChange == false && file.Extension == ".dll")
                    {
                        if (!list.Contains(DllKey(file)))
                        {
                            dllIsChange = true;
                        }
                    }
                });
            }
            return dllIsChange;
        }

        static void RecordDll_Change()
        {
            List<string> list = new List<string>();
            FileInquireUtil fileInquireUtil = new FileInquireUtil();
            fileInquireUtil.StartInquire(Application.dataPath + "/Scripts/Dll", (file) =>
            {
                if (file.Extension == ".dll")
                {
                    string time = file.LastWriteTime.ToString();
                    list.Add(DllKey(file));
                }
            });
            string filePath = Record_FilePath;
            File.WriteAllLines(filePath, list.ToArray());
        }

        static void Delete_PDB()
        {

            string[] pdbPaths = Directory.GetFiles(Application.dataPath + "/BaseScripts/Dll", "*.pdb", SearchOption.TopDirectoryOnly);
            int nCount = pdbPaths.Length;
            for (int i = 0; i < nCount; ++i)
            {
                File.Delete(pdbPaths[i]);
            }



        }

        [MenuItem("XGame/重新生成MDB文件")]
        /// <summary>
        /// 开始生成pdb文件
        /// </summary>
        static void StartCreateMDB()
        {
            Debug.Log("开始重新生成PDB文件");
            //Application.OpenURL("G:/WorkSpace/3002/Xgame3002_source/bin/Client/Game/Assets/Scripts/Dll/pdb2mdb2.bat");

            RunCmd();
            RecordDll_Change();
            Delete_PDB();
        }



        private static string CmdPath = @"C:\Windows\System32\cmd.exe";

        static void RunCmd()
        {
            string unityExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string dllFolder = Application.dataPath + "/BaseScripts/Dll";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows 下使用 bat 执行 pdb2mdb
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.WorkingDirectory = dllFolder;
                proc.StartInfo.FileName = "pdb2mdb.bat";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                Debug.LogWarning("[CheckScriptsDll] 当前为 macOS 平台，跳过 pdb2mdb 转换步骤（该工具仅适用于 Windows）");

            }
            else
            {
                Debug.LogWarning($"[CheckScriptsDll] 当前平台不支持：{Application.platform}，跳过 pdb2mdb 处理");
            }
        }
    }
}