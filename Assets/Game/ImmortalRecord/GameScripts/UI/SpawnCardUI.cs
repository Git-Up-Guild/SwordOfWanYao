// 文件名: SpawnCardUI.cs
// 挂载对象: SpawnCard_Prefab 预制件的根物体

using UnityEngine;
using UnityEngine.UI;
using TMPro;      // 如果你使用 TextMeshPro
using System;      // 为了使用 Action 事件

public class SpawnCardUI : MonoBehaviour
{
    // --- 在预制件的Inspector中链接这些UI元素 ---
    [Header("UI组件引用")]
    [SerializeField] private Image unitIcon;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private Image cooldownOverlay;

    // --- 兵种生产逻辑相关的私有变量 ---
    private float spawnInterval; // 这个兵种的生产总时长 (例如 10秒)
    private float cooldownTimer; // 当前的冷却倒计时
    private bool isInitialized = false; // 标记是否已初始化，防止在Update中过早执行

    /// <summary>
    /// 当冷却倒计时结束时触发的事件。
    /// 其他脚本（比如BattleUIManager）可以“订阅”这个事件，以便在冷却结束时收到通知。
    /// </summary>
    public Action OnCooldownComplete;

    void Update()
    {
        // 确保已经初始化后再执行倒计时逻辑
        if (!isInitialized)
        {
            return;
        }

        // 如果计时器正在倒计时...
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime; // 每帧减少计时器

            // 更新UI上旋转冷却遮罩的填充量
            if (cooldownOverlay != null && spawnInterval > 0)
            {
                cooldownOverlay.fillAmount = cooldownTimer / spawnInterval;
            }
        }
        else // 当 cooldownTimer <= 0 时，意味着冷却结束
        {
            // 1. 触发事件，通知外部脚本（如BattleUIManager）“该生产单位了！”
            // '?' 是空值检查，确保有对象订阅了这个事件才调用，防止报错。
            OnCooldownComplete?.Invoke();

            // 2. 重置计时器，立刻开始新一轮的冷却倒计时
            cooldownTimer = spawnInterval;
        }
    }

    /// <summary>
    /// 初始化这张卡片。由 BattleUIManager 在游戏开始时调用。
    /// </summary>
    /// <param name="icon">要显示的兵种图标</param>
    /// <param name="interval">该兵种的生产间隔（单位：秒）</param>
    public void Initialize(Sprite icon, float interval)
    {
        // 设置兵种图标
        if (unitIcon != null)
        {
            unitIcon.sprite = icon;
        }

        // 设置生产周期
        this.spawnInterval = interval;
        this.cooldownTimer = interval; // 让它从满冷却状态开始倒计时

        // 确保冷却遮罩可见并设置为满填充
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        // 标记为已初始化，Update中的逻辑可以开始执行了
        isInitialized = true;
    }

    /// <summary>
    // 更新在场单位数量的显示。由 BattleUIManager 在需要时调用。
    /// </summary>
    /// <param name="currentCount">当前战场上该兵种的数量</param>
    public void UpdatePopulation(int currentCount)
    {
        if (populationText != null)
        {
            populationText.text = currentCount.ToString();
        }
    }

    // 如果需要显示 "当前数量/最大上限" 的格式，可以使用这个重载版本

    //public void UpdatePopulation(int currentCount, int maxCount)
    //{
    //    if (populationText != null)
    //    {
    //        populationText.text = $"{currentCount}/{maxCount}";
    //    }
    //}

}