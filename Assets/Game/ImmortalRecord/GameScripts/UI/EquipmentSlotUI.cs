using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 这个脚本现在只包含它自己的逻辑，非常干净！
public class EquipmentSlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI levelText;
    public Sprite defaultIcon;

    public EquipmentType slotType; // 它现在可以直接使用在别处定义的 EquipmentType

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
        itemIcon.enabled = true; // 确保默认图标是可见的
        levelText.enabled = false;
    }
}