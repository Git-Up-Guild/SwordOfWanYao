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
            // DontDestroyOnLoad(gameObject); // �����Ҫ�ڲ�ͬ�����䱣�֣�����ȡ������ע��
        }
    }
}