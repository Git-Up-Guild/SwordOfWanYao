using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UGUI2PSD
{
    public static class PrefabLayerFactory
    {
        public static PrefabLayerBase CreateLayer(EPrefabLayerType ePrefabLayerType, Transform tran)
        {
            PrefabLayerBase result = null;
            switch (ePrefabLayerType)
            {
                case EPrefabLayerType.GroupStart:
                    result = new GroupStartLayer(tran);
                    break;
                case EPrefabLayerType.Image:
                    result = new ImageLayer(tran);
                    break;
                case EPrefabLayerType.Text:
                    result = new TextLayer(tran);
                    break;
                case EPrefabLayerType.GroupEnd:
                    result = new GroupEndLayer(tran);
                    break;
                case EPrefabLayerType.Extend:
                    result = new ExtendLayer(tran);
                    break;
                default:
                    Debug.LogError("创建layer类型错误：" + ePrefabLayerType);
                    break;
            }
            return result;
        }
    }
}
