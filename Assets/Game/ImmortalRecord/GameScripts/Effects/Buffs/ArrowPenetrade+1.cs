using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArrowPenetrade", order = 102)]
class ArrowPenetrade : EffectBase
{
    

    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.Archer, skill =>
        {
    if (skill is ProjectileSkillData pd)
        pd.maxPierce += 1;
        });
    }
}