// 文件名: LevelSwitchManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // 如果你使用TextMeshPro，请保留这行
using System.Collections.Generic;

public class LevelSwitchManager : MonoBehaviour
{
    [Header("1. 关卡数据库 (按顺序拖入所有关卡数据文件)")]
    [SerializeField] private List<LevelData> levelDatabase;

    [Header("2. UI组件引用")]
    [SerializeField] private RectTransform imageSlider;         // 所有关卡图片所在的滑动容器
    [SerializeField] private TextMeshProUGUI levelNumberText;   // 显示“第 X 关”的文本
    [SerializeField] private TextMeshProUGUI levelNameText;     // 显示关卡名称的文本
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button battleButton;

    [Header("3. 动画参数")]
    [SerializeField] private float slideSpeed = 10f; // 图片滑动的速度

    private int currentLevelIndex = 0;
    private float levelWidth;
    private Vector2 targetSliderPosition;

    void Start()
    {
        // 安全检查
        if (levelDatabase == null || levelDatabase.Count == 0)
        {
            Debug.LogError("关卡数据库未配置或为空！请在Inspector中拖入关卡数据。");
            // 禁用所有按钮，防止出错
            leftArrowButton.interactable = false;
            rightArrowButton.interactable = false;
            battleButton.interactable = false;
            return;
        }

        // 获取单个关卡的宽度 (我们假设所有关卡图片一样宽)
        levelWidth = imageSlider.GetComponent<RectTransform>().rect.width;

        // 绑定按钮事件
        leftArrowButton.onClick.AddListener(GoToPreviousLevel);
        rightArrowButton.onClick.AddListener(GoToNextLevel);
        battleButton.onClick.AddListener(StartBattle);

        // 初始化显示第一个关卡
        UpdateUI(true); // 传入true，表示是初始化，瞬间定位
    }

    void Update()
    {
        // 使用Vector2.Lerp让滑动条平滑地移动到目标位置
        imageSlider.anchoredPosition = Vector2.Lerp(imageSlider.anchoredPosition, targetSliderPosition, Time.deltaTime * slideSpeed);
    }

    private void GoToNextLevel()
    {
        if (currentLevelIndex < levelDatabase.Count - 1)
        {
            currentLevelIndex++;
            UpdateUI();
        }
    }

    private void GoToPreviousLevel()
    {
        if (currentLevelIndex > 0)
        {
            currentLevelIndex--;
            UpdateUI();
        }
    }

    // 更新所有UI元素
    // isInstant参数用于区分是初始化还是后续切换
    private void UpdateUI(bool isInstant = false)
    {
        LevelData currentLevel = levelDatabase[currentLevelIndex];

        // 1. 更新文本信息
        levelNumberText.text = "第 " + currentLevel.levelNumber + " 关";
        levelNameText.text = currentLevel.levelName;

        // 2. 计算并设置滑动条的目标位置
        targetSliderPosition = new Vector2(-currentLevelIndex * levelWidth, imageSlider.anchoredPosition.y);

        // 如果是初始化，就瞬间移动到目标位置
        if (isInstant)
        {
            imageSlider.anchoredPosition = targetSliderPosition;
        }

        // 3. 更新箭头按钮的可交互状态
        leftArrowButton.interactable = (currentLevelIndex > 0);
        rightArrowButton.interactable = (currentLevelIndex < levelDatabase.Count - 1);
    }

    public void StartBattle()
    {
        LevelData selectedLevel = levelDatabase[currentLevelIndex];

        // 检查场景名是否为空
        if (string.IsNullOrEmpty(selectedLevel.sceneNameToLoad))
        {
            Debug.LogError($"关卡 '{selectedLevel.levelName}' 没有设置要加载的场景名！");
            return;
        }

        Debug.Log($"准备进入关卡: {selectedLevel.levelName}, 加载场景: {selectedLevel.sceneNameToLoad}");

        // 加载战斗场景
        SceneManager.LoadScene(selectedLevel.sceneNameToLoad);
    }
}