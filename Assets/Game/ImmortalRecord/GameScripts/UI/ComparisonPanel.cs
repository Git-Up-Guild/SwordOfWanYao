using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I18N.Common;

public class ComparisonPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private EquipmentDisplayUI leftColumn; // ��ǰװ����
    [SerializeField] private EquipmentDisplayUI rightColumn; // ѡ��װ����
    [SerializeField] private Button equipButton;
    [SerializeField] private Button backButton;

    private EquipmentData newEquipmentToEquip;
    private UIManager uiManager;

    // �����࣬��������һ����UI
    [System.Serializable]
    public class EquipmentDisplayUI
    {
        public GameObject container; // �������ĸ����壬����һ����ʾ/����
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemLevelText;

        // 6�������ı�
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI defenseText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI attackSpeedText;
        public TextMeshProUGUI attackRangeText;
        public TextMeshProUGUI moveSpeedText;

        // ��ʾװ������
        public void Display(EquipmentData data)
        {
            container.SetActive(true);
            // --- ��������ӻ��޸� ---
            itemIcon.enabled = true; // �ؼ���ȷ��Image����Ǽ���ģ�
            itemIcon.color = Color.white; // 2. �ؼ�������ɫ����Ϊ��͸���İ�ɫ (��ͬ�� new Color(1, 1, 1, 1))

            // ----------------------
            itemIcon.sprite = data.icon;
            itemNameText.text = data.itemName;
            itemLevelText.text = "Lv." + data.level;

            // �������ֵΪ0����ʾ"��"��������ʾ��ֵ
            attackText.text = "������: " + (data.attack > 0 ? data.attack.ToString() : "��");
            defenseText.text = "������: " + (data.defense > 0 ? data.defense.ToString() : "��");
            healthText.text = "����ֵ: " + (data.health > 0 ? data.health.ToString() : "��");
            attackSpeedText.text = "����Ƶ��: " + (data.attackSpeed > 0 ? data.attackSpeed.ToString("F2") : "��"); // F2������λС��
            attackRangeText.text = "������Χ: " + (data.attackRange > 0 ? data.attackRange.ToString("F2") : "��");
            moveSpeedText.text = "�ƶ��ٶ�: " + (data.moveSpeed > 0 ? data.moveSpeed.ToString("F2") : "��");
        }

        // ��ʾ��״̬
        public void DisplayEmpty(string emptyText = "δװ��")
        {
            container.SetActive(true);
            itemNameText.text = emptyText;
            itemLevelText.text = "";
            // --- �޸��ⲿ�� ---
            itemIcon.enabled = true; // ͬ�����ּ���
            itemIcon.sprite = null;  // ��ͼƬ����Ϊ�գ������ǽ������
            itemIcon.color = new Color(1, 1, 1, 0);

            // �������Զ���ʾ"��"
            attackText.text = "������: ��";
            defenseText.text = "������: ��";
            healthText.text = "����ֵ: ��";
            attackSpeedText.text = "����Ƶ��: ��";
            attackRangeText.text = "������Χ: ��";
            moveSpeedText.text = "�ƶ��ٶ�: ��";
        }
    }

    // ��ʼ�����
    public void Initialize(UIManager manager)
    {
        uiManager = manager;
        // Ϊ��ť���¼�
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        backButton.onClick.AddListener(ClosePanel);
    }

    // ��ʾ��岢�������
    public void ShowPanel(EquipmentData currentEquipped, EquipmentData selected)
    {
        gameObject.SetActive(true);
        newEquipmentToEquip = selected;

        // �����ǰ��װ������ʾ������Ϣ
        if (currentEquipped != null)
        {
            leftColumn.Display(currentEquipped);
        }
        else // ������ʾ��״̬
        {
            leftColumn.DisplayEmpty();
        }

        // ��ʾѡ��װ������Ϣ
        rightColumn.Display(selected);
    }

    // "װ��"��ť�����
    public void OnEquipButtonClicked()
    {
        uiManager.EquipItem(newEquipmentToEquip);
        ClosePanel(); // װ����رնԱ����
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}