using UnityEngine;
[CreateAssetMenu(menuName = "Buff/AreaWind", order = 102)]
public class AreaWind : EffectBase
{
    [Header("增加范围数量")]
    public float AreaRadius = 1.2f;
    public override void ApplyEffect()
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.WindPriest, skill =>
        {
            if (skill is AreaSkillData pd)
            {
                pd.scaleMultiplier *= AreaRadius; 
            }
        });
        
    }

}