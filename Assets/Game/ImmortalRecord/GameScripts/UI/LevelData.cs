// 文件名: LevelData.cs
using UnityEngine;
using System.Collections.Generic; // 引入列表

// [CreateAssetMenu] 属性让我们可以从 Assets/Create 菜单创建这个类型的资源文件
[CreateAssetMenu(fileName = "NewLevelData", menuName = "MyGame/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("关卡基础信息")]
    public int levelNumber;         // 关卡编号，例如：1, 2, 3
    public string levelName;        // 关卡名称，例如：“竹林深处”
    public Sprite levelImage;       // 在选择界面显示的关卡大图

    [Header("战斗场景信息")]
    public string sceneNameToLoad;  // 点击“战斗”后需要加载的场景文件名
                                    // 重要：这个名字必须和你在 Build Settings 中添加的场景名字完全一致！
    [Header("关卡奖励信息")]
    // 新增一个列表，用来存放这个关卡胜利后会获得的所有奖励
    public List<RewardItemData> victoryRewards;
}