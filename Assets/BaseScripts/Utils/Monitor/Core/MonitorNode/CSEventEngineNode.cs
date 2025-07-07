using XGame.EventEngine;
using XClient.Common;
using System.Collections.Generic;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// C#的事件监控
    /// </summary>
    public class CSEventEngineNode : MonitorNodeBase
    {
        private Dictionary<string, int> recordDic = new Dictionary<string, int>();
        override public MonitorNodeData SnapShot()
        {
            IEventEngine eventEngine = GameGlobal.Instance.EventEngine;
            MonitorNodeData result = new MonitorNodeData();
            AppendEventInfo(ref result, eventEngine.GetExecuteEventInfo(), "Excute");
            AppendEventInfo(ref result, eventEngine.GetVoteEventInfo(), "Vote");
            return (result);
        }

        private void AppendEventInfo(ref MonitorNodeData target, Dictionary<ulong, EventNode> info, string src)
        {
            EventNode tempList;
            recordDic.Clear();
            foreach (var eventItem in info)
            {
                tempList = eventItem.Value;
                if (tempList == null) continue;
                ulong eventKey = eventItem.Key;
                ulong srcID = (eventKey) >> 32;
                ulong eventID = ((eventKey) & 0xFFFF0000) >> 16;
                ulong srcType = ((eventKey) & 0xFFFF);
                for (int i = 0; i < tempList.lstInfos.Count; i++)
                {
                    if (!tempList.lstInfos[i].m_bRemoveFlag)
                    {
                        //未删除
                        string key = string.Format("{0}: eventID:{1,-10} srcType:{2,-4} desc:{3}", src, eventID, srcType, tempList.lstInfos[i].m_desc);
                        if (!recordDic.ContainsKey(key))
                        {
                            recordDic.Add(key, 1);
                        }
                        else
                        {
                            ++recordDic[key];
                        }
                    }
                }
            }
            foreach (var item in recordDic)
            {
                target.Add(new MonitorDataUnit(item.Key, item.Value));
            }
        }
    }
}
