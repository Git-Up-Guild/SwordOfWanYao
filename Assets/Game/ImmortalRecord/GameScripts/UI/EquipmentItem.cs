// 文件名: EquipmentItem.cs
using UnityEngine;

// 这个脚本文件只包含数据类型定义和ScriptableObject模板，所以我们把旧的通用数据也移到这里来。

// 1. 装备类型枚举 (从旧的 EquipmentDataTypes.cs 移过来)
public enum EquipmentType
{
    Head,
    Armor,
    Weapon,
    Boots,
    Gloves,
    Accessory
}

// 2. 运行时的装备数据结构 (这个我们仍然需要，但可以简化)
[System.Serializable]
public class EquipmentData
{
    public string itemName;
    public int level;
    public Sprite icon;
    public EquipmentType type;

    public int attack;
    public int defense;
    public int health;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;
}

// 3. ScriptableObject 模板 (这是新的核心)
// [CreateAssetMenu] 是关键，它让我们可以直接在Project窗口右键创建这种类型的资源
[CreateAssetMenu(fileName = "NewEquipment", menuName = "MyGame/Create Equipment Item")]
public class EquipmentItem : ScriptableObject
{
    // 把 EquipmentData 里的所有属性都搬到这里
    [Header("基础信息")]
    public string itemName = "新装备";
    public int level = 1;
    public Sprite icon; // 在这里直接设置图标！
    public EquipmentType type;

    [Header("核心属性")]
    public int attack;
    public int defense;
    public int health;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;

    // 我们可以添加一个方法，方便地将自身数据转换成运行时的EquipmentData格式
    public EquipmentData ToEquipmentData()
    {
        EquipmentData data = new EquipmentData();
        data.itemName = this.itemName;
        data.level = this.level;
        data.icon = this.icon;
        data.type = this.type;
        data.attack = this.attack;
        data.defense = this.defense;
        data.health = this.health;
        data.attackSpeed = this.attackSpeed;
        data.attackRange = this.attackRange;
        data.moveSpeed = this.moveSpeed;
        return data;
    }
}