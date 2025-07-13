using UnityEngine;

public class SkillCastPivot : MonoBehaviour
{

    private SoldierModel m_model;
    private Transform targetObject;
    private float m_castingOffset;

    void Awake()
    {

        m_model = GetComponentInParent<SoldierModel>();

    }

    void Start()
    {

        m_castingOffset = (transform.position - m_model.transform.position).magnitude;

    }

    void Update()
    {

        targetObject = m_model.AttackTargetObject;

        if (targetObject == null) return;

        Vector3 direction = (m_model.AttackTargetObject.position - m_model.transform.position).normalized;
        transform.position = m_model.transform.position + direction * m_castingOffset;

    }


}