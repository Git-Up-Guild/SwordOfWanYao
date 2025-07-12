using UnityEngine;
using UnityEngine.UIElements;

public class SoldierSpriter : MonoBehaviour
{
    private SoldierModel m_model;

    [Header("渲染设置")]
    [SerializeField] float scaleSizeX;
    [SerializeField] float scaleSizeY;

    private void OnEnable()
    {
        m_model = GetComponentInParent<SoldierModel>();
        ScaleToConfiguredScale();
        SubscribeEvents();
    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            UnsubscribeEvents();
        }
    }

    private void ScaleToConfiguredScale()
    {
        transform.localScale = new Vector3(scaleSizeX, scaleSizeY, 1);
    }

    public void ScaleTo(float targetSizeX, float targetSizeY)
    {
        transform.localScale = new Vector3(targetSizeX, targetSizeY, 1);
    }

    public void Filp(bool faceLeft)
    {
        float sizeX = faceLeft ? scaleSizeX : -scaleSizeX;

        ScaleTo(sizeX, scaleSizeY);
    }

    [SerializeField] private Animator animator;

    private int isMovingHash = Animator.StringToHash("IsMoving");
    private int isStayingHash = Animator.StringToHash("IsStaying");
    private int isInvincibleHash = Animator.StringToHash("IsInvincible");
    private int isDeadHash = Animator.StringToHash("IsDead");
    private int isFreezingHash = Animator.StringToHash("IsFreezing");

    private void PlayDeath() => animator.SetBool(isDeadHash, true);
    private void SetInvincible(bool val) => animator.SetBool(isInvincibleHash, val);
    private void SetFreezing(bool val) => animator.SetBool(isFreezingHash, val);
    private void SetMoving(bool val) => animator.SetBool(isMovingHash, val);
    private void SetStaying(bool val) => animator.SetBool(isStayingHash, val);

    private void SubscribeEvents()
    {
        var mgr = EventManager.Instance;
        var src = m_model;

        mgr.Subscribe<IEventData>(SoldierEventNames.Died, src, HandleDeath);

        mgr.Subscribe<IEventData>(SoldierEventNames.InvincibleEntered, src, _ => SetInvincible(true));
        mgr.Subscribe<IEventData>(SoldierEventNames.InvincibleExited,  src, _ => SetInvincible(false));

        mgr.Subscribe<IEventData>(SoldierEventNames.MovingEntered,     src, _ => SetMoving(true));
        mgr.Subscribe<IEventData>(SoldierEventNames.MovingExited,      src, _ => SetMoving(false));

        mgr.Subscribe<IEventData>(SoldierEventNames.StayingEntered,    src, _ => SetStaying(true));
        mgr.Subscribe<IEventData>(SoldierEventNames.StayingExited,     src, _ => SetStaying(false));

        mgr.Subscribe<IEventData>(SoldierEventNames.FreezingEntered,   src, _ => SetFreezing(true));
        mgr.Subscribe<IEventData>(SoldierEventNames.FreezingExited,    src, _ => SetFreezing(false));
    }

    private void UnsubscribeEvents()
    {
        var mgr = EventManager.Instance;
        var src = m_model;

        mgr.Unsubscribe<IEventData>(SoldierEventNames.Died, src, HandleDeath);

        mgr.Unsubscribe<IEventData>(SoldierEventNames.InvincibleEntered, src, _ => SetInvincible(true));
        mgr.Unsubscribe<IEventData>(SoldierEventNames.InvincibleExited,  src, _ => SetInvincible(false));

        mgr.Unsubscribe<IEventData>(SoldierEventNames.MovingEntered,     src, _ => SetMoving(true));
        mgr.Unsubscribe<IEventData>(SoldierEventNames.MovingExited,      src, _ => SetMoving(false));

        mgr.Unsubscribe<IEventData>(SoldierEventNames.StayingEntered,    src, _ => SetStaying(true));
        mgr.Unsubscribe<IEventData>(SoldierEventNames.StayingExited,     src, _ => SetStaying(false));

        mgr.Unsubscribe<IEventData>(SoldierEventNames.FreezingEntered,   src, _ => SetFreezing(true));
        mgr.Unsubscribe<IEventData>(SoldierEventNames.FreezingExited,    src, _ => SetFreezing(false));
    }

    private void HandleDeath(object data) => PlayDeath();

}