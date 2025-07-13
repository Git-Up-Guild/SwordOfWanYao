using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Time+Light", order = 102)]
class TimeLight : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 6;

    
    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        
    }
}