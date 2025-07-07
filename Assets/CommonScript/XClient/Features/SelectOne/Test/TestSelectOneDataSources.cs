using System.Collections.Generic;
using Random = XClient.Rand.RandomUtility;

namespace XClient.SelectOne.Test
{
    public class TestData
    {
        public string name;
        public int iconID;
        public string intro;
    }


    public class TestSelectOneDataSources : ISelectOneDataSource
    {
        private List<TestData> m_DataList;

        public bool ConvertToOptionData(object item, SelectOneOptionData outData)
        {
            var data = item as TestData;
            outData.iconID = data.iconID;
            outData.desc = data.intro;
            outData.title = data.name;

            return true;
        }

        public bool Create()
        {
            m_DataList = new List<TestData>();

            for (var i = 0; i < 10; i++)
            {
                var data = new TestData();
                data.intro = $"自我介绍++++++ {i}";
                data.iconID = 6001 + i;
                data.name = $"新玩家{i}";

                m_DataList.Add(data);
            }

            return true;
        }

        public void GetAvailableItems(List<object> outAvaliableItems, int limit = 3)
        {
            outAvaliableItems.Clear();

            Random.ClearItems();

            for(var i = 0; i < m_DataList.Count; i++)
            {
                Random.AddItem(i, 100);
            }

            List<int> ret = new List<int>();

            if(Random.RandomItems(limit, false, ret))
            {
                for(var j = 0; j < ret.Count; j++)
                {
                    outAvaliableItems.Add(m_DataList[ret[j]]);
                }
            }
        }

        public void Release()
        {
            m_DataList.Clear();
        }

    }
}
