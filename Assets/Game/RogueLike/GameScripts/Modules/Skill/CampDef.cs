using GameScripts;
using GameScripts.KingHero;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;



//阵营定义
public class BATTLE_CAMP_DEF
{
    //刷新怪物的阵营
    public const uint BATTLE_CAMP_MONSTER = 1;

    //刷新棋子的阵营
    public const uint BATTLE_CAMP_PIECE = 2;

    //城墙阵营
    public const uint BATTLE_CAMP_WALL = 3;
}

public class CampDef 
{
    //获取当前客户端的阵营
    public static ulong GetLocalCamp(ulong camp)
    {
        ulong localCamp = camp;
        localCamp = (localCamp << 32) | (GameGlobal.RoleAgent.id>>32);
        return localCamp;
    }

    //获取远端的阵营
    public static ulong GetRemoteCamp(ulong camp)
    {
        ulong remoteCamp = camp;
       // remoteCamp = (remoteCamp << 32) | (GameGlobalEx.Battle.Data.OpponentID>>32);
        return remoteCamp;
    }

    //是否本地生物的阵营
    public static bool IsLocalCamp(ulong camp)
    {
        ulong roleCamp = GameGlobal.RoleAgent.id >> 32;
        camp = camp&0x00000000FFFFFFFF;

        return roleCamp== camp;
    }
}
