using UnityEngine;
using SwordOfWanYao;

[CreateAssetMenu(menuName = "Buff/AttackRangeAll", order = 102)]
public class AttackRangeAll : EffectBase
{
    [Header("全员攻击范围加成倍率（如1.5为+50%）")]
    public float rangeMultiplier = 1.5f;

    public override void ApplyEffect(SoldierController soldierController)
    {
        // 获取当前攻击范围
        float currentRange = soldierController.m_model.AttackRange;
        // 计算加成后的攻击范围
        float newRange = currentRange * rangeMultiplier;
        // 设置新攻击范围
        soldierController.SetAttackRange(newRange);

        // 更新攻击范围检测器的缩放
        if (soldierController.m_model.AttackRangeDetector != null)
        {
            soldierController.m_model.AttackRangeDetector.localScale *= rangeMultiplier;
        }

        Debug.Log($"全员增加攻击范围 Applied AttackRangeAll effect: x{rangeMultiplier} to {soldierController.name}");
    }
} 