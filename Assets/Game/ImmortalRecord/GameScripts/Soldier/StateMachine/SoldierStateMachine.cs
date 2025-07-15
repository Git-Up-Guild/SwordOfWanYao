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

        //m_model.AttackTargetObject = null;

        // 重新锁定新的目标
        Transform nextTarget = m_attackDetector.GetNearestEnemy();
        if (nextTarget != null)
        {
            HandleEnemyDetected(nextTarget);
        }
        else
        {
            // 若找不到新目标，则继续进攻敌方基地
            m_model.AttackTargetObject = null;
            HandleMovingToOppositeBase();
        }

    }

    // 锁定目标进入攻击范围时调用
    private void HandleEnterAttackRange(Transform enemy)
    {
        // 停止移动开始攻击
        // 注意当追踪的敌人消失，但是攻击范围内敌人没有全部消失时，不切换状态，攻击另一个敌人

        if (m_model.IsDead) return;

        // 只有当没有当前目标或当前目标无效时才设置新目标
        if (m_model.AttackTargetObject == null || !m_attackDetector.IsTargetTracked(m_model.AttackTargetObject))
        {
            // 但优先选择当前锁定的目标（如果有效）
            Transform validTarget = m_attackDetector.IsTargetTracked(m_lastTrackedTarget)
                ? m_lastTrackedTarget
                : enemy;

            m_model.AttackTargetObject = validTarget;
        }

        m_controller.ConvertState(SoldierStateType.IsStaying, true);
        m_controller.ConvertState(SoldierStateType.IsAttacking, true);
        m_mover.StopMoving();

    }

    // 锁定目标离开攻击范围时调用
    private void HandleExitAttackRange(Transform enemy)
    {
        if (enemy != m_model.AttackTargetObject) return;

        // 直接在攻击检测器中找新目标
        m_model.AttackTargetObject = m_attackDetector.GetNearestEnemy() 
                                ?? m_attackRangeDetector.GetNearestEnemy();
        
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
