// 文件名: EnemyManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    // 使用单例模式，方便其他脚本（比如敌人出生点）访问
    public static EnemyManager Instance { get; private set; }

    // --- “敌人死亡”事件现在由这个管理器来广播 ---
    public static event Action OnEnemyDied;

    // 一个列表，用来追踪所有在场上的敌人
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        // 设置单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // 在每一帧的后期执行，这能确保在敌人执行完自己的逻辑后再检查
    void LateUpdate()
    {
        // 从后往前遍历列表，这样在移除元素时不会打乱索引
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            // 如果列表中的某个GameObject变成了null（意味着它被Destroy了）
            if (activeEnemies[i] == null)
            {
                // 从列表中移除这个空引用
                activeEnemies.RemoveAt(i);

                // 广播“敌人死亡”事件！
                OnEnemyDied?.Invoke();

            }
        }
    }

    /// <summary>
    /// 公共方法：当一个新敌人出生时，调用此方法将它注册到管理器中。
    /// </summary>
    /// <param name="enemy">新出生的敌人GameObject</param>
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
        }
    }
}