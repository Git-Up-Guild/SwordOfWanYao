using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArrowSplit3", order = 102)]
class ArrowSplit3 : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 3;

    
    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        
    }
}