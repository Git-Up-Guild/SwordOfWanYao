using UnityEngine;
using SwordOfWanYao;

[CreateAssetMenu(menuName = "Buff/AttackRangeAll", order = 102)]
public class AttackRangeAll : EffectBase
{
    [Header("全员攻击范围加成倍率（如1.5为+50%）")]
    public float rangeValue = 1.5f;
    [Header("兵种类型")]
    public SoldierType soldierType = SoldierType.Archer;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        // 设置新攻击范围

        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            rang => rang.attackRange *= rangeValue
        );        

        Debug.Log($"弓兵增加攻击范围 Applied AttackRangeAll effect: x{rangeValue} to {soldierModel.name}");
    }
} 