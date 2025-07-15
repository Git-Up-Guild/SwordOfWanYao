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
        var model = other.GetComponentInParent<SoldierModel>();

        if (model != null && model.Camp != m_attacker.Camp)
            {
                DamageApplyer.ApplyDamage(m_attacker, model, m_damage, model.transform.position);
            }
            else
            {
                var destructible = other.GetComponent<IDestructible>();
                if (destructible != null && destructible.GetCamp() != m_attacker.Camp)
                {
                    destructible.TakeDamage(m_damage, m_attacker);
                }
            }
    }
}