using UnityEngine;
using System.Collections;
using System;

public class SoldierMovement : MonoBehaviour
{
    private SoldierModel m_model;
    private SoldierController m_controller;
    private SoldierSpriter m_spriter;
    private Coroutine moveCoroutine;

    private void OnEnable()
    {
        m_model = GetComponent<SoldierModel>();
        m_controller = GetComponent<SoldierController>();
        m_spriter = GetComponentInChildren<SoldierSpriter>();
    }

    public void MoveTo(Vector2 targetPos)
    {
        // 保留原逻辑，用于固定目标
        MoveTo(() => targetPos);
    }

    public void FollowDynamicTarget(Func<Vector2> dynamicTargetGetter)
    {
        // 命名为FollowDynamicTarget以区分用途
        MoveTo(dynamicTargetGetter);
    }

    private void MoveTo(Func<Vector2> getTargetPosition)
    {
        m_controller.ConvertState(SoldierStateType.IsMoving, true);

        if (!m_model.IsInitialized)
            CustomLogger.LogError("Data isn't initialized");

        // 动态更新目标指示器位置
        m_model.MoveTargetIndicator.position = getTargetPosition();

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveToCoroutine(getTargetPosition, m_model.MoveSpeed));
    }

    public void StopMoving()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        m_controller.ConvertState(SoldierStateType.IsStaying, true);
    }

    private IEnumerator MoveToCoroutine(Func<Vector2> dynamicTarget, float speed)
    {

        while (Vector2.Distance(transform.position, dynamicTarget()) > 0.05f && !m_model.IsDead && !m_model.IsFreezing)
        {
            Vector2 currentPos = transform.position;
            m_controller.ConvertState(SoldierStateType.IsMoving, true);
            Vector2 nextPos = Vector2.MoveTowards(
                transform.position,
                dynamicTarget(), // 实时获取目标位置
                speed * Time.deltaTime
            );

            Vector2 delta = nextPos - currentPos;

            // 判断移动方向来设置朝向
            if (delta.x > 0.01f)
            {
                m_spriter.Filp(false);
            }

            transform.position = nextPos;

            yield return null;
        }

        //m_controller.ConvertState(SoldierStateType.IsStaying, true);

    }

}
