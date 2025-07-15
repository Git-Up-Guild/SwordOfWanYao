// 文件名: UnitData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "MyGame/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("核心信息")]
    public string unitName;
    public Sprite icon; // 兵种图标
    public GameObject unitPrefab; // 战场上实际生成的兵种预制件

    [Header("生产信息")]
    public float spawnInterval = 10f; // 生产所需时间

    // 你还可以添加更多属性，比如：
    // public int cost; // 召唤或升级的消耗
    // public string description; // 兵种描述
}