using System.Collections.Generic;
using XClient.Entity;
using XGame;

namespace XClient.Common
{
    //代理模块的接口
    public interface IAgentModule : IModule
    {
        //创建一个agent
        bool CreateAgent(long agentID, int playerIdx);

        //销毁一个agent
        void DestroyAgent(long agentID);

        //获取一个Agent
        IAgent GetAgent(long agentID);

        /// <summary>
        /// 玩家自己的代理
        /// </summary>
        Agent roleAgnent { get; }

        //销毁一个agent
        void DestroyAllAgents();

        //客户端主动请求初始化数据，设置agent属性
        //初始化 long Prop()
        void SetBatchLongProp(long agentID,Dictionary<int,long> dicProp);

        /// <summary>
        /// 获取最小的Agent
        /// </summary>
        /// <returns></returns>
        IAgent GetMinIndexAgent();

        //获取当前代理数
        int GetAgentCount();
    }
}

