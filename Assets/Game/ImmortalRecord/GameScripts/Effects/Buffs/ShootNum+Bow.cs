using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ShootNum+Bow", order = 102)]
class ShootNumBow : EffectBase
{
    [Header("射击数量加成")]
    public int ShootNumValue = 1;
    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.Archer, skill =>
        {
            if (skill is SoldierDataBase pd)
            {
                pd.projectileCount += ShootNumValue;
            }
        });
        
    }
}