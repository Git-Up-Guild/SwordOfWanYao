using UnityEngine;

namespace XClient.Network
{
    public class NetObjectBehaviour<T> : MonoBehaviour, INetObjectBehaviour where T: MonoNetObject, new()
    {
        private T m_NetObj;

        public T NetObj
        {
            get
            {
                if(m_NetObj == null)
                {
                    m_NetObj = new T();
                    m_NetObj.Create();
                    m_NetObj.NetObjBehaviour = this;
                    OnNetObjectCreate();
                }
                return m_NetObj;
            }
        }

        public INetObject GetNetObject()
        {
            return NetObj;
        }

        protected virtual void OnDestroy()
        {
            if(null!= m_NetObj)
            {
                m_NetObj.Stop();
                m_NetObj.NetObjBehaviour = null;
                m_NetObj.Release();
                m_NetObj = null;
            }
           
        }

        public bool IsOwner => NetworkManager.Instance.IsLocalClient(m_NetObj.NetID);

        public MonoBehaviour Mono => this;

        private bool m_IsNetObjectPublic = false;
        public bool IsNetObjectPublic 
        { 
            get=> m_IsNetObjectPublic; 
            set
            {
                m_IsNetObjectPublic = value;
                if(m_NetObj != null && m_NetObj.IsStarted)
                {
                    m_NetObj.IsPublic = value;
                }
            }
        }

        protected void Update()
        {
           // if(NetObj.IsStarted)
            {
                OnUpdate();
            }
        }

        protected virtual void OnNetObjectCreate()
        {
        }

        public virtual void OnNetObjectStart()
        {
        }

        public virtual void OnNetObjectStop()
        {
        }

        protected virtual void OnUpdate() { }


    }
}
