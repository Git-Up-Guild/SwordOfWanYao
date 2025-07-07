using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XClient.Scripts.Monitor;
using XGame.LOP;

namespace XClient.Scripts.Monitor
{
    public class CSLOPObjectNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            ILOPObjectManager lopMgr = LOPObjectManagerInstance.obj;
            List<string> data = new List<string>();
            lopMgr.Output(data);
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            reuslt.Add(new MonitorDataUnit("LOPItems", data.Count));
            foreach (var item in data)
            {
                reuslt.Add(new MonitorDataUnit(item,1));
            }

            return new MonitorNodeData(reuslt);
        }

    }
}
