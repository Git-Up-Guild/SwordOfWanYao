using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Common
{
    public interface ILuaEngine
    {
        /// <summary>
        /// 运行一个Lun函数
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="args"></param>
        void RunLuaFunction(string funcName, params object[] args);

        /// <summary>
        /// 获取一个泛型对象，可以是接口也可以是函数，看具体的应用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcName"></param>
        /// <returns></returns>
        T Get<T>(string funcName);

        /// <summary>
        /// 执行一个脚本片段
        /// </summary>
        /// <param name="strScript"></param>
        void DoString(string strScript);

    }
}
