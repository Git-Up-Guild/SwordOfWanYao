/*******************************************************************
** 文件名:	ICollisionSink.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.27
** 版  本:	1.0
** 描  述:	
** 应  用:  碰撞回调

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XClient.Entity
{
    public interface ICollisionSink
    {
        //碰撞到实体对象
        void OnCollision(IDReco reco);
    }

}


