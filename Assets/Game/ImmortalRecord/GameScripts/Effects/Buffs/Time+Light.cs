using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Time+Light", order = 102)]
class TimeLight : EffectBase
{
    [Header("兵种技能持续时间加成数（秒）")]
    public int PlusSec = 2;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.LightMonk, skill =>
        {
            if (skill is AreaSkillData pd)
                pd.duration += PlusSec;
        });
    }
}