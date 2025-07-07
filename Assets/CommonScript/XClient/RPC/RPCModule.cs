/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: DataDispatcherModule.cs 
Module: DataDispatcher
Author: 曾嘉喜
Date: 2024.12.25
Description: 远程调用模块
***************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.Ini;
using XGame.Utils;

namespace XClient.RPC
{
    public class RPCModule : IRPCModule
    {
        public static IDebugEx debug = DebugEx.GetInstance("RPC");
        #region 生成
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        private RPCModuleMessageHandler m_MessageHandler;

        private static List<string> _tempParams = new List<string>();

        public RPCModule()
        {
			ModuleName = "DataDispatcher";
            ID = DModuleID.MODULE_ID_RPC;
        }

        public bool Create(object context, object config = null)
        {
            m_MessageHandler = new RPCModuleMessageHandler();
            m_MessageHandler.Create(this);
			
            Progress = 1f;
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
            m_MessageHandler?.Release();
            m_MessageHandler = null;
        }

        public bool Start()
        {
			m_MessageHandler?.Start();
		 
            State = ModuleState.Success;
            return true;
        }

        public void Stop()
        {
			m_MessageHandler?.Stop();
            registers.Clear();
        }

        public void Update()
        {
        }
		
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param)
        {

        }
        public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }
        
        #endregion

        #region 协议相关
        private Dictionary<string, OnRecieveRpcCallback> registers = new Dictionary<string, OnRecieveRpcCallback>();

        public void Register(string callName, OnRecieveRpcCallback registerCb)
        {
            if(registers.ContainsKey(callName))
            {
                debug.Error($"重复注册，Name={callName}");
                return;
            }

            registers.Add(callName.ToString(), registerCb);
        }

        public void Deregister(string callName, OnRecieveRpcCallback registerCb)
        {
            if(registers.ContainsKey(callName))
                registers.Remove(callName.ToString());
        }

        public void OnGetNetData(string funcName, string jsonData)
        {
            if (GameIni.Instance.enableDebug)
                debug.Log($"Name: {funcName},  Data: {jsonData}");

            if (registers.ContainsKey(funcName))
            {
                registers[funcName](jsonData);
            }
            else
            {
                debug.Error("接受到消息但无此消息回调，请检查！！！！" + funcName);
            }
        }

        public void OnRoleDataReady()
        {
        }

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="lstParams"></param>
        public void CallServer(string callName, List<string> lstParams)
        {
            var msgInst = GameGlobal.NetModule.GetAndInitGameMsg(true, gamepol.TCSMessage.MSG_ACTION_LUA_REQUEST_REQ);
            var message = msgInst.stTMSG_ACTION_LUA_REQUEST_REQ;
            message.set_szFuncName(callName);

            message.set_u8ParamCount((byte)lstParams.Count);

            var arrParams = message.set_arrParams();
       
            for(var i = 0; i < lstParams.Count; i++)
                arrParams[i].set_szVal(lstParams[i]);

            if(debug.isDebug)
            {
                Temps.tempStringBuilder.Clear();
                Temps.tempStringBuilder.Append($"Name：{callName}, Params：");
                foreach(var p in lstParams)
                    Temps.tempStringBuilder.Append(p).Append(",");

                debug.Log(Temps.tempStringBuilder.ToString());
                Temps.tempStringBuilder.Clear();
            }

            GameGlobal.Instance.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msgInst);
        }

        public void CallServer(string callName, params string[] ps)
        {
            _tempParams.Clear();
            _tempParams.AddRange(ps);
            CallServer(callName, _tempParams);
        }

        public void CallServer(string callName)
        {
            _tempParams.Clear();
            CallServer(callName, _tempParams);
        }

        public void CallServer(string callName, string p0)
        {
            _tempParams.Clear();
            _tempParams.Add(p0);
            CallServer(callName, _tempParams);
        }

        public void CallServer(string callName, string p0, string p1)
        {
            _tempParams.Clear();
            _tempParams.Add(p0);
            _tempParams.Add(p1);
            CallServer(callName, _tempParams);
        }

        public void CallServer(string callName, string p0, string p1, string p2)
        {
            _tempParams.Clear();
            _tempParams.Add(p0);
            _tempParams.Add(p1);
            _tempParams.Add(p2);
            CallServer(callName, _tempParams);
        }

        public void CallServer(string callName, string p0, string p1, string p2, string p3)
        {
            _tempParams.Clear();
            _tempParams.Add(p0);
            _tempParams.Add(p1);
            _tempParams.Add(p2);
            _tempParams.Add(p3);
            CallServer(callName, _tempParams);
        }

        #endregion
    }
}
