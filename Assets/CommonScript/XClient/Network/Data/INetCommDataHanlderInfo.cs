using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Network
{
    public delegate void OnHandleNetCommData<T>(T data) where T : NetCommData;

    internal interface INetCommDataHanlderInfo
    {
        /// <summary>
        /// 创建
        /// </summary>
        void Create(object data);

        /// <summary>
        /// 销毁
        /// </summary>
        void Release();

        /// <summary>
        /// 获得对应的对象
        /// </summary>
        NetCommData Obj { get;}

        /// <summary>
        /// 回调处理函数
        /// </summary>
        void InvokeHanlders();
    }

    internal class NetCommDataHandler<T> : INetCommDataHanlderInfo where T : NetCommData, new()
    {
        public OnHandleNetCommData<T> Handlers;

        public NetCommData Obj { get; private set; }

        public NetCommDataHandler() { }

        public void Create(object data)
        {
            Obj = data as T;
        }

        public void InvokeHanlders()
        {
            Handlers?.Invoke(Obj as T);
        }

        public void Release()
        {
            Obj?.Release();
            Obj = null;
        }
    }
}
