/*******************************************************************
** 文件名:	IBaseSDK.cs
** 版  权:	(C) 冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2020.10.23
** 版  本:	1.0
** 描  述:	
** 应  用:  SDK

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using UnityEngine;
using XGame.AssetScript.SDK.Core;

namespace XGame.AssetScript.SDK.Base
{
    /// <summary>
    /// BaseSDK的消息ID定义，需要与BaseSDK插件中定义的ID对应
    /// </summary>
    public class BaseSDKMessage
    {
        //返回设备标识符
        public static readonly int DeviceIdentity = 10000;

        //应用的用户数据
        public static readonly int AppUserData = 10001;

        //交易记录
        public static readonly int PurchaseRecords = 10002;
    }

    public interface IBaseSDK : ISDK
    {
        /// <summary>
        /// 是否为挖孔屏
        /// </summary>
        /// <returns></returns>
        bool HasNotchInScreen();

        /// <summary>
        /// 重启APP
        /// </summary>
        void RestartApp();

        /// <summary>
        /// 震动
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="time"></param>
        void VibratorApp(long delay, long time);

        /// <summary>
        /// 获取设备唯一标识
        /// </summary>
        /// <returns></returns>
        void RequestDeviceIdentify(Action<string, string> callback);

        /// <summary>
        /// 获取挖孔屏的安全边距
        /// </summary>
        /// <returns></returns>
        RectOffset GetNotchScreenPadding();

        /// <summary>
        /// 保存应用数据
        /// </summary>
        /// <param name="data"></param>
        void SaveApplicationData(string data);

        /// <summary>
        /// 读取应用数据
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        void LoadApplicationData(Action<string> callback);

        /// <summary>
        /// 保存应用数据
        /// </summary>
        /// <param name="data"></param>
        void SaveApplicationDataWithKey(string key, string data);

        /// <summary>
        /// 读取应用数据
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        void LoadApplicationDataWithKey(string key, Action<string> callback);
    }
}
