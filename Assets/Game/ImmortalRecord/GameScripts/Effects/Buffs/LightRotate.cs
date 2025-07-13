using UnityEngine;
[CreateAssetMenu(menuName = "Buff/LightRotate", order = 102)]
class LightRotate : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID = 6;

    
    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        
    }
}