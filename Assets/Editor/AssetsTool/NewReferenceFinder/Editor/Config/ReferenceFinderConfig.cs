using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ReferenceFinder.ReferenceFinderData;

namespace ReferenceFinder
{
    public class ReferenceFinderConfig : ScriptableObject
    {
        private static List<EAssetState> stateDefaultList = new List<EAssetState>()
        { EAssetState.NORMAL, EAssetState.CHANGED, EAssetState.MISSING, EAssetState.NODATA };
        private const string defaultFalseFolder = "Packages/";

        #region 可存储

        public bool focusMode = false;
        [HideInInspector]
        public bool isDepend = true;

        public List<string> selectedAssetGuid = new List<string>();
        public List<stringStruct> typeFilter = new List<stringStruct>();
        public List<stringStruct> folderFilter = new List<stringStruct>();
        public List<StateStruct> stateFilter = new List<StateStruct>();

        #endregion 可存储

        public HashSet<string> canReplaceExtension = new HashSet<string>()
        {
            ".jpg", ".png", ".tga", ".jpeg",
            ".mat", ".shader", ".cginc",
            ".lua", ".json",
            ".txt", ".xml", ".XML", ".md", ".doc", ".docx"
        };

        public Dictionary<string, stringStruct> typeFilterDic = new Dictionary<string, stringStruct>();
        public Dictionary<string, stringStruct> folderFilterDic = new Dictionary<string, stringStruct>();
        public Dictionary<EAssetState, StateStruct> stateFilterDic = new Dictionary<EAssetState, StateStruct>();

        //配置路径
        private static string relativePath;

        public static string RelativePath
        {
            get
            {
                if (string.IsNullOrEmpty(relativePath))
                {
                    //获取脚本自身路径
                    var tempSo = ScriptableObject.CreateInstance<ReferenceFinderConfig>();
                    string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(tempSo));
                    ScriptableObject.DestroyImmediate(tempSo);
                    var one = path.LastIndexOf('/');
                    var two = path.LastIndexOf('/', one - 1);
                    path = path.Substring(0, two);
                    relativePath = path;
                }
                return relativePath;
            }
        }

        public static string configPath => $"{RelativePath}/Config/ReferenceFinderConfig.asset";

        public static string helpDocPath = "资源引用查找窗口-说明书.docx";

        public static ReferenceFinderConfig _instance;

        public static ReferenceFinderConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<ReferenceFinderConfig>(configPath);
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<ReferenceFinderConfig>();

                        //文件夹不存在则创建
                        (new FileInfo(configPath)).Directory.Create();
                        AssetDatabase.CreateAsset(_instance, configPath);
                    }
                    _instance.Init();
                }
                return _instance;
            }
        }

        [Serializable]
        public class stringStruct : IComparable<stringStruct>
        {
            public string key;
            public bool isFilter;

            public int CompareTo(stringStruct other)
            {
                return string.CompareOrdinal(this.key, other.key);
            }
        }

        [Serializable]
        public class StateStruct : IComparable<StateStruct>
        {
            public EAssetState state;
            public bool isFilter;

            public int CompareTo(StateStruct other)
            {
                return state - other.state;
            }
        }

        private void Init()
        {
            typeFilterDic = typeFilter.ToDictionary(x => x.key, y => y);
            folderFilterDic = folderFilter.ToDictionary(x => x.key, y => y);
            stateFilterDic = stateFilter.ToDictionary(x => x.state, y => y);

            foreach (var data in AssetDict.Values)
            {
                if (!typeFilterDic.ContainsKey(data.extension))
                {
                    var typeS = new stringStruct();
                    typeS.key = data.extension;
                    typeS.isFilter = true;
                    typeFilter.Add(typeS);
                    typeFilterDic[data.extension] = typeS;
                }

                //string fullName = Path.GetFileName(data.path);
                //string[] split = data.path.Remove(data.path.Length - fullName.Length - 1, fullName.Length).Split('/');
                //int min = Mathf.Min(3, split.Length);
                string[] split = data.path.Split('/');
                int min = Mathf.Min(1, split.Length);
                string folder = string.Empty;
                for (int i = 0; i < min; i++)
                    folder += split[i] + "/";

                if (!folderFilterDic.ContainsKey(folder))
                {
                    var typeS = new stringStruct();
                    typeS.key = folder;
                    typeS.isFilter = folder.CompareTo(defaultFalseFolder) != 0;
                    folderFilter.Add(typeS);
                    folderFilterDic[folder] = typeS;
                }
            }
            foreach (EAssetState data in Enum.GetValues(typeof(EAssetState)))
            {
                if (!_instance.stateFilterDic.ContainsKey(data))
                {
                    var typeS = new StateStruct();
                    typeS.state = data;
                    typeS.isFilter = true;
                    stateFilter.Add(typeS);
                    stateFilterDic[data] = typeS;
                }
            }
            stateFilter.Sort();
            typeFilter.Sort();
            folderFilter.Sort();
        }
    }
}