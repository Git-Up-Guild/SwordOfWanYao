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
            // DontDestroyOnLoad(gameObject); // 如果需要在不同场景间保持，可以取消这行注释
        }
    }
}