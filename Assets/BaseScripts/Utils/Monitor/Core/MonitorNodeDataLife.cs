using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 监控节点的生命线
    /// </summary>
    public class MonitorNodeDataLife
    {
        public MonitorType MonitorType { get; private set; }

        public int Count { get => _dataList.Count; }

        public MonitorNodeData this[int index] { get => _dataList[index]; }

        /// <summary>
        /// 监控节点，对应了各个系统
        /// </summary>
        private IMonitorNode _node;

        /// <summary>
        /// 拍的快照数据
        /// </summary>
        private List<MonitorNodeData> _dataList = new List<MonitorNodeData>();

        public MonitorNodeDataLife(MonitorType monitorType, IMonitorNode node)
        {
            _node = node;
            MonitorType = monitorType;
        }

        /// <summary>
        /// 添加一个节点数据
        /// </summary>
        /// <param name="node"></param>
        public void Add(MonitorNodeData node)
        {
            _dataList.Add(node);
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            _dataList.Clear();
        }

        /// <summary>
        /// 拍一次快照
        /// </summary>
        /// <param name="curTime">当前时间</param>
        /// <param name="index">快照索引</param>
        public void Snapshot(int index)
        {
            var data = _node.SnapShot();
            data.SetSnapshotIndex(index);
            Add(data);
        }

        /// <summary>
        /// 获取快照数据
        /// </summary>
        /// <param name="index">快照索引</param>
        /// <returns></returns>
        public MonitorNodeData GetSnapshotData(int index)
        {
            int count = _dataList.Count;
            for (int i = 0; i < count; i++)
            {
                if (_dataList[i].SnapshotIndex == index)
                {
                    return _dataList[i];
                }
            }
            return MonitorNodeData.Empty;
        }

        /// <summary>
        /// 获取两个节点间数据差异
        /// </summary>
        /// <param name="oldIndex">旧的索引</param>
        /// <param name="newIndex">新的索引</param>
        /// <param name="isOnlyIncrease">是否只获取增量数据</param>
        /// <returns></returns>
        public MonitorNodeData GetDifferent(int oldIndex, int newIndex, bool isOnlyIncrease = false)
        {
            MonitorNodeData oldData = GetSnapshotData(oldIndex);
            MonitorNodeData newData = GetSnapshotData(newIndex);

            if (oldData == null)
                oldData = MonitorNodeData.Empty;

            if (newData == null)
                newData = MonitorNodeData.Empty;

            //对比快照数据
            return GetDifferent(oldData, newData, isOnlyIncrease);
        }

        /// <summary>
        /// 获取两个节点间数据差异
        /// </summary>
        /// <param name="oldData">旧的数据</param>
        /// <param name="newData">新的数据</param>
        /// <param name="isOnlyIncrease">是否只获取增量数据</param>
        /// <returns></returns>
        public MonitorNodeData GetDifferent(MonitorNodeData oldData, MonitorNodeData newData, bool isOnlyIncrease = false)
        {
            List<MonitorDataUnit> dataList = new List<MonitorDataUnit>();
            HashSet<string> sameKey = new HashSet<string>();

            MonitorDataUnit oldDataUnit;
            
            foreach (var newItem in newData.Data)
            {
                //在旧数据中也存在的
                if (oldData.Contains(newItem.Key))
                {
                    oldDataUnit = oldData[newItem.Key];
                    sameKey.Add(newItem.Key);

                    int diff = newItem.Value.Count - oldDataUnit.Count;

                    // 对比所有，或者只显示增量时
                    if (!isOnlyIncrease || diff > 0)
                    {
                        var newDataUnit = newItem.Value;
                        var retData = new MonitorDataUnit(newItem.Key, newDataUnit.Count - oldDataUnit.Count);

                        if (newDataUnit.Detail != null)
                        {
                            retData.Detail = newDataUnit.Detail.GetDiff(oldDataUnit.Detail);
                            retData.Detail.CompareNumFrom = oldDataUnit.Count;
                            retData.Detail.CompareNumTo = newDataUnit.Count;
                        }

                        dataList.Add(retData);
                    }
                }
                else
                {
                    if (newItem.Value.Count > 0)
                    {
                        var retData = new MonitorDataUnit(newItem.Key, newItem.Value.Count);
                        retData.Detail = newItem.Value.Detail;

                        dataList.Add(retData);
                    }
                }
            }

            //不是只统计增量的时候，还需要将已经完全清除的也统计
            if (!isOnlyIncrease)
            {
                foreach (var item in oldData.Data)
                {
                    if (!sameKey.Contains(item.Key))
                    {
                        dataList.Add(new MonitorDataUnit(item.Key, -1 * item.Value.Count));
                    }
                }
            }

            return new MonitorNodeData(dataList);
        }

    }
}
