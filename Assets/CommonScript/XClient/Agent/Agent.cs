using gamepol;
using XClient.Common;
using XClient.Entity;
using XGame.Entity;

namespace XGame
{
    public class Agent : IAgent
    {
        //roleid
        private long m_roleID;

        //  玩家序号. (进入房间的序号)
        private int m_iPlayerIdx;

        //属性回调
        private IAgentSink m_sink;

        //资源版本号
        private long m_propVer = 0;

        //属性列表
        private long[] arrVal = new long[gamepol.TCONST.PROP_NUM_IN_BATCH];   //val

        public AgentEntity entity { get; private set; }

        public bool isRoleAgent => m_roleID == GameGlobal.Room.GetLocalRoleID();

        public bool Create(long id, int playerIdx)
        {
            m_roleID = id;
            m_iPlayerIdx = playerIdx;

            //创建Agent实体
            var agentID = m_roleID << 32;
            entity = GameGlobal.EntityWorld.Role.CreateEntity(EntityType.Agent, (ulong)agentID, m_iPlayerIdx, null) as AgentEntity;

            return true;
        }

        public long[] GetAllLongProp()
        {
            return arrVal;
        }

        public long GetProp(int id)
        {
            if (id < 0 || id > gamepol.TCONST.PROP_NUM_IN_BATCH)
                return 0;

            return arrVal[id];
        }

        public long GetPropVersion()
        {
            return m_propVer;
        }

        public long GetRoleID()
        {
            return m_roleID;
        }

        public void SetBatchProp( TPropertySet stProperty, long ver)
        {
           // int id = 0;
           // long value = 0;

           // int iNum = stProperty.get_iNum();
           // int[] arrID = stProperty.get_arrID();
           // long[] arrVal = stProperty.get_arrVal();
           //for (int i=0;i<iNum;++i)
           // {
           //     id = arrID[i];
           //     value = arrVal[i];
           //     SetProp(id,value);
           //  }
        }

        public void Release()
        {
            //销毁实体对象
            if(entity != null)
            {
                GameGlobal.EntityWorld.Role?.DestroyEntity(entity);
                entity = null;
            }

            m_sink = null;
        }

        public void SetProp(int id, long value)
        {
            if (id < 0 || id > gamepol.TCONST.PROP_NUM_IN_BATCH)
                return ;

             arrVal[id] = value;

            m_sink?.OnPropIntChange(id, value);
        }

        public void SetSink(IAgentSink sink)
        {
            m_sink = sink;
        }

        public int GetPlayerIndex()
        {
            return m_iPlayerIdx;
        }

    }

}


