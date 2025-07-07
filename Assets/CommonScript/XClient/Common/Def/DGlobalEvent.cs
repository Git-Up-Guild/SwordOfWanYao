/*******************************************************************
** 文件名:	DGlobalEvent.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	李涛
** 日  期:	2016/1/18
** 版  本:	1.0
** 描  述:	事件码定义
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;

namespace XClient.Common
{
    public class DGlobalEvent
    {
        // 画质改变
        public const ushort EVENT_ENTITYVIEW_QUALITY_CHANGE = 1;

        //游戏状态将要切换状态
        public const ushort EVENT_GAME_STATE_PRE_CHANGE = 2;

        //游戏状态切换完成状态
        public const ushort EVENT_GAME_STATE_AFTER_CHANGE = 3;



        //登录相关
        public const ushort EVENT_LOGIN_BASE = 100;
        public const ushort EVENT_LOGIN_GATEWAY_SUC = EVENT_LOGIN_BASE + 1;                    //连接网关成功
        
        public const ushort EVENT_LOGIN_GATEWAY_FAIL = EVENT_LOGIN_BASE + 2;                   //连接网关失败
        public const ushort EVENT_LOGIN_HANDSHAKE_SUC = EVENT_LOGIN_BASE + 3;                           //握手成功
        public const ushort EVENT_LOGIN_HANDSHAKE_FAIL = EVENT_LOGIN_BASE + 4;                          //握手失败
        public const ushort EVENT_LOGIN_SUC = EVENT_LOGIN_BASE + 5;                                        //登录成功
        public const ushort EVENT_LOGIN_FAIL = EVENT_LOGIN_BASE + 6;                                       //登录失败
        public const ushort EVENT_NET_CONNECT_ERROR = EVENT_LOGIN_BASE + 7;                    //链接错误

        public const ushort EVENT_NET_RETURN_LOGIN = EVENT_LOGIN_BASE + 8;                    //返回登录
        public const ushort EVENT_NET_CLEAR = EVENT_LOGIN_BASE + 9;                    //清理系统
        public const ushort EVENT_NET_RECONNECT = EVENT_LOGIN_BASE + 10;                    //清理系统
        public const ushort EVENT_NET_ENTERGAME = EVENT_LOGIN_BASE + 11;                    //进入游戏
                                                                           
        //游戏时间(小时变化)
        public const ushort EVENT_LOGIN_HOUR_CHANGE = EVENT_LOGIN_BASE + 12;

        //显示选服
        public const ushort EVENT_SHOW_SELECT_SERVE = EVENT_LOGIN_BASE + 13;


        //实体相关
        public readonly static ushort EVENT_ENTITY_BASE = 120;                                                                              //实体创建
        public readonly static ushort EVENT_ENTITY_CREATE = EVENT_ENTITY_BASE++;                                        //实体创建
        public readonly static ushort EVENT_ENTITY_ROLE_CREATE = EVENT_ENTITY_BASE++;                               //玩家创建
        public readonly static ushort EVENT_ENTITY_DESTROY = EVENT_ENTITY_BASE++;                                       //实体销毁
        public readonly static ushort EVENT_ENTITY_UPDATE = EVENT_ENTITY_BASE++;                                       //实体销毁
        public readonly static ushort EVENT_ENTITY_ROLE_DESTROY = EVENT_ENTITY_BASE++;                           //玩家销毁
        public readonly static ushort EVENT_ENTITY_ROLE_DATA_READY = EVENT_ENTITY_BASE++;                    //玩家数据初始化完成
        public readonly static ushort EVENT_ENTITY_ROLE_DATA_UPDATE = EVENT_ENTITY_BASE++;                    //玩家数据初始化完成
        public readonly static ushort EVENT_ENTITY_ROLE_EXP_UPDATE = EVENT_ENTITY_BASE++;                    //玩家经验（游历次数）更新
        public readonly static ushort EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE = EVENT_ENTITY_BASE++;       //玩家数值物品更新, 现场：EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT
        public readonly static ushort EVENT_ENTITY_ROLE_NAME_CHANGE = EVENT_ENTITY_BASE++;       //玩家改名

        //房间事件相关
        public const ushort EVENT_ROOM_BASE = 140;
        public const ushort EVENT_ROOM_ENTER = EVENT_ROOM_BASE+1;
        public const ushort EVENT_ROOM_LEAVE = EVENT_ROOM_BASE + 2;
        public const ushort EVENT_ROOM_USER_ENTER = EVENT_ROOM_BASE + 3;
        public const ushort EVENT_ROOM_PROPERTY_UPDATED = EVENT_ROOM_BASE + 4;
        public const ushort EVENT_ROOM_ENTER_COMPLETE = EVENT_ROOM_BASE + 5;
        public const ushort EVENT_ROOM_ENTER_FAIL = EVENT_ROOM_BASE + 6;
        public const ushort EVENT_ROOM_INFO_UPDATE = EVENT_ROOM_BASE + 7;

        //代理事件相关
        public const ushort EVENT_AGENT_BASE = 160;
        public const ushort EVENT_AGENT_CREATE = EVENT_AGENT_BASE + 1;
        public const ushort EVENT_AGENT_DESTROY = EVENT_AGENT_BASE + 2;

        //商店系统事件
        private readonly static ushort EVENT_SIMPLE_SHOP_BASE = 180;
        public readonly static ushort EVENT_SIMPLE_SHOP_UPD = EVENT_SIMPLE_SHOP_BASE++;           //商店更新

        //任务系统事件
        private readonly static ushort EVENT_TASK_BASE = 200;
        public readonly static ushort EVENT_TASK_UPD = EVENT_TASK_BASE++;           //任务更新
        public readonly static ushort EVENT_TASK_FINISH = EVENT_TASK_BASE++;        //任务完成
        
        //物品实体变化
        private readonly static ushort EVENT_GOOD_BASE = 300;
        public readonly static ushort EVENT_GOOD_ENTITY_CHANGE = EVENT_GOOD_BASE++;        //物品变动

        //物品实体变化
        private readonly static ushort EVENT_STAMINA_BASE = 350;
        public readonly static ushort EVENT_STAMINA_UPDATE = EVENT_STAMINA_BASE++;        //体力变动

        //邮件相关
        private readonly static ushort EVENT_MAIL_BASE = 400;
        public readonly static ushort EVENT_MAIL_DATA_UPDATE = EVENT_STAMINA_BASE++;        //邮件数据变更

        //最大值
        public const ushort EVENT_ALL_MAXID = 10000;
    }

    //////////////////////////发送源类型 ///////////////////////////
    ///
    /// 发送源类型
    public class DEventSourceType
    {
        public static readonly byte SOURCE_TYPE_UNKNOW = 0;        // 类型ID根
        public static readonly byte SOURCE_TYPE_SYSTEM = 1;             //系统事件类型
        public static readonly byte SOURCE_TYPE_ROOM = 2;               // 房间类型
        public static readonly byte SOURCE_TYPE_AGENT = 3;              // 代理类型
        public static readonly byte SOURCE_TYPE_LOGIN = 4;             //登录
        public static readonly byte SOURCE_TYPE_ENTITY = 5;             //实体
    };

    /// <summary>
    /// 状态切换事件
    /// </summary>
    public class SEventGameStateChange_C
    {
        public int nOldState;       // 老状态
        public int nNewState;       // 新状态
    };

    //代理的创建和销毁现场
    public class SAngentCreateDestroy
    {
        public long agentID;         // 创建或者销毁的角色ID 
    };

    //代理的创建和销毁现场
    public class SRoomCreateDestroy
    {
        public long roleID;         // 角色ID 
        public int roomID; //房间ID
        public int iPlayerIdx;   //玩家进入房间序号
    };

    public class EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT
    {
        public static EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT Instance = new EVENT_ENTITY_ROLE_NUM_GOODS_UPDATE_CONTEXT();
        public List<int> lstGoodsID = new List<int>();

        public void Reset()
        {
            lstGoodsID.Clear();
        }
    }


    //   EVENT_ROOM_PROPERTY_UPDATED的消息现场
    public class SEVENT_ROOM_PROPERTY_UPDATED
    {
        public gamepol.TPropertySet stProperty;   //属性数据
        public long iVer;   //数据集新版本号
        public long iModifier;   //修改者的roleid
        public string szPassback;   //透传
    }

    //  房间进入完成
    public class SEVENT_ROOM_ENTER_COMPLETE
    {
        public long roleID;         // 角色ID 
        public int roomID; //房间ID
        public int iPlayerIdx;   //玩家进入房间序号
    }

    /// <summary>
    /// 状态切换事件
    /// </summary>
    public class GameStateChangeEventContext
    {
        public int nOldState;       // 老状态
        public int nNewState;       // 新状态
    };

    /// <summary>
    /// 商店更新事件
    /// </summary>
    public class ShopUpdateEventContext
    {
        private ShopUpdateEventContext() { }
        public static ShopUpdateEventContext instance = new ShopUpdateEventContext();

        public int shopID;
    }

}