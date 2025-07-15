using System;
using UnityEngine;

public class CardSelectionTrigger : MonoBehaviour
{
    public static CardSelectionTrigger Instance { get; private set; }

    public static event Action OnCardDrawTriggered;

    [Header("抽卡触发参数")]
    [Tooltip("第一次触发抽卡所需的击杀数")]
    [SerializeField] private int initialKillsToTrigger = 5;

    [Tooltip("每次触发后，下一次所需击杀数的增量")]
    [SerializeField] private int killsIncrement = 2;

    // --- 私有状态变量 ---
    private int killsSinceLastDraw = 0; // 自上次抽卡以来的击杀计数
    private int nextKillTarget; // 下一个触发抽卡的目标击杀数
    public int killCount = 0;
    private bool isActive = false; // 新增：一个开关，控制它是否开始计数

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 初始化第一次的目标
        nextKillTarget = initialKillsToTrigger;
    }

    /// <summary>
    /// 公共方法：由敌人死亡时调用，用来记录一次击杀。
    /// </summary>
    public void RegisterKill()
    {
        killsSinceLastDraw++;
        Debug.Log($"击杀计数: {killsSinceLastDraw} / {nextKillTarget}");

        // 检查是否达到了触发条件
        if (killsSinceLastDraw >= nextKillTarget)
        {
            TriggerCardDraw();
        }
    }

    // 专门用于游戏开始时第一次抽卡的方法
    private void TriggerFirstDraw()
    {
        Debug.Log("游戏开始，进行初始抽卡！");
        OnCardDrawTriggered?.Invoke();
        // 这里的暂停逻辑应该由CardSelectionManager处理，所以我们注释掉
        // Time.timeScale = 0f;
    }
    // 专门用于游戏中期击杀达标后抽卡的方法
    private void TriggerCardDraw()
    {
        Debug.Log($"击杀数达到目标 {nextKillTarget}！触发抽卡！");
        OnCardDrawTriggered?.Invoke();

        // --- 核心逻辑：更新下一次的目标 ---
        // 1. 将本次的击杀计数清零（或减去目标数）
        killsSinceLastDraw = 0;
        // 2. 增加下一次的目标数
        nextKillTarget += killsIncrement;

        Debug.Log($"下一次抽卡需要击杀 {nextKillTarget} 个敌人。");
    }

    public void TestTriggerDraw()
    {
        Debug.Log("测试按钮被点击，强制触发抽卡！");
        OnCardDrawTriggered?.Invoke();
        Time.timeScale = 0f;
    }
}
