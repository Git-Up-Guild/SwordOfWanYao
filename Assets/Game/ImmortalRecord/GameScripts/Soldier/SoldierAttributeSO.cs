using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/Runtime Attribute Template")]
public class SoldierAttributeSO : ScriptableObject
{
    public float maxHealth;
    public float attackPowerMutiplier = 1;
    public float defense;
    public float moveSpeed;
    public float lockOnRange;
    public float attackRange;
    public float attackSpeed;
    public int attackFrequency;
    public int projectileCount;

    public void CopyFrom(SoldierAttributes src)
    {
        maxHealth = src.maxHealth;
        attackPowerMutiplier = src.attackPowerMutiplier;
        defense = src.defense;
        moveSpeed = src.moveSpeed;
        lockOnRange = src.lockOnRange;
        attackRange = src.attackRange;
        attackSpeed = src.attackSpeed;
        attackFrequency = src.attackFrequency;
        projectileCount = src.projectileCount;
    }
}