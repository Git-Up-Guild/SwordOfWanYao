using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.SelectOne
{
    /// <summary>
    /// N选一功能数据源
    /// </summary>
    public interface ISelectOneDataSource
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        bool Create();

        /// <summary>
        /// 转换函数
        /// </summary>
        /// <param name="item"></param>
        /// <param name="data"></param>
        bool ConvertToOptionData(object item, SelectOneOptionData data);

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="outAvaliableItems"></param>
        void GetAvailableItems(List<object> outAvaliableItems, int limit = 3);

        /// <summary>
        /// 销毁
        /// </summary>
        void Release();
    }
}
