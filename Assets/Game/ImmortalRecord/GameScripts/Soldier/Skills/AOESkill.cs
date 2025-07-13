using UnityEngine;
using System.Collections;
using System.Linq;               
using System.Collections.Generic; 

public class AOESkill : SkillBase, IAnimationEventReceiver
{
    private Animator m_animator;
    private SoldierModel m_model;
    private AreaSkillData m_data;

    private MonoBehaviour m_coroutineHost;

    private int m_remainingAttacks;
    private bool m_isPerformingAnimation;

    public AOESkill(SoldierModel model, AreaSkillData data) : base(model, data)
    {
        m_model = model;
        m_data = data;

        m_animator = model.GetComponentInChildren<Animator>();
        m_coroutineHost = model.GetComponent<MonoBehaviour>();
        InitRelay(m_animator);
    }

    public override void Cast(Transform target = null, Vector3 position = default)
    {
        if (!m_model.IsAttacking || m_model.IsDead || m_model.IsFreezing || m_isPerformingAnimation)
            return;

        m_isPerformingAnimation = true;
        m_remainingAttacks = m_model.AttackFrequency;
        m_coroutineHost.StartCoroutine(PerformMultiAttack());

    }

    private IEnumerator PerformMultiAttack()
    {
        m_animator.speed = m_model.AttackFrequency;

        while (m_remainingAttacks > 0)
        {
            m_animator.SetTrigger("TriggerAttack");
            yield return new WaitUntil(() => !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")); // 等动画退出攻击状态
            yield return new WaitForSeconds(0.05f); 
            m_remainingAttacks--;
        }

        m_animator.speed = 1f;
        m_isPerformingAnimation = false;
    }

    public void TriggerAOE()
    {
        m_coroutineHost.StartCoroutine(TriggerAOECoroutine());
    }


    private IEnumerator TriggerAOECoroutine()
    {
        int aoeCount = m_model.ProjectileCount;
        if (aoeCount <= 0) aoeCount = 1;

        var targets = SelectTargets(aoeCount);
        if (targets.Count == 0) yield break;

        foreach (var tgt in targets)
        {
            if (tgt == null) continue;

            Vector3 spawnPos;
            Quaternion rot = Quaternion.identity;

            if (m_data.isAttachedToFollower)
            {
                spawnPos = m_model.SkillCastPivot.position;

                // 计算方向向量：从自己朝目标的方向
                Vector2 direction = (tgt.position - m_model.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rot = Quaternion.Euler(0, 0, angle - 90);
            }
            else
            {
                spawnPos = tgt.position;
            }

            var aoeGO = Object.Instantiate(m_data.AOEPrefab, spawnPos, rot);

            if (m_data.isAttachedToFollower)
                aoeGO.transform.SetParent(m_model.transform, worldPositionStays: true);

            aoeGO.transform.localScale *= m_data.scaleMultiplier;

            var effect = aoeGO.GetComponent<AOEEffect>();
            effect.Init(
                attacker: m_model,
                damage: m_data.damagePerTick,
                tickInterval: m_data.tickInterval,
                duration: m_data.duration,
                shape: m_data.AOEShape,
                canPull: m_data.canPull,
                pullStrength: m_data.pullStrength,
                moveSpeed: m_data.moveSpeed,
                canRotate: m_data.canRotate,
                rotateAngle: m_data.angle,
                rotatingSpeed: m_data.rotatingSpeed,
                rotatingInterval: m_data.rotatingInterval,
                isAttached: m_data.isAttachedToFollower
            );

            yield return new WaitForSeconds(0.03f);
        }
    }
    private List<Transform> SelectTargets(int count)
    {
        var result = new List<Transform>(count);

        // 1. 主目标
        Transform primary = m_model.AttackTargetObject;
        if (primary != null)
            result.Add(primary);

        // 2. 从范围列表里挑剩余不同目标
        var pool = m_model.AttackTargetObjectInRange
            .Where(t => t != null && t != primary &&
                        !t.GetComponentInParent<SoldierModel>().IsDead)
            .ToList();

        // Fisher–Yates 随机洗牌
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // 填充不同目标
        foreach (var t in pool)
        {
            if (result.Count >= count) break;
            result.Add(t);
        }

        // 3. 不够时重复已有目标
        while (result.Count < count && result.Count > 0)
        {
            int idx = Random.Range(0, result.Count);
            result.Add(result[idx]);
        }

        return result;
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "OnShootFrame")
            TriggerAOE();
    }
}
