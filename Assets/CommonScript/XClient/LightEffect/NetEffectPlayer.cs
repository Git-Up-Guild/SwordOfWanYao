/*******************************************************************
** 文件名:	NetEffectPlayer.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.01
** 版  本:	1.0
** 描  述:	
** 应  用:  网络光效播放器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.LightEffect;
using XClient.Network;
using XGame.Entity.Part;
using static NetEffectPlayer;


public class NetEffectPlayer : NetObjectBehaviour<NetEffectPlayerData>
{

    public class NetEffectPlayerData : MonoNetObject
    {
        //播放的位置
        public NetVarVector3 m_pos;

        //播放时间
        public NetVarFloat m_duration;

        //资源路径
        public NetVarString m_path;

        //播放的根部0，就是空
        public NetVarLong m_parentNetID;

        //光效版本号，有变更的就播放一个新的
        public NetVarLong m_ver;


        protected override void OnSetupVars()
        {
            //IsDebug = true;
            m_ver = SetupVar<NetVarLong>("m_ver");
            m_pos = SetupVar<NetVarVector3>("m_pos");
            m_duration = SetupVar<NetVarFloat>("m_duration");
            m_path = SetupVar<NetVarString>("m_path");
            m_parentNetID = SetupVar<NetVarLong>("m_parentID");
        }
    }

    private long m_lastVer = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    protected override void OnNetObjectCreate()
    {
        base.OnNetObjectCreate();

        NetObj.m_ver.OnChange.AddListener((o, v) =>
        {
            OnUpdate();
        });
    }

    //播放特效
    public void PlayEffect(string path, ref Vector3 pos, float duration, long parentNetID)
    {

        NetObj.m_pos.Value = pos;
        NetObj.m_duration.Value = duration;
        NetObj.m_path.Value = path;
        NetObj.m_parentNetID.Value = parentNetID;
        NetObj.m_ver.Value = NetObj.m_ver.Value + 1;
        NetObj.SyncImmediately();

    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (m_lastVer != NetObj.m_ver.Value)
        {
            m_lastVer = NetObj.m_ver.Value;

            if (string.IsNullOrEmpty(NetObj.m_path.Value) == false)
            {
                Vector3 pos = NetObj.m_pos.Value;
                Transform parent = null;
                if (NetObj.m_parentNetID.Value > 0)
                {
                    NetObject netObject = NetObjectManager.Instance.GetObject((ulong)NetObj.m_parentNetID.Value);
                    if (null != netObject)
                    {
                        MonoNetObject mo = netObject as MonoNetObject;
                        if (mo != null)
                        {
                            parent = mo.Mono.transform;
                        }

                    }
                }
                EffectMgr.Instance().PlayEffect(NetObj.m_path.Value, ref pos, NetObj.m_duration.Value, parent);
            }
        }
    }
}
