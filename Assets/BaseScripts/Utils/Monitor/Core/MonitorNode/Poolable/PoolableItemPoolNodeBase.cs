
using XGame.Poolable;
using XClient.Common;
using System;
using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 对象池监控
    /// </summary>
    abstract public class PoolableItemPoolNodeBase : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            var pool = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            var poolDic = pool.DicPools;
            int temCount;
            foreach (var item in poolDic)
            {
                temCount = item.Value.UsedObjs.Count;
                if (temCount > 0)
                    reuslt.Add(new MonitorDataUnit(item.Key.ToString(), temCount));
                
            }

            return new MonitorNodeData(reuslt);
        }

        abstract public int GetCount(PoolableItemPool pool);
    }
}
