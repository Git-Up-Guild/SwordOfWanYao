using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using System.Drawing;
using Color = UnityEngine.Color;

namespace SpriteAtlasSearchTool
{
    /// <summary>
    /// 处理类型
    /// </summary>
    enum ProcessType
    {
        计算单个预制体,
        计算多个预制体,
        计算文件夹下的预制体,
        目录结构调整,
        计算图集占用率,
        根据GUID查找资源路径,
    }

    enum Resolution
    {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
    }

    class PrefabDrawData
    {
        public string prefabName;//预制体 名称
        public HashSet<string> directorySet;//依赖图片目录集合
        public List<string> dependenciesImageFileList;//依赖图片集合
        public PrefabDrawData()
        {
            this.directorySet = new HashSet<string>();
            this.dependenciesImageFileList = new List<string>();
        }
    }

    class SpriteAtlasDrawData
    {
        public SpriteAtlas spriteAtlas;
        public int fillSize;
        public Resolution width;
        public Resolution height;
        public SpriteAtlasDrawData()
        {
            this.width = Resolution._128;
            this.height = Resolution._128;
        }
    }

    public class SpriteAtlasSearchTool : EditorWindow
    {
        #region 初始化窗口

        [MenuItem("XGame/资源工具/图片引用查找工具")]
        static void ShowWindow()
        {
            GetWindow<SpriteAtlasSearchTool>();
        }

        public SpriteAtlasSearchTool()
        {
            this.titleContent = new GUIContent("图集引用查找工具");
            this.minSize = new Vector2(750f,512f);
            this.m_processType = ProcessType.计算单个预制体;
        }

        #endregion

        //以单个预制体计算
        //多个预制体以单个预制体计算
        //遍历文件夹下的所有预制体计算

        Color DarkGreenColor = new Color(0f,0.75f,0f,1f);
        Color DarkRedColor = new Color(0.75f, 0f, 0f, 1f);

        ProcessType m_processType;

        List<string> m_ignoreDirectorys;//过滤文件夹
        List<string> m_shortIgnoreDirectorys;//简短 忽略 文件夹 目录 （因为 获得 的 图片 路径 是 相对路径）

        #region 计算单个预制体相关变量
        GameObject m_singleGameObject;
        PrefabDrawData m_singlePrefabDrawData;//单个预制体的绘制数据
        #endregion

        #region 计算多个预制体相关变量

        List<GameObject> m_combineGameObjects;//合并输出 的 预制体 列表
        PrefabDrawData m_combinePrefabDrawData;//合并输出 的预制体 的 绘制数据

        List<string> m_processPrefabsDirectories;//处理多个预制体 的 文件夹
        List<string> m_imagesDirectories;//删除未引用图片 的 文件夹 列表
        #endregion

        Vector2 m_scroll;
        #region  计算文件夹下的预制体相关变量

        List<string> m_ignoreImagesFilesDirectory;//忽略 未引用图片文件夹
        List<string> m_shortIgnoreImagesFilesDirectorys;//简短 忽略 未引用图片文件夹 目录 （因为 获得 的 图片 路径 是 相对路径）
        List<PrefabDrawData> m_prefabDrawDataList;//预制体文件夹中 预制体列表 的 绘制数据

        HashSet<string> m_allImageFilesDependenciesSet;//所有 预制体 依赖 图片集合
        HashSet<string> m_allImageFilesInDirectorySet;//图片文件夹下 所有 图片集合
        HashSet<string> m_unRefImageFilesSet;//图片文件夹下 未被引用的 图片集合

        string m_outputPrefabImageDependenciesPath;//所有预制体 图片依赖 csv 目录
        string m_outputUnrefImageFilePath;//未被引用的 图片 csv 目录

        #endregion

        #region 目录结构调整相关变量
        List<string> m_adjustDirectoryList;//调整目录列表
        #endregion

        #region 计算图集占用率相关变量
        List<SpriteAtlasDrawData> m_processSpriteAtlas;//等待处理 的 图集 列表
        #endregion

        string m_searchGuid = "";

        private void OnEnable()
        {
            this.m_ignoreDirectorys = new List<string>()
            {
                Application.dataPath + "/Game/ImmortalFamily/GameResources/Artist/Icon",
                Application.dataPath + "/Game/ImmortalFamily/GameResources/Artist/UI/Common",
            };

            this.m_adjustDirectoryList = new List<string>
            {
                Application.dataPath + "/G_Resources/Artist/UI1.0/Common",
            };

            this.m_ignoreImagesFilesDirectory = new List<string>()
            {
                Application.dataPath + "/G_Resources/Artist/UI1.0/Common",
            };

            UpdateShortIgnoreDirectoryList();
            UpdateShortIgnoreImageFilesDirectoryList();

            this.m_processPrefabsDirectories = new List<string>
            {
                 Application.dataPath + "/Game/ImmortalFamily/GameResources",
                Application.dataPath + "/G_Resources/Game/Prefab",
               
            };

            this.m_imagesDirectories =  new List<string> 
            {
                Application.dataPath + "/G_Resources/Artist/UI" ,
                Application.dataPath + "/Game/ImmortalFamily/GameResources/Artist/UI",
            };

            this.m_combineGameObjects = new List<GameObject>() { null };
            this.m_processSpriteAtlas = new List<SpriteAtlasDrawData>();

            differentResolutionArr = new int[23];
            for (int i = 0; i < differentResolutionArr.Length; i++)
            {
                differentResolutionArr[i] = 1 << i;
            }
        }

        private void OnGUI()
        {

            m_processType = (ProcessType)EditorGUILayout.EnumPopup("查询类型：", m_processType);

            if(m_processType == ProcessType.计算单个预制体 || m_processType == ProcessType.计算多个预制体 || m_processType == ProcessType.计算文件夹下的预制体)
            {
                GUILayout.Space(16f);
                if (GUILayout.Button("增加忽略目录（+）(预制体依赖图片结果中将过滤掉在忽略目录下的文件)"))
                {
                    m_ignoreDirectorys.Add("");
                }
                for (int i = 0; i < this.m_ignoreDirectorys.Count; i++)
                {
                    using (var hor = new GUILayout.HorizontalScope())
                    {
                        this.m_ignoreDirectorys[i] = EditorGUILayout.TextField("", this.m_ignoreDirectorys[i]);
                        if (GUILayout.Button("浏览",EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                        {
                            this.m_ignoreDirectorys[i] = EditorUtility.OpenFolderPanel("忽略文件夹", this.m_ignoreDirectorys[i], "");
                            UpdateShortIgnoreDirectoryList();
                        }
                        if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                        {
                            this.m_ignoreDirectorys.RemoveAt(i);
                            UpdateShortIgnoreDirectoryList();
                        }
                    }
                }
                GUILayout.Space(16);
            }

            
            switch(m_processType)
            {
                case ProcessType.计算单个预制体:
                    {
                        m_singleGameObject = EditorGUILayout.ObjectField("预制体", m_singleGameObject, typeof(GameObject), false) as GameObject;

                        if (GUILayout.Button("计算单个预制体所有依赖图片"))
                        {
                            SetupSinglePrefabImageDependencies();
                        }

                        if (m_singlePrefabDrawData != null)
                        {
                            DrawData(m_singlePrefabDrawData);
                        }

                        break;
                    }
                case ProcessType.计算多个预制体:
                    { 
                        if (GUILayout.Button("增加预制体（+）"))
                        {
                            m_combineGameObjects.Add(null);
                        }

                        for (int i = 0; i < this.m_combineGameObjects.Count; i++)
                        {
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_combineGameObjects[i] = EditorGUILayout.ObjectField(this.m_combineGameObjects[i] , typeof(GameObject) , false) as GameObject;
                            
                                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_combineGameObjects.RemoveAt(i);
                                }
                            }
                        }

                        if (GUILayout.Button("计算合并多个预制体所有依赖图片"))
                        {
                            SetupMultiPrefabImageDependencies();
                        }
                        if (m_combinePrefabDrawData != null)
                        {
                            DrawData(m_combinePrefabDrawData);
                        }
                        break;
                    }
                case ProcessType.计算文件夹下的预制体:
                    { 

                        if(GUILayout.Button("增加 预制体文件夹（+）"))
                        {
                            m_processPrefabsDirectories.Add("");
                        }
                        for (int i = 0; i < m_processPrefabsDirectories.Count; i++)
                        {
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_processPrefabsDirectories[i] = EditorGUILayout.TextField("预制体文件夹：", this.m_processPrefabsDirectories[i]);
                                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_processPrefabsDirectories[i] = EditorUtility.OpenFolderPanel("预制体文件夹", this.m_processPrefabsDirectories[i], "");
                                }
                                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_processPrefabsDirectories.RemoveAt(i);
                                }
                            }
                        }
                        

                        if (GUILayout.Button("增加 处理图片 文件夹（+）"))
                        {
                            this.m_imagesDirectories.Add("");
                        }
                        for(int i = 0; i < this.m_imagesDirectories.Count; i ++)
                        {
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_imagesDirectories[i] = EditorGUILayout.TextField("处理图片文件夹：", this.m_imagesDirectories[i]);
                                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_imagesDirectories[i] = EditorUtility.OpenFolderPanel("处理图片文件夹", this.m_imagesDirectories[i], "");
                                }
                            }
                        }
                        

                        if (GUILayout.Button("计算预制体所依赖图片X图片文件夹中所有图片"))
                        {
                            SetupMultiPrefabImageDependenciesInDirectory();
                            m_allImageFilesInDirectorySet = GetImageFileInDirectory(this.m_imagesDirectories);

                            m_unRefImageFilesSet = GetUnRefImageFilesSet(m_allImageFilesInDirectorySet, m_allImageFilesDependenciesSet, this.m_shortIgnoreImagesFilesDirectorys);
                        }

                        if (m_prefabDrawDataList != null && m_prefabDrawDataList.Count > 0)
                        {
                            DrawData(m_prefabDrawDataList);
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_outputPrefabImageDependenciesPath = EditorGUILayout.TextField("写入预制体依赖图片CSV文件路径", this.m_outputPrefabImageDependenciesPath);
                                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_outputPrefabImageDependenciesPath = EditorUtility.OpenFolderPanel("写入预制体依赖图片CSV文件路径", m_outputPrefabImageDependenciesPath, "");
                                    
                                }
                                if (GUILayout.Button("写入", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    WritePrefabImageDependenciesIntoCSV(m_prefabDrawDataList, m_outputPrefabImageDependenciesPath);
                                }
                            }
                        }

                        GUILayout.Space(16);

                        if (GUILayout.Button("增加忽略删除图片目录（+）（处于该目录下的图片文件将不会列入删除列表中）"))
                        {
                            m_ignoreImagesFilesDirectory.Add("");
                        }

                        for (int i = 0; i < this.m_ignoreImagesFilesDirectory.Count; i++)
                        {
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_ignoreImagesFilesDirectory[i] = EditorGUILayout.TextField("", this.m_ignoreImagesFilesDirectory[i]);
                                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_ignoreImagesFilesDirectory[i] = EditorUtility.OpenFolderPanel("忽略图片文件夹", this.m_ignoreImagesFilesDirectory[i], "");
                                    UpdateShortIgnoreImageFilesDirectoryList();
                                }
                                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_ignoreImagesFilesDirectory.RemoveAt(i);
                                    UpdateShortIgnoreImageFilesDirectoryList();
                                }
                            }
                        }

                        if(m_unRefImageFilesSet != null)
                        {
                            if (m_unRefImageFilesSet.Count > 0)
                            {
                                using (var hor = new GUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button("批量删除未被引用的图片资源（-）"))
                                    {
                                        DeleteFileInSet(m_unRefImageFilesSet);
                                    }
                                    if (GUILayout.Button("选择删除未被引用的图片资源（-）"))
                                    {
                                        DeleteFileInSet(m_unRefImageFilesSet);
                                    }
                                }
                                m_scroll_3 = GUILayout.BeginScrollView(m_scroll_3);
                                foreach (var item in m_unRefImageFilesSet)
                                {
                                    GUILayout.Label(item);
                                }
                                GUILayout.EndScrollView();


                                using (var hor = new GUILayout.HorizontalScope())
                                {
                                    this.m_outputUnrefImageFilePath = EditorGUILayout.TextField("写入预制体未依赖图片CSV文件路径", this.m_outputUnrefImageFilePath);
                                    if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                    {
                                        this.m_outputUnrefImageFilePath = EditorUtility.OpenFolderPanel("写入预制体未依赖图片CSV文件路径", m_outputUnrefImageFilePath, "");

                                    }
                                    if (GUILayout.Button("写入", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                    {
                                        WriteUnrefImageFilesIntoCSV(m_unRefImageFilesSet, m_outputUnrefImageFilePath);
                                    }
                                }
                            }
                        }
                    


                        break;
                    }
                case ProcessType.目录结构调整:
                    {
                        if (GUILayout.Button("增加调整目录目录（+）(目录下的图片文件将会移动到对应目录下)"))
                        {
                            m_adjustDirectoryList.Add("");
                        }
                        for (int i = 0; i < this.m_adjustDirectoryList.Count; i++)
                        {
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_adjustDirectoryList[i] = EditorGUILayout.TextField("", this.m_adjustDirectoryList[i]);
                                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_adjustDirectoryList[i] = EditorUtility.OpenFolderPanel("调整文件夹", this.m_adjustDirectoryList[i], "");
                                
                                }
                                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_adjustDirectoryList.RemoveAt(i);
                                }
                            }
                        }
                        if(GUILayout.Button("删除空目录"))
                        {
                            foreach (var item in m_adjustDirectoryList)
                            {
                                //TransformImageFilesIntoDirectory(item);
                                //CreateSpriteAtlasFile(item);
                                DeleteEmptyDirectory(item);
                            }
                        }
                        break;
                    }
                case ProcessType.计算图集占用率:
                    { 
                        if(GUILayout.Button("增加图集（+）"))
                        {
                            m_processSpriteAtlas.Add(new SpriteAtlasDrawData());
                        }
                        for (int i = 0; i < this.m_processSpriteAtlas.Count; i++)
                        {
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                this.m_processSpriteAtlas[i].spriteAtlas = EditorGUILayout.ObjectField("", this.m_processSpriteAtlas[i].spriteAtlas,typeof(SpriteAtlas),true) as SpriteAtlas;
                            
                                if (this.m_processSpriteAtlas[i].fillSize > 0)
                                {
                                    GUILayout.Label("手动确认图集大小ヽ(✿ﾟ▽ﾟ)ノ");
                                    this.m_processSpriteAtlas[i].width = (Resolution)EditorGUILayout.EnumPopup(this.m_processSpriteAtlas[i].width);
                                    GUILayout.Label("X");
                                    this.m_processSpriteAtlas[i].height = (Resolution)EditorGUILayout.EnumPopup(this.m_processSpriteAtlas[i].height);
                                    float rate = (float)(this.m_processSpriteAtlas[i].fillSize) / (float)((int)this.m_processSpriteAtlas[i].width * (int)this.m_processSpriteAtlas[i].height);
                                    GUILayout.Label($"图集填充率：{Math.Round(rate * 100f,2)}%");

                                }

                                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                                {
                                    this.m_processSpriteAtlas.RemoveAt(i);
                                }
                            }
                        }

                        if(GUILayout.Button("计算图集占用率"))
                        {
                            GetSpriteAtlasSpaceRate(this.m_processSpriteAtlas);
                        }
                        break;
                    }

                case ProcessType.根据GUID查找资源路径:
                    {
                        m_searchGuid = GUILayout.TextField( m_searchGuid);
                        if(GUILayout.Button("查找资源路径"))
                        {
                            string path = AssetDatabase.GUIDToAssetPath(m_searchGuid);
                            Debug.Log(path);
                        }
                        break;
                    }
            }

        }

        #region 之前写的方法 

       

        void UpdateProcessInfo(string title,string desc,float rate)
        {
            if(rate >= 0.99f)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar(title, $"{desc}:{rate * 100}%...", rate);
            }
        }

        void ShowLabelWithColor(string text,Color color)
        {
            EditorStyles.boldLabel.normal.textColor = color;
            GUILayout.Label(text, EditorStyles.boldLabel);
            EditorStyles.boldLabel.normal.textColor = Color.black;
        }

        void DrawData(PrefabDrawData data)
        {

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            ShowLabelWithColor(data.prefabName, data.directorySet.Count > 1 ? DarkRedColor : DarkGreenColor);

           
            GUILayout.EndHorizontal();

            GUILayout.Label("所有图片依赖（除忽略文件夹外）");
            this.m_scroll_1 = EditorGUILayout.BeginScrollView(this.m_scroll_1);
            foreach (var item in data.dependenciesImageFileList)
            {
                if(CheckDirectoryIsSimilar(item,data.prefabName))
                {
                    EditorStyles.boldLabel.normal.textColor = Color.green;
                    if(GUILayout.Button($"\t{item}（目录与预制体名称可能相似）", EditorStyles.boldLabel))
                    {
                        SelectCurObjByPath(item);
                    }
                    EditorStyles.boldLabel.normal.textColor = Color.black;
                }
                else
                {
                    if (GUILayout.Button("\t" + item, EditorStyles.label))
                    {
                        SelectCurObjByPath(item);
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.EndVertical();
           
        }

        Vector2 m_scroll_1, m_scroll_2, m_scroll_3;
        void DrawData(List<PrefabDrawData> data)
        {
            GUILayout.BeginVertical();
            this.m_scroll_2 = EditorGUILayout.BeginScrollView(this.m_scroll_2);
            foreach (var item in data)
            {

                ShowLabelWithColor(item.prefabName, item.directorySet.Count > 1 ? DarkRedColor : DarkGreenColor);



                GUILayout.Label("所有图片依赖（除忽略文件夹外）");
                foreach (var imageName in item.dependenciesImageFileList)
                {
                    if (CheckDirectoryIsSimilar(imageName, item.prefabName))
                    {
                        EditorStyles.boldLabel.normal.textColor = Color.green;
                        GUILayout.Label($"\t{imageName}（目录与预制体名称可能相似）",EditorStyles.boldLabel);
                        EditorStyles.boldLabel.normal.textColor = Color.black;
                    }
                    else
                    {
                        GUILayout.Label("\t" + imageName);
                    }
                }

            }


            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void SelectCurObjByPath(string path)
        {
            Object obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null)
                return;
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }


        /// <summary>
        /// 获取 目录下 所有图片文件（.png后缀）
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        HashSet<string> GetImageFileInDirectory(List<string> directories)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (var dir in directories)
            {
                string[] arr = Directory.GetFiles(dir,"*.???",SearchOption.AllDirectories);
                for (int i = 0; i < arr.Length; i++)
                {
                    if (IsImageFile(arr[i]))
                    {
                        arr[i] = arr[i].Replace('\\', '/');
                        int splitIndex = arr[i].IndexOf("Assets");
                        if (splitIndex >= 0)
                        {
                            set.Add(arr[i].Substring(splitIndex));
                        }
                        else
                        {
                            Debug.LogError("WTF");

                        }
                    }

                }
            }
            
            return set;
        }

        /// <summary>
        /// 获取 目录下 所有 预制体
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        GameObject[] GetPrefabsInDirectory(List<string> directories)
        {
            List<GameObject> prefabs = new List<GameObject>();

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Debug.LogErrorFormat("文件夹路径不存在 {0}", dir);
                    return prefabs.ToArray();
                }
                string[] prefabNames = Directory.GetFiles(dir, "*.prefab", SearchOption.AllDirectories);//绝对路径
                for (int i = 0; i < prefabNames.Length; i++)
                {
                    prefabNames[i] = prefabNames[i].Replace('\\', '/');//统一成 斜线
                }
                for (int i = 0; i < prefabNames.Length; i++)//转换成 相对路径（因为AssetDataBase读取路径 为 相对路径）
                {
                    int index = prefabNames[i].IndexOf("Assets/");
                    if (index < 0)
                    {
                        Debug.LogErrorFormat("预制体路径错误 {0}", prefabNames[i]);
                        return null;
                    }
                    prefabNames[i] = prefabNames[i].Substring(index);
                }


                for (int i = 0; i < prefabNames.Length; i++)
                {
                    UpdateProcessInfo("获取预制体", "获取预制体中", (float)(i + 1) / prefabNames.Length);

                    GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabNames[i], typeof(GameObject)) as GameObject;
                    if (prefab != null)
                        prefabs.Add(prefab);
                    else
                        Debug.Log("预制体不存在！ " + prefabNames[i]);
                }
                UpdateProcessInfo("获取预制体", "获取预制体完成", 1f);
            }

            
            return prefabs.ToArray();
        }

        /// <summary>
        /// 获得 游戏物体 的 路径（从 UIImageRefAltasChecker 那 Copy 过来的）
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string GetPrefabAssetPath(GameObject go)
        {
            if (go == null) return null;

            // Project中的Prefab是Asset不是Instance
            if (PrefabUtility.IsPartOfPrefabAsset(go))
            {
                // 预制体资源就是自身
                return AssetDatabase.GetAssetPath(go);
            }

            // Scene中的Prefab Instance是Instance不是Asset
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                // 获取预制体资源
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                return AssetDatabase.GetAssetPath(prefabAsset);
            }

            // PrefabMode中的GameObject既不是Instance也不是Asset
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(go);
            if (prefabStage != null)
            {
                // 预制体资源：prefabAsset = prefabStage.prefabContentsRoot
                return prefabStage.prefabAssetPath;
            }

            //在Run-Time时，
            int instID = go.GetInstanceID();
            return AssetDatabase.GetAssetPath(instID);
        }

        /// <summary>
        /// 获得 单个预制体 的 依赖文件 路径
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string[] GetSinglePrefabDependencies(GameObject go)
        {
            string goPath = GetPrefabAssetPath(go);
            return AssetDatabase.GetDependencies(goPath);
        }

        /// <summary>
        /// 获取 多个预制体 的 依赖文件 路径（依赖文件可能会重复）
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string[] GetMultiPrefabDependencies(GameObject[] gos)
        {
            string[] gosPath = new string[gos.Length];

            for (int i = 0; i < gosPath.Length; i++)
            {
                gosPath[i] = GetPrefabAssetPath(gos[i]);
            }

            return AssetDatabase.GetDependencies(gosPath);
        }

        /// <summary>
        /// 1.获取 依赖文件中 的所有 图片依赖文件
        /// 2.同时 过滤 重复文件（保证 返回数组中 元素 唯一）
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public static string[] GetPrefabImageDependencies(string[] dependencies)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < dependencies.Length; i++)
            {
                if (IsImageFile(dependencies[i]) && !result.Contains(dependencies[i]))//如果 依赖文件 是 图片 且 结果中未存在 该文件
                    result.Add(dependencies[i]);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 文件是否 为 图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static bool IsImageFile(string filePath)
       {
            return filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".tga");
       }

        

        void SetupSinglePrefabImageDependencies()
        {
            

            if (m_singleGameObject != null)
            {
                UpdateProcessInfo("获取预制体依赖", $"{m_singleGameObject.name}", 0f);
                string[] allDependencies = GetSinglePrefabDependencies(m_singleGameObject);
                UpdateProcessInfo("获取预制体依赖", $"{m_singleGameObject.name}", 1f);

                UpdateProcessInfo("获取预制体图片依赖", $"{m_singleGameObject.name}", 0f);
                string[] allImageDependencies = GetPrefabImageDependencies(allDependencies);
                UpdateProcessInfo("获取预制体图片依赖", $"{m_singleGameObject.name}", 1f);

                UpdateProcessInfo("排序图片路径", $"{m_singleGameObject.name}", 0f);
                Array.Sort(allImageDependencies);
                UpdateProcessInfo("排序图片路径", $"{m_singleGameObject.name}", 1f);

                

                HashSet<string> directoriesSet = new HashSet<string>();
                List<string> dependenciesImageFilesList = new List<string>();
                for (int i = 0; i < allImageDependencies.Length; i++)
                {
                    if (IsFileIndirectory(allImageDependencies[i], this.m_shortIgnoreDirectorys))
                        continue;

                    dependenciesImageFilesList.Add(allImageDependencies[i]);

                    int lastSymbolIndex = allImageDependencies[i].LastIndexOf('/');
                    if (lastSymbolIndex > 0)
                    {
                        string dir = allImageDependencies[i].Substring(0, lastSymbolIndex);
                        directoriesSet.Add(dir);
                    }
                    else
                    {
                        directoriesSet.Add(allImageDependencies[i]);
                    }

                }

                m_singlePrefabDrawData = new PrefabDrawData();
                m_singlePrefabDrawData.prefabName = m_singleGameObject.name;
                m_singlePrefabDrawData.directorySet = directoriesSet;
                m_singlePrefabDrawData.dependenciesImageFileList = dependenciesImageFilesList;
            }
            else
            {
                Debug.LogError("预制体为空");
            }
        }

        void SetupMultiPrefabImageDependencies()
        {
            if (m_combineGameObjects.Count > 0)
            {
                UpdateProcessInfo("获取组合预制体依赖", "", 0f);
                string[] allDependencies = GetMultiPrefabDependencies(m_combineGameObjects.ToArray());
                UpdateProcessInfo("获取组合预制体依赖", "", 1f);

                UpdateProcessInfo("获取组合预制体图片依赖", "", 0f);
                string[] allImageDependencies = GetPrefabImageDependencies(allDependencies);
                UpdateProcessInfo("获取组合预制体图片依赖", "", 1f);

                UpdateProcessInfo("排序图片路径", "", 0f);
                Array.Sort(allImageDependencies);
                UpdateProcessInfo("排序图片路径", "", 1f);


                HashSet<string> directoriesSet = new HashSet<string>();
                List<string> dependenciesImageFilesList = new List<string>();

                for (int i = 0; i < allImageDependencies.Length; i++)
                {
                    if (IsFileIndirectory(allImageDependencies[i], this.m_shortIgnoreDirectorys))
                        continue;

                    dependenciesImageFilesList.Add(allImageDependencies[i]);

                    int lastSymbolIndex = allImageDependencies[i].LastIndexOf('/');
                    if (lastSymbolIndex > 0)
                    {
                        string dir = allImageDependencies[i].Substring(0, lastSymbolIndex);
                        directoriesSet.Add(dir);
                    }
                    else
                    {
                        directoriesSet.Add(allImageDependencies[i]);
                    }

                }

                m_combinePrefabDrawData = new PrefabDrawData();
                m_combinePrefabDrawData.prefabName = "合并预制体";
                m_combinePrefabDrawData.directorySet = directoriesSet;
                m_combinePrefabDrawData.dependenciesImageFileList = dependenciesImageFilesList;

            }
            else
            {
                Debug.LogError("预制体列表为空");
            }

            
        }

        void SetupMultiPrefabImageDependenciesInDirectory()
        {
            GameObject[] allGameObjectsInDirectory = GetPrefabsInDirectory(m_processPrefabsDirectories);

            if(allGameObjectsInDirectory.Length > 0 )
            {
                m_prefabDrawDataList = new List<PrefabDrawData>();

                m_allImageFilesDependenciesSet = new HashSet<string>();

                for (int i = 0; i < allGameObjectsInDirectory.Length; i++)
                {
                    UpdateProcessInfo("获取预制体依赖", $"{allGameObjectsInDirectory[i].name}", 0f);
                    string[] allDependencies = GetSinglePrefabDependencies(allGameObjectsInDirectory[i]);
                    UpdateProcessInfo("获取预制体依赖", $"{allGameObjectsInDirectory[i].name}", 1f);


                    UpdateProcessInfo("获取预制体图片依赖", $"{allGameObjectsInDirectory[i].name}", 0f);
                    string[] allImageDependencies = GetPrefabImageDependencies(allDependencies);
                    UpdateProcessInfo("获取预制体图片依赖", $"{allGameObjectsInDirectory[i].name}", 1f);

                    UpdateProcessInfo("排序图片路径", $"{allGameObjectsInDirectory[i].name}", 0f);
                    Array.Sort(allImageDependencies);
                    UpdateProcessInfo("排序图片路径", $"{allGameObjectsInDirectory[i].name}", 1f);



                    HashSet<string> directoriesSet = new HashSet<string>();
                    List<string> dependenciesImageFilesList = new List<string>();
                    for (int j = 0; j < allImageDependencies.Length; j++)
                    {
                        if (IsFileIndirectory(allImageDependencies[j], this.m_shortIgnoreDirectorys))
                            continue;

                        dependenciesImageFilesList.Add(allImageDependencies[j]);

                        int lastSymbolIndex = allImageDependencies[j].LastIndexOf('/');
                        if (lastSymbolIndex > 0)
                        {
                            string dir = allImageDependencies[j].Substring(0, lastSymbolIndex + 1);
                            directoriesSet.Add(dir);
                        }
                        else
                        {
                            directoriesSet.Add(allImageDependencies[j]);
                        }

                        m_allImageFilesDependenciesSet.Add(allImageDependencies[j]);

                    }

                    PrefabDrawData prefabDrawData = new PrefabDrawData();
                    prefabDrawData.prefabName = allGameObjectsInDirectory[i].name;
                    prefabDrawData.directorySet = directoriesSet;
                    prefabDrawData.dependenciesImageFileList = dependenciesImageFilesList;

                    m_prefabDrawDataList.Add(prefabDrawData);
                }


            }
            else
            {
                Debug.LogError("当前目录下未找到预制体");
            }
        }

        /// <summary>
        /// 设置简短 忽略 文件夹 列表
        /// </summary>
        void UpdateShortIgnoreDirectoryList()
        {
            if (m_shortIgnoreDirectorys == null)
                m_shortIgnoreDirectorys = new List<string>();

            this.m_shortIgnoreDirectorys.Clear();
            foreach (var item in m_ignoreDirectorys)
            {
                int splitIndex = item.IndexOf("Assets");
                if (splitIndex >= 0)
                {
                    m_shortIgnoreDirectorys.Add(item.Substring(splitIndex));
                }
                else
                {
                    Debug.LogError("WTF");

                }
            }
        }

        void UpdateShortIgnoreImageFilesDirectoryList()
        {
            
            if (m_shortIgnoreImagesFilesDirectorys == null)
                m_shortIgnoreImagesFilesDirectorys = new List<string>();

            this.m_shortIgnoreImagesFilesDirectorys.Clear();
            foreach (var item in m_ignoreImagesFilesDirectory)
            {
                int splitIndex = item.IndexOf("Assets");
                if (splitIndex >= 0)
                {
                    m_shortIgnoreImagesFilesDirectorys.Add(item.Substring(splitIndex));
                }
                else
                {
                    Debug.LogError("WTF");

                }
            }
        }

        /// <summary>
        /// 检测 文件 是否 在 目录列表 中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="directorys"></param>
        /// <returns></returns>
        bool IsFileIndirectory(string filePath , List<string> directorys)
        {
            foreach (var item in directorys)
            {
                if (filePath.StartsWith(item))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 得到 文件夹下 未被 依赖的 图片集合，同时过滤 图标表中的 图片
        /// </summary>
        /// <param name="allImageSet"></param>
        /// <param name="haveRefImageSet"></param>
        /// <param name="iconImageSet"></param>
        /// <returns></returns>
        HashSet<string> GetUnRefImageFilesSet(HashSet<string> allImageSet, HashSet<string> haveRefImageSet,List<string> ignoreImageDiretories)
        {
            HashSet<string> set = new HashSet<string>();

            foreach (var item in allImageSet)
            {
                if(!haveRefImageSet.Contains(item) && !IsFileIndirectory(item,ignoreImageDiretories))
                {

                    set.Add(item);
                }
            }

            return set;
        }

        /// <summary>
        /// 删除 set 中 的 图片 （包含.meta 文件）
        /// </summary>
        /// <param name="set"></param>
        void DeleteFileInSet(HashSet<string> set)
        {
            int index = 1;
            float count = set.Count;
            int subIndex = Application.dataPath.IndexOf("Assets");
            string frontPath = Application.dataPath.Substring(0,subIndex);
            foreach (var item in set)
            {
                string absolutePath = $"{frontPath}{item}";
                if(File.Exists(absolutePath))
                {
                    UpdateProcessInfo("删除文件", absolutePath,index / count);
                    File.Delete(absolutePath);
                    if (File.Exists($"{absolutePath}.meta"))
                    {
                        File.Delete($"{absolutePath}.meta");
                    }
                    ++index;
                }
            }
        }


        static int[] differentResolutionArr;
        
        void GetSpriteAtlasSpaceRate(List<SpriteAtlasDrawData> list)
        {
            string keyWord = "totalSpriteSurfaceArea: ";

            foreach (var item in list)
            {

                string filePath = AssetDatabase.GetAssetPath(item.spriteAtlas);

                using (FileStream fs = new FileStream(filePath,FileMode.Open,FileAccess.Read))
                {
                    byte[] readBuffer = new byte[fs.Length];
                    fs.Read(readBuffer,0, readBuffer.Length);
                    string data = Encoding.UTF8.GetString(readBuffer);

                    int startIndex = data.IndexOf(keyWord);


                    for (int i = startIndex + keyWord.Length; i < data.Length; i++)
                    {
                        if(data[i] == '\n')
                        {
                            string sizeData = data.Substring(startIndex + keyWord.Length, i - startIndex - keyWord.Length);
                            item.fillSize = int.Parse(sizeData);

                            break;
                        }
                    }

                }

            }

        }

        /// <summary>
        /// 将目录下 图片文件 转移到 该文件夹下（包括.meta 文件）
        /// </summary>
        /// <param name="directory"></param>
        void TransformImageFilesIntoDirectory(string directory)
        {
            string[] allImageFiles = Directory.GetFiles(directory,"*.???",SearchOption.AllDirectories);

            int assetDirIndex = directory.IndexOf("Assets/");

            if(assetDirIndex > 0)
            {
                string dirShortName = directory.Substring(0, assetDirIndex);
                for (int i = 0; i < allImageFiles.Length; i++)
                {
                    if (IsImageFile(allImageFiles[i]))
                    {
                        allImageFiles[i] = allImageFiles[i].Replace('\\', '/');
                        int lastSymbolIndex = allImageFiles[i].LastIndexOf('/');

                        if (lastSymbolIndex > 0)
                        {

                            string dirName = allImageFiles[i].Substring(0, lastSymbolIndex);
                            if (dirName != dirShortName)//如果 改文件 不在该目录下 移动到 该目录下
                            {

                                MoveAsset(allImageFiles[i], directory);
                            }

                               
                        }

                    }

                }
            }






        }


        void MoveAsset(string src, string destDir)
        {
            string fileName = src;
            int lastSymbolIndex = src.LastIndexOf('/');
            if (lastSymbolIndex > 0)
            {
                fileName = src.Substring(lastSymbolIndex + 1);
            }
            string destPath = $"{destDir}/{fileName}";
            if (File.Exists(destPath))
            {
                //该目录下已存在同名文件
                int index = 1;
                string[] fileNameSplit = fileName.Split('.');
                if (fileNameSplit.Length == 2)
                {
                    while (File.Exists($"{destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}"))
                    {
                        index++;
                    }
                    File.Move(src, $"{destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}");
                    File.Move($"{src}.meta", $"{destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}.meta");
                    Debug.Log($"重命名 {src} ==> {destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}");
                }

            }
            else
            {
                File.Move(src, destPath);
                File.Move($"{src}.meta", $"{destPath}.meta");
                Debug.Log($"移动文件{src} ==> {destPath}");
            }
        }


        bool CheckDirectoryIsSimilar(string fileName,string prefabName)
        {
            List<string> prefabNameSplit = new List<string>();
            int startIndex = 0;
            for (int i = 0; i <= prefabName.Length; i++)
            {
                if(i == prefabName.Length)
                {
                    prefabNameSplit.Add(prefabName.Substring(startIndex));
                }
                else if(char.IsUpper(prefabName[i]))
                {
                    if (i - startIndex > 1)
                    {
                        prefabNameSplit.Add(prefabName.Substring(startIndex, i - startIndex));
                    }
                    startIndex = i;
                }

            }

            int lastSymbolIndex = fileName.LastIndexOf('/');
            string directory = fileName.Substring(0, lastSymbolIndex);

            foreach (var item in prefabNameSplit)
            {
                if (directory.Contains(item))
                    return true;
            }

            return false;
        }


        void WritePrefabImageDependenciesIntoCSV(List<PrefabDrawData> list , string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            /*
            int maxRow = 0;
            foreach (var item in list)
            {
                sb.Append(item.prefabName);
                sb.Append(',');
                maxRow = Math.Max(item.dependenciesImageFileList.Count,maxRow);
            }
            sb.Append('\n');
            for (int i = 1; i <= maxRow; i++)
            {
                foreach (var item in list)
                {
                    if(item.dependenciesImageFileList.Count >= i)
                    {
                        sb.Append(item.dependenciesImageFileList[i - 1]);
                    }
                    sb.Append(',');
                }
                sb.Append('\n');
            }
            */
            foreach (var item in list)
            {
                sb.Append(item.prefabName);
                sb.Append(',');
                foreach (var imageFile in item.dependenciesImageFileList)
                {
                    sb.Append($"{imageFile},");
                }
                sb.Append('\n');
            }
            string filePath = outputPath + "/预制体依赖图片.csv";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew,FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }

        }
    
        void WriteUnrefImageFilesIntoCSV(HashSet<string> list , string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append($"{item},\n");
            }
            string filePath = outputPath + "/未引用图片.csv";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }


        void DeleteEmptyDirectory(string dir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            if (directoryInfo.Exists)
            {
                DirectoryInfo[] allChildDir = directoryInfo.GetDirectories();

                for (int i = 0; i < allChildDir.Length; i++)
                {
                    DeleteEmptyDirectory(allChildDir[i].FullName);
                }


                FileInfo[] allChildFile = directoryInfo.GetFiles();
                allChildDir = directoryInfo.GetDirectories();
                if (allChildFile.Length == 0 && allChildDir.Length == 0)
                {
                    directoryInfo.Delete();
                    File.Delete($"{dir}.meta");
                }
            }
        }

        void CreateSpriteAtlasFile(string dir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            string targetPath = $"{directoryInfo.FullName}\\{directoryInfo.Name}.spriteatlas";
            targetPath = targetPath.Replace('\\','/');

            int index = targetPath.IndexOf("Assets/");
            if(index > 0)
            {
                targetPath = targetPath.Substring(index);
                SpriteAtlas sa = new SpriteAtlas();
                AssetDatabase.CreateAsset(sa, targetPath);
            }
        }
        #endregion


        /// <summary>
        /// 检测 目录下 同名 的 图片
        /// </summary>
        /// <param name="directory"></param>
        void CheckSameImageFile(string directory)
        {
            Dictionary<string, List<string>> imageFiles2DirectoryDict = new Dictionary<string, List<string>>();

            string[] allImageFilesArr = Directory.GetFiles(directory,"*.???",SearchOption.AllDirectories);

            for (int i = 0; i < allImageFilesArr.Length; i++)
            {
                if(IsImageFile(allImageFilesArr[i]))
                {
                    int index = allImageFilesArr[i].LastIndexOf('\\');
                    if (index > 0)
                    {
                        string fileName = allImageFilesArr[i].Substring(index);
                        if (imageFiles2DirectoryDict.ContainsKey(fileName))
                        {
                            imageFiles2DirectoryDict.Add(fileName, new List<string>() { allImageFilesArr[i] });
                        }
                        else
                        {
                            imageFiles2DirectoryDict[fileName].Add(allImageFilesArr[i]);
                        }
                    }
                }

                
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var item in imageFiles2DirectoryDict)
            {
                stringBuilder.Append($"{item.Key}:\n{{");
                foreach (var diffPath in item.Value)
                {
                    stringBuilder.Append($"{diffPath}\n");
                }
                stringBuilder.Append($"\n}}\n");
            }

            Debug.Log(stringBuilder.ToString());
        }
    }
}

