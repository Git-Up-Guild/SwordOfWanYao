/*******************************************************************
** 文件名:	FindComponentPath.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	赖建平 
** 日  期:	2017/4/20
** 版  本:	1.0
** 描  述:	查找资源引用
** 应  用: 
 *1.选中物体（可多选）的图集使用情况检测（除公用图集外是否有使用多张图集的情况）
 *2.按GUID查找选中预制体被别的哪些预制体引用了
 *3.设置图集标签名称：按文件夹、区分透明与半透
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.UI;
namespace XGameEditor
{
    public class FindUIReferanceRes : MonoBehaviour
    {
        #region 选中物体（可多选）的图集使用情况检测（除公用图集外是否有使用多张图集的情况）
        private static List<string> refTextureTag = new List<string>();//所有引用图集标签记录

        private static List<string> commonTag = new List<string> { "common", "common_effect", "commonico_res", "common_res", "objectdisplay", "commonico", "commonhead", "common_commontransparent" };

        /// <summary>
        /// 选中物体被引用查找使用的图集检测
        /// 1.可以多选
        /// 2.需要添加公用图集标签commonTag（resourse下面的图集标签需要添加_res后缀做区分）
        /// </summary>
        [MenuItem("Assets/XGame/UI工具/选中物体图集使用情况检查（可多选）")]
        private static void FindTextureTag()
        {
            foreach (var item in Selection.gameObjects)
            {
                FindTextureRef(item);
            }
        }
        private static void FindTextureRef(GameObject _go)
        {
            string path = AssetDatabase.GetAssetPath(_go);
            Debug.Log("<color=#00008B>查询物体：" + _go.name + "所有资源引用路径:</color>", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(path)));
            string[] refPaths = AssetDatabase.GetDependencies(path);//拿到所有引用物体路径
            List<GameObject> refObjList = new List<GameObject>();
            for (int i = 0; i < refPaths.Length; i++)//查找引用计数
            {
                Object refObj = AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(refPaths[i]));//引用物体对象
                if (refObj is Texture)
                {
                    TextureImporter tim = AssetImporter.GetAtPath(GetRelativeAssetsPath(refPaths[i])) as TextureImporter;
                    if (tim != null && tim.textureType == TextureImporterType.Sprite)//是否为ui格式
                    {
                        bool isCommon = false;
                        if (string.IsNullOrEmpty(tim.spritePackingTag))           //基本都需要设置图集打包标签
                        {
                            Debug.LogError(_go.name + "  path：" + refPaths[i] + "	未设置打包标签", refObj);
                        }
                        else
                        {
                            foreach (string item in commonTag)
                            {
                                if (item.ToLower() == tim.spritePackingTag.ToLower())
                                {
                                    isCommon = true;
                                    break;
                                }
                            }
                            if (!isCommon)
                            {
                                Debug.Log(_go.name + "  path：" + refPaths[i] + "	tag:" + "<color=#ff0000>" + tim.spritePackingTag + "</color>", refObj);
                            }
                        }

                        if (!isCommon && !refTextureTag.Contains(tim.spritePackingTag))
                        {
                            refTextureTag.Add(tim.spritePackingTag);
                        }
                    }
                    else
                    {
                        Debug.Log(_go.name + "  ：" + refPaths[i] + "	tag: tim is null", refObj);
                    }
                }
                else if (refObj is GameObject)//如果是预制体，需要再次查找该预制体的图集引用情况
                {
                    if ((GameObject)refObj != _go)
                    {
                        refObjList.Add((GameObject)refObj);
                    }
                }
            }

            if (refTextureTag.Count > 1)
            {
                string tag = "";
                bool hasNoTag = false;
                foreach (var item in refTextureTag)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        hasNoTag = true;
                    }
                    tag += item + "、";
                }
                string logStr = "资源交叉引用数超标：" + refTextureTag.Count + "	tags:" + tag;
                if (hasNoTag)
                {
                    logStr += "	存在未设置图集标签的资源";
                }
                Debug.LogError(logStr, _go);
            }
            else
            {
                //Debug.Log("<color=#00008B>查找结束,无交叉引用</color>", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(path)));

                //Debug.Log("<color=#00008B>查找结束</color>", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(path)));
            }

            foreach (GameObject item in refObjList)//如果是预制体，需要再次查找该预制体的图集引用情况
            {
                Debug.Log(_go.name + "  ：" + path + "	<color=#FF7F24>迭代引用</color>：", item);
                FindTextureRef(item);
            }
            refObjList.Clear();
            refObjList = null;
            refTextureTag.Clear();
        }
        #endregion

        #region 按GUID查找选中预制体被别的哪些预制体引用了
        private static List<string> matchFiles = new List<string>();

        private static string[] sertchPaths = new string[] {Application.dataPath+"/CRes/Prefabs/UI",	//搜索路径
			Application.dataPath+"/GResources/Prefab/UI" };

        private static string[] matchExtensions = new string[] { ".prefab" };                           //需要进行匹配的格式


        [MenuItem("Assets/XGame/UI工具/选中物体被引用查找", false, 10)]
        static private void FindRefByGUID()
        {
            Debug.Log("<color=#006400>开始查找</color>", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(AssetDatabase.GetAssetPath(Selection.activeObject))));
            EditorSettings.serializationMode = SerializationMode.ForceText;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                int startIndex = 0;
                matchFiles.Clear();
                foreach (var item in sertchPaths)
                {
                    string[] files = Directory.GetFiles(item + "", "*.*", SearchOption.AllDirectories)
                    .Where(s => matchExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                    matchFiles.AddRange(files);
                }
                EditorApplication.update = delegate ()
                {
                    string file = matchFiles[startIndex];

                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)matchFiles.Count);

                    if (Regex.IsMatch(File.ReadAllText(file), guid))
                    {
                        Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                    }

                    startIndex++;
                    if (isCancel || startIndex >= matchFiles.Count)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        startIndex = 0;
                        Debug.Log("<color=#006400>查找结束</color>", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(AssetDatabase.GetAssetPath(Selection.activeObject))));
                    }
                };
            }
        }
        #endregion

        #region 在配置表中查找被选中的物体（按名称，基本上不实用，因为可能没配表，只是写在脚本中）
        //[MenuItem("Assets/XGame/UI工具/选中物体被引用查找/按名称查找配置表", false, 11)]
        //static private void FindRefByCSV()
        //{
        //	//EditorCoroutineRunner.StartEditorCoroutine(OnFindRefByCSV());
        //	Debug.Log("<color=#006400>按名称查找配置表引用</color>");

        //	int count = 0;
        //	string scrPath = Application.dataPath + "/Resources/scp";
        //	Object resObj = Selection.activeObject;
        //	if (!string.IsNullOrEmpty(scrPath))
        //	{
        //		string[] files = Directory.GetFiles(scrPath, "*.*", SearchOption.AllDirectories);
        //		//.Where(s => s == withoutExtension).ToArray();
        //		int startIndex = 0;

        //		if (files == null || files.Length == 0)
        //		{
        //			Debug.Log("配置表为空");
        //			return;
        //		}
        //		EditorApplication.update = delegate()
        //		{
        //			string file = files[startIndex];

        //			bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配配置表中", file, (float)startIndex / (float)files.Length);

        //			if (Regex.IsMatch(File.ReadAllText(file), resObj.name))
        //			{
        //				count++;
        //				Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
        //			}

        //			startIndex++;
        //			if (isCancel || startIndex >= files.Length)
        //			{
        //				EditorUtility.ClearProgressBar();
        //				EditorApplication.update = null;
        //				startIndex = 0;
        //				Debug.Log(string.Format("<color=#006400>配置表匹配结束,匹配到:</color>{0}<color=#006400>个</color>", count), AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(AssetDatabase.GetAssetPath(resObj))));
        //			}
        //		};
        //	}
        //}

        #endregion

        #region 图片是否含有alpha通道，并没用，无法检测
        //[MenuItem("Assets/XGame/UI工具/图片alpha通道检查", false, 12)]
        private static void HasAlpha()
        {
            //Texture2D t = Selection.activeObject as Texture2D;
            Object se = Selection.activeObject;
            TextureImporter ti = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(se)) as TextureImporter;
            string res = JudgeTransparentPic(ti) ? "有alpha通道" : "没有alpha同道";
            Debug.Log(res, se);
        }
        private static bool JudgeTransparentPic(TextureImporter tim)
        {
            return tim.DoesSourceTextureHaveAlpha();
        }
        #endregion

        #region 给按钮添加缩放效果
        //[MenuItem("Assets/XGame/UI工具/添加按钮缩放", false, 13)]
        //public static void AddBtnScale()
        //{
        //	GameObject se = Selection.activeObject as GameObject;

        //	TweenScale ts = se.AddComponent<TweenScale>();
        //	ts.duration = 0.05f;
        //	ts.startValue = Vector3.one * 0.85f;
        //	ts.target = se.transform;

        //	//se.GetComponent<UnityEngine.UI.Button>().OnPointerDown();

        //	EventTrigger trigger = se.GetComponent<EventTrigger>();
        //	if (trigger == null) trigger = se.AddComponent<EventTrigger>();
        //	//实例化delegates  
        //	trigger.triggers = new List<EventTrigger.Entry>();
        //	//定义所要绑定的事件类型  
        //	EventTrigger.Entry entry = new EventTrigger.Entry();
        //	//设置事件类型  
        //	entry.eventID = EventTriggerType.PointerClick;
        //	//设置回调函数  
        //	entry.callback = new EventTrigger.TriggerEvent();
        //	UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(delegate(BaseEventData arg0)
        //	{
        //		ts.Play();
        //	}); 
        //	entry.callback.AddListener(callback);
        //	//添加事件触发记录到GameObject的事件触发组件  
        //	trigger.triggers.Add(entry); 		
        //}
        #endregion

        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }



        #region 获得AssetBundle所有依赖
        [MenuItem("Assets/XGame/UI工具/检查缺少CanvasRender")]
        private static void CheckCanvsRender()
        {
            Object[] models = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);

            for (int i = 0; i < models.Length; i++)
            {
                //获得文件夹/
                string fileName = models[i].name;

                string path = AssetDatabase.GetAssetPath(models[i]);

                if (path.Contains("."))
                {
                    Debug.LogError("请选择文件夹重试！！！！！！！！！！！");
                    return;
                }
                #region 查找文件夹中所有的文件
                ShowProgress("Collect textures...", 0, 1);
                string[] arrStrPath = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                for (int j = 0; j < arrStrPath.Length; j++)
                {
                    ShowProgress("Collect textures...", j, arrStrPath.Length);
                    GameObject assetPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(arrStrPath[j], typeof(GameObject));
                    if (assetPrefab != null)
                    {
                        Image image = assetPrefab.GetComponent<Image>();

                        if (image != null)
                        {
                            CanvasRenderer c = assetPrefab.GetComponent<CanvasRenderer>();

                            if (c == null)
                            {
                                Debug.LogError("CanvasRenderer Error:" + arrStrPath[j] + "  :" + image.name);
                            }
                        }
                    }
                }

                Debug.Log("Finish");
                EditorUtility.ClearProgressBar();
                #endregion
            }
        }

        private static void ShowProgress(string msg, int progress, int total)
        {
            EditorUtility.DisplayProgressBar(string.Format("ResourceVerifier", progress, total), string.Format("{0}...{1}/{2}", msg, progress, total), progress * 1.0f / total);
        }
        #endregion
    }

    /// <summary>
    /// 设置图集标签名称：按文件夹、区分透明与半透
    /// </summary>
    public class SetUITag : EditorWindow
    {
        string uiDirector;
        string result;
        int sumCount;

        Vector2 _scrollPos;
        private Rect rect;

        private string tagEx = "";                  //标签后缀
        bool isResources;

        private List<string> ignoreDir ;     //不需要设置tag的文件夹

        [MenuItem("Assets/XGame/UI工具/设置UI Tag名称")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(SetUITag));
        }

        private void OnEnable()
        {
            ignoreDir = new List<string> { Application.dataPath + "/Artist/Common/Texture/UI/Version1/Base" };     //不需要设置tag的文件夹
        }
        void OnGUI()
        {
            #region 通过拖拽获取路径
            //EditorGUILayout.LabelField();
            //获得一个长300的框  
            rect = EditorGUILayout.GetControlRect(GUILayout.Width(800));
            //将上面的框作为文本输入框  
            uiDirector = EditorGUI.TextField(rect, "拖动要设置UI图集标签的文件夹至此：", uiDirector);
            rect = EditorGUILayout.GetControlRect(GUILayout.Width(500));
            isResources = EditorGUI.Toggle(rect, "是否为resourse中的文件夹？（resourse中的会添加_res后缀）", isResources);//因为Resource中的图集打包标签需要添加_res后缀区分
            tagEx = isResources ? "_res" : "";

            //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
            if ((Event.current.type == EventType.DragUpdated
              || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                //改变鼠标的外表  
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    uiDirector = DragAndDrop.paths[0];
                }
            }
            #endregion

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("设置"))
            {
                sumCount = 0;
                string[] di = Directory.GetDirectories(uiDirector, "*.*", SearchOption.AllDirectories);
                result = SetTag(uiDirector);
                foreach (string item in di)
                {
                    result += SetTag(item);
                }
                AssetDatabase.SaveAssets();
                if (sumCount == 0)
                {
                    result = "共修改：0个";
                }
                else
                {
                    result = sumCount + "个:\n" + result;
                }
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.TextArea("修改结束，共修改了" + sumCount + "个");
            EditorGUILayout.EndScrollView();

        }
        private string opacityDir = "opacity", transparentDir = "transparent";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directorPath">"Assets/Artist/Common/Texture/UI/Version1/Country\\Opacity"</param>
        private string SetTag(string directorPath)
        {
            directorPath = directorPath.Replace('\\', '/');
            foreach (string ignore in ignoreDir)
            {
                string s = GetRelativeAssetsPath(ignore);
                if (directorPath == s)//不需要设置这个文件夹
                {
                    return "";
                }
            }

            int subCount = 0;
            string[] names = directorPath.Split('/');
            int length = names.Length;
            string lastName = names[length - 1];
            string tagName = "";
            if (lastName.ToLower().Contains(opacityDir.ToLower()) || lastName.ToLower().Contains(transparentDir.ToLower()))
            {
                lastName = names[length - 2].Replace("_", "") + "_" + lastName.Replace("_", "");
            }

            tagName = lastName + tagEx;


            string[] files = Directory.GetFiles(directorPath);
            foreach (string item in files)
            {
                TextureImporter tim = AssetImporter.GetAtPath(item) as TextureImporter;
                if (tim != null && tim.textureType == TextureImporterType.Sprite && tim.spritePackingTag != tagName.ToLower())
                {
                    tim.spritePackingTag = tagName.ToLower();
                    tim.SaveAndReimport();
                    subCount++;
                }
            }
            sumCount += subCount;
            return "";
        }


        private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }

    }


    /// <summary>
    /// 根据图集标签名称查找图片。偶尔会用到
    /// </summary>
    public class FindUIByTag : EditorWindow
    {
        string tag;

        string checkResult;

        Vector2 _scrollPos;

        [MenuItem("Assets/XGame/UI工具/根据Tag查找UI")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(FindUIByTag));
        }
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            tag = EditorGUILayout.TextField("图集标签", tag);
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (GUILayout.Button("拾取标签")) tag = (TextureImporter.GetAtPath(path) as TextureImporter).spritePackingTag;
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("查找"))
            {
                checkResult = Check(Application.dataPath + "/Artist/Common/Texture/UI/Version1", tag);
                string resPath = Application.dataPath + "/Artist/GResources_/UI";
                if (!Directory.Exists(resPath))
                {
                    resPath = Application.dataPath + "/Artist/GResources/UI";
                }
                checkResult += Check(resPath, tag);

            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            checkResult = string.IsNullOrEmpty(checkResult) ? "无" : checkResult;
            EditorGUILayout.TextArea("匹配结果：\n" + checkResult);
            EditorGUILayout.EndScrollView();

        }

        private static string Check(string path, string tag)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => !s.EndsWith(".meta")).ToArray();
            Debug.Log(string.Format("<color=#006400>匹配ui标签:</color>{0}<color=#006400></color>", tag), AssetDatabase.LoadAssetAtPath<Object>(path));
            string checkResult = "";
            foreach (var item in files)
            {
                TextureImporter tim = AssetImporter.GetAtPath(GetRelativeAssetsPath(item)) as TextureImporter;
                if (tim != null && tim.spritePackingTag == tag)
                {
                    Debug.Log(tim.assetPath, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));
                    checkResult += item.PadLeft(1) + "\n";
                }
            }
            return checkResult;
        }

        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }
    }

    /// <summary>                                                                                                                                                                                                                                                       
    /// 设置UI格式 暂时放弃
    /// </summary>
    public class SetUISpriteFormat : EditorWindow
    {
        string director;
        static string modifiResult;
        Vector2 _scrollPos;

        int count = 0;
        //string path;
        Rect rect;

        //[MenuItem("Assets/XGame/UI工具/设置UI格式")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(SetUISpriteFormat), true, "批量设置UI图片格式");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            #region 通过拖拽获取路径
            EditorGUILayout.LabelField("ui文件夹路径");
            //获得一个长300的框  
            rect = EditorGUILayout.GetControlRect(GUILayout.Width(800));
            //将上面的框作为文本输入框  
            director = EditorGUI.TextField(rect, director);

            //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
            if ((Event.current.type == EventType.DragUpdated
              || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                //改变鼠标的外表  
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    director = "E:/Game/YZSVN/Code/Bin/Client/Game/" + DragAndDrop.paths[0];
                }
            }
            #endregion

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.TextField("参数选择：暂无");
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("批量修改"))
            {
                string[] files = Directory.GetFiles(director, "*.*", SearchOption.AllDirectories)
                .Where(s =>
                    // (!s.EndsWith(".meta"))//s.EndsWith(".png") || s.EndsWith(".PNG") || s.EndsWith(".jpg"))&&
                    (AssetImporter.GetAtPath(GetRelativeAssetsPath(s)) as TextureImporter) != null
                    && (AssetImporter.GetAtPath(GetRelativeAssetsPath(s)) as TextureImporter).textureType == TextureImporterType.Sprite
                ).ToArray();
                count = files.Length;
                modifiResult = "";
                SholProgress(files, director);

            }

            if (GUILayout.Button("清除结果"))
            {
                modifiResult = "";
            }

            if (GUILayout.Button("保存修改"))
            {
                AssetDatabase.Refresh();
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.TextArea(string.Format("修改结束(共{0}个)：\n{1}", count, modifiResult));
            EditorGUILayout.EndScrollView();

        }


        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="files"></param>
        /// <param name="director"></param>
        private static void SholProgress(string[] files, string director)
        {
            int startIndex = 0;
            EditorApplication.update = delegate ()
            {
                string item = files[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("修改中", item, (float)startIndex / (float)files.Length);

                TextureImporter tim = AssetImporter.GetAtPath(GetRelativeAssetsPath(item)) as TextureImporter;
                if (tim != null && tim.textureType == TextureImporterType.Sprite)
                {
                //Debug.Log(tim.assetPath, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));

                tim.textureType = TextureImporterType.Default;
                    tim.anisoLevel = 0;
                    tim.textureType = TextureImporterType.Sprite;
                //tim.filterMode = FilterMode.Bilinear;

                if (string.IsNullOrEmpty(tim.spritePackingTag))
                    {
                        Debug.LogError(item + "_的_ spritePackingTag IsNullOrEmpty!!!!!", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));
                    //tim.mipmapEnabled = false;
                }

                    if (tim.mipmapEnabled)
                    {
                        Debug.LogError(item + "_的_ mipmapEnabled is True Need Check!!!!!", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));
                    //tim.mipmapEnabled = false;
                }
                    if (tim.isReadable)
                    {
                        Debug.LogError(item + "_的_ isReadable is True Need Check!!!!!", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));
                    //tim.mipmapEnabled = false;
                }

                //if (tim.sRGBTexture)
                //{
                //    Debug.LogError(item + "_的_ isReadable is Opacity!!!!!", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));
                //}


                bool isTransparent = tim.spritePackingTag.Contains("transparent");//true：不透明图集标签

                //pc端设置
                //TextureImporterPlatformSettings defaultTP = new TextureImporterPlatformSettings();
                //defaultTP.name = "Default";


                //pc端设置
                TextureImporterPlatformSettings standaloneTP = new TextureImporterPlatformSettings();
                    standaloneTP.name = "Standalone";
                    standaloneTP.overridden = false;
                    tim.SetPlatformTextureSettings(standaloneTP);

                //安卓平台设置
                TextureImporterPlatformSettings androidTP = new TextureImporterPlatformSettings();
                    androidTP.name = "Android";
                    androidTP.overridden = true;
                    androidTP.maxTextureSize = tim.maxTextureSize;
                    androidTP.format = isTransparent ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC_RGB4;
                    androidTP.allowsAlphaSplitting = true;
                    tim.SetPlatformTextureSettings(androidTP);

                //安卓平台设置
                TextureImporterPlatformSettings iPhoneTP = new TextureImporterPlatformSettings();
                    iPhoneTP.name = "iPhone";
                    iPhoneTP.overridden = true;
                    iPhoneTP.maxTextureSize = tim.maxTextureSize;
                    iPhoneTP.format = isTransparent ? TextureImporterFormat.RGBA16 : TextureImporterFormat.PVRTC_RGBA4;
                    iPhoneTP.allowsAlphaSplitting = true;
                    tim.SetPlatformTextureSettings(iPhoneTP);

                    startIndex++;
                    modifiResult += item + "\n";
                }
                else
                {
                    Debug.LogError("TextureImporter is null or TextureImporterType is not Sprite", AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(item)));
                }

                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    Debug.Log(string.Format("<color=#006400>修改结束:</color>{0}<color=#006400>个</color>", startIndex));
                    startIndex = 0;
                }
            };
        }

        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }

        public class CheckImage : EditorWindow
        {
            [MenuItem("Assets/XGame/UI工具/检测prefab Image是否null")]
            static void AddPrefabInfoHelper()
            {
                UnityEngine.Object[] selObjs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
                for (int i = 0; i < selObjs.Length; i++)
                {
                    Object selObj = selObjs[i];
                    string selfPath = AssetDatabase.GetAssetPath(selObj);
                    if (selfPath.EndsWith(".prefab"))
                    {
                        Debug.Log("开始检查");
                        string[] dependPaths = AssetDatabase.GetDependencies(selfPath);
                        GameObject go = GameObject.Instantiate(selObj) as GameObject;
                        Image[] imgs = go.GetComponentsInChildren<Image>();
                        bool isnone = false;
                        for (int j = 0; j < imgs.Length; j++)
                        {
                            if (imgs[j].sprite == null)
                            {

                                Transform parent = imgs[j].transform;

                                string str = "";

                                while (parent != null)
                                {
                                    str += "/" + parent.name;

                                    parent = parent.parent;
                                }
                                Debug.Log("Image NULL：" + str);
                            }
                        }

                        RectTransform[] rects = go.GetComponentsInChildren<RectTransform>();
                        for (int k = 0; k < rects.Length; k++)
                        {

                            if (rects[k].anchoredPosition3D.z != 0)
                            {
                                Transform parent = rects[k].transform;

                                string str = "";

                                while (parent != null)
                                {
                                    str += "/" + parent.name;

                                    parent = parent.parent;
                                }
                                Debug.Log("z!=0：" + str);
                            }
                        }
                        //PrefabInfoHelper pih = go.GetComponent<PrefabInfoHelper>();
                        //if (pih == null)
                        //{
                        //	pih = go.AddComponent<PrefabInfoHelper>();
                        //}
                        //pih.isInstanced = false;
                        //pih.selfPath = selfPath;
                        //pih.dependPathList.Clear();
                        //pih.dependPathList.AddRange(dependPaths);

                        //PrefabUtility.ReplacePrefab(go, selObj, ReplacePrefabOptions.ConnectToPrefab);
                        Debug.Log("检查结束");
                        GameObject.DestroyImmediate(go);
                        //GameObject.Destroy(go);
                        //go = null;

                    }

                }

            }
        }

    }
}
#endif