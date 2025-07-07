/*******************************************************************
** 文件名:	FindComponentPath.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	刘芳洲 
** 日  期:	2018/12/28
** 版  本:	1.0
** 描  述:	替换预制体上图片资源的引用
** 应  用: 
 *1.读取图片的引用,修改预制体的替换配置进行配置
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditorInternal;
namespace XGameEditor
{
    class TextureSwapEditor : EditorWindow
    {
        List<TextureInfo> BaseInfo = new List<TextureInfo>();
        List<TextureInfo> PoolTextureInfos = new List<TextureInfo>();
        TextureInfo RootTextureInfo;
        List<TextureInfo> SwapTextureInfos = new List<TextureInfo>();
        Dictionary<string, List<TextureSwapPrefabEx>> RefenrancePrefabDic = new Dictionary<string, List<TextureSwapPrefabEx>>();
        //Dictionary<string,List<>>  guid,prefab

        private string[] sertchPaths;
        private string[] matchExtensions = new string[] { ".prefab" };                           //需要进行匹配的格式

        private List<string> matchFiles = new List<string>();

        private List<TextureSwapPrefabEx> checkCache;
        private Dictionary<string, List<TextureSwapPrefabEx>> checkCacheDic = new Dictionary<string, List<TextureSwapPrefabEx>>();


        bool openPool = true;
        bool openSwap = true;

        bool swichPool = false;
        bool swichSwap = false;
        bool swichRoot = false;

        Color qianlan, qianhui, qianhuang, shenhong;

        Vector2 scrollPosition = Vector2.zero;

        int SwapCout = 0;

        public void Create(List<TextureInfo> Info)
        {
            BaseInfo = Info;
            PoolTextureInfos.Clear();
            RootTextureInfo = null;
            SwapTextureInfos.Clear();
            RefenrancePrefabDic.Clear();
            checkCache = null;
            checkCacheDic.Clear();

            for (int i = 0; i < BaseInfo.Count; i++)
            {
                PoolTextureInfos.Add(BaseInfo[i]);
            }
        }
        protected void OnEnable()
        {
            ColorUtility.TryParseHtmlString("#22ABFF", out qianlan);
            ColorUtility.TryParseHtmlString("#B4B4B4", out qianhui);
            ColorUtility.TryParseHtmlString("#F1ED75", out qianhuang);
            ColorUtility.TryParseHtmlString("#AA0000", out shenhong);

            sertchPaths = new string[] {Application.dataPath+"/CRes/Prefabs/UI",	//搜索路径
			Application.dataPath+"/GResources/Prefab/UI" };

            foreach (var item in sertchPaths)
            {
                string[] files = Directory.GetFiles(item + "", "*.*", SearchOption.AllDirectories)
                .Where(s => matchExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                matchFiles.AddRange(files);
            }
        }
        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUI.color = Color.yellow;
            if (GUILayout.Button("查找所有引用"))
            {
                CheckRenferance(BaseInfo);
            }
            GUI.color = Color.white;

            //=================================//
            //==============POOL===============//
            //================================//
            GUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("--POOL--");
            if (GUILayout.Button("切换显示"))
            {
                swichPool = !swichPool;
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < PoolTextureInfos.Count; i++)
            {
                TextureInfo info = PoolTextureInfos[i];
                EditorGUILayout.BeginHorizontal();
                if (swichPool)
                {
                    GUILayout.Label(info.file);
                }
                else
                    EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.file), typeof(Texture), false);

                GUI.color = qianhui;
                if (GUILayout.Button("加入更换队列"))
                {
                    PoolTextureInfos.Remove(info);
                    SwapTextureInfos.Add(info);
                }
                GUI.color = Color.gray;
                if (RootTextureInfo == null)
                {
                    if (GUILayout.Button("加入ROOT"))
                    {
                        PoolTextureInfos.Remove(info);
                        RootTextureInfo = info;
                    }
                }
                GUI.color = qianlan;
                if (GUILayout.Button("引用的预制体"))
                {
                    info.paramBool = !info.paramBool;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();

                if (info.paramBool)
                {
                    GUI.color = qianhuang;
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.yellow;
                    if (GUILayout.Button("查引用"))
                    {
                        //查引用
                        CheckRenferance(info.guid);
                    }
                    GUI.color = Color.white;

                    if (RefenrancePrefabDic.Keys.Contains(info.guid))
                    {
                        foreach (var item in RefenrancePrefabDic[info.guid])
                        {
                            if (swichPool)
                            {
                                GUILayout.Label(item.file);
                            }
                            else
                                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(item.file)), typeof(UnityEngine.Object), false);

                            //item.isSwap = EditorGUILayout.Toggle("是否替换", item.isSwap);
                        }
                    }
                    GUILayout.EndVertical();

                }
            }
            GUILayout.EndVertical();

            EditorGUILayout.Space();
            //=================================//
            //==============SWAP===============//
            //================================//
            GUI.color = qianhui;
            GUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("--SWAP--");
            if (GUILayout.Button("切换显示"))
            {
                swichSwap = !swichSwap;
            }
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;

            for (int i = 0; i < SwapTextureInfos.Count; i++)
            {
                TextureInfo info = SwapTextureInfos[i];
                EditorGUILayout.BeginHorizontal();
                if (swichSwap)
                {
                    GUILayout.Label(info.file);
                }
                else
                    EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.file), typeof(Texture), false);
                GUI.color = shenhong;
                if (GUILayout.Button("移出更换队列"))
                {
                    PoolTextureInfos.Add(info);
                    SwapTextureInfos.Remove(info);
                }
                GUI.color = qianlan;
                if (GUILayout.Button("引用的预制体"))
                {
                    info.paramBool = !info.paramBool;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();

                if (info.paramBool)
                {
                    GUI.color = qianhuang;
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.yellow;
                    if (GUILayout.Button("查引用"))
                    {
                        //查引用
                        CheckRenferance(info.guid);
                    }
                    GUI.color = Color.white;
                    if (RefenrancePrefabDic.Keys.Contains(info.guid))
                    {
                        foreach (var item in RefenrancePrefabDic[info.guid])
                        {
                            GUILayout.BeginHorizontal();

                            if (swichSwap)
                            {
                                GUILayout.Label(item.file);
                            }
                            else
                                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(item.file)), typeof(UnityEngine.Object), false);

                            item.isSwap = EditorGUILayout.Toggle(item.isSwap);
                            if (GUILayout.Button("是否替换"))
                            {
                                item.isSwap = !item.isSwap;
                            }

                            GUILayout.EndHorizontal();

                        }
                    }
                    GUILayout.BeginHorizontal();
                    if (RefenrancePrefabDic.Keys.Contains(info.guid))
                    {
                        if (RefenrancePrefabDic[info.guid].Count > 0)
                        {
                            if (GUILayout.Button("全选"))
                            {
                                foreach (var item in RefenrancePrefabDic[info.guid])
                                {
                                    item.isSwap = true;
                                }
                            }
                            if (GUILayout.Button("反选"))
                            {
                                foreach (var item in RefenrancePrefabDic[info.guid])
                                {
                                    item.isSwap = !item.isSwap;
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();

            EditorGUILayout.Space();
            //=================================//
            //==============ROOT===============//
            //================================//
            GUI.color = Color.gray;
            GUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("--ROOT--");
            if (GUILayout.Button("切换显示"))
            {
                swichRoot = !swichRoot;
            }
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;

            if (RootTextureInfo != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (swichRoot)
                {
                    GUILayout.Label(RootTextureInfo.file);
                }
                else
                    EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(RootTextureInfo.file), typeof(Texture), false);

                GUI.color = shenhong;
                if (GUILayout.Button("移出ROOT"))
                {
                    PoolTextureInfos.Add(RootTextureInfo);
                    RootTextureInfo = null;
                    return;
                }
                GUI.color = qianlan;
                if (GUILayout.Button("引用的预制体"))
                {
                    RootTextureInfo.paramBool = !RootTextureInfo.paramBool;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();

                if (RootTextureInfo.paramBool)
                {
                    GUI.color = qianhuang;
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.yellow;
                    if (GUILayout.Button("查引用"))
                    {
                        //查引用
                        CheckRenferance(RootTextureInfo.guid);
                    }
                    GUI.color = Color.white;

                    if (RefenrancePrefabDic.Keys.Contains(RootTextureInfo.guid))
                    {
                        foreach (var item in RefenrancePrefabDic[RootTextureInfo.guid])
                        {
                            if (swichRoot)
                            {
                                GUILayout.Label(item.file);
                            }
                            else
                                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(item.file)), typeof(UnityEngine.Object), false);
                        }
                    }
                    GUILayout.EndVertical();
                }

            }
            GUILayout.EndVertical();
            EditorGUILayout.Space();

            //=================================//
            //==============END===============//
            //================================//
            GUI.color = Color.green;
            if (GUILayout.Button("开始替换", GUILayout.Height(32)))
            {
                Swap();
            }
            GUI.color = Color.white;

            GUILayout.EndScrollView();
        }


        void CheckRenferance(string guid)
        {
            int startIndex = 0;

            checkCache = new List<TextureSwapPrefabEx>();

            EditorApplication.update = delegate ()
            {
                string file = matchFiles[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("引用查找中", file, (float)startIndex / (float)matchFiles.Count);

                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    TextureSwapPrefabEx prefabEx = new TextureSwapPrefabEx();
                    prefabEx.file = file;
                    checkCache.Add(prefabEx);
                //被引用
                //Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)));
            }

                startIndex++;

                if (isCancel || startIndex >= matchFiles.Count)
                {
                    startIndex = 0;
                    if (RefenrancePrefabDic.Keys.Contains(guid))
                    {
                        RefenrancePrefabDic[guid] = checkCache;

                    }
                    else
                    {
                        RefenrancePrefabDic.Add(guid, checkCache);
                    }
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                }
            };
        }

        void CheckRenferance(List<TextureInfo> list)
        {
            if (list == null || list.Count < 1) return;
            int startIndex = 0;

            checkCacheDic.Clear();

            EditorApplication.update = delegate ()
            {
                string file = matchFiles[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("引用查找中", file, (float)startIndex / (float)matchFiles.Count);

                string fileText = File.ReadAllText(file);
                foreach (var item in list)
                {
                    if (Regex.IsMatch(fileText, item.guid))
                    {
                        TextureSwapPrefabEx prefabEx = new TextureSwapPrefabEx();
                        prefabEx.file = file;
                        if (checkCacheDic.Keys.Contains(item.guid))
                        {
                            checkCacheDic[item.guid].Add(prefabEx);
                        }
                        else
                        {
                            List<TextureSwapPrefabEx> vlist = new List<TextureSwapPrefabEx>();
                            vlist.Add(prefabEx);
                            checkCacheDic.Add(item.guid, vlist);
                        }
                    }
                }


                startIndex++;

                if (isCancel || startIndex >= matchFiles.Count)
                {
                    startIndex = 0;
                    foreach (var item in checkCacheDic)
                    {
                        if (RefenrancePrefabDic.Keys.Contains(item.Key))
                        {
                            RefenrancePrefabDic[item.Key] = item.Value;

                        }
                        else
                        {
                            RefenrancePrefabDic.Add(item.Key, item.Value);
                        }
                    }
                    checkCacheDic.Clear();
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                }
            };
        }

        private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }


        void Swap()
        {
            if (RootTextureInfo == null || SwapTextureInfos.Count < 1 || SwapCout >= SwapTextureInfos.Count)
            {
                SwapCout = 0; return;
            }
            TextureInfo item = SwapTextureInfos[SwapCout];
            if (item == null)
            {
                SwapCout++;
                Swap();
                return;
            }

            DoGuidSwap(item.guid, RootTextureInfo.guid);

            SwapCout++;
        }

        void DoGuidSwap(string fromGuid, string ToGuid)
        {
            if (RefenrancePrefabDic.Keys.Contains(fromGuid))
            {
                int startIndex = 0;
                List<TextureSwapPrefabEx> fromlist = RefenrancePrefabDic[fromGuid];
                List<TextureSwapPrefabEx> tolist = null;
                if (RefenrancePrefabDic.Keys.Contains(ToGuid)) { tolist = RefenrancePrefabDic[ToGuid]; };
                List<TextureSwapPrefabEx> alreadySwapList = null;
                EditorApplication.update = delegate ()
                {
                    TextureSwapPrefabEx item = fromlist[startIndex];

                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("正在更换中", item.file, (float)startIndex / (float)fromlist.Count);

                    if (item.isSwap)
                    {
                        if (alreadySwapList == null) alreadySwapList = new List<TextureSwapPrefabEx>();
                        alreadySwapList.Add(item);
                        string szDoSwap = File.ReadAllText(item.file).Replace(fromGuid, ToGuid);
                        File.WriteAllText(item.file, szDoSwap);
                    }

                    startIndex++;

                    if (isCancel || startIndex >= fromlist.Count)
                    {
                        startIndex = 0;
                        if (alreadySwapList != null)
                        {
                            foreach (var swapItem in alreadySwapList)
                            {
                                if (tolist == null)
                                {
                                    tolist = new List<TextureSwapPrefabEx>();
                                    RefenrancePrefabDic.Add(ToGuid, tolist);
                                }
                                tolist.Add(swapItem);
                                fromlist.Remove(swapItem);
                            }
                        }

                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        if (!isCancel)
                        {
                            Swap();
                        }
                        else
                        {
                            SwapCout = 0;
                        }
                    }
                };
            }
        }
    }
    [System.Serializable]
    class TextureSwapPrefabEx
    {
        [SerializeField] public UnityEngine.Object prefab;
        [SerializeField] public string file;
        [SerializeField] string guid;
        [SerializeField] public bool isSwap = true;
    }


    class TextureSwapCustom : EditorWindow
    {
        public bool openFileOption = false;
        List<TextureInfo> cacheTInfoList = new List<TextureInfo>();
        bool cacheTInfoListDirty = false;

        [SerializeField]
        protected List<Texture> SearchFileObjectList = new List<Texture>();

        //序列化对象
        protected SerializedObject _serializedObject;
        protected ReorderableList reorderableList;
        //序列化属性 
        protected SerializedProperty _searchObjcetLstProperty;

        Vector2 scrollPosition = Vector2.zero;
        Vector2 scrollPosition2 = Vector2.zero;

        Color qianlan, qianhui, qianhuang, shenhong;

        [MenuItem("Assets/XGame/UI工具/图片自定义替换", false, 10)]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(TextureSwapCustom));
        }
        protected void OnEnable()
        {
            ColorUtility.TryParseHtmlString("#22ABFF", out qianlan);
            ColorUtility.TryParseHtmlString("#B4B4B4", out qianhui);
            ColorUtility.TryParseHtmlString("#F1ED75", out qianhuang);
            ColorUtility.TryParseHtmlString("#AA0000", out shenhong);
        }


        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUI.color = qianlan;
            if (GUILayout.Button("配置检查目录"))
            {
                openFileOption = !openFileOption;
            }
            GUI.color = Color.white;

            #region ###

            //if (openFileOption)
            //{
            //    GUI.color = Color.gray;
            //    GUILayout.BeginVertical("box");
            //    GUI.color = Color.white;

            //    EditorGUILayout.Space();

            //    _serializedObject.Update();

            //    EditorGUI.BeginChangeCheck();

            //    reorderableList.DoLayoutList();


            //    GUI.color = Color.yellow;
            //    GUILayout.BeginVertical("box");
            //    GUI.color = Color.white;
            //    GUILayout.Label("----文件夹拖拽区域---");
            //    var dragFileObject = EditorGUILayout.ObjectField(null, typeof(Texture), false, GUILayout.ExpandWidth(true), GUILayout.Height(64)) as Texture;
            //    if (dragFileObject != null)
            //    {
            //        SearchFileObjectList.Add(dragFileObject);
            //    }
            //    GUILayout.EndVertical();


            //    GUI.color = Color.red;
            //    if (GUILayout.Button("清空"))
            //    {
            //        SearchFileObjectList.Clear();
            //    }
            //    GUI.color = Color.white;

            //    if (EditorGUI.EndChangeCheck())
            //    {
            //        cacheTInfoList.Clear();
            //        _serializedObject.ApplyModifiedProperties();
            //    }

            //    EditorGUILayout.Space();
            //    GUILayout.EndVertical();
            //}
            #endregion

            if (openFileOption)
            {
                GUI.color = Color.gray;
                GUILayout.BeginVertical("box");
                GUI.color = Color.white;

                GUI.color = Color.yellow;
                GUILayout.BeginVertical("box");
                GUILayout.Label("----图片拖拽区域---");
                var dragFileObject = EditorGUILayout.ObjectField(null, typeof(Texture), false, GUILayout.ExpandWidth(true), GUILayout.Height(64)) as Texture;
                if (dragFileObject != null)
                {
                    if (!SearchFileObjectList.Contains(dragFileObject))
                    {
                        SearchFileObjectList.Add(dragFileObject);
                        cacheTInfoListDirty = true;
                    }
                }
                GUI.color = Color.white;

                GUILayout.EndVertical();

                scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);
                List<Texture> removeList = new List<Texture>();
                foreach (var item in SearchFileObjectList)
                {
                    if (GUILayout.Button(item, GUILayout.MinHeight(16), GUILayout.MaxHeight(128), GUILayout.MinWidth(16), GUILayout.MaxWidth(2048)))//GUILayout.MaxWidth(512)  GUILayout.ExpandWidth(true))
                    {
                        removeList.Add(item);
                        cacheTInfoListDirty = true;
                    }
                }
                for (int i = 0; i < removeList.Count; i++)
                {
                    SearchFileObjectList.Remove(removeList[i]);
                }
                GUILayout.EndScrollView();

                EditorGUILayout.Space();

                GUI.color = shenhong;
                if (GUILayout.Button("清空"))
                {
                    SearchFileObjectList.Clear();
                    cacheTInfoListDirty = true;
                }
                GUI.color = Color.white;

                EditorGUILayout.Space();
                GUILayout.EndVertical();

                //if (dirty)
                //{
                //    this.Repaint();
                //}
            }

            GUI.color = Color.green;
            if (GUILayout.Button("打开详细配置", GUILayout.Height(32)))
            {
                if (cacheTInfoListDirty)
                {
                    cacheTInfoList.Clear();
                    cacheTInfoListDirty = false;
                }
                if (cacheTInfoList.Count < 1)
                {
                    for (int i = 0; i < SearchFileObjectList.Count; i++)
                    {
                        var item = SearchFileObjectList[i];
                        if (item != null)
                        {
                            cacheTInfoList.Add(TextureInfo.Create(item));
                        }
                    }
                }
                if (cacheTInfoList.Count > 1)
                {
                    TextureSwapEditor swapWindow = EditorWindow.GetWindow(typeof(TextureSwapEditor)) as TextureSwapEditor;
                    swapWindow.Create(cacheTInfoList);
                }
            }
            GUI.color = Color.white;
            GUILayout.EndScrollView();

        }
    }
}
#endif