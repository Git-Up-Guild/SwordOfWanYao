/*******************************************************************
** �ļ���:	EnMonitorType.cs
** ��  Ȩ:	(C) ���ڱ������缼�����޹�˾
** ������:	������
** ��  ��:	2020/9/4
** ��  ��:	1.0
** ��  ��:	 
********************************************************************/
using XClient.Common;
using XGame.EtaMonitor;
using System;
using UnityEngine;

namespace XClient.Game
{
    public class NetMonitor : MonitorBase, INetMonitor
    {
        public NetMonitor(string monitorName, MonitorConfig config) : base(monitorName, config)
        {
#if DEBUG_LOG
            ///#            Debug.Log("<color=cyan>EtaMonitor >> ������Ϣ��������</color>");
#endif
        }

        public float GetTime()
        {
            return Time.realtimeSinceStartup;
        }


        public void OnReport(int id, string desc, float costTime)
        {
            base.Add(id, desc, costTime);
        }
    }

}
