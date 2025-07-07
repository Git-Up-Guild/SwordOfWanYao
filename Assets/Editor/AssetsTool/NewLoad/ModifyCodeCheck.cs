using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XGameEditor.NewLoad
{

     public class ModifyCodeCheck
    {
        //代码检测配置
        static string m_strCodeCheckCfg = Application.dataPath + "/../BuildCfg/code_check/code_check.csv";

        //检查代码是否有变化
        static public  void GenAndCheckCode(string lastPath,string newPath,out bool bModified,out List<string> listModifiedCodes)
        {
            bModified = true;
            listModifiedCodes = new List<string>();
            //产生新的Md5
            Dictionary<string, string> dicNewMd5 = new Dictionary<string, string>();
            if(GenMd5s(m_strCodeCheckCfg, dicNewMd5)==false)
            {
                return ;
            }

            //保存新的md5表格
            SaveMd5File(newPath, dicNewMd5);

            //读取老表格
            Dictionary<string, string> dicOldMd5 = new Dictionary<string, string>();
            LoadMd5File(lastPath, dicOldMd5);

            if(dicOldMd5.Count==0)
            {
                bModified =  true;
                return;
            }

            string changePath = newPath.Replace("codemd5.csv", "md5changefiles.csv");
            bModified =  CompareMd5(dicOldMd5, dicNewMd5, changePath, listModifiedCodes);
        }

        static public bool GenMd5s(string path, Dictionary<string, string> dicMd5)
        {
            dicMd5.Clear();

            if (File.Exists(path)==false)
            {
                Debug.LogError("配置文件不存在： " + path);
                return false;
            }

            HashSet<string> hashCheckPaths = new HashSet<string>();
            HashSet<string> hashFilterPaths = new HashSet<string>();

            int nParentLen = Application.dataPath.Length;

            string[] cfgs = File.ReadAllLines(path, System.Text.Encoding.GetEncoding("gb2312"));
            int nLen = cfgs.Length;
            //跳过第一行
            for (int i = 1; i < nLen; ++i)
            {
                string[] cfg = cfgs[i].Split(',');
                if (cfg.Length < 4)
                {
                    continue;
                }

                string checkpath = Application.dataPath + "/" + cfg[0];

                if(Directory.Exists(checkpath)==false)
                {
                    Debug.LogError("代码检测目录不存在：" + checkpath);
                    continue;
                }

                //添加需要检查的文件
                if(string.IsNullOrEmpty(cfg[1])==false)
                {
                    string[] paths = Directory.GetFiles(checkpath, cfg[1], SearchOption.AllDirectories);
                    int nCount = paths.Length;
                    for (int j = 0; j < nCount; ++j)
                    {
                        paths[j] = paths[j].Replace('\\', '/');
                        if (hashCheckPaths.Contains(paths[j]) == false)
                        {
                            hashCheckPaths.Add(paths[j]);
                        }
                    }
                }

                //添加需要过滤的文件
                if (string.IsNullOrEmpty(cfg[2]) == false)
                {
                    string[] paths = Directory.GetFiles(checkpath, cfg[2], SearchOption.AllDirectories);
                    int nCount = paths.Length;
                    for (int j = 0; j < nCount; ++j)
                    {
                        paths[j] = paths[j].Replace('\\', '/');
                        if (hashFilterPaths.Contains(paths[j]) == false)
                        {
                            hashFilterPaths.Add(paths[j]);
                        }
                    }
                }

                //过滤目录

                if (string.IsNullOrEmpty(cfg[3]) == false)
                {
                    checkpath = Application.dataPath + "/" + cfg[3];

                    if (Directory.Exists(checkpath))
                    {
                        string[] paths = Directory.GetFiles(checkpath, "*.*", SearchOption.AllDirectories);
                        int nCount = paths.Length;
                        for (int j = 0; j < nCount; ++j)
                        {
                            paths[j] = paths[j].Replace('\\', '/');
                            if (hashFilterPaths.Contains(paths[j]) == false)
                            {
                                hashFilterPaths.Add(paths[j]);
                            }
                        }

                    }

                   
                }



            }

            //删除过滤的文件
            HashSet<string> hashFinalPaths = new HashSet<string>();
            foreach (string filePath in hashCheckPaths)
            {
                
                if (hashFilterPaths.Contains(filePath)==false)
                {


                    
                    if (filePath.Contains("Editor/")==false&& filePath.Contains("/obj/Release") == false&& filePath.Contains("/obj/Debug") == false&& filePath.Contains("*.meta") == false && filePath.Contains(".unitypackage") == false)
                    {
                        hashFinalPaths.Add(filePath);
                    }

                    
                }
            }

            

            //计算md5 
            foreach (string filePath in hashFinalPaths)
            {
                string realPath = filePath.Substring(nParentLen + 1);
                string md5 = MD5.GetMD5(filePath);

                /*
                if(dicMd5.ContainsKey(realPath))
                {
                    Debug.LogError("存在重复的文件：" + realPath);
                    continue;
                }
                */

                dicMd5.Add(realPath, md5);
            }

            return true;
        }



        //从文件中加载md5
        static public void LoadMd5File(string path,Dictionary<string,string> dicMd5)
        {
            dicMd5.Clear();
            if(File.Exists(path)==false)
            {
                return;
            }

            string[] md5s = File.ReadAllLines(path);
            int nLen = md5s.Length;
            for(int i=0;i<nLen;++i)
            {
                string[] md5 = md5s[i].Split(',');
                if(md5.Length<2)
                {
                    continue;
                }

                if(dicMd5.ContainsKey(md5[0])==false)
                {
                    dicMd5.Add(md5[0], md5[1]);
                }
                
            }
        }

        //存储MD5文件
        static public void SaveMd5File(string path, Dictionary<string, string> dicMd5)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }

            if(dicMd5.Count==0)
            {
                return;
            }

            string[] contents = new string[dicMd5.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> kvp in dicMd5)
            {
                contents[i++] = kvp.Key + "," + kvp.Value;
            }

            File.WriteAllLines(path,contents);

        }

        //对比Md5
        static bool CompareMd5(Dictionary<string, string> dicOld, Dictionary<string, string> dicNew,string changePath, List<string> listModifiedCodes)
        {


            bool ret = false;

            foreach (KeyValuePair<string, string> kvp in dicNew)
            {
               if(dicOld.ContainsKey(kvp.Key)==false)
                {
                    Debug.LogError("代码变化: 新增加的代码: "+ kvp.Key);
                    //File.WriteAllText(changePath, kvp.Key);
                    listModifiedCodes.Add(kvp.Key);
                    ret =  true;
                }else if(dicOld[kvp.Key]!=kvp.Value)
                {
                    Debug.LogError("代码变化: 代码变更: " + kvp.Key);
                    //File.WriteAllText(changePath, kvp.Key);
                    listModifiedCodes.Add(kvp.Key);
                    ret =  true;
                }
            }

            File.WriteAllLines(changePath, listModifiedCodes);

            return ret;
        }


    }
}
