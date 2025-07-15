// 文件名: VictoryPanelUI.cs
// 挂载对象: 胜利结算界面的主面板 VictoryPanel

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // 如果使用TextMeshPro
using UnityEngine.SceneManagement; // <-- 关键！添加这一行
public class VictoryPanelUI : MonoBehaviour
{
    // --- 单例模式 ---
    public static VictoryPanelUI Instance { get; private set; }

    // --- UI组件引用 ---
    [Header("主面板与背景")]
    [SerializeField] private GameObject dimmerBackground;
    [Header("奖励区域")]
    [SerializeField] private Transform rewardsGrid;
    [SerializeField] private GameObject rewardItemPrefab;
    [Header("操作按钮")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button doubleRewardButton;

    // --- 新增的数据引用 ---
    [Header("关卡数据源")]
    [Tooltip("将本关卡对应的LevelData资源文件拖到这里")]
    [SerializeField] private LevelData levelData;

    [SerializeField] private GameObject gameObjects;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }

        // 游戏开始时，确保面板是隐藏的
        gameObject.SetActive(false);
        if (dimmerBackground != null) dimmerBackground.SetActive(false);
    }

    private void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        if (doubleRewardButton != null) doubleRewardButton.onClick.AddListener(OnDoubleRewardButtonClicked);
        gameObjects.SetActive(false);


    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (doubleRewardButton != null) doubleRewardButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 显示胜利面板。它会自动从配置的levelData中获取奖励信息。
    /// </summary>
    public void Show() // <-- 修改点1：不再需要接收 VictoryData 参数
    {
        // 安全检查：确认levelData已经被配置
        if (levelData == null)
        {
            Debug.LogError("VictoryPanelUI上的LevelData未配置！无法显示奖励。");
            return;
        }

        // 激活面板和背景
        if (dimmerBackground != null) dimmerBackground.SetActive(true);
        gameObjects.SetActive(true);

        // --- 填充奖励UI ---
        // 1. 清理旧的奖励图标 (防止重玩时UI重复)
        foreach (Transform child in rewardsGrid)
        {
            Destroy(child.gameObject);
        }

        // 2. 从我们自己持有的 levelData 中获取奖励列表
        if (levelData.victoryRewards != null)
        {
            // 3. 遍历数据，根据数据生成新的奖励图标
            foreach (var rewardData in levelData.victoryRewards)
            {
                // 实例化预制件，并指定父物体为rewardsGrid
                GameObject itemGO = Instantiate(rewardItemPrefab, rewardsGrid);

                // 获取预制件上的脚本
                RewardItemUI itemUI = itemGO.GetComponent<RewardItemUI>();
                if (itemUI != null)
                {
                    // 调用Setup方法，把奖励数据传递过去
                    itemUI.Setup(rewardData);
                }
            }
        }
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("返回按钮被点击！");
        // --- 在这里添加场景跳转的核心逻辑 ---

        // 恢复时间流速，防止主菜单也被暂停
        Time.timeScale = 1f;

        // 加载你的主界面场景
        // 重要：将 "MainMenu" 替换成你主界面场景的真实文件名！
        SceneManager.LoadScene("UI1");
    }

    private void OnDoubleRewardButtonClicked()
    {
        Debug.Log("奖励翻倍按钮被点击！");
        // ...
    }
}