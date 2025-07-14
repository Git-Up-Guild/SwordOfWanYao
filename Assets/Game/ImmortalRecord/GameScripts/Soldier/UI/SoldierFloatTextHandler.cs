using UnityEngine;
using XGame.FlowText;

public class SoldierFloatTextHandler : MonoBehaviour
{
    private SoldierModel m_model;

    [Header("飘字配置")]
    [SerializeField] private int DamageTextId = 101;
    [SerializeField] private int CriticalDamageTextId = 102;

    private void Awake()
    {
        // m_model = GetComponent<SoldierModel>();
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.Damaged, this, OnDamaged);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.Unsubscribe<IEventData>(SoldierEventNames.Damaged, this, OnDamaged);
    }

    private void OnDamaged(IEventData evt)
    {
        //Debug.Log("######### send event");
        var data = evt as SoldierDamagedEventData;

        if (data == null) return;

        ShowFloatText(data.FinalDamage, data.IsCritical, data.worldpos);
    }

    private void ShowFloatText(int value, bool isCritical, Vector3 pos)
    {
        var flowTextMgr = XGame.XGameComs.Get<IFlowTextManager>();
        if (flowTextMgr == null)
        {
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        Vector2 uiPos = flowTextMgr.ScreenPositionToLayerLocalPosition(
            isCritical ? CriticalDamageTextId : DamageTextId,
            screenPos,
            Camera.main
        );

        var context = new FlowTextContext
        {
            content = value.ToString(),
            startPosition = uiPos
        };

        flowTextMgr.AddFlowText(isCritical ? CriticalDamageTextId : DamageTextId, context);
    }
}
