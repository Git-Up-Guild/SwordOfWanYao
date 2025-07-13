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
    // 用来记录已订阅的模型，方便退出时退订
    private readonly Dictionary<SoldierModel, Action<IEventData>> m_deathCallbacks
        = new Dictionary<SoldierModel, Action<IEventData>>();

    [Header("漏检补偿检测间隔（秒）")]
    [SerializeField] private float m_checkInterval = 0.2f;
    private float m_checkTimer;
    private Collider2D m_collider;
    
    public void Init()
    {
        m_model = GetComponentInParent<SoldierModel>();

        m_enemyTag = (m_model.Camp == SoldierCamp.Ally) ? "Enemy" : "Ally";

        m_collider = GetComponent<Collider2D>();
        m_collider.isTrigger = true;
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

        // 针对这个新进入的敌人，订阅它的死亡事件
        var enemyModel = other.GetComponentInParent<SoldierModel>();
        if (enemyModel != null && !m_deathCallbacks.ContainsKey(enemyModel))
        {
            Action<IEventData> cb = evt =>
            {
                var sd = evt as SoldierModel.SoldierStateChangeData;
                if (sd != null && sd.StateType == SoldierStateType.IsDead)
                    HandleTargetDeath(sd.Source.transform, enemyModel);
            };
            m_deathCallbacks[enemyModel] = cb;
            EventManager.Instance.Subscribe<IEventData>(
                SoldierEventNames.Died,
                enemyModel,
                cb);
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(m_enemyTag)) return;

        var t = other.transform;
        if (m_targetsInRange.Remove(t))
            OnEnemyExit?.Invoke(t);

        // 退订它的死亡事件
        var enemyModel = other.GetComponentInParent<SoldierModel>();
        if (enemyModel != null && m_deathCallbacks.TryGetValue(enemyModel, out var cb))
        {
            EventManager.Instance.Unsubscribe<IEventData>(
                SoldierEventNames.Died,
                enemyModel,
                cb);
            m_deathCallbacks.Remove(enemyModel);
        }

    }

    private void Update()
    {
        m_checkTimer += Time.deltaTime;
        if (m_checkTimer >= m_checkInterval)
        {
            m_checkTimer -= m_checkInterval;
            CheckForMissedTargets();
        }
    }

    private void CheckForMissedTargets()
    {
        // 利用 OverlapCollider 扫描触发器范围内所有碰撞体
        var results = new Collider2D[16];
        var filter = new ContactFilter2D { useTriggers = true };
        int count = m_collider.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            var other = results[i];
            if (other != null && other.CompareTag(m_enemyTag))
            {
                // 如果不在列表里，就当作进入
                if (!m_targetsInRange.Contains(other.transform))
                    OnTriggerEnter2D(other);
            }
        }
    }

    // 真正处理“目标死亡”逻辑
    private void HandleTargetDeath(Transform deadT, SoldierModel deadModel)
    {
        if (m_targetsInRange.Remove(deadT))
        {
            OnEnemyExit?.Invoke(deadT);
        }

        // 死亡后也不用再听它了
        if (m_deathCallbacks.TryGetValue(deadModel, out var cb))
        {
            EventManager.Instance.Unsubscribe<IEventData>(
                SoldierEventNames.Died,
                deadModel,
                cb);
            m_deathCallbacks.Remove(deadModel);
        }

    }

    public Transform GetNearestEnemy()
    {
        Transform nearest = null;
        float minSqr = float.MaxValue;
        Vector2 selfPos = transform.position;

        foreach (var t in m_targetsInRange)
        {
            if (t == null) continue;
            
            var model = t.gameObject.GetComponentInParent<SoldierModel>();

            if (model && model.IsDead) continue;

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
    public bool HasAnyEnemyInRange()
    {
        foreach (var t in m_targetsInRange)
        {
            if (t == null) continue;

            var model = t.GetComponentInParent<SoldierModel>();
            if (model && !model.IsDead)
                return true;
        }

        return false;
    }
}
