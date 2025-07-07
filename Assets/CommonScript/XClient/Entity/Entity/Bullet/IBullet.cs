/*******************************************************************
** 文件名:	IBullet.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  子弹实体接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity;

namespace XClient.Entity
{
    //子弹类型
    public class BulletType
    {
        public const int NoShowBullet = 1; // 1：无弹道无特效类型
        public const int TrajectoryBullet = 2; // 2：有弹道类型
        public const int NoTrajectoryBullet = 3; // 3：无弹道有特效类型
    }

    //朝向类型
    public class BulletForwardType
    {
        public readonly static int forwardFront = 0; //朝前
        public readonly static int forwardTarget = 2; //朝向目标
    }


    public interface IBullet : ICreatureEntity
    {
        //弹射到下一个目标
        bool Jump2NextTarget(List<IDReco> filters);
    }
}