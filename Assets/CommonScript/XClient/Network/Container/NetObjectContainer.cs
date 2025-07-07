
using System.Collections.Generic;
using XClient.Common;

namespace XClient.Network
{
    /// <summary>
    /// 网络对象容器
    /// </summary>
    public class NetObjectContainer
    {
        private NetObjectContainer() { }
        public static NetObjectContainer Instance = new NetObjectContainer();

        /// <summary>
        /// 创建一个网络数据容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateObject<T>(bool isAutoAdd = false) where T: NetObjectElement, new()
        {
            var netID = GameGlobal.Role.entityIDGenerator.Next();
            T obj = new T();
            obj.Create();
            obj.SetupNetID(netID, 0);

            if(isAutoAdd)
            {
                obj.Start();
            }

            return obj;
        }

        /// <summary>
        /// 获取指定类型的数据容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void GetObjects<T>(List<T> rets) where T : NetObjectElement
        {
            foreach(var obj in NetObjectManager.Instance.AllObjects.Values)
            {
                if(obj is T)
                {
                    rets.Add(obj as T);
                }
            }
        }

        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isPublic">只有此对象的拥有者才能修改此属性</param>
        public void AddObject(NetObjectElement obj, bool isPublic = false)
        {
            obj.IsPublic = false;
            obj.Start();
            obj.IsPublic = isPublic;
        }

        /// <summary>
        /// 移除一个对象
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(NetObjectElement obj)
        {
            obj.Stop();
            obj.IsPublic = false;
        }
    }

}