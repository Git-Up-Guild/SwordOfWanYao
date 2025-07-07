using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Scripts.Monitor
{
    public class MonoNode : IMonitorNode
    {
        static public MonitorNodeData GetSnapShot()
        {
            List<MonitorDataUnit> result = new List<MonitorDataUnit>();
#if UNITY_EDITOR && !UNITY_ANDROID1
            MonitorMono.Q1Game_Monitor_AllData allData = new MonitorMono.Q1Game_Monitor_AllData();
            if (!MonitorMono.GetAllData(ref allData))
            {
                result.Add(new MonitorDataUnit("【获取mono数据失败】", 0));
                return new MonitorNodeData(result);
            }
            Dictionary<string, int> datas = new Dictionary<string, int>();
            Dictionary<string, ulong> dataSize = new Dictionary<string, ulong>();
            int count = allData.count;
            bool isCancel = false;
            for (int i = 0; i < count; i++)
            {
                var data = allData.data[i];
                if (data.strName == string.Empty)
                    continue;

#if UNITY_EDITOR
                if (i % 1000 == 0)
                {
                    isCancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar($"输出快照数据 {i}/{count}", $"{data.strName}", 1f * i / count);
                    if (isCancel)
                    {
                        UnityEditor.EditorUtility.ClearProgressBar();
                        break;
                    }
                }
#endif
                // csv表一个单元格要分行，前后加个双引号就可以了，换行用'\n'
                string key = $"{data.strName},{data.size},\"{allData.data[i].strTrace}\"";
                if (datas.ContainsKey(key))
                    ++datas[key];
                else
                {
                    datas.Add(key, 1);
                    dataSize.Add(key, data.size);
                }
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            foreach (var item in datas)
            {
                result.Add(new MonitorDataUnit(item.Key, item.Value, (int)dataSize[item.Key]));
            }
#endif
            return new MonitorNodeData(result);
        }

        public MonitorNodeData SnapShot()
        {
            return GetSnapShot();
        }
    }
}
