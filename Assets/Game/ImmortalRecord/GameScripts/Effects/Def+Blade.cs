using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefBlade", order = 103)]
class DefBlade : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=1;

    [Header("防御力加成倍率（如1.5为+50%）")]
    public float DefMultiplier=1.6f;

    public override void ApplyEffect(Soldiercontroller soldierController， soldierModel soldierModel)
    {
        if (soldier. == SoldierTypeID)
        {
            soldier.AttackPowerMutiplier *= DefMultiplier;

            Debug.Log($"Applied AtkBlade effect: {DefMultiplier} to {soldier.name}");
        }
        
    }
}