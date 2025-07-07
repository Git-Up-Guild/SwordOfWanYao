using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



namespace XGame.Asset.Load
{
    public class RecordResTool : MonoBehaviour
    {

#if UNITY_EDITOR
        //全局对象编辑器下用来监控

        static public RecordResTool g_RecordResTools = null;


        public bool isRecord = false;
        public bool isOnlySync = false;
        public bool save = false;
        public string showPath = string.Empty;

        public string RecordResPath { get => $"{Application.dataPath}/../RecordRes{(isOnlySync ? "_OnlySync" : "")}.txt"; }
        //private string _curTime = string.Empty;
        private bool _isInit = false;
        private IGAssetLoader _loadMgr;
        private HashSet<string> _recordList = new HashSet<string>();

        public void Start()
        {
            g_RecordResTools = this;
            showPath = RecordResPath;
            _isInit = false;
            if (!isRecord)
                enabled = false;

            IGAssetLoader gAssetLoader = XGameComs.Get<IGAssetLoader>();
            Init(gAssetLoader);
        }

        private void OnDestroy()
        {
#if DEBUG_LOG
///#///#            Debug.Log("资源录制完成！！");
#endif

            Save();
          
            if (_loadMgr != null)
            {
                _loadMgr.RemoveLoadCallback(LoadCallback);
            }
        }

        public void Update()
        {
            if(save==false)
            {
                return;
            }
            save = false;
            Save();


        }

        public void Init(XGame.Asset.IGAssetLoader loader)
        {

            if (loader != null && _isInit)
            {
                return;
            }

            _loadMgr = loader;
            _loadMgr.AddLoadCallback(LoadCallback);

            _isInit = true;

        }

        private void LoadCallback(string bundleName, bool isSync)
        {
            if (isOnlySync && !isSync)
            {
                return;
            }
            if (!_recordList.Contains(bundleName))
            {
                _recordList.Add(bundleName);
            }
        }

        private void Save()
        {

            if (_recordList.Count > 0)
            {
                //if (File.Exists(RecordResPath))
                {
                    File.WriteAllLines(RecordResPath, _recordList.ToArray());
                }

            }
        }
#endif
    }
}
