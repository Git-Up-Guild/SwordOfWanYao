/*******************************************************************
** 文件名:	NetVar.cs
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
using UnityEngine.Events;
using XGame.Poolable;
using XGame.Utils;
using static XClient.Network.NetVarValue;

namespace XClient.Network
{
    /// <summary>
    /// 一个网络变量值
    /// </summary>
    public class NetVarValue : LitePoolableObject
    {
        public enum ValueUseage
        {
            Value = 0,
            RemoteValueDelta,
        }

        public long lValue;
        public float fValue;
        public string sValue;
        public Vector3 vec3Value;
        public List<int> intArrValue;
        public List<float> floatArrValue;

        protected override void OnInit(object context = null)
        {
            intArrValue = new List<int>();
            floatArrValue = new List<float>();
        }

        protected override void OnRecycle()
        {
            sValue = string.Empty;
            intArrValue.Clear();
            floatArrValue.Clear();
        }
    }

    /// <summary>
    /// 网络变量，网络变量具有主动同步的能力，修改网络变量能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract  class NetVar<T> : LitePoolableObject, INetVar
    {
        public static IDebugEx Debug => NetworkManager.Debug;

        public class ChangeEvent : UnityEvent<T,T> { }
        public ChangeEvent OnChange = new ChangeEvent();

        public class DirtyEvent : UnityEvent<bool> { }
        public DirtyEvent OnDirty = new DirtyEvent();
        public DirtyEvent OnRemoteDirty = new DirtyEvent();

        /// <summary>
        /// 所属的网络对象
        /// </summary>
        public INetable Owner { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否为调试模式
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// 所属的网络对象ID
        /// </summary>
        public ulong OwnerID => Owner.NetID;

        public bool IsPublic { get; set; }

        public bool IsHasRight => IsPublic || Owner.IsHasRight;

        public NetVar() 
        { 
        }

        public NetVar(INetable owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// 设置拥有者ID
        /// </summary>
        /// <param name="netId"></param>
        public void SetOwner(INetable owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// 初始化回调
        /// </summary>
        /// <param name="context"></param>
        protected override void OnInit(object context = null)
        {
            Owner = context as INetable;
        }

        /// <summary>
        /// 是否为脏，脏了才需要同步
        /// </summary>
        private bool m_IsDirty;
        public bool IsDirty
        {
            get => m_IsDirty; 
            private set
            {
                if (m_IsDirty != value)
                {
                    m_IsDirty = value;

                    if(IsDebug)
                        NetworkManager.Debug.Log($"NetID={OwnerID}, {Name}: m_IsDirty=>{value}");

                    OnDirty?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// 真实的值
        /// </summary>
        protected T m_Value;

        /// <summary>
        /// 对远程变量的增量修改值
        /// </summary>
        protected T m_RemoteValueDelta;

        /// <summary>
        /// 变量增量修改的是变量的增量
        /// </summary>
        public virtual T RemoteValueDelta
        {
            get
            {
                if (IsHasRight)
                    return default(T);
                else
                    return m_RemoteValueDelta;
            }
            set
            {
                if(IsHasRight)
                {
                    m_RemoteValueDelta = default(T);

                    OnLocalDeltaValue(value);

                    Owner.OnNetVarChange(this);
                }
                else
                {
                    if (!IsEqual(m_RemoteValueDelta, value))
                    {
                        if (IsDebug)
                            NetworkManager.Debug.Log($"NetID={OwnerID}, {Name}: RemoteValueDelta=>{ValueString(value)}");

                        OnSetNewValue(value, ValueUseage.RemoteValueDelta);

                        IsRemoteValueDeltaDirty = true;

                        Owner.OnNetVarRemoteChange(this);
                    }
                }
            }
        }

        /// <summary>
        /// 同步值
        /// </summary>
        public T SyncValue
        {
            set
            {
                if (!IsEqual(m_Value, value))
                {
                    if (IsDebug)
                        NetworkManager.Debug.Log($"NetID={OwnerID}, {Name}: {ValueString(m_Value)}=>{ValueString(value)}");

                    var oldValue = m_Value;

                    OnSetNewValue(value, ValueUseage.Value);

                    Owner.OnNetVarChange(this);

                    OnChange.Invoke(oldValue, value);
                }
            }
        }

        /// <summary>
        /// 获取或者修改值
        /// </summary>
        public T Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (!IsEqual(m_Value, value))
                {
                    if (IsDebug)
                        NetworkManager.Debug.Log($"NetID={OwnerID}, {Name}: {ValueString(m_Value)}=>{ValueString(value)}, IsHasRight:{IsHasRight}");

                    var oldValue = m_Value;

                    OnSetNewValue(value, ValueUseage.Value);

                    //有权限的就设置脏标记，只有为脏的变量才会被同步
                    if (IsHasRight)
                        IsDirty = true;

                    //先回调出去，将对象标记为脏
                    Owner.OnNetVarChange(this);

                    OnChange.Invoke(oldValue, value);
                }
            }
        }

        protected virtual void OnSetNewValue(T newValue, ValueUseage useage)
        {
            if (useage == ValueUseage.Value)
                m_Value = newValue;
            else if (useage == ValueUseage.RemoteValueDelta)
                m_RemoteValueDelta = newValue;
            else
                return;
        }

        protected virtual void OnLocalDeltaValue(T deltaValue) { }

        protected virtual bool IsEqual(T val1, T val2)
        {
            return val1.Equals(val2);
        }

        public abstract NetVarDataType DataType { get; }

        /// <summary>
        /// 远端增量数据是否为脏
        /// </summary>
        private bool m_IsRemoteValueDeltaDirty;
        public bool IsRemoteValueDeltaDirty
        {
            get => m_IsRemoteValueDeltaDirty;
            private set
            {
                if (m_IsRemoteValueDeltaDirty != value)
                {
                    m_IsRemoteValueDeltaDirty = value;
                    OnRemoteDirty?.Invoke(value);
                }
            }
        }

        public void SetDirty()
        {
            IsDirty = true;
            Owner?.OnNetVarChange(this);
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }

        public void RemoveAllChangeListeners()
        {
            if (IsDebug)
                Debug.Log($"{Name}=>RemoveAllListeners");

            OnChange?.RemoveAllListeners();
        }

        public virtual void Read(INetVarSerializer serializer)  {  }

        public virtual void Write(INetVarSerializer serializer)  { }

        public void AddDirtyListener(UnityAction<bool> listener)
        {
            OnDirty.AddListener(listener);
        }

        public void RemoveDirtyListener(UnityAction<bool> listener)
        {
            OnDirty.RemoveListener(listener);
        }

        public void RemoveAllDirtyListeners()
        {
            OnDirty.RemoveAllListeners();
        }

        public void ClearRemoteValueDelta()
        {
            m_RemoteValueDelta = default(T);

            if (IsDebug)
                    NetworkManager.Debug.Log($"ClearRemoteValueDelta# NetID={OwnerID}, {Name}: RemoteValueDelta=>{ValueString(m_RemoteValueDelta)}");
        }

        public virtual void Clear()
        {
            m_Value = default(T);

            if (IsDebug)
                NetworkManager.Debug.Log($"Clear# NetID={OwnerID}, {Name}: Value=>{ValueString(m_Value)}");
        }

        public void Set(INetVar netVar)
        {
            if (netVar == null)
                return;

            if (DataType != netVar.DataType)
            {
                NetworkManager.Debug.Error($"NetID={OwnerID} 赋值失败，类型不一致！");
                return;
            }

            Value = (netVar as NetVar<T>).Value;
        }

        public abstract void Write(NetVarValue varValue);

        public abstract void Read(NetVarValue varValue);

        protected override void OnRecycle()
        {
            RemoveAllChangeListeners();
            RemoveAllDirtyListeners();
            RemoveAllRemoteDirtyListeners();
            Clear();
            ClearRemoteValueDelta();
            ClearDirty();
            ClearRemoteValueDeltaDirty();
        }

        public void SetRemoteValueDeltaDirty()
        {
            IsRemoteValueDeltaDirty = true;
            Owner?.OnNetVarRemoteChange(this);
        }

        public void ClearRemoteValueDeltaDirty()
        {
            IsRemoteValueDeltaDirty = false;
        }

        public void AddRemoteDirtyListener(UnityAction<bool> listener)
        {
            OnRemoteDirty.AddListener(listener);
        }

        public void RemoveRemoteDirtyListener(UnityAction<bool> listener)
        {
            OnRemoteDirty.RemoveListener(listener);
        }

        public void RemoveAllRemoteDirtyListeners()
        {
            OnRemoteDirty.RemoveAllListeners();
        }

        public virtual void ReadRemoteValueDelta(INetVarSerializer serializer) { }

        public virtual void WriteRemoteValueDelta(INetVarSerializer serializer) { }

        public override string ToString()
        {
            return $"NetID={OwnerID}, Name={Name}, Value={ValueString(Value)}, RemoteValueDelta={RemoteValueDelta}, IsDrity={IsDirty}, IsRemoteValueDeltaDirty={IsRemoteValueDeltaDirty}";
        }

        public virtual string ValueString(T val)
        {
            if (val == null)
                return string.Empty;
            return val.ToString();
        }
    }

}
