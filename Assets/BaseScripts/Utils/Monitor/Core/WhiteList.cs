using XGame.LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    /// <summary>
    /// 白名单
    /// </summary>
    static public class WhiteList
    {
        //static readonly string _folderName = "Assets/Scripts/Monitor/Config";
        //static readonly string _fileName = "monitor_app_white_list";
        //static public string FilePath { get => string.Format("{0}/{1}.txt", MonitorConfig.ConfigFolderPath, _fileName); }
        static public int Count { get => _dataDic.Count; }
        static private Dictionary<string, int> _dataDic = new Dictionary<string, int>();
        static public Dictionary<string, int> DataDic { get => _dataDic; }

        /// <summary>
        /// 在白名单内
        /// </summary>
        /// <param name="unit">数据单元</param>
        /// <returns></returns>
        static public bool IsIn(string name,int count)
        {
            return _dataDic.ContainsKey(name) && _dataDic[name] >= count;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        static public void Load()
        {
            if(File.Exists(MonitorConfig.WhiteListPath))
            {
                string textAsset = File.ReadAllText(MonitorConfig.WhiteListPath);
                _dataDic = JsonMapper.ToObject<Dictionary<string, int>>(textAsset);
            }
           
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        static public void Save()
        {
            var jsonStr = JsonMapper.ToJson(_dataDic);
            File.WriteAllText(MonitorConfig.WhiteListPath, jsonStr);
            Debug.Log($"保存白名单：{jsonStr}");
        }

        /// <summary>
        /// 添加白名单项
        /// </summary>
        static public void Add(string key, int count)
        {
            if (!string.IsNullOrEmpty(key) && !_dataDic.ContainsKey(key))
            {
                _dataDic.Add(key, count);
            }
        }

        /// <summary>
        /// 删除白名单项
        /// </summary>
        static public void Remove(string key)
        {
            if (_dataDic.ContainsKey(key))
            {
                _dataDic.Remove(key);
            }
        }

        /// <summary>
        /// 修改白名单项
        /// </summary>
        static public void Modify(string key, int count)
        {
            if (_dataDic.ContainsKey(key))
            {
                _dataDic[key] = count;
            }
        }
    }
}
