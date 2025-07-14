using UnityEngine;
[CreateAssetMenu(menuName = "Buff/Time+Wind", order = 102)]
class TimeWind : EffectBase
{
    [Header("兵种技能持续时间加成数（秒）")]
    public int PlusSec = 2;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.WindPriest, skill =>
        {
            if (skill is AreaSkillData pd)
                pd.duration += PlusSec;
        });
    }
}