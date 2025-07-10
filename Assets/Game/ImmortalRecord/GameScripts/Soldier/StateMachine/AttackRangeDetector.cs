using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackRangeDetector : MonoBehaviour
{
    public event Action<Transform> OnEnemyEnter;
    public event Action<Transform> OnEnemyExit;

    private SoldierModel m_model;
    private string m_enemyTag;
    private readonly List<Transform> m_targetsInRange = new List<Transform>();
    
    //检查间隔
    private float targetCheckInterval = 0.2f;
    private float lastCheckTime;

    public void Init()
    {
        m_model = GetComponentInParent<SoldierModel>();

        m_enemyTag = (m_model.Camp == SoldierCamp.Ally) ? "Enemy" : "Ally";

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (Time.time - lastCheckTime > targetCheckInterval)
        {
            m_targetsInRange.RemoveAll(t => t == null);
            lastCheckTime = Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(m_enemyTag)) return;

        var t = other.transform;
        if (!m_targetsInRange.Contains(t))
        {
            m_targetsInRange.Add(t);
            OnEnemyEnter?.Invoke(t);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(m_enemyTag)) return;

        var t = other.transform;
        if (m_targetsInRange.Remove(t))
            OnEnemyExit?.Invoke(t);
    }

    public Transform GetNearestEnemy()
    {
        Transform nearest = null;
        float minSqr = float.MaxValue;
        Vector2 selfPos = transform.position;

        foreach (var t in m_targetsInRange)
        {
            if (t == null) continue;

            Vector2 pos = t.position;

            float sqr = ((Vector2)t.position - selfPos).sqrMagnitude;
            if (sqr < minSqr)
            {
                minSqr = sqr;
                nearest = t;
            }
        }
        return nearest;
    }

    // 范围内是否还有敌人
    public bool HasAnyEnemyInRange() => m_targetsInRange.Count > 0;
}
