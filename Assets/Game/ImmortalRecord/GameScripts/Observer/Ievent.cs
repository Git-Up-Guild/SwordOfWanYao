public interface IEventData
{
    object Source { get; }

}

public static class SoldierEventNames
{
    // 属性变更类事件
    public const string HealthChanged = "Soldier.HealthChanged";
    public const string Damaged = "Soldier.Damaged";

    // 士兵死亡
    public const string Died = "Soldier.State.Died";

    // 状态进入/退出事件（每种状态都一进一出）
    public const string InvincibleEntered = "Soldier.State.Invincible.Enter";
    public const string InvincibleExited = "Soldier.State.Invincible.Exit";

    public const string TargetingAtBaseEntered = "Soldier.State.TargetingAtOppositeBase.Enter";
    public const string TargetingAtBaseExited = "Soldier.State.TargetingAtOppositeBase.Exit";

    public const string LockingOnEntered = "Soldier.State.LockingOnEnemy.Enter";
    public const string LockingOnExited = "Soldier.State.LockingOnEnemy.Exit";

    public const string AttackingEntered = "Soldier.State.Attacking.Enter";
    public const string AttackingExited = "Soldier.State.Attacking.Exit";

    public const string StayingEntered = "Soldier.State.Staying.Enter";
    public const string StayingExited = "Soldier.State.Staying.Exit";

    public const string MovingEntered = "Soldier.State.Moving.Enter";
    public const string MovingExited = "Soldier.State.Moving.Exit";

    public const string FreezingEntered = "Soldier.State.Freezing.Enter";
    public const string FreezingExited = "Soldier.State.Freezing.Exit";

}