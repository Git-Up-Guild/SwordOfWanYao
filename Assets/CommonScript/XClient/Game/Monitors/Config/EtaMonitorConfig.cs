using XGame.Attr;
using XGame.EtaMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Game
{
    public enum EtaMonitorType
    {
        //定时器
        Timer = 1,

        //事件服
        Event,

        //网络消息
        Net,

        //帧更新
        Frame,

        //组件
        XGameComs,
    }

    [Serializable]
    public class EtaMonitorConfig : MonitorConfig
    {
        [Header("监控器类型")]
        public EtaMonitorType monitorType;
    }
}
