using UnityEngine;

[CreateAssetMenu(menuName = "Buff/DefAll", order = 102)]
public class DefAll : EffectBase
{
    [Header("全员防御力加成倍率（如1.5为+50%）")]
    public float defMultiplier = 1.1f;

    public override void ApplyEffect(SoldierController soldier)
    {
        // 获取当前防御力
        float currentDef = soldier.m_model.Defense;
        // 计算加成后的防御力
        float newDef = currentDef * defMultiplier;
        // 设置新防御力
        soldier.SetDefense(newDef);

        Debug.Log($"全员增加防御力 Applied DefAll effect: x{defMultiplier} to {soldier.name}");
    }
}
