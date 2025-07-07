
using gamepol;
using System.Collections.Generic;
using XClient.Common;

namespace XClient.Network
{
    /// <summary>
    /// 网络数据容器管理器
    /// </summary>
    public class NetCommDataManager
    {
        private NetCommDataManager() { }
        public static NetCommDataManager Instance = new NetCommDataManager();

        /// <summary>
        /// 路径与Object映射
        /// </summary>
        private Dictionary<string, NetCommData> m_PathToObject = new Dictionary<string, NetCommData>();

        /// <summary>
        /// 网络通信数据处理器
        /// </summary>
        private Dictionary<string, INetCommDataHanlderInfo> m_HanderDataDict = new Dictionary<string, INetCommDataHanlderInfo>();

        /// <summary>
        /// 添加处理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void AddHandler<T>(OnHandleNetCommData<T> handler) where T : NetCommData, new()
        {
            var path = typeof(T).FullName;
            if (!m_HanderDataDict.TryGetValue(path, out INetCommDataHanlderInfo d))
            {
                d = new NetCommDataHandler<T>();
                d.Create(GetData<T>(false));
                m_HanderDataDict.Add(path, d);
            }

            var data = d as NetCommDataHandler<T>;
            data.Handlers += handler;
        }

        /// <summary>
        /// 移除处理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void RemoveHandler<T>(OnHandleNetCommData<T> handler) where T : NetCommData, new()
        {
            var path = typeof(T).FullName;
            if (m_HanderDataDict.TryGetValue(path, out INetCommDataHanlderInfo d))
            {
                (d as NetCommDataHandler<T>).Handlers -= handler;
            }
        }

        /// <summary>
        /// 收到网络数据
        /// </summary>
        /// <param name="path"></param>
        /// <param name="serializer"></param>
        public void OnReceive(string path, ulong netID, TMSG_NET_OBJ_CREATE_NTF message)
        {
            if (m_HanderDataDict.TryGetValue(path, out INetCommDataHanlderInfo d))
            {
                d.Obj.SetNetID(netID);
                NetworkManager.Instance.Syncer.ReceiveFromOthers(d.Obj, message);
                d.InvokeHanlders();
            }
        }

        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="targetID">0表示发送给所有人</param>
        /// <param name="data"></param>
        public void SendTo(ulong targetID, NetCommData data)
        {
            data.TargetClient.Value = (long)targetID;
            NetworkManager.Instance.Syncer.CreateToOthers(data);
        }

        /// <summary>
        /// 获取专门用来发送的数据对象。注意：这个对象是不能被缓存的，其数据随时可能被改写。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSendData<T>() where T : NetCommData, new()
        {
            return GetData<T>(true);
        }

        /// <summary>
        /// 获取数据对象。注意：这个对象是不能被缓存的，其数据随时可能被改写。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>(bool isForSend) where T : NetCommData, new()
        {
            var path = typeof(T).FullName;

            if (!m_PathToObject.TryGetValue(path, out NetCommData obj))
            {
                obj = new T();
                obj.Create();
                m_PathToObject.Add(path, obj);
            }

            obj.Reset();

            if (isForSend)
            {
                var netID = GameGlobal.Role.entityIDGenerator.Next();
                obj.SetNetID(netID);
            }

            return obj as T;
        }
    }
}