using gamepol;
using minigame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGame.Poolable;

namespace XClient.Network
{
    public class VirtualNetObjectManager
    {
        private VirtualNetObjectManager() { }
        public static VirtualNetObjectManager Intance = new VirtualNetObjectManager();

        private Dictionary<ulong, VirtualNetObject> _objects = new Dictionary<ulong, VirtualNetObject>();

        public void CreateObj(ulong netID)
        {
            if (_objects.ContainsKey(netID))
                return;

            var obj = LitePoolableObject.Instantiate<VirtualNetObject>();
            obj.SetNetID(netID);
            _objects.Add(netID, obj);
        }

        public void UpdateObj(ulong netID, TPropertySet propSet, bool isRemoteValueDelta)
        {
            if(_objects.TryGetValue(netID, out VirtualNetObject obj))
            {
                obj.Update(propSet, isRemoteValueDelta);
            }
        }

        public void DestroyObj(ulong netID)
        {
            if (_objects.TryGetValue(netID, out VirtualNetObject obj))
            {
                LitePoolableObject.Recycle(obj);
                _objects.Remove(netID);
            }
        }

        public VirtualNetObject GetObj(ulong netID)
        {
            if (_objects.TryGetValue(netID, out VirtualNetObject obj))
            {
                return obj;
            }
            return null;
        }
    }
}
