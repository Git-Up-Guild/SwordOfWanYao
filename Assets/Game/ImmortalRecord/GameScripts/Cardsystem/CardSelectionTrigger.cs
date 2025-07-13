using System;
using UnityEngine;

public class CardSelectionTrigger : MonoBehaviour
{
    private int killCount = 0;

    public static event Action OnCardDrawTriggered;

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

}
