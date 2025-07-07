////编辑器用的测试脚本，因为Unity Assets里没有合适位置便于上传
//#if UNITY_EDITOR

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using XGame.Asset.Loader;
//using rkt;
//using XClient.Common;
//using XClient.UI;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Events;

//namespace UI.TestUIWindow
//{
//    class SnapshotInfo
//    {
//        public DateTime snapshotStartTime = DateTime.MaxValue;
//        public DateTime snapshotFinishTime = DateTime.MinValue;
//        public FileInfo snapshotFileInfo =null;

//        public bool IsValid()
//        {
//            return snapshotStartTime < snapshotFinishTime;
//        }
//    }

//    public class ABSnapShotCollector : MonoBehaviour
//    {
//        #region UIWindows

//        public bool isUIWindowsCollecting = false;
//        public WindowModel curShownWindowModel;
//        string snapShotsDir = Directory.GetParent(Application.dataPath).FullName + "/DebugData/";

//        private Dictionary<WindowModel, SnapshotInfo> m_UIWindow2SnapshotInfo =
//            new Dictionary<WindowModel, SnapshotInfo>();

//        [ContextMenu("AttachUIWindows")]
//        public void AttachUIWindows()
//        {
//            UIWindowTester.Instance.onUIWindowTesterStart.AddListener(OnUIWindowTesterStart);
//            UIWindowTester.Instance.onUIWindowTesterFinish.AddListener(OnUIWindowTesterFinish);
//            UIWindowTester.Instance.onWindowShow.AddListener(OnWindowShow);
//            UIWindowTester.Instance.onWindowHide.AddListener(OnWindowHide);
//            Debug.Log("AttachUIWindows Success");
//        }

//        private void OnWindowHide(WindowModel windowModel)
//        {
//            if (windowModel != curShownWindowModel)
//            {
//                Debug.LogError("OnWindowHide windowModel != curShownWindowModel");
//                return;
//            }

//            GResources.ShowChangeFromLastSnapShot();
//            var snapShotInfo = m_UIWindow2SnapshotInfo[windowModel];
//            if (snapShotInfo == null)
//            {
//                Debug.LogError("OnWindowHide m_UIWindow2SnapshotInfo[windowModel] ==null ,windowModel: " + windowModel);
//                return;
//            }

//            snapShotInfo.snapshotFinishTime = DateTime.Now;
//        }

//        private void OnWindowShow(WindowModel windowModel)
//        {
//            curShownWindowModel = windowModel;
//            var snapShotInfo = new SnapshotInfo();
//            m_UIWindow2SnapshotInfo[windowModel] = snapShotInfo;
//            snapShotInfo.snapshotStartTime = DateTime.Now;
//            GResources.TakeSnapShot();
//        }

//        private void OnUIWindowTesterFinish()
//        {
//            isUIWindowsCollecting = false;
//            MatchFile2SnapShot();
//            SaveSnapShotInfosToCsv();
//        }

//        private void MatchFile2SnapShot()
//        {
//            DirectoryInfo folder = new DirectoryInfo(snapShotsDir);
//            if (!folder.Exists)
//                throw new Exception(string.Format("bad path: {0}", folder.ToString()));

//            var files = folder.GetFiles("*.txt");
//            foreach (var pair in m_UIWindow2SnapshotInfo)
//            {
//                SnapshotInfo info = pair.Value;
//                if (info.snapshotStartTime > info.snapshotFinishTime)
//                {
//                    Debug.LogError("info.snapshotStartTime > info.snapshotFinishTime, windowModel : " + pair.Key);
//                    continue;
//                }

//                foreach (var fileInfo in files)
//                {
//                    DateTime createTime = fileInfo.CreationTime;
//                    if (createTime > info.snapshotStartTime && createTime < info.snapshotFinishTime)
//                    {
//                        info.snapshotFileInfo = fileInfo;
//                        break;
//                    }
//                }
//            }
//        }

//        private void OnUIWindowTesterStart()
//        {
//            isUIWindowsCollecting = true;
//            if (Directory.Exists(snapShotsDir))
//            {
//                Directory.Delete(snapShotsDir, true);
//                Directory.CreateDirectory(snapShotsDir);
//            }
//        }

//        private void SaveSnapShotInfosToCsv()
//        {
//            List<string> resultData = new List<string>();
//            resultData.Add("UI界面,变化类型,变化对象");
//            try
//            {
//                int fileCount = m_UIWindow2SnapshotInfo.Count;
//                EditorUtility.DisplayProgressBar("SaveSnapShotInfosToCsv", fileCount.ToString() + " windowModels", 0);
//                int i = -1;
//                foreach (var pair in m_UIWindow2SnapshotInfo)
//                {
//                    i++;
//                    var snapShotInfo = pair.Value;
//                    if (snapShotInfo == null)
//                    {
//                        continue;
//                    }

//                    var windowModel = pair.Key.ToString();
//                    resultData.Add(windowModel + ",," + "IsValid:" + snapShotInfo.IsValid());

//                    FileInfo file = snapShotInfo.snapshotFileInfo;
//                    if (file == null)
//                    {
//                        Debug.LogError("snapShotInfo.snapshotFileInfo ==null, window:" + windowModel);
//                        continue;
//                    }

//                    var fileFullName = file.FullName;

//                    EditorUtility.DisplayProgressBar("SaveSnapShotInfosToCsv", "window:" + windowModel,
//                        (float) i / fileCount);
//                    using (var reader = File.OpenText(fileFullName))
//                    {
//                        while (!reader.EndOfStream)
//                        {
//                            string line = reader.ReadLine().Replace(", ", ",");
//                            resultData.Add(string.Format("{0},{1}", windowModel, line));
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError(string.Format("load snapshot error ! msg ={0}", ex.Message + ex.StackTrace));
//                EditorUtility.ClearProgressBar();
//                return;
//            }

//            DirectoryInfo folder = new DirectoryInfo(snapShotsDir);
//            if (!folder.Exists)
//                Debug.LogError(string.Format("bad path: {0}", folder.ToString()));

//            var outputCsvPath = snapShotsDir + "/result.csv";
//            using (StreamWriter sw = new StreamWriter(outputCsvPath))
//            {
//                for (int i = 0; i < resultData.Count; i++)
//                {
//                    sw.WriteLine(resultData[i]);
//                }
//            }

//            EditorUtility.RevealInFinder(outputCsvPath);
//            EditorUtility.ClearProgressBar();
//        }

//        #endregion
//    }

//}
//#endif