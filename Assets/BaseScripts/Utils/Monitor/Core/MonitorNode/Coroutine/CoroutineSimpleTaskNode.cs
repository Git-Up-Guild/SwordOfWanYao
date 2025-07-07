using XGame.CoroutinePool;
using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 协程池监控
    /// </summary>
    public class CoroutineSimpleTaskNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            var poolMgr = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            var poolDic = poolMgr.DicPools;
            var type = typeof(ISimpleTaskExecutor);
            int count = 0;
            foreach (var item in poolDic)
            {
                if (item.Key.IsSubclassOf(type))
                {
                    var usedPool = poolDic[type].UsedObjs;
                    count += usedPool.Count;
                    break;
                }
            }
            reuslt.Add(new MonitorDataUnit("SimpleTask", count));

            return new MonitorNodeData(reuslt);
        }

    }
}
