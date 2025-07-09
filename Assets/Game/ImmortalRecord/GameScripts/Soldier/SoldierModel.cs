using UnityEngine;

public enum SoldierAttributeType
{
    ID,
    Camp,
    DisplayName,
    Rarity,
    Health,
    AttackPowerMutiplier,
    MoveSpeed,
    Defense,
    AttackSpeed,
    LockOnRange,
    AttatckFrequency

}

public enum SoldierStateType
{
    IsInvincible,
    IsShield,
    IsDead,
    IsTargetingAtBase,
    IsLockingOn,
    IsAttacking,
    IsStaying,
    IsMoving,
    IsFreezing
}
public class SoldierModel : MonoBehaviour
{
    [SerializeField] private int m_id;
    [SerializeField] private string m_displayName;
    [SerializeField] private SoldierRarity m_rarity;
    [SerializeField] private SoldierCamp m_camp;
    [SerializeField] private float m_maxHealth;
    [SerializeField] private float m_attackPowerMutiplier;
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_defense;
    [SerializeField] private float m_attackSpeed;
    [SerializeField] private float m_lockOnRange;
    [SerializeField] private float m_attackFrequency;

    [SerializeField] Transform m_targetTransform;

    [SerializeField] private float m_currentHealth;

    [SerializeField] private bool m_isDead;
    private bool m_deathEventTriggered;
    [SerializeField] private bool m_isInvincible;
    [SerializeField] private bool m_isMovingToOppositeBase;
    [SerializeField] private bool m_isLockingOnEnemy;
    [SerializeField] private bool m_isAttacking;
    [SerializeField] private bool m_isStaying;
    [SerializeField] private bool m_isMoving;
    [SerializeField] private bool m_isFreezing;

    private void Awake()
    {
        InitAttributes();
        InitStates();

    }

    //属性访问器
    public int ID
    {
        get => m_id;
        set => m_id = value;

    }
    public string DisplayName
    {
        get => m_displayName;
        set => m_displayName = value;

    }
    public float MaxHealth
    {
        get => m_maxHealth;
        set => m_maxHealth = value;

    }
    public SoldierRarity Rarity
    {
        get => m_rarity;
        set => m_rarity = value;

    }

    public Transform MoveTargetIndicator
    {
        get => m_targetTransform;
        set => m_targetTransform = value;

    }

    public float MoveSpeed
    {
        get => m_moveSpeed;
        set => m_moveSpeed = value;

    }

    public float Defense
    {
        get => m_defense;
        set => m_defense = value;

    }

    public float AttackPowerMutiplier
    {
        get => m_attackPowerMutiplier;
        set => m_attackPowerMutiplier = value;

    }

    public float AttackSpeed
    {
        get => m_attackSpeed;
        set => m_attackSpeed = value;

    }

    public float LockOnRange
    {
        get => m_lockOnRange;
        set => m_lockOnRange = value;

    }

    public float AttackFrequency
    {
        get => m_attackFrequency;
        set => m_attackFrequency = value;

    }

    public SoldierCamp Camp
    {
        get => m_camp;
        set => m_camp = value;

    }

    public float Health
    {

        get => m_currentHealth;

        set
        {

            var previous = m_currentHealth;
            m_currentHealth = Mathf.Clamp(value, 0, m_maxHealth);

            EventManager.Instance.TriggerEvent(
                SoldierEventNames.HealthChanged,
                    new SoldierAttributeChangeData(
                        this,
                        SoldierAttributeType.Health,
                        m_currentHealth,
                        m_currentHealth - previous

                    )

            );

            if (m_currentHealth <= 0 && previous > 0)
            {

                IsDead = true;

            }

        }

    }

    //状态访问器
    public bool IsDead
    {

        get => m_isDead;
        set
        {

            if (m_isDead != value)
            {

                m_isDead = value;

                if (m_isDead && !m_deathEventTriggered)
                {

                    m_deathEventTriggered = true;
                    EventManager.Instance.TriggerEvent(

                    SoldierEventNames.Died,
                    new SoldierStateChangeData(this, SoldierStateType.IsDead)
                    );

                }

            }

        }

    }
    public bool IsInvincible
    {

        get => m_isInvincible;
        set
        {

            if (m_isInvincible != value)
            {

                m_isInvincible = value;
                EventManager.Instance.TriggerEvent(

                    value ? SoldierEventNames.InvincibleEntered : SoldierEventNames.InvincibleExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsInvincible)

                );

            }

        }

    }

    public bool IsTargetingAtBase
    {

        get => m_isMovingToOppositeBase;
        set
        {

            if (m_isMovingToOppositeBase != value)
            {

                m_isMovingToOppositeBase = value;
                EventManager.Instance.TriggerEvent(
                    value ? SoldierEventNames.TargetingAtBaseEntered : SoldierEventNames.TargetingAtBaseExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsTargetingAtBase)

                );

            }

        }

    }

    public bool IsLockingOnEnemy
    {

        get => m_isLockingOnEnemy;
        set
        {

            if (m_isLockingOnEnemy != value)
            {

                m_isLockingOnEnemy = value;
                EventManager.Instance.TriggerEvent(
                    value ? SoldierEventNames.LockingOnEntered : SoldierEventNames.LockingOnExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsLockingOn)

                );

            }

        }

    }

    public bool IsAttacking
    {

        get => m_isAttacking;
        set
        {

            if (m_isAttacking != value)
            {

                m_isAttacking = value;
                EventManager.Instance.TriggerEvent(
                    value ? SoldierEventNames.AttackingEntered : SoldierEventNames.AttackingExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsAttacking)

                );

            }

        }

    }

    public bool IsStaying
    {

        get => m_isStaying;
        set
        {

            if (m_isStaying != value)
            {

                m_isStaying = value;
                EventManager.Instance.TriggerEvent(
                    value ? SoldierEventNames.StayingEntered : SoldierEventNames.StayingExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsStaying)

                );

            }

        }

    }

    public bool IsMoving
    {

        get => m_isMoving;
        set
        {

            if (m_isMoving != value)
            {

                m_isMoving = value;
                EventManager.Instance.TriggerEvent(
                    value ? SoldierEventNames.MovingEntered : SoldierEventNames.MovingExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsMoving)

                );

            }

        }

    }

    public bool IsFreezing
    {

        get => m_isFreezing;
        set
        {

            if (m_isFreezing != value)
            {

                m_isFreezing = value;
                EventManager.Instance.TriggerEvent(
                    value ? SoldierEventNames.FreezingEntered : SoldierEventNames.FreezingExited,
                    new SoldierStateChangeData(this, SoldierStateType.IsFreezing)

                );

            }

        }

    }

    private void InitAttributes()
    {

        m_currentHealth = m_maxHealth;

    }

    private void InitStates()
    {

        m_isDead = false;
        m_deathEventTriggered = false;

        m_isMovingToOppositeBase = false;
        m_isLockingOnEnemy = false;
        m_isAttacking = false;
        m_isStaying = false;
        m_isFreezing = false;

    }

    //属性更新数据类
    public class SoldierAttributeChangeData : IEventData
    {

        public SoldierModel Source { get; }
        public readonly SoldierAttributeType Type;
        public readonly float CurrentValue;
        public readonly float CurrentAmount;

        public SoldierAttributeChangeData(SoldierModel source, SoldierAttributeType type, float current,
        float delta)
        {
            Source = source;
            Type = type;
            CurrentValue = current;
            CurrentAmount = delta;
        }

        object IEventData.Source => Source;

    }

    //状态更新数据类
    public class SoldierStateChangeData : IEventData
    {
        public SoldierModel Source { get; }
        public readonly SoldierStateType StateType;
        public float ChangeTime { get; set; }

        public SoldierStateChangeData(SoldierModel source, SoldierStateType type)
        {
            Source = source;
            StateType = type;
            ChangeTime = Time.time;
        }

        object IEventData.Source => Source;

    }

}
