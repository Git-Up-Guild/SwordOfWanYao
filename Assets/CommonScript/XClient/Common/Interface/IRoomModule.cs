using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XClient.Common
{
    //房间模块接口
    public interface IRoomModule : IModule
    {
        //进入房间
        void EnterRoom(int RoomID);

        //退出房间请求
        void LeaveRoom();

        //获取本玩家的RoleID
        long GetLocalRoleID();

        Dictionary<int, long> GetAllProperty(int iBook);

        long GetIntProperty(int iBook, int PropID);

        /// <summary>
        /// 设置房间属性
        /// </summary>
        /// <param name="iBook"></param>
        /// <param name="PropID"></param>
        /// <param name="Val"></param>
        /// <param name="bCheckVer"> 是否需要服务器检查数据集版本. 
        ///       如果为true, 调用这个方法后, 本地数据不会立即更新. 需要等到服务器回复. 
        ///       例如      
        ///             id_5  = 222
        ///             SetIntProperty(0, 5, 9999, true);   将id5  设置为9999
        ///             GetIntProperty(0, 5);               返回 222,  本地不更新. 
        ///       应用层可以订阅EVENT_ROOM_PROPERTY_UPDATED 进行更新后处理
        ///       
        /// 
        ///       如果为false, 不需要检查, 设置后本地数据即时更新.   
        ///                 
        /// 
        /// </param>
        /// <param name="sPassback"> 透传参数, 在EVENT_ROOM_PROPERTY_UPDATED事件中返回 </param>
        /// <returns></returns>
        void SetIntProperty(int iBook, int PropID, long Val, bool bCheckVer, string sPassback);


        /// <summary>
        /// 设置房间属性 (多属性版本)
        /// </summary>
        /// <param name="iBook"></param>
        /// <param name="arrPropID"></param>
        /// <param name="arrVal"></param>
        /// <param name="iLen"></param>
        /// <param name="bCheckVer"></param>
        /// <param name="sPassback"></param>
        /// <returns></returns>
        void SetIntProperty(int iBook, int[] arrPropID, long[] arrVal, int iLen, bool bCheckVer, string sPassback);

        //获取进入房间序号
        int GetPlayerIndex();

        /// <summary>
        /// 请求房间信息
        /// </summary>
        /// <param name="RoomID">房间ID</param>
        void ReqRoomInfo(int    RoomID);
        
        /// <summary>
        /// 获取房间人数
        /// </summary>
        /// <param name="RoomID">房间ID</param>
        /// <returns></returns>
        int  GetRoomRoleNum(int RoomID);
    }


}