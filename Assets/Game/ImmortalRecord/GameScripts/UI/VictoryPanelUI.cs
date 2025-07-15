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


   
}