/*******************************************************************
** 文件名:	IEffectAction.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.08
** 版  本:	1.0
** 描  述:	
** 应  用:  效果相关接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Poolable;


namespace XClient.Entity
{
    public interface IEffectAction : IPoolable
    {
        //开始
        void Start();

        //停止
        void Stop();

        //释放已经完成
        bool IsFinish();

        //推动更新
        void OnUpdate();
    }
}

