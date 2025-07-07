
using XClient.Network;

namespace XClient.Entity
{
    /// <summary>
    /// 实体创建共享现场
    /// </summary>
    public class NetEntityShareInitContext
    {
        /// <summary>
        /// 网络实体初始化现场
        /// </summary>
        public class NetInitContext
        {
            //网络数据对象在进行创建的时候携带了 数据部件的数据（通过一个序列化对象导入）
            public INetableSerializer netDataSerializer;

            //那些通过资源路径来构建的网络实体需要资源路径
            public string resPath;

            public void Reset()
            {
                netDataSerializer = null;
                resPath = null;
            }
        }

        private NetEntityShareInitContext() { }

        public static NetEntityShareInitContext instance = new();

        /// <summary>
        /// 本地创建自定义现场
        /// </summary>
        public object localInitContext;

        /// <summary>
        /// 是否为从网络过来的初始化
        /// </summary>
        public bool isInitFromNet;

        /// <summary>
        /// 网络创建对象初始化现场。
        /// </summary>
        public NetInitContext netInitContext = new NetInitContext();

        public void Reset()
        {
            localInitContext = null;
            isInitFromNet = false;
            netInitContext.Reset();
        }

    }

}
