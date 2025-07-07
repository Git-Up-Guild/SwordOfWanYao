/*****************************************************
** 文 件 名：NetworkReachabilityMonitor
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 17:04:20
** 内容简述：网络连接状态监控
** 修改记录：
日期	版本	修改人   
*****************************************************/

using System;
using UnityEngine;
using XGame.Utils;

namespace XGame.AssetScript.Net
{
    public class NetworkReachabilityMonitor : MonoBehaviourEX<NetworkReachabilityMonitor>
    {
        /// <summary>
        /// 检查间隔
        /// </summary>
        public float checkInterval = 0.1f;

        /// <summary>
        /// 当前类型
        /// </summary>
        public NetworkReachability currentType;

        /// <summary>
        /// 最后检查的时间
        /// </summary>
        private float m_LastCheckTick;

        /// <summary>
        /// 回调函数
        /// </summary>
        private Action<NetworkReachability> m_Listeners;

        /// <summary>
        /// 增加回调
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(Action<NetworkReachability> listener)
        {
            m_Listeners -= listener;
            m_Listeners += listener;
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(Action<NetworkReachability> listener)
        {
            m_Listeners -= listener;
        }

        private void Start()
        {
            currentType = Application.internetReachability;
        }

        private void Update()
        {
            if(Time.time - m_LastCheckTick > checkInterval)
            {
                m_LastCheckTick = Time.time;

                if(currentType != Application.internetReachability)
                {
                    currentType = Application.internetReachability;

                    //回调
                    m_Listeners?.Invoke(currentType);
                }
            }
        }
    }
}