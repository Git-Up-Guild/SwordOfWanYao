/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：Login.cs 
Author：郑秀程
Date：2024.06.17
Description：登录模块消息处理器
***************************************************************************/

using cgpol;
using gamepol;
using UnityEngine;
using XClient.Common;
using XGame.Ini;
using XGame.Server;
using XGame.UI.Framework;
using XGame.UI.Framework.Box;

namespace XClient.Login
{
    public class LoginModuleMessageHandler : ModuleMessageHandler<LoginModule>
    {
        protected override void OnSetupHandlers()
        {
            var desc = GetType().Name;

            netMessageRegister.AddHandler(TCGMessage.MSG_GATEWAY_CONNECT_NTF, ON_MSG_GATEWAY_CONNECT_NTF, desc);
            netMessageRegister.AddHandler(TCGMessage.MSG_GATEWAY_KICK_CLIENT_NTF, ON_MSG_GATEWAY_KICK_CLIENT_NTF, desc);

            netMessageRegister.AddHandler(TCSMessage.MSG_LOGIN_LOGIN_RSP, ON_RECEIVE_MSG_LOGIN_LOGIN_RSP, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_LOGIN_HANDSHAKE_RSP, ON_RECEIVE_MSG_LOGIN_HANDSHAKE_RSP, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_LOGIN_KICKOUT_NTF, ON_RECEIVE_MSG_LOGIN_KICKOUT_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_LOBBY_ERROR_NTF, ON_RECEIVE_MSG_LOBBY_ERROR_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_LOGIN_ERROR_NTF, ON_RECEIVE_MSG_LOGIN_ERROR_NTF, desc);
            netMessageRegister.AddHandler(TCSMessage.MSG_LOGIN_SERVERMAINTAIN_NTF, ON_RECEIVE_MSG_LOGIN_SERVERMAINTAIN_NTF, desc);
        }

        private void ON_MSG_GATEWAY_CONNECT_NTF(TCGMessage msg)
        {
            var msgBody = msg.stTMSG_GATEWAY_CONNECT_NTF;
            var errorCode = msgBody.get_dwError();
            if(errorCode != 0)
            {
                Debug.LogError($"[Login] 连接网关失败！errorCode={errorCode}");
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_GATEWAY_FAIL, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            }
            else
            {
                Debug.Log("[Login] 连接网关成功！");
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_GATEWAY_SUC, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            }
        }
        private void ON_MSG_GATEWAY_KICK_CLIENT_NTF(TCGMessage msg)
        {
            var msgBody = msg.stTMSG_GATEWAY_KICK_CLIENT_NTF;
            uint reason = msgBody.get_dwReason();
            if (GameIni.Instance.enableDebug)
            {
                Debug.Log($"服务器踢人 原因{reason}");
            }
            MessageBoxShareData.Instance.Reset();
            //GameGlobalEx.NetModule.SetKick(true);
            MessageBoxShareData.Instance.title = "服务器踢人";
            MessageBoxShareData.Instance.content = $"原因:{reason} \n点击\"确定\"重连，\n\"取消\"返回登录界面";
            MessageBoxShareData.Instance.style = MessageBoxStyle.YesNo;
            MessageBoxShareData.Instance.callback = (isOK, data) =>
            {
                if (isOK)
                {
                    //GameGlobalEx.NetModule.SetKick(false);
                   // GameGlobalEx.NetModule.ReConnect();
                }
                else
                {
                    //GameGlobalEx.NetModule.SetKick(false);
                    //返回登录界面
                    GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_RETURN_LOGIN, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);

                }
            };



            MessageBoxManager.Instance.ShowBox(MessageBoxShareData.Instance);

        }

        private void ON_RECEIVE_MSG_LOGIN_LOGIN_RSP(TCSMessage msg) 
		{
			var msgBody = msg.stTMSG_LOGIN_LOGIN_RSP;
            var errorCode = msgBody.get_u32ErrCode();
            if(errorCode > 0)
            {
                Debug.LogError($"[Login] 登录服务器失败！(LOGIN_RSP), errorCode={errorCode}");
                //GameGlobalEx.NetModule.OnLoginFail(EnNetConnFailType.ServerLoginFailed, EnNetLoginFailType.Login_Fail, errorCode);
                if (errorCode != (int)EN_CLIENT_OP_ERROR_CODE.enOP_LoginSvr_Maintaining)
                    GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_FAIL, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);

              
                
            }
            else
            {
                //同步时间
                uint curTime = msgBody.get_u32CurTime();
                GameServer.Instance.UpdateServerTime(curTime);




                Debug.Log($"[Login] 登录服务器成功！");
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_SUC, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);

      
            }
        }

		private void ON_RECEIVE_MSG_LOGIN_HANDSHAKE_RSP(TCSMessage msg) 
		{
			var msgBody = msg.stTMSG_LOGIN_HANDSHAKE_RSP;
            var errorCode = msgBody.get_u32ErrCode();
            if (errorCode != 0)
            {
                Debug.LogError($"[Login] 与服务器握手失败！errorCode={errorCode}");
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_HANDSHAKE_FAIL, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);

            }
            else
            {
                Debug.Log("[Login] 与服务器握手成功！");
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_HANDSHAKE_SUC, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            }
		}

		private void ON_RECEIVE_MSG_LOGIN_KICKOUT_NTF(TCSMessage msg) 
		{
			var msgBody = msg.stTMSG_LOGIN_KICKOUT_NTF;
            int reason = msgBody.get_kickReason();
            if (GameIni.Instance.enableDebug)
            {
                Debug.Log($"服务器踢人 原因{reason}");
            }
            MessageBoxShareData.Instance.Reset();
            //GameGlobalEx.NetModule.SetKick(true);
            MessageBoxShareData.Instance.title = "服务器踢人";
            MessageBoxShareData.Instance.content = $"原因:{reason} \n点击\"确定\"重连，\n\"取消\"返回登录界面";
            MessageBoxShareData.Instance.style = MessageBoxStyle.YesNo;
            MessageBoxShareData.Instance.callback = (isOK, data) =>
            {
                if (isOK)
                {
                    //GameGlobalEx.NetModule.SetKick(false);
                   // GameGlobalEx.NetModule.ReConnect();
                }
                else
                {
                    //GameGlobalEx.NetModule.SetKick(false);
                    //返回登录界面
                    GameGlobal.EventEgnine.FireExecute(DGlobalEvent.EVENT_NET_RETURN_LOGIN, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);

                }
            };
           // GameGlobalEx.NetModule.Disconnect();
            MessageBoxManager.Instance.ShowBox(MessageBoxShareData.Instance);

        }

        //此消息为大厅收到第3方平台结果后回复，没错误不回
        private void ON_RECEIVE_MSG_LOBBY_ERROR_NTF(TCSMessage msg) 
		{
			var msgBody = msg.stTMSG_LOBBY_ERROR_NTF;
            var errorCode = msgBody.get_dwError();

           
          //  Debug.LogError($"[Login] 登录服务器失败！(LOBBY_NTF), errorCode={errorCode}");
          //  GameUtility.ShowSystemFlowText("登录服务器失败, 错误码" + errorCode);

            //GameGlobalEx.NetModule.OnLoginFail(EnNetConnFailType.ServerLoginFailed, EnNetLoginFailType.Login_Fail, errorCode);
            GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_FAIL, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
        }

        //此消息是 登陆服收到第3方平台结果后回复, 没错误不回
        private void ON_RECEIVE_MSG_LOGIN_ERROR_NTF(TCSMessage msg) 
		{
			var msgBody = msg.stTMSG_LOGIN_ERROR_NTF;
            var errorCode = msgBody.get_nError();
            if(errorCode != 0)
            {
                Debug.LogError($"[Login] 登录服务器失败！(LOGIN_NTF), errorCode={errorCode}");
               // GameUtility.ShowSystemFlowText("登录服务器失败, 错误码" + errorCode);
                //GameGlobalEx.NetModule.OnLoginFail(EnNetConnFailType.ServerLoginFailed, EnNetLoginFailType.Login_Fail, (uint)errorCode);
                GameGlobal.Instance.EventEngine.FireExecute(DGlobalEvent.EVENT_LOGIN_FAIL, DEventSourceType.SOURCE_TYPE_LOGIN, 0, null);
            }
        }


        //服务器维护公告
        private void ON_RECEIVE_MSG_LOGIN_SERVERMAINTAIN_NTF(TCSMessage msg)
        {
            TMSG_LOGIN_SERVERMAINTAIN_NTF msgBody = msg.stTMSG_LOGIN_SERVERMAINTAIN_NTF;

            var p = UIWindowShareInputParams.Instance;

            /*
            NoticeContext.Instance.content = msgBody.get_szNotice();

            int type = 2;
            if(int.TryParse(NoticeContext.Instance.content,out type)==false)
            {
                type = 3;
            }


            //type = 3;
            NoticeContext.Instance.type = type;
            p.ObjData = NoticeContext.Instance;

            UIWindowManager.Instance.ShowWindow<UINotice>(p);
            */
            //Debug.LogError("ON_RECEIVE_MSG_LOGIN_SERVERMAINTAIN_NTF");  
            //断网，不能重连
            GameGlobal.NetModule.Disconnect();  

        }


        public void SEND_MSG_LOGIN_LOGIN_REQ() 
        {
            

            TCSMessage msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_LOGIN_LOGIN_REQ);
            TMSG_LOGIN_LOGIN_REQ msgBody = msg.stTMSG_LOGIN_LOGIN_REQ;

            var loginData = LoginDataManager.instance.current;

#if !UNITY_EDITOR
            //真机下都用冰川登录
            loginData.partnerID = (int)gamepol.EnLoginPartnerID.enLoginPartnerID_Glacier;
                
#endif

           // loginData.partnerID = (int)gamepol.EnLoginPartnerID.enLoginPartnerID_Test;

            var baseData = msgBody.set_stBaseData();
            baseData.set_szUserName(loginData.userName);
            baseData.set_szPassword(loginData.password);
            baseData.set_i64World(loginData.serverID);



            baseData.set_nPartnerID(loginData.partnerID);


            baseData.set_szSession(loginData.session);
            TLoginExtraContext stLoginExContext = msgBody.set_stLoginExContext();
            stLoginExContext.set_szVerifyCode(loginData.userName);

            Debug.Log($"[Login] 请求登录服务器！{loginData}");

            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_LOGIN, msg);

        }

        public void SEND_MSG_LOGIN_HANDSHAKE_REQ() 
        {
            Debug.Log("[Login] 请求和服务器握手！");

            TCSMessage msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_LOGIN_HANDSHAKE_REQ);
            TMSG_LOGIN_HANDSHAKE_REQ msgBody = msg.stTMSG_LOGIN_HANDSHAKE_REQ;
            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_LOGIN, msg);
        }

        //上报账号基础信息
        public void SEND_MSG_CLIENT_LOGIN_OSS_REQ()
        {

            TCSMessage msg = GameGlobal.NetModule.GetAndInitGameMsg(true, TCSMessage.MSG_CLIENT_LOGIN_OSS_REQ);
            TMSG_CLIENT_LOGIN_OSS_REQ msgBody = msg.stTMSG_CLIENT_LOGIN_OSS_REQ;
            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, NetDefine.ENDPOINT_ZONE, msg);
        }
    }
}
