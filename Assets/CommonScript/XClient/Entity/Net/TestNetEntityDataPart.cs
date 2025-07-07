
using XClient.Network;

namespace XClient.Entity.Net
{
    public class TestNetEntityDataPart : NetDataPart
    {
        public NetVarLong testValue;
        public NetVarVector3 testVec3Value;

        protected override void OnSetupVars()
        {
            IsDebug = true;
            testValue = SetupVar<NetVarLong>("testValue");
            testVec3Value = SetupVar<NetVarVector3>("testVec3Value");
        }
    }
}
