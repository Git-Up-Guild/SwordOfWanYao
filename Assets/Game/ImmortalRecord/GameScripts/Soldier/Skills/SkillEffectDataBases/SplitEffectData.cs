using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/SkillEffects/SplitEffect", order = 17)]
public class SplitEffectData : SkillEffectDataBase
{
    [Header("分裂效果设置")]
    public int splitCount;
    public float splitDamage;
}
