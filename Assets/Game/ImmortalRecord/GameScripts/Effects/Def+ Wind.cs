using UnityEngine;
[CreateAssetMenu(menuName = "Buff/DefWind", order = 103)]
class DefWind : EffectBase
{
    [Header("兵种类型ID")]
    public int SoldierTypeID=5;

    [Header("防御力加成倍率（如1.5为+50%）")]
    public float DefMultiplier=1.6f;

    public override void ApplyEffect(SoldierModel soldierModel, SoldierController soldierController)
    {
        if (soldierModel.ID == SoldierTypeID)
        {
            float currentDef = soldierModel.CurrentDefense;
            // 计算加成后的防御力  
            float newDef = currentDef * DefMultiplier;
            // 设置新防御力
            soldierController.SetDefense(newDef);
            
            Debug.Log($"Applied AtkBlade effect: {DefMultiplier} to {soldierModel.name}");
        
        }
        
    }
}