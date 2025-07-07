/*******************************************************************
** 文件名:	MonsterDataPart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.02
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物同步数据

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XClient.Network;

public class MonsterDataPart : NetDataPart
{
    //血量
    public NetVarLong m_hp;

    //最大血量
    public NetVarLong m_maxHp;

    //阵营ID
    public NetVarLong m_camp;

    //初始化位置
    public NetVarVector3 m_pos;
    //初始化位置
    public NetVarVector3 m_localPos;

    //是否localPos
    public NetVarBool m_isLocalPos;

    //初始化朝向
    public NetVarVector3 m_forward;

    //显示方式
    public NetVarInt m_visibleType;

    //用户数据
    public NetVarInt userData;

    protected override void OnSetupVars()
    {
        m_hp = SetupVar<NetVarLong>();
        m_maxHp = SetupVar<NetVarLong>();
        m_camp = SetupVar<NetVarLong>();
        m_pos = SetupVar<NetVarVector3>();
        m_localPos = SetupVar<NetVarVector3>();
        m_isLocalPos = SetupVar<NetVarBool>();
        m_forward = SetupVar<NetVarVector3>();
        m_visibleType = SetupVar<NetVarInt>();
        userData = SetupVar<NetVarInt>();
    }
}