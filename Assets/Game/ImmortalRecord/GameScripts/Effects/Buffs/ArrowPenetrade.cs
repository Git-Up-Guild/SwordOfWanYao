using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArrowPenetrade", order = 102)]
public class ArrowPenetrade : EffectBase
{
    

    
    public override void ApplyEffect()
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.Archer, skill =>
        {
    if (skill is ProjectileSkillData pd)
        pd.maxPierce += 1;
        });
    }
}