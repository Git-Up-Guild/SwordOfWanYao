using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefFire", order = 103)]
class DefFire : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.FireMage;

    [Header("防御力加成倍率（如1.5为+50%）")]
    public int DefValue=10;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        
        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            def => def.defense += DefValue
        );        
        
    }
}