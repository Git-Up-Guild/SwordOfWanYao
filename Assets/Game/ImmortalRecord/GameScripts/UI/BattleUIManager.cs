// �ļ���: BattleUIManager.cs (����������)
// ���ض���: �����е� BattleUIManager GameObject

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    #region �ֶζ��� (Fields & Properties)

    // --- ģ��һ����Ϸ������� ---
    [Header("ģ��һ����Ϸ�������")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [SerializeField] private Button gameSpeedButton;
    [SerializeField] private TextMeshProUGUI speedButtonText;

    private float timer = 0f;

    // --- ģ������ؿ���Ϣ�뾭���� ---
    [Header("ģ������ؿ���Ϣ���")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TextMeshProUGUI levelText; // �������������ʾ�ȼ�
    [SerializeField] private TextMeshProUGUI killCountText; // ������ר����ʾ��ɱ�����ı�

    private int killCount = 0; // �����ۼƻ�ɱ�ı���

    // --- ģ�������Զ�������� ---
    [Header("ģ�������Զ��������")]
    [SerializeField] private List<SpawnCardUI> spawnSlots;

    // ����������������洢��ҵ�ǰ�ĳ�ս����
    private List<UnitData> playerDeck = new List<UnitData>(5);

    // Ϊ�˲��ԣ�����ֱ�������ﶨ���������
    [System.Serializable]
    public class SpawnableUnitInfo
    {
        public string unitName;
        public Sprite icon;
        public float spawnInterval;
    }
    [SerializeField] private List<SpawnableUnitInfo> unitSpawnList;

    #endregion


    #region Unity�������ں��� (Unity Lifecycle)

    // ���ű�ʵ��������ʱ����
    private void Awake()
    {
        // ������Է�һЩ����ģʽ�ĳ�ʼ�����룬�����Ҫ�Ļ�
    }

    // ������ű����������ʱ���� (��Start֮ǰ)
    private void OnEnable()
    {
        // ��ʼ���������ģ���EnemyManager�㲥�ĵ��������¼�
        EnemyManager.OnEnemyDied += HandleEnemyDied;
    }

    // ������ű���������û�����ʱ����
    private void OnDisable()
    {
        // ֹͣ������ȡ�����ģ����˵������¼�����ֹ�ڴ�й©
        EnemyManager.OnEnemyDied -= HandleEnemyDied;
    }

    // ��Ϸ��ʼ��һ֡ǰ����
    void Start()
    {
        // --- ��ʼ��ģ��һ ---
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseButtonClicked);
        if (gameSpeedButton != null) gameSpeedButton.onClick.AddListener(OnGameSpeedButtonClicked);
        Time.timeScale = 1.0f;
        if (speedButtonText != null) speedButtonText.text = "x1";

        // --- ��ʼ��ģ��� (ʹ�ò�������) ---
        SetLevelName("3-���´���");
        UpdateLevel(1);
        UpdateExperience(0.3f);
        UpdateKillCountUI(); // ��ʼ����ɱ����ʾΪ0

        // --- ��ʼ��ģ���� ---
        InitializeSpawnSlots();
        // ��ʼ��ʱ�����Ǽ����ս�����ǿյ�
        InitializeSpawnPanel();

        // -------------------------------------------------------------------
        // --- ������������߼�����Ϸ��ʼʱǿ�ƽ���һ�γ鿨 ---
        // -------------------------------------------------------------------

        // �ӳ�һС��ʱ���ٵ��ã�ȷ�����ж�������ʼ����ϣ�������ֵĳ�ͻ��
        // 0.1����ӳ����ۻ����������������Գ����ȶ��Ժ��кô���
        Invoke(nameof(TriggerInitialCardDraw), 0.1f);
    }

    // --- �ڽű����������µķ��� ---
    private void TriggerInitialCardDraw()
    {
        // ���CardSelectionManager�Ƿ����
        if (CardSelectionManager.Instance != null)
        {
            Debug.Log("��Ϸ��ʼ�����г�ʼ�鿨��");

            // ֱ�ӵ��� CardSelectionManager �� ShowCards ����
            // ��Ϊ���ṩһ��ȷ��ѡ���Ļص�����
            CardSelectionManager.Instance.ShowCards(selectedCard => {
                // ���������γ�ʼ�鿨��ȷ��ѡ�������Ĵ���ᱻִ��
                Debug.Log($"��ʼ����ѡ�����: {selectedCard.Name}");

                // ������Ҫ�������ֶ�Ӧ��Ч������Ϊ��β���ͨ��Trigger������
                // ������Ҫ����һ�� ApplyCardEffect ���߼�
                if (GameManagers.Instance != null && GameManagers.Instance.buffManager != null)
                {
                    GameManagers.Instance.buffManager.AddBuff(selectedCard.Effect);

                    // �տ�ʼ��Ϸ������û�е�λ�����Բ���Ҫˢ�³��ϵ�λ
                }
            });
        }
        else
        {
            Debug.LogError("CardSelectionManager δ�ҵ����޷����г�ʼ�鿨��");
        }
    }

    private void InitializeSpawnPanel()
    {
        for (int i = 0; i < spawnSlots.Count; i++)
        {
            // ����ս���������λ���Ƿ��е�λ
            if (i < playerDeck.Count && playerDeck[i] != null)
            {
                // ����У��ͳ�ʼ����Ƭ
                UnitData unitData = playerDeck[i];
                spawnSlots[i].Initialize(unitData.icon, unitData.spawnInterval);

                int unitIndex = i;
                spawnSlots[i].OnCooldownComplete += () => { SpawnUnit(unitIndex); };

                spawnSlots[i].UpdatePopulation(0); // ��ʼ�˿�Ϊ0
                spawnSlots[i].gameObject.SetActive(true); // ��ʾ��Ƭ
            }
            else
            {
                // ���û�У�������������ۣ���ʾ�ǿյ�
                spawnSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void AddUnitToDeck(UnitData newUnitData, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spawnSlots.Count)
        {
            Debug.LogError("��Ч�Ĳ�λ������");
            return;
        }

        // ȷ���б��㹻��
        while (playerDeck.Count <= slotIndex)
        {
            playerDeck.Add(null);
        }

        // ��������
        playerDeck[slotIndex] = newUnitData;

        // ����ˢ��UI
        RefreshSingleSpawnSlot(slotIndex);
    }

    // ˢ�µ�����λ����ʾ
    private void RefreshSingleSpawnSlot(int slotIndex)
    {
        UnitData unitData = playerDeck[slotIndex];
        SpawnCardUI cardUI = spawnSlots[slotIndex];

        if (unitData != null)
        {
            // ��ʼ����Ƭ����ʾ
            cardUI.Initialize(unitData.icon, unitData.spawnInterval);

            // ���°��¼� (���Ƴ��ɵģ�������µ�)
            cardUI.OnCooldownComplete = null;
            cardUI.OnCooldownComplete += () => { SpawnUnit(slotIndex); };

            cardUI.UpdatePopulation(0);
            cardUI.gameObject.SetActive(true);
        }
        else
        {
            // �������Ϊ�գ������ؿ�Ƭ
            cardUI.gameObject.SetActive(false);
        }
    }
    // ÿһ֡����
    void Update()
    {
        // --- ����ģ��һ�ļ�ʱ�� ---
        if (gameTimerText != null)
        {
            timer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer % 60F);
            gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    #endregion


    #region �¼����� (Event Handlers)

    // --- ������������������¼��ķ��� ---
    private void HandleEnemyDied()
    {
        killCount++;
        UpdateKillCountUI();
    }

    #endregion


    #region UI�����뽻������ (UI Update & Interaction)

    // --- ģ��һ�ķ��� ---
    private void OnPauseButtonClicked()
    {
        Debug.Log("��Ϸ��ͣ��ť�����");
        // ...
    }

    private void OnGameSpeedButtonClicked()
    {
        if (Time.timeScale < 2.0f)
        {
            Time.timeScale = 2.0f;
            if (speedButtonText != null) speedButtonText.text = "x2";
        }
        else
        {
            Time.timeScale = 1.0f;
            if (speedButtonText != null) speedButtonText.text = "x1";
        }
    }

    // --- ģ����ķ��� ---
    public void SetLevelName(string name)
    {
        if (levelNameText != null) levelNameText.text = name;
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = level + "��";
    }

    public void UpdateExperience(float fillRatio)
    {
        if (xpFillImage != null) xpFillImage.fillAmount = Mathf.Clamp01(fillRatio);
    }

    // ������ר�Ÿ��»�ɱ��UI�ķ���
    private void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "��ɱ: " + killCount;
        }
    }

    // --- ģ�����ķ��� ---
    private void InitializeSpawnSlots()
    {
        if (spawnSlots.Count != unitSpawnList.Count)
        {
            Debug.LogError("UI��λ�������������������ƥ�䣡����BattleUIManager��Inspector�м�顣");
            return;
        }

        for (int i = 0; i < spawnSlots.Count; i++)
        {
            spawnSlots[i].Initialize(unitSpawnList[i].icon, unitSpawnList[i].spawnInterval);
            int unitIndex = i;
            spawnSlots[i].OnCooldownComplete += () => { SpawnUnit(unitIndex); };
            spawnSlots[i].UpdatePopulation(0);
        }
    }

    private void SpawnUnit(int unitIndex)
    {
        // ȷ��������Ч
        if (unitIndex < playerDeck.Count && playerDeck[unitIndex] != null)
        {
            UnitData unitToSpawn = playerDeck[unitIndex];
            Debug.Log($"ʱ�䵽����������һ�� '{unitToSpawn.unitName}'!");
            // GameManager.Instance.SpawnUnit(unitToSpawn.unitPrefab);
        }
    }

    public void UpdateUnitPopulationDisplay(int unitIndex, int currentCount)
    {
        if (unitIndex >= 0 && unitIndex < spawnSlots.Count)
        {
            spawnSlots[unitIndex].UpdatePopulation(currentCount);
        }
    }

    #endregion
}