
using XGame.Timer;
using XClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// C#的计时器
    /// </summary>
    public class CSTimerAxisNode : MonitorNodeBase
    {
        override public MonitorNodeData SnapShot()
        {
            MonitorNodeData result = new MonitorNodeData();
            ITimerManager timerMgr = GameGlobal.Instance.TimerManager;
            var info = timerMgr.GetTimerInfo();
            TimerCollection temTimeList;
            Dictionary<string, int> timerCount = new Dictionary<string, int>();

            foreach (var item in info)
            {
                temTimeList = item.Value;

                if (temTimeList == null)
                    continue;

                for (int k = 0; k < temTimeList.Count; k++)
                {
                    if (!temTimeList[k].bDeleteFlag)
                    {
                        var name = string.Format("Timer ID:{0,-10} desc:{1,-40}", temTimeList[k].timerId, temTimeList[k].debugInfo);
                        if (!timerCount.ContainsKey(name))
                        {
                            timerCount.Add(name, 1);
                        }
                        else
                        {
                            ++timerCount[name];
                        }
                    }
                }
            }

            foreach (var item in timerCount)
            {
                result.Add(new MonitorDataUnit(item.Key, item.Value));
            }

            return (result);
        }

    }
}
