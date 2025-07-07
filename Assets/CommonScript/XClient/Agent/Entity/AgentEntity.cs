using UnityEngine;
using XClient.Common;
using XClient.Network;
using XGame.Entity;

namespace XClient.Entity
{
    public class AgentEntity : VisibleEntity
    {
        //数据部件
        public AgentDataPart data=>GetPart<AgentDataPart>();

        //是否为玩家自己的代理
        public bool isRoleAgent { get; private set; }

        protected override void OnInit(object context)
        {
            base.OnInit(context);
            NetID.Temp.Set(id);
            isRoleAgent = NetID.Temp.ClientID == (ulong)GameGlobal.Room.GetLocalRoleID();
        }

    public override string GetResPath()
        {
            return GameGlobal.Instance.GameInitConfig.playerInitData.playerPrefab.path;
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);

            data.position.OnChange.AddListener(OnPositionChange);
        }

        public override void UpdatePosition(Vector3 newPos)
        {
            data.position.Value = newPos;
        }

        private void OnPositionChange(Vector3 o, Vector3 n)
        {
            visiblePart?.SetPosition(n);
        }

        protected override bool IsSupportPart(PartInfo info)
        {
            if(info.type == EntityPartType.Move)
            {
                return isRoleAgent;
            }
            else
            {
                return true;
            }
        }
    }
}
