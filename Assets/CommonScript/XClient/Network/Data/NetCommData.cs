/*******************************************************************
** 文件名:	NetCommData.cs
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
using XGame.Poolable;

namespace XClient.Network
{
    public abstract class NetCommData : INetable
    {
        private NetID m_ID = new NetID(0, 0, 0);

        public ulong NetID => m_ID.ID;

        private List<INetVar> m_NetVars = new List<INetVar>();

        public List<INetVar> NetVars => m_NetVars;

        /// <summary>
        /// 接受端
        /// </summary>
        public NetVarLong TargetClient;

        /// <summary>
        /// 是否发送给我的
        /// </summary>
        public bool IsSendToMe
        {
            get
            {
                if (TargetClient.Value == 0)
                    return true;
                return NetworkManager.Instance.IsLocalClient((ulong)TargetClient.Value);
            }
        }

        public bool IsDebug { get; set; }

        public bool IsPublic => true;

        public bool IsHasRight => true;

        public NetCommData() { }

        public void Create()
        {
            TargetClient = SetupVar<NetVarLong>();
            OnSetupVars();
        }

        public void Reset() 
        {
            foreach (var v in m_NetVars)
            {
                v.Clear();
            }
        }

        public void SetNetID(ulong netID)
        {
            m_ID.Set(netID);
        }

        protected abstract void OnSetupVars();

        public void Release()
        {
            //回收变量对象
            foreach(var v in m_NetVars)
                LitePoolableObject.Recycle(v as LitePoolableObject);

            m_NetVars.Clear();
        }

        public T SetupVar<T>(string name = "", bool isPublic = false, bool isDebug = false) where T : LitePoolableObject, INetVar, new()
        {
            var v = LitePoolableObject.Instantiate<T>(this);
            v.IsPublic = isPublic;
            v.IsDebug = IsDebug;
            v.Name = name;
            m_NetVars.Add(v);
            return v;
        }

        public void OnNetVarChange(INetVar var)
        {
        }

        public void OnNetVarRemoteChange(INetVar var)
        {
        }
    }

}
