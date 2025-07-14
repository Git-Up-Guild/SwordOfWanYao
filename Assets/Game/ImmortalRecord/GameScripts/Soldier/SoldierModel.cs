using UnityEngine;
using System.Collections.Generic;

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
    AttackRange,
    LockOnRange,
    AttatckFrequency,
    ProjectileCount,

}

public enum SoldierStateType
{
    IsInvincible,
    IsDead,
    IsTargetingAtOppositeBase,
    IsLockingOn,
    IsAttacking,
    IsStaying,
    IsMoving,
    IsFreezing
}

public enum SoldierTargtes
{
    MoveTargetIndicator,
    AttackTargetDetector,
    AttackRangeDetector,
    AttackTargetObject,
    AttackTargetObjectInRange,
    SkillCastPoint,

}

public class SoldierModel : MonoBehaviour
{
    [SerializeField] private int m_id;
    [SerializeField] private string m_displayName;
    [SerializeField] private SoldierRarity m_rarity;
    [SerializeField] private SoldierCamp m_camp;
    [SerializeField] private SoldierType m_type;

    private SoldierAttributeSO m_attributes; // 运行士兵属性时共享副本

    [SerializeField] Transform m_moveTargetIndicator;
    [SerializeField] Transform m_attackTargetDetector;
    [SerializeField] Transform m_attackRangeDetector;
    [SerializeField] Transform m_attackTargetObject;
    [SerializeField] List<Transform> m_attackTargetObjectInRange;
    [SerializeField] Transform m_skillCastPivot;

    [SerializeField] private float m_currentHealth;
    private List<SoldierSkillDataBase> m_localSkillDataList;
    private List<SkillBase> m_runtimeSkillList;

    [SerializeField] private bool m_isDead;
    private bool m_deathEventTriggered;
    [SerializeField] private bool m_isInvincible;
    [SerializeField] private bool m_isMovingToOppositeBase;
    [SerializeField] private bool m_isLockingOnEnemy;
    [SerializeField] private bool m_isAttacking;
    [SerializeField] private bool m_isStaying;
    [SerializeField] private bool m_isMoving;
    [SerializeField] private bool m_isFreezing;
    [SerializeField] private SoldierDataBase m_data;
    [SerializeField] private float m_scaleSize;

    //初始化完成标志
    public bool IsInitialized { get; private set; }

    // 基本信息字段（不共享）
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

    public SoldierRarity Rarity
    {
        get => m_rarity;
        set => m_rarity = value;
    }

    public SoldierCamp Camp
    {
        get => m_camp;
        set => m_camp = value;
    }

    public SoldierType Type
    {
        get => m_type;
        set => m_type = value;
    }

    // 运行时属性（共享副本）

    public float MaxHealth
    {
        get => m_attributes.maxHealth;
        set => m_attributes.maxHealth = value;
    }

    public float AttackPowerMutiplier
    {
        get => m_attributes.attackPowerMutiplier;
        set => m_attributes.attackPowerMutiplier = value;
    }

    public float MoveSpeed
    {
        get => m_attributes.moveSpeed;
        set => m_attributes.moveSpeed = value;
    }

    public float Defense
    {
        get => m_attributes.defense;
        set => m_attributes.defense = value;
    }

    public float AttackSpeed
    {
        get => m_attributes.attackSpeed;
        set => m_attributes.attackSpeed = value;
    }

    public float AttackRange
    {
        get => m_attributes.attackRange;
        set => m_attributes.attackRange = value;
    }

    public float LockOnRange
    {
        get => m_attributes.lockOnRange;
        set => m_attributes.lockOnRange = value;
    }

    public int AttackFrequency
    {
        get => m_attributes.attackFrequency;
        set => m_attributes.attackFrequency = value;
    }

    public int ProjectileCount
    {
        get => m_attributes.projectileCount;
        set => m_attributes.projectileCount = value;
    }

    public Transform MoveTargetIndicator
    {
        get => m_moveTargetIndicator;
        set => m_moveTargetIndicator = value;
    }

    public Transform AttackTargetDetector
    {
        get => m_attackTargetDetector;
        set => m_attackTargetDetector = value;
    }
    public Transform AttackRangeDetector
    {
        get => m_attackRangeDetector;
        set => m_attackRangeDetector = value;
    }

    public Transform AttackTargetObject
    {
        get => m_attackTargetObject;
        set => m_attackTargetObject = value;
    }

    public List<Transform> AttackTargetObjectInRange
    {
        get => m_attackTargetObjectInRange;
        set => m_attackTargetObjectInRange = value;
    }

    public Transform SkillCastPivot
    {
        get => m_skillCastPivot;
        set => m_skillCastPivot = value;
    }

    public List<SoldierSkillDataBase> LocalSkillDataList
    {
        get => m_localSkillDataList;
        set => m_localSkillDataList = value;
    }

    public List<SkillBase> RuntimeSkillList
    {
        get => m_runtimeSkillList;
        set => m_runtimeSkillList = value;
    }

    public float Health
    {

        get => m_currentHealth;
        
        set
        {

            var previous = m_currentHealth;
            m_currentHealth = Mathf.Clamp(value, 0, MaxHealth);

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
                    new SoldierStateChangeData(this, SoldierStateType.IsTargetingAtOppositeBase)

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
    public void InitModel()
    {

        InitData();
        InitAttributes();
        InitStates();
        InitTargets();

        IsInitialized = true;

    }

    private void InitData()
    {
    
        //基础信息
        ID = m_data.ID;
        Rarity = m_data.rarity;
        DisplayName = m_data.displayName;
        Camp = m_data.camp;
        m_type = m_data.soldierType;

        //运行时士兵属性
        m_attributes = RuntimeSoldierAttributeHub.Instance.Get(m_data.soldierType);

        // 技能数据：共享副本
        m_localSkillDataList = RuntimeSoldierSkillHub.Instance.GetSkills(m_type);

        m_runtimeSkillList = new List<SkillBase>();
        foreach (var data in m_localSkillDataList)
        {
            var skill = SoldierSkillFactory.Create(this, data);
            m_runtimeSkillList.Add(skill);
        }

        //目标对象
        AttackTargetObjectInRange = new List<Transform>();

    }

    private void InitAttributes()
    {

        m_currentHealth = MaxHealth;
        transform.localScale *= m_scaleSize;

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

    private void InitTargets()
    {

        AttackTargetDetector.localScale *= LockOnRange;
        AttackRangeDetector.localScale *= AttackRange;

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
