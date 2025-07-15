using UnityEngine;
using UnityEngine.UI;

public class BaseHealthDisplay : MonoBehaviour
{
    [Header("血条 UI 绑定")]
    [SerializeField] private Slider healthSlider;

    [Header("平滑显示参数")]
    [SerializeField] private float smoothTime = 0.1f;

    private float healthVelocity;
    private float targetHealth;
    private IDestructible m_base;

    private void Start()
    {
        m_base = GetComponentInParent<IDestructible>();

        healthSlider.maxValue = m_base.GetMaxHealth(); // 初始 max = 当前（假设此时满血）
        targetHealth = m_base.GetHealth();
        healthSlider.value = targetHealth;

    }

    private void Update()
    {
        float current = m_base.GetHealth();
        if (Mathf.Abs(targetHealth - current) > 0.01f)
            targetHealth = current;

        healthSlider.value = Mathf.SmoothDamp(
            healthSlider.value,
            targetHealth,
            ref healthVelocity,
            smoothTime
        );
    }
}