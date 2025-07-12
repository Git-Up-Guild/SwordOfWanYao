using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Skills/Projectile Skill", order = 11)]
public class ProjectileSkillData : SoldierSkillDataBase
{
    [Header("投射物设置")]
    public GameObject projectilePrefab;
    public float moveSpeed;
    public int maxPierce;
    public int damagePerHit;
}