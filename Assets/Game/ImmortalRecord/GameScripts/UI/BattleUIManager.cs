// 文件名: BattleUIManager.cs (最终完整版)
// 挂载对象: 场景中的 BattleUIManager GameObject

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    #region 字段定义 (Fields & Properties)

    // --- 模块一：游戏控制面板 ---
    [Header("模块一：游戏控制面板")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [SerializeField] private Button gameSpeedButton;
    [SerializeField] private TextMeshProUGUI speedButtonText;

    private float timer = 0f;

    // --- 模块二：关卡信息与经验条 ---
    [Header("模块二：关卡信息面板")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TextMeshProUGUI levelText; // 这个现在用来显示等级
    [SerializeField] private TextMeshProUGUI killCountText; // 新增：专门显示击杀数的文本

    private int killCount = 0; // 用于累计击杀的变量

    // --- 模块三：自动生产面板 ---
    [Header("模块三：自动生产面板")]
    [SerializeField] private List<SpawnCardUI> spawnSlots;

    // 这个变量现在用来存储玩家当前的出战队列
    private List<UnitData> playerDeck = new List<UnitData>(5);

    // 为了测试，我们直接在这里定义兵种数据
    [System.Serializable]
    public class SpawnableUnitInfo
    {
        public string unitName;
        public Sprite icon;
        public float spawnInterval;
    }
    [SerializeField] private List<SpawnableUnitInfo> unitSpawnList;

    #endregion


    #region Unity生命周期函数 (Unity Lifecycle)

    // 当脚本实例被加载时调用
    private void Awake()
    {
        // 这里可以放一些单例模式的初始化代码，如果需要的话
    }

    // 当这个脚本组件被激活时调用 (在Start之前)
    private void OnEnable()
    {
        // 开始收听（订阅）由EnemyManager广播的敌人死亡事件
        EnemyManager.OnEnemyDied += HandleEnemyDied;
    }

    // 当这个脚本组件被禁用或销毁时调用
    private void OnDisable()
    {
        // 停止收听（取消订阅）敌人的死亡事件，防止内存泄漏
        EnemyManager.OnEnemyDied -= HandleEnemyDied;
    }

    // 游戏开始第一帧前调用
    void Start()
    {
        // --- 初始化模块一 ---
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseButtonClicked);
        if (gameSpeedButton != null) gameSpeedButton.onClick.AddListener(OnGameSpeedButtonClicked);
        Time.timeScale = 1.0f;
        if (speedButtonText != null) speedButtonText.text = "x1";

        // --- 初始化模块二 (使用测试数据) ---
        SetLevelName("3-天下大乱");
        UpdateLevel(1);
        UpdateExperience(0.3f);
        UpdateKillCountUI(); // 初始化击杀数显示为0

        // --- 初始化模块三 ---
        InitializeSpawnSlots();
        // 初始化时，我们假设出战队列是空的
        InitializeSpawnPanel();

        // -------------------------------------------------------------------
        // --- 在这里添加新逻辑：游戏开始时强制进行一次抽卡 ---
        // -------------------------------------------------------------------

        // 延迟一小段时间再调用，确保所有东西都初始化完毕，避免奇怪的冲突。
        // 0.1秒的延迟肉眼基本看不出来，但对程序稳定性很有好处。
        Invoke(nameof(TriggerInitialCardDraw), 0.1f);
    }

    // --- 在脚本中添加这个新的方法 ---
    private void TriggerInitialCardDraw()
    {
        // 检查CardSelectionManager是否存在
        if (CardSelectionManager.Instance != null)
        {
            Debug.Log("游戏开始，进行初始抽卡！");

            // 直接调用 CardSelectionManager 的 ShowCards 方法
            // 并为它提供一个确认选择后的回调函数
            CardSelectionManager.Instance.ShowCards(selectedCard => {
                // 当玩家在这次初始抽卡中确认选择后，这里的代码会被执行
                Debug.Log($"初始卡牌选择完成: {selectedCard.Name}");

                // 我们需要在这里手动应用效果，因为这次不是通过Trigger触发的
                // 所以需要复制一份 ApplyCardEffect 的逻辑
                if (GameManagers.Instance != null && GameManagers.Instance.buffManager != null)
                {
                    GameManagers.Instance.buffManager.AddBuff(selectedCard.Effect);

                    // 刚开始游戏，场上没有单位，所以不需要刷新场上单位
                }
            });
        }
        else
        {
            Debug.LogError("CardSelectionManager 未找到，无法进行初始抽卡！");
        }
    }

    private void InitializeSpawnPanel()
    {
        for (int i = 0; i < spawnSlots.Count; i++)
        {
            // 检查出战队列中这个位置是否有单位
            if (i < playerDeck.Count && playerDeck[i] != null)
            {
                // 如果有，就初始化卡片
                UnitData unitData = playerDeck[i];
                spawnSlots[i].Initialize(unitData.icon, unitData.spawnInterval);

                int unitIndex = i;
                spawnSlots[i].OnCooldownComplete += () => { SpawnUnit(unitIndex); };

                spawnSlots[i].UpdatePopulation(0); // 初始人口为0
                spawnSlots[i].gameObject.SetActive(true); // 显示卡片
            }
            else
            {
                // 如果没有，就隐藏这个卡槽，表示是空的
                spawnSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void AddUnitToDeck(UnitData newUnitData, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spawnSlots.Count)
        {
            Debug.LogError("无效的槽位索引！");
            return;
        }

        // 确保列表足够大
        while (playerDeck.Count <= slotIndex)
        {
            playerDeck.Add(null);
        }

        // 更新数据
        playerDeck[slotIndex] = newUnitData;

        // 立即刷新UI
        RefreshSingleSpawnSlot(slotIndex);
    }

    // 刷新单个槽位的显示
    private void RefreshSingleSpawnSlot(int slotIndex)
    {
        UnitData unitData = playerDeck[slotIndex];
        SpawnCardUI cardUI = spawnSlots[slotIndex];

        if (unitData != null)
        {
            // 初始化卡片并显示
            cardUI.Initialize(unitData.icon, unitData.spawnInterval);

            // 重新绑定事件 (先移除旧的，再添加新的)
            cardUI.OnCooldownComplete = null;
            cardUI.OnCooldownComplete += () => { SpawnUnit(slotIndex); };

            cardUI.UpdatePopulation(0);
            cardUI.gameObject.SetActive(true);
        }
        else
        {
            // 如果数据为空，则隐藏卡片
            cardUI.gameObject.SetActive(false);
        }
    }
    // 每一帧调用
    void Update()
    {
        // --- 更新模块一的计时器 ---
        if (gameTimerText != null)
        {
            timer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer % 60F);
            gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    #endregion


    #region 事件处理 (Event Handlers)

    // --- 新增：处理敌人死亡事件的方法 ---
    private void HandleEnemyDied()
    {
        killCount++;
        UpdateKillCountUI();
    }

    #endregion


    #region UI更新与交互方法 (UI Update & Interaction)

    // --- 模块一的方法 ---
    private void OnPauseButtonClicked()
    {
        Debug.Log("游戏暂停按钮被点击");
        // ...
    }

    private void OnGameSpeedButtonClicked()
    {
        if (Time.timeScale < 2.0f)
        {
            Time.timeScale = 2.0f;
            if (speedButtonText != null) speedButtonText.text = "x2";
        }
        else
        {
            Time.timeScale = 1.0f;
            if (speedButtonText != null) speedButtonText.text = "x1";
        }
    }

    // --- 模块二的方法 ---
    public void SetLevelName(string name)
    {
        if (levelNameText != null) levelNameText.text = name;
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = level + "级";
    }

    public void UpdateExperience(float fillRatio)
    {
        if (xpFillImage != null) xpFillImage.fillAmount = Mathf.Clamp01(fillRatio);
    }

    // 新增：专门更新击杀数UI的方法
    private void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "击杀: " + killCount;
        }
    }

    // --- 模块三的方法 ---
    private void InitializeSpawnSlots()
    {
        if (spawnSlots.Count != unitSpawnList.Count)
        {
            Debug.LogError("UI槽位数量与兵种数据数量不匹配！请在BattleUIManager的Inspector中检查。");
            return;
        }

        for (int i = 0; i < spawnSlots.Count; i++)
        {
            spawnSlots[i].Initialize(unitSpawnList[i].icon, unitSpawnList[i].spawnInterval);
            int unitIndex = i;
            spawnSlots[i].OnCooldownComplete += () => { SpawnUnit(unitIndex); };
            spawnSlots[i].UpdatePopulation(0);
        }
    }

    private void SpawnUnit(int unitIndex)
    {
        // 确保索引有效
        if (unitIndex < playerDeck.Count && playerDeck[unitIndex] != null)
        {
            UnitData unitToSpawn = playerDeck[unitIndex];
            Debug.Log($"时间到！请求生产一个 '{unitToSpawn.unitName}'!");
            // GameManager.Instance.SpawnUnit(unitToSpawn.unitPrefab);
        }
    }

    public void UpdateUnitPopulationDisplay(int unitIndex, int currentCount)
    {
        if (unitIndex >= 0 && unitIndex < spawnSlots.Count)
        {
            spawnSlots[unitIndex].UpdatePopulation(currentCount);
        }
    }

    #endregion
}