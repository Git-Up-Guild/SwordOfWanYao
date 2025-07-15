using UnityEngine;

public class HurtEffectHandler : MonoBehaviour
{
    private SoldierModel m_model;
    private HurtColorFlash flashEffect;

    private void Start()
    {
        m_model = GetComponent<SoldierModel>();
        flashEffect = GetComponentInChildren<HurtColorFlash>();

        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.HealthChanged, m_model, OnHealthChanged);
    }

    private void OnHealthChanged(object data)
    {
        var change = (SoldierModel.SoldierAttributeChangeData)data;

        // 判断是受到伤害（而不是回血）
        if (change.CurrentAmount < 0)
        {
            flashEffect?.Flash();
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.Unsubscribe<IEventData>(SoldierEventNames.HealthChanged, m_model, OnHealthChanged);
    }
}