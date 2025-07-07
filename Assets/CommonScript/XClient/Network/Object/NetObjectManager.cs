/*******************************************************************
** 文件名:	NetObjectManager.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2024/5/21 15:35:30
** 版  本:	1.0
** 描  述:	
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using XGame.Poolable;

namespace XClient.Network
{
    /// <summary>
    /// 网络对象的类型
    /// </summary>
    public static class NetObjectType
    {
        //网络数据容器中的数据
        public const int Element = 99887766;

        //网络通信数据
        public const int ComData = 99887765;

        //网络Mono数据，通常依附于网络实体对象
        public const int Mono = 99887764;

        //网络实体的数据对象，依附于网络实体
        public const int EntityData = 99887763;
    }

    /// <summary>
    /// 网络对象管理器
    /// </summary>
    public class NetObjectManager
    {
        private NetObjectManager() { }
        public static NetObjectManager Instance = new NetObjectManager();

        /// <summary>
        /// 网络对象列表
        /// </summary>
        private Dictionary<ulong, NetObject> m_NetObjectDict = new Dictionary<ulong, NetObject>();

        /// <summary>
        /// 所有网络对象
        /// </summary>
        public Dictionary<ulong, NetObject> AllObjects => m_NetObjectDict;

        /// <summary>
        /// 网络对象黑名单列表
        /// </summary>
        private Dictionary<ulong, long> m_NetObjecBlackList = new Dictionary<ulong, long>();

        /// <summary>
        /// 白名单客户端列表
        /// </summary>
        private Dictionary<ulong, byte> m_ClientWhiteList = new Dictionary<ulong, byte>();

        /// <summary>
        /// 是否启用了客户端白名单。当启用了白名单后，只有在白名单中的客户端才能进行在本地创建
        /// 相关网络对象。这些对象包括：NetEntity, NetObjectType.Mono, NetObjectType.EntityData
        /// </summary>
        public bool IsClientWhiteListEnabled { get; private set; }

        /// <summary>
        /// 判断端是否有效。如果没有开启白名单，都是有效的，
        /// 如果开启了白名单，则必须在白名单中的端才是有效的
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool IsValidClient(ulong netID)
        {
            if (!IsClientWhiteListEnabled)
                return true;

            NetID.Temp.Set(netID);
            bool isValid = m_ClientWhiteList.ContainsKey(NetID.Temp.ClientID);

            //if(!isValid)
            //{
            //    NetworkManager.Debug.Error($"客户端不在白名单中：{NetID.Temp}");
            //}

            return isValid;
        }

        /// <summary>
        /// 将网络对象添加到黑名单，不在白名单中的客户端创建的网络对象都记录在这里
        /// 也就是说底层网络对象还是全部进行了广播，但是没有被正式创建，全部被拦截了下来
        /// </summary>
        /// <param name="netID"></param>
        public bool AddObjectToBlackList(ulong netID)
        {
            if (!IsClientWhiteListEnabled)
                return false;

            if (!m_NetObjecBlackList.ContainsKey(netID))
            {
                NetID.Temp.Set(netID);
                //NetworkManager.Debug.Error($"{NetID.Temp}被加入黑名单！");
                m_NetObjecBlackList.Add(netID, NetID.Temp.ClientID);
            }

            return true;
        }

        /// <summary>
        /// 将网络对象移出黑名单，不在白名单中的客户端创建的网络对象都记录在这里
        /// </summary>
        /// <param name="netID"></param>
        public bool RemoveObjectFromBlackList(ulong netID)
        {
            if (!IsClientWhiteListEnabled)
                return false;

            if (m_NetObjecBlackList.ContainsKey(netID))
            {
                m_NetObjecBlackList.Remove(netID);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 将网络对象移出黑名单，不在白名单中的客户端创建的网络对象都记录在这里
        /// </summary>
        /// <param name="netID"></param>
        public bool RemoveObjectFromBlackListByClient(long clientID)
        {
            List<ulong> netIds = PoolableList.Get<ulong>();
            foreach(var item in m_NetObjecBlackList)
            {
                if(item.Value == clientID)
                    netIds.Add(item.Key);
            }

            foreach(var id in netIds)
            {
                m_NetObjecBlackList.Remove(id);
            }

            PoolableList.Recycle(netIds);

            return false;
        }

        /// <summary>
        /// 是否在黑名单中
        /// </summary>
        /// <param name="netID"></param>
        public bool IsObjectInBlackList(ulong netID)
        {
            bool isIn =  m_NetObjecBlackList.ContainsKey(netID);

            //if (isIn)
            //{
            //    NetID.Temp.Set(netID);
            //    NetworkManager.Debug.Error($"在黑名单中的实体：{NetID.Temp}");
            //}

            return isIn;
        }

        /// <summary>
        /// 添加到白名单
        /// </summary>
        /// <param name="netID"></param>
        public void AddClientToWhiteList(ulong netID)
        {
            if (!IsClientWhiteListEnabled)
                return;

            NetID.Temp.Set(netID);
            if (!m_ClientWhiteList.ContainsKey(NetID.Temp.ClientID))
                m_ClientWhiteList.Add(NetID.Temp.ClientID, 1);
        }

        /// <summary>
        /// 添加到白名单
        /// </summary>
        /// <param name="netID"></param>
        public void RemoveClientFromWhiteList(ulong netID)
        {
            if (!IsClientWhiteListEnabled)
                return;

            NetID.Temp.Set(netID);
            if(m_ClientWhiteList.ContainsKey(NetID.Temp.ClientID))
                m_ClientWhiteList.Remove(NetID.Temp.ClientID);
        }

        /// <summary>
        /// 清空白名单列表
        /// </summary>
        public void ClearClientWhiteList()
        {
            m_ClientWhiteList.Clear();
        }

        /// <summary>
        /// 启用了白名单
        /// </summary>
        public void EnableClientWhiteList()
        {
            IsClientWhiteListEnabled = true;
            m_ClientWhiteList.Clear();
        }

        /// <summary>
        /// 禁用客户端白名单
        /// </summary>
        public void DisableClientWhiteList()
        {
            IsClientWhiteListEnabled = false;
            m_ClientWhiteList.Clear();
        }

        /// <summary>
        /// 添加网络对象
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(NetObject obj)
        {
            if(m_NetObjectDict.ContainsKey(obj.NetID))
            {
                NetID.Temp.Set(obj.NetID);
                NetworkManager.Debug.Log($"重复添加NetObject对象, NetID={NetID.Temp}");
                return;
            }

            if(obj.IsDebug)
                NetworkManager.Debug.Log($"添加网络对象, NetID={obj.NetID}, type={obj.GetType().FullName}");

            m_NetObjectDict.Add(obj.NetID, obj);

            //独立对象要自主广播
            if(obj.IsOwner && obj.IsIndependent)
            {
                NetworkManager.Instance.Syncer.CreateToOthers(obj);
                //NetObjectSyncer.UpdateToOthers(obj, false);
                obj.ClearDirty();
            }
        }

        /// <summary>
        /// 移除网络对象
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(NetObject obj)
        {
            if (m_NetObjectDict.ContainsKey(obj.NetID))
            {
                if (obj.IsDebug)
                    NetworkManager.Debug.Log($"移除网络对象, NetID={obj.NetID}, type={obj.GetType().FullName}");

                m_NetObjectDict.Remove(obj.NetID);

                //独立对象要自主广播
                if (obj.IsOwner && obj.IsIndependent)
                {
                    NetworkManager.Instance.Syncer.DestroyToOthers(obj);
                }
            }
        }

        /// <summary>
        /// 获取网络对象
        /// </summary>
        /// <param name="entID"></param>
        /// <returns></returns>
        public NetObject GetObject(ulong entID)
        {
            if (m_NetObjectDict.TryGetValue(entID, out NetObject obj))
                return obj;
            return null;
        }

        /// <summary>
        /// 立即同步
        /// </summary>
        /// <param name="obj"></param>
        public void SyncObjectImmediately(NetObject obj)
        {
            //属性变更通知
            if (obj.IsHasRight && obj.IsDirty)
            {
                if(obj.GetType().FullName == "GameScripts.CardShop.PublicCardGroupNetData")
                {
                    Debug.Log("GameScripts.CardShop.PublicCardGroupNetData 开始i同步！");
                }

                NetworkManager.Instance.Syncer.UpdateToOthers(obj, true);
                obj.ClearDirty();
            }

            //属性变更请求
            if (obj.IsRemoteDirty)
            {
                NetworkManager.Instance.Syncer.SendPropChangeReqToTarget(obj);
                obj.ClearRemoteDirty();
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            foreach (var obj in m_NetObjectDict.Values)
            {
                SyncObjectImmediately(obj);
            }
        }
    }
}
