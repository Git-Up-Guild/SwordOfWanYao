#if UNITY_EDITOR
using System;
using basetools;
using UnityEngine;
using XGame.Utils;
using basetools.DataCenterLite;
using System.Collections.Generic;

namespace XClient.Client.Scheme
{
    public class EditorSchemeCenter : Singleton<EditorSchemeCenter>
    {
        // 表名对应iFileID
        private Dictionary<string, int> schemeNameIdDic = new Dictionary<string, int>();

        // 数据路径
        private string DataPath { get => Application.dataPath + "/G_Resources/GameScp/"; }

        // 表接口
        private Cgamescheme _gameScheme;
        public Cgamescheme GameScheme { get => _gameScheme; }
        private IBaseToolsLib pToolsLib;
        private IExternalFileSystem gameSchemeFileSystem;
        private IDataCenterLite dataCenterLite;
        // 新表初始化完回调
        private Action<bool> onSchemeInited;
        private IDataCenterLite CreateDataCenter()
        {
            if (pToolsLib == null)
            {
                pToolsLib = CBaseToolsExporter.CreateBaseToolsLib();
            }
            if (gameSchemeFileSystem == null)
            {
                gameSchemeFileSystem = new CDefaultFileSystem();
            }
            return pToolsLib.CreateDataCenter();
        }
        
        public void Start(IExternalFileSystem fileSystem)
        {
            gameSchemeFileSystem = fileSystem;
            string dsDataPath = DataPath;
            dataCenterLite = CreateDataCenter();
            if (!dataCenterLite.OpenDataSet(dsDataPath + "gamescheme.bytes", dsDataPath, gameSchemeFileSystem))
            {
                Debug.LogError("打开.dsr表数据文件失败：" + dataCenterLite.GetLastError());
            }
            _gameScheme = new Cgamescheme();
            if (!_gameScheme.Create(dataCenterLite))
            {
                Debug.LogError("创建C#表数据失败：" + dataCenterLite.GetLastError());
            }
        }
    }
}

#endif