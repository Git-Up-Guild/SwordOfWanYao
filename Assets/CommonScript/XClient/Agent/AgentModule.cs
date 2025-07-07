using gamepol;
using I18N.Common;
using System.Collections.Generic;
using UnityEngine;

using XClient.Common;
using XClient.Entity;
using XClient.Game;
using XClient.Network;
using XGame;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.Poolable;

namespace XClient.Client
{
    //代理实现模块,存放代理数据
    public class AgentModule : IAgentModule, OnMessageSink
    {
        public string ModuleName { get; set; }

        public ModuleState State { get; set; }

        public float Progress { get; set; }

        public int ID { get; set; }

        public Agent roleAgnent { get; private set; }

        //代理列表
        private Dictionary<long, IAgent> m_dicAgents = new Dictionary<long, IAgent>();

        public bool Create(object context, object config = null)
        {
            //注册实体类型
            GameGlobal.EntityWorld.RegisterEntityType<AgentEntity>(EntityType.Agent);
            GameGlobal.EntityWorld.RegisterEntityPartType<AgentDataPart>(EntityType.Agent, EntityPartType.Data);

            //激活控制功能
            if (GameGlobal.Instance.GameInitConfig&&GameGlobal.Instance.GameInitConfig.enablePlayerControl)
            {
                GameGlobal.EntityWorld.RegisterEntityPartType<AgentVisiblePart>(EntityType.Agent, EntityPartType.Visible);
                GameGlobal.EntityWorld.RegisterEntityPartType<MovePart>(EntityType.Agent, EntityPartType.Move);
            }

            return true;
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }

        public void Release()
        {
            roleAgnent = null;
            for (int i = 0; i < m_dicAgents.Count; i++)
            {
                m_dicAgents[i].Release();
            }
            m_dicAgents.Clear();
        }

        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param)
        {
            for (int i = 0; i < m_dicAgents.Count; i++)
            {
                m_dicAgents[i].Release();
            }
            m_dicAgents.Clear();
            roleAgnent = null;
        }

        public bool Start()
        {
            INetModule netModule = CGame.Instance.NetModule;
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_SET_PROP_RSP, this, "AgentModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_GET_PROP_RSP, this, "AgentModule:Start");
            netModule.AddMessageHandler(TCSMessage.MSG_MINIGAME_PROP_UPDATED_NTF, this, "AgentModule:Start");
            return true;
        }

        public void Stop()
        {
            INetModule netModule = CGame.Instance.NetModule;
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_SET_PROP_RSP, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_GET_PROP_RSP, this);
            netModule.RemoveMessageHandler(TCSMessage.MSG_MINIGAME_PROP_UPDATED_NTF, this);
            m_dicAgents.Clear();
        }

        public void Update()
        {
        }

        public bool CreateAgent(long agentID, int playerIdx)
        {
            if(m_dicAgents.ContainsKey(agentID))
            {
                Debug.LogError("重复创建 agent id= " + agentID);
                return false;
            }

            Agent agent = new Agent();
            agent.Create(agentID, playerIdx);
            m_dicAgents.Add(agentID, agent);

            //玩家自己的代理
            if (agent.isRoleAgent)
                roleAgnent = agent;

            //发送 agent 创建消息
            IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
            SAngentCreateDestroy data = poolManger.PopObjectItem<SAngentCreateDestroy>();
            data.agentID = agentID;
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_AGENT_CREATE, (byte)DEventSourceType.SOURCE_TYPE_AGENT, 0, data);
            poolManger.PushObjectItem(data);

            return true;
        }

        public void DestroyAgent(long agentID)
        {
            if (m_dicAgents.TryGetValue(agentID, out IAgent agent))
            {
                if (agent == roleAgnent)
                    roleAgnent = null;

                //发送agent 销毁消息
                IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
                SAngentCreateDestroy data = poolManger.PopObjectItem<SAngentCreateDestroy>();
                data.agentID = agentID;
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_AGENT_DESTROY, (byte)DEventSourceType.SOURCE_TYPE_AGENT, 0, data);
                poolManger.PushObjectItem(data);

                if (agent != null)
                {
                    agent.Release();
                }

                m_dicAgents.Remove(agentID);
            }

            return ;
        }

        //删除所有agent
        public void DestroyAllAgents()
        {
            long roleID = 0;
            foreach(IAgent agent in m_dicAgents.Values)
            {
                if (agent != null)
                {
                    roleID = agent.GetRoleID();
                    //发送agent 销毁消息
                    IItemPoolManager poolManger = XGame.XGameComs.Get<XGame.Poolable.IItemPoolManager>();
                    SAngentCreateDestroy data = poolManger.PopObjectItem<SAngentCreateDestroy>();
                    data.agentID = roleID;
                    GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_AGENT_DESTROY, (byte)DEventSourceType.SOURCE_TYPE_AGENT, 0, data);
                    poolManger.PushObjectItem(data);

                    agent.Release();
                }
            }

            m_dicAgents.Clear();

            roleAgnent = null;

            return;
        }

        public void OnMessage(TCSMessage msg)
        {
            uint msgID = msg.stHead.get_iMsgID();
            switch (msgID)
            {
                case TCSMessage.MSG_MINIGAME_SET_PROP_RSP:
                    {
                        TMSG_MINIGAME_SET_PROP_RSP context = msg.stTMSG_MINIGAME_SET_PROP_RSP;
                        __OnMSG_MINIGAME_SET_PROP_RSP(context);
                    }
                    break;

                case TCSMessage.MSG_MINIGAME_GET_PROP_RSP:
                    {
                        TMSG_MINIGAME_GET_PROP_RSP context = msg.stTMSG_MINIGAME_GET_PROP_RSP;
                        __OnMSG_MINIGAME_GET_PROP_RSP(context);
                    }
                    break;

                case TCSMessage.MSG_MINIGAME_PROP_UPDATED_NTF:
                    {
                        TMSG_MINIGAME_PROP_UPDATED_NTF context = msg.stTMSG_MINIGAME_PROP_UPDATED_NTF;
                        __OnMSG_MINIGAME_PROP_UPDATED_NTF(context);
                    }
                    break;

                default:
                    break;
            }
        }

        //属性设置回复
        private void __OnMSG_MINIGAME_SET_PROP_RSP(TMSG_MINIGAME_SET_PROP_RSP context)
        {
            int iError = context.get_iError();
            if(0!=iError)
            {
                Debug.LogError("__OnMSG_MINIGAME_SET_PROP_RSP error = "+ iError);
                
                return;
            }

            long i64RoleID = context.get_i64RoleID();
            IAgent agent = null;
            if (m_dicAgents.TryGetValue(i64RoleID, out agent))
            {
                TPropertySet stProperty = context.get_stProperty();
                agent.SetBatchProp(stProperty, context.get_iVer());
            }

        }

        private void __OnMSG_MINIGAME_GET_PROP_RSP(TMSG_MINIGAME_GET_PROP_RSP context)
        {
            int iError = context.get_iError();
            if (0 != iError)
            {
                Debug.LogError("__OnMSG_MINIGAME_GET_PROP_RSP error = " + iError);

                return;
            }
        }

        private void __OnMSG_MINIGAME_PROP_UPDATED_NTF(TMSG_MINIGAME_PROP_UPDATED_NTF context)
        {
            long i64RoleID = context.get_iOwnerRoleID();
            IAgent agent = null;
            if (m_dicAgents.TryGetValue(i64RoleID, out agent))
            {
                TPropertySet stProperty = context.get_stProperty();
                agent.SetBatchProp(stProperty,  context.get_iVer());
            }

        }

        //本地请求初始化属性
        public void SetBatchLongProp(long agentID, Dictionary<int, long> dicProp)
        {
            IAgent agent = null;
            if (m_dicAgents.TryGetValue(agentID, out agent))
            {
                int iNum = dicProp.Count;
                // 发送设置属性请求
                TCSMessage tcsMessage = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_MINIGAME_SET_PROP_REQ);
                tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_i64RoleID(agentID);
                tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_bCheckVer(0);
                tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_iType((int)EnPropType.enPT_Role);
                TPropertySet stProperty = tcsMessage.stTMSG_MINIGAME_SET_PROP_REQ.set_stProperty();
                stProperty.set_iNum(iNum);
                int[] arrID = stProperty.set_arrID();
                TAbsValue[] arrVal = stProperty.set_arrVal();

                int index = 0;
                foreach (var key in dicProp.Keys)
                {
                    arrID[index] = key;
                    arrVal[index].set_iVal(dicProp[key]);
                    ++index;
                }

                GameHelp.SendMessage_CS(tcsMessage);

                //本地先设置一次属性
                agent.SetBatchProp(stProperty, 0);
            }
        }

        //通过ID，获取某个agent
        public IAgent GetAgent(long agentID)
        {
            m_dicAgents.TryGetValue(agentID, out IAgent agent);
            return agent;
        }

        public IAgent GetMinIndexAgent()
        {
            if (m_dicAgents.Count <= 0)
            {
                return null;
            }
            
            long minKey = long.MaxValue;
            foreach (var key in m_dicAgents.Keys)
            {
                if (key < minKey)
                {
                    minKey = key;
                }
            }
            return m_dicAgents[minKey];
        }

        public int GetAgentCount()
        {
            if (m_dicAgents != null)
            {
                return m_dicAgents.Count;
            }

            return 0;
        }

        public void OnGameStateChange(int newStateID, int oldStateID)
        {
            if (newStateID == (int)GameState.Login)
            {
                DestroyAllAgents();
            }
        }

        public void OnRoleDataReady()
        {
        }
    }
}

