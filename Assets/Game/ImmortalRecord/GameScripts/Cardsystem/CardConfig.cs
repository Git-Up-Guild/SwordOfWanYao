using UnityEngine;
using System.Collections.Generic; // 引入列表

// 1. 定义一个新的枚举，用来区分卡牌类型
public enum CardType
{
    UnlockUnit, // 解锁新兵种的卡
    UnitSkill,  // 强化兵种的技能卡
    GlobalBuff  // 全局增益卡
}


[CreateAssetMenu(fileName = "NewCard", menuName = "Card Config")]
public class CardConfig : ScriptableObject
{
    public int ID;
    public Sprite ICON;
    public string Name;
    [TextArea] public string Description;

    [Header("卡牌机制")]
    public CardType Type; // 2. 添加卡牌类型字段
    public EffectBase Effect;
    public int MaxOwnable = 6;

    [Header("解锁条件 (仅对技能卡有效)")]
    // 3. 添加解锁条件字段
    // 这个列表里存放了“需要哪些兵种被解锁后，这张卡才能出现”
    // 注意：这里我们直接用 UnitData 来做关联，更直观
    public List<CardConfig> RequiredUnlockCards;
}
