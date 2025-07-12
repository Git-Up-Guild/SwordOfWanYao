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

    [Header("兵种类型")]
    public SoldierType soldierType;
    public SoldierCamp camp;

    [Header("属性配置")]
    public SoldierAttributes attributes;

    [Header("技能配置")]
    public List<SoldierSkillDataBase> skills;
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

public enum SoldierType
{
    Blade, //刀战
    Spear, //长矛
    Archer, //弓箭
    FireMage, //火法
    WindPriest, //风祭司
    LightMonk, //光罗汉
    Grunt, //普通小怪
    Swiftbeak, //迅鸟怪
    IronWarlord, //全甲魔将
    DarkArcher, //魔弓手
    Exploder, //自爆怪
    PlagueHealer //魔疫医 远程
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

