using UnityEngine;
using System.Collections;

class SoldierController : MonoBehaviour
{

    [SerializeField] private SoldierDataBase m_data;
    private SoldierModel m_model;
    private Coroutine m_invincibleCoroutine;


    private void OnEnable()
    {

        m_model = GetComponent<SoldierModel>();
        InitializeModel();

    }

    public void TakeDamage(int amount)
    {

        if (m_model.IsDead || m_model.IsInvincible)
            return;

        m_model.Health -= amount;

    }

    public void Heal(int amount)
    {

        if (m_model.Health + amount > m_model.MaxHealth)
        {

            m_model.Health = m_model.MaxHealth;

        }
        else
        {

            m_model.Health += amount;

        }

    }

    void InitializeModel()
    {
        //基础设置
        m_model.Camp = m_data.camp;
        m_model.MaxHealth = m_data.attributes.maxHealth;

        //核心属性
        m_model.MoveSpeed = m_data.attributes.moveSpeed;
        m_model.Defense = m_data.attributes.defense;
        m_model.AttackSpeed = m_data.attributes.attackSpeed;
        m_model.AttackFrequency = m_data.attributes.attackFrequency;

        //信息
        m_model.ID = m_data.ID;
        m_model.Rarity = m_data.rarity;
        m_model.DisplayName = m_data.displayName;

        //技能与特效 ToDo
        foreach (var skillData in m_data.skills)
        {

        }
    }

    //属性设置器

    public void SetMoveSpeed(float speed) => m_model.MoveSpeed = Mathf.Max(0, speed);
    public void SetDefense(float defense) => m_model.Defense = Mathf.Max(0, defense);
    public void SetAttackSpeed(float speed) => m_model.AttackSpeed = Mathf.Max(0, speed);
    public void SetAttackFrequency(float freq) => m_model.AttackFrequency = Mathf.Max(0, freq);
    public void SetLockOnRange(float range) => m_model.LockOnRange = Mathf.Max(0, range);
    public void SetAttackPowerMultiplier(float mul) => m_model.AttackPowerMutiplier = Mathf.Max(0, mul);
    public void SetMaxHealth(float max)
    {
        m_model.MaxHealth = Mathf.Max(1, max);
        m_model.Health = Mathf.Min(m_model.Health, m_model.MaxHealth);
    }

    public void EnableInvincibleForDuration(float duration)
    {

        //如果已经有正在持续的无敌协程，先停止
        if (m_invincibleCoroutine != null)
        {

            StopCoroutine(m_invincibleCoroutine);

        }

        m_invincibleCoroutine = StartCoroutine(InvincibleDurationRoutine(duration));

    }

    private IEnumerator InvincibleDurationRoutine(float duration)
    {

        SetIsInvincible(true);

        yield return new WaitForSeconds(duration);

        SetIsInvincible(false);

        m_invincibleCoroutine = null;

    }

    //状态设置器
    public void SetIsDead(bool val) => m_model.IsDead = val;
    public void SetIsInvincible(bool val) => m_model.IsInvincible = val;
    public void SetIsStaying(bool val) => m_model.IsStaying = val;
    public void SetIsMoving(bool val) => m_model.IsMoving = val;
    public void SetIsLockingOn(bool val) => m_model.IsLockingOnEnemy = val;
    public void SetIsTargetingAtBase(bool val) => m_model.IsTargetingAtBase = val;
    public void SetIsAttacking(bool val) => m_model.IsAttacking = val;
    public void SetIsFreezing(bool val) => m_model.IsFreezing = val;


    //状态切换
    public void ConvertState(SoldierStateType targetState, bool value)
    {

        if (!value)
        {
            switch (targetState)
            {
                // 互斥组 1：IsTargetingAtBase 与 IsLockingOn
                case SoldierStateType.IsTargetingAtBase:
                    SetIsTargetingAtBase(false);
                    SetIsLockingOn(true);
                    return;

                case SoldierStateType.IsLockingOn:
                    SetIsLockingOn(false);
                    SetIsTargetingAtBase(true);
                    return;

                // 互斥组 2：IsMoving 与 IsStaying
                case SoldierStateType.IsMoving:
                    SetIsMoving(false);
                    SetIsStaying(true); 
                    return;

                case SoldierStateType.IsStaying:
                    SetIsStaying(false);
                    SetIsMoving(true);
                    return;

                default:
                    SetSingleState(targetState, false);
                    return;
            }
        }

        switch (targetState)
        {
            // 互斥组 1：IsTargetingAtBase 与 IsLockingOn
            case SoldierStateType.IsTargetingAtBase:
                SetIsLockingOn(false);
                SetIsTargetingAtBase(true);
                break;

            case SoldierStateType.IsLockingOn:
                SetIsTargetingAtBase(false);
                SetIsLockingOn(true);
                break;

            // 互斥组 2：IsMoving 与 IsStaying
            case SoldierStateType.IsMoving:
                SetIsStaying(false);
                SetIsMoving(true);
                break;

            case SoldierStateType.IsStaying:
                SetIsMoving(false);
                SetIsStaying(true);
                break;

            default:
                SetSingleState(targetState, true);
                break;
        }
    }

    //状态切换辅助方法
    private void SetSingleState(SoldierStateType state, bool value)
    {
        switch (state)
        {
            case SoldierStateType.IsDead:
                SetIsDead(value);
                break;
            case SoldierStateType.IsInvincible:
                SetIsInvincible(value);
                break;
            case SoldierStateType.IsTargetingAtBase:
                SetIsTargetingAtBase(value);
                break;
            case SoldierStateType.IsLockingOn:
                SetIsLockingOn(value);
                break;
            case SoldierStateType.IsAttacking:
                SetIsAttacking(value);
                break;
            case SoldierStateType.IsFreezing:
                SetIsFreezing(value);
                break;
            case SoldierStateType.IsStaying:
                SetIsStaying(value);
                break;
            case SoldierStateType.IsMoving:
                SetIsMoving(value);
                break;
            default:
                CustomLogger.LogWarning($"未处理状态类型: {state}");
                break;
        }
    }

}
