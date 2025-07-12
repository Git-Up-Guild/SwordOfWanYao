using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootTimesLance", order = 103)]
class ShootTimesLance : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=2;

    [Header("攻击频率增速（如1为+1次攻击）")]
    public int PlusTime=1;

    public override void ApplyEffect(Soldiercontroller soldierController， soldierModel soldierModel)
    {
        if (soldierModel.ID == SoldierTypeID)
        {
            soldier.AttackFrequency += PlusTime;

            Debug.Log($"Applied ShootTimesBlade effect: {PlusTime} to {soldier.name}");
        
        }
        
    }
}