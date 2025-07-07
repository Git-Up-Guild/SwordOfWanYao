/*******************************************************************
** 文件名:	IMonster.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物实体接口

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
    
    public enum EMonsterAni
    {
        Idle,
        BattleIdle,
        Move,
        Dead,
        Type2SkillAct,
        Attack,
        StartBattle,
        ExitBattle,
        Dodge,
    }

    public class MonsterAniHelper
    {
        public static string GetAniName(EMonsterAni aniType)
        {
            switch (aniType)
            {
                case EMonsterAni.Idle:
                    return "idle";
                case EMonsterAni.BattleIdle:
                    return "idle_atk";
                case EMonsterAni.StartBattle:
                    return "begin";
                case EMonsterAni.Move:
                    return "move";
                case EMonsterAni.Dead:
                    return "die";
                case EMonsterAni.ExitBattle:
                    return "fall";
                case EMonsterAni.Attack:
                    return "atk";
                case EMonsterAni.Type2SkillAct:
                    return "shifa";
                case EMonsterAni.Dodge:
                    return "shanbi";
                default:
                    return "idle";
            }
        }
    }
    
    public interface IMonster: ICreatureEntity
    {

    }

}


