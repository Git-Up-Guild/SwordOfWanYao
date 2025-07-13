using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArShootNum+lighteaFire", order = 102)]
class ShootNumlight : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 6;

    
    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        
    }
}