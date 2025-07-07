using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace XClient.Scripts.Monitor
{
    public class MonoGameRuntimeTest : MonoBehaviour
    {
        [Header("输出目录")]
        public string outputFolder = "MonitorOutput";

        [Header("过滤指定桶")]
        public string sizeMapFilterKey;

        [Header("是否可以工作")]
        public bool isCanWork;


#if UNITY_EDITOR && !UNITY_ANDROID
        private void OnEnable()
        {
            //Debug.LogError("MonoGameRuntimeTest OnEnable");
            var isValid = MonitorMono.IsVaild();
            if (!isValid)
                return;
            if (sizeMapFilterKey.Length >= 7) // 限制key长度
                sizeMapFilterKey = sizeMapFilterKey.Substring(0, 7);
            MonitorMono.Q1Game_Monitor_SetSizeMapFilter(sizeMapFilterKey);
            MonitorMono.LoadFilter();
            MonitorMono.Init();
            MonitorMono.Start();
            isCanWork = MonitorMono.IsCanWork();
        }

        private void OnDisable()
        {
            //Debug.LogError("MonoGameRuntimeTest OnDisable");
            if (!isCanWork)
                return;
            var curTime = DateTime.Now;
            MonitorMono.OutputMonoSnapshot(curTime);
            MonitorMono.OutputSizeMapData(curTime);
            MonitorMono.Stop();
            MonitorMono.Dispose();
        }
#endif

    
    }
}