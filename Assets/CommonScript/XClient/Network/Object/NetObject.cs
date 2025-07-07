/*******************************************************************
** 文件名:	NetObject.cs
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
using XGame.Utils;

namespace XClient.Network
{
    public abstract class NetObject : INetObject
    {
        public static IDebugEx Debug => NetworkManager.Debug;

        private NetID m_ID = new NetID(0, 0, 0);

        public ulong NetID => m_ID.ID;

        private List<INetVar> m_NetVars = new List<INetVar>();

        public List<INetVar> NetVars => m_NetVars;

        /// <summary>
        /// 是否为独立的，如果是独立的会自主进行广播
        /// </summary>
        public virtual bool IsIndependent => true;

        /// <summary>
        /// 是否拥有权限进行修改
        /// </summary>
        public bool IsHasRight => IsPublic || NetworkManager.Instance.IsHasRight(NetID);

        /// <summary>
        /// 是否为拥有者
        /// </summary>
        public bool IsOwner => NetworkManager.Instance.IsLocalClient(NetID);

        /// <summary>
        /// 是否启用同步
        /// </summary>
        private bool m_IsEnableSync;
        public bool IsEnableSync {
            get => m_IsEnableSync;
            set
            {
                if (m_IsEnableSync == value)
                    return;

                m_IsEnableSync = value;

                if(IsStarted)
                {
                    if (value)
                        NetworkManager.Instance.AddObject(this);
                    else
                        NetworkManager.Instance.RemoveObject(this);
                }
            }
        }

        /// <summary>
        /// 是否公开
        /// </summary>
        public virtual bool IsPublic 
        {
            get => m_IsPublicFlag.Value;
            set
            {
                //只有拥有者才能修改公开标记
                if(IsOwner)
                {
                    m_IsPublicFlag.Value = value;
                }
            }
        }

        /// <summary>
        ///  公开标记，只有拥有者才能决定是否公开此标记
        /// </summary>
        private NetVarBool m_IsPublicFlag;

        /// <summary>
        /// 是否脏了
        /// </summary>
        private bool m_IsDrity;
        public bool IsDirty 
        { 
            get=> m_IsDrity; 
            private set
            { 
                if(m_IsDrity != value)
                {
                    m_IsDrity = value;

                    if (IsDebug)
                        Debug.Log($"{NetID}, {GetType().FullName}: IsDirty=>{value}");
                }
            } 
        }

        /// <summary>
        /// 是否远端数据为脏
        /// </summary>
        private bool m_IsRemoteDirty;
        public bool IsRemoteDirty 
        { 
            get => m_IsRemoteDirty; 
            private set
            {
                if(m_IsRemoteDirty != value)
                {
                    m_IsRemoteDirty = value;

                    if (IsDebug)
                        Debug.Log($"{NetID}, {GetType().FullName}: IsRemoteDirty=>{value}");
                }
            }
        }

        /// <summary>
        /// 是否已经启动了
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// 是否为调试模式
        /// </summary>
        private bool m_IsDebug;
        public bool IsDebug
        {
            get => m_IsDebug;
            set
            {
                m_IsDebug = value;
                m_IsPublicFlag.IsDebug = value;
            }
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }

        public void SetRemoteDirty()
        {
            IsRemoteDirty = true;
        }

        public void ClearRemoteDirty()
        {
            IsRemoteDirty = false;
        }

        public bool Create()
        {
            m_IsEnableSync = true;

            m_IsPublicFlag = SetupVar<NetVarBool>("IsPublicFlag", false, IsDebug);

            OnSetupVars();
            OnCreate();

            return true;
        }

        protected virtual void OnCreate()  { }

        public void SetupNetID(ulong netID)
        {
            m_ID.Set(netID);
        }

        public void SetupNetID(ulong netID, byte objIndex)
        {
            m_ID.Set(netID);
            m_ID.Set(m_ID.ClientID, m_ID.SerialNo, objIndex);
        }

        public void Start(bool isAddToManager = true)
        {
            if (IsStarted)
                Stop();

            IsStarted = true;
            OnStart();

            if(IsEnableSync)
                NetworkManager.Instance.AddObject(this);
        }

        public void ClearVarsValue()
        {
            foreach (var v in m_NetVars)
            {
                v.Clear();
                v.ClearRemoteValueDelta();
            }
        }

        public void SyncImmediately()
        {
            if((IsDirty || IsRemoteDirty) && IsStarted && IsEnableSync)
            {
                NetworkManager.Instance.SyncObjectImmediately(this);
            }
        }

        private void ClearVarsAll()
        {
            foreach (var v in m_NetVars)
            {
                v.RemoveAllChangeListeners();
                v.Clear();
                v.ClearRemoteValueDelta();
                v.ClearDirty();
                v.ClearRemoteValueDeltaDirty();
            }
        }

        private void ClearVarsDirty()
        {
            foreach (var v in m_NetVars)
            {
                v.Clear();
                v.ClearRemoteValueDelta();
                v.ClearDirty();
                v.ClearRemoteValueDeltaDirty();
            }
        }

        protected virtual void OnStart() { }

        public void Stop()
        {
            if(IsStarted)
            {
                NetworkManager.Instance.RemoveObject(this);
                OnStop();
            }

            ClearVarsDirty();

            IsRemoteDirty = false;
            IsDirty = false;
            IsStarted = false;
        }

        protected virtual void OnStop() { }

        protected abstract void OnSetupVars();

        public void Release()
        {
            Stop();
            ClearVarsAll();
            OnRelease();

            //回收变量对象
            foreach(var v in m_NetVars)
                LitePoolableObject.Recycle(v as LitePoolableObject);

            m_NetVars.Clear();
        }

        protected virtual void OnRelease() { }

        public T SetupVar<T>(string name = "", bool isPublic = false, bool isDebug = false) where T : LitePoolableObject, INetVar, new()
        {
            var v = LitePoolableObject.Instantiate<T>(this);

            v.IsPublic = isPublic;
            v.IsDebug = isDebug;
            v.Name = name;

            m_NetVars.Add(v);
            return v;
        }

        public virtual void OnNetVarRemoteChange(INetVar var)
        {
            if(var.IsRemoteValueDeltaDirty)
                IsRemoteDirty = true;
        }

        public virtual void OnNetVarChange(INetVar var)
        {
            if(var.IsDirty)
                IsDirty = true;
        }

    }

}
