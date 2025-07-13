using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    private SoldierModel m_attacker;
    private int m_damage;
    private SoldierCamp m_camp;

    public void Init(SoldierModel attacker, int damage)
    {
        m_attacker = attacker;
        m_damage = damage;

        m_camp = m_attacker.Camp;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var target = other.GetComponentInParent<SoldierModel>();

        if (target == null) return;

        if (target.Camp != m_camp && !target.IsDead)
        {
            DamageApplyer.ApplyDamage(m_attacker, target, m_damage);
        }
    }
}