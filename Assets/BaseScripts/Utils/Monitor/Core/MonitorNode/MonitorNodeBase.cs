using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 详细数据
    /// </summary>
    public class MonitorDataUnitDetail
    {
        /// <summary>
        /// 对比的数量1（在作为对比结果时才会设置）
        /// </summary>
        public int CompareNumFrom { get; set; }

        /// <summary>
        /// 对比的数量2（在作为对比结果时才会设置）
        /// </summary>
        public int CompareNumTo { get; set; }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <returns></returns>
        public virtual string GetDetailString()
        {
            return ToString();
        }

        /// <summary>
        /// 获取对比数据
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual MonitorDataUnitDetail GetDiff(MonitorDataUnitDetail other)
        {
            return null;
        }
    }

    abstract public class MonitorNodeBase : IMonitorNode
    {
        /// <summary>
        /// 拍快照
        /// </summary>
        /// <returns></returns>
        abstract public MonitorNodeData SnapShot();
    }
}
