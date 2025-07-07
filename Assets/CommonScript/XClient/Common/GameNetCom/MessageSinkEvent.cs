using System.Collections.Generic;
using System;
using UnityEngine;

namespace XClient.Common
{
    /// <summary>
    /// 网络消息事件
    /// </summary>
    public interface OnMessageSink
    {
        void OnMessage(gamepol.TCSMessage msg);
    }

    public interface IMessageSinkEvent
    {
    }

    ///事件管理
    //MessageSink 做成对象池
    public class MessageSinkEvent : IMessageSinkEvent
    {
        public uint nMsgID;
        Dictionary<OnMessageSink, string> m_msgSinkDict;
        List<OnMessageSink> m_msgSinkList;

        Dictionary<OnMessageAction, string> m_msgActionDict;
        List<OnMessageAction> m_msgActionList;

        private INetMonitor monitorSink;
        private readonly string prefixDesc = "C#_";
        private float startCallTime = 0;

#if UNITY_EDITOR
        Dictionary<string, uint> m_msgCallCountRecd = new Dictionary<string, uint>();
#endif

        public MessageSinkEvent(uint nMsgID, object context)
        {
            monitorSink = context as INetMonitor;

            this.nMsgID = nMsgID;
            m_msgSinkList = new List<OnMessageSink>();
            m_msgSinkDict = new Dictionary<OnMessageSink, string>();

            m_msgActionDict = new Dictionary<OnMessageAction, string>();
            m_msgActionList = new List<OnMessageAction>();
        }

        public void Reset()
        {
            Clear();
        }

        public void Clear()
        {
            m_msgSinkList.Clear();
            m_msgSinkDict.Clear();
            m_msgActionDict.Clear();
            m_msgActionList.Clear();
        }
  
        public void AddSink(OnMessageSink sink, string desc)
        {
            if (string.IsNullOrEmpty(desc))
            {
                Debug.LogError("添加消息处理失败，原因：desc==null");
                return;
            }
            if (!m_msgSinkDict.ContainsKey(sink))
            {
                m_msgSinkDict.Add(sink, desc);
                m_msgSinkList.Add(sink);
            }
        }
        public void RemoveSink(OnMessageSink sink)
        {
            if (m_msgSinkDict.ContainsKey(sink))
            {
                m_msgSinkDict.Remove(sink);
                m_msgSinkList.Remove(sink);
            }
        }

        public void AddSink(OnMessageAction action, string desc)
        {
            if (string.IsNullOrEmpty(desc))
            {
                Debug.LogError("添加消息处理失败，原因：desc==null");
                return;
            }
            if (!m_msgActionDict.ContainsKey(action))
            {
                m_msgActionDict.Add(action, desc);
                m_msgActionList.Add(action);
            }
        }
        public void RemoveSink(OnMessageAction action)
        {
            if (!m_msgActionDict.ContainsKey(action))
            {
                m_msgActionDict.Remove(action);
                m_msgActionList.Remove(action);
            }
        }

        /// <summary>
        /// 获取网络消息的注册情况
        /// </summary>
        /// <param name="dict"></param>
        public void GetNetNodeInfo(ref Dictionary<string, int> dict)
        {
            int count = m_msgSinkList.Count;
            for (int i = 0; i < count; i++)
            {
                string desc = GetMsgDesc(m_msgSinkList[i]); 
                string key = $"sink网络消息{nMsgID}_{desc}" ;
                if (dict.ContainsKey(key))
                {
                    dict[key] = dict[key] + 1;
                }
                else
                {
                    dict.Add(key, 1);
                }
            }


             count = m_msgActionList.Count;
            for (int i = 0; i < count; i++)
            {
                string desc = GetMsgDesc(m_msgActionList[i]); 
                string key = $"action网络消息{nMsgID}_desc";
                if (dict.ContainsKey(key))
                {
                    dict[key] = dict[key] + 1;
                }
                else
                {
                    dict.Add(key, 1);
                }
            }
        }

        /// <summary>
        /// 获取网络消息调用次数
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,uint> GetCallInfoCount()
        {
#if UNITY_EDITOR
            return m_msgCallCountRecd;
#endif
            return null;
        }

        string GetMsgDesc(OnMessageSink sink)
        {
            string desc = "";
            if (!m_msgSinkDict.TryGetValue(sink, out desc) || desc == null || desc == "")
            {
                desc = sink.ToString();
            }
            return desc;
        }



        string GetMsgDesc(OnMessageAction action)
        {
            string desc = "";
            if (!m_msgActionDict.TryGetValue(action, out desc) || desc == null || desc == "")
            {
                desc = action.ToString();
            }
            return desc;
        }


        public void Fire(uint nMsgID, gamepol.TCSMessage msg)
        {
            int count = m_msgSinkList.Count;
            OnMessageSink sink = null;
            for (int i = 0; i < count; i++)
            {
                sink = m_msgSinkList[i];
                if (sink != null)
                {
                    try
                    {
#if UNITY_EDITOR  //添加监控网络消息回调次数
                        string key = $"sink网络消息{nMsgID}_{GetMsgDesc(sink)}";
                        uint callCount = 1;
                        if(m_msgCallCountRecd.TryGetValue(key,out callCount))
                        {
                            callCount++;
                            m_msgCallCountRecd[key] = callCount;
                        }
                        else
                        {
                            m_msgCallCountRecd.Add(key, 1);
                        }
                        
#endif
                        if (monitorSink != null)
                            startCallTime = monitorSink.GetTime();
                        sink.OnMessage(msg);
                        if (monitorSink != null)
                        {
                            float endCallTime = monitorSink.GetTime();
                            monitorSink.OnReport((int)nMsgID, $"{prefixDesc}{m_msgSinkDict[sink]}", endCallTime - startCallTime);
                        }
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("消息处理异常，来源：" + GetMsgDesc(sink) + "，错误信息：" +
                          e.ToString());
                    }
                }
            }

            count = m_msgActionList.Count;
            for (int i = 0; i < m_msgActionList.Count; i++)
            {
                OnMessageAction action = m_msgActionList[i];
                try
                {
                    if (action != null)
                    {
#if UNITY_EDITOR  //添加监控网络消息回调次数
                        string key = $"action网络消息{nMsgID}_{GetMsgDesc(action)}";
                        uint callCount = 1;
                        if (m_msgCallCountRecd.TryGetValue(key, out callCount))
                        {
                            callCount++;
                            m_msgCallCountRecd[key] = callCount;
                        }
                        else
                        {
                            m_msgCallCountRecd.Add(key, 1);
                        }

#endif

                        if (monitorSink != null)
                            startCallTime = Time.realtimeSinceStartup;
                        action(msg);
                        if (monitorSink != null)
                        {
                            float endCallTime = Time.realtimeSinceStartup;
                            monitorSink.OnReport((int)nMsgID, $"{prefixDesc}{m_msgActionDict[action]}", endCallTime - startCallTime);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("消息处理异常，来源：" + m_msgActionDict[action] + "，错误信息：" +
                        e.ToString());
                }
            }

        }
    }



}
