using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefLance", order = 103)]
class DefLance : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.Spear;

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