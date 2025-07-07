/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: Login.cs 
Module: Login
Author: 郑秀程
Date: 2024.06.17
Description: 登录模块
***************************************************************************/

using XClient.Common;

namespace XClient.Login
{

    //登录行为
    public enum LOGOUT_ACTION
    {
        NON,//空状态
        LOGIN,//登录
        SWITCH_ACCOUNTS, //切换账号
        SWITCH_SERVER, //选服
    }

    public interface ILoginModule : IModule
    {
        /// <summary>
        /// 登录房间
        /// </summary>
        /// <param name="roomID"></param>
        void Login(int roomID = 0);

        /// <summary>
        /// 退出登录
        /// </summary>
        void Logout(LOGOUT_ACTION action = LOGOUT_ACTION.LOGIN);

        //获取登出行为
        LOGOUT_ACTION GetLogoutAction();

        /// <summary>
        /// 开始进入房间
        /// </summary>
        /// <param name="roomID"></param>
        void StartEnterRoom(int roomID);

        //退出房间
        void ExitRoom();

        //检测整点事件
        void CheckIntegralHourPoint();

        ServerSelectManager GetServerSelectManager();

        
    }
}