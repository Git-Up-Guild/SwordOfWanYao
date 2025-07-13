using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootTimesBow", order = 103)]
class ShootTimesBow : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=3;

    [Header("攻击频率增速（如1为+1次攻击）")]
    public int PlusTime=1;

    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        if (soldierModel.ID == SoldierTypeID)
        {
            soldierModel.AttackFrequency += PlusTime;

            Debug.Log($"Applied ShootTimesBlade effect: {PlusTime} to {soldierModel.name}");
        
        }
        
    }
}