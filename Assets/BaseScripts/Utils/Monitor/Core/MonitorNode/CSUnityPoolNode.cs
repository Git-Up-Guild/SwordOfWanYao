//using XGame.Asset.Loader;
using XGame.UnityObjPool;
using XClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 对象池监控
    /// </summary>
    public class CSUnityPoolNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            IUnityObjectPool objPool = GameGlobal.Instance.UnityObjectPool;
            Dictionary<Type, Dictionary<string, uint>> data = new Dictionary<Type, Dictionary<string, uint>>();
            objPool.Output(data);
            List<MonitorDataUnit> reuslt = new List<MonitorDataUnit>();
            Dictionary<string, uint> pool;
            foreach (var item in data)
            {
                pool = item.Value;
                foreach (var poolItem in pool)
                {
                    reuslt.Add(new MonitorDataUnit(string.Format("【key:{0,-40}】 【pool:{1,-20}】", item.Key, poolItem.Key), (int)poolItem.Value));
                }
            }

            return new MonitorNodeData(reuslt);
        }
    }
}
