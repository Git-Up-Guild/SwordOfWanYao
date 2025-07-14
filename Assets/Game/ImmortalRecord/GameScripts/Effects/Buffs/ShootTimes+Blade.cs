using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootTimesBlade", order = 103)]
class ShootTimesBlade : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.Blade;

    [Header("连发数加成数（如1.5为+50%）")]
    public int AtkFrequen=1;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        
        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            fre => fre.attackFrequency += AtkFrequen
        );        
        
    }
}