using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff/AtkAll", order = 102)]
public class AtkAll : EffectBase
{
    [Header("全员攻击力加成倍率（如1.5为+50%）")]
    public float atkMultiplier = 1.1f;
    
    public override void ApplyEffect(SoldierModel soldier)
    {
        // 乘算攻击力倍率
        soldier.AttackPowerMutiplier *= atkMultiplier;

        // 调试日志
        Debug.Log($"全员增加攻击力Applied AtkAll effect: x{atkMultiplier} to {soldier.name}");
    }
}