using UnityEngine;
using System;
using System.Collections.Generic;

#region —— 士兵基础数据 ——
[CreateAssetMenu(menuName = "Soldier System/Soldier Data Base", order = 1)]
public class SoldierDataBase : ScriptableObject
{
    [Header("基础设置")]
    public int ID;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite portrait;
    public SoldierRarity rarity;
    public SoldierCamp camp;

    [Header("属性配置")]
    public SoldierAttributes attributes;

    [Header("技能配置")]
    public List<SoldierSkillData> skills;
}

public enum SoldierRarity
{
    Normal,
    Elite
}

public enum SoldierCamp
{
    Ally,
    Enemy,
    Neutral

}
#endregion

#region —— 属性模块 ——
[Serializable]
public class SoldierAttributes
{
    [Header("最大生命值")]
    public int maxHealth;

    [Header("攻击力乘算倍率")]
    public float attackPowerMutiplier = 1;

    [Header("防御力")]
    public int defense;

    [Header("移动速度")]
    public float moveSpeed;

    [Header("索敌范围")]
    public float lockOnRange;
    [Header("攻击范围")]
    public float attackRange;

    [Header("攻击速度")]
    public float attackSpeed;

    [Header("攻击次数")]
    public int attackFrequency;
    
}
#endregion

#region —— 技能基类与派生 ——
/// <summary>
/// 所有敌人技能的抽象基类，继承自 ScriptableObject，方便分别创建资产
/// </summary>
public abstract class SoldierSkillData : ScriptableObject
{
    [Header("通用技能设置")]
    public string skillName;
    public float castDelay;

    [Header("技能效果列表")]
    public List<SkillEffectData> effects;

    /// <summary>
    /// 技能类型，用于运行时做分支、筛选
    /// </summary>
    public SkillCategory category;
}

public enum SkillCategory
{
    Melee,         // 近战普通攻击
    Ranged,        // 远程投射物
    AreaOfEffect,  // 范围伤害
    SelfDestruct,  // 自爆怪专用
    Regeneration,  // 回复/回血
    Buff,          // 增益类
    Debuff         // 减益类
}
#endregion

#region —— 各类技能参数示例 ——

[CreateAssetMenu(menuName = "Soldier System/Skills/Melee Skill", order = 10)]
public class MeleeSkillData : SoldierSkillData
{
    [Header("近战设置")]
    public GameObject meleeEffect;
    public int damage;
    public float attackRadius;
}

[CreateAssetMenu(menuName = "Soldier System/Skills/Projectile Skill", order = 11)]
public class ProjectileSkillData : SoldierSkillData
{
    [Header("投射物设置")]
    public GameObject projectilePrefab;
    public float speed;
    public int maxPierce;   
    public int damagePerHit;
}

[CreateAssetMenu(menuName = "Soldier System/Skills/Area Skill", order = 12)]
public class AreaSkillData : SoldierSkillData
{
    [Header("AOE 设置")]
    public GameObject AOEPrefab;
    public float radius;
    public float duration;
    public int damagePerTick;
    public float tickInterval;
}

[CreateAssetMenu(menuName = "Soldier System/Skills/Self Destruct Skill", order = 13)]
public class SelfDestructSkillData : SoldierSkillData
{
    [Header("自爆设置")]
    public GameObject explosionPrefab;
    public float explosionRadius;
    public int explosionDamage;
    public float triggerDelay;
}

[CreateAssetMenu(menuName = "Soldier System/Skills/Regeneration Skill", order = 14)]
public class RegenerationSkillData : SoldierSkillData
{
    [Header("回血设置")]
    public int healAmount;
    public float healInterval;
    public float totalDuration;
}

[CreateAssetMenu(menuName = "Soldier System/Skills/Buff Skill", order = 15)]
public class BuffSkillData : SoldierSkillData
{
    [Header("增益 Buff 设置")]
    public BuffType buffType;
    public float buffValue;
    public float buffDuration;
}

public enum BuffType
{
    IncreaseAttack,
    IncreaseDefense,
    IncreaseSpeed,
    Shield
}

#endregion

// 拓展效果基类
public abstract class SkillEffectData : ScriptableObject
{
    public EffectType effectType;
    [Tooltip("触发几率(0-1)")]
    public float chance = 1f;
}

public enum EffectType
{
    Explosion,
    Split,
    Freeze,
    Wind,
    LightBeamRotation

}

public class ExplosionEffectData : SkillEffectData
{
    [Header("爆炸效果设置")]
    public GameObject explosionPrefab;
    public int damage;
    public float radius;
}

public class SplitEffectData : SkillEffectData
{
    [Header("分裂效果设置")]
    public int splitCount;
    public float splitDamage;
}

public class FreezeEffectData : SkillEffectData
{
    [Header("冰冻效果设置")]
    public float freezeDuration;
}

public class WindEffectData : SkillEffectData
{
    [Header("风眼效果设置")]
    public float pullStrength;
    public float moveSpeed;

}

public class LightBeamRotation : SkillEffectData
{
    [Header("光柱旋转效果设置")]
    public float angle;
    public float rotatingSpeed;

}