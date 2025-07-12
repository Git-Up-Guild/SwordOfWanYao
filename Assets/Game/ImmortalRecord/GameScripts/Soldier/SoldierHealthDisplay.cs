using UnityEngine;
using UnityEngine.UI;

public class SoldierHealthDisplay : MonoBehaviour
{
    [Header("绑定的 Slider")]
    [SerializeField] private Slider healthSlider;

    [Header("平滑移动参数")]
    [SerializeField] private float smoothTime = 0.1f;

    private float healthVelocity;
    private float targetHealth;

    private SoldierModel m_model;

    private void Start()
    {

        m_model = GetComponentInParent<SoldierModel>();

        targetHealth = m_model.Health;

        healthSlider.maxValue = m_model.MaxHealth;
        healthSlider.value = targetHealth;

        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.HealthChanged, m_model, OnHealthChanged);

    }

    private void Update()
    {

        healthSlider.value = Mathf.SmoothDamp(
            healthSlider.value,
            targetHealth,
            ref healthVelocity,
            smoothTime
        );

    }

    private void OnHealthChanged(object data)
    {

        var changeData = (SoldierModel.SoldierAttributeChangeData)data;
        healthSlider.maxValue = m_model.MaxHealth;

        targetHealth = changeData.CurrentValue;

        healthSlider.gameObject.SetActive(changeData.CurrentValue >= 0);

    }

    private void OnDisable()
    {

        if (EventManager.Instance == null) return;

        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.HealthChanged, m_model, OnHealthChanged);

    }

}