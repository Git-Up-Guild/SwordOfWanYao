using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Network
{
    public interface INetObjectBehaviour
    {
        /// <summary>
        /// 获取网络对象
        /// </summary>
        /// <returns></returns>
        INetObject GetNetObject();

        /// <summary>
        /// NetObject是否为一个公开对象
        /// </summary>
        /// <returns></returns>
        bool IsNetObjectPublic { get; set; }

        /// <summary>
        /// 获取对应的Mono对象
        /// </summary>
        MonoBehaviour Mono { get; }

        /// <summary>
        /// NetObject对象启动时调用
        /// </summary>
        void OnNetObjectStart();

        /// <summary>
        /// NetObject对象停止时调用
        /// </summary>
        void OnNetObjectStop();
    }
}
