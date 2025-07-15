using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefBow", order = 103)]
public class DefBow : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.Archer;

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