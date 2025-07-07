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
    public class MonoFilterList
    {
        public readonly string FilePath;

        private List<string> _filterList = new List<string>();
        public List<string> FilterList { get => _filterList; }

        public MonoFilterList(string filePath)
        {
            FilePath = filePath;
        }


        /// <summary>
        /// 在列表
        /// </summary>
        /// <returns></returns>
        public bool IsIn(string name)
        {
            return _filterList.Contains(name);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public void Load()
        {
            if (!File.Exists(FilePath))
                File.Create(FilePath);
            string textAsset = File.ReadAllText(FilePath);
            _filterList = JsonMapper.ToObject<List<string>>(textAsset);
            if (_filterList == null)
                _filterList = new List<string>();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            var jsonStr = JsonMapper.ToJson(_filterList);
            File.WriteAllText(FilePath, jsonStr);
            Debug.Log($"保存过滤列表：{FilePath} \ndata: {jsonStr}");
        }

        /// <summary>
        /// 添加白名单项
        /// </summary>
        public void Add(string key)
        {
            if (!string.IsNullOrEmpty(key) && !_filterList.Contains(key))
            {
                _filterList.Add(key);
            }
        }

        /// <summary>
        /// 删除白名单项
        /// </summary>
        public void Remove(string key)
        {
            if (_filterList.Contains(key))
            {
                _filterList.Remove(key);
            }
        }
    }
}
