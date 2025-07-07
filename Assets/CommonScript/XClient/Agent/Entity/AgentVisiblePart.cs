using XGame.Cam;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    class AgentVisiblePart : VisiblePart
    {
        protected AgentEntity agentMaster => master as AgentEntity;

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            if(agentMaster.isRoleAgent)
            {
                switch (id)
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
}
