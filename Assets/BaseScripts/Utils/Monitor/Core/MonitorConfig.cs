using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 全局配置
    /// </summary>
    public static class MonitorConfig
    {
        public static string OutputFolder = "MonitorOutput";
        public static readonly string ConfigFolderPath = "Assets/Editor/Monitor/Config";
        public static readonly string WhiteListName = "monitor_app_white_list";
        public static readonly string WhiteListPath = $"{ConfigFolderPath}/{WhiteListName}.txt";
        public static readonly string MonoOnlyAddFilterName = "monitor_mono_only_add";
        public static readonly string MonoOnlyAddFilterPath = $"{ConfigFolderPath}/{MonoOnlyAddFilterName}.txt";
        public static readonly string MonoNotAddFilterName = "monitor_mono_not_add";
        public static readonly string MonoNotAddFilterPath = $"{ConfigFolderPath}/{MonoNotAddFilterName}.txt";

        public static readonly string MonoNotGetTraceFilterName = "monitor_mono_get_trace";
        public static readonly string MonoNotGetTraceFilterPath = $"{ConfigFolderPath}/{MonoNotGetTraceFilterName}.txt";

        //无效监控节点
        public static readonly IMonitorNode InvalidNode = new InvalidNode();

        /// <summary>
        /// 创建监控字典
        /// </summary>
        public static readonly Dictionary<MonitorType, IMonitorNode> MonitorCreateDic = new Dictionary<MonitorType, IMonitorNode>()
        {
            {MonitorType.CSEventEngine, new CSEventEngineNode()},
            {MonitorType.CSAsset, new CSAssetNode()},
            {MonitorType.CSTimerAxis, new CSTimerAxisNode()},
            {MonitorType.CSUnityPool, new CSUnityPoolNode()},
            {MonitorType.PoolableItemQueue, new PoolableItemPoolQueueCountNode()},
            {MonitorType.PoolableItemNew, new PoolableItemPoolNewCountNode()},
            {MonitorType.PoolableItemIncrease, new PoolableItemPoolIncreaseCountNode()},
            {MonitorType.PoolableSimpleQueue, new PoolableObjectItemPoolQueueCountNode()},
            {MonitorType.PoolableSimpleNew, new PoolableObjectItemPoolNewCountNode()},
            {MonitorType.PoolableSimpleIncrease, new PoolableObjectItemPoolIncreaseNode()},
            {MonitorType.CoroutineTickTask, new CoroutineTickTaskNode()},
            {MonitorType.CoroutineRunnableTask, new CoroutineRunnableTaskNode()},
            {MonitorType.CoroutineSimpleTask, new CoroutineSimpleTaskNode()},
            {MonitorType.CSFrameUpdateManager, new CSFrameUpdateManagerNode()},
            {MonitorType.CSNetCallCount, new CSNetCallCountNode()},
            {MonitorType.CSNetModuleNode, new CSNetModuleNode()},
            {MonitorType.CSLOPObject, new CSLOPObjectNode()},
            //{MonitorType.Mono, new MonoNode()},
        };

        /// <summary>
        /// 获取监控节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IMonitorNode GetMonitorNode(MonitorType type)
        {
            if (MonitorCreateDic.ContainsKey(type))
            {
                return MonitorCreateDic[type];
            }
            return InvalidNode;
        }
    }

    /// <summary>
    /// 监控目标类型
    /// </summary>
    public enum MonitorType
    {
        CSEventEngine = 1 << 1,
        CSTimerAxis = 1 << 2,
        CSAsset = 1 << 3,
        CSUnityPool = 1 << 4,

        PoolableItemQueue = 1 << 9,
        PoolableItemNew = 1 << 10,
        PoolableItemIncrease = 1 << 11,

        PoolableSimpleQueue = 1 << 12,
        PoolableSimpleNew = 1 << 13,
        PoolableSimpleIncrease = 1 << 14,

        CoroutineTickTask = 1 << 21,
        CoroutineRunnableTask = 1 << 22,
        CoroutineSimpleTask = 1 << 23,

        CSFrameUpdateManager = 1 << 25,

        CSNetCallCount = 1 << 26,
        CSNetModuleNode= 1 << 27,

        CSLOPObject = 1 << 30,

        //Mono = 1 << 31,
    }
}
