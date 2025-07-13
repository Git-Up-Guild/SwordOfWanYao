// �ļ���: EquipmentItem.cs
using UnityEngine;

// ����ű��ļ�ֻ�����������Ͷ����ScriptableObjectģ�壬�������ǰѾɵ�ͨ������Ҳ�Ƶ���������

// 1. װ������ö�� (�Ӿɵ� EquipmentDataTypes.cs �ƹ���)
public enum EquipmentType
{
    Head,
    Armor,
    Weapon,
    Boots,
    Gloves,
    Accessory
}

// 2. ����ʱ��װ�����ݽṹ (���������Ȼ��Ҫ�������Լ�)
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

// 3. ScriptableObject ģ�� (�����µĺ���)
// [CreateAssetMenu] �ǹؼ����������ǿ���ֱ����Project�����Ҽ������������͵���Դ
[CreateAssetMenu(fileName = "NewEquipment", menuName = "MyGame/Create Equipment Item")]
public class EquipmentItem : ScriptableObject
{
    // �� EquipmentData ����������Զ��ᵽ����
    [Header("������Ϣ")]
    public string itemName = "��װ��";
    public int level = 1;
    public Sprite icon; // ������ֱ������ͼ�꣡
    public EquipmentType type;

    [Header("��������")]
    public int attack;
    public int defense;
    public int health;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;

    // ���ǿ������һ������������ؽ���������ת��������ʱ��EquipmentData��ʽ
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