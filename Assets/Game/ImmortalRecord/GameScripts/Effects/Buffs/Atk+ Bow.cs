using UnityEngine;
[CreateAssetMenu(menuName = "Buff/AtkBow", order = 103)]
class AtkBow : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType;

    [Header("攻击力加成倍率（如1.5为+50%）")]
    public float AtkValue=1.6f;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        
        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            attr => attr.attackPowerMutiplier *= AtkValue
        );        
    }
}