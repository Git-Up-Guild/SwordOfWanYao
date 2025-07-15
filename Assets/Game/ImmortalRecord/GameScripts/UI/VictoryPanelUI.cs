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


   
}