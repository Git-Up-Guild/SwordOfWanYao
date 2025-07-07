using System;
using System.Collections.Generic;
using gamepol;
using UnityEngine;
using XGame;
using XGame.Poolable;

namespace XClient.Common.VirtualServer
{
    public class VirtualServer : IVirtualServer
    {
        //看是否开启这个
        private bool OPEN_VIRTUAL_SEREVER = true;

        List<IMessageHandler> messageHandlers = new List<IMessageHandler>();

        private void InitMessageHandlers()
        {
            //todo handlers只需要写这里！！
            //添加handle相关内容
            //RegMsgModule<GMVirMsgHandler>();
            //RegMsgModule<SeedingVirMsgHandler>();
            // RegMsgModule<ShowPrizeVirMsgHandler>();
        }

        protected CommonNetMessageRegister netMessageRegister { get; set; }
        private bool bEnable;
        public VirtualNet virtualNet;
        private readonly Dictionary<Type, IMessageHandler> msgModules = new Dictionary<Type, IMessageHandler>();

        public void Initialize()
        {
            bEnable = false;

            netMessageRegister = LitePoolableObject.Instantiate<CommonNetMessageRegister>();
            // gLuaEventServer.SubscribeAction(TCSMessage. EVENT_STATE_ENTER, null, ON_EVENT_STATE_ENTER);
        }

        public void Start()
        {
            if (OPEN_VIRTUAL_SEREVER)
            {
                Enable();
            }
        }

        public void Stop()
        {
            if (OPEN_VIRTUAL_SEREVER)
            {
                Disable();
            }
        }

        private void Enable()
        {
            if (bEnable) return;
            virtualNet = new VirtualNet();
            virtualNet.Start();
            var desc = "VirtualServer";
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_CREATE_ROLE_NTF, ON_EVENT_ROLE_CREATED, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_ENTITY_CREATE_ROLE_PART_NTF, ON_EVENT_ROLE_PART_CREATED, desc);

            bEnable = true;
        }

        private void Disable()
        {
            bEnable = false;
            foreach (var module in msgModules.Values)
            {
                module.OnVirtualServerStoped(virtualNet);
            }

            virtualNet.Stop();
        }

        public void ON_EVENT_ROLE_CREATED(TCSMessage msg)
        {
            InitMessageHandlers();

            // 原有的角色创建处理逻辑
            foreach (var module in msgModules.Values)
            {
                module.ON_EVENT_ROLE_CREATED(virtualNet);
            }
        }

        public void ON_EVENT_ROLE_PART_CREATED(TCSMessage msg)
        {
            // 原有的角色部件创建处理逻辑
            foreach (var module in msgModules.Values)
            {
                module.ON_EVENT_ROLE_PART_CREATED(virtualNet);
            }
        }

        public void RegMsgModule<T>() where T : IMessageHandler, new()
        {
            if (msgModules.ContainsKey(typeof(T))) return;

            IMessageHandler handler = CreateHandler<T>();
            if (handler != null)
            {
                msgModules[typeof(T)] = handler;
                if (bEnable)
                {
                    handler.OnVirtualServerStarted(virtualNet);
                }
            }
        }

        public void UnregMsgModule<T>() where T : IMessageHandler, new()
        {
            if (!msgModules.ContainsKey(typeof(T))) return;

            var handler = msgModules[typeof(T)];
            handler.OnVirtualServerStoped(virtualNet);
            msgModules.Remove(typeof(T));
        }

        private IMessageHandler CreateHandler<T>() where T : IMessageHandler, new()
        {
            return new T();
        }
    }
}