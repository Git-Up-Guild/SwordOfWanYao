using System.Collections.Generic;
using UnityEngine.Pool;

namespace XClient.Rand
{
    public enum RandomItemResult
    {
        //随机成功
        Success,

        //Item的数量为0，无法随机
        ItemCountIsZero,

        //内部错误，正常情况下不应该返回
        InternalError,
    }

    public static class RandomUtility
    {
        class ItemInfo
        {
            public int Id;
            public int Num;
            public uint Probability;
            public bool IsCheckNum;
        }

        private static ObjectPool<ItemInfo> s_Pool;

        private static List<ItemInfo> s_ItemInfos;

        private static uint s_TotalProbability;

        /// <summary>
        /// 最有一次进行Item随机的结果
        /// </summary>
        public static RandomItemResult LastRandomItemsResult { get; private set; }

        static RandomUtility()
        {
            s_Pool = new ObjectPool<ItemInfo>(() => {
                return new ItemInfo();
            });

            s_ItemInfos = new List<ItemInfo>();
        }

        /// <summary>
        /// 纯随机一个出来
        /// </summary>
        /// <returns></returns>
        public static int Range(int min, int max)
        {
            return UnityEngine.Random.Range(min, max+1);
        }

        /// <summary>
        /// 添加随机项
        /// </summary>
        /// <param name="itemID">ID</param>
        /// <param name="probability">权重</param>
        /// <param name="num">数量</param>
        /// <param name="isCheckNum">是否限制数量，如果限制了数量，那么当数量变成0的时候将不会被随机到</param>
        public static void AddItem(int itemID, uint probability, int num = 1, bool isCheckNum = false)
        {
            //这种情况下是不能被随机到的，因此不添加
            if (isCheckNum && num <= 0)
                return;

            var itemInfo = s_Pool.Get();
            itemInfo.Id = itemID;
            itemInfo.Num = num == 0 ? 1 : num;
            itemInfo.Probability = probability;
            itemInfo.IsCheckNum = isCheckNum;

            s_ItemInfos.Add(itemInfo);
            s_TotalProbability += probability;
        }

        /// <summary>
        /// 清除随机项
        /// </summary>
        public static void ClearItems()
        {
            foreach(var info in s_ItemInfos)
            {
                s_Pool.Release(info);
            }
            s_ItemInfos.Clear();
            s_TotalProbability = 0;
        }

        /// <summary>
        /// 圆桌随机
        /// </summary>
        /// <param name="num">要随机的数量</param>
        /// <param name="bIndependence">是否独立随机，独立随机时数据可能会重复</param>
        /// <param name="outResult">返回结果</param>
        /// <returns>随机是否成功，如果返回失败，可以通过 LastRandomItemsResult 获取详细信息</returns>
        public static bool RandomItems(int num, bool bIndependence, List<int> outResult)
        {
            //没有任何Item
            if (s_ItemInfos.Count == 0)
            {
                LastRandomItemsResult = RandomItemResult.ItemCountIsZero;
                return false;
            }

            //非独立随机时，Item数量不足
            if (num > s_ItemInfos.Count && !bIndependence)
                num = s_ItemInfos.Count;

            var maxItemIndex = s_ItemInfos.Count - 1;
            for(var i = 0; i < num; i++)
            {
                //随机出来一个
                var itemIndex = RandomOneItem(maxItemIndex, s_TotalProbability, out var itemInfo);
                if(itemIndex < 0)
                {
                    LastRandomItemsResult = RandomItemResult.InternalError;
                    return false;
                }

                outResult.Add(itemInfo.Id);

                //如果是有数量限制的，还需要把数量减少数量
                var isOverNum = false;
                if (itemInfo.IsCheckNum && itemInfo.Num > 0)
                {
                    itemInfo.Num--;
                    if(itemInfo.Num == 0)
                        isOverNum = true;
                }

                //非独立随机&数量不足的则将刚随机的这个Item 与 最后一个有效的Item进行交换，并且将参与随机的Item最大索引-1，
                if (!bIndependence || isOverNum)
                {
                    var temp = s_ItemInfos[maxItemIndex];
                    s_ItemInfos[maxItemIndex] = itemInfo;
                    s_ItemInfos[itemIndex] = temp;
                    s_TotalProbability -= itemInfo.Probability;

                    //提出了一个，所以最大索引也需要减少
                    maxItemIndex--;
                }
            }

            LastRandomItemsResult = RandomItemResult.Success;
            return true;
        }

        /// <summary>
        /// 进行一次随机
        /// </summary>
        /// <param name="maxItemIndex"></param>
        /// <param name="totalProbability"></param>
        /// <param name="outItemInfo">随机到的Item</param>
        /// <returns>-1表示随机失败, 其它表示随机到的Item索引</returns>
        private static int RandomOneItem(int maxItemIndex, uint totalProbability, out ItemInfo outItemInfo)
        {
            outItemInfo = null;

            var randProbability = Range(1, (int)totalProbability);
            uint addProbability = 0;

            //遍历检查，看落在了哪一个Item上
            for (var i = 0; i <= maxItemIndex; i++)
            {
                var itemInfo = s_ItemInfos[i];

                addProbability += itemInfo.Probability;

                if (randProbability <= addProbability)
                {
                    outItemInfo = itemInfo;
                    return i;
                }
            }

            return -1;
        }

    }
}
