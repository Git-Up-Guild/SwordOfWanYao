/*******************************************************************
** 文件名:	KingAttrModifier.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.15
** 版  本:	1.0
** 描  述:	
** 应  用:  生物属性修改器,通常是buff的effect需要修改

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using GameScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame.Utils;

namespace GameScripts.KingHero
{
    public class KingAttrModifier : Singleton<KingAttrModifier>, IATTRModifier
    {
        public int OnModifiedHP(ICreatureEntity entity, int hp)
        {
            return hp;
        }

        public int OnModifiedIntProp(int propID, int val)
        {
            return val;
        }

        public float OnModifiedSpeed(ICreatureEntity entity, float baseSpeed)
        {
            //发送攻击事件
            /*
            SEVENT_CALC_MOVE_SPEED context = SEVENT_CALC_MOVE_SPEED.Instance;
            context.srcID = entity.id;
            context.baseSpeed = baseSpeed;
            context.addtionSpeedCoff = 0;

            GameGlobalEx.EventEgnine.FireExecute(DGlobalEventEx.EVENT_CALC_MOVE_SPEED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, context);

            float result =  baseSpeed * (1 + (float)context.addtionSpeedCoff / 1000.0f);
            */
            //Debug.LogError("baseSpeed=" + baseSpeed + ", addtionSpeedCoff=" + context.addtionSpeedCoff);

            return baseSpeed;
            //return result;
        }

        //坐标转换
        public Vector3 WorldPositionToBattlePosition(Vector3 worldPosition, bool isMyBattle)
        {
            return worldPosition;

          //  var pos = GameGlobalEx.Battle.WorldPositionToBattlePosition(worldPosition, isMyBattle);
            //return GameGlobalEx.Battle.BattlePositionToWorldPosition(pos);
        }
    }
}

