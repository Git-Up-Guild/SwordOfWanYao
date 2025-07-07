
using XGame.Poolable;
using XClient.Common;
using System;
using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 对象池监控，池子存有数量，和创建数量一样才正常
    /// </summary>
    public class PoolableItemPoolQueueCountNode : PoolableItemPoolNodeBase
    {
        override public int GetCount(PoolableItemPool pool)
        {
            return pool.QueueCount;
        }
    }
}
