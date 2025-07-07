/*******************************************************************
** 文件名:	FindComponentPath.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	刘芳洲 
** 日  期:	2018/12/28
** 版  本:	1.0
** 描  述:	查找文件夹中重复的图片资源
** 应  用: 
 *1.查找目录下 (与目标图片) 重复的图片资源
 *2.关联到替换图片引用窗口
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
using System.Text;
using System.Collections.Generic;
using System;
using UnityEditorInternal;
namespace XGameEditor
{
    class FindRepetitiveUIRes : EditorWindow
    {
        protected Texture TargetTextrue;

        protected TextureInfo TargetTextureInfo;

        [SerializeField]
        protected List<SearchFileObjcet> SearchFileObjectList = new List<SearchFileObjcet>();
        protected List<string> FileList = new List<string>();
        protected Dictionary<string, List<TextureInfo>> md5Dic = new Dictionary<string, List<TextureInfo>>();
        protected bool openFileOption = false;
        protected bool openSearchResult = true;
        protected bool inMD5Search = false;
        private static string[] matchExtensions = new string[] { ".png", ".jpg" };                           //需要进行匹配的格式

        Vector2 scrollPosition = Vector2.zero;
        Vector2 scrollPosition2 = Vector2.zero;

        Color qianlan, qianhui, qianhuang, shenhong;

        Rect FileDragRect;

        [MenuItem("Assets/XGame/UI工具/图片重复查找", false, 10)]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(FindRepetitiveUIRes));
        }

        //序列化对象
        protected SerializedObject _serializedObject;
        protected ReorderableList reorderableList;

        //序列化属性 
        protected SerializedProperty _searchObjcetLstProperty;

        protected void OnEnable()
        {
            ColorUtility.TryParseHtmlString("#22ABFF", out qianlan);
            ColorUtility.TryParseHtmlString("#B4B4B4", out qianhui);
            ColorUtility.TryParseHtmlString("#F1ED75", out qianhuang);
            ColorUtility.TryParseHtmlString("#AA0000", out shenhong);

            //使用当前类初始化
            _serializedObject = new SerializedObject(this);
            //获取当前类中可序列话的属性
            _searchObjcetLstProperty = _serializedObject.FindProperty("SearchFileObjectList");
            reorderableList = new ReorderableList(_serializedObject, _searchObjcetLstProperty);
            reorderableList.elementHeight = 16;
            reorderableList.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    var element = _searchObjcetLstProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element);
                };

            try
            {
                TargetTextrue = Selection.activeObject as Texture;

            }
            catch (Exception)
            {

            }

            //var defaultColor = GUI.backgroundColor;
            //reorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            //{
            //    GUI.backgroundColor = Color.yellow;
            //};
            //reorderableList.drawHeaderCallback = (rect) => { rect.height = 0;
            //    EditorGUI.LabelField(rect, _searchObjcetLstProperty.displayName);
            //};

        }
        void OnGUI()
        {
            scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);

            TargetTextrue = EditorGUILayout.ObjectField("目标图片:", TargetTextrue, typeof(Texture), false, GUILayout.ExpandWidth(true)) as Texture;

            GUI.color = qianlan;
            if (GUILayout.Button("配置检查目录"))
            {
                openFileOption = !openFileOption;
            }
            GUI.color = Color.white;


            if (openFileOption)
            {
                GUI.color = Color.gray;
                GUILayout.BeginVertical("box");
                GUI.color = Color.white;

                EditorGUILayout.Space();

                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                reorderableList.DoLayoutList();


                GUI.color = Color.yellow;
                GUILayout.BeginVertical("box");
                GUILayout.Label("----文件夹拖拽区域---");
                var dragFileObject = EditorGUILayout.ObjectField(null, typeof(UnityEngine.Object), false, GUILayout.ExpandWidth(true), GUILayout.Height(64));
                if (dragFileObject != null)
                {
                    SearchFileObjcet searchObj = new SearchFileObjcet(dragFileObject);
                    SearchFileObjectList.Add(searchObj);
                }
                GUI.color = Color.white;

                GUILayout.EndVertical();


                GUI.color = shenhong;
                if (GUILayout.Button("清空"))
                {
                    SearchFileObjectList.Clear();
                }
                GUI.color = Color.white;

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.Space();
                GUILayout.EndVertical();
            }

            GUI.color = Color.green;
            if (GUILayout.Button("开始检测", GUILayout.Height(32)))
                Check();
            GUI.color = Color.white;

            GUI.color = qianlan;
            if (GUILayout.Button("查看结果"))
            {
                openSearchResult = !openSearchResult;
            }
            GUI.color = Color.white;

            if (openSearchResult && !inMD5Search)
            {
                if (md5Dic.Keys.Count > 0)
                {
                    GUI.color = Color.gray;
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.white;
                    EditorGUILayout.Space();

                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    if (TargetTextureInfo == null)
                    {
                        foreach (var item in md5Dic)
                        {
                            if (item.Value.Count > 1)
                            {
                                //if (GUILayout.Button(item.Key))
                                if (GUILayout.Button(AssetDatabase.LoadAssetAtPath<Texture>(item.Value[0].file), GUILayout.MaxWidth(512)))
                                {
                                    TextureSwapEditor swapWindow = EditorWindow.GetWindow(typeof(TextureSwapEditor)) as TextureSwapEditor;
                                    swapWindow.Create(item.Value);
                                }
                            }
                        }

                    }
                    else
                    {
                        List<TextureInfo> list = md5Dic[TargetTextureInfo.md5];
                        if (list.Count > 1)
                        {
                            if (GUILayout.Button(AssetDatabase.LoadAssetAtPath<Texture>(TargetTextureInfo.file), GUILayout.MaxWidth(512)))
                            {
                                TextureSwapEditor swapWindow = EditorWindow.GetWindow(typeof(TextureSwapEditor)) as TextureSwapEditor;
                                swapWindow.Create(list);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    EditorGUILayout.Space();
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();
        }
        private void Check()
        {
            TargetTextureInfo = null;

            FileList.Clear();

            md5Dic.Clear();

            //1.
            FileObjectToFile();

            if (TargetTextrue != null)
            {
                string szTexture = AssetDatabase.GetAssetPath(TargetTextrue);
                TextureInfo textureInfo = TextureInfo.Create(szTexture);
                TargetTextureInfo = textureInfo;
                md5DicAdd(textureInfo);
                if (FileList.Count < 1)
                {
                    //获取图片所在目录
                    int index = szTexture.LastIndexOf('/');
                    szTexture = szTexture.Substring(0, index + 1);
                    FileListAdd(szTexture, true);
                }
            }
            CheckReptitive();
        }
        private void FileObjectToFile()
        {
            List<SearchFileObjcet> noChild = new List<SearchFileObjcet>();

            for (int i = 0; i < SearchFileObjectList.Count; i++)
            {
                var item = SearchFileObjectList[i];
                if (item == null || item.fileObject == null)
                    continue;
                bool checkChild = item.searchOption == SearchOption.AllDirectories ? true : false;
                if (checkChild)
                {
                    string file = AssetDatabase.GetAssetPath(item.fileObject) + "/";
                    FileListAdd(file, true);
                }
                else
                {
                    noChild.Add(item);
                }
            }

            for (int i = 0; i < noChild.Count; i++)
            {
                var item = noChild[i];
                string file = AssetDatabase.GetAssetPath(item.fileObject) + "/";
                FileListAdd(file, false);
            }

        }
        private void CheckReptitive()
        {
            List<string> matchFiles = new List<string>();
            //SearchOption searchOption = checkChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            matchFiles.Clear();

            for (int i = 0; i < FileList.Count; i++)
            {
                string path = FileList[i];

                Debug.Log("目录：" + path);

                string[] files = Directory.GetFiles(path + "", "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => matchExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();

                matchFiles.AddRange(files);
            }

            if (matchFiles.Count < 1)
            {
                Debug.Log("<color=#006400>没有重复的资源</color>");

                return;
            }
            Debug.Log("图片数量：" + matchFiles.Count);

            int startIndex = 0;

            EditorApplication.update = delegate ()
                {
                    inMD5Search = true;
                    string file = matchFiles[startIndex];

                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("搜索中", file, (float)startIndex / (float)matchFiles.Count);

                    if (TargetTextureInfo == null || !TargetTextureInfo.file.Equals(file))
                    {

                        TextureInfo textureInfo = TextureInfo.Create(file);

                        md5DicAdd(textureInfo);
                    }

                    startIndex++;

                    if (isCancel || startIndex >= matchFiles.Count)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        startIndex = 0;
                        inMD5Search = false;
                        CheckReptitiveEnd();
                    //Debug.Log("<color=#006400>查找结束</color>", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(AssetDatabase.GetAssetPath(Selection.activeObject))));
                }
                };

        }
        private void CheckReptitiveEnd()
        {
            if (TargetTextureInfo != null)
            {
                List<TextureInfo> list = md5Dic[TargetTextureInfo.md5];
                if (list.Count > 1)
                {
                    Debug.Log("<color=#006400>有重复的资源</color>");
                    foreach (var item in list)
                    {
                        Debug.Log(item.file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.file));
                    }
                }
                else
                {
                    Debug.Log("<color=#006400>没有重复的资源</color>");
                }
            }
            else
            {
                int num = 1;
                foreach (var item in md5Dic)
                {
                    if (item.Value.Count > 1)
                    {
                        Debug.Log(string.Format("<color=#006400>有重复的资源 {0}</color>: ", num));
                        foreach (var sitem in item.Value)
                        {
                            Debug.Log(sitem.file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sitem.file));
                        }
                        num++;
                    }
                }
                if (num == 1)
                {
                    Debug.Log("<color=#006400>没有重复的资源</color>");
                }
            }
        }


        private void md5DicAdd(TextureInfo info)
        {
            string md5 = info.md5;
            if (md5Dic.ContainsKey(md5))
            {
                List<TextureInfo> list = md5Dic[md5];
                list.Add(info);
            }
            else
            {
                List<TextureInfo> list = new List<TextureInfo>();
                list.Add(info);
                md5Dic.Add(md5, list);
            }
        }

        private void FileListAdd(string file, bool checkChild)
        {
            if (FileList.Contains(file)) return;
            FileList.Add(file);
            if (checkChild)
            {
                DirectoryInfo dir = new DirectoryInfo(file);
                DirectoryInfo[] fil = dir.GetDirectories();
                foreach (var _fil in fil)
                {
                    string _filName = _fil.FullName;
                    _filName = _filName.Substring(_filName.IndexOf("Assets"));
                    _filName = _filName.Replace('\\', '/') + "/";
                    FileListAdd(_filName, true);
                }
            }
        }
    }

    class FindRepetitiveUITools
    {
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
        }
        private static Color _qianlan, _qianhui, _qianhuang, _shenhong;

        public static Color qianlan { get { if (_qianlan == null) ColorUtility.TryParseHtmlString("#22ABFF", out _qianlan); return _qianlan; } }
        public static Color qianhui { get { if (_qianhui == null) ColorUtility.TryParseHtmlString("#B4B4B4", out _qianlan); return _qianlan; } }
        public static Color qianhuang { get { if (_qianhuang == null) ColorUtility.TryParseHtmlString("#F1ED75", out _qianlan); return _qianlan; } }
        public static Color shenhong { get { if (_shenhong == null) ColorUtility.TryParseHtmlString("#6A0000", out _qianlan); return _qianlan; } }
    }

    class TextureInfo
    {
        public string md5;
        public string file;
        public string guid;
        public int paramInt;
        public bool paramBool;
        public string paramStr;

        public void create(string file)
        {
            this.file = file;
            md5 = FindRepetitiveUITools.GetMD5HashFromFile(file);
            guid = AssetDatabase.AssetPathToGUID(file);
            paramInt = -1;
            paramBool = false;
            paramStr = "";
        }

        public void create(Texture texure)
        {
            this.file = AssetDatabase.GetAssetPath(texure);
            md5 = FindRepetitiveUITools.GetMD5HashFromFile(file);
            guid = AssetDatabase.AssetPathToGUID(file);
            paramInt = -1;
            paramBool = false;
            paramStr = "";
        }


        public static TextureInfo Create(string file)
        {
            TextureInfo textureInfo = new TextureInfo();
            textureInfo.create(file);
            return textureInfo;
        }
        public static TextureInfo Create(Texture texure)
        {
            TextureInfo textureInfo = new TextureInfo();
            textureInfo.create(texure);
            return textureInfo;
        }
    }

    [System.Serializable]
    public class SearchFileObjcet
    {
        [SerializeField]
        public UnityEngine.Object fileObject;

        [SerializeField]
        public SearchOption searchOption;
        public SearchFileObjcet(UnityEngine.Object Object)
        {
            fileObject = Object;
            searchOption = SearchOption.AllDirectories;
        }
    }

    [CustomPropertyDrawer(typeof(SearchFileObjcet))]
    public class SearchFileObjcetDrawer : PropertyDrawer
    {
        private SearchFileObjcet fileObjcet;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {

                //设置属性名宽度 Name HP
                EditorGUIUtility.labelWidth = 100;
                //输入框高度，默认一行的高度
                position.height = EditorGUIUtility.singleLineHeight;

                //ico 位置矩形
                var fileObjectRect = new Rect(position)
                {
                    width = 128,
                    height = 16
                };

                var searchOptionRect = new Rect(position)
                {
                    width = 256,
                    x = position.x + 128
                };
                //          找到每个属性的序列化值
                var fileObjectProperty = property.FindPropertyRelative("fileObject");
                var searchOptionProperty = property.FindPropertyRelative("searchOption");

                //          绘制GUI
                fileObjectProperty.objectReferenceValue = EditorGUI.ObjectField(fileObjectRect, fileObjectProperty.objectReferenceValue, typeof(UnityEngine.Object), false);

                EditorGUI.PropertyField(searchOptionRect, searchOptionProperty, true);


            }
        }
    }
}
#endif
