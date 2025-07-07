using XClient.Common;
using XGame.Cam;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    public class RoleVisiblePart : VisiblePart
    {
        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            switch(id)
            {
                case EntityMessageID.ResLoaded:
                    {
                        if (CameraControllerManager.Instance.IsValid)
                            CameraControllerManager.Instance.SetTarget(partMaster.transform);
                    }
                    break;
                case EntityMessageID.ResUnloaded:
                    {
                        if (CameraControllerManager.Instance.IsValid)
                            CameraControllerManager.Instance.ClearTarget();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
