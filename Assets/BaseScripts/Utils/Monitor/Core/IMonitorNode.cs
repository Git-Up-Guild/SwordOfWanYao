using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 监控节点，对应各个系统
    /// </summary>
    public interface IMonitorNode
    {
        /// <summary>
        /// 拍个快照
        /// </summary>
        /// <returns></returns>
        MonitorNodeData SnapShot();
    }
}
