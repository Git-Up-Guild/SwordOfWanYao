using UnityEngine;

public class MeleeSkill : SkillBase, IAnimationEventReceiver
{
    private Animator animator;
    private SoldierModel m_model;
    private MeleeSkillData m_data;

    public MeleeSkill(SoldierModel model, MeleeSkillData data) : base(model, data)
    {
        m_model = model;
        m_data = data;

        animator = model.GetComponentInChildren<Animator>();
        InitRelay(animator);
    
    }

    public override void Cast(Transform target = null, Vector3 position = default)
    {
        if (!m_model.IsAttacking || m_model.IsDead || m_model.IsFreezing)
            return;

        animator.SetTrigger("TriggerAttack");

    }

    // 动画事件中调用
    public void TriggerHit()
    {
        var meleeData = m_data;
        if (meleeData == null) return;

        Vector3 center = m_model.SkillCastPivot.position;
        float radius = meleeData.attackRadius;

        string enemyTag = m_model.Camp == SoldierCamp.Ally ? "Enemy" : "Ally";

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag(enemyTag)) continue;

            var targetModel = hit.GetComponentInParent<SoldierModel>();
            if (targetModel == null || targetModel.IsDead) continue;

            DamageApplyer.ApplyDamage(m_model, targetModel, m_data.damage);
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
