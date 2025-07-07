/*******************************************************************
** 文件名:	BuffBaseComponent.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.08
** 版  本:	1.0
** 描  述:	
** 应  用:  BuffBaseComponent相关组件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Network;
using XGame.Entity;
using XGame.Poolable;
using static XClient.Entity.BuffBaseComponent;

namespace XClient.Entity
{
    public class BuffBaseComponent : NetObjectBehaviour<BuffData>
    {
        public class BuffData : MonoNetObject
        {
            //是否在移动
            public NetVarLong m_version;

            //ClientID 
            public NetVarLong m_clentID;

            //buffer的添加列表
            public NetVarIntArray m_AddList;

            //buffer添加列表的ID
            public NetVarIntArray m_AddListSID;

            //buffer的移除列表
            public NetVarIntArray m_RemoveListID;

            protected override void OnSetupVars()
            {
                //IsDebug = true;

                m_version = SetupVar<NetVarLong>("m_version", true);
                m_clentID = SetupVar<NetVarLong>("m_clentID", true);
                m_AddList = SetupVar<NetVarIntArray>("m_AddList", true);
                m_AddListSID = SetupVar<NetVarIntArray>("m_AddListID", true);
                m_RemoveListID = SetupVar<NetVarIntArray>("m_RemoveListID", true);
                m_clentID.Value = 1; // (long)(GameGlobal.RoleAgent.id);
            }
        }

        //效果创建器
        public IEffectActionCreate effectActionCreate;

        //buff标识符
        public IDReco reco;

        //buff列表
        private List<IBuff> listBuffs = new List<IBuff>();

        //当前的最大ID
        private int m_nMaxID = 1;

        //上次的版本号
        private long m_lastVersion = 0;

        //删除列表
        static private List<IBuff> m_waitDelBuff = new List<IBuff>();

        //创建现场
        static private BuffCreateContext buffCerateContext = new BuffCreateContext();


        // Start is called before the first frame update
        public virtual void Awake()
        {
            if (null == reco)
            {
                reco = GetComponent<IDReco>();
            }

            //公开的NetObject对象
            IsNetObjectPublic = true;
        }

        public override void OnNetObjectStart()
        {
            base.OnNetObjectStart();


            /*
            if (!NetworkManager.Instance.IsLocalClient(NetObj.NetID))
            {
                AddBuff(30012);
            }
            */
        }


        //添加buff

        public virtual void AddBuff(int buffID, int layer =1)
        {
            IBuff buff = __CreateBuff(0, buffID, layer);
            if (null != buff)
            {
                buff.Start();
                listBuffs.Add(buff);

                //同步到远程
                int sid = ++m_nMaxID;
                buff.SetSID(sid);
                NetObj.m_version.Value = __MakeVersion(NetObj.m_version.Value);
                NetObj.m_clentID.Value = 0;
                NetObj.m_AddList.Value.Clear();
                NetObj.m_AddListSID.Value.Clear();
                NetObj.m_AddList.Value.Add(buffID);
                NetObj.m_AddListSID.Value.Add(sid);
                NetObj.m_RemoveListID.Value.Clear();

                NetObj.m_AddList.SetDirty();
                NetObj.m_AddListSID.SetDirty();
                NetObj.m_RemoveListID.SetDirty();

                m_lastVersion = NetObj.m_version.Value;
                NetObj.SyncImmediately();
            }
        }

        //移除buff
        public void RemoveBuff(int buffID)
        {
            IBuff buff = FindBuffByID(buffID);
            if (null != buff)
            {
                __NotifyRemoteRemove(buff);

                buff.Stop();
                listBuffs.Remove(buff);
                __RecycleBuff(buff);
            }
        }

        /*
        private void Start()
        {
            AddBuff(30012);
        }
        */

        override protected void OnUpdate()
        {
            //是否远程对象
            // if(NetObj.IsHasRight==false)
            {
                //有buff更新
                if (m_lastVersion != NetObj.m_version.Value)
                {
                    __UpdateRemoteBuff();
                }
            }

            m_waitDelBuff.Clear();
            int nCount = listBuffs.Count;
            for (int i = 0; i < nCount; ++i)
            {
                if (listBuffs[i].IsFinish() == false)
                {
                    listBuffs[i].OnUpdate();
                }
                else
                {
                    //删除列表
                    m_waitDelBuff.Add(listBuffs[i]);
                }
            }

            //删除列表
            nCount = m_waitDelBuff.Count;
            for (int i = 0; i < nCount; ++i)
            {
                __NotifyRemoteRemove(m_waitDelBuff[i]);
                m_waitDelBuff[i].Stop();
                __RecycleBuff(m_waitDelBuff[i]);
                listBuffs.Remove(m_waitDelBuff[i]);
            }
        }

        public void Reset()
        {
            //同步远程
            NetObj.m_version.Value = NetObj.m_version.Value + 1;
            NetObj.m_AddList.Value.Clear();
            NetObj.m_AddListSID.Value.Clear();
            NetObj.m_RemoveListID.Value.Clear();

            int nCount = listBuffs.Count;
            for (int i = 0; i < nCount; ++i)
            {
                NetObj.m_RemoveListID.Value.Add(listBuffs[i].GetSID());
                listBuffs[i].Stop();
                __RecycleBuff(listBuffs[i]);
            }

            listBuffs.Clear();


            NetObj.m_AddList.SetDirty();
            NetObj.m_AddListSID.SetDirty();
            NetObj.m_RemoveListID.SetDirty();
            NetObj.SyncImmediately();
        }

        //通过ID查找Buff
        protected IBuff FindBuffByID(int buffID)
        {
            int nCount = listBuffs.Count;
            for (int i = 0; i < nCount; ++i)
            {
                if (listBuffs[i].GetBuffID() == buffID)
                {
                    return listBuffs[i];
                }
            }

            return null;
        }

        //通过SID查找buff
        protected IBuff FindBuffBySID(long clientID, int sid)
        {
            int nCount = listBuffs.Count;
            for (int i = 0; i < nCount; ++i)
            {
                if (clientID == listBuffs[i].GetClientMaster() && listBuffs[i].GetSID() == sid)
                {
                    return listBuffs[i];
                }
            }

            return null;
        }

        //创建一个buff
        private IBuff __CreateBuff(long clientID, int buffID, int layer = 1)
        {
            //Debug.Log($"__CreateBuff = {buffID}");

            buffCerateContext.srcID = reco != null ? reco.entID : 0;
            buffCerateContext.buffID = buffID;
            buffCerateContext.buffLayer = layer;
            buffCerateContext.effectActionCreate = effectActionCreate;
            buffCerateContext.clientID = clientID;
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            return itemPoolMgr.Pop<Buff>(buffCerateContext);
        }

        //回收buff
        private void __RecycleBuff(IBuff buff)
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            itemPoolMgr.Push(buff);
        }

        private void OnDestroy()
        {
            Reset();

            listBuffs = null;
        }

        private void __UpdateRemoteBuff()
        {
            m_lastVersion = NetObj.m_version.Value;

            //删除
            List<int> listRemove = NetObj.m_RemoveListID.Value;
            int nCount = listRemove.Count;
            for (int i = 0; i < nCount; ++i)
            {
                IBuff buff = FindBuffBySID(NetObj.m_clentID.Value, listRemove[i]);
                if (null != buff)
                {
                    buff.Stop();
                    listBuffs.Remove(buff);
                    __RecycleBuff(buff);
                }
            }

            if (NetObj.m_AddList.Value.Count != NetObj.m_AddListSID.Value.Count)
            {
                Debug.LogError("NetObj.m_AddList.Value.Count!= NetObj.m_AddList.Value.Count");
                return;
            }

            List<int> listAdd = NetObj.m_AddList.Value;
            nCount = listAdd.Count;
            for (int i = 0; i < nCount; ++i)
            {
                IBuff buff = __CreateBuff(NetObj.m_clentID.Value, listAdd[i]);
                if (null != buff)
                {
                    buff.SetSID(NetObj.m_AddListSID.Value[i]);
                    buff.Start();
                    listBuffs.Add(buff);
                }
            }
        }

        //通知远程buff删除
        private void __NotifyRemoteRemove(IBuff buff)
        {
            //同步远程(本客户端添加的，本客户端通知)
            if (buff.GetClientMaster() == (long)0)
            {
                NetObj.m_version.Value = __MakeVersion(NetObj.m_version.Value);
                NetObj.m_clentID.Value = (long)0;
                NetObj.m_AddList.Value.Clear();
                NetObj.m_AddListSID.Value.Clear();
                NetObj.m_RemoveListID.Value.Clear();
                NetObj.m_RemoveListID.Value.Add(buff.GetSID());

                NetObj.m_AddList.SetDirty();
                NetObj.m_AddListSID.SetDirty();
                NetObj.m_RemoveListID.SetDirty();

                NetObj.SyncImmediately();
            }
        }

        private long __MakeVersion(long ver)
        {
            if (ver == 0)
            {
                ver = (long)(0 & 0xFFFFFFFF00000000);
            }

            ver += 1;
            return ver;
        }
    }
}