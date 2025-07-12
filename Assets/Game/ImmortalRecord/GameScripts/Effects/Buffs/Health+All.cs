using UnityEngine;
class HealthAll : EffectBase
{
    public int HealthValue;

    public override void ApplyEffect(SoldierController soldierController, SoldierModel soldierModel)
    {
        // Apply the health value to the soldier
        soldierModel.Health *= HealthValue;

        // Log the effect application for debugging
        Debug.Log($"Applied HealthAll effect: {HealthValue} to {soldierModel.name}");
    }
}