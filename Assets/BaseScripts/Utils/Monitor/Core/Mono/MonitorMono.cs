using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR && !UNITY_ANDROID1
namespace XClient.Scripts.Monitor
{
    public class MonitorMono
    {
        static public bool isEnableGetAllTrace = false;     // 是否获取堆栈信息

        private const int DATA_WORD_LENGTH = 17;         // 无符号长整型 转换成字符串的长度，16 + 1位 最后一位是‘\0’
        private const int DATA_NAME_LENGTH = 128;        // 数据名字长度
        private const int DATA_OUTPUT_MAX = 1024;        // 最多输出个数
        private const int DATA_STACK_MAX = 5;            // 栈信息最大层数
        private const int DATA_TRACE_MAX = 512;          // 栈信息字符串长度
        private const int DATA_SIZE_MAP_KEY_MAX = 8;     // 桶Key
        private const int DATA_SIZE_MAP_OBJTYPE_MAX = 4096;     // 桶对象类型数量
        private const int DATA_SIZE_MAP_COUNT_MAX = 256;     // 桶数量
        private const int DATA_SIZE_OBJTYPE_MAX = 256;     // 桶对象类型数量

        private const string AssetsFolder = "Assets\\";

        static public MonoFilterList OnlyAddFilter { get; private set; }
        static public MonoFilterList NotAddFilter { get; private set; }
        static public MonoFilterList GetTraceFilter { get; private set; }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_UnitData
        {
            public ulong size;       // 占用内存大小

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_WORD_LENGTH)]
            public string strKey;       // 存内存地址

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_NAME_LENGTH)]
            public string strName;      // 存对象名字（命名空间.子命名空间.类名.内嵌类名）

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_TRACE_MAX)]
            public string strTrace;     // 创建堆栈
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_AllData
        {
            public int count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DATA_OUTPUT_MAX)]
            public Q1Game_Monitor_UnitData[] data;  // 所有输出数据

        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_SizeMapObjData
        {
            public int objCount;        // 对象数量
            public int gcCount;        // 对象数量
            public ulong size;          // 总占内存大小
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_NAME_LENGTH)]
            public string name;      // 存对象名字（命名空间.子命名空间.类名.内嵌类名）
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_SizeMapData
        {
            public int isSmallObj;     // 是否小对象
            public int objTypeCount;    // 对象类型数量
            public int gcCount;         // gc次数
            public ulong allocateSize;  // 分配内存大小
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_SIZE_MAP_KEY_MAX)]
            public string strKey;      // 存桶名字"是否小对象_桶索引"

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DATA_SIZE_OBJTYPE_MAX)]
            public Q1Game_Monitor_SizeMapObjData[] objDataTypeArr;  // 所有输出数据
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_SizeMapDataList
        {
            public int count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DATA_SIZE_MAP_COUNT_MAX)]
            public Q1Game_Monitor_SizeMapData[] data;  // 所有输出数据
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_SizeMapKey
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_SIZE_MAP_KEY_MAX)]
            public string strKey;      // 存桶名字"是否小对象_桶索引"
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_Monitor_SizeMapKeyList
        {
            public int count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DATA_SIZE_MAP_COUNT_MAX)]
            public Q1Game_Monitor_SizeMapKey[] data;  // 所有输出数据Key
        }

        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_Init(GetStackTraceCallback traceHandle);
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_Destroy();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_Start();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_Stop();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_IsStart();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_IsCanWork();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_GetDataSize();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_GetAddCount();
        [DllImport("__Internal")]
        private extern static int Q1Game_Monitor_GetRemoveCount();

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private extern static int Q1Game_Monitor_GetSnapshot(ref Q1Game_Monitor_AllData data, int startIndex);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private extern static void Q1Game_Monitor_SetOnlyAddFilter(string[] names, int count);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public extern static void Q1Game_Monitor_SetNotAddFilter(string[] names, int count);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public extern static void Q1Game_Monitor_SetGetTraceFilter(string[] names, int count);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public extern static void Q1Game_Monitor_DestroyGetTraceFilter();
        
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public extern static void Q1Game_Monitor_SetSizeMapFilter(string sizeMapFilter);


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetStackTraceCallback(ref Q1Game_Monitor_UnitData str);


        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private extern static int Q1Game_Monitor_GetSizeMapSnapshot(ref Q1Game_Monitor_SizeMapDataList dataList);
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private extern static int Q1Game_Monitor_GetSizeMapKeyList(ref Q1Game_Monitor_SizeMapKeyList keyList);
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private extern static int Q1Game_Monitor_GetSizeMapData(string key, ref Q1Game_Monitor_SizeMapData data,int MaxCount);


        private static System.Diagnostics.StackTrace _temTrace;
        private static System.Diagnostics.StackFrame _temFrame;
        private static System.Reflection.MethodBase _temMethod;
        private static string _temFrameFileName;
        private static StringBuilder _temStringBuilder = new StringBuilder();

        /// <summary>
        /// 获取堆栈，塞进监控数据里面
        /// </summary"
        /// <param name="data"></param>
        private static void GetStackTraceCallbackFunc(ref Q1Game_Monitor_UnitData data)
        {

/*
            if (data.strName.IndexOf("I18N.CJK.CP936Decoder") >= 0)
            {
                Debug.LogError("I18N.CJK.CP936Decoder");
            }
*/
            _temTrace = new System.Diagnostics.StackTrace(1, true);
            _temStringBuilder.Clear();
            var validIndex = 0;
            for (int i = 0; validIndex < DATA_STACK_MAX && i < _temTrace.FrameCount; i++)
            {
                _temFrame = _temTrace.GetFrame(i);
                _temFrameFileName = _temFrame.GetFileName();
                if (_temFrameFileName == null)  // 为空可能是跑到Mono里面了
                    continue;
                int assetIndex = _temFrameFileName.IndexOf(AssetsFolder);
                if (assetIndex > 0)
                    _temFrameFileName = _temFrameFileName.Substring(assetIndex + AssetsFolder.Length);
                _temMethod = _temFrame.GetMethod();
                _temStringBuilder.AppendLine($"{_temMethod.DeclaringType.ToString()}.{_temMethod.Name} (at {_temFrameFileName}:{_temFrame.GetFileLineNumber().ToString()})");
                ++validIndex;
            }
            data.strTrace = _temStringBuilder.ToString();

            //if (data.strTrace.Contains("Test_String"))
            //{
            //    Debug.LogError($"红红火火恍恍惚惚：【{_temTrace}】");
            //}
            //else
            //{
            //    Debug.Log($"红红火火恍恍惚惚：【{_temTrace}】");
            //}
            //Debug.Log(str.strTrace.ToString());
            //Debug.Log(UnityEngine.StackTraceUtility.ExtractStackTrace());
        }

        /// <summary>
        /// 加载过滤表
        /// </summary>
        public static void LoadFilter()
        {
            OnlyAddFilter = new MonoFilterList(MonitorConfig.MonoOnlyAddFilterPath);
            OnlyAddFilter.Load();
            NotAddFilter = new MonoFilterList(MonitorConfig.MonoNotAddFilterPath);
            NotAddFilter.Load();
            GetTraceFilter = new MonoFilterList(MonitorConfig.MonoNotGetTraceFilterPath);
            GetTraceFilter.Load();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            Q1Game_Monitor_Init(GetStackTraceCallbackFunc);
            LoadFilter();
            MonitorMono.SetOnlyAddFilter(OnlyAddFilter.FilterList.ToArray());
            MonitorMono.SetNotAddFilter(NotAddFilter.FilterList.ToArray());
            if (isEnableGetAllTrace)
            {
                Q1Game_Monitor_DestroyGetTraceFilter();
            }
            else
            {
                MonitorMono.SetGetTraceFilter(GetTraceFilter.FilterList.ToArray());
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public static void Dispose()
        {
            Q1Game_Monitor_Destroy();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public static void Reset()
        {
            Q1Game_Monitor_Stop();
            Q1Game_Monitor_Destroy();
            Q1Game_Monitor_Init(GetStackTraceCallbackFunc);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            Q1Game_Monitor_Start();
        }

        /// <summary>
        /// 启动
        /// </summary>
        public static void Stop()
        {
            Q1Game_Monitor_Stop();
        }

        /// <summary>
        /// 是否启动了
        /// </summary>
        public static bool IsStart()
        {
            return Q1Game_Monitor_IsStart() == 1;
        }

        /// <summary>
        /// 是否有效监控
        /// </summary>
        /// <returns></returns>
        public static bool IsVaild()
        {
            try
            {
                Q1Game_Monitor_IsCanWork();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 是否可以工作，初始化完且成功，并且启动了
        /// </summary>
        /// <returns></returns>
        public static bool IsCanWork()
        {
            bool isCanWork = false;
            try
            {
                isCanWork = Q1Game_Monitor_IsCanWork() == 1;
            }
            catch (Exception e)
            {
                return false;
            }

            return isCanWork;
        }

        /// <summary>
        /// 获取监控数量
        /// </summary>
        /// <returns></returns>
        public static int GetRecordCount()
        {
            //string a = "aaaaa" + "bb";
            //string b = "cc";
            var result = Q1Game_Monitor_GetDataSize();
            //string c = "ccccc" + "bb";
            //string d = "ddd";
            return result;
        }

        /// <summary>
        /// 获取添加监控的数量
        /// </summary>
        /// <returns></returns>
        public static int GetAddCount()
        {
            return Q1Game_Monitor_GetDataSize();
        }

        /// <summary>
        /// 获取移除监控的数量
        /// </summary>
        /// <returns></returns>
        public static int GetRemoveCount()
        {
            return Q1Game_Monitor_GetDataSize();
        }

        /// <summary>
        /// 获取快照数据
        /// </summary>
        /// <returns></returns>
        public static bool GetAllData(ref Q1Game_Monitor_AllData allData)
        {
            List<Q1Game_Monitor_UnitData> result = new List<Q1Game_Monitor_UnitData>();
            var index = 0;
            bool isCancel = false;
            while (true)
            {

#if UNITY_EDITOR
                isCancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar($"获取快照数据", $"获取数量：{index}", 0f);
                if (isCancel)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    break;
                }
#endif
                MonitorMono.Q1Game_Monitor_AllData temData = new MonitorMono.Q1Game_Monitor_AllData();
                temData.data = new Q1Game_Monitor_UnitData[DATA_OUTPUT_MAX];
                if (Q1Game_Monitor_GetSnapshot(ref temData, index) != 0)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.ClearProgressBar();
#endif
                    return false;
                }
                result.AddRange(temData.data);
                if (temData.count < DATA_OUTPUT_MAX)
                    break;
                else
                    index += DATA_OUTPUT_MAX;
            }
            allData.count = result.Count;
            allData.data = result.ToArray();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
            return true;
        }

        /// <summary>
        /// 获取快照数据
        /// </summary>
        /// <returns></returns>
        public static Q1Game_Monitor_AllData OutputSnapshot()
        {
            Q1Game_Monitor_AllData allData = new Q1Game_Monitor_AllData();
            if (!GetAllData(ref allData))
            {
                Debug.LogError("获取mono监控数据失败");
                return allData;
            }
            allData.data = new Q1Game_Monitor_UnitData[DATA_OUTPUT_MAX];

            Q1Game_Monitor_GetSnapshot(ref allData, 0);
            Debug.Log($" 总共 {allData.count} 个数据");

            for (int i = 0; i < allData.count; i++)
            {
                //Debug.LogError($" {i}: p:{allData.data[i].strKey}   name:{allData.data[i].strName}");
                Debug.Log($" {i}: name:【{allData.data[i].strName}】【size:{allData.data[i].size}】 trace:【{allData.data[i].strTrace}】");
            }
            return allData;
        }

        /// <summary>
        /// 设置只添加过滤名字
        /// </summary>
        /// <param name="names"></param>
        public static void SetOnlyAddFilter(params string[] names)
        {
            Q1Game_Monitor_SetOnlyAddFilter(names, names.Length);
        }

        /// <summary>
        /// 设置不过滤名字
        /// </summary>
        public static void SetNotAddFilter(params string[] names)
        {
           // names = new string[0];
            Q1Game_Monitor_SetNotAddFilter(names, names.Length);
        }

        /// <summary>
        /// 设置获取堆栈名字
        /// </summary>
        public static void SetGetTraceFilter(params string[] names)
        {
            Q1Game_Monitor_SetGetTraceFilter(names, names == null ? 0 : names.Length);

            /*
            if(names.Length>0)
            {
                Q1Game_Monitor_SetSizeMapFilter(names[0]);
            }
            */
        }

        /// <summary>
        /// 获取桶快照数据
        /// </summary>
        /// <returns></returns>
        public static bool GetAllSizeMapData(ref Q1Game_Monitor_SizeMapDataList allData)
        {
            //废弃，数据不能通过一个函数一下子获取大量数据，会崩
            //return Q1Game_Monitor_GetSizeMapSnapshot(ref allData) == 0;

            Q1Game_Monitor_SizeMapKeyList allKey = new Q1Game_Monitor_SizeMapKeyList();
            var errCode = Q1Game_Monitor_GetSizeMapKeyList(ref allKey);
            if (errCode != 0)
            {
                Debug.LogError($"获取所有桶Key失败 errCode:{errCode}");
                return false;
            }

            allData.count = allKey.count;
            allData.data = new Q1Game_Monitor_SizeMapData[DATA_SIZE_MAP_COUNT_MAX];

            Debug.Log($" 总共 {allKey.count} 个数据");
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < allKey.count; i++)
            {

                Q1Game_Monitor_SizeMapData map = new Q1Game_Monitor_SizeMapData();
                map.objDataTypeArr = new Q1Game_Monitor_SizeMapObjData[DATA_SIZE_OBJTYPE_MAX];
                string key = System.Text.RegularExpressions.Regex.Unescape(allKey.data[i].strKey);
                errCode = Q1Game_Monitor_GetSizeMapData(allKey.data[i].strKey, ref map, DATA_SIZE_OBJTYPE_MAX);
                //errCode = Q1Game_Monitor_GetSizeMapData_Test2(ref map);
                if (errCode != 0)
                {
                    Debug.LogError($"获取桶Key数据失败！key:{allKey.data[i].strKey} errCode:{errCode}");
                    continue;
                }
                allData.data[i] = map;
            }
            return true;
        }

        /// <summary>
        /// 获取桶数据文件名
        /// </summary>
        /// <returns></returns>
        static public string GetOuputDataFileName(DateTime time)
        {
            return $"MonoData_{MonitorUtil.FormatTime(time)}.csv";
        }

        /// <summary>
        /// 获取桶数据文件名
        /// </summary>
        /// <returns></returns>
        static public string GetOuputSizeMapFileName(DateTime time)
        {
            return $"MonoSizeMap_{MonitorUtil.FormatTime(time)}.csv";
        }

        /// <summary>
        /// 获取桶数据文件名
        /// </summary>
        /// <returns></returns>
        static public string GetOuputSizeMapObjFileName(DateTime time)
        {
            return $"MonoSizeMapObj_{MonitorUtil.FormatTime(time)}.csv";
        }

        public static void OutputMonoSnapshot(DateTime curTime)
        {
            string content = "类名,单个对象大小,堆栈,数量\n";
            content += MonitorResultReader.GetNodeData(MonoNode.GetSnapShot());
            var file = GetOuputDataFileName(curTime);

            if(Directory.Exists(MonitorConfig.OutputFolder)==false)
            {
                Directory.CreateDirectory(MonitorConfig.OutputFolder);
            }
            var path = Path.Combine(MonitorConfig.OutputFolder, file);
            MonitorUtil.WriteToFile(path, content, $"输出mono对象数据：{path}");
        }

        /// <summary>
        /// 桶数据
        /// </summary>
        public static MonitorMono.Q1Game_Monitor_SizeMapDataList GetSizeMapDataList()
        {
            MonitorMono.Q1Game_Monitor_SizeMapDataList allData = new MonitorMono.Q1Game_Monitor_SizeMapDataList();
            MonitorMono.GetAllSizeMapData(ref allData);
            return allData;
        }

        /// <summary>
        /// 输出桶数据
        /// </summary>
        public static void OutputSizeMapData(DateTime curTime)
        {
            var allData = GetSizeMapDataList();
            var sizeMapFile = GetOuputSizeMapFileName(curTime);
            var sizeMapDataFile = GetOuputSizeMapObjFileName(curTime);

            StringBuilder sizeMapStr = new StringBuilder();
            sizeMapStr.AppendLine("桶Key,是否小对象,GC总次数,总申请内存,对象种类数量");

            StringBuilder sizeMapDataStr = new StringBuilder();
            sizeMapDataStr.AppendLine("桶Key,是否小对象,对象名字,对象数量,总申请内存,对象GC次数");

            bool isCancel = false;
            var count = allData.count;
            for (int i = 0; i < count; i++)
            {
                var map = allData.data[i];

#if UNITY_EDITOR
                isCancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar($"输出SizeMapData: ({i} / {count})", $"{map.strKey}", (float)i / count);
                if (isCancel)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    break;
                }
#endif
                //sizeMapStr.AppendLine($"    key:{map.strKey}, isSmallObj:{map.isSmallObj == 1}, gcCount:{map.gcCount}, allocateSize:{map.allocateSize}, objTypeCount:{map.objTypeCount}");
                sizeMapStr.AppendLine($"{map.strKey},{map.isSmallObj},{map.gcCount},{map.allocateSize},{map.objTypeCount}");
                for (int j = 0; j < map.objTypeCount; j++)
                {
                    var objData = map.objDataTypeArr[j];
                    sizeMapDataStr.AppendLine($"{map.strKey},{map.isSmallObj},{objData.name},{objData.objCount},{objData.size},{objData.gcCount}");
                    //str.AppendLine($"        {j} name:{objData.name}, objCount:{objData.objCount}, size:{objData.size}");
                }
                //Debug.LogError($" {i}: p:{allData.data[i].strKey}   name:{allData.data[i].strName}");
            }
            Debug.Log(sizeMapStr.ToString());

#if UNITY_EDITOR
            isCancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar($"输出数据写入表格", $"输出数据写入表格", 1f);
#endif

            var sizeMapPath = Path.Combine(MonitorConfig.OutputFolder, sizeMapFile);
            MonitorUtil.WriteToFile(sizeMapPath, sizeMapStr.ToString(), $"输出桶数据成功：{sizeMapPath}");

            var sizeMapObjPath = Path.Combine(MonitorConfig.OutputFolder, sizeMapDataFile);
            MonitorUtil.WriteToFile(sizeMapObjPath, sizeMapDataStr.ToString(), $"输出桶对象数据成功：{sizeMapObjPath}");
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        #region 测试

        [DllImport("__Internal")]
        //[MethodImpl(MethodImplOptions.InternalCall)]
        private extern static int Q1Game_Test();
        [DllImport("__Internal")]
        //[MethodImpl(MethodImplOptions.InternalCall)]
        private extern static int Q1Game_TestA();

        [DllImport("__Internal")]
        //[MethodImpl(MethodImplOptions.InternalCall)]
        private extern static int Q1Game_Test2();

        [DllImport("__Internal", CharSet = CharSet.Unicode)]
        private extern static void Q1Game_Test3(StringBuilder str, int size);
        //[DllImport("__Internal", CharSet = CharSet.Unicode)]
        //private extern static void Q1Game_Test3(string str, int size);

        private static StringBuilder _temResult = new StringBuilder(256);
        public static string GetTest3Result()
        {
            _temResult.Clear();
            Q1Game_Test3(_temResult, _temResult.Capacity);
            return _temResult.ToString();

            //string tem = "";
            //Q1Game_Test3(tem, _temResult.Capacity);
            //return tem;
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_TestSubStruct
        {
            public int intValue;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public int[] arrIntValue;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strValue;
        }

        [StructLayout(LayoutKind.Sequential)]       // 按顺序字节对齐
        public struct Q1Game_TestStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Q1Game_TestSubStruct[] arrSubStruct;


        }
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private extern static void Q1Game_Test4(ref Q1Game_TestStruct data);


        public static Q1Game_TestStruct GetTest4Result()
        {
            Q1Game_TestStruct data = new Q1Game_TestStruct();
            data.arrSubStruct = new Q1Game_TestSubStruct[10];

            //for (int i = 0; i < data.arrSubStruct.Length; i++)
            //{
            //    Debug.LogError($"前 {i}  {data.arrSubStruct[i].c}");
            //}
            Q1Game_Test4(ref data);

            for (int i = 0; i < data.arrSubStruct.Length; i++)
            {
                Debug.LogError($"后 {i}  {data.arrSubStruct[i].intValue}  {data.arrSubStruct[i].strValue}");
            }
            return data;
        }



        #endregion
    }
}
#endif