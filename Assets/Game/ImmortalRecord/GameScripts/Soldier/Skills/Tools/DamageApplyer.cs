using UnityEngine;
using XGame.FlowText;

public static class DamageApplyer
{
    private const float CRIT_RATE = 0.2f;
    private const float CRIT_MULTIPLIER = 2f;

    // 进行伤害计算、扣血并触发事件
    public static void ApplyDamage(SoldierModel attacker, SoldierModel defender, float baseDamage, Vector3 pos)
    {
        if (defender == null || defender.IsDead || attacker == null) return;

        bool isCrit = Random.value < CRIT_RATE;

        float attackValue = baseDamage * attacker.AttackPowerMutiplier;

        if (isCrit)
        {
            attackValue *= CRIT_MULTIPLIER;
        }

        int finalDamage = Mathf.Max(1, Mathf.CeilToInt(attackValue - defender.Defense));

        // 扣血
        defender.Health -= finalDamage;

        if (defender.Camp != SoldierCamp.Ally)
            ShowFloatText(finalDamage, isCrit, pos);

        // 发伤害事件
        EventManager.Instance.TriggerEvent(
            SoldierEventNames.Damaged,
            new SoldierDamagedEventData(defender, finalDamage, isCrit, pos)
        );
    }

    private static void ShowFloatText(int value, bool isCritical, Vector3 pos)
    {
        var flowTextMgr = XGame.XGameComs.Get<IFlowTextManager>();
        if (flowTextMgr == null)
        {
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        Vector2 uiPos = flowTextMgr.ScreenPositionToLayerLocalPosition(
            isCritical ? 102 : 101,
            screenPos,
            Camera.main
        );

        var context = new FlowTextContext
        {
            content = value.ToString(),
            startPosition = uiPos
        };

        flowTextMgr.AddFlowText(isCritical ? 102 : 101, context);
    }
}
