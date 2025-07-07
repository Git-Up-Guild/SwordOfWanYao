using minigame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XClient.Net
{
    public class TransferMessageSinkEvent 
    {
        public uint nMsgID;
        Dictionary<IGameMessageSink, string> m_msgSinkDict;
        List<IGameMessageSink> m_msgSinkList;


        public TransferMessageSinkEvent(uint nMsgID, object context)
        {
 

            this.nMsgID = nMsgID;
            m_msgSinkList = new List<IGameMessageSink>();
            m_msgSinkDict = new Dictionary<IGameMessageSink, string>();

        
        }

        public void Reset()
        {
            Clear();
        }

        public void Clear()
        {
            m_msgSinkList.Clear();
            m_msgSinkDict.Clear();
        }

        public void AddSink(IGameMessageSink sink, string desc)
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
        public void RemoveSink(IGameMessageSink sink)
        {

            //这里稍后处理一下一边删除一边fire的情况

            if (m_msgSinkDict.ContainsKey(sink))
            {
                m_msgSinkDict.Remove(sink);
                m_msgSinkList.Remove(sink);
            }
        }

        public void Fire(uint nMsgID, TGameMessage msg)
        {
            int count = m_msgSinkList.Count;
            IGameMessageSink sink = null;
            for (int i = 0; i < count; i++)
            {
                sink = m_msgSinkList[i];
                if (sink != null)
                {
                    try
                    {

                        sink.OnGameMessage(msg);
                       
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("消息处理异常，来源：" + GetMsgDesc(sink) + "，错误信息：" +
                          e.ToString());
                    }
                }
            }

          

        }

        string GetMsgDesc(IGameMessageSink action)
        {
            string desc = "";
            if (!m_msgSinkDict.TryGetValue(action, out desc) || desc == null || desc == "")
            {
                desc = action.ToString();
            }
            return desc;
        }


    }

}

