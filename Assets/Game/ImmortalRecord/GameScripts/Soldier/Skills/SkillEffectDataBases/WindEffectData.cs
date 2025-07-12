using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/SkillEffects/WindEffect", order = 19)]
public class WindEffectData : SkillEffectDataBase
{
    [Header("风眼效果设置")]
    public float pullStrength;
    public float moveSpeed;
}