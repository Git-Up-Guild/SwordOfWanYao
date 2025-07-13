using UnityEngine;

public class AOESkill : SkillBase, IAnimationEventReceiver
{
    private Animator m_animator;
    private SoldierModel m_model;
    private AreaSkillData m_data;

    public AOESkill(SoldierModel model, AreaSkillData data) : base(model, data)
    {
        m_model = model;
        m_data = data;

        m_animator = model.GetComponentInChildren<Animator>();
        InitRelay(m_animator);
    }

    public override void Cast(Transform target = null, Vector3 position = default)
    {
        if (!m_model.IsAttacking || m_model.IsDead || m_model.IsFreezing)
            return;

        m_animator.SetTrigger("TriggerAttack");
    }

    public void TriggerAOE()
    {
        Vector3 spawnPos = m_model.SkillCastPivot.position;
        Quaternion rotation = Quaternion.identity;

        if (m_model.AttackTargetObject != null)
        {
            Vector2 dir = (m_model.AttackTargetObject.position - m_model.transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0, 0, angle);
        }

        GameObject aoeGO = null;

        if (m_data.isAttachedToFollower)
        {
            aoeGO = Object.Instantiate(m_data.AOEPrefab, spawnPos, rotation);
            aoeGO.transform.SetParent(m_model.transform);
        }
        else
        {
            aoeGO = Object.Instantiate(m_data.AOEPrefab, m_model.AttackTargetObject.position, rotation);
        }

        aoeGO.transform.localScale *= m_data.scaleMultiplier;

        AOEEffect effect = aoeGO.GetComponent<AOEEffect>();

        effect.Init(
            m_model,
            m_data.damagePerTick,
            m_data.tickInterval,
            m_data.duration,
            m_data.AOEShape
        );

        if (m_data.isAttachedToFollower)
        {
            aoeGO.transform.SetParent(m_model.transform);
        }
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "OnShootFrame")
            TriggerAOE();
    }
}
