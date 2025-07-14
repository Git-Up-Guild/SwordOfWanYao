using UnityEngine;
[CreateAssetMenu(menuName = "Buff/HealthAll", order = 104)]
class HealthAll : EffectBase
{
    [Header("全员生命值加成值")]
    public float HealthValue = 10;
    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        foreach (SoldierType type in System.Enum.GetValues(typeof(SoldierType)))
        {
            RuntimeSoldierAttributeHub.Instance.Modify(type, health => health.maxHealth += HealthValue);
        }
        Debug.Log($"全员增加防御力力Applied DefAll effect: x{HealthValue} to {soldierModel.name}");
    }
}