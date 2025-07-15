using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class ProjectileSkill : SkillBase, IAnimationEventReceiver
{
    private Animator animator;
    private SoldierModel m_model;
    private ProjectileSkillData m_data;
    private MonoBehaviour m_coroutineHost;

    private int m_remainingShots;
    private bool m_isPerformingAnimation;


    public ProjectileSkill(SoldierModel model, ProjectileSkillData data) : base(model, data)
    {

        m_model = model;
        m_data = data;

        animator = model.GetComponentInChildren<Animator>();
        m_coroutineHost = model.GetComponent<MonoBehaviour>();
        InitRelay(animator);

    }

    public override void Cast(Transform target = null, Vector3 position = default)
    {
        if (!m_model.IsAttacking || m_model.IsDead || m_model.IsFreezing || m_isPerformingAnimation)
            return;

        m_isPerformingAnimation = true;
        m_remainingShots = m_model.AttackFrequency;

        m_coroutineHost.StartCoroutine(PerformMultiShoot());

    }

    private IEnumerator PerformMultiShoot()
    {
        animator.speed = m_model.AttackFrequency;

        while (m_remainingShots > 0)
        {
            animator.SetTrigger("TriggerAttack");

            yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")); // 等待退出动画状态
            yield return new WaitForSeconds(0.05f); // 间隔

            m_remainingShots--;
        }

        animator.speed = 1f;
        m_isPerformingAnimation = false;
    }

    public void TriggerShoot()
    {
        m_coroutineHost.StartCoroutine(TriggerShootCoroutine());
    }
    
    private IEnumerator TriggerShootCoroutine()
    {
        int projectileCount = m_model.ProjectileCount;
        List<Transform> targets = SelectTargets(projectileCount);
        if (targets.Count == 0) yield break;

        Vector3 firePos = m_model.SkillCastPivot.position;

        foreach (var tgt in targets)
        {
            if (tgt == null) continue;

            Vector3 targetPos = tgt.position;
            Vector2 dir = (targetPos - firePos).normalized;
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);

            GameObject bulletGO = Object.Instantiate(m_data.projectilePrefab, firePos, rot);
            var bullet = bulletGO.GetComponent<ProjectileBullet>();
            bullet.Init(
                attacker: m_model,
                direction: dir,
                speed: m_data.moveSpeed,
                damage: m_data.damagePerHit,
                maxPierce: m_data.maxPierce,
                canExplode: m_data.canExplode,
                explosionDamage: m_data.damage,
                explosionScaleMutiplier: m_data.scaleMutiplier,
                explosionPrefab: m_data.explosionPrefab,
                canSplit: m_data.canSplit,
                splitBulletPrefab: m_data.splitBulletPrefab
            );

            // 间隔 0.03 秒
            yield return new WaitForSeconds(0.03f);
        }
    }
    // 选目标逻辑：主目标 → 不同目标 → 随机补足
    private List<Transform> SelectTargets(int count)
    {
        var result = new List<Transform>(count);

        // 1. 主目标优先
        Transform primary = m_model.AttackTargetObject;
        if (primary != null)
            result.Add(primary);

        // 2. 从范围内列表筛掉主目标和死亡目标
        var pool = m_model.AttackTargetObjectInRange
        .Where(t =>
            t != null &&
            t != primary &&
            t.GetComponentInParent<SoldierModel>() != null &&
            !t.GetComponentInParent<SoldierModel>().IsDead)
        .ToList();

        // Fisher–Yates 随机洗牌
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // 添加不同目标直到凑够
        foreach (var t in pool)
        {
            if (result.Count >= count) break;
            result.Add(t);
        }

        // 3. 不够时随机从已有结果补足
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
        {
            TriggerShoot();
        }
    }

}
