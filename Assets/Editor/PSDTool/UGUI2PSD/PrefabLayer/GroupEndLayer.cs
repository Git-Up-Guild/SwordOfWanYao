using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UGUI2PSD
{
    public class GroupEndLayer : PrefabLayerBase
    {
        public override EPrefabLayerType LayerType => EPrefabLayerType.GroupEnd;

        public GroupEndLayer(Transform tran) : base(tran)
        {
            LayerName += UGUI2PSDTool.GroupEndPostfix;
            ResolveLayerInfo();
        }

        public override void ResolveLayerInfo()
        {
        }
    }
}
