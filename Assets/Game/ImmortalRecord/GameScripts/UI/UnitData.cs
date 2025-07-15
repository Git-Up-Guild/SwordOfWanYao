// �ļ���: UnitData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "MyGame/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("������Ϣ")]
    public string unitName;
    public Sprite icon; // ����ͼ��
    public GameObject unitPrefab; // ս����ʵ�����ɵı���Ԥ�Ƽ�

    [Header("������Ϣ")]
    public float spawnInterval = 10f; // ��������ʱ��

    // �㻹������Ӹ������ԣ����磺
    // public int cost; // �ٻ�������������
    // public string description; // ��������
}