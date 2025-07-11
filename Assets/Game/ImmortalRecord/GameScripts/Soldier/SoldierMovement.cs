using UnityEngine;
using System.Collections;
using System;

public class SoldierMovement : MonoBehaviour
{
    private SoldierModel m_model;
    private SoldierController m_controller;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        m_model = GetComponent<SoldierModel>();
        m_controller = GetComponent<SoldierController>();

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
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        m_controller.ConvertState(SoldierStateType.IsStaying, true);

    }
}
