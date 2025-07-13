using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Skills/Melee Skill", order = 10)]
public class MeleeSkillData : SoldierSkillDataBase
{
    [Header("近战设置")]
    public int damage;
    public float attackRadius;

}