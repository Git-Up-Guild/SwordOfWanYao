// UnitCardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCardUI : MonoBehaviour
{
    // --- 在预制件的Inspector中拖拽好 ---
    public Button summonButton;
    public Image unitIcon;
    public TextMeshProUGUI unitLevelText;
    public TextMeshProUGUI populationText;
    public Image cooldownOverlay; // 冷却遮罩

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float totalCooldown = 0f;

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownOverlay.fillAmount = cooldownTimer / totalCooldown;

            if (cooldownTimer <= 0)
            {
                EndCooldown();
            }
        }
    }

    public void Setup(Sprite icon, int level, string popText)
    {
        unitIcon.sprite = icon;
        unitLevelText.text = level.ToString();
        populationText.text = popText;
        EndCooldown();
    }

    public void StartCooldown(float duration)
    {
        isOnCooldown = true;
        totalCooldown = duration;
        cooldownTimer = duration;
        cooldownOverlay.gameObject.SetActive(true);
        summonButton.interactable = false;
    }

    private void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTimer = 0f;
        cooldownOverlay.gameObject.SetActive(false);
        summonButton.interactable = true;
    }
}