using UnityEngine;
using System.Collections;

public class SoldierStateMachine : MonoBehaviour
{
    private SoldierController m_controller;
    private SoldierModel m_model;
    private SoldierMovement m_mover;
    private AttackTargetDetector m_attackDetector;
    private AttackRangeDetector m_attackRangeDetector;

    //基地位置
    [SerializeField] private float m_enemybaseTargetY = 300;
    [SerializeField] private float m_allybaseTargetY = -300;

    // 上一次锁定的追踪目标
    private Transform m_lastTrackedTarget;

    private void Awake()
    {
        m_controller = GetComponent<SoldierController>();
        m_model = GetComponent<SoldierModel>();
        m_mover = GetComponent<SoldierMovement>();

        m_attackDetector = GetComponentInChildren<AttackTargetDetector>();
        m_attackRangeDetector = GetComponentInChildren<AttackRangeDetector>();
    }

    public void InitStateMachine()
    {

        m_attackDetector.Init();
        m_attackRangeDetector.Init();

        // 默认状态：朝向敌方基地移动
        HandleMovingToOppositeBase();

        //订阅事件
        m_attackDetector.OnEnemyEnter += HandleEnemyEnterDetection;
        m_attackDetector.OnEnemyExit += HandleEnemyExitDetection;

        m_attackRangeDetector.OnEnemyEnter += HandleEnterAttackRange;
        m_attackRangeDetector.OnEnemyExit += HandleExitAttackRange;

        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.Died, m_model, _ => HandleDeath());

    }
    private void OnDisable()
    {
        // 取消订阅
        m_attackRangeDetector.OnEnemyEnter -= HandleEnterAttackRange;
        m_attackRangeDetector.OnEnemyExit -= HandleExitAttackRange;

        m_attackDetector.OnEnemyEnter -= HandleEnemyEnterDetection;
        m_attackDetector.OnEnemyExit -= HandleEnemyExitDetection;

        if (EventManager.Instance != null)
            EventManager.Instance.Unsubscribe<IEventData>(SoldierEventNames.Died, m_model, _ => HandleDeath());

    }

    private void Update()
    {
        if (m_model.IsDead) return;

        //CheckIfTargetIsNearest();

        // 只需处理目标丢失情况
        if (m_model.AttackTargetObject == null && m_model.IsLockingOnEnemy)
        {
            HandleEnemyLost();
        }

    }

    private void CheckIfTargetIsNearest()
    {

        if ((m_model.IsLockingOnEnemy || m_model.IsTargetingAtBase) && !m_model.IsAttacking)
        {
            Transform currentTarget = m_attackDetector.GetNearestEnemy();

            if (currentTarget != m_lastTrackedTarget)
            {
                if (currentTarget != null)
                {
                    HandleEnemyDetected(currentTarget);
                }
                else
                {
                    m_model.AttackTargetObject = null;
                    m_lastTrackedTarget = null;
                    HandleMovingToOppositeBase();
                }
            }
        }
    }

    private void HandleMovingToOppositeBase()
    {

        if (m_model.IsDead) return;

         m_mover.MoveTo(GetFallbackPosition());

        m_controller.ConvertState(SoldierStateType.IsTargetingAtOppositeBase, true);
        m_controller.ConvertState(SoldierStateType.IsMoving, true);
        m_controller.ConvertState(SoldierStateType.IsAttacking, false);

    }

    // 索敌器探测到敌人时调用
    private void HandleEnemyDetected(Transform enemy)
    {
        if (m_model.IsDead) return;

        // 设置新的锁定目标
        m_model.AttackTargetObject = enemy;
        m_controller.ConvertState(SoldierStateType.IsLockingOn, true);

        // 使用动态目标追踪
        m_mover.FollowDynamicTarget(() => 
            m_model.AttackTargetObject != null 
                ? m_model.AttackTargetObject.position 
                : GetFallbackPosition()  // 备用位置
        );
    }

    // 索敌器内所有敌人消失或死亡时,或锁定的敌人不再是最近敌人时调用
    private void HandleEnemyLost()
    {
        if (m_model.IsDead) return;

        // 优先检查攻击范围内是否有目标
        Transform nearestInRange = m_attackRangeDetector.GetNearestEnemy();
        if (nearestInRange != null)
        {
            HandleEnterAttackRange(nearestInRange); // 直接进入攻击状态
            return;
        }

        // 如果没有攻击范围内的目标，则从探测范围内寻找
        Transform nextTarget = m_attackDetector.GetNearestEnemy();
        if (nextTarget != null)
        {
            HandleEnemyDetected(nextTarget);
        }
        else
        {
            HandleMovingToOppositeBase();
        }
    }

    // 锁定目标进入攻击范围时调用
    private void HandleEnterAttackRange(Transform enemy)
    {
        if (m_model.IsDead) return;

        // 总是从攻击范围内选择最近的目标（忽略传入的enemy参数）
        Transform nearestTarget = m_attackRangeDetector.GetNearestEnemy();
        
        if (nearestTarget != null)
        {
            m_model.AttackTargetObject = nearestTarget;
            m_controller.ConvertState(SoldierStateType.IsLockingOn, true);
            
            m_controller.ConvertState(SoldierStateType.IsStaying, true);
            m_controller.ConvertState(SoldierStateType.IsAttacking, true);
            m_mover.StopMoving();
        }
    }

    // 锁定目标离开攻击范围时调用
    private void HandleExitAttackRange(Transform enemy)
    {
        if (enemy != m_model.AttackTargetObject) return;

        // 优先从攻击范围内选择最近目标
        m_model.AttackTargetObject = m_attackRangeDetector.GetNearestEnemy();
        
        // 如果攻击范围内没有目标，再从探测范围内寻找
        if (m_model.AttackTargetObject == null)
        {
            m_model.AttackTargetObject = m_attackDetector.GetNearestEnemy();
        }

        if (m_model.AttackTargetObject != null) 
        {
            // 复用动态跟踪逻辑
            HandleEnemyDetected(m_model.AttackTargetObject);
        }
        else 
        {
            HandleMovingToOppositeBase();
        }
    }


    private void HandleEnemyEnterDetection(Transform enemy)
    {
        if (!m_model.IsLockingOnEnemy && !m_model.IsAttacking)
        {
            HandleEnemyDetected(enemy);
        }
    }

    private void HandleEnemyExitDetection(Transform enemy)
    {
        if (enemy == m_model.AttackTargetObject)
        {
            HandleEnemyLost();
        }
    }

    private void HandleDeath()
    {

        m_mover.StopMoving();
        m_controller.ConvertState(SoldierStateType.IsAttacking, false);
        m_controller.ConvertState(SoldierStateType.IsMoving, false);
        m_controller.ConvertState(SoldierStateType.IsFreezing, false);
        m_controller.ConvertState(SoldierStateType.IsDead, true);

        var hitBox = transform.Find("HitBox");
        if (hitBox != null)
        {
            var col = hitBox.GetComponent<Collider2D>();
            if (col != null)
            {
                Destroy(col);
            }
        }

        StartCoroutine(DestroyAfterDelay(2f));

    }
    private Vector2 GetFallbackPosition()
    {
        // 根据阵营返回对应的基地位置
        return m_model.Camp switch
        {
            SoldierCamp.Ally => new Vector2(transform.position.x, m_enemybaseTargetY),
            SoldierCamp.Enemy => new Vector2(transform.position.x, m_allybaseTargetY),
            _ => new Vector2(transform.position.x, m_enemybaseTargetY) // 默认
        };
    }
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(m_model.gameObject);
    }
    


}
