using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 监控数据单元，最小监控数据单位
    /// </summary>
    public class MonitorDataUnit
    {
        public string Name { get; private set; }

        public int Count { get; private set; }

        public int Weight { get; private set; }

        /// <summary>
        /// 详情数据。具体用途由监控器来定义
        /// </summary>
        public MonitorDataUnitDetail Detail { get; set; }

        public MonitorDataUnit(string name, int count, int weight = 1)
        {
            Name = name;
            Count = count;
            Weight = weight;
        }
    }

}
