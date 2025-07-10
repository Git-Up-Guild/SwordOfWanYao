using UnityEngine;

public abstract class EffectBase : ScriptableObject
{
    public abstract void ApplyEffect(SoldierController soldierController);
}
