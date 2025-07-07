using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Network
{
    /// <summary>
    /// 嵌入式的网络对象
    /// </summary>
    public abstract class MonoNetObject : NetObject
    {
        public INetObjectBehaviour NetObjBehaviour { get; set; }
        public MonoBehaviour Mono => NetObjBehaviour.Mono;

        /// <summary>
        /// 不是拥有者的网络对象需要与远端的的网络对象建立好连接后，才能进行正常的处理
        /// 否则一旦远端的数据同步过来，会覆盖掉本地的修改。
        /// </summary>
        public bool IsConnected { get; private set; }

        public void SetConnectComplete()
        {
            if(!IsConnected)
            {
                IsConnected = true;

                if(IsDebug)
                    NetworkManager.Debug.Log($"{NetID} {GetType().FullName} 连接完成！");

                if(IsStarted)
                {
                    NetObjBehaviour.OnNetObjectStart();
                }
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            NetObjBehaviour = null;
            IsConnected = false;
        }

        protected override void OnStop()
        {
            base.OnStop();
            NetObjBehaviour?.OnNetObjectStop();
            IsConnected = false;
        }

        protected override void OnStart()
        {
            base.OnStart();

            //拥有者直接是连接状态
            if (IsOwner)
                IsConnected = true;

            //是否为公开对象
            IsPublic = NetObjBehaviour.IsNetObjectPublic;

            //看是否存在虚拟对象，如果存在则从虚拟对象中读取数据
            var virtualNetObj = VirtualNetObjectManager.Intance.GetObj(NetID);
            if (virtualNetObj != null)
            {
                //将虚拟对象中的数据导入
                for(var i = 0; i < virtualNetObj.NetVarValues.Count; i++)
                {
                    var v = virtualNetObj.NetVarValues[i];
                    if(v != null)
                    {
                        var localVar = NetVars[i];
                        localVar.Read(v);
                    }
                }

                //导入后，虚拟对象要被销毁掉
                VirtualNetObjectManager.Intance.DestroyObj(NetID);

                IsConnected = true;
            }

            if(IsConnected)
            {
                NetObjBehaviour.OnNetObjectStart();
            }
            else
            {
                if (IsDebug)
                    NetworkManager.Debug.Log($"{NetID} {GetType().FullName} 等待连接！");
            }
            
        }
    }
}
