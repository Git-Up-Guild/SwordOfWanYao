
using XGame.Poolable;
using XClient.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 对象池监控
    /// </summary>
    public class PoolableObjectItemPoolNewCountNode : PoolableObjectItemPoolNodeBase
    {
        override public int GetCount(ObjectItemPool pool)
        {
            return pool.NewCount;
        }

        protected override void BuildDataUnitDetail(ObjectItemPool pool, MonitorDataUnit dataUnit)
        {
            //这种对象池特殊处理，记录当前所有的对象信息
            if (pool.ObjectType.IsSubclassOf(typeof(LitePoolableObject)))
            {
                //将缓存的和使用中的都记录下来
                var detail = new PoolableObjectItemMonitorNodeDataDetail();

                //记录下当前分配的最大ID，那么下进行对比的时候，可以根据这个ID来找到具体哪些对象是新增加的
                detail.MaxAllocID = LitePoolableObject.MAX_ALLOC_ID;

                foreach (var o in pool.CachedObjs)
                {
                    var litO = o as LitePoolableObject;

                    if (!detail.Data.ContainsKey(litO.AllocID))
                        detail.Data.Add(litO.AllocID, litO.StackTree);
                    else
                        Debug.LogError($"严重错误！对象池中缓存的对象重复！Type={litO.GetType().Name}, StackTree={litO.StackTree}");
                }

                foreach (var o in pool.UsedObjs)
                {
                    var litO = o as LitePoolableObject;
                    detail.Data.Add(litO.AllocID, litO.StackTree);
                }

                dataUnit.Detail = detail;
            }
        }
    }
}
