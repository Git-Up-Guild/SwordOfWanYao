
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XGameEditor
{
    /// <summary>
    /// 自定义AB打包工具
    /// </summary>
    public class CustomABBuilder : EditorWindow
    {
        static public string exportCopyBatPath = "";
        static public string androidHotUpdateABPath = "/storage/emulated/0/Android/data/com.q1.survival/files/assetbundles/android";
        static private string androidFile = "StreamingAssets/assetbundles/android/android";
        static private string androidMainfestFile = "StreamingAssets/assetbundles/android/android.manifest";
        static private string AndroidFile { get { return Application.dataPath + "/" + androidFile; ; } }
        static private string AndroidMainfestFile { get { return Application.dataPath + "/" + androidMainfestFile; ; } }
        readonly static private string TemFileSuffix = ".tmp";
        static private string[] _allABNames;
        static private string _outputPath = "StreamingAssets/assetbundles/android";
        static private int _selectedABIdx;
        static private List<string> _selectedABList = new List<string>();
        static private string _errMsg;


        [MenuItem("XGame/打包工具/自定义打AB工具")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<CustomABBuilder>("自定义打AB工具");
            window.Show();
        }

        // 刷新AB列表
        static public void FrocusUpdate()
        {
            _allABNames = AssetDatabase.GetAllAssetBundleNames();
            _selectedABList.Clear();

            if (string.IsNullOrEmpty(exportCopyBatPath))
            {
                exportCopyBatPath = Application.dataPath;
            }
        }

        // 检查安卓mainfest文件是否存在
        static private bool CheckFile(out string errMsg)
        {
            string androiF = AndroidFile;
            if (!File.Exists(androiF))
            {
                errMsg = "找不到存在文件：" + androiF;
                return false;
            }
            string androiMainfestF = AndroidMainfestFile;
            if (!File.Exists(androiMainfestF))
            {
                errMsg = "找不到存在文件：" + androiMainfestF;
                return false;
            }
            errMsg = "";
            return true;
        }

        private void OnGUI()
        {
            UpdateUI();
        }

        static public void UpdateUI()
        {
            if (!CheckFile(out _errMsg))
            {
                EditorGUILayout.HelpBox(_errMsg, MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("导出根目录：" + _outputPath);
            if (GUILayout.Button("刷新AB列表") || _allABNames == null)
            {
                FrocusUpdate();
            }
            if (_allABNames == null)
            {
                return;
            }
            EditorGUILayout.LabelField(string.Format("共 {0} 个AB路径", _allABNames.Length));

            EditorGUILayout.BeginHorizontal();
            _selectedABIdx = EditorGUILayout.Popup("选择要更新的AB", _selectedABIdx, _allABNames);

            if (GUILayout.Button("添加"))
            {
                string selectedAB = _allABNames[_selectedABIdx];
                if (!_selectedABList.Contains(selectedAB))
                {
                    _selectedABList.Add(selectedAB);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            ShowSelectedList();
            EditorGUILayout.EndVertical();

            string outputPath = string.Format("{0}/{1}", Application.dataPath, _outputPath);
            if (GUILayout.Button("直接打AB到路径：" + outputPath))
            {
                DoExport(outputPath);
            }
            EditorGUILayout.Space();
            ShowExportCopyBat(outputPath);
        }

        // 展示可选AB列表
        static private void ShowSelectedList()
        {
            int count = _selectedABList.Count;
            EditorGUILayout.LabelField(string.Format("选中的AB列表，共 {0} 个", count));
            for (int i = 0; i < count; i++)
            {
                string curSelectdABName = _selectedABList[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(curSelectdABName);

                if (GUILayout.Button("删除"))
                {
                    _selectedABList.RemoveAt(i);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        // 是否合法
        static private bool IsExportValid(string outputPath)
        {
            int count = _selectedABList.Count;
            if (_selectedABList.Count == 0)
            {
                Debug.LogError("要打AB列表为空！");
                return false;
            }
            if (!Directory.Exists(outputPath))
            {
                Debug.LogError("不存在输出路径:" + outputPath);
                return false;
            }
            return true;
        }

        // 导出生成的AB
        static private void DoExport(string outputPath)
        {
            if (!IsExportValid(outputPath))
                return;
            int count = _selectedABList.Count;
            var targetAsset = new AssetBundleBuild[count];
            for (int i = 0; i < count; i++)
            {
                string curABName = _selectedABList[i];
                targetAsset[i].assetBundleName = curABName;
                targetAsset[i].assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(curABName);
                Debug.LogFormat("导出AB：包名:{0}  包含资源数量：{1}  路径：{2}", curABName, targetAsset[i].assetNames.Length, outputPath);
            }

            MoveFile(AndroidFile, AndroidFile + TemFileSuffix);
            MoveFile(AndroidMainfestFile, AndroidMainfestFile + TemFileSuffix);
            BuildPipeline.BuildAssetBundles(outputPath, targetAsset, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            MoveFile(AndroidFile + TemFileSuffix, AndroidFile);
            MoveFile(AndroidMainfestFile + TemFileSuffix, AndroidMainfestFile);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        // 移动文件（如果之前有，删掉之前的）
        static private bool MoveFile(string file, string targetFile)
        {
            if (!File.Exists(file))
            {
                Debug.LogError("找不到文件：" + file);
                return false;
            }
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            File.Move(file, targetFile);

            return true;
        }

        // 导出复制到手机批处理
        static private void ShowExportCopyBat(string srcPath)
        {
            EditorGUILayout.TextField("手机热更AB路径：", androidHotUpdateABPath);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("‘复制AB到手机’批处理路径：", exportCopyBatPath);
            if (GUILayout.Button("浏览"))
            {
                exportCopyBatPath = EditorUtility.OpenFolderPanel("选择批处理生成目录", "", "");
            }
            EditorGUILayout.EndHorizontal();
            string outputPath = exportCopyBatPath + "/AB_copy.bat";
            if (GUILayout.Button("生成 ‘复制AB到手机’ 批处理"))
            {
                if (!IsExportValid(srcPath))
                    return;
                StringBuilder content = new StringBuilder();
                int count = _selectedABList.Count;
                for (int i = 0; i < count; i++)
                {
                    string srcABPath = srcPath + "/" + _selectedABList[i];
                    string desABPath = androidHotUpdateABPath + "/" + _selectedABList[i];

                    content.AppendFormat("adb push {0} {1}", srcABPath, desABPath);
                    content.AppendLine();
                }
                content.AppendLine("pause");
                Debug.Log("导出‘直接复制AB到手机’批处理路径:\n" + content.ToString());
                File.WriteAllText(outputPath, content.ToString());
            }
        }
    }
}
