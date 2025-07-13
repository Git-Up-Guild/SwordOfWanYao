using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArrowPenetrade", order = 102)]
class ArrowPenetrade : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 3;

    
    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        
    }
}