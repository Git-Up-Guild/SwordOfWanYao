// �ļ���: UnlockManager.cs
// ���ض���: �����е�һ����GameObject����Ϊ "UnlockManager"
using UnityEngine;
using System.Collections.Generic;

public class UnlockManager : MonoBehaviour
{
    // ʹ�õ���ģʽ������ȫ�ַ���
    public static UnlockManager Instance { get; private set; }

    // ʹ��һ��HashSet���洢�Ѿ�ѡ����ġ�����������ID����ѯЧ�ʸ�
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
    /// ��¼һ�š��������֡��Ŀ����Ѿ���ѡ�����
    /// </summary>
    /// <param name="card">��ѡ��Ľ�����</param>
    public void RecordUnlockCardSelected(CardConfig card)
    {
        // ȷ����Ƭ��Ϊ�գ�������ȷʵ�ǽ�����������֮ǰû�м�¼��
        if (card != null && card.Type == CardType.UnlockUnit && !unlockedCardIDs.Contains(card.ID))
        {
            unlockedCardIDs.Add(card.ID);
            Debug.Log($"[UnlockManager] �������ѱ���¼: {card.Name} (ID: {card.ID})");
        }
    }

    /// <summary>
    /// ���һ�ż��ܿ�������ǰ�ý������Ƿ��ѱ�ѡ��
    /// </summary>
    /// <param name="card">Ҫ���ļ��ܿ�</param>
    /// <returns>�������ǰ�����������㣬�򷵻�true</returns>
    public bool AreRequirementsMetForCard(CardConfig card)
    {
        // --- ������־1 ---
        Debug.Log($"--- ��ʼ��鿨�� '{card.Name}' �Ľ������� ---");

        if (card.RequiredUnlockCards == null || card.RequiredUnlockCards.Count == 0)
        {
            // --- ������־2 ---
            Debug.Log($"���� '{card.Name}' û�н���Ҫ��ֱ��ͨ����");
            return true;
        }

        Debug.Log($"���� '{card.Name}' ��Ҫ {card.RequiredUnlockCards.Count} ��ǰ��������");

        foreach (var requiredCard in card.RequiredUnlockCards)
        {
            // --- ������־3 ---
            Debug.Log($" -> ���ڼ��ǰ������: '{requiredCard.Name}' (ID: {requiredCard.ID})");

            if (!unlockedCardIDs.Contains(requiredCard.ID))
            {
                // --- ������־4 ---
                Debug.LogWarning($"!!! ����������: ǰ�ÿ� '{requiredCard.Name}' ��δ���������ʧ�ܣ�");
                return false;
            }
            else
            {
                // --- ������־5 ---
                Debug.Log($"    -> ��������: ǰ�ÿ� '{requiredCard.Name}' �ѽ�����");
            }
        }

        // --- ������־6 ---
        Debug.Log($"���� '{card.Name}' ������ǰ�������������㣬���ͨ����");
        return true;
    }
}