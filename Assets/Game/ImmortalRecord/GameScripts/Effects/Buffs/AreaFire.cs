using UnityEngine;
[CreateAssetMenu(menuName = "Buff/AreaFire", order = 102)]
public class AreaFire : EffectBase
{
    [Header("增加半径比例")]
    public float AreaRadius = 1.2f;
    
    public override void ApplyEffect()
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.FireMage, skill =>
        {
            if (skill is AreaSkillData pd)
            {
                pd.scaleMultiplier *= AreaRadius;
            }
        });
    }
}