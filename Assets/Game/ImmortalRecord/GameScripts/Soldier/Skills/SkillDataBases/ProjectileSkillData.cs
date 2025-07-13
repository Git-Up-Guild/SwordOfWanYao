using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Skills/Projectile Skill", order = 11)]
public class ProjectileSkillData : SoldierSkillDataBase
{
    [Header("投射物设置")]
    public GameObject projectilePrefab;
    public float moveSpeed;
    public int maxPierce;
    public int damagePerHit;

    [Header("爆炸效果设置")]
    public bool canExplode;
    public GameObject explosionPrefab;
    public int damage;
    public float scaleMutiplier;

    [Header("分裂效果设置")]
    public GameObject splitBulletPrefab;
    public bool canSplit;
    public int lockOnRange;

}