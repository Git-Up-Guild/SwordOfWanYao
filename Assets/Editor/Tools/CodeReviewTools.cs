/*******************************************************************
** 文件名:	OptimizationFlagTools.cs
** 版  权:	(C) 深圳冰川网络股份有限公司
** 创建人:	郑秀程
** 日  期:	2019-10-20
** 版  本:	1.0
** 描  述:	优化标记导出工具。在进行CodeReview的时候，遇到要进行优化的地地方使用 //$$优化 进行标注，然后通过这个工具导出来
** 应  用:  
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.Tools
{
    class CodeReviewTools : Editor
    {
        enum FileType
        {
            CS,
            LUA,
        }

        /// <summary>
        /// 过滤配置
        /// </summary>
        class ExportConfig
        {
            public string name;
            public string tag;
            public string baseDir;
            public string[] subDir;
            public string fileExt;
            public FileType fileType;
        }

        class FlagData
        {
            public string fileType;
            public string filePath;
            public int lineNo;
            public string desc;
        }

        private static ExportConfig csFileConfig;
        private static ExportConfig luaFileConfig;

        static CodeReviewTools()
        {
            //CS文件日志配置
            csFileConfig = new ExportConfig();
            csFileConfig.name = "CS文件CodeReview标记导出";
            csFileConfig.tag = @"//$$优化";
            csFileConfig.baseDir = Directory.GetParent(Application.dataPath).Parent.Parent.Parent.FullName + "/Client";
            csFileConfig.subDir = new string[] { /*"Base", */"Client", "ClientEX", "Common", "controller", "Game", "Render", "UI", "Coms/Q1GameBase", "Coms/Q1GameEngine" };
            csFileConfig.fileExt = "*.cs";
            csFileConfig.fileType = FileType.CS;

            //LUA文件日志配置
            luaFileConfig = new ExportConfig();
            luaFileConfig.name = "LUA文件CodeReview标记导出";
            luaFileConfig.tag = @"--//$$优化";
            luaFileConfig.baseDir = Application.dataPath + "/G_Resources/Game/Lua/";
            luaFileConfig.subDir = new string[] { "Sources" };
            luaFileConfig.fileExt = "*.lua";
            luaFileConfig.fileType = FileType.LUA;
        }

        [MenuItem("XGame/其它/Code Review Export")]
        public static void Export()
        {
            List<FlagData> lsFlags = new List<FlagData>();
            DoExport(csFileConfig, lsFlags);
            DoExport(luaFileConfig, lsFlags);
            EditorUtility.ClearProgressBar();

            //保存文件
            Save(lsFlags);
        }

        private static void Save(List<FlagData> lsFlags)
        {
            string[] content = new string[lsFlags.Count + 1];
            content[0] = "序号,脚本类型,文件名称,行号,修改人,描述";

            for(var i = 0; i < lsFlags.Count; ++i)
            {
                FlagData data = lsFlags[i];
                content[i + 1] = $"{i},{data.fileType},{data.filePath},{data.lineNo},,{data.desc}";
            }

            string baseDir = XGameEditorUtilityEx.GetParentDir(Application.dataPath);
            string fileDir = baseDir + "/CodeReview";
            XGameEditorUtilityEx.CreateDir(fileDir);
            string fileName = System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
            string filePath = fileDir + "/CodeReview_" + fileName + ".csv";
            File.WriteAllLines(filePath, content);
        }

        private static void DoExport(ExportConfig config, List<FlagData> lsFlags)
        {
            for (int i = 0; i < config.subDir.Length; i++)
            {
                string tempPath = config.baseDir + "/" + config.subDir[i];
                string[] files = Directory.GetFiles(tempPath, config.fileExt, SearchOption.AllDirectories);

                for (int j = 0; j < files.Length; j++)
                {
                    HandleSingleFile(files[j], config, lsFlags);
                    EditorUtility.DisplayProgressBar(config.name + "：" + config.subDir[i], files[j], (float)j / files.Length);
                }
            }
        }

        private static void HandleSingleFile(string fileName, ExportConfig config, List<FlagData> lsFlags)
        {
            //文件不存在
            if (!File.Exists(fileName))
                return;

            string[] lines = File.ReadAllLines(fileName, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                //已经包含了标记了，不处理
                if (!line.StartsWith(config.tag))
                    continue;

                //包含了标记
                FlagData flag = new FlagData();
                flag.filePath = fileName.Substring(config.baseDir.Length);
                flag.lineNo = i;
                flag.fileType = config.fileType.ToString();
                flag.desc = line;

                //添加
                lsFlags.Add(flag);
            }
        }
    }
}
