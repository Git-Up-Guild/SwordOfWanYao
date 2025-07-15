// 文件名: CardSelectionManager.cs
// 挂载对象: 战斗场景中的 CardSelectionManager GameObject

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro; // 如果你使用 TextMeshPro
using SwordOfWanYao; // 确保引用了你队友定义的命名空间
using System.Collections; // 引入协程所需的命名空间

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
    
    private Dictionary<CardConfig, int> cardInventory = new Dictionary<CardConfig, int>();
    private bool isAwaitingPlayerChoice = false; // 新增：标记是否正在等待玩家选择

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
        CardSelectionTrigger.OnCardDrawTriggered -= TriggerMidGameCardDraw;
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

        StartCoroutine(OpeningSequenceRoutine());
    }

    /// <summary>
    /// 核心的开局流程协程
    /// </summary>
    private IEnumerator OpeningSequenceRoutine()
    {
        // --- 第一次抽卡 ---
        Debug.Log("--- 开始进行第一次初始抽卡 ---");
        isAwaitingPlayerChoice = true;
        ShowCards(); // 直接调用，不再传递回调

        yield return new WaitUntil(() => !isAwaitingPlayerChoice);
        Debug.Log("--- 第一次初始抽卡选择完成 ---");

        // --- 第二次抽卡 ---
        Debug.Log("--- 开始进行第二次初始抽卡 ---");
        isAwaitingPlayerChoice = true;
        ShowCards(); // 直接调用

        yield return new WaitUntil(() => !isAwaitingPlayerChoice);
        Debug.Log("--- 第二次初始抽卡选择完成 ---");

        // --- 流程结束，恢复游戏时间 ---
        Time.timeScale = 1f;

        // --- 解锁全局卡池并进入常规游戏阶段 ---
        if (UnlockManager.Instance != null)
        {
            UnlockManager.Instance.UnlockGlobalAttributeCards();
        }

        // 现在，才开始监听游戏中期的击杀触发事件
        CardSelectionTrigger.OnCardDrawTriggered += TriggerMidGameCardDraw;
        Debug.Log("初始流程结束，已开始监听常规抽卡触发。");
    }
    // 当游戏中期击杀达标时，调用此方法
    private void TriggerMidGameCardDraw()
    {
        ShowCards();
    }


    #endregion


    #region 核心抽卡与逻辑连接

    // 当收到抽卡信号时，执行此方法
    private void TriggerCardSelection()
    {
        // 调用ShowCards，并定义一个回调函数，告诉它确认选择后该做什么
        ShowCards();
            
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
    public void ShowCards()
    {
        
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
        {// 1. 应用效果
            ApplyCardEffect(selectedCard);


            if (cardInventory.ContainsKey(selectedCard))
            {
                cardInventory[selectedCard]++;
            }

            // --- 在这里添加新的逻辑 ---
            // 如果这是一张解锁兵种的卡，就通知UnlockManager记录下来
            if (selectedCard.Type == CardType.UnlockUnit)
            {
                if (UnlockManager.Instance != null)
                {
                    UnlockManager.Instance.RecordUnlockCardSelected(selectedCard);
                }
            }

            if (CardPanel != null) CardPanel.SetActive(false);
            Time.timeScale = 1f;

            if (toggleGroup != null) toggleGroup.SetAllTogglesOff();
            // 5. 关键！直接打开协程的门闩！
            isAwaitingPlayerChoice = false;


        }
    }

    private List<CardConfig> GetAvailableCards()
    {
        List<CardConfig> available = new List<CardConfig>();

        // 检查UnlockManager是否存在
        if (UnlockManager.Instance == null)
        {
            Debug.LogError("UnlockManager未找到，无法正确筛选卡牌！");
            return available; // 返回一个空列表，避免后续错误
        }
        // 获取当前全局卡是否解锁的状态
        bool globalsUnlocked = UnlockManager.Instance.AreGlobalAttributeCardsUnlocked();


        foreach (var card in CardPool)
        {
            // --- 新的筛选逻辑 ---
            // 1. 如果是全局属性卡，但全局卡尚未解锁，则跳过
            if (card.Type == CardType.GlobalAttribute && !globalsUnlocked)
            {
                continue;
            }

            // 2. 检查拥有数量和前置条件（旧逻辑保持不变）
            bool canOwnMore = GetCardCount(card) < card.MaxOwnable;
            bool requirementsMet = UnlockManager.Instance.AreRequirementsMetForCard(card);

            if (canOwnMore && requirementsMet)
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