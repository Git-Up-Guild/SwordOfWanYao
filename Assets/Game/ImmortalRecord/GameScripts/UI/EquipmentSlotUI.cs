using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ����ű�����ֻ�������Լ����߼����ǳ��ɾ���
public class EquipmentSlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI levelText;
    public Sprite defaultIcon;

    public EquipmentType slotType; // �����ڿ���ֱ��ʹ���ڱ𴦶���� EquipmentType

    private EquipmentData currentEquipment;

    void Start()
    {
        ClearSlot();
    }

    public void EquipItem(EquipmentData newEquipment)
    {
        currentEquipment = newEquipment;
        itemIcon.sprite = newEquipment.icon;
        levelText.text = "Lv." + newEquipment.level;
        itemIcon.enabled = true;
        levelText.enabled = true;
    }

    public void UnequipItem()
    {
        currentEquipment = null;
        ClearSlot();
    }

    private void ClearSlot()
    {
        itemIcon.sprite = defaultIcon;
        itemIcon.enabled = true; // ȷ��Ĭ��ͼ���ǿɼ���
        levelText.enabled = false;
    }
}