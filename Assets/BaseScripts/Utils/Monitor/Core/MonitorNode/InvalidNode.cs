

namespace XClient.Scripts.Monitor
{
    public class InvalidNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            return MonitorNodeData.Invalid;
        }
    }
}
