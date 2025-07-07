using XClient.Common;
using System.Collections.Generic;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// cs 的事件
    /// </summary>
    public class CSNetModuleNode : MonitorNodeBase
    {

        public override MonitorNodeData SnapShot()
        {
            
            Dictionary<string, int> data = GameGlobal.Instance.NetModule.GetNetNodeInfo(); 
            if(data!=null)
            {
                List<MonitorDataUnit> result = new List<MonitorDataUnit>();
                foreach (var item in data)
                {
                    result.Add(new MonitorDataUnit(item.Key, item.Value));
                }
                return new MonitorNodeData(result);
            }
            return MonitorNodeData.Invalid;
        }
    }

    public class CSNetCallCountNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            var luaEngine = GameGlobal.Instance.NetModule;

            Dictionary<string, uint> data = GameGlobal.Instance.NetModule.GetCallInfoCount(); 
            if (data != null)
            {
                List<MonitorDataUnit> result = new List<MonitorDataUnit>();
                foreach (var item in data)
                {
                    result.Add(new MonitorDataUnit(item.Key, (int)item.Value));
                }
                return new MonitorNodeData(result);
            }
            return MonitorNodeData.Invalid;
        }
    }
}
