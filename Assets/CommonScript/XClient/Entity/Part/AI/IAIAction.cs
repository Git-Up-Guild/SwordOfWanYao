/*******************************************************************
** 文件名:	IAAction.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.26
** 版  本:	1.0
** 描  述:	
** 应  用:  AI行为的基类

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

    //AI的类型行为
    public enum AI_ACTION_TYPE
    {
        AI_ACTION_MOVE_FORWARD = 1,//向前移动
        AI_ACTION_CIRCLE_SKILL,//循环施法
        AI_ACTION_COLLISION_EXPLOSION,//碰撞自爆
        AI_ACTION_ROUTE_MOVE,//根据路由点移动
    }

    //AI行为创建器
    public interface IAIActionCreator
    {
        //创建
        IAIAction CreateAIAction(int id, object context);

        //释放对象
        void ReleaseAIAction(IAIAction action);

    }

    //AI的类型接口
    public interface IAIAction: IPoolable
    {

        //设置AI的拥有者
        void SetMaster(ICreatureEntity master);

        //开始启用Action
        void Start();

        //开始启用Action
        void Stop();


        //获取优先级
        int GetPriority();

        //更新
        bool OnExeUpdate();

        //转发实体消息
        void OnReceiveEntityMessage(uint id, object data = null);
    }


}


