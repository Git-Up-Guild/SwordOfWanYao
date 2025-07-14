using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EquipmentSelectionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform contentParent; // Scroll View的Content
    [SerializeField] private GameObject selectItemPrefab; // 列表项的预制件

    private UIManager uiManager;

    // 由UIManager在游戏开始时调用，进行初始化
    public void Initialize(UIManager manager)
    {
        uiManager = manager;
    }

    // 显示面板，并根据类型填充内容
    public void ShowPanel(EquipmentType type, List<EquipmentData> playerInventory)
    {
        if (uiManager == null || selectItemPrefab == null)
        {
            Debug.LogError("EquipmentSelectionPanel 未正确初始化或未设置预制件!");
            return;
        }

        gameObject.SetActive(true);
        titleText.text = "选择 " + GetTypeName(type); // 将枚举名转为中文

        // 1. 清理上一次生成的列表项，防止重复
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 遍历玩家背包，筛选出对应类型的装备并实例化
        foreach (EquipmentData equipment in playerInventory)
        {
            if (equipment.type == type)
            {
                // 实例化列表项预制件，并将其父物体设置为Content
                GameObject itemGO = Instantiate(selectItemPrefab, contentParent);

                // 获取预制件上的脚本，并用装备数据来设置它
                SelectItemUI itemUI = itemGO.GetComponent<SelectItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(equipment, uiManager);
                }
            }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // (可选) 一个小辅助函数，让标题更好看
    private string GetTypeName(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Head: return "头部装备";
            case EquipmentType.Armor: return "身体装备";
            case EquipmentType.Weapon: return "武器";
            case EquipmentType.Boots: return "足部装备";
            case EquipmentType.Gloves: return "手部装备";
            case EquipmentType.Accessory: return "饰品";
            default: return "未知类型";
        }
    }
}