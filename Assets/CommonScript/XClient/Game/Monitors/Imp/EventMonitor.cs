/*******************************************************************
** �ļ���:	EnMonitorType.cs
** ��  Ȩ:	(C) ���ڱ������缼�����޹�˾
** ������:	������
** ��  ��:	2020/9/4
** ��  ��:	1.0
** ��  ��:	 �������ö��
********************************************************************/
using XGame.EtaMonitor;
using XGame.EventEngine;
using System;
using UnityEngine;

namespace XClient.Game
{
    public class EventMonitor : MonitorBase, IEventMonitor
    {
        public EventMonitor(string monitorName, MonitorConfig config) : base(monitorName, config)
        {
#if DEBUG_LOG
///#            Debug.Log("<color=cyan>EtaMonitor >> �¼�����������</color>");
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
