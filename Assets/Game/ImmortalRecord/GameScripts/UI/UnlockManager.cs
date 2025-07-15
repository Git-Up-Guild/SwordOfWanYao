// 文件名: UnlockManager.cs
// 挂载对象: 场景中的一个空GameObject，名为 "UnlockManager"
using UnityEngine;
using System.Collections.Generic;

public class UnlockManager : MonoBehaviour
{
    // 使用单例模式，方便全局访问
    public static UnlockManager Instance { get; private set; }

    // 使用一个HashSet来存储已经选择过的“解锁卡”的ID，查询效率高
    private HashSet<int> unlockedCardIDs = new HashSet<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 记录一张“解锁兵种”的卡牌已经被选择过了
    /// </summary>
    /// <param name="card">被选择的解锁卡</param>
    public void RecordUnlockCardSelected(CardConfig card)
    {
        // 确保卡片不为空，且类型确实是解锁卡，并且之前没有记录过
        if (card != null && card.Type == CardType.UnlockUnit && !unlockedCardIDs.Contains(card.ID))
        {
            unlockedCardIDs.Add(card.ID);
            Debug.Log($"[UnlockManager] 解锁卡已被记录: {card.Name} (ID: {card.ID})");
        }
    }

    /// <summary>
    /// 检查一张技能卡的所有前置解锁卡是否都已被选择
    /// </summary>
    /// <param name="card">要检查的技能卡</param>
    /// <returns>如果所有前置条件都满足，则返回true</returns>
    public bool AreRequirementsMetForCard(CardConfig card)
    {
        // --- 调试日志1 ---
        Debug.Log($"--- 开始检查卡牌 '{card.Name}' 的解锁条件 ---");

        if (card.RequiredUnlockCards == null || card.RequiredUnlockCards.Count == 0)
        {
            // --- 调试日志2 ---
            Debug.Log($"卡牌 '{card.Name}' 没有解锁要求，直接通过。");
            return true;
        }

        Debug.Log($"卡牌 '{card.Name}' 需要 {card.RequiredUnlockCards.Count} 个前置条件。");

        foreach (var requiredCard in card.RequiredUnlockCards)
        {
            // --- 调试日志3 ---
            Debug.Log($" -> 正在检查前置条件: '{requiredCard.Name}' (ID: {requiredCard.ID})");

            if (!unlockedCardIDs.Contains(requiredCard.ID))
            {
                // --- 调试日志4 ---
                Debug.LogWarning($"!!! 条件不满足: 前置卡 '{requiredCard.Name}' 尚未解锁。检查失败！");
                return false;
            }
            else
            {
                // --- 调试日志5 ---
                Debug.Log($"    -> 条件满足: 前置卡 '{requiredCard.Name}' 已解锁。");
            }
        }

        // --- 调试日志6 ---
        Debug.Log($"卡牌 '{card.Name}' 的所有前置条件均已满足，检查通过！");
        return true;
    }
}