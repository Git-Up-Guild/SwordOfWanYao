using UnityEngine;
using DG.Tweening;

public class CameraShaker : MonoBehaviour {
    public static CameraShaker Instance { get; private set; }

    private Vector3 initialLocalPos;
    private Tween currentShake;

    [Header("震动参数")]
    [SerializeField]
    private float strengthOffsetDivider = 20f;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {

            Destroy(gameObject);
            return;

        }

        Instance = this;
        initialLocalPos = transform.localPosition;

    }

    public void ShakeOnce(float strength = 1f, int vibrato = 10, float duration = 0.5f, float fadeOutTime = 0.2f) {

        if (currentShake != null && currentShake.IsActive())
            currentShake.Kill();

        transform.localPosition = initialLocalPos;

        strength /= strengthOffsetDivider; 

        currentShake = transform.DOShakePosition(
            duration,
            strength,
            vibrato,
            90,
            false,
            true
        ).OnKill(() => transform.localPosition = initialLocalPos)
         .OnComplete(() => transform.DOLocalMove(initialLocalPos, fadeOutTime));

    }

    public void StopShaking() {

        if (currentShake != null && currentShake.IsActive()) {

            currentShake.Kill();
            transform.localPosition = initialLocalPos;

        }

    }
}
