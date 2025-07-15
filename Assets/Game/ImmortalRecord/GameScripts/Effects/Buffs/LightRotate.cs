using UnityEngine;

[CreateAssetMenu(menuName = "Buff/LightRotate", order = 105)]
public class LightRotate : EffectBase
{
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.LightMonk, skill =>
        {
            if (skill is AreaSkillData area)
                area.canRotate = true;
        });

        Debug.Log("光罗汉光柱旋转增益已生效");
    }
}