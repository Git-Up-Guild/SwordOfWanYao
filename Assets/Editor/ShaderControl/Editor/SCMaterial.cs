using UnityEngine;
using System.Collections.Generic;

namespace ShaderControl
{
    public class SCMaterial
    {
        public string name = "";
        public string path = "";
        public string GUID = "";

        //材质使用到的关键字
        public List<SCKeyword> keywords = new List<SCKeyword>();

        public bool pendingChanges;

        HashSet<string> keywordSet = new HashSet<string>();

        public SCMaterial(string name, string path, string GUID)
        {
            this.name = name;
            this.path = path;
            this.GUID = GUID;
        }

        public void SetKeywords(string[] names)
        {
            for (int k = 0; k < names.Length; k++)
            {
                if (!keywordSet.Contains(names[k]))
                {
                    keywordSet.Add(names[k]);
                    SCKeyword keyword = new SCKeyword(names[k]);
                    keywords.Add(keyword);
                }
            }
            keywords.Sort(delegate (SCKeyword k1, SCKeyword k2) { return k1.name.CompareTo(k2.name); });
        }

        /// <summary>
        /// 是否包含指定的关键字
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsKeyword(string name)
        {
            return keywordSet.Contains(name);
        }

        public void RemoveKeyword(string name)
        {
            for (int k = 0; k < keywords.Count; k++)
            {
                if (keywords[k].name.Equals(name))
                {
                    keywords.RemoveAt(k);
                    return;
                }
            }
        }
    }

}