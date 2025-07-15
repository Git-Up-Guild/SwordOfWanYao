using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArrowSplit3", order = 102)]
public  class ArrowSplit3 : EffectBase
{
    [Header("箭矢是否可以分裂")]
    public bool IsSplit = true;

    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        if(IsSplit)
        {
            RuntimeSoldierSkillHub.Instance.Modify(SoldierType.Archer, skill =>
            {
                if (skill is ProjectileSkillData pd)
                {
                    pd.canSplit = true;
                }
            });
        }
        
    }
}