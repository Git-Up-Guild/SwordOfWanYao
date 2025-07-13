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

    // ��ʼ������б���
    public void Setup(EquipmentData data, UIManager uiManager)
    {
        equipmentData = data;
        itemIcon.sprite = data.icon;
        levelText.text = "Lv." + data.level;

        // Ϊ��ť��ӵ���¼������ʱ֪ͨUIManager
        selectButton.onClick.RemoveAllListeners(); // ���Ƴ��ɵļ�������ֹ�ظ�
        selectButton.onClick.AddListener(() => {
            uiManager.OnSelectItemClicked(equipmentData);
        });
    }
}