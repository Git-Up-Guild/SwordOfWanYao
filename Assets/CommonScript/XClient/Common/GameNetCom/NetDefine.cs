using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XClient.Common
{
    public partial class NetDefine
    {
        //非特定端
        public const byte ENDPOINT_NORMAL = 0;
        //Login 登录
        public const byte ENDPOINT_LOGIN = 1;
        //场景服
        public const byte ENDPOINT_ZONE = 2;
        //DB前端 网关
        public const byte ENDPOINT_DBFRONT = 3;
        //Gateway 网关
        public const byte ENDPOINT_GATEWAY = 4;
    }

    public enum EnNetConnFailType
    {
        ConnectError = 1,    //--套接字连接错误
        ManualDisconn = 2,   //--主动断开连接
        NetNotReachable = 3, //网络不可用
        ServerInvalid = 4,  //服务器地址无效
        ServerConnectFailed = 5, //服务器无法连接
        ServerLoginFailed = 6,  //登录服务器失败
    }
    public enum EnNetLoginFailType
    {
        Gateway_Fail = 1,    //--套接字连接错误
        Gateway_Error = 2,   //--主动断开连接
        HandleShake_Fail = 3, //网络不可用
        Login_Fail = 4, //服务器登录失败
    }

    public enum EnNetDisconnCode
    {
        Unkown = -1, //--未知错误
        ConnectError = 1, //连接错误
        ConnectLost = 2, //连接丢失
        Manual = 3, //手动断开连接
    }

    public enum EnNetManualDisconnReasonType
    {
        ToLogin,
        Reconnect,
        Unkown,
    }
}
