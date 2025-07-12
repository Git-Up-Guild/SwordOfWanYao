using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/SkillEffects/LightBeamRotationEffect", order = 20)]
public class LightBeamRotationEffect : SkillEffectDataBase
{
    [Header("光柱旋转效果设置")]
    public float angle;
    public float rotatingSpeed;
}