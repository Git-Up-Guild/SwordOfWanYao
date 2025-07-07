using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CreateAssetMenu]
public class SameFunctionUIPrefabConfig : ScriptableObject
{
    public SameFunctionUIPrefabData[] config;
    [HideInInspector]
    public Dictionary<GameObject, SameFunctionUIPrefabData> prefab2ConfigDict;


    [System.Serializable]
    public class SameFunctionUIPrefabData
    {
        [Tooltip("这是一系列UI预制体的功能描述")]
        public string m_functionDesc;
        [Tooltip("这是之后UI1.0目录下可能会创建出来的文件夹名称（如果UI1.0下没有该目录将创建），如果一张图片的只被这些预制体依赖，那么这张图片将会被移动到该名称的目录下")]
        public string m_functionDirectoryName;
        [Tooltip("这是相同功能组的UI预制体目录")]
        public string m_prefabsDirectory;
        [Tooltip("这是一系列UI预制体")]
        public GameObject[] m_prefabs;


        [HideInInspector]
        public string m_destImageDirectory;//图片将要移动至的目录路径
        public HashSet<GameObject> m_allPrefabSet;//所有相关预制体集合

    }

    [CustomEditor(typeof(SameFunctionUIPrefabConfig))]
    class SameFunctionUIPrefabConfigDraw : Editor
    {
        SameFunctionUIPrefabConfig config;
        
        private void OnEnable()
        {
            config = target as SameFunctionUIPrefabConfig;
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Label("对呀 如果一个预制体被多个功能组共用 就说明有点通用的感觉对吧");
            if (GUILayout.Button($"检查单个预制体是否被分为多个功能组中"))
            {
                config.SetupData();
            }
            base.OnInspectorGUI();
            
            //for (int i = 0; i < config.config.Length; i++)
            //{
            //    if(GUILayout.Button($"赋值第{i}预制体目录"))
            //    {
            //        config.config[i].m_prefabsDirectory = EditorUtility.OpenFolderPanel("预制体文件夹", config.config[i].m_prefabsDirectory,$"{Application.dataPath}/G_Resources/Game/Prefab/LuaUI");
            //    }
            //}
        }
    }


    public bool SetupData()
    {
        prefab2ConfigDict = new Dictionary<GameObject, SameFunctionUIPrefabData>();
        for (int i = 0; i < config.Length; i++)
        {
            if(string.IsNullOrEmpty(config[i].m_functionDirectoryName))
            {
                Debug.LogError($"{i} 索引配置未配置目录名称");
                return false;
            }

            config[i].m_destImageDirectory = $"{Application.dataPath}/Game/ImmortalFamily/GameResources/Artist/{ ImageOptimizeTool.MainUIDir}/{config[i].m_functionDirectoryName}";

            config[i].m_allPrefabSet = new HashSet<GameObject>();



            if (!string.IsNullOrEmpty(config[i].m_prefabsDirectory))
            {
                config[i].m_prefabsDirectory = config[i].m_prefabsDirectory.Replace('\\','/');
                int index = config[i].m_prefabsDirectory.IndexOf("Assets");
                config[i].m_prefabsDirectory = config[i].m_prefabsDirectory.Substring(index);
                var allprefabsList = ImageOptimizeUtility.GetPrefabsInDirectory(config[i].m_prefabsDirectory);
                foreach (var prefab in allprefabsList)
                {
                    config[i].m_allPrefabSet.Add(prefab);
                }
            }


            for (int j = 0; j < config[i].m_prefabs.Length; j++)
            {
                var prefab = config[i].m_prefabs[j];
                config[i].m_allPrefabSet.Add(prefab);

                if (prefab2ConfigDict.TryGetValue(prefab,out var data))
                {
                    Debug.LogErrorFormat("{0} 属于两个不同的功能组配置 {1} 和 {2}", prefab.name , data.m_functionDesc , config[i].m_functionDesc);
                    return false;
                }
                prefab2ConfigDict.Add(prefab, config[i]);
            }
        }

        Debug.Log("配置文件没有问题");
        return true;
    }
}



