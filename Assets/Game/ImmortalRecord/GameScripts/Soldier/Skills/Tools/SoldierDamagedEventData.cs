public class SoldierDamagedEventData : IEventData
{
    public SoldierModel Source;        // 攻击者
    public SoldierModel Target;        // 受击者
    public int FinalDamage;            // 扣血数值
    public bool IsCritical;            // 是否暴击

    public SoldierDamagedEventData(SoldierModel source, SoldierModel target, int damage, bool isCritical)
    {
        Source = source;
        Target = target;
        FinalDamage = damage;
        IsCritical = isCritical;
    }

    object IEventData.Source => Source;
}
