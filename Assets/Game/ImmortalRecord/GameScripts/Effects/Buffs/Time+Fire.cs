using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Time+Fire", order = 102)]
class TimeFire : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 4;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        
    }
}