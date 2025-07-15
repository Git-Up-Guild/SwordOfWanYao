// �ļ���: GameManager.cs
using UnityEngine;

public class GameManagers : MonoBehaviour
{
    // ʹ�õ���ģʽ���������ű�����ͨ�� GameManager.Instance ������
    public static GameManagers Instance { get; private set; }

    // ��Inspector�У������Ǵ�����BuffManager��Դ�ļ��ϵ�����
    [Header("ȫ�ֹ���������")]
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
            // --- ������ִ��������Ҫ���õĲ��� ---
            InitializeManagers();
        }
    }
    private void InitializeManagers()
    {
        // ���BuffManager�Ƿ���ڣ�Ȼ������������÷���
        if (buffManager != null)
        {
            buffManager.ResetBuffs();
        }

        // �Ժ��������������Ҫ���õĹ�������Ҳ���������
        // if (unlockManager != null)
        // {
        //     unlockManager.ResetUnlocks();
        // }
    }
}