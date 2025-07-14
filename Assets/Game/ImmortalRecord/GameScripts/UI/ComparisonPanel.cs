using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I18N.Common;

public class ComparisonPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private EquipmentDisplayUI leftColumn; // 当前装备栏
    [SerializeField] private EquipmentDisplayUI rightColumn; // 选中装备栏
    [SerializeField] private Button equipButton;
    [SerializeField] private Button backButton;

    private EquipmentData newEquipmentToEquip;
    private UIManager uiManager;

    // 辅助类，用来管理一栏的UI
    [System.Serializable]
    public class EquipmentDisplayUI
    {
        public GameObject container; // 整个栏的父物体，方便一起显示/隐藏
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemLevelText;

        // 6个属性文本
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI defenseText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI attackSpeedText;
        public TextMeshProUGUI attackRangeText;
        public TextMeshProUGUI moveSpeedText;

        // 显示装备数据
        public void Display(EquipmentData data)
        {
            container.SetActive(true);
            // --- 在这里添加或修改 ---
            itemIcon.enabled = true; // 关键！确保Image组件是激活的！
            itemIcon.color = Color.white; // 2. 关键！将颜色重置为不透明的白色 (等同于 new Color(1, 1, 1, 1))

            // ----------------------
            itemIcon.sprite = data.icon;
            itemNameText.text = data.itemName;
            itemLevelText.text = "Lv." + data.level;

            // 如果属性值为0，显示"—"，否则显示数值
            attackText.text = "攻击力: " + (data.attack > 0 ? data.attack.ToString() : "—");
            defenseText.text = "防御力: " + (data.defense > 0 ? data.defense.ToString() : "—");
            healthText.text = "生命值: " + (data.health > 0 ? data.health.ToString() : "—");
            attackSpeedText.text = "攻击频率: " + (data.attackSpeed > 0 ? data.attackSpeed.ToString("F2") : "—"); // F2保留两位小数
            attackRangeText.text = "攻击范围: " + (data.attackRange > 0 ? data.attackRange.ToString("F2") : "—");
            moveSpeedText.text = "移动速度: " + (data.moveSpeed > 0 ? data.moveSpeed.ToString("F2") : "—");
        }

        // 显示空状态
        public void DisplayEmpty(string emptyText = "未装备")
        {
            container.SetActive(true);
            itemNameText.text = emptyText;
            itemLevelText.text = "";
            // --- 修改这部分 ---
            itemIcon.enabled = true; // 同样保持激活
            itemIcon.sprite = null;  // 将图片设置为空，而不是禁用组件
            itemIcon.color = new Color(1, 1, 1, 0);

            // 所有属性都显示"—"
            attackText.text = "攻击力: —";
            defenseText.text = "防御力: —";
            healthText.text = "生命值: —";
            attackSpeedText.text = "攻击频率: —";
            attackRangeText.text = "攻击范围: —";
            moveSpeedText.text = "移动速度: —";
        }
    }

    // 初始化面板
    public void Initialize(UIManager manager)
    {
        uiManager = manager;
        // 为按钮绑定事件
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        backButton.onClick.AddListener(ClosePanel);
    }

    // 显示面板并填充数据
    public void ShowPanel(EquipmentData currentEquipped, EquipmentData selected)
    {
        gameObject.SetActive(true);
        newEquipmentToEquip = selected;

        // 如果当前有装备，显示它的信息
        if (currentEquipped != null)
        {
            leftColumn.Display(currentEquipped);
        }
        else // 否则显示空状态
        {
            leftColumn.DisplayEmpty();
        }

        // 显示选中装备的信息
        rightColumn.Display(selected);
    }

    // "装备"按钮被点击
    public void OnEquipButtonClicked()
    {
        uiManager.EquipItem(newEquipmentToEquip);
        ClosePanel(); // 装备后关闭对比面板
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}