using basetools.DataCenterLite;
using basetools;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace XGameEditor.Scheme
{
    internal class CDefaultFileSystem : IExternalFileSystem
    {
        private string m_strLastError = "";

        public byte[] ReadAllBytes(string strFilePath)
        {
            if (File.Exists(strFilePath))
            {
                return File.ReadAllBytes(strFilePath);
            }

            string fileEditorPath = strFilePath.Replace(".dat", ".bytes").Replace("//", "/");

            if (File.Exists(fileEditorPath))
            {
                return File.ReadAllBytes(fileEditorPath);
            }
            else
            {
                return null;
            }
        }

        public bool IsFileExist(string strFilePath)
        {
            string fileEditorPath = strFilePath.Replace(".dat", ".bytes").Replace("//", "/");
            return File.Exists(fileEditorPath);
        }

        public bool WriteAllBytes(string strFilePath, byte[] arrMBS)
        {
            try
            {
                string fileEditorPath = strFilePath.Replace(".dat", ".bytes").Replace("//", "/");
                File.WriteAllBytes(strFilePath, arrMBS);
            }
            catch (Exception ex)
            {
                m_strLastError = $"写入文件时发生异常({ex.Message}).  File:{strFilePath}";
                return false;
            }

            return true;
        }

        public string GetLastError()
        {
            return m_strLastError;
        }
    }

    public class EditorGameSchame
    {
        private static EditorGameSchame _instance;
        public static EditorGameSchame Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorGameSchame();

                    _instance.InitScheme();
                    _instance.LoadAllScheme();
                }

                return _instance;
            }
        }

        public Cgamescheme Scheme { get; private set; }

        private IBaseToolsLib m_toolsLib;
        private IExternalFileSystem m_gameSchemeFileSystem;
        private IDataCenterLite m_dataCenterLite;

        // 数据路径
        private string DataPath { get => Application.dataPath + "/G_Resources/Game/GameScp"; }

        // 表名对应iFileID
        private Dictionary<string, int> m_dicSchemeName2ID = new Dictionary<string, int>();

        /// <summary>
        /// 根据脚本索引来加载脚本
        /// </summary>
        /// <param name="nIndex"></param>
        public void LoadScheme(int nIndex)
        {
            var fileInfo = m_dataCenterLite.GetFileInfo(nIndex);
            var datName = fileInfo.strBinFileName;

            // 加到字典， 方便查
            if (m_dicSchemeName2ID.ContainsKey(datName))
            {
                Debug.LogError("初始化加载所有表，发现重复表名:" + datName);
            }
            m_dicSchemeName2ID.Add(datName, fileInfo.iFileID);

            //这里触发加载
            m_dataCenterLite.GetRecordByIndex(fileInfo.iFileID, 0);
        }

        /// <summary>
        /// 加载表配置的表
        /// </summary>
        /// <returns></returns>
        public void LoadAllScheme()
        {
            int count = m_dataCenterLite.GetFileNums();

            TDataFileInfo fileInfo;
            string datName;

            for (int i = 0; i < count; i++)
            {
                fileInfo = m_dataCenterLite.GetFileInfo(i);
                datName = fileInfo.strBinFileName;

                // 加到字典， 方便查
                if (m_dicSchemeName2ID.ContainsKey(datName))
                {
                    Debug.LogError("初始化加载所有表，发现重复表名:" + datName);
                    continue;
                }
                m_dicSchemeName2ID.Add(datName, fileInfo.iFileID);

                //这里触发加载
                m_dataCenterLite.GetRecordByIndex(fileInfo.iFileID, 0);
            }
        }

        /// <summary>
        /// 初始化表格
        /// </summary>
        /// <returns></returns>
        private bool InitScheme()
        {
            string dsDataPath = DataPath;
            m_dataCenterLite = CreateDataCenter();

            if (!m_dataCenterLite.OpenDataSet(dsDataPath + "/gamescheme.bytes", dsDataPath, m_gameSchemeFileSystem))
            {
                string error = m_dataCenterLite.GetLastError().Replace("iFileID=", "").Replace("iFileID = ", "");
                int fileID = 0;
                if (int.TryParse(error, out fileID))
                {
                    error = ((Cgamescheme.EnFileID)(fileID)).ToString();
                }
                else
                {
                    error = m_dataCenterLite.GetLastError();
                }
                Debug.LogError("打开.dsr表数据文件失败：" + error);
                return false;
            }

            Scheme = new Cgamescheme();

            if (!Scheme.Create(m_dataCenterLite))
            {
                string error = m_dataCenterLite.GetLastError().Replace("iFileID=", "").Replace("iFileID = ", "");
                int fileID = 0;
                if (int.TryParse(error, out fileID))
                {
                    error = ((Cgamescheme.EnFileID)(fileID)).ToString();
                }
                else
                {
                    error = m_dataCenterLite.GetLastError();
                }

                Debug.LogError("创建C#表数据失败：" + error + "，可能表格不是编译版本，需要先把表格bdr目录更新至编译版本！！！");
                return false;
            }

            return true;
        }

        private IDataCenterLite CreateDataCenter()
        {
            if (m_toolsLib == null)
                m_toolsLib = CBaseToolsExporter.CreateBaseToolsLib();

            m_gameSchemeFileSystem = new CDefaultFileSystem();
            return m_toolsLib.CreateDataCenter();
        }
    }
}
