using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Skills/Area Skill", order = 12)]
public class AreaSkillData : SoldierSkillDataBase
{
    [Header("AOE 设置")]
    public GameObject AOEPrefab;
    public AOEShape AOEShape;

    [Tooltip("生成的 AOE 特效整体缩放倍率(1 = 原始大小)")]
    public float scaleMultiplier;
    public float duration;
    public int damagePerTick;
    public float tickInterval;
    public bool isAttachedToFollower;
}

public enum AOEShape
{
    Rectangle,
    Circle
}