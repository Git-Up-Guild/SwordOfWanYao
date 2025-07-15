using UnityEngine;
using System.Collections;

public class HurtColorFlash : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private Color originalColor = Color.white;
    private Coroutine flashCoroutine;

    [Header("受伤时闪烁的颜色")]
    public Color flashColor = Color.red;

    [Header("闪烁持续时间")]
    public float duration = 0.1f;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        foreach (var r in renderers)
            r.color = flashColor;

        yield return new WaitForSeconds(duration);

        foreach (var r in renderers)
            r.color = originalColor;

        flashCoroutine = null;
    }
}