using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Time+Wind", order = 102)]
class TimeWind : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 5;

    
    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        
    }
}