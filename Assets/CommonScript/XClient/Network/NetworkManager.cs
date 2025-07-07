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

using gamepol;
using XGame.Utils;

namespace XClient.Network
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager
    {
        public static IDebugEx Debug = DebugEx.GetInstance("Network");

        private NetworkManager() 
        {
            HostClientID = 999999999;
            LocalClientID = HostClientID - 1;

            Syncer = new NetableSyncer();
        }

        /// <summary>
        /// 实例对象
        /// </summary>
        public static NetworkManager Instance = new NetworkManager();

        /// <summary>
        /// 本地客户端ID
        /// </summary>
        public uint LocalClientID { set; get; }

        /// <summary>
        /// 主机客户端ID
        /// </summary>
        public uint HostClientID { get; set; }

        /// <summary>
        /// 同步器
        /// </summary>
        public NetableSyncer Syncer { get; private set; }

        /// <summary>
        /// 是否为本地主机
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool IsLocalClient(NetID netID)
        {
            return LocalClientID == netID.ClientID;
        }

        /// <summary>
        /// 是否为本地主机
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool IsLocalClient(ulong netID)
        {
            NetID.Temp.Set(netID);
            return LocalClientID == NetID.Temp.ClientID;
        }

        /// <summary>
        /// 是否为主机
        /// </summary>
        /// <returns></returns>
        public bool IsHost()
        {
            return HostClientID == LocalClientID;
        }

        /// <summary>
        /// 是否拥有操作权限（拥有者和主机都可以修改）
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool IsHasRight(NetID netID)
        {
            return IsLocalClient(netID) || IsHost();
        }

        /// <summary>
        /// 是否拥有操作权限（拥有者和主机都可以修改）
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool IsHasRight(ulong netID)
        {
            return IsLocalClient(netID) || IsHost();
        }

        /// <summary>
        /// 添加网络对象
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(NetObject obj)
        {
            NetObjectManager.Instance.AddObject(obj);
        }

        /// <summary>
        /// 移除网络对象
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(NetObject obj)
        {
            NetObjectManager.Instance.RemoveObject(obj);
        }

        /// <summary>
        /// 立即同步某个对象
        /// </summary>
        /// <param name="obj"></param>
        public void SyncObjectImmediately(NetObject obj)
        {
            NetObjectManager.Instance.SyncObjectImmediately(obj);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            Syncer.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            Syncer.Stop();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            NetObjectManager.Instance.Update();
        }
    }
}
