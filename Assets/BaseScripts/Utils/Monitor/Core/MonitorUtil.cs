﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    static public class MonitorUtil
    {
        /// <summary>
        /// 是否包含监控类型
        /// </summary>
        /// <param name="srcMask">已选监控掩码</param>
        /// <param name="target">目标监控类型</param>
        /// <returns></returns>
        static public bool ContainType(MonitorType srcMask, MonitorType target)
        {
            return (srcMask & target) != 0;
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        static public string FormatTime(DateTime time)
        {
            return time.ToString("yyMMddHHmmss");
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="successLog"></param>
        /// <returns></returns>
        static public bool WriteToFile(string path, string content, string successLog,bool isCheck = true, bool showFolder = true)
        {
#if UNITY_EDITOR
            if (!File.Exists(path) || UnityEditor.EditorUtility.DisplayDialog("存在同名文件夹！", string.Format("是否覆盖文件：{0}", path), "确定", "取消"))
#endif
            {
                File.WriteAllText(path, content);
#if UNITY_EDITOR
                if(showFolder)
                {
                    UnityEditor.EditorUtility.RevealInFinder(path);
                }
               
#endif
                Debug.Log(successLog);
                return true;
            }
            return false;
        }


    }
}
