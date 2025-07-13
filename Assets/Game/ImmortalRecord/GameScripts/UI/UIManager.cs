// 文件名: UIManager.cs (最终版)
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

    // --- 这是新的部分！ ---
    [Header("装备数据库 (拖入所有装备资源文件)")]
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
            Debug.LogError("UIManager 的某些UI引用没有在Inspector中设置！请检查！");
            return;
        }

        selectionPanel.Initialize(this);
        comparisonPanel.Initialize(this);

        // --- 修改的部分 ---
        // 1. 删除对旧的 CreateTestData() 的调用
        // CreateTestData(); 

        // 2. 调用新的加载函数
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

    // --- 这是新的加载函数 ---
    void LoadInventoryFromDatabase()
    {
        playerInventory.Clear(); // 清空旧的列表

        if (equipmentDatabase == null || equipmentDatabase.Count == 0)
        {
            Debug.LogWarning("装备数据库为空，请在UIManager的Inspector中配置！");
            return;
        }

        // 遍历所有装备资源，把它们转换成运行时的EquipmentData，并加入玩家“背包”
        foreach (var itemAsset in equipmentDatabase)
        {
            playerInventory.Add(itemAsset.ToEquipmentData());
        }
        Debug.Log($"从数据库加载了 {playerInventory.Count} 件装备到玩家背包。");
    }
    // ------------------------

    // --- 旧的 CreateTestData() 函数可以完全删除了 ---
    /*
    void CreateTestData() 
    { 
        // ...所有旧的 playerInventory.Add(...) 代码...
    }
    */
    // --- 其他所有函数 (OnSlotClicked, OnSelectItemClicked, EquipItem) 保持不变 ---

    public void OnSlotClicked(EquipmentSlotUI slot)
    {
        Debug.Log("点击了槽位: " + slot.slotType);
        selectionPanel.ShowPanel(slot.slotType, playerInventory);
    }

    public void OnSelectItemClicked(EquipmentData selectedData)
    {
        Debug.Log("选择了物品: " + selectedData.itemName);
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
        Debug.Log("已成功装备: " + itemToEquip.itemName);
    }
}