using XGame.Asset;
using System.Collections.Generic;
using XGame;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 资源监控
    /// </summary>
    public class CSAssetNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            IGAssetLoader assetLoader = XGameComs.Get<IGAssetLoader>();
            Dictionary<string, int> data = new Dictionary<string, int>();
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            string[] info = assetLoader.GetLoadedInfo();
            string name;
            for (int i = 0; i < info.Length; i++)
            {
                name = info[i];
                if (data.ContainsKey(name))
                {
                    ++data[name];
                }
                else
                {
                    data.Add(name, 1);
                }
            }
            foreach (var item in data)
            {
                reuslt.Add(new MonitorDataUnit(item.Key, item.Value));
            }

            return new MonitorNodeData(reuslt);
        }
    }
}
