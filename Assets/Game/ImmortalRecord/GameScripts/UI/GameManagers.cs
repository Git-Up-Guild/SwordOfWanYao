// 文件名: GameManager.cs
using UnityEngine;

public class GameManagers : MonoBehaviour
{
    // 使用单例模式，让其他脚本可以通过 GameManager.Instance 访问它
    public static GameManagers Instance { get; private set; }

    // 在Inspector中，把我们创建的BuffManager资源文件拖到这里
    [Header("全局管理器引用")]
    public BuffManager buffManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // --- 在这里执行所有需要重置的操作 ---
            InitializeManagers();
        }
    }
    private void InitializeManagers()
    {
        // 检查BuffManager是否存在，然后调用它的重置方法
        if (buffManager != null)
        {
            buffManager.ResetBuffs();
        }

        // 以后如果你有其他需要重置的管理器，也在这里调用
        // if (unlockManager != null)
        // {
        //     unlockManager.ResetUnlocks();
        // }
    }
}