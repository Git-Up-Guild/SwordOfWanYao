using UnityEngine;
[CreateAssetMenu(menuName = "Buff/ArShootNum+lighteaFire", order = 102)]
class ShootNumlight : EffectBase
{
    [Header("射击数量加成")]
    public int ShootNumValue = 1;
    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.LightMonk, skill =>
        {
            if (skill is SoldierDataBase pd)
            {
                pd.projectileCount += ShootNumValue;
            }
        });
        
    }
}