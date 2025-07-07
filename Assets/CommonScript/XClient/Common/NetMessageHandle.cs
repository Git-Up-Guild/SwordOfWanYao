using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XClient.Common
{
    /// <summary>
    /// 网络消息注册器
    /// </summary>
    public interface IMessageRegisterHandler
    {
        //消息统一订阅回调接口
        void Register(IMessageDispatcher dispatcher);
        //消息统一退订回调接口
        void UnRegister(IMessageDispatcher dispatcher);
    }

    /// <summary>
    /// 消息订阅helper
    /// </summary>
    public class NetHelper
    {
        IMessageDispatcher m_dispatcher;
        public NetHelper(IMessageDispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
        }

        public IMessageDispatcher GetDispatcher()
        {
            return m_dispatcher;
        }
        /// <summary>
        /// 设置handler，进行消息订阅
        /// </summary>
        /// <param name="handler"></param>
        public void SetHandler(IMessageRegisterHandler handler)
        {
            if (handler == null)
            {
                return;
            }
            handler.Register(m_dispatcher);
        }

        /// <summary>
        /// 移除handler，退订消息
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveHandle(IMessageRegisterHandler handler)
        {
            if (handler == null)
            {
                return;
            }
            handler.UnRegister(m_dispatcher);
        }
    }
}
