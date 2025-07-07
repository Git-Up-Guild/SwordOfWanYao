/*******************************************************************
** 文件名:	IEntityMovePart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  普通实体移动部件(非主角)接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity.Part;


namespace XClient.Entity
{
    public interface IEntityMovePart : IEntityPart
    {
        //是否增在移动
        bool IsMoving();

        //移动目标点
        Vector3 GetTargetPos();

        //移动目标
        void MoveTo(Vector3 target);
    }

}
