using UnityEngine;
using SwordOfWanYao;

[CreateAssetMenu(menuName = "Buff/DefAll", order = 102)]
public class DefAll : EffectBase
{
    [Header("全员攻击力加成倍率（如1.5为+50%）")]
    public float DefValue = 10;
    
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        foreach (SoldierType type in System.Enum.GetValues(typeof(SoldierType)))
        {
            RuntimeSoldierAttributeHub.Instance.Modify(type, def => def.defense += DefValue);
        }
        Debug.Log($"全员增加防御力力Applied DefAll effect: x{DefValue} to {soldierModel.name}");
    }
}
