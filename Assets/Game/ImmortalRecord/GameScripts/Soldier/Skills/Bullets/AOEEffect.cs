using System.Collections;
using UnityEngine;

public class AOEEffect : MonoBehaviour
{
    private SoldierModel m_attacker;
    private float m_tickInterval;
    private float m_duration;
    private int m_damage;
    private AOEShape m_shape;

    private Collider2D m_collider;

    public void Init(SoldierModel attacker, int damage, float tickInterval, float duration, AOEShape shape)
    {
        m_attacker = attacker;
        m_damage = damage;
        m_tickInterval = tickInterval;
        m_duration = duration;
        m_shape = shape;

        m_collider = GetComponent<Collider2D>();

        StartCoroutine(DamageLoop());
        Destroy(gameObject, duration);
    }

    private IEnumerator DamageLoop()
    {
        float elapsed = 0;
        while (elapsed < m_duration)
        {
            DealDamage();
            yield return new WaitForSeconds(m_tickInterval);
            elapsed += m_tickInterval;
        }
    }

    private void DealDamage()
    {

        Collider2D[] hits = null;

        switch (m_shape)
        {
            case AOEShape.Circle:
                var circle = m_collider as CircleCollider2D;
                if (circle != null)
                {
                    Vector2 center = (Vector2)transform.position + circle.offset;
                    hits = Physics2D.OverlapCircleAll(center, circle.radius * transform.lossyScale.x);
                }
                break;

            case AOEShape.Rectangle:
                var box = m_collider as BoxCollider2D;
                if (box != null)
                {
                    Vector2 center = (Vector2)transform.position + box.offset;
                    Vector2 size = Vector2.Scale(box.size, transform.lossyScale);
                    hits = Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z);
                }
                break;
        }

        if (hits == null) return;

        foreach (var hit in hits)
        {
            SoldierModel model = hit.GetComponentInParent<SoldierModel>();
            if (model != null && model.Camp != m_attacker.Camp)
            {
                DamageApplyer.ApplyDamage(m_attacker, model, m_damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
    }
}
