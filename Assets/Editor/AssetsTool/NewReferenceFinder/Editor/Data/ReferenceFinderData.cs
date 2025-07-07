//using CustomEditorCoroutine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace ReferenceFinder
{
    public class ReferenceFinderData
    {
        //缓存路径
        private const string CACHE_PATH = "Library/ReferenceFinderCache";

        //标记序列化的内容是否有改变
        private const string CACHE_VERSION = "V1";

        //private static EditorCoroutine collectCor;

        //资源引用信息字典
        private static Dictionary<string, AssetDescription> assetDict;

        public static Dictionary<string, AssetDescription> AssetDict
        {
            get
            {
                if (assetDict == null || assetDict.Count == 0)
                {
                    if (!ReadFromCache())
                    {
                        CollectDependenciesInfo();
                    }
                    else
                    {
                        UpdateReferenceInfo();
                    }
                }
                return assetDict;
            }
        }

        //获取资源信息
        public static AssetDescription Get(string guid)
        {
            if (AssetDict.TryGetValue(guid, out var result))
            {
                return result;
            }
            return null;
        }

        //收集资源引用信息并更新缓存
        public static void CollectDependenciesInfo(bool isSync = false, Action callback = null)
        {
            StopCollect();
            //collectCor = EditorCoroutine.Start(CollectDependenciesInfoCor(isSync, callback));
            CollectDependenciesInfoCor(isSync, callback);
        }

        private static void CollectDependenciesInfoCor(bool isSync, Action callback)
        {
            var paused = EditorApplication.isPaused;
            var timeScale = Time.timeScale;

            try
            {
                if (isSync)
                {
                    //EditorApplication.isPaused = false;
                    //Time.timeScale = 0.001f;
                }

                ReadFromCache();
                var allAssets = AssetDatabase.GetAllAssetPaths();
                int totalCount = allAssets.Length;
                for (int i = 0; i < allAssets.Length; i++)
                {
                    if ((i % 100 == 0))
                    {
                        if (isSync)
                        {
                            //yield return null;
                        }
                        if (EditorUtility.DisplayCancelableProgressBar("Refresh", string.Format("Collecting {0} assets", i), (float)i / totalCount))
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                    }
                    if (File.Exists(allAssets[i]))
                    {
                        ImportAsset(allAssets[i]);
                    }
                    if (i % 20000 == 0)
                    {
                        GC.Collect();
                    }
                }
                GC.Collect();
                //生成引用数据
                EditorUtility.DisplayCancelableProgressBar("Refresh", "Generating asset reference info", 1f);
                UpdateReferenceInfo();
                //将信息写入缓存
                EditorUtility.DisplayCancelableProgressBar("Refresh", "Write to cache", 1f);
                WriteToChache();
                EditorUtility.ClearProgressBar();

                if (isSync)
                {
                    //var lastFrameTime = EditorApplication.timeSinceStartup;
                    //while (EditorApplication.timeSinceStartup - lastFrameTime < 0.1f) yield return null;
                }
            }
            finally
            {
                EditorApplication.isPaused = paused;
                Time.timeScale = timeScale;
                EditorUtility.ClearProgressBar();
                StopCollect();
                callback?.Invoke();
            }
        }

        //终止异步收集
        private static void StopCollect()
        {
            //if (collectCor != null)
            //{
            //    collectCor.Stop();
            //    collectCor = null;
            //}
        }

        //强制刷新引用信息
        public static void ForceUpdateRefrenceInfo()
        {
            var assetDict = AssetDict;
            foreach (var asset in assetDict.Values)
            {
                asset.references.Clear();
            }
            UpdateReferenceInfo();
        }

        //通过依赖信息更新引用信息
        private static void UpdateReferenceInfo()
        {
            int updateCount = 0;
            void curUpdateReferenceInfo()
            {
                if (updateCount >= 10)
                {
                    Debug.LogError("资源引用查找 >> 出错，已经递归遍历了十遍");
                    return;
                }
                updateCount++;
                bool isHaveAdd = false;
                foreach (var asset in assetDict)
                {
                    for (int i = asset.Value.dependencies.Count - 1; i >= 0; i--)
                    {
                        var assetGuid = asset.Value.dependencies[i];
                        if (assetDict.ContainsKey(assetGuid))
                        {
                            assetDict[assetGuid].references.Add(asset.Key);
                        }
                        else
                        {
                            string path = AssetDatabase.GUIDToAssetPath(assetGuid);
                            if (File.Exists(path))
                            {
                                isHaveAdd = true;
                                ImportAsset(path);
                            }else
                            {
                                Debug.LogError("不存在文件 path=" + path);
                            }
                        }
                    }
                }
                if (isHaveAdd)
                {
                    curUpdateReferenceInfo();
                }
            }
            curUpdateReferenceInfo();
        }

        //生成并加入引用信息
        private static void ImportAsset(string path, Action<string, List<string>> externSetDenpen = null)
        {
            //通过path获取guid进行储存
            string guid = AssetDatabase.AssetPathToGUID(path);
            Hash128 assetDependencyHash = AssetDatabase.GetAssetDependencyHash(path);
            string hashStr = assetDependencyHash.ToString();
            //如果AssetDict没包含该guid或包含了修改时间不一样则需要更新
            if (!assetDict.ContainsKey(guid) || assetDict[guid].assetDependencyHash.CompareTo(hashStr) != 0)
            {
                //将每个资源的直接依赖资源转化为guid进行储存
                var guids = new List<string>();
                foreach (var p in AssetDatabase.GetDependencies(path, false))
                {
                    guids.Add(AssetDatabase.AssetPathToGUID(p));
                }

                externSetDenpen?.Invoke(path, guids);

                //生成asset依赖信息，被引用需要在所有的asset依赖信息生成完后才能生成
                AssetDescription ad = new AssetDescription(path);
                ad.assetDependencyHash = hashStr;
                ad.dependencies = guids;

                if (assetDict.ContainsKey(guid))
                {
                    assetDict[guid] = ad;
                }
                else
                {
                    assetDict.Add(guid, ad);
                }
            }
        }

        //添加材质额外引用
        private static void AddMaterialDenpencied(string path, List<string> guids)
        {
            Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            if (mat)
            {
                string shaderPath = AssetDatabase.GetAssetPath(mat.shader);
                string shaderGuid = AssetDatabase.AssetPathToGUID(shaderPath);
                if (!string.IsNullOrEmpty(shaderGuid) && !guids.Contains(shaderGuid))
                {
                    guids.Add(shaderGuid);
                }
            }
        }

        //读取缓存信息
        public static bool ReadFromCache()
        {
            assetDict = new Dictionary<string, AssetDescription>();
            if (!File.Exists(CACHE_PATH))
            {
                return false;
            }

            var serializedGuid = new List<string>();
            var serializedDependencyHash = new List<string>();
            var serializedDenpendencies = new List<int[]>();
            //反序列化数据
            FileStream fs = File.OpenRead(CACHE_PATH);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                string cacheVersion = (string)bf.Deserialize(fs);
                if (cacheVersion != CACHE_VERSION)
                {
                    return false;
                }

                EditorUtility.DisplayCancelableProgressBar("Import Cache", "Reading Cache", 0);
                serializedGuid = (List<string>)bf.Deserialize(fs);
                serializedDependencyHash = (List<string>)bf.Deserialize(fs);
                serializedDenpendencies = (List<int[]>)bf.Deserialize(fs);
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                fs.Close();
            }

            for (int i = 0; i < serializedGuid.Count; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(serializedGuid[i]);
                if (!string.IsNullOrEmpty(path))
                {
                    var ad = new AssetDescription(path);
                    ad.assetDependencyHash = serializedDependencyHash[i];
                    assetDict.Add(serializedGuid[i], ad);
                }
            }

            for (int i = 0; i < serializedGuid.Count; ++i)
            {
                string guid = serializedGuid[i];
                if (assetDict.ContainsKey(guid))
                {
                    var guids = serializedDenpendencies[i]
                        .Select(index => serializedGuid[index])
                        .Where(g => assetDict.ContainsKey(g))
                        .ToList();
                    assetDict[guid].dependencies = guids;
                }
            }
            return true;
        }

        //写入缓存
        private static void WriteToChache()
        {
            if (File.Exists(CACHE_PATH))
            {
                File.Delete(CACHE_PATH);
            }

            var serializedGuid = new List<string>();
            var serializedDependencyHash = new List<string>();
            var serializedDenpendencies = new List<int[]>();
            //辅助映射字典
            var guidIndex = new Dictionary<string, int>();
            //序列化
            using (FileStream fs = File.OpenWrite(CACHE_PATH))
            {
                foreach (var pair in assetDict)
                {
                    guidIndex.Add(pair.Key, guidIndex.Count);
                    serializedGuid.Add(pair.Key);
                    serializedDependencyHash.Add(pair.Value.assetDependencyHash);
                }

                foreach (var guid in serializedGuid)
                {
                    int[] indexes = assetDict[guid].dependencies
                        .Where(s => guidIndex.ContainsKey(s))
                        .Select(s => guidIndex[s]).ToArray();
                    serializedDenpendencies.Add(indexes);
                }

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, CACHE_VERSION);
                bf.Serialize(fs, serializedGuid);
                bf.Serialize(fs, serializedDependencyHash);
                bf.Serialize(fs, serializedDenpendencies);
            }
        }

        //更新引用信息状态
        public static void UpdateAssetState(string guid)
        {
            AssetDescription ad = Get(guid);
            if (ad != null && ad.state != EAssetState.NODATA)
            {
                if (File.Exists(ad.path))
                {
                    //修改时间与记录的不同为修改过的资源
                    if (ad.assetDependencyHash != AssetDatabase.GetAssetDependencyHash(ad.path).ToString())
                    {
                        ad.state = EAssetState.CHANGED;
                    }
                    else
                    {
                        //默认为普通资源
                        ad.state = EAssetState.NORMAL;
                    }
                }
                //不存在为丢失
                else
                {
                    ad.state = EAssetState.MISSING;
                }
            }
            //字典中没有该数据
            else if (ad == null)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ad = new AssetDescription(path);
                ad.state = EAssetState.NODATA;
                assetDict.Add(guid, ad);
            }
        }

        //更新引用信息
        public static void UpdateAssetDepencies(string guid)
        {
            AssetDescription ad = Get(guid);

            if (ad != null)
            {
                foreach (var dep in ad.dependencies)
                {
                    UpdateAssetDepencies(dep);
                }

                var guids = new List<string>();
                foreach (var p in AssetDatabase.GetDependencies(ad.path, false))
                {
                    guids.Add(AssetDatabase.AssetPathToGUID(p));
                }

                ad.dependencies = guids;

                UpdateAssetState(guid);
            }
        }

        public static void ConvertDependcyRecords(string srcPath, string dstPath)
        {
            HashSet<string> hashPath = new HashSet<string>();
            ReferenceFinderData.CollectDependenciesInfo();
            string[] allResPath = File.ReadAllLines(srcPath);
            int nCount = allResPath.Length;
            for(int i=0;i< nCount;++i)
            {

                string dir = Application.dataPath + "/" + allResPath[i];
                if(Directory.Exists(dir))
                {
                    string[] foldFiles = Directory.GetFiles(dir,"*.*", SearchOption.AllDirectories);
                    for(int j=0;j<foldFiles.Length;++j)
                    {
                        if(foldFiles[j].IndexOf(".meta")>=0)
                        {
                            continue;
                        }

                        foldFiles[j] = foldFiles[j].Replace("\\", "/");
                        __AddDepencyPath(hashPath, foldFiles[j].Substring(foldFiles[j].IndexOf("Assets/")));
                    }
                }else
                {
                    __AddDepencyPath(hashPath, "Assets/" + allResPath[i]);
                }

                
            }

            int len = "Assets/".Length;
            allResPath = new string[hashPath.Count];
            int nIndex = 0;
            foreach (var str in hashPath)
            {
                if(str.IndexOf("Assets")>=0)
                {
                    allResPath[nIndex] = str.Substring(len);
                }else
                {
                    allResPath[nIndex] = str;
                }
               

                ++nIndex;
            }

            //保存文件
            File.WriteAllLines(dstPath, allResPath);
        }

        private static void __AddDepencyPath(HashSet<string> hashPath,string path)
        {
            //已经添加过了的，不再计算
            if(hashPath.Contains(path))
            {
                return;
            }

            /*
            if(path.IndexOf("com.unity.render-pipelines.universal")>=0)
            {
                int a = 0;
                ++a;
            }*/

            //加入自己,免得循环依赖
            hashPath.Add(path);



            string assetsPath = path;
            string guid = AssetDatabase.AssetPathToGUID(assetsPath);
            AssetDescription ad = Get(guid);
            if(null!= ad&& ad.dependencies!=null)
            {
                for (int i = 0; i < ad.dependencies.Count; ++i)
                {
                    assetsPath = AssetDatabase.GUIDToAssetPath(ad.dependencies[i]);
                    if (assetsPath.LastIndexOf(".cs") >= 0 || assetsPath.LastIndexOf(".dll") >= 0)
                    {
                        continue;
                    }

                    __AddDepencyPath(hashPath, assetsPath);
                }
            }

          
           

        }
    }
}