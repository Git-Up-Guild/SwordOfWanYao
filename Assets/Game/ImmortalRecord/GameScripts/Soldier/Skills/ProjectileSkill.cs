using UnityEngine;

public class ProjectileSkill : SkillBase, IAnimationEventReceiver
{
    private Animator animator;
    private SoldierModel m_model;
    private ProjectileSkillData m_data;

    public ProjectileSkill(SoldierModel model, ProjectileSkillData data) : base(model, data)
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

    public void TriggerShoot()
    {

        if (m_model.AttackTargetObject == null) return;

        Vector3 firePos = m_model.SkillCastPivot.position;
        Vector3 targetPos = m_model.AttackTargetObject.position;

        Vector2 direction = (targetPos - firePos).normalized;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, direction);

        GameObject bulletGO = Object.Instantiate(m_data.projectilePrefab, firePos, rot);
        var bullet = bulletGO.GetComponent<ProjectileBullet>();

        bullet.Init(
            attacker: m_model,
            direction: direction,
            speed: m_data.moveSpeed,
            damage: m_data.damagePerHit,
            maxPierce: m_data.maxPierce
        );
    }

    public override void OnAnimationEvent(string eventName)
    {

        if (eventName == "OnShootFrame")
        {
            TriggerShoot();
        }
    }

}
