using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UGUI2PSD
{
    public class GroupStartLayer : PrefabLayerBase
    {
        public override EPrefabLayerType LayerType => EPrefabLayerType.GroupStart;

        public GroupStartLayer(Transform tran) : base(tran)
        {
            ResolveLayerInfo();
        }

        public override void ResolveLayerInfo()
        {
        }
    }
}
