using UnityEngine;
using System.Collections.Generic; // �����б�

// 1. ����һ���µ�ö�٣��������ֿ�������
public enum CardType
{
    UnlockUnit, // �����±��ֵĿ�
    UnitSkill,  // ǿ�����ֵļ��ܿ�
    GlobalBuff  // ȫ�����濨
}


[CreateAssetMenu(fileName = "NewCard", menuName = "Card Config")]
public class CardConfig : ScriptableObject
{
    public int ID;
    public Sprite ICON;
    public string Name;
    [TextArea] public string Description;

    [Header("���ƻ���")]
    public CardType Type; // 2. ��ӿ��������ֶ�
    public EffectBase Effect;
    public int MaxOwnable = 6;

    [Header("�������� (���Լ��ܿ���Ч)")]
    // 3. ��ӽ��������ֶ�
    // ����б������ˡ���Ҫ��Щ���ֱ����������ſ����ܳ��֡�
    // ע�⣺��������ֱ���� UnitData ������������ֱ��
    public List<CardConfig> RequiredUnlockCards;
}
