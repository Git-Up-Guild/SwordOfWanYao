using System;
using UnityEngine;

public class CardSelectionTrigger : MonoBehaviour
{
    public static CardSelectionTrigger Instance { get; private set; }

    public static event Action OnCardDrawTriggered;
    
    public int killCount = 0;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // 调用此方法以记录击杀
    public void RegisterKill()
    {
        killCount++;
        if (killCount % 10 == 0)
        {
            // 触发抽卡事件
            OnCardDrawTriggered?.Invoke();
            Time.timeScale = 0f;
        }
    }
    public void TestTriggerDraw()
    {
        Debug.Log("测试按钮被点击，强制触发抽卡！");
        OnCardDrawTriggered?.Invoke();
        Time.timeScale = 0f;
    }
}
