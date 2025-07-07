#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.VersionControl;
using XGame;
using XGame.Asset.Load;
using XGameEditor;
using System.Collections.Generic;
using System;

using UnityEditor.Build.Pipeline;
using UnityEngine.Build.Pipeline;


public class InvalidScriptFinder : EditorWindow
{
    public static Asset2BundleRecords s_asset2bundle;
    public static Dictionary<string, AssetBundle> s_loadedAssetBunbles = new Dictionary<string, AssetBundle>();
    //[MenuItem("XGame/预制体工具/清理加载所有AssetBundle")]
    static void UnloadAllSetBunbles()
    {
        foreach (var kv in s_loadedAssetBunbles)
        {
            if (kv.Value != null)
                kv.Value.Unload(true);
        }
        s_loadedAssetBunbles.Clear();
    }


    //[MenuItem("XGame/预制体工具/加载所有AssetBundle")]
    static void LoadBundle()
    {
        UnloadAllSetBunbles();

        var instance = NewGAsset2BundleRecords.BuildABInstance;
        Asset2BundleRecords asset2bundle = null;
        string Asset2BundleRecordsPath = Application.streamingAssetsPath + "/data/" + UpdateConfig.ASSET_BUNDLE_RECORD;
        if (File.Exists(Asset2BundleRecordsPath))
        {
            Debug.Log("读取: " + Asset2BundleRecordsPath);
            asset2bundle = new Asset2BundleRecords();
            asset2bundle.Load(Asset2BundleRecordsPath);
        }
        s_asset2bundle = asset2bundle;

        var recordList = instance.allRecord.RecordList;
        for (int i = 0; i < recordList.Count; i++)
        {
            var record = recordList[i];
            if (record.assetPath.EndsWith(".prefab"))
            {
                var bundleRecord = asset2bundle.GetRecord("Game/ImmortalFamily/GameResources/Artist/Battle/Buff/buff_ranshaoloop.prefab");
                if (bundleRecord != null)
                    LoadAssetBundleInternal(bundleRecord, 0);
                else
                    Debug.LogWarning(" BundleRecord is Null:"+ record.assetPath);

                //string[] names = s_loadedAssetBunbles[record.bundleName].GetAllAssetNames();
                //for(int k = 0; k < names.Length; k++)
                //{
                //    Debug.LogError(" name:" + names[k] + "path:" + bundleRecord.assetName);                    
                //}
                if (s_loadedAssetBunbles.ContainsKey(bundleRecord.bundleName))
                {
                    var assetObj = s_loadedAssetBunbles[bundleRecord.bundleName].LoadAsset(bundleRecord.assetName);
                    if (assetObj != null)
                    {
                        GameObject obj = (GameObject)GameObject.Instantiate(assetObj);
                        if (obj != null)
                        {
                            Debug.LogError($"加载成功:" + record.assetPath);
                            //GameObject.DestroyImmediate(obj);
                        }
                    }


                }
                //UnloadAllSetBunbles();
                return;
            }

        }
        UnloadAllSetBunbles();

        //////获取所有AB
        //string[] ABPaths = Directory.GetFiles(Application.streamingAssetsPath + "/data/", "*.bin", SearchOption.AllDirectories);
        //int nLen = ABPaths.Length;
        //for (int i = 0; i < nLen; ++i)
        //{
        //    ABPaths[i] = ABPaths[i].Replace("\\", "/");
        //    if (i == 60)
        //    {
        //        int a = 1;

        //    }
        //}

            //AssetBundle ab = AssetBundle.LoadFromFile(ABPaths[i], 0, 4);
            //if (ab == null)
            //{
            //    Debug.LogError($"加载失败{ABPaths[i]}");
            //}
            //else
            //{
            //    if (!ab.isStreamedSceneAssetBundle)
            //    {
            //        File.WriteAllText(Application.streamingAssetsPath + "/abcd.text", i.ToString());
            //        Object[] objs = ab.LoadAllAssets();

            //        for (int j = 0; j < objs.Length; j++)
            //        {
            //            //if (i == 60)
            //            //{
            //            //    Debug.LogError($"加载==={objs[j].name} abPath:{ABPaths[i]}");
            //            //    ab.Unload(true);
            //            //    return;
            //            //}

            //            if (objs[j] is GameObject prefab)
            //            {
            //                var obj = GameObject.Instantiate(objs[j]);
            //                Debug.Log($"发现预制体: {objs[j].name}");
            //                GameObject.DestroyImmediate(obj);
            //            }
            //            else if (objs[j] is Material mat)
            //            {
            //                var obj = GameObject.Instantiate(objs[j]);
            //                GameObject.DestroyImmediate(obj);
            //                //Debug.Log($"材质着色器: {mat.shader.name}");
            //            }

            //        }
            //        Debug.LogError($"加载成功{ABPaths[i]}");

            //    }
            //    ab.Unload(true);
            //}

            //} 
        }

    static void LoadAssetBundleInternal(A2BRecord record, int depth = 0)
    {
        if (record != null)
        {
            int nCount = record.dependArr.Length;
            for (int i = 0; i < nCount; ++i)
            {
                var depend = record.dependArr[i];
                var dependRecord = GetRecordByIndex(depend);
                LoadAssetBundleInternal(dependRecord, depth);
            }
            if (s_loadedAssetBunbles.ContainsKey(record.bundleName))
            {
            }
            else
            {
                var ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/data/" + record.bundleName, 0, 4);
                if (ab != null)
                {
                    Debug.LogError("ab load success:" + record.bundleName);
                    s_loadedAssetBunbles.Add(record.bundleName, ab);
                    ab.LoadAllAssets();

                }
                else
                {
                    Debug.LogError("ab is null:"+ record.bundleName);
                }
            }
        }
        else
        {
            Debug.LogWarning("is null");
        }


    }
    static A2BRecord GetRecordByIndex(int index)
        {
            var record = s_asset2bundle.GetRecord(index);
            if (record == null)
            {
                Debug.LogError($"LoadMgr 查找不到资源记录映射关系信息：index:{index}");
                return null;
            }

            if (string.IsNullOrEmpty(record.assetName) || string.IsNullOrEmpty(record.bundleName))
            {
                Debug.LogError($"LoadMgr.LoadResInternal>> 获取资源的加载AB名称失败！ index：{index}, assetName:{record.assetName}, bundleName:{record.bundleName}");
                return null;
            }

            return record;
        }

    [MenuItem("XGame/预制体工具/查找prefab,mat,asset和playable无效资源")]
    static void FindInvalidScripts()
    {
        int counter = 0;
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in allAssets)
        {
            // 过滤非必要资源类型
            if (assetPath.EndsWith(".prefab") ||
                assetPath.EndsWith(".asset") ||
                assetPath.EndsWith(".mat") ||
                assetPath.EndsWith(".playable"))
            //if(assetPath.EndsWith("TestBugCanvas.prefab"))
            //if (assetPath.EndsWith("UICommonDesc.prefab"))
            {
                Debug.Log("CheckAsset:" + assetPath);
                string yamlContent = File.ReadAllText(assetPath);

                // 使用正则表达式匹配 m_Script 字段
                MatchCollection matches = Regex.Matches(yamlContent,
                    @"m_Script: \{fileID: \d+, guid: ([a-f0-9]+), type: \d+\}");

                foreach (Match match in matches)
                {
                    string guid = match.Groups[1].Value;
                    string scriptPath = AssetDatabase.GUIDToAssetPath(guid);

                    // 检查 GUID 有效性
                    if (string.IsNullOrEmpty(scriptPath))
                    {
                        Debug.LogError($"发现无效脚本引用！\n资源路径: {assetPath}\n无效GUID: {guid}",
                            AssetDatabase.LoadMainAssetAtPath(assetPath));
                        counter++;
                    }
                }
            }
        }

        Debug.Log($"扫描完成，共发现 {counter} 个无效脚本引用");
    }


    [MenuItem("XGame/预制体工具/打包单个prefab资源")]
    static async void BuildAssetBundleWithSBP2()
    {
        // 1. 配置要打包的Prefab路径
        //string prefabPath = "Assets/Prefabs/MyPrefab.prefab"; // 修改为你的Prefab路径

        // 2. 创建AssetBundleBuild配置
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0] = new AssetBundleBuild
        {
            assetBundleName = "my_sbp_bundle",
            assetBundleVariant = "",
            assetNames = new[] { 
                "Assets/Game/ImmortalFamily/GameResources/Artist/Battle/Buff/buff_ranshaoloop.prefab" } // 可添加多个资源
        };

        // 3. 配置输出路径
        string outputPath = Application.streamingAssetsPath + "/SBPBundles2";
        System.IO.Directory.CreateDirectory(outputPath);
        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup activeGroup = BuildPipeline.GetBuildTargetGroup(activeTarget);
        // 4. 使用SBP打包
        BundleBuildParameters buildParams = new BundleBuildParameters(
            activeTarget, // 根据目标平台修改
            activeGroup,
            outputPath
        );

        //IBundleBuildResults iReuslt;
        //ReturnCode exitCode = ContentPipeline.BuildAssetBundles(
        //    buildParams,
        //    new BundleBuildContent(buildMap),
        //    out iReuslt
        //);

        BuildAssetBundleOptions option = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;
        //CompatibilityAssetBundleManifest manifest = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, buildMap, option, EditorUserBuildSettings.activeBuildTarget);
        CompatibilityAssetBundleManifest manifest = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, buildMap, option, EditorUserBuildSettings.activeBuildTarget);


        if (manifest == null)
            throw (new Exception("生成AB包失败！"));
    }


    //[MenuItem("XGame/预制体工具/Clean Invalid MonoBehaviour Blocks")]
    //public static void CleanInvalidScriptBlocks()
    //{
    //    // 示例处理单个文件，实际应遍历所有预制体/场景文件
    //    string targetFile = "Assets/G_Artist/Effect/Materials/AB_fire_lizi_001_AB.mat";
    //    ProcessYamlFile(targetFile);
    //    AssetDatabase.Refresh();
    //}

    //private static bool IsMonoBehaviourNode(YamlNode node)
    //{
    //    return node is YamlMappingNode mappingNode &&
    //           mappingNode.Children.TryGetValue("MonoBehaviour", out _);
    //}

    //private static bool IsScriptValid(YamlNode node)
    //{
    //    var mapping = (YamlMappingNode)node;
    //    if (mapping.Children.TryGetValue("m_Script", out YamlNode scriptNode))
    //    {
    //        var scriptValue = scriptNode.ToString();
    //        if (TryExtractGuid(scriptValue, out string guid))
    //        {
    //            // Unity 的 GUID 有效性检查
    //            return !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid));
    //        }
    //    }
    //    return false;
    //}

    //private static bool TryExtractGuid(string input, out string guid)
    //{
    //    // 解析类似 {fileID: 11500000, guid: d0353a89b1f911e48b9e16bdc9f2e058, type: 3} 的格式
    //    var guidStart = input.IndexOf("guid:") + 5;
    //    if (guidStart > 5)
    //    {
    //        var guidEnd = input.IndexOf(',', guidStart);
    //        guid = input.Substring(guidStart, guidEnd - guidStart).Trim();
    //        return true;
    //    }
    //    guid = null;
    //    return false;
    //}

    //private static void ProcessYamlFile(string filePath)
    //{
    //    var yaml = new YamlStream();
    //    List<YamlNode> nodesToRemove = new List<YamlNode>();

    //    using (var reader = new StreamReader(filePath))
    //    {
    //        yaml.Load(reader);

    //        // 遍历所有顶级文档节点
    //        foreach (YamlDocument doc in yaml.Documents)
    //        {
    //            // 查找所有 MonoBehaviour 块
    //            if (doc.RootNode is YamlMappingNode rootMapping)
    //            {
    //                foreach (var entry in rootMapping.Children)
    //                {
    //                    // 识别 MonoBehaviour 标签 !u!114
    //                    if (IsMonoBehaviourNode(entry.Value))
    //                    {
    //                        if (!IsScriptValid(entry.Value))
    //                        {
    //                            nodesToRemove.Add(entry.Key);
    //                        }
    //                    }
    //                }

    //                // 删除无效块
    //                foreach (var key in nodesToRemove)
    //                {
    //                    rootMapping.Children.Remove(key);
    //                }
    //            }
    //        }
    //    }
    //    // 创建保留 Unity 格式的发射器
    //    var emitterSettings = new EmitterSettings(3, int.MaxValue, true, 128);


    //    // 保存修改后的 YAML
    //    using (var writer = new StreamWriter(filePath))
    //    {
    //        yaml.Save(new YamlDotNet.Core.Emitter(writer), false);
    //    }
    //}


    //[MenuItem("XGame/预制体工具/Clean Invalid MonoBehaviour Blocks2")]
    public static void CleanInvalidComponents()
    {
        string targetFile = "Assets/G_Artist/Effect/Materials/AB_fire_lizi_001_AB.mat";
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(targetFile);
        bool modified = false;

        foreach (UnityEngine.Object asset in assets)
        {
            //// 跳过主资源（根据类型过滤）
            //if (asset == targetAsset) continue;

            if (asset == null)
            {
                Debug.Log($"Removing invalid MonoBehaviour: {targetFile}");
                UnityEngine.Object.DestroyImmediate(asset, true);
                modified = true;
                continue;
            }
            SerializedObject so = new SerializedObject(asset);
            SerializedProperty scriptProp = so.FindProperty("m_Script");

            if (scriptProp != null && IsScriptInvalid(scriptProp))
            {
                Debug.Log($"Removing invalid MonoBehaviour: {targetFile}");
                UnityEngine.Object.DestroyImmediate(asset, true);
                modified = true;
            }
        }

        if (modified)
        {
            AssetDatabase.ForceReserializeAssets(new[] { targetFile });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    private static bool IsScriptInvalid(SerializedProperty scriptProperty)
    {
        if (scriptProperty.objectReferenceValue == null)
        {
            return true;
        }

        // 进一步验证GUID是否存在
        string scriptPath = AssetDatabase.GetAssetPath(scriptProperty.objectReferenceValue);
        return string.IsNullOrEmpty(scriptPath);
    }
    private static void ProcessGameObject(Material go)
    {
        SerializedObject serializedObject = new SerializedObject(go);
        SerializedProperty componentProperty = serializedObject.FindProperty("m_Component");

        if (componentProperty == null || !componentProperty.isArray) return;

        List<int> invalidIndices = new List<int>();

        // 第一步：扫描无效组件
        for (int i = 0; i < componentProperty.arraySize; i++)
        {
            SerializedProperty element = componentProperty.GetArrayElementAtIndex(i);
            SerializedProperty componentRef = element.FindPropertyRelative("component");

            if (IsInvalidMonoBehaviour(componentRef))
            {
                invalidIndices.Add(i);
            }
        }

        // 第二步：倒序删除（防止索引变化）
        for (int i = invalidIndices.Count - 1; i >= 0; i--)
        {
            int index = invalidIndices[i];
            componentProperty.DeleteArrayElementAtIndex(index);
            Debug.Log($"Removed invalid MonoBehaviour at index {index} on {go.name}");
        }

        serializedObject.ApplyModifiedProperties();
    }

    static bool IsInvalidMonoBehaviour(SerializedProperty componentRef)
    {
        if (componentRef.objectReferenceValue == null) return true;

        // 仅处理 MonoBehaviour 组件
        if (!(componentRef.objectReferenceValue is MonoBehaviour)) return false;

        SerializedObject scriptSerialized = new SerializedObject(componentRef.objectReferenceValue);
        SerializedProperty scriptProperty = scriptSerialized.FindProperty("m_Script");

        if (scriptProperty == null) return true;

        // 验证脚本是否存在
        return scriptProperty.objectReferenceValue == null;
    }



    [MenuItem("XGame/预制体工具/清理mat和playable无效引用")]
    static void ValidateScripts()
    {
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in allAssets)
        {
            // 过滤非必要资源类型
            if (assetPath.EndsWith(".mat") || assetPath.EndsWith(".playable"))
            //if (assetPath.EndsWith(".playable"))
            {

                //path = "Assets/G_Artist/Effect/Materials/AB_fire_lizi_001_AB.mat";
                ProcessMaterial(assetPath);

            }
        }

    }

    static void ProcessMaterial(string path)
    {
        // 加载原始文本
        string[] lines = File.ReadAllLines(path);
        bool modified = false;
        bool isTargetMonoBehaviour = false;
        bool shouldRemove = false;
        int blockStart = -1;
        int blockEnd = -1;

        // 第一次遍历：查找需要删除的MonoBehaviour块
        for (int i = 0; i < lines.Length;i++)
        {
            if (lines[i].StartsWith("--- !u!") && !isTargetMonoBehaviour )
            {
                isTargetMonoBehaviour = true;
                blockStart = i;
                shouldRemove = false;

                continue;
            }

            if (isTargetMonoBehaviour)
            {
                if (lines[i].Contains("m_Script:"))
                {
                    Match guidMatch = Regex.Match(lines[i], @"guid: (\w+)");
                    if (guidMatch.Success)
                    {
                        string scriptGuid = guidMatch.Groups[1].Value;
                        shouldRemove = !IsScriptValid(scriptGuid);
                    }
                }

                if ((lines[i].StartsWith("--- !u!") && i != blockStart) || i == lines.Length -1) 
                {
                    if (i != lines.Length -1)
                        i--;
                    blockEnd = i;
                    isTargetMonoBehaviour = false;

                    if (shouldRemove)
                    {
                        //Debug.LogError($"删除行{blockStart} {blockEnd}");
                        modified = true;
                        // 标记要删除的行范围
                        for (int j = blockStart; j <= blockEnd; j++)
                        {
                            lines[j] = null;
                        }
                    }
                    shouldRemove = false;
                    //i不变
                    continue;
                }
            }
 
        }

        // 第二次遍历：构建新文件内容
        if (modified)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (string line in lines)
                {
                    if (line != null)
                    {
                        writer.WriteLine(line);
                    }
                }
            }                        
            Debug.LogError($"修改资源:{path}");
            AssetDatabase.ImportAsset(path);
        }
    }

    static bool IsScriptValid(string scriptGuid)
    {
        //// 通过查找项目中的.meta文件验证GUID有效性
        //string[] allMetaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories);
        //foreach (string metaFile in allMetaFiles)
        //{
        //    string content = File.ReadAllText(metaFile);
        //    if (content.Contains($"guid: {scriptGuid}"))
        //    {
        //        return true;
        //    }
        //}
        //return false;
        return !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(scriptGuid));
    }

}
#endif