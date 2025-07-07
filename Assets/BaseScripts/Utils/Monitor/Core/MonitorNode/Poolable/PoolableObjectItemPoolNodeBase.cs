
using XGame.Poolable;
using XClient.Common;
using System;
using System.Collections.Generic;
using XGame.Utils;

namespace XClient.Scripts.Monitor
{
    public class PoolableObjectItemMonitorNodeDataDetail : MonitorDataUnitDetail
    {
        public int MaxAllocID { get; set; }
        public Dictionary<int, string> Data = new Dictionary<int, string>();

        /// <summary>
        /// 获得详情
        /// </summary>
        /// <returns></returns>
        public override string GetDetailString()
        {
            Temps.tempStringBuilder.Clear();

            foreach(var item in Data)
            {
                Temps.tempStringBuilder.Append($"{item.Key}=>\n{item.Value}\n");
            }

            return Temps.tempStringBuilder.ToString();
        }

        public override string ToString()
        {
            return GetDetailString();
        }

        /// <summary>
        /// 获取差异
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override MonitorDataUnitDetail GetDiff(MonitorDataUnitDetail o)
        {
            var other = o as PoolableObjectItemMonitorNodeDataDetail;
            var diffData = new PoolableObjectItemMonitorNodeDataDetail();

            diffData.MaxAllocID = Math.Max(other.MaxAllocID, MaxAllocID);

            var minAllocID = Math.Min(other.MaxAllocID, MaxAllocID);

            var temp = minAllocID == MaxAllocID ? other : this;
           
            foreach(var item in temp.Data)
            {
                if(item.Key > minAllocID)
                    diffData.Data.Add(item.Key, item.Value);
            }

            return diffData;
        }
    }

    /// <summary>
    /// 对象池监控
    /// </summary>
    abstract public class PoolableObjectItemPoolNodeBase : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            var poolManager = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            
            //处理简单对象池
            var poolDic = poolManager.DicSimplePools;
            int temCount;
            foreach (var item in poolDic)
            {
                var pool = item.Value;

                temCount = pool.UsedObjs.Count;

                if (temCount > 0)
                {
                    var dataUnit = new MonitorDataUnit(item.Key.ToString(), temCount);
                    BuildDataUnitDetail(pool, dataUnit);
                    reuslt.Add(dataUnit);
                }
            }

            return new MonitorNodeData(reuslt);
        }

        abstract public int GetCount(ObjectItemPool pool);

        protected virtual void BuildDataUnitDetail(ObjectItemPool pool, MonitorDataUnit dataUnit) { }
    }

}
