using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XGame.Attr;
using static XClient.Network.NetObjectBehaviourTest;

namespace XClient.Network
{
    public class NetObjectBehaviourTest : NetObjectBehaviour<TestData>
    {
        public class TestData : MonoNetObject
        {
            public NetVarLong hp;

            protected override void OnSetupVars()
            {
                hp = SetupVar<NetVarLong>();
            }
        }


        [Label("值")]
        public long m_hp;

        [Label("设置值")]
        public bool isChangeValue;

        [Label("增量")]
        public long remoteHpValueDelta;

        [Label("设置增量")]
        public bool isChangeRemoteValue;

        private void Awake()
        {
        }

        protected override void OnUpdate()
        {
            if (isChangeValue)
            {
                if(IsOwner)
                    NetObj.hp.Value = m_hp;
                isChangeValue = false;
            }
            else
            {
                m_hp = NetObj.hp.Value;
            }

            if(isChangeRemoteValue)
            {
                if(!IsOwner)
                    NetObj.hp.RemoteValueDelta = remoteHpValueDelta;

                isChangeRemoteValue = false;
            }
        }
    }
}
