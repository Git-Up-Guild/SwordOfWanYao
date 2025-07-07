using gamepol;

namespace XClient.Network
{
    public static class NetVarUtility
    {
        public static NetVarValue TAbsValueToNetVarValue(TAbsValue val, NetVarValue netVarValue, bool isRemoteValueDelta)
        {
            if (val.has_fVal())
            {
                if(isRemoteValueDelta)
                    netVarValue.fValue += val.get_fVal();
                else
                    netVarValue.fValue = val.get_fVal();
            }
            else if (val.has_iVal())
            {
                if (isRemoteValueDelta)
                    netVarValue.lValue += val.get_iVal();
                else
                    netVarValue.lValue = val.get_iVal();
            }
            else if (val.has_sVal())
            {
                netVarValue.sValue = val.get_sVal();
            }
            else if (val.has_vec3())
            {
                var vv = val.get_vec3();
                if (isRemoteValueDelta)
                    netVarValue.vec3Value += new UnityEngine.Vector3(vv.get_x(), vv.get_y(), vv.get_z());
                else
                    netVarValue.vec3Value = new UnityEngine.Vector3(vv.get_x(), vv.get_y(), vv.get_z());
            }

            return netVarValue;
        }
    }
}
