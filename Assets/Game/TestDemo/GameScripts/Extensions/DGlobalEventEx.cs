using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;

public class DGlobalEventEx : DGlobalEvent
{
    //卡牌相关
    public const ushort EVENT_CARD_BASE = EVENT_ALL_MAXID;
    public const ushort EVENT_CARD_DATA_UPDATE = EVENT_CARD_BASE + 1; //卡牌数据变更

}
