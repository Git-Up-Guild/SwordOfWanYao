using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefLight", order = 103)]
public class DefLight : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.LightMonk;

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