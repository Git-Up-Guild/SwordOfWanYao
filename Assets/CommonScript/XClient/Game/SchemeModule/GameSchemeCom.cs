/*****************************************************
** 文 件 名：GameScheme
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/11/9 10:58:18
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using basetools;
using basetools.DataCenterLite;
using XGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Client.Scheme
{
    public class GameSchemeCom : ICom
    {
        public Cgamescheme gameScheme { get; private set; }
        private IBaseToolsLib m_toolsLib;
        private IExternalFileSystem m_gameSchemeFileSystem;
        private IDataCenterLite m_dataCenterLite;

        // 是否初始化完
        private bool isInited = false;

        // 数据路径
        private string DataPath { get => Application.dataPath + "/G_Resources/Game/GameScp/"; }

        // 表名对应iFileID
        private Dictionary<string, int> m_dicSchemeName2ID = new Dictionary<string, int>();

        public int ID { get; set; }

        public bool Create(object context, object config = null)
        {
            InitScheme();
            return true;
        }

        public void Release()
        {
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
        }

        public void Update()
        {
        }

        /// <summary>
        /// 获取配置脚本的数量
        /// </summary>
        /// <returns></returns>
        public int GetSchemeCount()
        {
            return m_dataCenterLite.GetFileNums();
        }

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
           // UnityEngine.Debug.LogError("private bool InitScheme()");

            string dsDataPath = DataPath;
            m_dataCenterLite = CreateDataCenter();
            if (!m_dataCenterLite.OpenDataSet(dsDataPath + "gamescheme.bytes", dsDataPath, m_gameSchemeFileSystem))
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

            Cgamescheme gameScheme = new Cgamescheme();
            this.gameScheme = gameScheme;
            if (!gameScheme.Create(m_dataCenterLite))
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

#if UNITY_EDITOR

            //foreach (Cgamescheme.EnFileID fileID in Enum.GetValues(typeof(Cgamescheme.EnFileID)))
            //{
            //    if ((int)fileID == 0 ) continue;
            //    bool loadSucess = m_dataCenterLite.PreloadData((int)fileID);

            //    if (loadSucess == false)
            //    {
            //        string lastError = m_dataCenterLite.GetLastError();
            //        Debug.LogError($"表格加载失败：fileID={fileID},loadSucess={loadSucess},lastError={lastError}");
            //        return false;
            //    }
            //}

#endif
            return true;
        }

        private IDataCenterLite CreateDataCenter()
        {
            if (m_toolsLib == null)
            {
                m_toolsLib = CBaseToolsExporter.CreateBaseToolsLib();
            }
            m_gameSchemeFileSystem = new CDefaultFileSystem();
            return m_toolsLib.CreateDataCenter();
        }
    }
}
