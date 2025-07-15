using UnityEngine;

public class SoldierDamagedEventData : IEventData
{
    public SoldierModel Target;        // 受击者
    public int FinalDamage;            // 扣血数值
    public bool IsCritical;            // 是否暴击
    public Vector3 worldpos;

    public SoldierDamagedEventData(SoldierModel target, int damage, bool isCritical, Vector3 pos)
    {
        Target = target;
        FinalDamage = damage;
        IsCritical = isCritical;
        worldpos = pos;
    }

    object IEventData.Source => Target;
}
