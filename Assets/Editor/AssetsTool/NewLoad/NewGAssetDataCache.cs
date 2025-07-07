using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEditor;
using XGame.Utils;
using XGame.I18N;
using ReferenceFinder;

namespace XGameEditor
{
    public class NewGAssetDataCache : ScriptableObject
    {
        /// <summary>
        /// 资源数据
        /// </summary>
        [System.Serializable]
        public class NewAssetData
        {
            // 资源名
            public string assetPath;

            // 是否打包配置目录下的资源
            public bool isMainAsset;

            // 依赖资源名
            public List<string> dependAssetList = new List<string>();

            // 缓存最后修改标记（用来比对资源是否发生了改变，当前使用AssetDependencyHash）
            public string hash;
        }

        private static NewGAssetDataCache _instance;

        //全局实例
        public static NewGAssetDataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<NewGAssetDataCache>(NewAssetDataDef.AssetDataCacheFilePath);
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<NewGAssetDataCache>();
                        NewAssetDataDef.CheckCreateDirectory(NewAssetDataDef.AssetDataCacheFilePath);
                        AssetDatabase.CreateAsset(_instance, NewAssetDataDef.AssetDataCacheFilePath);
                        _instance.InitDic();
                    }

                }
                return _instance;
            }
        }

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<NewAssetData> assetDataList = new List<NewAssetData>();
        private Dictionary<string, NewAssetData> _assetDataDic = new Dictionary<string, NewAssetData>();
        public List<string> SearchFolder { get => NewGAssetBuildConfig.Instance.curMainResPath; }
        public List<string> SearchFilterFolder { get => NewGAssetBuildConfig.Instance.mainResFilterDirList; }


        public void InitDic()
        {
            if (assetDataList.Count != _assetDataDic.Count)
            {
                _assetDataDic.Clear();
                foreach (var assetData in assetDataList)
                {
                    _assetDataDic.Add(assetData.assetPath, assetData);
                }
            }
        }

        /// <summary>
        /// 是否在过滤文件夹中
        /// </summary>
        /// <param name="path"></param>
        private bool IsInFilterFolder(string path)
        {
            int count = SearchFilterFolder.Count;
            for (int i = 0; i < count; i++)
            {
                if (path.StartsWith(SearchFilterFolder[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 刷新AB纪录缓存
        /// </summary>
        public void RefreshCache()
        {
            assetDataList.Clear();
            _assetDataDic.Clear();
            HashSet<string> allAsset = new HashSet<string>();
            NewGAssetBuildConfig.Instance.ForceRefreshMainResPathList();

            //统计assets目录之外的资源
            NewAssetDataDef.outOfAssetsCount = 0;

            //所有文件的guid
            string[] guids = AssetDatabase.FindAssets(null, SearchFolder.ToArray());
            bool isCancel;
            var startTime = DateTime.Now;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                //过滤掉不需要打包的程序
                if (NewAssetDataDef.IsValidAsset(path))
                {
                    if (IsInFilterFolder(path))
                    {
                        Debug.Log($"过滤掉：{path}");
                        continue;
                    }

                    /*
                    if (EditorI18NConfig.IsDontBuild(path))
                    {
                        //Debug.Log($"I18N 国际化 过滤掉：{path}");
                        continue;
                    }
                    */

                    if (i % 100 == 0)
                    {
                        //显示处理进度
                        isCancel = EditorUtility.DisplayCancelableProgressBar($"刷新缓存:({i}/{guids.Length})", path, i * 1.0f / guids.Length);
                        //Debug.Log(path);
                        if (isCancel)
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                    }
                    UpdateBasicData(path, ref allAsset);
                }
            }
            InitDic();
            Debug.Log($"刷新缓存经过时间：startTime:{startTime}, endTime:{DateTime.Now}, 总数:{guids.Length}");
            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 检查循环依赖
        /// </summary>
        /// <returns></returns>
        public List<string> CheckLoopDepend()
        {
            InitDic();
            List<string> result = new List<string>();
            HashSet<string> tem = new HashSet<string>();
            for (int i = 0; i < assetDataList.Count; i++)
            {
                tem.Clear();
                var path = assetDataList[i].assetPath;
                bool isFind;
                var link = CheckLoopSame(path, path, ref tem, out isFind);
                if (isFind)
                {
                    result.Add(link);
                }
                Debug.Log($"isFind:{isFind}, link:{link}, mapCount:{tem.Count}");

            }
            return result;
        }

        /// <summary>
        /// 检查循环依赖
        /// </summary>
        /// <returns></returns>
        public string CheckLoopSame(string srcPath, string subPath, ref HashSet<string> mark, out bool isFindSame)
        {
            InitDic();
            isFindSame = false;
            if (mark.Contains(subPath))
                return string.Empty;
            if (!_assetDataDic.ContainsKey(srcPath))
                return string.Empty;

            mark.Add(subPath);
            string link = subPath;
            var dependList = _assetDataDic[subPath].dependAssetList;
            for (int i = 0; i < dependList.Count; i++)
            {
                var depend = dependList[i];
                if (depend == srcPath)
                {
                    isFindSame = true;
                    return depend;
                }
                bool isFind;
                var subLink = CheckLoopSame(srcPath, depend, ref mark, out isFind);
                if (isFind)
                {
                    isFindSame = true;
                    return depend + "," + subLink;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 更新依赖、Hash标记
        /// </summary>
        /// <param name="path"></param>
        private void UpdateBasicData(string path, ref HashSet<string> assetMap)
        {
            // 检查重复
            if (assetMap.Contains(path))
                return;
            assetMap.Add(path);

            // 生成资源
            NewAssetData assetData = GetOrCreateAssetData(path);



            // 场景文件不用查依赖，Unity自己会打依赖
            //if (!path.EndsWithEx(".unity"))
            {
                //获取依赖列表
                string[] dps = AssetDatabase.GetDependencies(path, false);
                assetData.dependAssetList.Clear();
                foreach (var dp in dps)
                {
                    if(dp.IndexOf(".skel.bytes") >=0)
                    {
                        Debug.Log("尝试增加" + dp);
                    }

                    if (NewAssetDataDef.IsValidAsset(dp))
                    {
                        if(NewGAssetBuildConfig.Instance.CanBeDependent(dp))
                        {
                            assetData.dependAssetList.Add(dp);
                        }else
                        {
                            Debug.Log("dependAssetList 跳过不能被静态依赖的资源"+ dp);
                        }
                        
                        UpdateBasicData(dp, ref assetMap);  // 循环查找依赖
                    }
                }
            }

            // 生成Hash，用来对比资源是否发生变化
            assetData.hash = AssetDatabase.GetAssetDependencyHash(path).ToString();

        }

        /// <summary>
        /// 获取资源数据，没有就创建
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private NewAssetData GetOrCreateAssetData(string path)
        {
            NewAssetData assetData;
            if (!_assetDataDic.TryGetValue(path, out assetData))
            {
                assetData = new NewAssetData();
                assetData.assetPath = path;
                assetData.isMainAsset = IsInMainAsset(path);
                _assetDataDic.Add(path, assetData);
                assetDataList.Add(assetData);
            }
            return assetData;
        }

        /// <summary>
        /// 获取资源数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public NewAssetData GetAssetData(string path)
        {
            InitDic();
            if (_assetDataDic.ContainsKey(path))
            {
                return _assetDataDic[path];
            }
            return null;
        }

        /// <summary>
        /// 是否存在这资源
        /// </summary>
        /// <param name="path"></param>
        public bool Contains(string path)
        {
            InitDic();
            return _assetDataDic.ContainsKey(path);
        }

        /// <summary>
        /// 检测资源是否发生了改变
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>如果发生了改变，返回True，否则返回False</returns>
        public bool IsAssetChanged(string path)
        {
            InitDic();
            NewAssetData assetData;
            if (!_assetDataDic.TryGetValue(path, out assetData))
            {
                return true;
            }
            else
            {
                Hash128 hash = AssetDatabase.GetAssetDependencyHash(path);
                return !assetData.hash.Equals(hash.ToString());
            }
        }

        /// <summary>
        /// 是否主资源，动态资源
        /// </summary>
        /// <returns></returns>
        public bool IsInMainAsset(string asset)
        {
   
            return NewGAssetBuildConfig.Instance.IsInMainAsset(asset);
        }

        // 临时的
        private HashSet<string> _temMap = new HashSet<string>();
        /// <summary>
        /// 外部用 添加新的
        /// </summary>
        /// <param name="path">相对Assets底下</param>
        /// <returns></returns>
        public bool Add(string path)
        {
            if (Contains(path))
                return false;
            _temMap.Clear();
            UpdateBasicData(path, ref _temMap);
            EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        /// 外部用 刷新
        /// </summary>
        /// <param name="path">相对Assets底下</param>
        /// <returns></returns
        public void UpdateCache(string path)
        {
            _temMap.Clear();
            UpdateBasicData(path, ref _temMap);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 外部用 移除
        /// </summary>
        /// <param name="path">相对Assets底下</param>
        /// <returns></returns
        public void Remove(string path)
        {
            if (!Contains(path))
                return;
            var data = _assetDataDic[path];
            assetDataList.Remove(data);
            _assetDataDic.Remove(path);
            var count = assetDataList.Count;
            for (int i = 0; i < count; i++)
            {
                if (assetDataList[i].dependAssetList.Contains(path))
                {
                    assetDataList[i].dependAssetList.Remove(path);
                }
            }
            EditorUtility.SetDirty(this);
        }
    }
}
