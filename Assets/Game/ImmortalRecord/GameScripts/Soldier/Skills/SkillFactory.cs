using System;
using UnityEngine;

public static class SoldierSkillFactory
{
    public static SkillBase Create(SoldierModel model, SoldierSkillDataBase data)
    {
        switch (data.category)
        {
            case SkillCategory.Melee:
                if (data is MeleeSkillData meleeData)
                    return new MeleeSkill(model, meleeData);
                break;
            case SkillCategory.Projectile:
                if (data is ProjectileSkillData projectileData)
                    return new ProjectileSkill(model, projectileData);
                break;
            case SkillCategory.AreaOfEffect:
                if (data is AreaSkillData aoeData)
                    return new AOESkill(model, aoeData);
                break;

            default:
                Debug.LogWarning($"未处理的技能类型: {data.category}");
                break;
        }

        return null;
    }
}
