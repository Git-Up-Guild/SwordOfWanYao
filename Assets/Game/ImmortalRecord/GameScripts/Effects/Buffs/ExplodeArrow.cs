using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ExplodeArrow", order = 102)]
class ExplodeArrow : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 3;

    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        
    }
}