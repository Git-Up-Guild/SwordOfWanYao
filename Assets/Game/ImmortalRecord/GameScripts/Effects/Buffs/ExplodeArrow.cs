using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ExplodeArrow", order = 102)]
public class ExplodeArrow : EffectBase
{   
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.Archer, skill =>
        {
            if (skill is ProjectileSkillData pd)
                pd.canExplode = true;
        });
    }
}
