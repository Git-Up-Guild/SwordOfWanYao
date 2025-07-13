using UnityEngine;
using System.Collections.Generic;

public class SoldierSkillDataBase : ScriptableObject
{

    [Header("通用技能设置")]
    public string skillName;
    public bool isAutoReleased;

    [Header("技能效果列表")]
    public List<SkillEffectDataBase> effects;

    public SkillCategory category;

}

public enum SkillCategory
{
    Melee,         // 近战普通攻击
    Projectile,    // 远程投射物
    AreaOfEffect,  // 范围伤害
    SelfDestruct,  // 自爆怪专用
    Regeneration,  // 回复/回血
    Buff,          // 增益类
    Debuff         // 减益类
}
