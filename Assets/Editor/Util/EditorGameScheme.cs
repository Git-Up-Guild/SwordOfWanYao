

using basetools;
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using basetools.DataCenterLite;

#if UNITY_EDITOR
namespace XGameEditor.Util
{
    /// <summary>
    /// 编辑器读配置表工具
    /// </summary>
    public class EditorGameScheme
    {
        static private EditorGameScheme _instance;
        static public EditorGameScheme Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorGameScheme();
                    _instance.ReloadScheme();
                }
                return _instance;
            }
        }

        public Cgamescheme GameScheme { get; private set; } 
        private string DataPath { get => Application.dataPath + "/G_Resources/Game/GameScp/"; }
        private IBaseToolsLib pToolsLib;
        private IExternalFileSystem gameSchemeFileSystem;
        private IDataCenterLite dataCenterLite;
        private IDataCenterLite CreateDataCenter()
        {
            if (pToolsLib == null)
            {
                pToolsLib = CBaseToolsExporter.CreateBaseToolsLib();
            }
            gameSchemeFileSystem = new EditorCSVFileSystem();
            return pToolsLib.CreateDataCenter();
        }

        //初始化表格
        public bool ReloadScheme()
        {
            string dsDataPath = DataPath;
            dataCenterLite = CreateDataCenter();
            if (!dataCenterLite.OpenDataSet(dsDataPath + "gamescheme.bytes", dsDataPath, gameSchemeFileSystem))
            {
                string error = dataCenterLite.GetLastError().Replace("iFileID=","").Replace("iFileID = ", "");
                Debug.LogError("打开.dsr表数据文件失败：" + error);
                return false;
            }

            GameScheme = new Cgamescheme();
            if (!GameScheme.Create(dataCenterLite))
            {
                string error = dataCenterLite.GetLastError().Replace("iFileID=", "").Replace("iFileID = ", "");
                Debug.LogError("创建C#表数据失败：" + error);
                return false;
            }

            return true;
        }
    }

    public class EditorCSVFileSystem : IExternalFileSystem
    {
        private readonly string AssetsFolder = "Assets";
        private string m_strLastError = "";

        private string GetResourcePath(string strFilePath)
        {
            string filePath = strFilePath.Substring(strFilePath.IndexOf(AssetsFolder));
            filePath = filePath.Replace(".dat", ".bytes").Replace("//", "/");
            return filePath;
        }

        public byte[] ReadAllBytes(string strFilePath)
        {
            byte[] arrContant = null;
            var path = GetResourcePath(strFilePath);
            try
            {
                arrContant = File.ReadAllBytes(path);
            }
            catch (Exception pErr)
            {
                m_strLastError = string.Format("读取文件时发生异常({0}) .  File:{1}", pErr.Message, path);
            }

            return arrContant;
        }


        public bool IsFileExist(string strFilePath)
        {
            string filePath = GetResourcePath(strFilePath);
            return File.Exists(filePath);
        }

        public bool WriteAllBytes(string strFilePath, byte[] arrMBS)
        {
            throw new Exception("表数据 不准写！！strFilePath " + strFilePath);
        }

        public string GetLastError()
        {
            return m_strLastError;
        }
    }
}
#endif
