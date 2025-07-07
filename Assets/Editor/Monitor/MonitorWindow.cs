using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using XClient.Scripts.Test;
using XGame.EventEngine;
using XGame.Memory;
using XClient.Game;
#if UNITY_EDITOR && !UNITY_ANDROID1
using XClient.Scripts.Monitor;
#endif

using UnityEngine.Profiling;


namespace  XClient.Scripts.Monitor
{

    public class MonitorWindow : EditorWindow, IEventExecuteSink
    {
        static readonly private MonitorType AllMonitorType = (MonitorType)(-1);

        static private MonitorMgr _monitorMgr = new MonitorMgr();
        private MonitorType _selectedType = AllMonitorType;
        private MonitorType _oldSelectedType = 0;
        private bool _isIncrease;
        //private bool _isOpenWriteList;
        //private bool _isOpenOutput;
        //private bool _isOpenOthers;
        private Vector2 _scrollViewPos;
        private string _temAddKey;
        private int _temAddCount;
        private string _snapshotDesc;
        //private bool _onlyAddToggle;
        //private bool _notAddToggle;
        private Dictionary<string, bool> _showBoxDic = new Dictionary<string, bool>();

        [MenuItem("XGame/监控器")]
        static public void ShowWindow()
        {
            EditorWindow window = GetWindow<MonitorWindow>("监控器");
            window.Show();
        }

#if UNITY_EDITOR && !UNITY_ANDROID1

        void OutputMonitorState(int nResult)
        {
           
            DoSnapshot();
            OutputResult(false);
            Close();

        }

        private void OnDisable()
        {
            try
            {
                UnloadEvent();
                MonitorMono.IsCanWork();
                MonitorMono.Stop();
                MonitorMono.Dispose();
            }
            catch (Exception e)
            {
                Debug.Log($"监控器：{e}");
                return;
            }
    }
#endif

        private void DrawBox(string label, Action customDraw, bool isDefualtShow = false)
        {
            if (!_showBoxDic.ContainsKey(label))
            {
                _showBoxDic.Add(label, isDefualtShow);
            }
            using (var area = new GUILayout.VerticalScope(new GUIStyle("FrameBox")))
            {
                _showBoxDic[label] = EditorGUILayout.Foldout(_showBoxDic[label], label);
                if (_showBoxDic[label])
                {
                    customDraw.Invoke();
                }
            }
        }
        private void OnEnable()
        {
            WhiteList.Load();
#if UNITY_EDITOR && !UNITY_ANDROID1
            MonitorMono.LoadFilter();
#endif
            _oldSelectedType = 0;
            _outputCurIndex = 0;
            _outputOldIndex = 0;
            _outputNewIndex = 0;
            _snapshotDesc = null;
            _oldInputTabOption = -1;
            _monitorMgr.RefreshMonitorType(_selectedType);
        }

        private void OnGUI()
        {
            _scrollViewPos = GUILayout.BeginScrollView(_scrollViewPos);
            var lastSnapshotInfo = GetSnapshotInfo(_monitorMgr.SnapshotIndex);
            GUILayout.Label(string.Format("当前快照索引：{0,-20} 快照时间：{1,-30}  快照描述：{2}", _monitorMgr.SnapshotIndex, lastSnapshotInfo.time, lastSnapshotInfo.desc));

            DrawMonitorTab();

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 获取快照信息
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private SnapshotInfo GetSnapshotInfo(int index)
        {
            return _monitorMgr.GetSnapshotInfo(index);
        }

#region 选择监控类型
        private int _inputTabOption = 0;
        private int _oldInputTabOption = -1;

        /// <summary>
        /// 绘制输入选择
        /// </summary>
        private void DrawMonitorTab()
        {
            using (var scope = new GUILayout.VerticalScope("Box"))
            {

                _oldInputTabOption = _inputTabOption;
                _inputTabOption = GUILayout.Toolbar(_inputTabOption, new string[] { "应用监控", "Mono监控" });
                EditorGUILayout.Space();
                switch (_inputTabOption)
                {
                    case 0:
                        DrawAPPMonitor();
                        if (_oldInputTabOption != _inputTabOption)
                            _monitorMgr.RefreshMonitorType(_selectedType);
                        break;
                    case 1:
                        DrawMonoInput();
                        //if (_oldInputTabOption != _inputTabOption)
                        //    _monitorMgr.RefreshMonitorType(MonitorType.Mono);
                        break;
                }
            }
        }

        /// <summary>
        /// 应用监控 
        /// </summary>
        private void DrawAPPMonitor()
        {
            DrawAppInput();
            DrawBox("输出数据", DrawOutput, true);
            DrawBox("其他", DrawOthers);
        }

        /// <summary>
        /// 应用层输入
        /// </summary>
        private void DrawAppInput()
        {
            _selectedType = (MonitorType)EditorGUILayout.EnumFlagsField("监控类型", _selectedType);
            //_selectedType &= ~MonitorType.Mono;     //去掉mono
            if (_oldSelectedType != _selectedType)
                _monitorMgr.RefreshMonitorType(_selectedType);
            _oldSelectedType = _selectedType;
            GUILayout.Label("监控器数量：" + _monitorMgr.MonitorCount);
            if (Application.isPlaying)
            {
                if(null!= CGame.Instance)
                {
                    CGame.Instance.m_destroyCallbck = OutputMonitorState;
                }
                


               DrawSnapshot();
            }
                
            else
                EditorGUILayout.HelpBox("只能在游戏状态拍应用层快照！", MessageType.Error);

            DrawBox($"白名单:{MonitorConfig.WhiteListPath}", DrawWhiteList);

        }

        private Stack<ThreadTest> stackTest = new Stack<ThreadTest>();
        private List<ThreadTest> listTest = new List<ThreadTest>();
        private LinkedList<ThreadTest> linkedListTest = new LinkedList<ThreadTest>();
        private Queue<ThreadTest> queueTest = new Queue<ThreadTest>();
        private ThreadTest threadTest = new ThreadTest();
        private ByteData byteData;

        public delegate void OnFrameEvent(Animator animator, int id);
        private OnFrameEvent t;
        private void OnFrameEventFunc(Animator animator, int id)
        {

        }

        private void DoFrameEvent(OnFrameEvent t)
        {

        }
        private OnFrameEvent t2 => OnFrameEventFunc;

        private Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        Dictionary<string, string> testDic = new Dictionary<string, string>();
        Dictionary<string, string> testDic2 = new Dictionary<string, string>(2);
        Dictionary<string, string> testDic3 = new Dictionary<string, string>(128);
        int key = 0;
        private void Test()
        {
            ++key;
            Profiler.BeginSample("AAA ProfilerMonitor Start");
            testDic.Add(key.ToString(), "1");
            Profiler.EndSample();
            Profiler.BeginSample("BBB ProfilerMonitor Start");
            testDic2.Add(key.ToString(), "2");
            Profiler.EndSample();
            Profiler.BeginSample("CCC ProfilerMonitor Start");
            testDic3.Add(key.ToString(), "3");
            Profiler.EndSample();

            //var go = GameObject.Find("DefaultSceneUI");
            //Profiler.BeginSample("AAA ProfilerMonitor Start");
            ////var t1 = RecursiveFindChild(go.transform, "DefaultSceneUI");
            //go.name = "DefaultSceneUI1";
            //Profiler.EndSample();
            //Profiler.BeginSample("BBB ProfilerMonitor Start");
            ////var t2 = go.transform.FindEx("DefaultSceneUI");
            //var a = go.name;
            //Profiler.EndSample();
            //Debug.LogError($"t1:{t1}, t2:{t2}");
            //Profiler.BeginSample("AAA ProfilerMonitor Start");
            //DoFrameEvent((animator, id) => { });
            //Profiler.EndSample();
            //Profiler.BeginSample("BBB ProfilerMonitor Start");
            //DoFrameEvent(OnFrameEventFunc);
            //Profiler.EndSample();
            //Profiler.BeginSample("CCC ProfilerMonitor Start");
            //DoFrameEvent(t);
            //Profiler.EndSample();
            //Profiler.BeginSample("DDD ProfilerMonitor Start");
            //DoFrameEvent(t2);
            //Profiler.EndSample();
            //byteData = new ByteData();
            //Debug.LogError("byteData");
            //i++;
            //Profiler.BeginSample($"AAA ProfilerMonitor Start stackTest");
            //stackTest.Push(threadTest);
            //Profiler.EndSample();
            //Debug.Log("stack数量：" + stackTest.Count);

            //Profiler.BeginSample($"BBB ProfilerMonitor Start listTest");
            //listTest.Add(threadTest);
            //Profiler.EndSample();
            //Debug.Log("list数量：" + listTest.Count);

            //Profiler.BeginSample($"CCC ProfilerMonitor Start linkedListTest");
            //linkedListTest.AddLast(threadTest);
            //Profiler.EndSample();
            //Debug.Log("linkedList数量：" + linkedListTest.Count);


            //Profiler.BeginSample($"DDD ProfilerMonitor Start queueTest");
            //queueTest.Enqueue(threadTest);
            //Profiler.EndSample();
            //Debug.Log("queueTest数量：" + queueTest.Count);

            //Profiler.BeginSample($"AAA ProfilerMonitor Start stackTest Relese");
            //stackTest.Pop();
            //Profiler.EndSample();
            //Debug.Log("stack数量：" + stackTest.Count);

            //Profiler.BeginSample($"BBB ProfilerMonitor Start listTest Relese");
            //listTest.RemoveAt(0);
            //Profiler.EndSample();
            //Debug.Log("list数量：" + listTest.Count);

            //Profiler.BeginSample($"CCC ProfilerMonitor Start linkedListTest Relese");
            //linkedListTest.RemoveFirst();
            //Profiler.EndSample();
            //Debug.Log("linkedList数量：" + linkedListTest.Count);


            //Profiler.BeginSample($"DDD ProfilerMonitor Start queueTest Relese");
            //queueTest.Dequeue();
            //Profiler.EndSample();
            //Debug.Log("queueTest数量：" + queueTest.Count);



            ////UnityEditor.SceneModeUtility.SearchForType(typeof(GameObject));
            ////var a = new Q1Game.AAA[123];
            ////var b = new Q1Game.BBB();
            ////var c = new Q1Game.AAA.CCC[2];
            ////var d = new Q1Game.BBB.DDD[4];
            ////var e = new EEE();
            //////var t = new Q1Game.MonoTestClass();
            //////var m = new TestMonitorClassA();
            //////var a = new Q1Game.ThreadTest.SubThreadTestA[10];
            //var c = new Q1Game.ThreadTest();
            ////var v = new Q1Game.ThreadTest[0];
            ////var v2 = new Q1Game.ThreadTest[1];
            ////var v3 = new Q1Game.ThreadTest[2];
            ////var v4 = new Q1Game.ThreadTest[2000];
            ////var a = (int)TestEnum.AAA;
            ////var b = (TestEnum)2;
            ////var e = rkt.Common.CmdSys.EnCmdID.FallAdd;
            ////var f = (int)rkt.Common.CmdSys.EnCmdID.FallAdd;
            ////var h = (rkt.Common.CmdSys.EnCmdID)106;
            ////if (rkt.Common.CmdSys.EnCmdID.BeMoved < rkt.Common.CmdSys.EnCmdID.FallAdd)
            ////{
            ////}
            ////TestEnumFunc(rkt.Common.CmdSys.EnCmdID.FallAdd);
            //Profiler.BeginSample("AAA ProfilerMonitor Start");
            //int a = 0;
            //Q1Game.ThreadTest.TestAction((t) =>
            //{
            //    int b = a;
            //});
            //Profiler.EndSample();
            //Profiler.BeginSample("AAAAA ProfilerMonitor Start");
            //int aa = 0;
            //Q1Game.ThreadTest.TestAction((t) =>
            //{
            //    int b = a;
            //    int bb = aa;
            //});
            //Profiler.EndSample();

            //Profiler.BeginSample("BBB ProfilerMonitor Start");
            //Q1Game.ThreadTest.TestAction((t) =>
            //{
            //    int d = 22;
            //});
            //Profiler.EndSample();

            //Profiler.BeginSample("CCC ProfilerMonitor Start");
            //Q1Game.ThreadTest.TestAction(TestEnumFunc);
            //Profiler.EndSample();

            //Profiler.BeginSample("DDD ProfilerMonitor Start");
            //Q1Game.ThreadTest.TestAction((t) => { TestEnumFunc(null); });
            //Profiler.EndSample();

            //Profiler.BeginSample("EEE ProfilerMonitor Start");
            //Q1Game.ThreadTest.TestAction((t) => {  });
            //Profiler.EndSample();
            ////var s = rkt.Common.CmdSys.EnCmdID.FallAdd.ToString();
            //Debug.Log("aaaa");
            //Debug.Log($"SubThreadTestA :{sizeof(Q1Game.ThreadTest.SubThreadTestA).ToString()}");
            //var b = new Q1Game.ThreadTest.SubThreadTestB();
            //Thread thread1 = new Thread(()=>
            //{
            //    Debug.Log("开线程");
            //    var b = new Q1Game.ThreadTest.SubThreadTestB();
            //});
            //thread1.Start();
        }

        /// <summary>
        /// Mono层输入
        /// </summary>
        private void DrawMonoInput()
        {
            //HierarchyProperty hierarchyProperty = new HierarchyProperty(HierarchyType.GameObjects);
            //hierarchyProperty.SetSearchFilter("g", 0);
#if UNITY_EDITOR && !UNITY_ANDROID1
            if (!MonitorMono.IsVaild())
            {
                EditorGUILayout.HelpBox("监控mono不可用！请查看是否换了mono的DLL", MessageType.Error);
                return;
            }

            using (var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("初始化监控")) { MonitorMono.Init(); }
                if (GUILayout.Button("销毁监控")) { MonitorMono.Dispose(); }
            }
            using (var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("开启监控")) { MonitorMono.Start(); }
                if (GUILayout.Button("关闭监控")) { MonitorMono.Stop(); }
            }
            using (var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("重置监控")) { MonitorMono.Reset(); }
                if (GUILayout.Button("GC.Collect")) { GC.Collect(); }
                //if (GUILayout.Button("打印快照（慎用）")) { MonitorMono.OutputSnapshot(); }
                //if (GUILayout.Button("GetRecordCount")) { MonitorMono.GetRecordCount(); }
            }
            using (var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("测试创建一个对象"))
                {
                    Test();
                }
                //if (GUILayout.Button("测试创建一个对象2"))
                //{
                //    var a = new Q1Game.ThreadTest.SubThreadTestB();
                //}
            }

            using (var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("mono监控输出"))
                {
                    MonitorMono.Stop();
                    var curTime = DateTime.Now;
                    MonitorMono.OutputMonoSnapshot(curTime);
                    MonitorMono.OutputSizeMapData(curTime);

                    //MonitorMono.OutputSizeMapSnapshot();
                }
            }
            MonitorMono.isEnableGetAllTrace = GUILayout.Toggle(MonitorMono.isEnableGetAllTrace, "是否获取所有堆栈（否则看堆栈过滤表）");

            GUILayout.Label($"监控是否能跑：{MonitorMono.IsCanWork().ToString()}");
            GUILayout.Label($"当前监控记录个数：{MonitorMono.GetRecordCount().ToString()}");

            DrawMonoFilter(MonitorMono.OnlyAddFilter, $"只添加过滤表:", MonitorMono.OnlyAddFilter.FilePath);
            DrawMonoFilter(MonitorMono.NotAddFilter, $"不添加过滤表:", MonitorMono.NotAddFilter.FilePath);
            DrawMonoFilter(MonitorMono.GetTraceFilter, $"获取堆栈过滤表:", MonitorMono.GetTraceFilter.FilePath);
            DrawBox("过滤指定桶Key", DrawSizeMapKeyFilter);

            //DrawSnapshot();
#else
            EditorGUILayout.HelpBox("安卓平台 监控mono不可用！会导致", MessageType.Error);
#endif
        }

        private void TestEnumFunc(ThreadTest a)
        {
            //int f = 353;
            //int ff = 353;
            //var b = arg;
            //a?.Invoke();
            //a?.Invoke(new ThreadTest());
        }
#endregion

#region 拍快照
        private void DrawSnapshot()
        {
            string curSnapshotDescTip = $"索引'{(_monitorMgr.SnapshotIndex + 1)}'的快照描述";
            if (string.IsNullOrEmpty(_snapshotDesc))
            {
                _snapshotDesc = curSnapshotDescTip;
            }
            _snapshotDesc = EditorGUILayout.TextField(curSnapshotDescTip, _snapshotDesc);
            if (GUILayout.Button($"快照来一个，索引：{_monitorMgr.SnapshotIndex + 1}"))
            {
                DoSnapshot();
            }
        }


        private  void DoSnapshot()
        {
            int curIdx = _monitorMgr.Snapshot(_snapshotDesc);
            _snapshotDesc = string.Empty;
            _outputCurIndex = curIdx;
            Debug.LogFormat("拍了一个快照，索引：{0}", curIdx);
        }



#endregion

        #region 白名单

        private void DrawWhiteList()
        {
            using (var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("重新加载"))
                {
                    WhiteList.Load();
                }
                if (GUILayout.Button("保存"))
                {
                    WhiteList.Save();
                }
            }
            using (var scope = new GUILayout.VerticalScope("Box"))
            {
                string needRemoveKey = null;

                foreach (var item in WhiteList.DataDic)
                {
                    using (var scope2 = new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(item.Key);
                        var value = EditorGUILayout.IntField(item.Value);
                        if (GUILayout.Button("删除"))
                        {
                            needRemoveKey = item.Key;
                            break;
                        }
                        if (value != item.Value)
                        {
                            WhiteList.Modify(item.Key, value);
                            break;
                        }
                    }
                }

                if (needRemoveKey != null)
                {
                    WhiteList.Remove(needRemoveKey);
                    return;
                }

                using (var scope3 = new GUILayout.HorizontalScope())
                {
                    _temAddKey = EditorGUILayout.TextField(_temAddKey);
                    _temAddCount = EditorGUILayout.IntField(_temAddCount);
                    if (GUILayout.Button("添加"))
                    {
                        WhiteList.Add(_temAddKey, _temAddCount);
                    }
                }
            }
        }
#endregion

#region Mono过滤列表

        private void DrawMonoFilter(MonoFilterList list, string name, string path)
        {

            DrawBox(name + path, () =>
            {

                using (var scope = new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("选中文件"))
                    {
                        UnityEditor.EditorUtility.RevealInFinder(path);
                    }
                    if (GUILayout.Button("重新加载"))
                    {
                        list.Load();
                    }
                    if (GUILayout.Button("保存"))
                    {
                        list.Save();
                    }
                }
                //using (var scope = new GUILayout.VerticalScope("Box"))
                {
                    string needRemoveKey = null;

                    foreach (var item in list.FilterList)
                    {
                        using (var scope2 = new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(item);
                            if (GUILayout.Button("删除"))
                            {
                                needRemoveKey = item;
                                break;
                            }
                        }
                    }

                    if (needRemoveKey != null)
                    {
                        list.Remove(needRemoveKey);
                        return;
                    }

                    using (var scope3 = new GUILayout.HorizontalScope())
                    {
                        _temAddKey = EditorGUILayout.TextField(_temAddKey);
                        if (GUILayout.Button("添加"))
                        {
                            list.Add(_temAddKey);
                        }
                    }
                }
            });
        }

        private string _sizeMapFilter = "1_3";
        /// <summary>
        /// 过滤桶key
        /// </summary>
        private void DrawSizeMapKeyFilter()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                _sizeMapFilter = EditorGUILayout.TextField("过滤桶Key（清空为不过滤）", _sizeMapFilter);
                if (GUILayout.Button("刷新过滤桶Key"))
                {
                    if (_sizeMapFilter.Length >= 7) // 限制key长度
                    {
                        _sizeMapFilter = _sizeMapFilter.Substring(0, 7);
                    }
#if UNITY_EDITOR && !UNITY_ANDROID
                   // Q1Game_Monitor_SetSizeMapFilter(_sizeMapFilter);
#endif
                }
            }
        }

#endregion

#region 输出数据
        private MonitorType _outputSelectedType = AllMonitorType;
        private int _outputCurIndex = 0;
        private int _outputOldIndex = 0;
        private int _outputNewIndex = 0;
        private bool _outputIsIncrease = true;
        private bool _outputDetail = false;
        private int _outputTabOption = 0;

        private bool m_changeMapAutoRecord;
 

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            _outputOldIndex = _outputNewIndex;
            int curIdx = _monitorMgr.Snapshot(_snapshotDesc);
            _snapshotDesc = string.Empty;
            _outputCurIndex = curIdx;
            _outputNewIndex = _outputCurIndex;
            Debug.LogFormat("拍了一个快照，索引：{0}", curIdx);

          
            string output = GetOutputStr(false);
            if (!Directory.Exists(MonitorConfig.OutputFolder))
            {
                Directory.CreateDirectory(MonitorConfig.OutputFolder);
                //EditorUtility.DisplayDialog("不存在文件夹！", string.Format("文件目录：{0}", MonitorConfig.OutputFolder), "确定");
            }
            //else
            //{
            var path = Path.Combine(MonitorConfig.OutputFolder, GetOutputFileName(_outputTabOption == 1));
            MonitorUtil.WriteToFile(path, output, $"输出监控数据成功：{path}");
        }

        bool addRecord = false;
        void UnloadEvent()
        {
            if (Application.isPlaying&& addRecord)
            {
                /*
                GameHelp.GetEventEngine().UnSubscibe(this, DGlobalEvent.EVENT_SCENE_LOAD_FINISH_EX,
                (byte)EMSOURCE_TYPE.SOURCE_TYPE_SYSTEM, 0);
                */
            }
        }
        /// <summary>
        /// 绘制导出编辑
        /// </summary>
        private void DrawOutput()
        {
            m_changeMapAutoRecord = GUILayout.Toggle(m_changeMapAutoRecord, "切换地图自动记录");
            if (m_changeMapAutoRecord)
            {
                if (Application.isPlaying&&!addRecord)
                {
                    addRecord = true;
                    /*
                    GameHelp.GetEventEngine().Subscibe(this, DGlobalEvent.EVENT_SCENE_LOAD_FINISH_EX,
                  (byte)EMSOURCE_TYPE.SOURCE_TYPE_SYSTEM, 0, "MonitonWin");
                    */
                }
                else
                {
                    addRecord = false;
                }
              
            }
            else
            {
                UnloadEvent();
            }

            using (var scope = new GUILayout.HorizontalScope())
            {
                MonitorConfig.OutputFolder = EditorGUILayout.TextField("输出目录", MonitorConfig.OutputFolder);
                if (GUILayout.Button("浏览"))
                {
                    MonitorConfig.OutputFolder = EditorUtility.OpenFolderPanel("监控数据输出目录", MonitorConfig.OutputFolder, "");
                }
            }
            using (var scope = new GUILayout.VerticalScope("Box"))
            {
                _outputTabOption = GUILayout.Toolbar(_outputTabOption, new string[] { "输出快照", "输出对比" });
                EditorGUILayout.Space();
                _outputSelectedType = (MonitorType)EditorGUILayout.EnumFlagsField("输出监控类型", _outputSelectedType);
                switch (_outputTabOption)
                {
                    case 0:
                        _outputCurIndex = ShowIntPopup("数据索引", _outputCurIndex, 0, _monitorMgr.SnapshotIndex + 1);
                        break;
                    case 1:
                        using (var scope2 = new GUILayout.HorizontalScope())
                        {
                            _outputOldIndex = ShowIntPopup("旧数据索引", _outputOldIndex, 0, _monitorMgr.SnapshotIndex + 1);
                            _outputNewIndex = ShowIntPopup("新数据索引", _outputNewIndex, _outputOldIndex + 1, _monitorMgr.SnapshotIndex - _outputOldIndex);
                        }
                        _outputIsIncrease = GUILayout.Toggle(_outputIsIncrease, "是否只打印增量");
                        _outputDetail = GUILayout.Toggle(_outputDetail, "是否包含详情数据");
                        
                        break;
                }
                using (var scope3 = new GUILayout.HorizontalScope())
                {
                    DrawOutputBtn();
                }
            }
        }


        private void OutputResult(bool showFolder)
        {
            string output = GetOutputStr(false);
            if (!Directory.Exists(MonitorConfig.OutputFolder))
            {
                Directory.CreateDirectory(MonitorConfig.OutputFolder);
                //EditorUtility.DisplayDialog("不存在文件夹！", string.Format("文件目录：{0}", MonitorConfig.OutputFolder), "确定");
            }
            //else
            //{
            var path = Path.Combine(MonitorConfig.OutputFolder, GetOutputFileName(_outputTabOption == 1));
            MonitorUtil.WriteToFile(path, output, $"输出监控数据成功：{path}",true, showFolder);
            //}
        }
        /// <summary>
        /// 绘制输出按钮
        /// </summary>
        private void DrawOutputBtn()
        {
            if (GUILayout.Button("输出到控制台"))
            {
                string output = GetOutputStr(true);
                Debug.Log(output);
            }
            if (GUILayout.Button("输出到输出目录"))
            {
                OutputResult(true);
            }
            /*
            if (GUILayout.Button("全输出"))
            {
                int targetIndex = _outputOldIndex;
                switch (_outputTabOption)
                {
                    case 0:
                        targetIndex = _outputCurIndex;
                        break;
                    case 1:
                        targetIndex = _outputOldIndex;
                        break;
                }
                var lifeList = _monitorMgr.GetLifeList(_outputSelectedType);
                int count = lifeList.Count;
                MonitorResultReader.IsColorful = false;
                var nullNode = GetSnapshotInfo(-1);
                nullNode.index = -1;
                string output = MonitorResultReader.GetAllLifeDataDifferent(lifeList, nullNode, GetSnapshotInfo(targetIndex), true);

                var path = Path.Combine(MonitorConfig.OutputFolder, GetOutputFileName(_outputTabOption == 1));
                MonitorUtil.WriteToFile(path, output, $"输出监控数据成功：{path}");
            }*/
        }
        

        List<int> _temIntList = new List<int>();
        List<string> _temIntStrList = new List<string>();
        /// <summary>
        /// 显示Int下拉列表
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="cur">当前索引</param>
        /// <param name="start">起始值</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        private int ShowIntPopup(string label, int cur, int start, int length)
        {
            _temIntList.Clear();
            _temIntStrList.Clear();
            for (int i = start; i < start + length; i++)
            {
                _temIntList.Add(i);
                _temIntStrList.Add($"{i.ToString()}：{_monitorMgr.GetSnapshotInfo(i).desc}");
            }
            int result = EditorGUILayout.IntPopup(label, cur, _temIntStrList.ToArray(), _temIntList.ToArray());
            if (result < start || result >= start + length)
            {
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 获取输出字符串，控制台输出变化量会带颜色
        /// </summary>
        /// <param name="isConsole">是否是控制台输出</param>
        /// <returns></returns>
        private string GetOutputStr(bool isConsole)
        {
            string output = "【无效输出】";
            var lifeList = _monitorMgr.GetLifeList(_outputSelectedType);
            int count = lifeList.Count;

            MonitorResultReader.IsUseForConsole = isConsole;

            switch (_outputTabOption)
            {
                case 0:
                    if (_outputCurIndex != -1)
                    {
                        MonitorResultReader.IsColorful = false;
                        MonitorResultReader.IsIncludeDetail = false;
                       output = MonitorResultReader.GetAllLifeDataAtIndex(lifeList, GetSnapshotInfo(_outputCurIndex));
                    }
                    break;
                case 1:
                    if (_outputOldIndex != -1 && _outputNewIndex != -1)
                    {
                        MonitorResultReader.IsColorful = isConsole;
                        MonitorResultReader.IsIncludeDetail = _outputDetail;
                        output = MonitorResultReader.GetAllLifeDataDifferent(lifeList, 
                            GetSnapshotInfo(_outputOldIndex), GetSnapshotInfo(_outputNewIndex), _outputIsIncrease);
                    }
                    break;
            }

            return output;
        }

        /// <summary>
        /// 获取输出文件名
        /// </summary>
        /// <param name="isDifferent">是否是差异文件，否则是快照</param>
        /// <returns></returns>
        private string GetOutputFileName(bool isDifferent)
        {
            return isDifferent ? GetOutputDiffFileName() : GetOuputSnapshotFileName();
        }

        /// <summary>
        /// 获取输出差异文件名
        /// </summary>
        /// <returns></returns>
        private string GetOutputDiffFileName()
        {
            return string.Format("Different_{0}_{1}_{2}.csv", GetFormatTime(_outputNewIndex), _outputOldIndex, _outputNewIndex);
        }

        /// <summary>
        /// 获取输出快照文件名
        /// </summary>
        /// <returns></returns>
        private string GetOuputSnapshotFileName()
        {
            return string.Format("Snapshot_{0}_{1}.csv", GetFormatTime(_outputCurIndex), _outputCurIndex);
        }

        /// <summary>
        /// 获取格式化的快照时间
        /// </summary>
        /// <param name="index">快照索引</param>
        /// <returns></returns>
        private string GetFormatTime(int index)
        {
            return MonitorUtil.FormatTime(GetSnapshotInfo(index).time);
        }
        

#endregion

#region 其他

        private void DrawOthers()
        {
            if (GUILayout.Button("清空所有数据"))
            {
                _monitorMgr.ClearAll();
                _monitorMgr.RefreshMonitorType(_selectedType);
            }
        }

#endregion
    }
}