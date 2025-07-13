using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Fire", order = 103)]
class AtkFire : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=4;

    [Header("攻击力加成倍率（如1.5为+50%）")]
    public float AtkValue=1.6f;

    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        if (soldierModel.ID == SoldierTypeID)
        {
            soldierModel.AttackPowerMutiplier *= AtkValue;

            Debug.Log($"Applied AtkBlade effect: {AtkValue} to {soldierModel.name}");
        
        }
        
    }
}