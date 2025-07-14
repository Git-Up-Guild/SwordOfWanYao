using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootTimesFire", order = 103)]
class ShootTimesFire : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=4;

    [Header("攻击频率增速（如1为+1次攻击）")]
    public int PlusTime=1;

    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        if (soldierModel.ID == SoldierTypeID)
        {
            // soldier.AttackFrequency += PlusTime;
            //soldierController.SetAttackFrequency(soldierModel.AttackFrequency + PlusTime);
            // Debug.Log($"Applied ShootTimesBlade effect: {PlusTime} to {soldier.name}");

        }
        
    }
}