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

    private void OnDisable()
    {
        // 退订所有还没退的死亡事件
        foreach (var kv in m_deathCallbacks)
            EventManager.Instance.Unsubscribe<IEventData>(
                SoldierEventNames.Died,
                kv.Key,
                kv.Value);
        m_deathCallbacks.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        Debug.Log(other.name);
        if (!other.CompareTag(m_enemyTag)) return;

        var t = other.transform;
        if (!m_targets.Contains(t))
        {
            m_targets.Add(t);
            OnEnemyEnter?.Invoke(t);
        }

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
        if (m_targets.Remove(t))
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
                if (!m_targets.Contains(other.transform))
                    OnTriggerEnter2D(other);
            }
        }
    }

    private void HandleTargetDeath(Transform deadT, SoldierModel deadModel)
    {
        if (m_targets.Remove(deadT))
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

    // 是否还有可追踪的敌人
    public bool HasAnyEnemy()
    {
        foreach (var t in m_targets)
        {
            if (t == null) continue;

            var model = t.GetComponentInParent<SoldierModel>();
            if (model != null && !model.IsDead)
                return true;
        }

        return false;
    }

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
            var model = t.gameObject.GetComponentInParent<SoldierModel>();

            if (model && !model.IsDead)
            {

                float sqr = ((Vector2)t.position - selfPos).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    nearest = t;
                }
            }
        }
        return nearest;
    }
}

