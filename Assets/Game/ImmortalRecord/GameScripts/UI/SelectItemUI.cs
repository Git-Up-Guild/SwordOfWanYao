using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I18N.Common;

public class SelectItemUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI levelText;
    public Button selectButton;

    private EquipmentData equipmentData;

    // 初始化这个列表项
    public void Setup(EquipmentData data, UIManager uiManager)
    {
        equipmentData = data;
        itemIcon.sprite = data.icon;
        levelText.text = "Lv." + data.level;

        // 为按钮添加点击事件，点击时通知UIManager
        selectButton.onClick.RemoveAllListeners(); // 先移除旧的监听，防止重复
        selectButton.onClick.AddListener(() => {
            uiManager.OnSelectItemClicked(equipmentData);
        });
    }
}