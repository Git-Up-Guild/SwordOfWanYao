using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackTargetDetector : MonoBehaviour
{
    public event Action<Transform> OnEnemyEnter;
    public event Action<Transform> OnEnemyExit;

    private SoldierModel m_model;
    private string m_enemyTag;
    private readonly List<Transform> m_targets = new List<Transform>();

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
            m_targets.RemoveAll(t => t == null);
            lastCheckTime = Time.time;
        }

    }


    private void OnTriggerEnter2D(Collider2D other)
    {


        if (!other.CompareTag(m_enemyTag)) return;

        var t = other.transform;
        if (!m_targets.Contains(t))
        {
            m_targets.Add(t);
            OnEnemyEnter?.Invoke(t);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (!other.CompareTag(m_enemyTag)) return;

        var t = other.transform;
        if (m_targets.Remove(t))
            OnEnemyExit?.Invoke(t);
    }

    // 是否还有可追踪的敌人
    public bool HasAnyEnemy() => m_targets.Count > 0;

    // 当前是否正在追踪该目标
    public bool IsTargetTracked(Transform target) => m_targets.Contains(target);

    // 返回距离自己最近的敌人（或 null）
    public Transform GetNearestEnemy()
    {
        Transform nearest = null;
        float minSqr = float.MaxValue;
        Vector2 selfPos = transform.position;

        foreach (var t in m_targets)
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
}

