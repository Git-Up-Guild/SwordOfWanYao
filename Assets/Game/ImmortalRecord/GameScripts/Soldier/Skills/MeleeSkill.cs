using UnityEngine;
using System.Collections;

public class MeleeSkill : SkillBase, IAnimationEventReceiver
{
    private Animator m_animator;
    private SoldierModel m_model;
    private MeleeSkillData m_data;
    private MonoBehaviour m_coroutineHost;

    private int m_remainingAttacks;
    private bool m_isPerformingAnimation;

    public MeleeSkill(SoldierModel model, MeleeSkillData data) : base(model, data)
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

        m_isPerformingAnimation = false;

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

    // 动画事件中调用
    public void TriggerHit()
    {
        var meleeData = m_data;
        if (meleeData == null) return;

        string enemyTag = m_model.Camp == SoldierCamp.Ally ? "Enemy" : "Ally";
        if (m_model.AttackTargetObject == null) return;
        var targetModel = m_model.AttackTargetObject.GetComponentInParent<SoldierModel>();
        if (targetModel != null && targetModel.Camp != m_model.Camp && !targetModel.IsDead)
        {
            DamageApplyer.ApplyDamage(m_model, targetModel, m_data.damage, targetModel.transform.position);
        }

        // 检测建筑等可破坏目标
        if (m_model.AttackTargetObject == null) return;
        var destructible = m_model.AttackTargetObject.GetComponent<IDestructible>();
        if (destructible != null && destructible.GetCamp() != m_model.Camp)
        {
            destructible.TakeDamage(m_data.damage, m_model);
        }

    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "OnHitFrame")
        {
            TriggerHit();
        }
    }

}
