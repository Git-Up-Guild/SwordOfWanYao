using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Network
{
    /// <summary>
    /// 让对象具有网络处理的能力
    /// </summary>
    public interface INetable
    {
        /// <summary>
        /// 网络ID
        /// </summary>
        ulong NetID { get; }

        /// <summary>
        /// 拥有的网络变量列表
        /// </summary>
        List<INetVar> NetVars { get; }

        /// <summary>
        /// 是否公开的
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// 是否有权限
        /// </summary>
        bool IsHasRight { get; }

        /// <summary>
        /// 是否调试模式
        /// </summary>
        bool IsDebug { get; set; }

        /// <summary>
        /// 网络变量更新
        /// </summary>
        /// <param name="var"></param>
        void OnNetVarChange(INetVar var);

        /// <summary>
        /// 网络变量更新
        /// </summary>
        /// <param name="var"></param>
        void OnNetVarRemoteChange(INetVar var);
    }
}
