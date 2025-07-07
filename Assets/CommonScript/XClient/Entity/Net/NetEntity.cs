
using System.Collections.Generic;
using UnityEngine;
using XClient.Network;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity.Net
{
    public class NetEntity : BaseEntity, INetEntity
    {
        private List<INetObjectBehaviour> m_NetObjs;

        protected List<INetObjectBehaviour> netObjs => m_NetObjs;

        public ulong netId { get; private set; }

        public bool isHasRight => NetworkManager.Instance.IsHasRight(netId);

        /// <summary>
        /// 校验初始化现场是否有效
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override bool ValidatInitContext(object context)
        {
            if (context == null)
                return false;

            var ctx = context as NetEntityShareInitContext;
            if (ctx != null)
                return true;

            netId = id;

            Debug.LogError("NetEntity对象的初始化现场必须由NetEntityShareInitContext来设置！");

            return false;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_NetObjs = new List<INetObjectBehaviour>();
        }

        protected override void OnReset()
        {
            ClearNetObjects();
            base.OnReset();
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);

            //导入网络数据
            NetEntityShareInitContext shareCtx = context as NetEntityShareInitContext;
            if (shareCtx != null && shareCtx.netInitContext != null)
            {
                var netDataPart = GetPart(EntityPartInnerType.Data) as NetDataPart;
                if (netDataPart != null)
                {
                    shareCtx.netInitContext.netDataSerializer.Unserializer(netDataPart);
                }
            }
        }

        public override void SendEntityMessage(uint msgId, object data = null)
        {
            if (msgId == EntityMessageID.ResLoaded)
            {
                var prefab = data as IPrefabResource;
                if(prefab == null)
                {
                    Debug.LogError($"预制体资源部件，需要继承 IPrefabResource 接口，且作为 EntityMessageID.ResLoaded 消息的现场！");
                    return;
                }

                if(m_NetObjs.Count > 0)
                {
                    Debug.LogError($"一个实体只能有一个加载预制体的部件！");
                    return;
                }
                
                prefab.gameObject.GetComponentsInChildren<INetObjectBehaviour>(m_NetObjs);

                for (var i = 0; i < m_NetObjs.Count; i++)
                {
                    var obj = m_NetObjs[i];
                    var netObj = obj.GetNetObject();
                    netObj.SetupNetID(id, (byte)(i + 10)); //0-9给NetDataPart来使用
                    netObj.Start();
                }

            }
            else if (msgId == EntityMessageID.ResUnloaded)
            {
                ClearNetObjects();
            }

            base.SendEntityMessage(msgId, data);

        }

        private void ClearNetObjects()
        {
            foreach (var obj in m_NetObjs)
            {
                var netObj = obj.GetNetObject();
                netObj?.Stop();
            }
            m_NetObjs.Clear();
        }
    }
}
