using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/SkillEffects/FreezeEffect", order = 18)]
public class FreezeEffectData : SkillEffectDataBase
{
    [Header("冰冻效果设置")]
    public float freezeDuration;
}