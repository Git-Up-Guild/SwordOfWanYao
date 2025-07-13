using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EquipmentSelectionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform contentParent; // Scroll View��Content
    [SerializeField] private GameObject selectItemPrefab; // �б����Ԥ�Ƽ�

    private UIManager uiManager;

    // ��UIManager����Ϸ��ʼʱ���ã����г�ʼ��
    public void Initialize(UIManager manager)
    {
        uiManager = manager;
    }

    // ��ʾ��壬�����������������
    public void ShowPanel(EquipmentType type, List<EquipmentData> playerInventory)
    {
        if (uiManager == null || selectItemPrefab == null)
        {
            Debug.LogError("EquipmentSelectionPanel δ��ȷ��ʼ����δ����Ԥ�Ƽ�!");
            return;
        }

        gameObject.SetActive(true);
        titleText.text = "ѡ�� " + GetTypeName(type); // ��ö����תΪ����

        // 1. ������һ�����ɵ��б����ֹ�ظ�
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. ������ұ�����ɸѡ����Ӧ���͵�װ����ʵ����
        foreach (EquipmentData equipment in playerInventory)
        {
            if (equipment.type == type)
            {
                // ʵ�����б���Ԥ�Ƽ��������丸��������ΪContent
                GameObject itemGO = Instantiate(selectItemPrefab, contentParent);

                // ��ȡԤ�Ƽ��ϵĽű�������װ��������������
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

    // (��ѡ) һ��С�����������ñ�����ÿ�
    private string GetTypeName(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Head: return "ͷ��װ��";
            case EquipmentType.Armor: return "����װ��";
            case EquipmentType.Weapon: return "����";
            case EquipmentType.Boots: return "�㲿װ��";
            case EquipmentType.Gloves: return "�ֲ�װ��";
            case EquipmentType.Accessory: return "��Ʒ";
            default: return "δ֪����";
        }
    }
}