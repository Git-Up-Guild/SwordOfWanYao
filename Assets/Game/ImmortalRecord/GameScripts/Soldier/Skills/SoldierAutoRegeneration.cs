using UnityEngine;
using System.Collections;

public class SoldierAutoRegeneration : MonoBehaviour
{
    [Header("回血设置")]
    [SerializeField] private int healAmount = 5;
    [SerializeField] private float healInterval = 1.5f;

    private SoldierModel m_model;
    private Coroutine m_healCoroutine;

    private void Awake()
    {
        m_model = GetComponent<SoldierModel>();
    }

    private void Start()
    {
        if (m_model != null && m_healCoroutine == null)
        {
            m_healCoroutine = StartCoroutine(HealLoop());
        }
    }

    private void OnDisable()
    {
        if (m_healCoroutine != null)
        {
            StopCoroutine(m_healCoroutine);
            m_healCoroutine = null;
        }
    }

    private IEnumerator HealLoop()
    {
        while (!m_model.IsDead)
        {
            m_model.Health += healAmount;
            yield return new WaitForSeconds(healInterval);
        }
    }
}
