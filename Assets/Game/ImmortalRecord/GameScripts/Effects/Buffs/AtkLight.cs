using UnityEngine;
[CreateAssetMenu(menuName = "Buff/AtkLight", order = 103)]
public class AtkLight : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.LightMonk;

    [Header("攻击力加成倍率（如1.5为+50%）")]
    public float AtkValue=1.6f;

    public override void ApplyEffect()
    {
        
        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            attr => attr.attackPowerMutiplier *= AtkValue
        );        
        
    }
}
