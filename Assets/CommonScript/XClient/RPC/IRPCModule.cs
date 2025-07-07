/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: IDataDispatcherModule.cs 
Module: DataDispatcher
Author: 曾嘉喜
Date: 2024.12.25
Description: 远程调用模块
***************************************************************************/

using System;
using System.Collections.Generic;
using XClient.Common;

namespace XClient.RPC
{
    /// <summary>
    /// 远程调用回调
    /// </summary>
    /// <param name="data"></param>
    public delegate void OnRecieveRpcCallback(string data);

    public interface IRPCModule : IModule
    {
        /// <summary>
        /// 注册协议方法，只建议在messageHandler里面处理
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="registerCb"></param>
        public void Register(string callName, OnRecieveRpcCallback registerCb);

        /// <summary>
        /// 注销协议方法，目前不太需要，Module走Stop会清理
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="registerCb"></param>
        public void Deregister(string callName, OnRecieveRpcCallback registerCb);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="lstParams"></param>
        public void CallServer(string callName, List<string> lstParams);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="ps"></param>
        public void CallServer(string callName, params string[] ps);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        public void CallServer(string callName);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="p0"></param>
        public void CallServer(string callName, string p0);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public void CallServer(string callName, string p0, string p1);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void CallServer(string callName, string p0, string p1, string p2);

        /// <summary>
        /// 调用服务器
        /// </summary>
        /// <param name="callName"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public void CallServer(string callName, string p0, string p1, string p2, string p3);
    }
}