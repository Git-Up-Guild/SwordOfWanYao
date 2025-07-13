// �ļ���: UIManager.cs (���հ�)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private EquipmentSelectionPanel selectionPanel;
    [SerializeField] private ComparisonPanel comparisonPanel;

    [Header("Main UI Slots")]
    [SerializeField] private List<EquipmentSlotUI> equipmentSlots;

    // --- �����µĲ��֣� ---
    [Header("װ�����ݿ� (��������װ����Դ�ļ�)")]
    [SerializeField] private List<EquipmentItem> equipmentDatabase;
    // --------------------

    private Dictionary<EquipmentType, EquipmentData> equippedItems = new Dictionary<EquipmentType, EquipmentData>();
    private List<EquipmentData> playerInventory = new List<EquipmentData>();

    void Awake()
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

    void Start()
    {
        if (selectionPanel == null || comparisonPanel == null || equipmentSlots.Count == 0)
        {
            Debug.LogError("UIManager ��ĳЩUI����û����Inspector�����ã����飡");
            return;
        }

        selectionPanel.Initialize(this);
        comparisonPanel.Initialize(this);

        // --- �޸ĵĲ��� ---
        // 1. ɾ���Ծɵ� CreateTestData() �ĵ���
        // CreateTestData(); 

        // 2. �����µļ��غ���
        LoadInventoryFromDatabase();
        // --------------------

        foreach (var slot in equipmentSlots)
        {
            Button slotButton = slot.GetComponent<Button>();
            if (slotButton != null)
            {
                EquipmentSlotUI currentSlot = slot;
                slotButton.onClick.AddListener(() => OnSlotClicked(currentSlot));
            }
        }

        selectionPanel.ClosePanel();
        comparisonPanel.ClosePanel();
    }

    // --- �����µļ��غ��� ---
    void LoadInventoryFromDatabase()
    {
        playerInventory.Clear(); // ��վɵ��б�

        if (equipmentDatabase == null || equipmentDatabase.Count == 0)
        {
            Debug.LogWarning("װ�����ݿ�Ϊ�գ�����UIManager��Inspector�����ã�");
            return;
        }

        // ��������װ����Դ��������ת��������ʱ��EquipmentData����������ҡ�������
        foreach (var itemAsset in equipmentDatabase)
        {
            playerInventory.Add(itemAsset.ToEquipmentData());
        }
        Debug.Log($"�����ݿ������ {playerInventory.Count} ��װ������ұ�����");
    }
    // ------------------------

    // --- �ɵ� CreateTestData() ����������ȫɾ���� ---
    /*
    void CreateTestData() 
    { 
        // ...���оɵ� playerInventory.Add(...) ����...
    }
    */
    // --- �������к��� (OnSlotClicked, OnSelectItemClicked, EquipItem) ���ֲ��� ---

    public void OnSlotClicked(EquipmentSlotUI slot)
    {
        Debug.Log("����˲�λ: " + slot.slotType);
        selectionPanel.ShowPanel(slot.slotType, playerInventory);
    }

    public void OnSelectItemClicked(EquipmentData selectedData)
    {
        Debug.Log("ѡ������Ʒ: " + selectedData.itemName);
        equippedItems.TryGetValue(selectedData.type, out EquipmentData currentEquipped);
        comparisonPanel.ShowPanel(currentEquipped, selectedData);
        selectionPanel.ClosePanel();
    }

    public void EquipItem(EquipmentData itemToEquip)
    {
        equippedItems[itemToEquip.type] = itemToEquip;
        EquipmentSlotUI slotUI = equipmentSlots.FirstOrDefault(s => s.slotType == itemToEquip.type);
        if (slotUI != null)
        {
            slotUI.EquipItem(itemToEquip);
        }
        Debug.Log("�ѳɹ�װ��: " + itemToEquip.itemName);
    }
}