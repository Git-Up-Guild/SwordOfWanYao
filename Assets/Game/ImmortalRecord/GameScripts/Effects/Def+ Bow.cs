using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefBow", order = 103)]
class DefBow : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=3;

    [Header("防御力加成倍率（如1.5为+50%）")]
    public float DefMultiplier=1.6f;

    public override void ApplyEffect(SoldierModel soldier)
    {
        if (soldier.ID == SoldierTypeID)
        {
            soldier.AttackPowerMutiplier *= DefMultiplier;

            Debug.Log($"Applied AtkBlade effect: {DefMultiplier} to {soldier.name}");
        
        }
        
    }
}