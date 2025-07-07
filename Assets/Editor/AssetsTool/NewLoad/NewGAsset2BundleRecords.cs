using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using XGame.Utils;
using XGame.Asset.Load;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace XGameEditor
{
    [System.Serializable]
    public class EditorA2BRecord
    {
        public int index;
        public string assetPath;
        public string bundleName;
        public string assetName;
        public string[] dependArr;
        public bool isStaticAndInOnceBundle;
        
        public EditorA2BRecord(string assetPath, string bundleName, string assetName)
        {
            this.assetPath = assetPath;
            this.assetName = assetName;
            this.bundleName = bundleName;
        }
    }

    [System.Serializable]
    public class EditorAsset2BundleRecords
    {
        /// <summary>
        /// 记录列表
        /// </summary>
        public List<EditorA2BRecord> _recordList = new List<EditorA2BRecord>();
        public List<EditorA2BRecord> RecordList => _recordList;
        public int Count => _recordList.Count;

        /// <summary>
        /// 资源到AB的映射关系
        /// </summary>
        private Dictionary<string, EditorA2BRecord> _recordDic = new Dictionary<string, EditorA2BRecord>();


        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool Add(EditorA2BRecord record)
        {
            if (_recordDic.ContainsKey(record.assetPath))
            {
                //Debug.LogError($"尝试添加记录失败！存在重复资源路径资源：{record.assetPath}");
                return false;
            }
            record.index = _recordList.Count;
            _recordList.Add(record);
            _recordDic.Add(record.assetPath, record);
            return true;
        }

        public EditorA2BRecord GetRecord(string path)
        {
            RefreshDic();
            if (_recordDic.ContainsKey(path))
            {
                return _recordDic[path];
            }
            return null;
        }

        public void SetRecordList(List<EditorA2BRecord> list)
        {
            Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Add(list[i]);
            }
        }


        public void Clear()
        {
            _recordList.Clear();
            _recordDic.Clear();
        }

        public void RefreshDic()
        {
            if (_recordDic.Count == 0)
            {
                for (int i = 0; i < _recordList.Count; i++)
                {
                    var record = _recordList[i];
                    if (_recordDic.ContainsKey(record.assetPath))
                    {
                        Debug.LogError($"尝试刷新记录失败！存在重复资源路径资源：{record.assetPath}");
                        continue;
                    }
                    _recordDic.Add(record.assetPath, record);
                }
            }
        }
    }

    public class NewGAsset2BundleRecords : ScriptableObject
    {
        private static NewGAsset2BundleRecords _instance;


        //是否保留原始图集的名字
        public bool _keepOrgName = false;

        //全局实例
        public static NewGAsset2BundleRecords Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<NewGAsset2BundleRecords>(NewAssetDataDef.Asset2BundlePath);
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<NewGAsset2BundleRecords>();
                        NewAssetDataDef.CheckCreateDirectory(NewAssetDataDef.Asset2BundlePath);
                        AssetDatabase.CreateAsset(_instance, NewAssetDataDef.Asset2BundlePath);
                    }

                }
                return _instance;
            }
        }

        // 导出前简化版数据：lua简化只有一条记录，表格也是
        private static NewGAsset2BundleRecords _finalInstance;
        public static NewGAsset2BundleRecords FinalInstance
        {
            get
            {
                if (_finalInstance == null)
                {
                    _finalInstance = AssetDatabase.LoadAssetAtPath<NewGAsset2BundleRecords>(NewAssetDataDef.FinalAsset2BundlePath);
                    if (_finalInstance == null)
                    {
                        _finalInstance = ScriptableObject.CreateInstance<NewGAsset2BundleRecords>();
                        NewAssetDataDef.CheckCreateDirectory(NewAssetDataDef.FinalAsset2BundlePath);
                        AssetDatabase.CreateAsset(_finalInstance, NewAssetDataDef.FinalAsset2BundlePath);
                    }

                }
                return _finalInstance;
            }
        }


        // 打包用的配置
        private static NewGAsset2BundleRecords _buildABInstance;
        public static NewGAsset2BundleRecords BuildABInstance
        {
            get
            {
                if (_buildABInstance == null)
                {
                    _buildABInstance = AssetDatabase.LoadAssetAtPath<NewGAsset2BundleRecords>(NewAssetDataDef.BuildABAsset2BundlePath);
                    if (_buildABInstance == null)
                    {
                        _buildABInstance = ScriptableObject.CreateInstance<NewGAsset2BundleRecords>();
                        NewAssetDataDef.CheckCreateDirectory(NewAssetDataDef.BuildABAsset2BundlePath);
                        AssetDatabase.CreateAsset(_buildABInstance, NewAssetDataDef.BuildABAsset2BundlePath);
                    }

                }
                return _buildABInstance;
            }
        }

        /// <summary>
        /// 记录表
        /// </summary>
        public EditorAsset2BundleRecords allRecord = new EditorAsset2BundleRecords();

        /// <summary>
        /// 静态资源被依赖表
        /// </summary>
        private Dictionary<string, List<string>> _staticBeDependDic = new Dictionary<string, List<string>>();


        /// <summary>
        /// 获取包名
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public string GetBundleName(string assetPath)
        {
            var record = allRecord.GetRecord(assetPath);
            return record == null ? string.Empty : record.bundleName;
        }

        /// <summary>
        /// 格式化ab资源名或包名
        /// </summary>
        /// <param name="assetPath"></param>
        private string GetRelativeABName(string name)
        {
            return XGameEditorUtilityEx.RelativeToAssetsPath(name);
        }

        static public string _bundleSuffix = ".bin";
        /// <summary>
        /// 通过打包配置和缓存记录刷新ab名
        /// </summary>
        /// <param name="cache"></param>
        public void RefreshRecord()
        {
            NewGAssetBuildConfig config = NewGAssetBuildConfig.Instance;
            NewGAssetDataCache cache = NewGAssetDataCache.Instance;
            allRecord.Clear();
            _staticBeDependDic.Clear();
            HashSet<string> checkDependRecord = new HashSet<string>();
            Dictionary<string, List<string>> bundleAsset = new Dictionary<string, List<string>>();
            int assetCount = cache.assetDataList.Count;
            bool isCancel;
            var startTime = DateTime.Now;
            try
            {

                for (int i = 0; i < assetCount; i++)        // 遍历动态资源，添加到记录里面
                {
                    var data = cache.assetDataList[i];
                    var assetPath = data.assetPath;
                    var recordAssetPath = GetRelativeABName(assetPath);
                    if (data.isMainAsset)
                    {
                   
                        var bundleName = GetRelativeABName(config.GetAssetBundleName(assetPath)).Replace("\\", "/").ToLower() + _bundleSuffix;
                        var fileName = Path.GetFileNameWithoutExtension(assetPath);
                        var assetName = fileName.ToLower();

                        if (i % 100 == 0)
                        {
                            //显示处理进度
                            isCancel = EditorUtility.DisplayCancelableProgressBar($"动态资源：生成资源包名映射:({i}/{assetCount})", assetPath, i * 1.0f / assetCount);
                            //Debug.Log($"动态资源：生成资源包名映射:({i}/{assetCount})  {assetPath}");
                            //Debug.Log(path);
                            if (isCancel)
                            {
                                EditorUtility.ClearProgressBar();
                                break;
                            }
                        }
                        if (!bundleAsset.ContainsKey(bundleName))
                            bundleAsset.Add(bundleName, new List<string>());
                        if (bundleAsset[bundleName].Contains(assetName))
                        {
                            var newBundleName = recordAssetPath;


                            Debug.LogWarning($"同一个包，资源名字相同 更换资源名字：bundleName:{bundleName}, Old assetName:{assetName}, New assetName:{newBundleName}");

                            assetName = newBundleName.Replace("\\", "_").Replace("/", "_"); 

                            //var newBundleName = bundleName + "_" + Path.GetFileName(assetPath).ToLower().Replace('.', '_');

                            /*
                            if (bundleAsset.ContainsKey(newBundleName))
                            {
                                Debug.LogError($"动态资源中存在同包名且同资源名！改一下打包粒度或更换资源名：bundleName:{newBundleName}, assetName:{assetName}");
                                continue;
                            }
                            else
                            {
                                bundleName = newBundleName;
                                bundleAsset.Add(bundleName, new List<string>());
                            }
                            */

                        }
                        bundleAsset[bundleName].Add(assetName);
                        
                        // 加到记录内
                        var record = new EditorA2BRecord(recordAssetPath, bundleName, assetName);
                        allRecord.Add(record);

                    }
                    int dependCount = data.dependAssetList.Count;
                    for (int j = 0; j < dependCount; j++)
                    {
                        RefreshStaticDependRecord(cache, recordAssetPath, data.dependAssetList[j], ref checkDependRecord); // 静态资源记录一下被哪些动态资源依赖
                    }
                }
                Debug.Log($"动态资源：生成资源包名映射文件经过时间：startTime:{startTime}, endTime:{DateTime.Now}, 缓存总数:{assetCount}, 记录总数:{allRecord.Count}");

                // 检查循环依赖



                HashSet<string> needSetStaticBundleName = new HashSet<string>();
                // 静态资源处理
                startTime = DateTime.Now;
                int beDependCount = _staticBeDependDic.Count;
                int curDependIndex = 0;
                foreach (var dependRes in _staticBeDependDic)
                {
                    ++curDependIndex;
                    var name = dependRes.Key;
                    var beDepend = dependRes.Value;
                    var data = cache.GetAssetData(name);
                    if (data == null)
                    {
                        Debug.LogError($"从缓存中找不到：{name}，不应该出现这种情况！！！！");
                        continue;
                    }
                    if (curDependIndex % 100 == 0)
                    {
                        isCancel = EditorUtility.DisplayCancelableProgressBar($"静态资源生成资源包名映射:({curDependIndex}/{beDependCount})", name, curDependIndex * 1.0f / beDependCount);
                        //Debug.Log(path);
                        if (isCancel)
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                    }

                    // 静态资源且和动态资源打在一个包里，不需要导出最终记录，减少记录个数
                    bool isStaticAndInOnceBundle = false;
                    EditorA2BRecord record = null;
                    //Debug.Log($"静态资源:assetName:{name}, beDepend.Count:{beDepend.Count}, hasPackMode:{config.IsHasPackMode(name)}");
                    var assetPath = GetRelativeABName(name);



                   
                    var assetName = Path.GetFileNameWithoutExtension(assetPath).ToLower();

                    //判断引用的动态资源是否都来自一个AB包
                    bool onlyOneParentBundle = true;
                    bool isNeedWaitParent = false;
                    string lastBundleName = string.Empty;
                    foreach(var bedependchild in beDepend)
                    {
                        var dependRecord = allRecord.GetRecord(bedependchild);
                        if (dependRecord == null)
                        {
                            var parent = cache.GetAssetData("Assets/" + bedependchild);
                            //这是一个还未记录的静态资源|||
                            if (parent != null && !parent.isMainAsset)
                            {
                                needSetStaticBundleName.Add(data.assetPath);
                                isNeedWaitParent = true;
                            }
                            continue;
                        }

                        if (string.IsNullOrEmpty(lastBundleName))
                        {
                            lastBundleName = dependRecord.bundleName;
                        }
                        else if (lastBundleName.CompareTo(dependRecord.bundleName) != 0)
                        {
                            onlyOneParentBundle = false;
                            break;
                        }
                    }

                    string bundleName = string.Empty;
                    if (config.IsHasPackMode(name) || !onlyOneParentBundle || beDepend[0].Contains(".unity"))      // 有打包规则，或者被动态资源AB依赖大于1的，按打包规则设置包名
                    {
                        bundleName = GetRelativeABName(config.GetAssetBundleName(name)).Replace("\\", "/").ToLower() + _bundleSuffix;

                        if (!bundleAsset.ContainsKey(bundleName))
                            bundleAsset.Add(bundleName, new List<string>());


                        if (bundleAsset[bundleName].Contains(assetName))
                        {
                            /*
                            var newBundleName = bundleName + "_" + Path.GetFileName(assetPath).ToLower().Replace('.', '_');
                            newBundleName = newBundleName.Replace("\\", "/");
                            if (bundleAsset.ContainsKey(newBundleName))
                            {
                                Debug.LogError($"1静态资源中存在同包名且同资源名！改一下打包粒度或更换资源名：bundleName:{newBundleName}, assetName:{assetName}");
                            }
                            else
                            {
                                bundleName = newBundleName;
                                bundleAsset.Add(bundleName, new List<string>());
                                bundleAsset[bundleName].Add(assetName);
                            }
                            */

                            assetName = assetPath;
                            bundleAsset[bundleName].Add(assetName);
                        }
                        else
                        {
                            bundleAsset[bundleName].Add(assetName);
                        }
                    }
                    else//没有打包规则，且被一个资源依赖，和资源打到一个包里
                    {
                        var beDependAsset = beDepend[0];
                        var parent = cache.GetAssetData("Assets/" + beDependAsset);
                        if (isNeedWaitParent || (parent != null && !parent.isMainAsset))
                        {
                            //Debug.Log($"静态资源依赖 另一个静态资源，要改包名为被依赖包名：[{parent.assetPath}] -> [{data.assetPath}]");
                            if (!needSetStaticBundleName.Contains(data.assetPath))
                            {
                                needSetStaticBundleName.Add(data.assetPath);
                            }
                            continue;
                        }

                        
                        var srcRecord = allRecord.GetRecord(beDependAsset);
                        if (srcRecord == null)
                        {
                            Debug.LogError($"找不到依赖源数据：{beDependAsset}，不应该出现这种情况！！！！");
                            continue;
                        }
                        var parentBundleName = allRecord.GetRecord(beDependAsset).bundleName;

                        if (bundleAsset[parentBundleName].Contains(assetName))      // 动态资源包内已经存在相同资源名，自己单独打包吧
                        {
                            //bundleName = GetRelativeABName(config.GetAssetBundleName(name)).ToLower() + _bundleSuffix;


                            
                            Debug.LogWarning($"2静态资源中存在同包名且同资源名！更改名字：parentBundleName:{parentBundleName}, bundleName:{bundleName}, assetName:{assetName}， NewName:{assetPath}");
                            assetName = assetPath.Replace("\\", "_").Replace("/", "_"); 
                        }
                       // else
                       // {
                            bundleName = parentBundleName;      // 没有打包规则，且被一个动态资源依赖，和动态资源打到一个包里
                            isStaticAndInOnceBundle = true;
                       // }


                        if (!bundleAsset.ContainsKey(bundleName))
                            bundleAsset.Add(bundleName, new List<string>());
                        if (bundleAsset[bundleName].Contains(assetName))
                        {
                            Debug.LogError($"3静态资源中存在同包名且同资源名！改一下打包粒度或更换资源名：bundleName:{bundleName}, assetName:{assetName}");
                            continue;
                        }
                        bundleAsset[bundleName].Add(assetName);

                        //bundleName = GetRelativeABName(beDepend[0]);
                        //Debug.Log($"没有打包规则，且被一个动态资源依赖:assetName:{record.assetName}, bundle:{record.bundleName}");
                    }



                    bundleName = bundleName.Replace("\\", "/");
                    if (!bundleAsset.ContainsKey(bundleName))
                        bundleAsset.Add(bundleName, new List<string>());
                    if (!bundleAsset[bundleName].Contains(assetName))
                    {
                        bundleAsset[bundleName].Add(assetName);
                    }
                    record = new EditorA2BRecord(assetPath, bundleName, assetName);
                    record.isStaticAndInOnceBundle = isStaticAndInOnceBundle;

                    //Debug.Log($"【【静态资源依赖:assetPath:{assetPath}, assetName:{record.assetName}, bundle:{record.bundleName}");
                    allRecord.Add(record);
                }

                // 刷新
                int doCount = 0;
                while (true)
                {
                    ++doCount;

                    Debug.Log($"【刷新次数：{doCount}】");
                    if (needSetStaticBundleName.Count <= 0)
                    {
                        break;
                    }

                   

                    if (doCount > 10)
                    {
                        Debug.LogError("走了10遍了没刷新完静态资源，应该是有循环依赖了 需要排查的列表:");

                        foreach(var path in needSetStaticBundleName)
                        {
                            Debug.LogError("path=: "+ path);
                        }



                        break;
                    }
                    // 静态资源且和动态资源打在一个包里，不需要导出最终记录，减少记录个数
                    curDependIndex = 0; 
                    foreach (var dependRes in _staticBeDependDic)
                    {
                        bool isStaticAndInOnceBundle = false;
                        ++curDependIndex;
                        var name = dependRes.Key;

                        // 不在需要检查的列表直接继续
                        if (!needSetStaticBundleName.Contains(name))
                        {
                            continue;
                        }

                        /*
                        if (name.IndexOf("Blend_Comming")>=0)
                        {
                            Debug.Log($"【依赖预警：{name}】");
                        }
                        */

                        var beDepend = dependRes.Value;
                        var data = cache.GetAssetData(name);
                        if (data == null)
                        {
                            Debug.LogError($"2-{doCount}-从缓存中找不到：{name}，不应该出现这种情况！！！！");
                            continue;
                        }

                        // 依赖资源还没记录好 跳过
                        var beDependAsset = beDepend[0];
                        if (allRecord.GetRecord(beDependAsset) == null)
                        {
                            continue;
                        }

                        EditorA2BRecord record = null;
                        //Debug.Log($"静态资源:assetName:{name}, beDepend.Count:{beDepend.Count}, hasPackMode:{config.IsHasPackMode(name)}");
                        var assetPath = GetRelativeABName(name);



                        var assetName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                        string bundleName = string.Empty;
                        var parent = cache.GetAssetData("Assets/" + beDependAsset);

                        if(null== parent&&beDependAsset.IndexOf("Packages/")>=0)
                        {
                            parent = cache.GetAssetData(beDependAsset);
                        }

                        if (parent == null)
                        {
                            continue;
                        }
                        var parentBundle = allRecord.GetRecord(beDependAsset);
                        var parentBundleName = parentBundle.bundleName;

                        if (!bundleAsset.ContainsKey(parentBundleName))
                        {
                            bundleAsset.Add(parentBundleName, new List<string>());
                            //Debug.LogError($"-{doCount}-【【这个静态资源 父AB包没记录!!!parentBundleName:{parentBundleName}, assetName:{assetName}");
                        }
                        if (bundleAsset[parentBundleName].Contains(assetName))      // 动态资源包内已经存在相同资源名，自己单独打包吧
                        {

                            Debug.LogWarning($"6-{doCount}-静态资源中存在同包名且同资源名！改资源名字：parentBundleName:{parentBundleName}, bundleName:{bundleName}, assetName:{assetName}, NewName:{assetPath}");

                            assetName = assetPath.Replace("\\", "_").Replace("/","_");
                            /*
                            var checkParentBundle = parentBundle;
                            while (true)    // 循环父对象都设isStaticAndInOnceBundle 为false
                            {
                                if (checkParentBundle == null || !checkParentBundle.isStaticAndInOnceBundle)
                                    break;
                                checkParentBundle.isStaticAndInOnceBundle = false;
                                if (!bundleAsset.ContainsKey(checkParentBundle.bundleName) || bundleAsset[checkParentBundle.bundleName].Count == 0)
                                {
                                    break;
                                }
                                checkParentBundle = allRecord.GetRecord(bundleAsset[checkParentBundle.bundleName][0]);
                                
                            }
                            bundleName = GetRelativeABName(config.GetAssetBundleName(name)).ToLower() + _bundleSuffix;
                            Debug.LogWarning($"6-{doCount}-静态资源中存在同包名且同资源名！单独打一个包：parentBundleName:{parentBundleName}, bundleName:{bundleName}, assetName:{assetName}");
                            */
                        }
                        // else

                        //打包策略里面有指定的，优先打包策略指定的包名
                        if(config.IsHasPackMode(name))
                        {
                            bundleName = GetRelativeABName(config.GetAssetBundleName(name)).Replace("\\", "/").ToLower() + _bundleSuffix;
                        }else
                        {
                            //没有打包策略指定的，使用依赖者的包名
                            //判断引用的动态资源是否都来自一个AB包
                            bool onlyOneParentBundle = true;
                            bool isNeedWaitParent = false;
                            string lastBundleName = string.Empty;
                            foreach (var bedependchild in beDepend)
                            {
                                var dependRecord = allRecord.GetRecord(bedependchild);
                                if (dependRecord == null)
                                {
                                    var dependParent = cache.GetAssetData("Assets/" + bedependchild);
                                    //这是一个还未记录的静态资源，这里会死循环也就是说有静态资源的多份循环依赖
                                    if (dependParent != null && !dependParent.isMainAsset)
                                    {
                                        //跳过自己
                                        if (data.assetPath == dependParent.assetPath)
                                        {
                                            continue;
                                        }

                                        isNeedWaitParent = true;
                                        break;
                                    }
                                    else
                                    {
                                        Debug.LogError("检查动态资源是否添加了记录");
                                    }
                                    continue;
                                }

                                if (string.IsNullOrEmpty(lastBundleName))
                                {
                                    lastBundleName = dependRecord.bundleName;
                                }
                                else if (lastBundleName.CompareTo(dependRecord.bundleName) != 0)
                                {
                                    onlyOneParentBundle = false;
                                    break;
                                }
                            }
                            if(isNeedWaitParent)
                            {
                                continue;
                            }
                            if(onlyOneParentBundle)
                            {
                                bundleName = parentBundleName;      // 没有打包规则，且被一个资源依赖，和这个资源打到一个包里
                                isStaticAndInOnceBundle = true;
                            }
                            else
                            {
                                //再构建bundle名称
                                bundleName = GetRelativeABName(config.GetAssetBundleName(name)).Replace("\\", "/").ToLower() + _bundleSuffix;
                            }
                        }

                        bundleName = bundleName.Replace("\\", "/");
                        if (!bundleAsset.ContainsKey(bundleName))
                            bundleAsset.Add(bundleName, new List<string>());
                        if (bundleAsset[bundleName].Contains(assetName))
                        {
                            needSetStaticBundleName.Remove(name);
                            var newBundleName = bundleName + "_" + Path.GetFileName(assetPath).ToLower().Replace('.', '_');
                            if (bundleAsset.ContainsKey(newBundleName))
                            {
                                Debug.LogError($"7-{doCount}-静态资源中存在同包名且同资源名！改一下打包粒度或更换资源名：bundleName:{bundleName}, assetName:{assetName}");
                                continue;
                            }
                            else
                            {
                                bundleName = newBundleName;
                                bundleAsset.Add(bundleName, new List<string>());
                            }
                        }
                        bundleAsset[bundleName].Add(assetName);

                        //bundleName = GetRelativeABName(beDepend[0]);
                        //Debug.Log($"没有打包规则，且被一个动态资源依赖:assetName:{record.assetName}, bundle:{record.bundleName}");
                        record = new EditorA2BRecord(assetPath, bundleName.Replace("\\", "/"), assetName);
                        //Debug.Log($"8-{doCount}-静态资源依赖:assetPath:{assetPath}, assetName:{record.assetName}, bundle:{record.bundleName}");
                        record.isStaticAndInOnceBundle = isStaticAndInOnceBundle;
                        allRecord.Add(record);
                        needSetStaticBundleName.Remove(name);
                    }
                }
                Debug.Log($"静态资源：生成资源包名映射文件经过时间：startTime:{startTime}, endTime:{DateTime.Now}, 缓存总数:{assetCount}, 记录总数:{allRecord.Count}");



                //////////////////////////   //合并非主资源,都是单独的包，

                MergeBundle(bundleAsset);


                //////////////////////////   //合并非主资源,都是单独的包，


                //重刷一次ab名稱
                // RefreshRecordBundle();

                // 刷新依赖表
                RefreshRecordDepend();


              
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.SetDirty(this);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }


        // 刷新依赖表
        public void MergeBundle(Dictionary<string, List<string>> bundleAsset)
        {
            NewGAssetBuildConfig config = NewGAssetBuildConfig.Instance;

            List<string> depPaths = null;

            var count = allRecord.RecordList.Count;
            for (int i = 0; i < count; i++)
            {
                EditorA2BRecord record = allRecord.RecordList[i];
                var path = "Assets/" + record.assetPath;

                //不是图片资源
                if (path.EndsWith(".png")==false)
                {
                    continue;
                }

                //是主资源
                if(config.IsInMainAsset(path))
                {
                    continue;
                }

                //不是单独打包的
                if(bundleAsset.ContainsKey(record.bundleName)==false|| bundleAsset[record.bundleName].Count>1)
                {
                    continue;
                }

                //找到被依赖的资源
                if(_staticBeDependDic.TryGetValue(path, out depPaths))
                {
                   
                    int nCount = depPaths.Count;
                    //没有依赖，直接跳过
                    if(nCount==0)
                    {
                        continue;
                    }

                    //检测是否都是一个依赖
                    bool onlyOne = true;
                    for (int j=0;j<nCount;++j)
                    {
                        EditorA2BRecord beDepRecord = allRecord.GetRecord(depPaths[j]);
                        if (bundleAsset.ContainsKey(beDepRecord.bundleName) == false || bundleAsset[beDepRecord.bundleName].Count > 1)
                        {
                            onlyOne = false;
                            break;
                        }
                    }

                    //改变包名
                    if(onlyOne)
                    {
                        List<string> assetsList = bundleAsset[record.bundleName];
                        for (int j = 0; j < nCount; ++j)
                        {
                            EditorA2BRecord beDepRecord = allRecord.GetRecord(depPaths[j]);

                            var resPath = "Assets/" + beDepRecord.assetPath;
                            //是主资源
                            if (config.IsInMainAsset(resPath))
                            {
                                continue;
                            }


                            Debug.LogWarning("合并 bundle的资源 assetPath="+ beDepRecord.assetPath+ ",bundleName="+ beDepRecord.bundleName);

                            bundleAsset.Remove(beDepRecord.bundleName);
                            assetsList.Add(beDepRecord.assetName);
                            beDepRecord.bundleName = record.bundleName;


                            if(assetsList.Contains(beDepRecord.assetName))
                            {
                                beDepRecord.assetName = beDepRecord.bundleName + "_" + beDepRecord.assetName + "_" +j;
                            }
                            assetsList.Add(beDepRecord.assetName);

                           

                        }
                    }
                }
               




            }
        }

        //刷新Bundle的配置規則
        public void RefreshRecordBundle()
        {
            NewGAssetBuildConfig config = NewGAssetBuildConfig.Instance;

            var count = allRecord.RecordList.Count;
            for (int i = 0; i < count; i++)
            {
                var record = allRecord.RecordList[i];
                var path = "Assets/" + record.assetPath;


                if (path.IndexOf(".shader") >= 0)
                {
                    Debug.LogError(path);
                }

                if (config.IsHasPackMode(path))
                {
                    var bundleName = GetRelativeABName(config.GetAssetBundleName(path)).Replace("\\", "/").ToLower() + _bundleSuffix;
                    if(record.bundleName!= bundleName)
                    {
                        record.bundleName = bundleName;
                    }
                }
                    
            }
        }

        /// <summary>
        /// 刷新依赖列表
        /// </summary>
        public void RefreshRecordDepend()
        {
            try
            {

                List<string> dependList = new List<string>();
                var count = allRecord.RecordList.Count;
                for (int i = 0; i < count; i++)
                {
                    var record = allRecord.RecordList[i];
                    var path = "Assets/" + record.assetPath;
                    if (i % 100 == 0)
                    {
                        EditorUtility.DisplayCancelableProgressBar($"刷新依赖列表:({i}/{count})", record.assetPath, i * 1.0f / count);
                    }


                    dependList.Clear();
                    //获取依赖列表
                    string[] dps = AssetDatabase.GetDependencies(path, false);
                    foreach (var dp in dps)
                    {
                        if (NewAssetDataDef.IsValidAsset(dp))
                        {
                            if (NewGAssetBuildConfig.Instance.CanBeDependent(dp))
                            {
                                dependList.Add(GetRelativeABName(dp));
                            }
                            else
                            {
                                Debug.Log("dependAssetList 跳过不能被静态依赖的资源" + dp);
                            }

                            
                        }
                    }

                    //删除自身依赖
                    if(dependList.Remove(record.assetPath))
                    {
                        Debug.Log("删除自身依赖 assetPath=" + record.assetPath);
                    }

                    record.dependArr = dependList.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"刷新依赖列表异常：{e}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 添加静态资源记录
        /// </summary>
        private void RefreshStaticDependRecord(NewGAssetDataCache cache, string parentPath, string dependPath, ref HashSet<string> checkDependRecord)
        {
            if (checkDependRecord.Contains($"{dependPath}===={parentPath}"))
            {
                return;
            }
            checkDependRecord.Add($"{dependPath}===={parentPath}");
            var data = cache.GetAssetData(dependPath);
            if (data == null)
            {
                Debug.LogError($"依赖资源在缓存中找不到！！！parentPath:{parentPath}, dependPath:{dependPath}");
                return;
            }
            if (data.isMainAsset)
            {
                return;
            }

            if (!_staticBeDependDic.ContainsKey(dependPath))
            {
                _staticBeDependDic.Add(dependPath, new List<string>());
            }

            if (_staticBeDependDic[dependPath].Contains(parentPath))
            {
                return;
            }

            //跳过自身
            if(dependPath!= parentPath)
            {
                _staticBeDependDic[dependPath].Add(parentPath);
            }
            else
            {
                Debug.Log("跳过静态依赖自身的资源: dependPath= " + dependPath);
            }
                

            // 检查子依赖
            int subDependCount = data.dependAssetList.Count;
            for (int i = 0; i < subDependCount; i++)
            {
                if(dependPath.StartsWith("Assets/"))
                {
                    RefreshStaticDependRecord(cache, dependPath.Substring("Assets/".Length), data.dependAssetList[i], ref checkDependRecord);
                }else
                {
                    RefreshStaticDependRecord(cache, dependPath, data.dependAssetList[i], ref checkDependRecord);
                }
               
            }
        }

        private void SyncAsset2BundleRecordsHash(NewGAsset2BundleRecords src, NewGAsset2BundleRecords des)
        {
            var count = des.allRecord.Count;
            for (int i = 0; i < count; i++)
            {
                var desRecord = des.allRecord._recordList[i];
                var srcRecord = src.allRecord.GetRecord(desRecord.assetPath);
                if (srcRecord != null)
                {
                    desRecord.assetName = srcRecord.assetName;
                    desRecord.bundleName = srcRecord.bundleName;
                }
                else
                {
                    Debug.LogError($"设置Hash异常，找不到：{desRecord.assetPath}");
                }
            }
            EditorUtility.SetDirty(des);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 导出映射文件
        /// </summary>
        /// <param name="path"></param>
        public void Export(string path = null)
        {
            NewExport(path);
            return;

            SimplifyAsset2BundleRecords(allRecord, FinalInstance);


            if(_keepOrgName)
            {
                BuildABInstance.allRecord._recordList = new List<EditorA2BRecord>(Instance.allRecord.RecordList);
            }else
            {
                SimplifyAsset2BundleRecordsHash(Instance, BuildABInstance);
            }


            SyncAsset2BundleRecordsHash(BuildABInstance, FinalInstance);
            var records = ExportEditorAsset2BundleRecords(FinalInstance.allRecord);

            //var records = ExportEditorAsset2BundleRecords(allRecord);
            //return;
            BinaryFormatter binFormat = new BinaryFormatter();
            Stream fStream = null;
            //保存数据
            string filePath = NewAssetDataDef.Asset2BundleDataWindowPath;
            //Window的可能不存在
            if (File.Exists(filePath))
            {
                fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                binFormat.Serialize(fStream, records);
                fStream.Close();
            }

            //Andorid 先删了再创建，避免FileStream写入前面 后面原来的数据没清掉
            filePath = NewAssetDataDef.Asset2BundleDataAndroidPath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                //File.Create(filePath);
            }
            fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            binFormat.Serialize(fStream, records);
            fStream.Close();
            EditorUtility.ClearProgressBar();
            Debug.Log($"导出映射文件:{filePath}");
        }

        /// <summary>
        /// 导出前简化版数据：lua简化只有一条记录，表格也是
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsNeedExportDirList(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return false;
            }
            var dontExportDirList = NewGAssetBuildConfig.Instance.dontExportDirList;
            var needExportFileList = NewGAssetBuildConfig.Instance.needExportDirList;
            for (int i = 0; i < dontExportDirList.Count; i++)
            {
                if (file.StartsWith(dontExportDirList[i]))
                {
                    for (int j = 0; j < needExportFileList.Count; j++)
                    {
                        if (file.StartsWith(needExportFileList[j]))
                        {
                            return true;
                        }
                    }
                    return false;
                } 
            }

            return true;
        }

        /// <summary>
        /// 简化记录
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        public void SimplifyAsset2BundleRecords(EditorAsset2BundleRecords src, NewGAsset2BundleRecords des)
        {
            des.allRecord.Clear();
            var count = src.RecordList.Count;
            for (int i = 0; i < count; i++)
            {
                var editorRecord = src.RecordList[i];
                if (IsNeedExportDirList(editorRecord.assetPath) && !editorRecord.isStaticAndInOnceBundle)
                {
                    var newRecord = new EditorA2BRecord(editorRecord.assetPath, editorRecord.bundleName, editorRecord.assetName);

                    HashSet<string> newDependList = new HashSet<string>();

                    FindStaticDepend(src, editorRecord, newDependList);

                    newRecord.dependArr = newDependList.ToArray();

                    des.allRecord.Add(newRecord);
                }
            }
            EditorUtility.SetDirty(des);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void FindStaticDepend(EditorAsset2BundleRecords src, EditorA2BRecord editorRecord, HashSet<string> newRecordDependList)
        {
            var dependList = editorRecord.dependArr;
            for (int j = 0; j < dependList.Length; j++)
            {
                var assetPath = dependList[j];
                var dependRecord = src.GetRecord(assetPath);

                //调整自身的依赖
                if(assetPath== editorRecord.assetPath)
                {
                    continue;
                }

                /*
                if (assetPath.IndexOf("Blend_Comming") >=0)
                {
                    Debug.LogError(assetPath);
                }
                */


                //Debug.Log($"查找依賴isStaticAndInOnceBundle， editorRecord.assetPath：{editorRecord.assetPath}, dependRecord:{dependRecord?.assetPath}, isStaticAndInOnceBundle:{dependRecord?.isStaticAndInOnceBundle}");
                if (dependRecord == null || dependRecord.isStaticAndInOnceBundle)
                {
                    //这个资源为静态且未被其他引用时，把这个资源的依赖加进来
                    FindStaticDepend(src, dependRecord, newRecordDependList);
                }
                else
                {
                    newRecordDependList.Add(assetPath);
                }
            }
        }

        public class EditorA2BRecordCompaer : IEqualityComparer<EditorA2BRecord>
        {
            public bool Equals(EditorA2BRecord x, EditorA2BRecord y) { return x.assetPath.CompareTo(y) == 0; }

            public int GetHashCode(EditorA2BRecord obj) { return obj.assetPath.GetHashCode(); }
        }

        /// <summary>
        /// 简化记录
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        public bool SimplifyAsset2BundleRecordsHash(NewGAsset2BundleRecords src, NewGAsset2BundleRecords des)
        {
            Dictionary<string, List<EditorA2BRecord>> assetBundleBulidDic = new Dictionary<string, List<EditorA2BRecord>>();
            Dictionary<string, HashSet<EditorA2BRecord>> assetBundleBulidHashsetDic = new Dictionary<string, HashSet<EditorA2BRecord>>();
            EditorA2BRecordCompaer compaer = new EditorA2BRecordCompaer();
            List<AssetBundleBuild> assetBundleBulidList = new List<AssetBundleBuild>();
            HashSet<string> resultRecord = new HashSet<string>();

            Dictionary<string, string> assetBundlehNameHashDic = new Dictionary<string, string>();
            string GetABNameHash(string inputStr)
            {
                if (assetBundlehNameHashDic.ContainsKey(inputStr))
                {
                    return assetBundlehNameHashDic[inputStr];
                }
                else
                {
                    var result = A2BRecord.GetHashCodeString(inputStr);
                    assetBundlehNameHashDic[inputStr] = result;
                    return result;
                }
            }

            des.allRecord._recordList = new List<EditorA2BRecord>(src.allRecord.RecordList);
            // 构建ab包
            var recordList = des.allRecord.RecordList;
            int count = recordList.Count;
            bool isCancel = false;
            for (int i = 0; i < count; i++)
            {
                var record = recordList[i]; 
                string abName = GetABNameHash(record.bundleName);
                if (i % 100 == 0)
                {
                    isCancel = EditorUtility.DisplayCancelableProgressBar($"简化记录为Hash: ({i} / {count})", $"{record.assetPath}", (float)i / count);
                    if (isCancel)
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                }

                if (!assetBundleBulidDic.ContainsKey(abName))
                {
                    assetBundleBulidDic.Add(abName, new List<EditorA2BRecord>());
                    assetBundleBulidHashsetDic.Add(abName, new HashSet<EditorA2BRecord>(compaer));
                }
                var a2bList = assetBundleBulidHashsetDic[abName];
                if (a2bList.Contains(record))
                {
                    Debug.LogError($"添加AB包异常，映射表居然存在两个引用相同的记录对象！！！！assetName:{record.assetName}");
                    continue;
                }
                bool isFaild = false;
                foreach (var item in a2bList)
                {
                    if (item.assetName == record.assetName)
                    {
                        isFaild = true;
                        Debug.LogError($"同一个包中存在多个同名资源，改一下名字或者打包规则：bundle:{abName}, assets:{record.assetName}");
                        break;
                    }
                    if (item.assetPath.EndsWith(".unity"))
                    {
                        Debug.LogError($"存在和场景文件同一个AB包的资源，bundle:{abName}, asset:{record.assetPath}");
                        return false;
                    }
                }
                if (isFaild)
                {
                    continue;
                }
                resultRecord.Add(record.assetPath);
                assetBundleBulidDic[abName].Add(record);
            }
            if (isCancel)
            {
                EditorUtility.ClearProgressBar();
                return false;
            }




            foreach (var item in assetBundleBulidDic)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var record = item.Value[i];
                    // record.assetName = A2BRecord.GetInt32String(i);
                    record.bundleName = item.Key + _bundleSuffix;
                }
            }
            

            ExportInvertedABRecord(assetBundlehNameHashDic);
            EditorUtility.SetDirty(des);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        //导出AB包名反向索引表
        private void ExportInvertedABRecord(Dictionary<string, string> assetBundlehNameHashDic)
        {
            Dictionary<string, string> dic = assetBundlehNameHashDic.ToDictionary(x => x.Value, y => y.Key);

            BinaryFormatter binFormat = new BinaryFormatter();
            Stream fStream = null;
            //保存数据
            string filePath = NewAssetDataDef.AssetBundleNameWindowPath;
            if (File.Exists(filePath)) { File.Delete(filePath); }

            NewAssetDataDef.CheckCreateDirectory(filePath);

            fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            binFormat.Serialize(fStream, dic);
            fStream.Close();

            filePath = NewAssetDataDef.AssetBundleNameAndroidPath;
            if (File.Exists(filePath)) { File.Delete(filePath); }
            NewAssetDataDef.CheckCreateDirectory(filePath);

            fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            binFormat.Serialize(fStream, dic);
            fStream.Close();
        }

        /// <summary>
        /// 编辑器记录数据导出到实际存储数据
        /// </summary>
        /// <param name="allRecord"></param>
        /// <returns></returns>
        public Asset2BundleRecords ExportEditorAsset2BundleRecords(EditorAsset2BundleRecords allRecord)
        {
            Asset2BundleRecords result = new Asset2BundleRecords();
            //result.dicAsset2Bundle = new Dictionary<int, A2BRecord>();
            //var count = allRecord.RecordList.Count;
            //for (int i = 0; i < count; i++)
            //{
            //    var editorRecord = allRecord.RecordList[i];
            //    var record = new A2BRecord(editorRecord.bundleName, editorRecord.assetName);
            //    List<int> dependHashList = new List<int>();
            //    for (int j = 0; j < editorRecord.dependArr.Length; j++)
            //    {
            //        var dependName = editorRecord.dependArr[j];
            //        dependHashList.Add(A2BRecord.GetHashCode(dependName));
            //    }
            //    record.dependArr = dependHashList.ToArray();
            //    var hash = A2BRecord.GetHashCode(editorRecord.assetPath);
            //    //Debug.Log($"导出Hash计值：hash:{hash}, path:[{editorRecord.assetPath}]");

            //    if (!result.dicAsset2Bundle.ContainsKey(hash))
            //    {
            //        result.dicAsset2Bundle.Add(hash, record);
            //    }
            //    else
            //    {
            //        Debug.LogError($"导出AB映射异常！存在资源路径Hash计算出来的值有相同：hash:{hash}, path1:[{editorRecord.assetPath}], path2:[{result.dicAsset2Bundle[hash].assetName}]");
            //    }
            //}
            return result;
        }

        class DepencyNode
        {
            public string bundleName;
            public HashSet<string> listDepencys = null;
        }

        Dictionary<string, string> dicPath2Bundle = new Dictionary<string, string>();
        Dictionary<string, EditorA2BRecord> dicPath2Record = new Dictionary<string, EditorA2BRecord>();

        private void __BuldDepencyTree(Dictionary<string, DepencyNode> dicDepencyTree, EditorA2BRecord record)
        {
            DepencyNode node = null;
            EditorA2BRecord depencyRecord = null;


            if (dicDepencyTree.TryGetValue(record.bundleName, out node) == false)
            {
                node = new DepencyNode();
                node.bundleName = record.bundleName;
                dicDepencyTree.Add(record.bundleName, node);
            }

            if (record.dependArr != null && record.dependArr.Length > 0)
            {
                /*
                if (record.bundleName.IndexOf("g_artist/shader") >= 0)
                {
                    Debug.LogError(record.bundleName);
                }
                */

                if (node.listDepencys == null)
                {
                    node.listDepencys = new HashSet<string>();
                }
                int nLen = record.dependArr.Length;
                for (int j = 0; j < nLen; ++j)
                {
                    depencyRecord = dicPath2Record[record.dependArr[j]];
                    if (node.listDepencys.Contains(depencyRecord.bundleName)==false)
                    {
                        node.listDepencys.Add(depencyRecord.bundleName);
                    }
                }
            }
        }

        private bool  __IsBundleDepencySelf(Dictionary<string, DepencyNode> dicDepencyTree, string bundleName, HashSet<string> dependcys, List<string> dependcyPaths)
        {
            bool bRet = false;

            //没有依赖项的，跳过
            DepencyNode node = dicDepencyTree[bundleName];
            if(node.listDepencys!=null&& node.listDepencys.Count>0)
            {
                //添加到依赖列表
                if (dependcys.Contains(bundleName)==false)
                {
                    dependcys.Add(bundleName);
                }


                foreach (var v in node.listDepencys)
                {
                    if (bundleName!=v)
                    {
                        //ab 包含在父类的ab包中
                        if (dependcys.Contains(v))
                        {
                            dependcyPaths.Add(v);
                            //Debug.LogError("存在循环依赖:" + bundleName);
                            return true;
                        }

                        //查找子依赖

                        HashSet<string> dependcysNew = new HashSet<string>(dependcys);
                        List<string> dependcyPathsNew = new List<string>(dependcyPaths);
                        dependcyPathsNew.Add(v);

                        if(__IsBundleDepencySelf(dicDepencyTree, v, dependcysNew, dependcyPathsNew))
                        {
                            string depencyPaths = "循环依赖路径:  ";
                            int nCount = dependcyPathsNew.Count;
                            for(int i=0;i<nCount; ++i)
                            {
                                depencyPaths +=dependcyPathsNew[i]+"->";
                            }

                            //Debug.LogError("开始打印依赖路径：");
                            Debug.LogError(depencyPaths);
                            //Debug.LogError("打印依赖路径结束");
                            bRet = true;
                        }
                        
                    }
                      
                }

                //删除本bundleName
                dependcys.Remove(bundleName);
            }
           

            return bRet;
        }

        public void __CheckDepency(List<EditorA2BRecord> listRecord)
        {

            dicPath2Bundle.Clear();
            dicPath2Record.Clear();

            for (int i=0;i< listRecord.Count;++i)
            {
                dicPath2Bundle.Add(listRecord[i].assetPath, listRecord[i].bundleName);
                dicPath2Record.Add(listRecord[i].assetPath, listRecord[i]);
            }

            Dictionary<string, DepencyNode> dicDepencyTree= new Dictionary<string, DepencyNode>();
            EditorA2BRecord record = null;

            //构建依赖树
            for (int i = 0; i < listRecord.Count; ++i)
            {
                record = listRecord[i];
                __BuldDepencyTree(dicDepencyTree,record);
            }

            //检测是否循环依赖
            HashSet<string> dependcys = new HashSet<string>();
            List<string> dependcyPaths = new List<string>();
            foreach (var v in dicDepencyTree.Values)
            {
                dependcys.Clear();
                dependcyPaths.Clear();
                if (__IsBundleDepencySelf(dicDepencyTree, v.bundleName, dependcys, dependcyPaths))
                {
                    Debug.LogError("起始检测bundleName=: " + v.bundleName);
                }
            }


        }

        public void NewExport(string path)
        {
            SimplifyAsset2BundleRecords(allRecord, FinalInstance);

            //检测是否循环依赖
            __CheckDepency(FinalInstance.allRecord.RecordList);

            if (_keepOrgName)
            {
                BuildABInstance.allRecord._recordList = new List<EditorA2BRecord>(Instance.allRecord.RecordList);
            }
            else
            {
                SimplifyAsset2BundleRecordsHash(Instance, BuildABInstance);
            }

            SyncAsset2BundleRecordsHash(BuildABInstance, FinalInstance);
            string filePathWindow = NewAssetDataDef.Asset2BundleDataWindowPath;
            string filePathAndroid = NewAssetDataDef.Asset2BundleDataAndroidPath;

            //缓存，中途取消时不影响之前生成的索引表
            string tempCachePath = filePathWindow + ".cache";

            NewExportEditorAsset2BundleRecords(FinalInstance.allRecord, tempCachePath);
            if (File.Exists(tempCachePath))
            {
                if (File.Exists(filePathWindow)) { File.Delete(filePathWindow); }
                if (File.Exists(filePathAndroid)) { File.Delete(filePathAndroid); }
                
                File.Copy(tempCachePath, filePathWindow);
                File.Copy(tempCachePath, filePathAndroid);
                if(null!= path)
                {
                    if (File.Exists(path)) { File.Delete(path); }

                    NewAssetDataDef.CheckCreateDirectory(path);
                    File.Copy(tempCachePath, path);
                }
               

                File.Delete(tempCachePath);
            }

            Debug.Log($"导出映射文件:{filePathWindow}, {filePathAndroid}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public bool AddDepencysAB(string path, Dictionary<string, EditorA2BRecord> dicPathRecord, Dictionary<string, string> dicABs,int nLayer)
        {
            //超过10层，当作已经添加，字体会循环依赖
            if(++nLayer>10)
            {
                Debug.LogWarning("优化的依赖超过10层： path="+ path);
                return true;
            }

            bool bRet = false;

            EditorA2BRecord editorRecord = null;
            if (dicPathRecord.TryGetValue(path, out editorRecord))
            {
                //判断自己的ab是否能够附加
                if (dicABs.ContainsKey(editorRecord.bundleName) == false)
                {
                    dicABs.Add(editorRecord.bundleName, editorRecord.bundleName);
                    bRet = true;
                }

                //计算依赖项的ab
                string[] deparr = editorRecord.dependArr;
                for (int i = 0; i < deparr.Length; ++i)
                {
                    bRet |= AddDepencysAB(deparr[i], dicPathRecord, dicABs,nLayer);
                }
            }


           return bRet;
        }

        int opIndex = 0;
        List<string> listDep = new List<string>();
        Dictionary<string, string> dicABs = new Dictionary<string, string>();
        public string[] ParseDependcys(Dictionary<string, EditorA2BRecord> dicPathRecord, string mainPath,string mainABName, string[] deparr)
        {
           
            if(deparr.Length==0)
            {
                return deparr;
            }

            dicABs.Clear();
            listDep.Clear();
            dicABs.Add(mainABName, mainABName);
            EditorA2BRecord editorRecord = null;
            for (int i=0;i< deparr.Length;++i)
            {
                if(dicPathRecord.TryGetValue(deparr[i], out editorRecord))
                {

                    if(AddDepencysAB(deparr[i], dicPathRecord, dicABs,0))
                    {
                        listDep.Add(deparr[i]);
                    }else
                    {
                        Debug.LogWarning("优化下标："+ ++opIndex + "，优化掉的依赖资源：resPath= " + mainPath + " , depRes= " + deparr[i]);
                    }

                    //没有包含包的，放进列表
                    /*
                    if(dicABs.ContainsKey(editorRecord.bundleName) ==false)
                    {
                        dicABs.Add(editorRecord.bundleName, editorRecord.bundleName);
                        listDep.Add(deparr[i]);
                    }else
                    {
                        //已经包含了在这个包了，判断这个资源依赖是否为0
                        if(editorRecord.dependArr!=null&&editorRecord.dependArr.Length==0)
                        {
                            listDep.Add(deparr[i]);
                        }else
                        {
                            //跳过依赖资源为0，但是ab包又被包含的资源
                            Debug.LogWarning("优化掉的依赖资源：resPath= " + mainPath + " , depRes= " + deparr[i]);
                        }

                    }
                    */
                }else
                {
                    Debug.LogError("没有找到资源的 ab包名 + path = "+ deparr[i]);
                }
                
            }

            return listDep.ToArray();
        }


        public void NewExportEditorAsset2BundleRecords(EditorAsset2BundleRecords allRecord, string filePath)
        {
            if (File.Exists(filePath)) { File.Delete(filePath); }

            var filePath1 = filePath + "1.cache";
            var filePath2 = filePath + "2.cache";
            var filePath3 = filePath + "3.cache";
            if (File.Exists(filePath1)) { File.Delete(filePath1); }
            if (File.Exists(filePath2)) { File.Delete(filePath2); }
            if (File.Exists(filePath3)) { File.Delete(filePath3); }

            //过滤特殊资源不导出索引表
            NewGAssetBuildConfig config = NewGAssetBuildConfig.Instance;
            List<EditorA2BRecord> recordList = new List<EditorA2BRecord>();
            List<EditorA2BRecord>  orgList = allRecord.RecordList;
            bool bSpecial = false;

            Dictionary<string, string> dicPathABs = new Dictionary<string, string>();
            Dictionary<string, EditorA2BRecord> dicPathRecord = new Dictionary<string, EditorA2BRecord>();

            for (int i = 0; i < orgList.Count; ++i)
            {
                bSpecial = false;
                foreach (string inOnePath in config.resInOnePackList)
                {
                    if(orgList[i].assetPath.IndexOf(inOnePath)>=0)
                    {
                        bSpecial = true;
                        break;
                    }
                }

                if(bSpecial==false)
                {
                    recordList.Add(orgList[i]);
                }

                //建立path->ABName索引
                dicPathABs.Add(orgList[i].assetPath, orgList[i].bundleName);
                //建立 path->EditorA2BRecord 索引
                dicPathRecord.Add(orgList[i].assetPath, orgList[i]);

            }


            //添加特殊表
            foreach (string inOnePath in config.resInOnePackList)
            {
                string pathName = GetRelativeABName(config.GetAssetBundleName(inOnePath)).Replace("\\", "/") + _bundleSuffix;
                string bundleName = A2BRecord.GetHashCodeString(pathName)+ _bundleSuffix;

                var specialRecord = new EditorA2BRecord(pathName, bundleName, bundleName);
                specialRecord.dependArr = new string[0];
                recordList.Add(specialRecord);
            }



            //最终输出 文件索引区起始位置(long) + 资源记录区起始位置(long) + 3个区
            var fStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var bw = new BinaryWriter(fStream);

            //AB区 (int)count + [(short)length + (string)name] * count
            var fStream1 = new FileStream(filePath1, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            var bw1 = new BinaryWriter(fStream1);

            //文件索引区 (int)assetCount + [(int)pathHash + (int)itemIndex] * assetCount
            var fStream2 = new FileStream(filePath2, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            var bw2 = new BinaryWriter(fStream2);

            //资源记录区 [(short)assetName + (uint)abNameIndex + (short)appendCount + [(uint)appendPathIndex * appendLength](不定长) ] * assetCount
            var fStream3 = new FileStream(filePath3, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            var bw3 = new BinaryWriter(fStream3);

            

            Dictionary<string, List<EditorA2BRecord>> assetBundleBulidDic = new Dictionary<string, List<EditorA2BRecord>>();
            //<最终包名，地址偏移>
            Dictionary<string, int> assetBundleNameDic = new Dictionary<string, int>();
            //<资源路径，地址偏移>
            Dictionary<string, int> assetPathToIndexDic = new Dictionary<string, int>();
            //<资源路径，路径编码>
            Dictionary<string, int> hashCodeDic = new Dictionary<string, int>();
            int GetHashCode(string inputStr) { 
                if(hashCodeDic.ContainsKey(inputStr))
                {
                    return hashCodeDic[inputStr];
                }
                else
                {
                    var result = A2BRecord.GetHashCode(inputStr);
                    hashCodeDic[inputStr] = result;
                    return result;
                }
            }

            int count = recordList.Count;

            bw1.Write(0);
            bw2.Write(count);
            //Debug.Log($"资源总数 >> {count}");

            bool isCancel = false;
            for (int i = 0; i < count; i++)
            {
                var record = recordList[i];
                string abName = record.bundleName;
                isCancel = EditorUtility.DisplayCancelableProgressBar($"记录索引表: ({i} / {count})", $"{record.assetPath}", (float)i / count);
                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }

                if (!assetBundleNameDic.ContainsKey(abName))
                {
                    int abNameIndex = (int)bw1.BaseStream.Position;
                    assetBundleNameDic.Add(abName, abNameIndex);
                    //uint abNameHash = (uint)Convert64BitHelper.i32ToInt(abName);
                    //Debug.Log($"添加AB包 >> {abName} : {(short)abNameIndex}");
                    short abNameLength = (short)System.Text.Encoding.Default.GetByteCount(abName);
                    //2 byte
                    bw1.Write(abNameLength);
                    //abName.Length byte 不定长
                    bw1.Write(abName);
                    assetBundleBulidDic.Add(abName, new List<EditorA2BRecord>());
                }
                var a2bList = assetBundleBulidDic[abName];
                if (a2bList.Contains(record))
                {
                    Debug.LogError($"添加AB包异常，映射表居然存在两个引用相同的记录对象！！！！assetName:{record.assetName}");
                    continue;
                }
                bool isFaild = false;
                foreach (var item in a2bList)
                {
                    if (item.assetName == record.assetName)
                    {
                        isFaild = true;
                        Debug.LogError($"同一个包中存在多个同名资源，改一下名字或者打包规则：bundle:{abName}, assets:{record.assetName}");
                        break;
                    }
                    if (item.assetPath.EndsWith(".unity"))
                    {
                        Debug.LogError($"存在和场景文件同一个AB包的资源，bundle:{abName}, asset:{record.assetPath}");
                    }
                }
                if (isFaild)
                {
                    continue;
                }

                //记录所有资源的下标，用于设置资源记录区的依赖下标

                if(assetPathToIndexDic.ContainsKey(record.assetPath)==false)
                {
                    assetPathToIndexDic.Add(record.assetPath, i);
                }else
                {
                    Debug.LogError("资源异常 record.assetPath  = "+ record.assetPath);
                }

                
            }

            bw1.Seek(0, SeekOrigin.Begin);
            bw1.Write(assetBundleNameDic.Count);

            for (int i = 0; i < count; i++)
            {
                var record = recordList[i];
                string abName = record.bundleName;

                isCancel = EditorUtility.DisplayCancelableProgressBar($"序列化索引表: ({i} / {count})", $"{record.assetPath}", (float)i / count);
                if (isCancel)
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }

                int assetPathHash = GetHashCode(record.assetPath);
                //4 byte
                bw2.Write(assetPathHash);
                //4 byte
                uint assetItemIndex = (uint)bw3.BaseStream.Position;
                bw2.Write(assetItemIndex);
                //Debug.Log($"添加资源 >> {assetPathHash} item地址: {assetItemIndex}");

                //Convert64BitHelper短的可还原
                //short assetName = (short)Convert64BitHelper.i32ToInt(record.assetName);
                short assetNameLength = (short)System.Text.Encoding.Default.GetByteCount(record.assetName);
                //2 byte
                bw3.Write(assetNameLength);
                //assetNameLength byte 不定长
                bw3.Write(record.assetName);
                //4 byte
                bw3.Write(assetBundleNameDic[abName]);


                //优化一下依赖列表，相同的ab的合并一下
                string[] dependArr =   ParseDependcys(dicPathRecord, record.assetPath,abName, record.dependArr); // //record.dependArr;//

                //2 byte
                bw3.Write((short)dependArr.Length);
                //Debug.Log($"AssetItem >> {assetName} ab下标{assetBundleNameDic[abName]} 依赖长度: {(short)record.dependArr.Length}");
                //不定长
                foreach (var depend in dependArr)
                {
                    //4 byte
                    bw3.Write(GetHashCode(depend));
                    //Debug.Log($"添加资源依赖 >> {GetHashCode(depend)}");
                }

            }

            EditorUtility.ClearProgressBar();
            if (isCancel)
            {
                bw.Close(); fStream.Close();
                bw1.Close(); fStream1.Close();
                bw2.Close(); fStream2.Close();
                bw3.Close(); fStream3.Close();
                return;
            }
            else
            {
                //写入头部
                bw.Write(bw1.BaseStream.Length);
                bw.Write(bw2.BaseStream.Length);
                //bw.Write(bw3.BaseStream.Length);

                //文件合并
                bw1.BaseStream.Flush();
                bw2.BaseStream.Flush();
                bw3.BaseStream.Flush();
                bw1.Close(); fStream1.Close();
                bw2.Close(); fStream2.Close();
                bw3.Close(); fStream3.Close();

                int readLength = 0;
                byte[] buffer = new byte[1024 * 100];
                void AppendBinFile(string fp)
                {
                    using (var fs = new FileStream(fp, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        while ((readLength = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.BaseStream.Write(buffer, 0, readLength);
                        }
                        fs.Close();
                    }
                }
                AppendBinFile(filePath1);
                AppendBinFile(filePath2);
                AppendBinFile(filePath3);

                bw.BaseStream.Flush();
                bw.Close(); fStream.Close();
            }

            if (File.Exists(filePath1)) { File.Delete(filePath1); }
            if (File.Exists(filePath2)) { File.Delete(filePath2); }
            if (File.Exists(filePath3)) { File.Delete(filePath3); }
        }
    }
}
