
using XGame.Poolable;
using XClient.Common;
using System;
using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 对象池监控
    /// </summary>
    public class PoolableObjectItemPoolQueueCountNode : PoolableObjectItemPoolNodeBase
    {
        override public int GetCount(ObjectItemPool pool)
        {
            return pool.QueueCount;
        }
    }
}
