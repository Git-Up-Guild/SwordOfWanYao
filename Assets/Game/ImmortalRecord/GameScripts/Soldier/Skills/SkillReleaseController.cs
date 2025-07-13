using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SoldierModel), typeof(SoldierStateMachine))]
public class SkillReleaseController : MonoBehaviour
{
    private SoldierModel m_model;
    private Coroutine m_attackCoroutine;
    private SkillBase m_autoAttackSkill;

    private void Awake()
    {
        m_model = GetComponent<SoldierModel>();
    }

    private void Start()
    {
        SubscribeStateEvents();

        // 从运行时技能列表中选出自动攻击技能
        m_autoAttackSkill = m_model.RuntimeSkillList.FirstOrDefault(s => s.Data.isAutoReleased)
                             ?? m_model.RuntimeSkillList.FirstOrDefault();

        if (m_autoAttackSkill == null)
        {
            Debug.LogWarning($"[{nameof(SkillReleaseController)}] 找不到自动攻击技能，{gameObject.name}");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeStateEvents();
    }

    private void SubscribeStateEvents()
    {
        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.AttackingEntered, m_model, OnAttackStateEntered);
        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.AttackingExited, m_model, OnAttackStateExited);
    }

    private void UnsubscribeStateEvents()
    {

        if (!EventManager.Instance) return;

        EventManager.Instance.Unsubscribe<IEventData>(SoldierEventNames.AttackingEntered, m_model, OnAttackStateEntered);
        EventManager.Instance.Unsubscribe<IEventData>(SoldierEventNames.AttackingExited, m_model, OnAttackStateExited);
    }

    private void OnAttackStateEntered(IEventData evt)
    {
        if (m_attackCoroutine == null && m_autoAttackSkill != null)
            m_attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private void OnAttackStateExited(IEventData evt)
    {
        if (m_attackCoroutine != null)
        {
            StopCoroutine(m_attackCoroutine);
            m_attackCoroutine = null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        float interval = 1f / Mathf.Max(0.01f, m_model.AttackSpeed);

        while (true)
        {
            m_autoAttackSkill.Cast();
            yield return new WaitForSeconds(interval);
        }
    }
}
