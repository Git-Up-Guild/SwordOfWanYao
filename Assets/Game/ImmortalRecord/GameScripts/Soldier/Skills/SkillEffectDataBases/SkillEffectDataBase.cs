using UnityEngine;

public class SkillEffectDataBase : ScriptableObject
{
    public EffectType effectType;
    [Tooltip("触发几率(0-1)")]
    public float chance = 1f;
}

public enum EffectType
{
    Explosion,
    Split,
    Freeze,
    Wind,
    LightBeamRotation
}