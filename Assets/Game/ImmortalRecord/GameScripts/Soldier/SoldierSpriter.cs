using UnityEngine;
using UnityEngine.UIElements;

public class SoldierSpriter : MonoBehaviour
{
    private SoldierModel m_Model;

    [Header("渲染设置")]
    [SerializeField] float scaleSize;

    public void ScaleTo(float targetSizeX, float targetSizeY)
    {

        transform.localScale = new Vector3(targetSizeX, targetSizeY, 1);

    }

    public void Filp(bool faceRight)
    {

        float sizeX = faceRight ? scaleSize : -scaleSize;

        ScaleTo(sizeX, scaleSize);

    }

    [SerializeField] private Animator animator;

    private int isMovingHash = Animator.StringToHash("IsMoving");
    private int isStayingHash = Animator.StringToHash("IsStaying");
    private int isInvincibleHash = Animator.StringToHash("IsInvincible");
    private int isDeadHash = Animator.StringToHash("IsDead");
    private int isFreezingHash = Animator.StringToHash("IsFreezing");

    private void OnEnable()
    {
        m_Model = GetComponentInParent<SoldierModel>();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            UnsubscribeEvents();
        }
    }

    private void PlayDeath() => animator.SetBool(isDeadHash, true);
    private void SetInvincible(bool val) => animator.SetBool(isInvincibleHash, val);
    private void SetFreezing(bool val) => animator.SetBool(isFreezingHash, val);
    private void SetMoving(bool val) => animator.SetBool(isMovingHash, val);
    private void SetStaying(bool val) => animator.SetBool(isStayingHash, val);

    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.Died,
            HandleDeath);

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.InvincibleEntered,
            _ => SetInvincible(true)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.InvincibleExited,
            _ => SetInvincible(false)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.MovingEntered,
            _ => SetMoving(true)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.MovingExited,
            _ => SetMoving(false)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.StayingEntered,
            _ => SetStaying(true)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.StayingExited,
            _ => SetStaying(false)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.FreezingEntered,
            _ => SetFreezing(true)
        );

        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.StayingExited,
            _ => SetFreezing(false)
        );

    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.Died,
            HandleDeath);

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.InvincibleEntered,
            _ => SetInvincible(true)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.InvincibleExited,
            _ => SetInvincible(false)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.MovingEntered,
            _ => SetMoving(true)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.MovingExited,
            _ => SetMoving(false)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.StayingEntered,
            _ => SetStaying(true)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.StayingEntered,
            _ => SetStaying(false)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.FreezingEntered,
            _ => SetFreezing(true)
        );

        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.FreezingExited,
            _ => SetFreezing(false)
        );
    }


    private void HandleDeath(object data) => PlayDeath();

}