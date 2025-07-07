using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XClient.Common
{
    /// <summary>
    /// 模块ID，由于在Lua中处理枚举有点麻烦，所以干脆定义成int
    /// </summary>
    public class DModuleID
    {
        public static int CUR = 0;
        public static readonly int MIN = CUR;
        public static readonly int MODULE_ID_REDDOT = ++CUR;     //红点模块
        //public static readonly int MODULE_ID_GUIDE = ++CUR;     //引导模块
        public static readonly int MODULE_ID_GAME_LOADER = MIN; //游戏加载器
        public static readonly int MODULE_ID_SCHEME = ++CUR;    //配置脚本
        public static readonly int MODULE_ID_NET = ++CUR;// 网络模块
        public static readonly int MODULE_ID_AGENT = ++CUR;    //玩家代理模块
        public static readonly int MODULE_ID_ROOM = ++CUR;    //房间模块

        public static readonly int MODULE_ID_NET_TRANSFER = ++CUR;    //上层demo网络中转层

        public static readonly int MODULE_ID_RPC = ++CUR;       //远程调用模块

        public static readonly int MODULE_ID_LOGIN = ++CUR;     //登录模块
        public static readonly int MODULE_ID_ENTITY = ++CUR;    //实体模块

        //public static readonly int MODULE_ID_BAG = ++CUR;       //背包模块

        public static readonly int MODULE_ID_BASE_MAX = ++CUR;    //最大基础值

        public static readonly int MODULE_ID_SIMPLE_SHOP = ++CUR;     //商店模块

        public static readonly int MAX = 48;
    }
}
