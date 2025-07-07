/*******************************************************************
** 文件名: IncrementalPackTool.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 代文鹏 
** 日  期: 2020/07/21
** 版  本: 1.0
** 描  述: 增量打包工具
** 应  用: 
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/


using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XGameEditor;
//using Zip;

public class IncrementalPackWindow : EditorWindow
{
    #region 基础路径
    private static readonly string baseDir = "BuildAB";
    private static readonly string recordDir = "Records";
    private static readonly string abDir = "AssetBundles";
    private static readonly string prefix_abDir = "ab_";
    private static readonly string logFile = "log.txt";
    private static readonly char spliteSign = ':';  //分割符号
    private static readonly string postfix_abRes = ".txt";

    private static string BasePath => Application.dataPath.Replace("Assets", "").Replace("\\", "/") + baseDir;
    private static string RecordPath => GetCombinePath(BasePath, recordDir);
    private static string AbOutPutPath => GetCombinePath(BasePath, abDir);
    private static string LogFullPath => GetCombinePath(BasePath, logFile);
    #endregion

    private string[] RecordsList;   //旧版资源集合
    private Dictionary<string, string> OldPath2HashDic = new Dictionary<string, string>();    //旧资源路径 - hash
    private List<string> DifferentResList = new List<string>();  //差异资源集合
    private Dictionary<string, string> NewPath2HashDic = new Dictionary<string, string>();    //新资源路径 - hash

    [MenuItem("XGame/打包工具/生成增量AB包")]
    public static void OpenPackWindow()
    {
        var window = GetWindow<IncrementalPackWindow>("增量AB包生成窗口");
        window.UpdateWindow();
        window.Show(true);
    }

    private void UpdateWindow()
    {
        Start();
        Show();
    }


    private void Start()
    {
        RecordsList = new string[0];
        OldPath2HashDic = new Dictionary<string, string>();
        DifferentResList = new List<string>();
        NewPath2HashDic = new Dictionary<string, string>();

        LoadRecordsList();
    }

    private void OnGUI()
    {
        DrawGUI();
    }

    private void ResetWindow()
    {
        RecordsList = null;
        _versionStr = "";
        selectFileIndex = 0;
        //isCreateZip = true;
        //isCreateLocalRecords = true;

        UpdateWindow();
    }

    #region GUI编辑器绘制
    private Vector2 _guiScrollPos = new Vector2();
    private string _versionStr = "";  //当前版本号
    private int selectFileIndex = 0;    //当前选择的资源文件下标，相对于RecordsList
    private bool isCreateZip = true;   //是否生成压缩包

    private bool isUseLocalRecords = false;    //是否使用本地的记录表
    private int newVerIndex = 0;    //当前资源文件下标

    //private bool IsGetLatestResEnd => DifferentResList.Count > 0;   //是否获取最新资源结束
    private string GetCurVerAbDirName => prefix_abDir + _versionStr;    //获取当前版本ab包名字
    private bool IsHasRecords => RecordsList.Length > 0;    //是否有记录表

    private void DrawGUI()
    {
        if (string.IsNullOrEmpty(_versionStr))
            _versionStr = System.DateTime.Now.ToString("yyyyMMddHHmm");

        using (var srocllArea = new EditorGUILayout.ScrollViewScope(_guiScrollPos))
        {
            _guiScrollPos = srocllArea.scrollPosition;
            EditorGUILayout.Space();

            //开始绘制相关功能
            //GUI.enabled = !IsGetLatestResEnd;
            EditorGUILayout.Space();
            _versionStr = EditorGUILayout.TextField("版本号：", _versionStr);
            EditorGUILayout.Space();

            //是否有旧记录信息
            if (!IsHasRecords)
            {
                if (GUILayout.Button("暂无资源记录表，点击手动生成一份最新记录表"))
                {
                    GetLatestRes();
                    CreateNewResRecords(GetCombinePath(RecordPath, GetCurVerAbDirName) + postfix_abRes, NewPath2HashDic);
                    ResetWindow();
                }
                return;
            }

            //旧版资源记录表路径
            selectFileIndex = EditorGUILayout.Popup("旧版资源文件：", selectFileIndex, RecordsList);
            EditorGUILayout.Space();

            //是否更新本地旧版资源记录表
            isUseLocalRecords = EditorGUILayout.Toggle("使用本地记录表生成增量包", isUseLocalRecords);
            EditorGUILayout.Space();

            //新版资源记录列表
            if (isUseLocalRecords)
            {
                newVerIndex = EditorGUILayout.Popup("新版资源文件：", newVerIndex, RecordsList);
                EditorGUILayout.Space();
            }

            //是否生成压缩包
            isCreateZip = EditorGUILayout.Toggle("同时生成压缩包", isCreateZip);
            EditorGUILayout.Space();

            //生成最新资源记录表
            if (GUILayout.Button("生成增量AB包"))
            {
                if (!CheckAlreadyExist(GetCombinePath(AbOutPutPath, GetCurVerAbDirName)))
                {
                    //加载版本选择表
                    LoadSelectVerRes(GetCombinePath(RecordPath, RecordsList[selectFileIndex]), ref OldPath2HashDic);

                    if (isUseLocalRecords)
                    {
                        LoadSelectVerRes(GetCombinePath(RecordPath, RecordsList[newVerIndex]), ref NewPath2HashDic);
                    }
                    else
                    {
                        //获取最新资源
                        GetLatestRes();

                        //生成新资源记录文件
                        CreateNewResRecords(GetCombinePath(RecordPath, GetCurVerAbDirName) + postfix_abRes, NewPath2HashDic);
                    }

                    //生成差异资源
                    GenerateDifferentAssets();

                    if (DifferentResList.Count > 0)
                    {
       
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("提示", "未发现差异资源，无法生成增量AB包!", "确定");
                    }
                }
                else
                {
                    Debug.LogError("增量AB包生成失败，原因：版本号重复，手动取消！");
                }
            }
            EditorGUILayout.Space();
            //GUI.enabled = IsGetLatestResEnd;

            //if (IsGetLatestResEnd)
            //{
            //    //开始生成增量ab
            //    if (GUILayout.Button("生成增量资源包"))
            //    {
            //        CreateIncrementalPack();
            //    }
            //}

            if (GUILayout.Button("重置窗口"))
            {
                ResetWindow();
                return;
            }
        }
    }
    #endregion

    //加载记录文件
    private void LoadRecordsList()
    {
        if (Directory.Exists(RecordPath))
        {
            string[] files = Directory.GetFiles(RecordPath);
            int length = files.Length;
            RecordsList = new string[length];
            for (int i = 0; i < length; i++)
            {
                RecordsList[i] = Path.GetFileName(files[i]);
            }
            System.Array.Reverse(RecordsList);
        }
        else
        {
            Directory.CreateDirectory(RecordPath);
            Debug.Log("创建了新的记录文件夹：" + RecordPath);
        }
    }

    //加载选择的资源记录文件
    private void LoadSelectVerRes(string filePath, ref Dictionary<string, string> path2HashDic)
    {
        if (path2HashDic == null) path2HashDic = new Dictionary<string, string>();
        else path2HashDic.Clear();

        if (File.Exists(filePath))
        {
            string[] allLines = File.ReadAllLines(filePath);
            foreach (string data in allLines)
            {
                string[] splites = data.Split(spliteSign);
                if (splites.Length != 2)
                {
                    Debug.LogError("数据解析有误：" + data);
                    continue;
                }

                path2HashDic.Add(splites[0], splites[1]);
            }
        }
        else
        {
            Debug.LogError("文件路径有误：" + filePath);
        }
    }

    //生成新资源记录文件
    private void CreateNewResRecords(string filePath, Dictionary<string, string> Path2HashDic)
    {
        List<string> allLines = new List<string>();
        foreach (var item in Path2HashDic)
        {
            allLines.Add(item.Key + spliteSign + item.Value);
        }

        File.WriteAllLines(filePath, allLines.ToArray());
    }

    //检查版本是否已存在
    private bool CheckAlreadyExist(string outputDirPath)
    {
        if (Directory.Exists(outputDirPath))
        {
            bool isDel = EditorUtility.DisplayDialog("版本号重复", $"路径[{outputDirPath}]已存在资源包，是否继续执行操作？\n 继续将会删除已存在资源重新生成，取消将会取消本次操作", "确定", "取消");
            if (isDel)
                Directory.Delete(outputDirPath, true);

            return !isDel;
        }
        return false;
    }

    //获取最新资源
    private void GetLatestRes()
    {
        //先清除记录
        NewPath2HashDic.Clear();

        //刷新ab缓存，要很久....
        NewGAssetDataCache.Instance.RefreshCache();

        //刷新完毕后，获取所有ab缓存
        List<NewGAssetDataCache.NewAssetData> newAssetDatas = NewGAssetDataCache.Instance.assetDataList;
        foreach (var AssetData in newAssetDatas)
        {
            //记录新表
            NewPath2HashDic.Add(AssetData.assetPath, AssetData.hash);
        }
    }

    //生成差异资源，按生成ab包需要的格式
    private void GenerateDifferentAssets()
    {
        DifferentResList.Clear();

        bool isNeedUpdate = false;
        foreach (var item in NewPath2HashDic)
        {
            isNeedUpdate = false;
            //判断是否是差异资源
            if (OldPath2HashDic.ContainsKey(item.Key))
            {
                isNeedUpdate = OldPath2HashDic[item.Key] != item.Value;
            }
            else
            {
                isNeedUpdate = true;
            }

            //记入差异资源列表
            if (isNeedUpdate)
                DifferentResList.Add(item.Key.Replace("assets/", "").Replace("Assets/", ""));
        }
    }


    //获取合并的路径
    private static string GetCombinePath(string path1, string path2)
    {
        return Path.Combine(path1, path2).Replace("\\", "/");
    }
}
