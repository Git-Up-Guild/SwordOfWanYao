using gamepol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Network
{
    public static class NetworkUtility
    {
        static StringBuilder sb = new StringBuilder();
        static StringBuilder sb2 = new StringBuilder();
        public static string GetPropertySetDetail(TPropertySet propSet, INetable netable = null)
        {
            var count = propSet.get_iNum();
            var arrPropID = propSet.get_arrID();
            var arrPropValue = propSet.get_arrVal();

            sb.Clear();
            sb2.Clear();

            for (int i = 0; i < count; i++)
            {
                var id = arrPropID[i];
                var val = arrPropValue[i];

                var name = string.Empty;
                if (netable != null)
                {
                    var netVar = netable.NetVars[id];
                    if(!string.IsNullOrEmpty(netVar.Name))
                        name = "#" + netable.NetVars[id].Name;
                }

                if (i > 0)
                    sb.Append(",");

                if (val.has_iVal())
                    sb.Append($"{id}{name}:{val.get_iVal()}");
                else if (val.has_fVal())
                    sb.Append($"{id}{name}:{val.get_fVal()}");
                else if (val.has_sVal())
                    sb.Append($"{id}{name}:{val.get_sVal()}");
                else if (val.has_vec3())
                {
                    var vec = val.get_vec3();
                    sb.Append($"{id}{name}:({vec.get_x()}, {vec.get_y()}, {vec.get_z()})");
                }
                else if (val.has_iIntValNum())
                {
                    var num = val.get_iIntValNum();
                    var arrVals = val.get_arrIntVal();
                    sb2.Clear();
                    for (var k = 0; k < num; k++)
                    {
                        if (k < num - 1)
                            sb2.Append(arrVals[k]).Append(",");
                        else
                            sb2.Append(arrVals[k]);
                    }
                    sb.Append($"{id}{name}:[{sb2}]");
                }
                else if (val.has_iFloatValNum())
                {
                    var num = val.get_iFloatValNum();
                    var arrVals = val.get_arrFloatVal();
                    sb2.Clear();
                    for (var k = 0; k < num; k++)
                    {
                        if (k < num - 1)
                            sb2.Append(arrVals[k]).Append(",");
                        else
                            sb2.Append(arrVals[k]);
                    }
                    sb.Append($"{id}{name}:[{sb2}]");
                }
            }

            return sb.ToString();
        }

        public static void PrintPropertySet(TPropertySet propSet)
        {
            NetworkManager.Debug.Log(GetPropertySetDetail(propSet));
        }
    }
}
