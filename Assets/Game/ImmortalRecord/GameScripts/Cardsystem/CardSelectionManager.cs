// 文件名: CardSelectionManager.cs
// 挂载对象: 战斗场景中的 CardSelectionManager GameObject

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro; // 如果你使用 TextMeshPro
using SwordOfWanYao; // 确保引用了你队友定义的命名空间

public class CardSelectionManager : MonoBehaviour
{
    // 使用单例模式，方便其他脚本（如触发器）轻松访问
    public static CardSelectionManager Instance { get; private set; }

    [Header("UI 组件引用")]
    [Tooltip("抽卡时弹出的主面板")]
    public GameObject CardPanel;
    [Tooltip("3个用于显示卡牌选项的UI槽位")]
    public CardUI[] CardUISlots;
    [Tooltip("确认选择的按钮")]
    public Button confirmButton;
    [Tooltip("管理卡牌选项单选的ToggleGroup")]
    public ToggleGroup toggleGroup;

    [Header("预留功能面板 (暂不配置)")]
    public GameObject CollectionPanel;
    public GameObject MenuButton;
    public GameObject DeathPanel;

    [Header("卡牌数据配置")]
    [Tooltip("所有可能被抽到的卡牌都在这里配置")]
    public List<CardConfig> CardPool;

    // --- 私有变量 ---
    private CardConfig selectedCard;
    private Action<CardConfig> onConfirmedCallback;
    private Dictionary<CardConfig, int> cardInventory = new Dictionary<CardConfig, int>();


    #region Unity 生命周期

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        // 订阅抽卡触发事件
        CardSelectionTrigger.OnCardDrawTriggered += TriggerCardSelection;
        Debug.Log("CardSelectionManager 已开始收听抽卡事件。"); // 添加调试日志
    }

    private void OnDisable()
    {
        // 在对象销毁时取消订阅，防止内存泄漏
        CardSelectionTrigger.OnCardDrawTriggered -= TriggerCardSelection;
        Debug.Log("CardSelectionManager 已停止收听抽卡事件。"); // 添加调试日志
    }

    void Start()
    {
        InitializeInventory();

        if (confirmButton != null)
        {
            // 确保只添加一次监听器，或者在添加前移除旧的
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (CardPanel != null) CardPanel.SetActive(false);
        if (MenuButton != null) MenuButton.SetActive(false);
        if (CollectionPanel != null) CollectionPanel.SetActive(false);
    }

    #endregion


    #region 核心抽卡与逻辑连接

    // 当收到抽卡信号时，执行此方法
    private void TriggerCardSelection()
    {
        // 调用ShowCards，并定义一个回调函数，告诉它确认选择后该做什么
        ShowCards(selectedCardConfig => {
            // 当玩家确认选择后，这里的代码会被执行
            Debug.Log("玩家最终选择了卡牌: " + selectedCardConfig.Name);

            // --- 核心连接逻辑：在这里应用效果 ---
            ApplyCardEffect(selectedCardConfig);
        });
    }

    /// <summary>
    /// 将选中的卡牌效果应用到游戏中
    /// </summary>
    private void ApplyCardEffect(CardConfig selectedCardConfig)
    {
        // 检查 GameManager 和 BuffManager 是否存在
        if (GameManager.Instance == null || GameManagers.Instance.buffManager == null)
        {
            Debug.LogError("GameManager 或 BuffManager 未找到！无法应用卡牌效果。");
            return;
        }

        // 1. 将新选择的卡牌效果添加到全局Buff管理器中
        GameManagers.Instance.buffManager.AddBuff(selectedCardConfig.Effect);

        // 2. 立即将这个新Buff应用到所有已在场上的士兵身上
        //    (更准确地说，是重新应用所有Buff，以确保状态一致性)
        SoldierModel[] allSoldiers = FindObjectsOfType<SoldierModel>();
        foreach (var soldier in allSoldiers)
        {
            GameManagers.Instance.buffManager.ApplyAllBuffs(soldier);
        }

        Debug.Log($"已将效果 '{selectedCardConfig.Effect.name}' 应用到 {allSoldiers.Length} 个单位上。");
    }

    /// <summary>
    /// 显示抽卡界面，提供随机卡牌供玩家选择。
    /// </summary>
    /// <param name="onConfirmed">当玩家点击确认按钮后要执行的回调函数</param>
    public void ShowCards(Action<CardConfig> onConfirmed)
    {
        this.onConfirmedCallback = onConfirmed;
        selectedCard = null;
        if (confirmButton != null) confirmButton.interactable = false;

        List<CardConfig> availableCards = GetAvailableCards();
        List<CardConfig> randomCards = GetRandomCards(3, availableCards);

        for (int i = 0; i < CardUISlots.Length; i++)
        {
            if (i < randomCards.Count)
            {
                CardUISlots[i].gameObject.SetActive(true);
                CardUISlots[i].Initialize(randomCards[i], toggleGroup, OnCardToggled);
                CardUISlots[i].UpdateCount(GetCardCount(randomCards[i]) + "/" + randomCards[i].MaxOwnable);
            }
            else
            {
                CardUISlots[i].gameObject.SetActive(false);
            }
        }

        if (CardPanel != null) CardPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    #endregion


    #region 私有辅助方法

    private void InitializeInventory()
    {
        cardInventory.Clear();
        foreach (var card in CardPool)
        {
            if (!cardInventory.ContainsKey(card))
            {
                cardInventory.Add(card, 0);
            }
        }
    }

    private void OnCardToggled(CardConfig card, bool isOn)
    {
        if (isOn)
        {
            selectedCard = card;
            if (confirmButton != null) confirmButton.interactable = true;
        }
    }

    private void OnConfirmClicked()
    {
        if (selectedCard != null)
        {
            if (cardInventory.ContainsKey(selectedCard))
            {
                cardInventory[selectedCard]++;
            }

            if (CardPanel != null) CardPanel.SetActive(false);
            Time.timeScale = 1f;

            if (toggleGroup != null) toggleGroup.SetAllTogglesOff();

            onConfirmedCallback?.Invoke(selectedCard);
        }
    }

    private List<CardConfig> GetAvailableCards()
    {
        List<CardConfig> available = new List<CardConfig>();
        foreach (var card in CardPool)
        {
            if (GetCardCount(card) < card.MaxOwnable)
            {
                available.Add(card);
            }
        }
        return available;
    }

    private List<CardConfig> GetRandomCards(int count, List<CardConfig> sourceList)
    {
        List<CardConfig> shuffled = new List<CardConfig>(sourceList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
        int finalCount = Mathf.Min(count, shuffled.Count);
        return shuffled.GetRange(0, finalCount);
    }

    public int GetCardCount(CardConfig card)
    {
        return cardInventory.TryGetValue(card, out int count) ? count : 0;
    }

    #endregion
}