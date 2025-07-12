using UnityEngine;
[CreateAssetMenu(menuName = "Buff/AtkWind", order = 103)]
class AtkWind : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=5;

    [Header("攻击力加成倍率（如1.5为+50%）")]
    public float AtkValue=1.6f;

    public override void ApplyEffect(Soldiercontroller soldierController， soldierModel soldierModel)
    {
        if (soldierModel.ID == SoldierTypeID)
        {
            soldier.AttackPowerMutiplier *= AtkValue;

            Debug.Log($"Applied AtkBlade effect: {AtkValue} to {soldier.name}");
        
        }
        
    }
}