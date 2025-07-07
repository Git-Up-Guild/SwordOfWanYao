
using UnityEngine;
using XClient.Network;


namespace XClient.Entity
{
    public class AgentDataPart : NetDataPart
    {
        //血量
        public NetVarLong hp;

        //坐标
        public NetVarVector3 position;

        //金币数量
        public NetVarLong goldNum;

        //钻石数量
        public NetVarLong diamondNum;

        //钻石数量（充值获得）
        public NetVarLong diamondNum2;

        //玩家等级
        public NetVarInt level;

        //玩家名称
        public NetVarString name;

        protected override void OnSetupVars()
        {
            //IsDebug = true;
            hp = SetupVar<NetVarLong>();
            position = SetupVar<NetVarVector3>();
            goldNum = SetupVar<NetVarLong>();
            diamondNum = SetupVar<NetVarLong>();
            diamondNum2 = SetupVar<NetVarLong>();
            level = SetupVar<NetVarInt>();
            name = SetupVar<NetVarString>();
        }
    }
}
