using UnityEngine;
[CreateAssetMenu(menuName = "Buff/AreaLight", order = 102)]
class AreaLight : EffectBase
{
    [Header("增加范围比例")]
    public float AreaRadius = 1.2f;
    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.LightMonk, skill =>
        {
            if (skill is AreaSkillData pd)
            {
                pd.scaleMultiplier *= AreaRadius;
            }
        });
        
    }

}