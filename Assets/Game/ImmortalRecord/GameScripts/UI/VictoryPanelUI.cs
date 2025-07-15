// �ļ���: VictoryPanelUI.cs
// ���ض���: ʤ��������������� VictoryPanel

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // ���ʹ��TextMeshPro

public class VictoryPanelUI : MonoBehaviour
{
    [Header("������뱳��")]
    [SerializeField] private GameObject dimmerBackground; // ��͸����������

    [Header("��������")]
    [SerializeField] private Transform rewardsGrid; // ������Ʒ����ĸ�����
    [SerializeField] private GameObject rewardItemPrefab; // ����������Ʒ��Ԥ�Ƽ�

    [Header("ս��ͳ������")]
    [SerializeField] private Transform statsContent; // ͳ���б�Scroll View��Content
    [SerializeField] private GameObject statRowPrefab; // ����ͳ�����ݵ�Ԥ�Ƽ�

    [Header("������ť")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button doubleRewardButton;

    // --- Ϊ����ʾ��������Start�е��ò��� ---
    // ʵ����Ϸ�У����Show����Ӧ�������GameManager��ս��ʤ��ʱ����
    void Start()
    {
        // ��Ϸ��ʼʱ�������Լ�
        gameObject.SetActive(false);
        if (dimmerBackground != null) dimmerBackground.SetActive(false);

        // --- ���Դ��� ---
        // ����һЩ�ٵĽ�������������UI��ʾ
        CreateAndShowTestData();
    }

    private void OnEnable()
    {
        // Ϊ��ť���¼�
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        if (doubleRewardButton != null) doubleRewardButton.onClick.AddListener(OnDoubleRewardButtonClicked);
    }

    private void OnDisable()
    {
        // �Ƴ��¼���������ϰ��
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (doubleRewardButton != null) doubleRewardButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// ��ʾʤ����岢���������UI
    /// </summary>
    /// <param name="data">�������н�����Ϣ�����ݰ�</param>
    public void Show(VictoryData data)
    {
        // �������ͱ���
        if (dimmerBackground != null) dimmerBackground.SetActive(true);
        gameObject.SetActive(true);

        // --- ��佱��UI ---
        // 1. ����ɵĽ���ͼ��
        foreach (Transform child in rewardsGrid)
        {
            Destroy(child.gameObject);
        }
        // 2. �������������µĽ���ͼ��
        foreach (var rewardData in data.rewards)
        {
            GameObject itemGO = Instantiate(rewardItemPrefab, rewardsGrid);
            // ��������Ҫһ�� RewardItemUI.cs �ű�����Ԥ�Ƽ��ϣ���������ͼ�������
            // itemGO.GetComponent<RewardItemUI>().Setup(rewardData.itemIcon, rewardData.quantity);
        }

        // --- ���ս��ͳ��UI ---
        // 1. ����ɵ�ͳ����
        foreach (Transform child in statsContent)
        {
            Destroy(child.gameObject);
        }
        // 2. �������������µ�ͳ����
        foreach (var statData in data.unitStats)
        {
            GameObject rowGO = Instantiate(statRowPrefab, statsContent);
            // ��������Ҫһ�� StatRowUI.cs �ű�����Ԥ�Ƽ��ϣ����������������
            // rowGO.GetComponent<StatRowUI>().Setup(statData);
        }
    }

    // --- ��ť����¼����� ---
    private void OnBackButtonClicked()
    {
        Debug.Log("���ذ�ť�������");
        // ��������ӷ������˵���ؿ�ѡ����߼�
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


    private void OnDoubleRewardButtonClicked()
    {
        Debug.Log("����������ť�������");
        // ��������ӹۿ���沢�ַ�˫���������߼�
    }


    // --- �����ڲ��Եĸ������� ---
    private void CreateAndShowTestData()
    {
        VictoryData testData = new VictoryData();

        // �����ٵĽ�������
        testData.rewards = new List<RewardItemData>
        {
            new RewardItemData { itemIcon = null, quantity = 1 },
            new RewardItemData { itemIcon = null, quantity = 150 },
            new RewardItemData { itemIcon = null, quantity = 1 },
            new RewardItemData { itemIcon = null, quantity = 7 },
            new RewardItemData { itemIcon = null, quantity = 20 },
            new RewardItemData { itemIcon = null, quantity = 3 },
            new RewardItemData { itemIcon = null, quantity = 15 },
        };

        // �����ٵ�ս��ͳ������
        testData.unitStats = new List<UnitStatData>
        {
            new UnitStatData { unitName = "��ʦ", unitLevel = 4, damagePercentage = 0.266f, damageValueText = "69.4��", tankPercentage = 0.027f, tankValueText = "12244" },
            new UnitStatData { unitName = "�����", unitLevel = 4, damagePercentage = 0.204f, damageValueText = "53.1��", tankPercentage = 0.353f, tankValueText = "15.7��" },
            new UnitStatData { unitName = "��ǹ��", unitLevel = 5, damagePercentage = 0.203f, damageValueText = "52.9��", tankPercentage = 0.256f, tankValueText = "11.4��" },
            new UnitStatData { unitName = "����ʦ", unitLevel = 4, damagePercentage = 0.165f, damageValueText = "43.1��", tankPercentage = 0.025f, tankValueText = "11335" },
        };

        // �ü���������ʾUI
        Show(testData);
    }
}