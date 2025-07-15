// 文件名: VictoryPanelUI.cs
// 挂载对象: 胜利结算界面的主面板 VictoryPanel

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // 如果使用TextMeshPro

public class VictoryPanelUI : MonoBehaviour
{
    [Header("主面板与背景")]
    [SerializeField] private GameObject dimmerBackground; // 半透明背景遮罩

    [Header("奖励区域")]
    [SerializeField] private Transform rewardsGrid; // 奖励物品网格的父物体
    [SerializeField] private GameObject rewardItemPrefab; // 单个奖励物品的预制件

    [Header("战斗统计区域")]
    [SerializeField] private Transform statsContent; // 统计列表Scroll View的Content
    [SerializeField] private GameObject statRowPrefab; // 单条统计数据的预制件

    [Header("操作按钮")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button doubleRewardButton;

    // --- 为了演示，我们在Start中调用测试 ---
    // 实际游戏中，这个Show方法应该由你的GameManager在战斗胜利时调用
    void Start()
    {
        // 游戏开始时先隐藏自己
        gameObject.SetActive(false);
        if (dimmerBackground != null) dimmerBackground.SetActive(false);

        // --- 测试代码 ---
        // 创建一些假的结算数据来测试UI显示
        CreateAndShowTestData();
    }

    private void OnEnable()
    {
        // 为按钮绑定事件
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        if (doubleRewardButton != null) doubleRewardButton.onClick.AddListener(OnDoubleRewardButtonClicked);
    }

    private void OnDisable()
    {
        // 移除事件监听，好习惯
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (doubleRewardButton != null) doubleRewardButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 显示胜利面板并用数据填充UI
    /// </summary>
    /// <param name="data">包含所有结算信息的数据包</param>
    public void Show(VictoryData data)
    {
        // 激活面板和背景
        if (dimmerBackground != null) dimmerBackground.SetActive(true);
        gameObject.SetActive(true);

        // --- 填充奖励UI ---
        // 1. 清理旧的奖励图标
        foreach (Transform child in rewardsGrid)
        {
            Destroy(child.gameObject);
        }
        // 2. 根据数据生成新的奖励图标
        foreach (var rewardData in data.rewards)
        {
            GameObject itemGO = Instantiate(rewardItemPrefab, rewardsGrid);
            // 这里你需要一个 RewardItemUI.cs 脚本挂在预制件上，用来设置图标和数量
            // itemGO.GetComponent<RewardItemUI>().Setup(rewardData.itemIcon, rewardData.quantity);
        }

        // --- 填充战斗统计UI ---
        // 1. 清理旧的统计行
        foreach (Transform child in statsContent)
        {
            Destroy(child.gameObject);
        }
        // 2. 根据数据生成新的统计行
        foreach (var statData in data.unitStats)
        {
            GameObject rowGO = Instantiate(statRowPrefab, statsContent);
            // 这里你需要一个 StatRowUI.cs 脚本挂在预制件上，用来填充所有数据
            // rowGO.GetComponent<StatRowUI>().Setup(statData);
        }
    }

    // --- 按钮点击事件处理 ---
    private void OnBackButtonClicked()
    {
        Debug.Log("返回按钮被点击！");
        // 在这里添加返回主菜单或关卡选择的逻辑
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


    private void OnDoubleRewardButtonClicked()
    {
        Debug.Log("奖励翻倍按钮被点击！");
        // 在这里添加观看广告并分发双倍奖励的逻辑
    }


    // --- 仅用于测试的辅助方法 ---
    private void CreateAndShowTestData()
    {
        VictoryData testData = new VictoryData();

        // 创建假的奖励数据
        testData.rewards = new List<RewardItemData>
        {
            new RewardItemData { itemIcon = null, quantity = 1 },
            new RewardItemData { itemIcon = null, quantity = 150 },
            new RewardItemData { itemIcon = null, quantity = 1 },
            new RewardItemData { itemIcon = null, quantity = 7 },
            new RewardItemData { itemIcon = null, quantity = 20 },
            new RewardItemData { itemIcon = null, quantity = 3 },
            new RewardItemData { itemIcon = null, quantity = 15 },
        };

        // 创建假的战斗统计数据
        testData.unitStats = new List<UnitStatData>
        {
            new UnitStatData { unitName = "火法师", unitLevel = 4, damagePercentage = 0.266f, damageValueText = "69.4万", tankPercentage = 0.027f, tankValueText = "12244" },
            new UnitStatData { unitName = "轻骑兵", unitLevel = 4, damagePercentage = 0.204f, damageValueText = "53.1万", tankPercentage = 0.353f, tankValueText = "15.7万" },
            new UnitStatData { unitName = "长枪兵", unitLevel = 5, damagePercentage = 0.203f, damageValueText = "52.9万", tankPercentage = 0.256f, tankValueText = "11.4万" },
            new UnitStatData { unitName = "冰法师", unitLevel = 4, damagePercentage = 0.165f, damageValueText = "43.1万", tankPercentage = 0.025f, tankValueText = "11335" },
        };

        // 用假数据来显示UI
        Show(testData);
    }
}