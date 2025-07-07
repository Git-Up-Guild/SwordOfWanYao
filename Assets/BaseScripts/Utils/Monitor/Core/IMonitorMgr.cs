using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    public interface IMonitorMgr 
    {
        void AddMonitor(MonitorType monitorType);
        void RemoveMonitor(MonitorType monitorType);
    }
}
