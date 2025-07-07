using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XClient.Common
{
    /// <summary>
    /// 组件ID，由于在Lua中处理枚举有点麻烦，所以干脆定义成int
    /// </summary>
    public class DComID
    {
        public static int CUR = 0;
        public static readonly int MIN = CUR;
        public static readonly int COM_ID_EVENT = MIN;              //事件
        public static readonly int COM_ID_TIMER = ++CUR;            //定时器
        public static readonly int COM_ID_GAME_SCHEME = ++CUR;              //表格组件
        public static readonly int MAX = ++CUR;
    }
}
