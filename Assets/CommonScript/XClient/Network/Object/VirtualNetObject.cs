using gamepol;
using System.Collections.Generic;
using XGame.Poolable;

namespace XClient.Network
{
    public class VirtualNetObject : LitePoolableObject
    {
        public ulong NetID { get; set; }

        public List<NetVarValue> NetVarValues { get; private set; }

        public void SetNetID(ulong netID)
        {
            NetID = netID;
        }

        public void Update(TPropertySet propSet, bool isRemoteValueDelta)
        {
            var num = propSet.get_iNum();
            var arrID = propSet.get_arrID();
            var arrVals = propSet.get_arrVal();

            for(var i = 0; i < num; i++)
            {
                var netAbsVarIndex = arrID[i];
                var netAbsVarVal = arrVals[i];

                //先将位置占好
                if(netAbsVarIndex >= NetVarValues.Count)
                    AddNetVarNullElements(num - i);

                var netVarValue = NetVarValues[netAbsVarIndex];
                if(netVarValue == null)
                {
                    netVarValue = LitePoolableObject.Instantiate<NetVarValue>();
                    NetVarValues[netAbsVarIndex] = netVarValue;
                }

                NetVarUtility.TAbsValueToNetVarValue(netAbsVarVal, netVarValue, isRemoteValueDelta);
            }
        }

        private void AddNetVarNullElements(int count)
        {
            for (var i = 0; i < count; i++)
            {
                NetVarValues.Add(null);
            }
        }

        protected override void OnInit(object context = null)
        {
            if(NetVarValues == null)
                NetVarValues = new List<NetVarValue>();
        }

        protected override void OnRecycle()
        {
            foreach (var v in NetVarValues)
                LitePoolableObject.Recycle(v as LitePoolableObject);

            NetVarValues.Clear();
        }
    }
}
