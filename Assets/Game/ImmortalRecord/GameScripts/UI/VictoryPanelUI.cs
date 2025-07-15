// �ļ���: VictoryPanelUI.cs
// ���ض���: ʤ��������������� VictoryPanel

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // ���ʹ��TextMeshPro
using UnityEngine.SceneManagement; // <-- �ؼ��������һ��
public class VictoryPanelUI : MonoBehaviour
{
    // --- ����ģʽ ---
    public static VictoryPanelUI Instance { get; private set; }

    // --- UI������� ---
    [Header("������뱳��")]
    [SerializeField] private GameObject dimmerBackground;
    [Header("��������")]
    [SerializeField] private Transform rewardsGrid;
    [SerializeField] private GameObject rewardItemPrefab;
    [Header("������ť")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button doubleRewardButton;

    // --- �������������� ---
    [Header("�ؿ�����Դ")]
    [Tooltip("�����ؿ���Ӧ��LevelData��Դ�ļ��ϵ�����")]
    [SerializeField] private LevelData levelData;

    [SerializeField] private GameObject gameObjects;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }

        // ��Ϸ��ʼʱ��ȷ����������ص�
        gameObject.SetActive(false);
        if (dimmerBackground != null) dimmerBackground.SetActive(false);
    }

    private void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        if (doubleRewardButton != null) doubleRewardButton.onClick.AddListener(OnDoubleRewardButtonClicked);
        gameObjects.SetActive(false);


    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (doubleRewardButton != null) doubleRewardButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// ��ʾʤ����塣�����Զ������õ�levelData�л�ȡ������Ϣ��
    /// </summary>
    public void Show() // <-- �޸ĵ�1��������Ҫ���� VictoryData ����
    {
        // ��ȫ��飺ȷ��levelData�Ѿ�������
        if (levelData == null)
        {
            Debug.LogError("VictoryPanelUI�ϵ�LevelDataδ���ã��޷���ʾ������");
            return;
        }

        // �������ͱ���
        if (dimmerBackground != null) dimmerBackground.SetActive(true);
        gameObjects.SetActive(true);

        // --- ��佱��UI ---
        // 1. ����ɵĽ���ͼ�� (��ֹ����ʱUI�ظ�)
        foreach (Transform child in rewardsGrid)
        {
            Destroy(child.gameObject);
        }

        // 2. �������Լ����е� levelData �л�ȡ�����б�
        if (levelData.victoryRewards != null)
        {
            // 3. �������ݣ��������������µĽ���ͼ��
            foreach (var rewardData in levelData.victoryRewards)
            {
                // ʵ����Ԥ�Ƽ�����ָ��������ΪrewardsGrid
                GameObject itemGO = Instantiate(rewardItemPrefab, rewardsGrid);

                // ��ȡԤ�Ƽ��ϵĽű�
                RewardItemUI itemUI = itemGO.GetComponent<RewardItemUI>();
                if (itemUI != null)
                {
                    // ����Setup�������ѽ������ݴ��ݹ�ȥ
                    itemUI.Setup(rewardData);
                }
            }
        }
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("���ذ�ť�������");
        // --- ��������ӳ�����ת�ĺ����߼� ---

        // �ָ�ʱ�����٣���ֹ���˵�Ҳ����ͣ
        Time.timeScale = 1f;

        // ������������泡��
        // ��Ҫ���� "MainMenu" �滻���������泡������ʵ�ļ�����
        SceneManager.LoadScene("UI1");
    }

    private void OnDoubleRewardButtonClicked()
    {
        Debug.Log("����������ť�������");
        // ...
    }
}