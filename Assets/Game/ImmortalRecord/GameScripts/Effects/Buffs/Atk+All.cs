using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff/AtkAll", order = 102)]
public class AtkAll : EffectBase
{
    [Header("全员攻击力加成倍率（如1.5为+50%）")]
    public float atkMultiplier = 1.1f;
    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        foreach (SoldierType type in System.Enum.GetValues(typeof(SoldierType)))
        {
            RuntimeSoldierAttributeHub.Instance.Modify(type, attr => attr.attackPowerMutiplier *= atkMultiplier);
        }
        Debug.Log($"全员增加攻击力Applied AtkAll effect: x{atkMultiplier} to {soldierModel.name}");
    }
}