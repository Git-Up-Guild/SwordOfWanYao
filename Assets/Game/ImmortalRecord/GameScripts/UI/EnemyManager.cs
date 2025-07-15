// �ļ���: EnemyManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    // ʹ�õ���ģʽ�����������ű���������˳����㣩����
    public static EnemyManager Instance { get; private set; }

    // --- �������������¼�������������������㲥 ---
    public static event Action OnEnemyDied;

    // һ���б�����׷�������ڳ��ϵĵ���
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        // ���õ���
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // ��ÿһ֡�ĺ���ִ�У�����ȷ���ڵ���ִ�����Լ����߼����ټ��
    void LateUpdate()
    {
        // �Ӻ���ǰ�����б��������Ƴ�Ԫ��ʱ�����������
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            // ����б��е�ĳ��GameObject�����null����ζ������Destroy�ˣ�
            if (activeEnemies[i] == null)
            {
                // ���б����Ƴ����������
                activeEnemies.RemoveAt(i);

                // �㲥�������������¼���
                OnEnemyDied?.Invoke();

            }
        }
    }

    /// <summary>
    /// ������������һ���µ��˳���ʱ�����ô˷�������ע�ᵽ�������С�
    /// </summary>
    /// <param name="enemy">�³����ĵ���GameObject</param>
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
        }
    }
}