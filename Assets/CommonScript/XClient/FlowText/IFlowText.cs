using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XClient.Common;
using XGame;
using XGame.Atlas;
using XGame.FlowText;

namespace XClient.FlowText
{
    public enum EFlowTextType
    {
        SystemTips = 1, //系统飘字
        ItemFlowText = 2, //物品飘字
        BattleDmgNormal = 101, //普通伤害
        BattleDmgNormal_Fatal = 102, //普通伤害暴击
        BattleDmgTrue = 103, //真实伤害
        BattleDmgTrue_Fatal = 104, //真实伤害暴击
        BattleDmgFlash = 105, //闪电伤害
        BattleDmgFlash_Fatal = 106, //闪电伤害暴击
        BattleDmgPoison = 107, //毒伤害
        BattleDmgPoison_Fatal = 108, //毒伤害暴击
        BattleHeal = 120, //治疗
        BattleMiss = 200, //闪避
        BattleStun = 201, //击晕
        BattleCombo = 202, //连击
        BattleCounterAtk = 203, //反击
        BattleNuqiji = 204, //怒气技
        BattleBuff = 301, //buff瓢字
    }
}