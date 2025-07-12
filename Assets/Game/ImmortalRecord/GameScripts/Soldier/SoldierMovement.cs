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
        m_controller.ConvertState(SoldierStateType.IsMoving, true);

        if (!m_model.IsInitialized)
            CustomLogger.LogError("Data isn't initialized");

        m_model.MoveTargetIndicator.position = targetPos;

        float speed = m_model.MoveSpeed;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveToCoroutine(targetPos, speed));

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

    private IEnumerator MoveToCoroutine(Vector2 targetPos, float speed)
    {

        while (Vector2.Distance(transform.position, targetPos) > 0.05f && !m_model.IsDead && !m_model.IsFreezing)
        {
            Vector2 currentPos = transform.position;
            Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);

            Vector2 delta = nextPos - currentPos;

            // 判断移动方向来设置朝向
            if (delta.x > 0.01f)
            {
                m_spriter.Filp(false);
            }

            transform.position = nextPos;

                yield return null;
            }

        transform.position = targetPos;
        m_controller.ConvertState(SoldierStateType.IsStaying, true);

    }
}
