// �ļ���: LevelSwitchManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // �����ʹ��TextMeshPro���뱣������
using System.Collections.Generic;

public class LevelSwitchManager : MonoBehaviour
{
    [Header("1. �ؿ����ݿ� (��˳���������йؿ������ļ�)")]
    [SerializeField] private List<LevelData> levelDatabase;

    [Header("2. UI�������")]
    [SerializeField] private RectTransform imageSlider;         // ���йؿ�ͼƬ���ڵĻ�������
    [SerializeField] private TextMeshProUGUI levelNumberText;   // ��ʾ���� X �ء����ı�
    [SerializeField] private TextMeshProUGUI levelNameText;     // ��ʾ�ؿ����Ƶ��ı�
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button battleButton;

    [Header("3. ��������")]
    [SerializeField] private float slideSpeed = 10f; // ͼƬ�������ٶ�

    private int currentLevelIndex = 0;
    private float levelWidth;
    private Vector2 targetSliderPosition;

    void Start()
    {
        // ��ȫ���
        if (levelDatabase == null || levelDatabase.Count == 0)
        {
            Debug.LogError("�ؿ����ݿ�δ���û�Ϊ�գ�����Inspector������ؿ����ݡ�");
            // �������а�ť����ֹ����
            leftArrowButton.interactable = false;
            rightArrowButton.interactable = false;
            battleButton.interactable = false;
            return;
        }

        // ��ȡ�����ؿ��Ŀ�� (���Ǽ������йؿ�ͼƬһ����)
        levelWidth = imageSlider.GetComponent<RectTransform>().rect.width;

        // �󶨰�ť�¼�
        leftArrowButton.onClick.AddListener(GoToPreviousLevel);
        rightArrowButton.onClick.AddListener(GoToNextLevel);
        battleButton.onClick.AddListener(StartBattle);

        // ��ʼ����ʾ��һ���ؿ�
        UpdateUI(true); // ����true����ʾ�ǳ�ʼ����˲�䶨λ
    }

    void Update()
    {
        // ʹ��Vector2.Lerp�û�����ƽ�����ƶ���Ŀ��λ��
        imageSlider.anchoredPosition = Vector2.Lerp(imageSlider.anchoredPosition, targetSliderPosition, Time.deltaTime * slideSpeed);
    }

    private void GoToNextLevel()
    {
        if (currentLevelIndex < levelDatabase.Count - 1)
        {
            currentLevelIndex++;
            UpdateUI();
        }
    }

    private void GoToPreviousLevel()
    {
        if (currentLevelIndex > 0)
        {
            currentLevelIndex--;
            UpdateUI();
        }
    }

    // ��������UIԪ��
    // isInstant�������������ǳ�ʼ�����Ǻ����л�
    private void UpdateUI(bool isInstant = false)
    {
        LevelData currentLevel = levelDatabase[currentLevelIndex];

        // 1. �����ı���Ϣ
        levelNumberText.text = "�� " + currentLevel.levelNumber + " ��";
        levelNameText.text = currentLevel.levelName;

        // 2. ���㲢���û�������Ŀ��λ��
        targetSliderPosition = new Vector2(-currentLevelIndex * levelWidth, imageSlider.anchoredPosition.y);

        // ����ǳ�ʼ������˲���ƶ���Ŀ��λ��
        if (isInstant)
        {
            imageSlider.anchoredPosition = targetSliderPosition;
        }

        // 3. ���¼�ͷ��ť�Ŀɽ���״̬
        leftArrowButton.interactable = (currentLevelIndex > 0);
        rightArrowButton.interactable = (currentLevelIndex < levelDatabase.Count - 1);
    }

    public void StartBattle()
    {
        LevelData selectedLevel = levelDatabase[currentLevelIndex];

        // ��鳡�����Ƿ�Ϊ��
        if (string.IsNullOrEmpty(selectedLevel.sceneNameToLoad))
        {
            Debug.LogError($"�ؿ� '{selectedLevel.levelName}' û������Ҫ���صĳ�������");
            return;
        }

        Debug.Log($"׼������ؿ�: {selectedLevel.levelName}, ���س���: {selectedLevel.sceneNameToLoad}");

        // ����ս������
        SceneManager.LoadScene(selectedLevel.sceneNameToLoad);
    }
}