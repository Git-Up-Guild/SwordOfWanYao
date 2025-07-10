// using System;

// namespace SwordOfWanYao
// {
//     / <summary>
//     / 卡牌类型
//     / </summary>
//     public enum CardType
//     {
//         Unit,   // 兵种卡
//         Buff    // 增益卡
//     }

//     / <summary>
//     / 兵种类型
//     / </summary>
//     public enum UnitType
//     {
//         None,
//         刀战将,
//         长矛将,
//         神弓将,
//         火法师,
//         风祭司,
//         光罗汉
//     }

//     / <summary>
//     / 卡牌基础信息配置
//     / </summary>
//     [Serializable]
//     public class CardConfig
//     {
//         / <summary>
//         / 卡牌唯一标志
//         / </summary>
//         public string Id;
//         / <summary>
//         / 卡牌名称
//         / </summary>
//         public string Name;
//         / <summary>
//         / 卡牌类型（兵种/增益）
//         / </summary>
//         public CardType Type;
//         / <summary>
//         / 目标兵种（如有）
//         / </summary>
//         public UnitType TargetUnit;
//         / <summary>
//         / 卡牌描述
//         / </summary>
//         public string Description;
//         / <summary>
//         / 图标资源路径或标志
//         / </summary>
//         public string Icon;
//     }
// } 