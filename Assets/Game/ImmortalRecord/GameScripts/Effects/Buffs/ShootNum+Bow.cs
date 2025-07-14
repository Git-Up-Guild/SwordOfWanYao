using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootNum+Bow", order = 102)]
class ShootNumBow : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 3;

    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        
    }
}