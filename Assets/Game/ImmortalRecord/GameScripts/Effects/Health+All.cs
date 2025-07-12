using UnityEngine;
class HealthAll : EffectBase
{
    public int HealthValue;

    public override void ApplyEffect(SoldierModel soldier)
    {
        // Apply the health value to the soldier
        soldier.Health *= HealthValue;

        // Log the effect application for debugging
        Debug.Log($"Applied HealthAll effect: {HealthValue} to {soldier.name}");
    }
}