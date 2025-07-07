/*******************************************************************
** 文件名:	BulletDataPart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.02
** 版  本:	1.0
** 描  述:	
** 应  用:  子弹的远程同步数据类

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using XClient.Entity;
using XClient.Network;

public class BulletDataPart : NetDataPart
{
    //血量
    public NetVarLong m_hp;

    //血量
    public NetVarLong m_maxHp;

    //阵营ID
    public NetVarLong m_camp;

    //初始化位置
    public NetVarVector3 m_pos;

    public NetVarVector3 m_localPos;

    //是否localPos
    public NetVarBool m_isLocalPos;

    //初始化朝向
    public NetVarVector3 m_forward;

    //发射器
    public NetVarLong m_netLauncherObjectID;

    //显示方式
    public NetVarInt m_visibleType;

    protected override void OnSetupVars()
    {
        m_hp = SetupVar<NetVarLong>();
        m_maxHp = SetupVar<NetVarLong>();
        m_camp = SetupVar<NetVarLong>();
        m_pos = SetupVar<NetVarVector3>();
        m_localPos = SetupVar<NetVarVector3>();
        m_isLocalPos = SetupVar<NetVarBool>();
        m_forward = SetupVar<NetVarVector3>();
        m_netLauncherObjectID = SetupVar<NetVarLong>();
        m_visibleType = SetupVar<NetVarInt>();
    }
}