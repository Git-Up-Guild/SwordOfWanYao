using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Skills/Regeneration Skill", order = 14)]
public class RegenerationSkillData : SoldierSkillDataBase
{
    [Header("回血设置")]
    public int healAmount;
    public float healInterval;
}