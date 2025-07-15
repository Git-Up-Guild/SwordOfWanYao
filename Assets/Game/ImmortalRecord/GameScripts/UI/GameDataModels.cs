using UnityEngine;

// ����ļ����������Ϸ���õ��ĸ�������ģ��

// 1. ����������Ʒ������
[System.Serializable] // ����������Inspector����ʾ
public class RewardItemData
{
    public Sprite itemIcon;
    public int quantity;
}

// 2. ������λ��ս��ͳ������
[System.Serializable]
public class UnitStatData
{
    public Sprite unitIcon;
    public string unitName;
    public int unitLevel;
    public float damagePercentage; // �˺��ٷֱ� (0��1)
    public string damageValueText; // �˺�������ֵ�ı� (�� "69.4��")
    public float tankPercentage;   // ���˰ٷֱ� (0��1)
    public string tankValueText;   // ���˾�����ֵ�ı� (�� "12244")
}

// 3. ����ʤ�������������ݰ�
[System.Serializable]
public class VictoryData
{
    // public string levelName; // �����Ҫ��ʾ�ؿ���
    public System.Collections.Generic.List<RewardItemData> rewards;
    public System.Collections.Generic.List<UnitStatData> unitStats;
}