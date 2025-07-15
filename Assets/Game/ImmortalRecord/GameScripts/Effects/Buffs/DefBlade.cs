using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefBlade", order = 103)]
public class DefBlade : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.Blade;

    [Header("防御力加成倍率（如1.5为+50%）")]
    public int DefValue=10;

    public override void ApplyEffect()
    {
        
        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            def => def.defense += DefValue
        );        
        
    }
}