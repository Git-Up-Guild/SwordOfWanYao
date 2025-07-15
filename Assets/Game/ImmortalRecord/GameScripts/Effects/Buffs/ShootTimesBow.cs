using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootTimesBow", order = 103)]
public class ShootTimesBow : EffectBase
{
    [Header("兵种类型")]
    public SoldierType soldierType= SoldierType.Archer;

    [Header("连发数加成数（如1.5为+50%）")]
    public int AtkFrequen = 1;

    public override void ApplyEffect()
    {
        
        RuntimeSoldierAttributeHub.Instance.Modify
        (
            soldierType,
            fre => fre.attackFrequency += AtkFrequen
        );        
        
    }
}