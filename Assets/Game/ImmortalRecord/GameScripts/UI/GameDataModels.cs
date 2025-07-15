using UnityEngine;

// 这个文件用来存放游戏中用到的各种数据模型

// 1. 单个奖励物品的数据
[System.Serializable] // 让它可以在Inspector中显示
public class RewardItemData
{
    public Sprite itemIcon;
    public int quantity;
}

// 2. 单个单位的战斗统计数据
[System.Serializable]
public class UnitStatData
{
    public Sprite unitIcon;
    public string unitName;
    public int unitLevel;
    public float damagePercentage; // 伤害百分比 (0到1)
    public string damageValueText; // 伤害具体数值文本 (如 "69.4万")
    public float tankPercentage;   // 承伤百分比 (0到1)
    public string tankValueText;   // 承伤具体数值文本 (如 "12244")
}

// 3. 整个胜利结算界面的数据包
[System.Serializable]
public class VictoryData
{
    // public string levelName; // 如果需要显示关卡名
    public System.Collections.Generic.List<RewardItemData> rewards;
    public System.Collections.Generic.List<UnitStatData> unitStats;
}