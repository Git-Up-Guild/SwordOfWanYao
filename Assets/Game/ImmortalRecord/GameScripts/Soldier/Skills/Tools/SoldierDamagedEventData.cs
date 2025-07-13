public class SoldierDamagedEventData : IEventData
{
    public SoldierModel Target;        // 受击者
    public int FinalDamage;            // 扣血数值
    public bool IsCritical;            // 是否暴击

    public SoldierDamagedEventData(SoldierModel target, int damage, bool isCritical)
    {
        Target = target;
        FinalDamage = damage;
        IsCritical = isCritical;
    }

    object IEventData.Source => Target;
}
