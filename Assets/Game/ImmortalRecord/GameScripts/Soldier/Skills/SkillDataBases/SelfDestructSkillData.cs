using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Skills/Self Destruct Skill", order = 13)]
public class SelfDestructSkillData : SoldierSkillDataBase
{
    [Header("自爆设置")]
    public GameObject explosionPrefab;
    public float explosionRadius;
    public int explosionDamage;
    public float triggerDelay;
}