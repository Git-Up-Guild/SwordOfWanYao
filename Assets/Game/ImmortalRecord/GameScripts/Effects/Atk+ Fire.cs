using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Fire", order = 103)]
class AtkFire : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=4;

    [Header("攻击力加成倍率（如1.5为+50%）")]
    public float AtkValue=1.6f;

    public override void ApplyEffect(SoldierModel soldier)
    {
        if (soldier.ID == SoldierTypeID)
        {
            soldier.AttackPowerMutiplier *= AtkValue;

            Debug.Log($"Applied AtkBlade effect: {AtkValue} to {soldier.name}");
        
        }
        
    }
}