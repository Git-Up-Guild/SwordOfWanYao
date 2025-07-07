using XGame.CoroutinePool;
using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 协程池监控
    /// </summary>
    abstract public class CoroutineBaseExcutorNodeBase<TKey,TValue> : MonitorNodeBase where TKey : BaseExecutor<TValue>
    {
        public override MonitorNodeData SnapShot()
        {
            var poolMgr = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            var poolDic = poolMgr.DicPools;
            var type = typeof(TKey);
            if (poolDic.ContainsKey(type))
            {
                var usedPool = poolDic[type].UsedObjs;
                int i = 0;
                foreach (var item in usedPool)
                {
                    ++i;
                    var tickTask = item as TKey;
                    reuslt.Add(new MonitorDataUnit($"{tickTask.Desc}_Asyn", tickTask.AsyncTaskCount));
                    reuslt.Add(new MonitorDataUnit($"{tickTask.Desc}_Exe", tickTask.ExecutingCount));
                }
            }

            return new MonitorNodeData(reuslt);
        }

    }
}
