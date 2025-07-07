using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;

public class ImageOptimizeTool : EditorWindow//好吧 我真的不想移动这些图片 因为有些代码就是直接读取路径下图片 改完估计又要报错 ┴─┴︵╰（‵□′╰） 
{

    [MenuItem("XGame/资源工具/替换预制体相同依赖图片 - 图片目录调整")]
    static void ShowWindow()
    {
        CreateWindow<ImageOptimizeTool>("替换预制体相同依赖图片 - 图片目录调整");
    }


    public static string MainArtistDir = "";

    public static string MainUIDir = "UI";

    public static string CommonMoveDir = "Misc_Common";

    

    string m_PrefabPath;//UI窗口预制体目录
    string m_CommonDirectory;//Common 图片目录
    SameFunctionUIPrefabConfig m_sameFunctionUIPrefabConfig;//相同功能信息配置文件

    List<string> filterSourceDirectories;//限制移动的 图片目录（源文件只有在这些目录下的图片才会移动）
    List<string> ignoreDirectories;//忽略移动的 图片目录

    Dictionary<string, List<string>> m_sameImage2FilePathDict;//相同图片 对应 图片路径列表
    Dictionary<string, List<GameObject>> m_imageFiles2PrefabListDict;//被依赖图片 对应 依赖预制体列表
    //Dictionary<GameObject, HashSet<string>> m_prefab2AllDependencyImageFileDict;//预制体 对应 所有 依赖图片 字典

    List<KeyValuePair<string, List<GameObject>>> m_waitToMoveAgainList;//等待 再次移动的图片引用数据 列表
    Dictionary<GameObject, int> m_prefabDependenceCountDict;//预制体 - 依赖图片数 字典

    private void OnEnable()
    {
        MainArtistDir = $"{Application.dataPath}/Game/ImmortalFamily/GameResources/Artist/";
        m_PrefabPath = $"{Application.dataPath}/Game/ImmortalFamily/GameResources/Prefabs/UI";
        m_CommonDirectory = MainArtistDir + MainUIDir+ "/Common";

        filterSourceDirectories = new List<string>()
        {
           // $"{Application.dataPath}/G_Artist",
            $"{MainArtistDir}",
        };

        ignoreDirectories = new List<string>()
        {
            $"{MainArtistDir}Icon",//因为这是 Icon 图标 的 目录 so 忽略
            $"{MainArtistDir}TmpSpriteAtlas",//因为有很多加载是直接 读取 这个文件夹下的 所以 先忽略掉
            $"{MainArtistDir}UI_I18N" ,
            $"{MainArtistDir}"+MainUIDir+"/Common",//因为有很多加载是直接 读取 这个文件夹下的 所以 先忽略掉
            $"{Application.dataPath}/G_Artist",//因为有很多加载是直接 读取 这个文件夹下的 所以 先忽略掉
        };

        m_sameFunctionUIPrefabConfig = AssetDatabase.LoadAssetAtPath<SameFunctionUIPrefabConfig>("Assets/Editor/AssetsTool/ImageOptimizeRelation/Config.asset");

    }


    private void OnGUI()
    {
        GUILayout.Label("设置查找字典");
        Setup();
        GUILayout.Label("替换预制体相同依赖图片");
        Replace();
        GUILayout.Label("UI图片目录调整");
        Adjust();
    }


    /// <summary>
    /// 构建查找字典
    /// </summary>
    void Setup()
    {
        SelectFolder(ref m_PrefabPath, "UI预制体目录");

        if (GUILayout.Button("构建 图片->预制体 字典"))
        {
            m_imageFiles2PrefabListDict = ImageOptimizeUtility.GetPrefasImageDependData(m_PrefabPath);
        }
    }

    /// <summary>
    /// 替换预制体引用图片
    /// </summary>
    void Replace()
    {
        if (GUILayout.Button("构建 相同图片 字典"))
        {

            __BuildSameImg2DicImp();
         }

        if (GUILayout.Button("替换预制体相同图片依赖"))
        {
            __RepalceSameImgeImp();

        }
    }

    void __BuildSameImg2DicImp()
    {
        List<string> allfiles = new List<string>();
        foreach (var data in m_imageFiles2PrefabListDict)
        {
            allfiles.Add(data.Key);
        }
        m_sameImage2FilePathDict = ImageOptimizeUtility.GetSameImage2PathDict(allfiles.ToArray());

    }

    void __RepalceSameImgeImp()
    {
        if(m_sameImage2FilePathDict != null && m_imageFiles2PrefabListDict != null && m_sameImage2FilePathDict.Count > 0 && m_imageFiles2PrefabListDict.Count > 0 )
        {
            ReplacePrefabDependencies();
            AssetDatabase.Refresh();
        }
    }



    /// <summary>
    /// 图片目录调整
    /// </summary>
    void Adjust()
    {
        if (m_imageFiles2PrefabListDict != null && m_imageFiles2PrefabListDict.Count > 0)
        {
            if (GUILayout.Button("增加可以移动的图片目录"))
            {
                string path = $"{MainArtistDir}"+ MainUIDir;
                path = EditorUtility.OpenFolderPanel("过滤文件夹", path, "");
                filterSourceDirectories.Add(path);
            }
            for (int i = 0; i < filterSourceDirectories.Count; i++)
            {
                using (var hor = new GUILayout.HorizontalScope())
                {
                    this.filterSourceDirectories[i] = EditorGUILayout.TextField("", this.filterSourceDirectories[i]);
                    if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                    {
                        this.filterSourceDirectories[i] = EditorUtility.OpenFolderPanel("过滤文件夹", this.filterSourceDirectories[i], "");
                    }
                    if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                    {
                        this.filterSourceDirectories.RemoveAt(i);
                    }
                }
            }
            GUILayout.Space(16f);
            if (GUILayout.Button("增加忽略文件夹（+）（忽略文件夹下的图片将不会移动）"))
            {
                string path = $"{MainArtistDir}"+ MainUIDir;
                path = EditorUtility.OpenFolderPanel("忽略文件夹", path, "");
                ignoreDirectories.Add(path);
                
            }
            for (int i = 0; i < ignoreDirectories.Count; i++)
            {
                using (var hor = new GUILayout.HorizontalScope())
                {
                    this.ignoreDirectories[i] = EditorGUILayout.TextField("", this.ignoreDirectories[i]);
                    if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                    {
                        this.ignoreDirectories[i] = EditorUtility.OpenFolderPanel("忽略文件夹", this.ignoreDirectories[i], "");
                    }
                    if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                    {
                        this.ignoreDirectories.RemoveAt(i);
                    }
                }
            }
            GUILayout.Space(16f);
            m_sameFunctionUIPrefabConfig = EditorGUILayout.ObjectField(name, m_sameFunctionUIPrefabConfig, typeof(SameFunctionUIPrefabConfig), true) as SameFunctionUIPrefabConfig;
            GUILayout.Space(16f);
            if (GUILayout.Button("移动依赖图片"))
            {

                m_imageFiles2PrefabListDict = ImageOptimizeUtility.GetPrefasImageDependData(m_PrefabPath);
                __BuildSameImg2DicImp();
                //__RepalceSameImgeImp();
                __MoveImagesFileImp();

            }

            if (GUILayout.Button("替换图片依赖"))
            {

                m_imageFiles2PrefabListDict = ImageOptimizeUtility.GetPrefasImageDependData(m_PrefabPath);
                __BuildSameImg2DicImp();
                __RepalceSameImgeImp();

            }
        }
    }

    private void __MoveImagesFileImp()
    {
        bool success = false;
        if (m_sameFunctionUIPrefabConfig != null)
        {
            success = m_sameFunctionUIPrefabConfig.SetupData();
        }
        if (success)
        {
            MoveImageFile(m_imageFiles2PrefabListDict, this.filterSourceDirectories, this.ignoreDirectories, m_sameFunctionUIPrefabConfig);
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 显示 选择目录
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="name">字段名</param>
    void SelectFolder(ref string path, string name)
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            path = EditorGUILayout.TextField(name, path);
            if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                path = EditorUtility.OpenFolderPanel(name, path, "");
            }
        }
    }

    /// <summary>
    /// 更新处理进度条
    /// </summary>
    /// <param name="title"></param>
    /// <param name="desc"></param>
    /// <param name="rate"></param>
    static void UpdateProcessBar(string title, string desc, float rate)
    {
        if (rate >= 0.99f)
        {
            EditorUtility.ClearProgressBar();
        }
        else
        {
            EditorUtility.DisplayProgressBar(title, $"{desc}:{rate * 100}%...", rate);
        }
    }

    /// <summary>
    /// 如果相同图片存在 将依赖图片的预制体 替换成唯一 依赖
    /// </summary>
    void ReplacePrefabDependencies()
    {
        /// <summary>
        /// 从多于一个路径的列表中选择一个路径
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
         void ChooseOneFilePath(List<string> list2, out string path, out List<string> otherPathList)
        {

            //先找Common
            foreach (var item in list2)
            {
                if (item.Contains("/Common") )//如果 路径包含 Common 或 HUD 或 MainUI 目录 
                {
                    path = item;
                    otherPathList = new List<string>();
                    for (int i = 0; i < list2.Count; i++)
                    {
                        if (list2[i] != path)
                            otherPathList.Add(list2[i]);
                    }
                    return;
                }
            }

            //再找 Misc_Common
            foreach (var item in list2)
            {
                if (item.Contains(CommonMoveDir))//如果 路径包含 Common 或 HUD 或 MainUI 目录 
                {
                    path = item;
                    otherPathList = new List<string>();
                    for (int i = 0; i < list2.Count; i++)
                    {
                        if (list2[i] != path)
                            otherPathList.Add(list2[i]);
                    }
                    return;
                }
            }

            path = list2[0];
            otherPathList = new List<string>();
            for (int i = 1; i < list2.Count; i++)
            {
                otherPathList.Add(list2[i]);
            }
        }


        List<string> list = new List<string>();

        foreach (var item in m_sameImage2FilePathDict)
        {
            if (item.Value.Count > 1)//目录存在 相同图片文件
            {
                ChooseOneFilePath(item.Value, out string newImageFilePath, out List<string> oldImageFilePaths);

                string newDependencyGUID = AssetDatabase.AssetPathToGUID(newImageFilePath);

                foreach (var oldImageFilePath in oldImageFilePaths)
                {
                    if (m_imageFiles2PrefabListDict.ContainsKey(oldImageFilePath))//如果 旧图片 被 预制体依赖
                    {
                        string oldDependencyGUID = AssetDatabase.AssetPathToGUID(oldImageFilePath);

                        float index = 1f;
                        foreach (var go in m_imageFiles2PrefabListDict[oldImageFilePath])
                        {
                            UpdateProcessBar("替换预制体依赖", go.name, index / m_imageFiles2PrefabListDict[oldImageFilePath].Count);
                            ImageOptimizeUtility.ReplacePrefabDependencies(go, oldDependencyGUID, newDependencyGUID,"");

                            list.Add($"{go.name}\t替换资源 ({oldImageFilePath}) ==> ({newImageFilePath}),");
                        }
                    }

                }
            }
        }

        EditorUtility.ClearProgressBar();
        string fileName = $"{Application.dataPath}/预制体替换相同图片依赖_{System.DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }


        FileStream fs = File.Create(fileName);
        StreamWriter sw = new StreamWriter(fs);
        foreach (var data in list)
        {
            sw.WriteLine(data);
        }
        sw.Flush();
        sw.Close();
        fs.Close();
    }


    /// <summary>
    /// 根据 配置信息 或者 图片 依赖信息 移动图片
    /// （规则：
    /// 如果一张图片被多个UI预制体依赖，查找配置文件，判断这些预制体是否为配置文件中某个配置中预制体列表的子集，如果是就移动该图片到配置文件中对应目录下；
    /// 如果没有找到配置文件，则移动该文件到Common 目录下；
    /// 如果该图片只被单个UI预制体依赖，再次遍历配置文件查找是否为某个配置文件预制体列表的子集，如果是就移动该图片到配置文件中对应目录下；
    /// 如果没有找到就在UI1.0下创建预制体同名文件夹，并将图片移动到该目录下
    /// ）
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="targetPath"></param>
    void MoveImageFile(Dictionary<string, List<GameObject>> dict, List<string> filterSourceDirectories, List<string> ignoreDirectories, SameFunctionUIPrefabConfig config)//这里还要做下限制 特定目录下 的 不移动
    {
        //检查 预制体列表 是否是 预制体集合 的 子集
         bool IfSetContainsList(HashSet<GameObject> set, List<GameObject> list2)
        {
            foreach (var item in list2)
            {
                if (!set.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        //检查文件是否 在这些目录下
         bool IfImageFileIsInThoseDirectories(string fileName, string[] thoseDirectories)
        {
            for (int i = 0; i < thoseDirectories.Length; i++)
            {
                if (fileName.StartsWith(thoseDirectories[i]))
                    return true;
            }


            return false;
        }

         string GetGameObjectListString(List<GameObject> list2)
        {
            string msg = " ";
            foreach (var go in list2)
            {
                msg += $"{go.name} , ";
            }
            return msg;
        }


        if (config == null)
        {
            Debug.LogError("配置文件为空");
            return;
        }

        m_waitToMoveAgainList = new List<KeyValuePair<string, List<GameObject>>>();
        m_prefabDependenceCountDict = new Dictionary<GameObject, int>();


        string[] ignoreDirectoriesArray = new string[ignoreDirectories.Count];
        for (int i = 0; i < ignoreDirectoriesArray.Length; i++)
        {
            int sybolIndex = ignoreDirectories[i].IndexOf("/Assets");
            ignoreDirectoriesArray[i] = ignoreDirectories[i].Substring(sybolIndex + 1);
        }

        string[] filterDirectoriesArray = new string[filterSourceDirectories.Count];
        for (int i = 0; i < filterDirectoriesArray.Length; i++)
        {
            int sybolIndex = filterSourceDirectories[i].IndexOf("/Assets");
            filterDirectoriesArray[i] = filterSourceDirectories[i].Substring(sybolIndex + 1);
        }

        string localCommonDirectory = ImageOptimizeUtility.AbsolutePath2AssetPath(m_CommonDirectory);

        List<string> list = new List<string>();

        float index = 0f;

        foreach (var imageData in dict)
        {

            if (IfImageFileIsInThoseDirectories(imageData.Key, ignoreDirectoriesArray))
            {
                list.Add($"z from\t{imageData.Key}\t未移动\treason:在忽略目录中");
                continue;
            }

            if (!IfImageFileIsInThoseDirectories(imageData.Key, filterDirectoriesArray))//如果被移动图片不是在过滤源文件目录下 那么不要处理 同时写入日志中
            {
                list.Add($"z from\t{imageData.Key}\t未移动\treason:不在过滤源文件目录中，被预制体引用（{GetGameObjectListString(imageData.Value)}）");
                continue;
            }

            UpdateProcessBar("移动图片路径", imageData.Key, ++index / dict.Count);

            if (imageData.Value.Count > 1)//如果图片被多个预制体依赖
            {

                if (config.prefab2ConfigDict.TryGetValue(imageData.Value[0], out var data) && IfSetContainsList(data.m_allPrefabSet, imageData.Value))
                {
                    //如果配置文件中的预制体列表 包含 该图片所有被依赖预制体 那么 将该图片移动到 配置目录下

                    string destDir = data.m_destImageDirectory;
                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                    if (ImageOptimizeUtility.MoveAsset(imageData.Key, destDir))
                        list.Add($"from\t{imageData.Key}\tto\t{destDir}\treason:多个个UI预制体引用，移动至配置文件中的 {data.m_functionDesc} 配置文件夹下");
                }
                else
                {
                    m_waitToMoveAgainList.Add(imageData);
                    foreach (var prefab in imageData.Value)
                    {
                        if (!m_prefabDependenceCountDict.ContainsKey(prefab))
                        {
                            m_prefabDependenceCountDict.Add(prefab, 0);
                        }
                        m_prefabDependenceCountDict[prefab]++;
                    }

                    //当配置文件找不到对应文件夹 就移动到Common目录下
                    //if (ImageOptimizeUtility.MoveAsset(imageData.Key, localCommonDirectory))
                    //{
                    //      list.Add($" from\t{imageData.Key}\tto\t{localCommonDirectory}\treason:多个个UI预制体引用（{GetGameObjectListString(imageData.Value)}），未找到包含图片所有引用预制体的配置，移动至Common目录下");
                    //}
                }

            }
            else//如果图片被单个个预制体依赖，先检查是否有相关配置文件，如果没有就创建一个预制体同名的文件夹并移动到该预制体到该文件夹下
            {

                if(imageData.Value.Count>0)
                {
                    var singlePrefab = imageData.Value[0];

                    if (config.prefab2ConfigDict.TryGetValue(singlePrefab, out var data))
                    {
                        string destDir = data.m_destImageDirectory;
                        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                        if (ImageOptimizeUtility.MoveAsset(imageData.Key, destDir))
                            list.Add($"from\t{imageData.Key}\tto\t{destDir}\treason:单个UI预制体引用，移动至配置文件中的配置文件夹下");
                    }
                    /*
                    else
                    {
                        //string prefabDirectoryName = $"{m_CommonDirectory}/{singlePrefab.name}";
                        string prefabDirectoryName = $"{MainArtistDir}{MainUIDir}/{ CommonMoveDir}/Common_{singlePrefab.name}";

                        if (!Directory.Exists(prefabDirectoryName))
                            Directory.CreateDirectory(prefabDirectoryName);

                        if (ImageOptimizeUtility.MoveAsset(imageData.Key, prefabDirectoryName))
                            list.Add($"from\t{imageData.Key}\tto\t{prefabDirectoryName}\treason:单个UI预制体引用，移动至预制体同名文件夹下");
                    }
                    */
                }


            }
        }


        MoveImageAgain(m_waitToMoveAgainList, list);//再次移动

        EditorUtility.ClearProgressBar();

        list.Sort();

        string filePath = $"{Application.dataPath}/被依赖图片移动日志_{System.DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream fs = File.Create(filePath);

        StreamWriter sw = new StreamWriter(fs);
        foreach (var item in list)
        {
            sw.WriteLine(item);
        }
        sw.Flush();
        sw.Close();
        fs.Close();
    }

    /// <summary>
    /// 如果 被多个预制体引用 而且 配置文件 又找不到配置 那么 再次 移动到Common下的某个目录下
    /// </summary>
    /// <param name="data"></param>
    void MoveImageAgain(List<KeyValuePair<string, List<GameObject>>> data,List<string> debugMsgList)
    {
        foreach (var item in data)
        {
            List<PrefabDependenceCountData> list = new List<PrefabDependenceCountData>();

            foreach (var prefab in item.Value)
            {
                if (m_prefabDependenceCountDict.TryGetValue(prefab, out int count))
                {
                    list.Add(new PrefabDependenceCountData()
                    {
                        prefab = prefab,
                        dependceCount = count,
                    });
                }
            }

            list.Sort((PrefabDependenceCountData a, PrefabDependenceCountData b) => -(a.dependceCount.CompareTo(b.dependceCount)));

            var first = list[0];

            string prefabDirectoryName = $"{MainArtistDir}{MainUIDir}/{CommonMoveDir}/Common_{first.prefab.name}";

            if (!Directory.Exists(prefabDirectoryName))
                Directory.CreateDirectory(prefabDirectoryName);

            if (ImageOptimizeUtility.MoveAsset(item.Key, prefabDirectoryName))
                debugMsgList.Add($"from\t{item.Key}\tto\t{prefabDirectoryName}\treason:多个个UI预制体引用，配置文件找不到配置，移动至 Misc_Common 最多引用预制体同名文件夹下");


        }
    }


    class PrefabDependenceCountData
    {
        public GameObject prefab;
        public int dependceCount;//在上次移动操作中没有被移动的图片中 这个预制体依赖了这部分的多少图片
    }


    /// <summary>
    /// 获取 目录下 被单个 预制体依赖的 图片
    /// </summary>
    /// <param name="directory"></param>
    //List<ImageDependencyData> MoveSingleDependedImageInDirectory(Dictionary<string, List<GameObject>> dict,string directory , SameFunctionUIPrefabConfig config)
    //{
    //    int index = directory.IndexOf("Assets/");
    //    if (index > 0)
    //        directory = directory.Substring(index);

    //    List<ImageDependencyData> result = new List<ImageDependencyData>();
    //    foreach (var imageData in dict)
    //    {

    //        if (imageData.Key.StartsWith(directory) && imageData.Value.Count == 1)
    //        {
    //            GameObject go = imageData.Value[0];

    //            var imageFilesSet = m_prefab2AllDependencyImageFileDict[go];
    //            Dictionary<string, int> directory2DenpendCountDict = new Dictionary<string, int>();


    //            foreach (var imageFile in imageFilesSet)
    //            {
    //                int lastIndex = imageFile.LastIndexOf('/');
    //                if(lastIndex > 0)
    //                {
    //                    string dir = imageFile.Substring(0,lastIndex);
    //                    if(directory2DenpendCountDict.TryGetValue(dir,out int count))
    //                    {
    //                        directory2DenpendCountDict[dir] = count + 1;
    //                    }
    //                    else
    //                    {
    //                        directory2DenpendCountDict.Add(dir, 1);
    //                    }
    //                }
    //            }

    //            int maxCount = 0;
    //            string maxDir = "";
    //            foreach (var dependData in directory2DenpendCountDict)
    //            {
    //                if(dependData.Value > maxCount)
    //                {
    //                    maxCount = dependData.Value;
    //                    maxDir = dependData.Key;
    //                }
                        
    //            }


    //            result.Add(new ImageDependencyData()
    //            {
    //                path = imageData.Key,
    //                gameObjectName = go.name,
    //                mostImageDirectory = maxDir,
    //            });

    //        }
    //    }
    //    return result;
    //}

    //class ImageDependencyData
    //{
    //    public string path;
    //    public string gameObjectName;
    //    public string mostImageDirectory;//依赖图片 最多 的 目录
    //}


    ///// <summary>
    ///// 將 Common目錄下 被預製體 單一依賴 的 圖片 移動到 對應預製體 依賴圖片 最多 的 目錄 下
    ///// </summary>
    ///// <param name="data"></param>
    //void MoveSingledependImageIntoOtherFolder(List<ImageDependencyData> data)
    //{
    //    int index = m_CommonDirectory.IndexOf("Assets/");
    //    string shortDir = "";
    //    if (index > 0)
    //    {
    //        shortDir = m_CommonDirectory.Substring(index);
    //    }
    //    else
    //    {
    //        Debug.LogError("Common 目录 不在Assets 目录下");
    //        return;
    //    }



    //    List<string> msgList = new List<string>();

    //    foreach (var item in data)
    //    {
    //        if (item.mostImageDirectory.StartsWith(shortDir))//如果 预制体 依赖图片 最多 的 目录 仍然  是 Common 目录 那先不管
    //        {
    //            msgList.Add($"--------{item.path}\t暂时不移动 被 {item.gameObjectName}\t单一依赖\n");
    //        }
    //        else
    //        {
    //            msgList.Add($"{item.path}\t移动至\t{item.mostImageDirectory} \t被 {item.gameObjectName}\t单一依赖\n");
    //            ImageOptimizeUtility.MoveAsset(item.path, item.mostImageDirectory);
    //        }
    //    }
    //    msgList.Sort();

    //    string filePath = $"{Application.dataPath}/单一依赖图片依赖移动日志.txt";
    //    if (File.Exists(filePath))
    //    {
    //        File.Delete(filePath);
    //    }
    //    FileStream fs = File.Create(filePath);

    //    StreamWriter sw = new StreamWriter(fs);

    //    sw.WriteLine("单一依赖图片依赖移动日志\n移动方式：如果是被某个预制体单一依赖，则移动到该预制体依赖图片最多的目录下\n不移动的图片是由于对应预制体锁依赖图片大都处于Common目录下，故暂时不移动\n");
        
    //    foreach (var line in msgList)
    //    {
    //        sw.WriteLine(line);
    //    }
    //    sw.Flush();
    //    sw.Close();
    //    fs.Close();
    //}

}
