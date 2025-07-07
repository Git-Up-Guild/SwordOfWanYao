/*******************************************************************
** �ļ���:	EnMonitorType.cs
** ��  Ȩ:	(C) ���ڱ������缼�����޹�˾
** ������:	������
** ��  ��:	2020/9/4
** ��  ��:	1.0
** ��  ��:	 
********************************************************************/

using XGame.EtaMonitor;
using XGame.Timer;
using System;
using UnityEngine;
using XGame;

namespace XClient.Game
{
    public class XGameComsMonitor : MonitorBase, IXGameComsMonitor
    {
        public XGameComsMonitor(string monitorName, MonitorConfig config) : base(monitorName, config)
        {
#if DEBUG_LOG
///#            Debug.Log("<color=cyan>EtaMonitor >> ��ʱ����������</color>");
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
