/*****************************************************************
** 文件名:	TextSettingLua.cs
** 版  权:	(C) 深圳冰川网络
** 创建人:  许德纪
** 日  期:	2022/1.17
** 版  本:	1.0
** 描  述:	跳字管理
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame;
using XGame.FrameUpdate;
using XGame.Poolable;

namespace XClient.Scripts.Api
{

    //跳字结构
    public class SkipInfoNode : IPoolable
    {
        public int nID;
        public Text text;
        public TextMeshPro tmp;
        public TextMeshProUGUI tmpugui;
        public float startValue;
        public float endValue;
        public bool bLine;
        public string prefix;
        public string suffix;
        public float startTime = 0;
        public float duration = 0;
        private bool bFinish = false;
        private bool bShowUnit = true;
        private Func<float, string> CustomFormat = null;

        //Keep two decimal places
        private bool bKeepTwoDecimal = false;
        private TweenCallback finishCallback;
        static private StringBuilder m_sbTemp = new StringBuilder();

        public float easeOutQuad(float t, float b, float c, float d)
        {
            // t = t / d - 1;
            //return c * (t * t * t + 1) + b;
            t /= d;
            return -c * (t) * (t - 2) + b;
        }

        public float easeLine(float t, float b, float c, float d)
        {
            return b + t * c / d;
        }

        /*
        public int Play(int nID,Text text, float startValue, float endValue,  bool bLine, string prefix, string suffix,float durationTime, bool showUnit = true, bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {

            return PlayIner(null, text, startValue,  endValue,  bLine,  prefix,  suffix,  durationTime,  showUnit ,  keepTwoDecimal , finishCallback);
        }

        public int Play(TextMeshPro text, float startValue, float endValue, bool bLine, string prefix, string suffix, float durationTime, bool showUnit = true, bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {

            return PlayIner(text, null, startValue, endValue, bLine, prefix, suffix, durationTime, showUnit, keepTwoDecimal, finishCallback);
        }
        */

        public int Play(int nID, TextMeshPro tmp, Text text, TextMeshProUGUI tmpugui, float startValue, float endValue, bool bLine, string prefix, string suffix, float durationTime, bool showUnit, Func<float, string> CustomFormat, bool keepTwoDecimal, TweenCallback finishCallback)
        {
            this.nID = nID;
            this.tmp = tmp;
            this.text = text;
            this.tmpugui = tmpugui;
            this.prefix = prefix;
            this.startValue = startValue;
            this.endValue = endValue;
            this.suffix = suffix;
            this.bLine = bLine;
            startTime = Time.realtimeSinceStartup;
            this.bShowUnit = showUnit;
            bKeepTwoDecimal = keepTwoDecimal;
            this.finishCallback = finishCallback;
            this.CustomFormat = CustomFormat;

            //文字动画时间
            if (durationTime >= 0)
            {
                duration = durationTime;
            }
            else
            {
                duration = 0.04f * Mathf.Abs(endValue - startValue);
                duration = Mathf.Clamp(duration, 0.4f, 1.5f);
            }
            OnSkip(startValue);
            bFinish = false;
            return this.nID;
        }

        public bool Update(float curTime)
        {
            if (bFinish == false)
            {
                float t = curTime - startTime;
                float v = 0;
                if (bLine)
                {
                    v = easeLine(t, startValue, endValue - startValue, duration);
                }
                else
                {
                    v = easeOutQuad(t, startValue, endValue - startValue, duration);
                }
                bool isFinish = false;
                if (startValue > endValue)
                {
                    if (v <= endValue)
                    {
                        isFinish = true;
                        v = endValue;
                    }
                }
                else
                {
                    if (v >= endValue)
                    {
                        isFinish = true;
                        v = endValue;
                    }
                }

                //if(v>= endValue)
                //{
                //    v = endValue;
                //}
                OnSkip(v);
                if (isFinish || t >= duration)
                {
                    OnSkip(endValue);
                    finishCallback?.Invoke();
                    bFinish = true;
                }
            }
            return bFinish;
        }

        public bool Create()
        {
            return true;
        }

        public void Init(object context = null)
        {
        }

        public void Release()
        {
        }

        public void Reset()
        {
        }

        public void OnSkip(float v)
        {
            string text = null;
            m_sbTemp.Clear();
            m_sbTemp.Append(prefix);
            //以前专用规则 不用  by崔卫华
            //if (bShowUnit)
            //{
            //    if (endValue > 10000000)
            //    {
            //        v = v / 10000000;

            //        if (!bKeepTwoDecimal)
            //        {
            //            m_sbTemp.Append(v.ToString("0.0"));

            //        }else
            //        {
            //            m_sbTemp.Append(v.ToString("0.00"));
            //        }

            //        m_sbTemp.Append("M");
            //        m_sbTemp.Append(suffix);
            //        text = m_sbTemp.ToString();

            //    }
            //    else if (endValue > 100000)
            //    {
            //        v = v / 1000;

            //        if(!bKeepTwoDecimal)
            //        {
            //            int curValue = Mathf.FloorToInt(v);
            //            m_sbTemp.Append(curValue.ToString());
            //            //text = prefix + curValue + "K" + suffix;
            //        }else
            //        {
            //            m_sbTemp.Append(v.ToString("0.00"));
            //        }

            //        m_sbTemp.Append("K");
            //        m_sbTemp.Append(suffix);
            //        text = m_sbTemp.ToString();

            //    }
            //}
            //针对修仙专用的滚字
            if (bShowUnit)
            {
                if (CustomFormat != null)
                {
                    text = CustomFormat(v);
                }
                else
                {
                    if (endValue > 100000000)
                    {
                        v = v * 0.00000001f;
                        if (!bKeepTwoDecimal)
                        {
                            m_sbTemp.Append(v.ToString("0.0"));
                        }
                        else
                        {
                            m_sbTemp.Append(v.ToString("0.00"));
                        }
                        m_sbTemp.Append("亿");
                        m_sbTemp.Append(suffix);
                        text = m_sbTemp.ToString();
                    }
                    else if (endValue > 1000000)
                    {
                        v = v * 0.0001f;
                        if (!bKeepTwoDecimal)
                        {
                            int curValue = Mathf.FloorToInt(v);
                            m_sbTemp.Append(curValue.ToString());
                            //text = prefix + curValue + "K" + suffix;
                        }
                        else
                        {
                            m_sbTemp.Append(v.ToString("0.00"));
                        }
                        m_sbTemp.Append("万");
                        m_sbTemp.Append(suffix);
                        text = m_sbTemp.ToString();
                    }
                }
                
            }
            if (null == text)
            {
                int curValue = Mathf.FloorToInt(v);
                m_sbTemp.Append(curValue.ToString());
                m_sbTemp.Append(suffix);
                text = m_sbTemp.ToString();
                //text = prefix + curValue + suffix;
            }
            if (this.tmpugui != null)
            {
                tmpugui.text = text;
            }
            else if (this.tmp != null)
            {
                tmp.text = text;
            }
            else if (this.text != null)
            {
                this.text.text = text;
            }
        }

        public void OnCompelete()
        {
            TextSkipMgr.Instance.Recycle(this);
        }
    }

    public class TextSkipMgr
    {
        private static TextSkipMgr _inst = null;

        //正在运行的 dotween
        private Dictionary<int, SkipInfoNode> m_dicRuns = new Dictionary<int, SkipInfoNode>();
        private List<SkipInfoNode> m_listRecycle = new List<SkipInfoNode>();

        //运动序列
        //private Sequence m_se;

        //是否有订阅帧更新事件
        private bool m_registerFrame = false;
        private string updateDesc = "TextSkipMgrDecs";
        public static TextSkipMgr Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new TextSkipMgr();
                    //_inst.m_se = DOTween.Sequence();
                }
                return _inst;
            }
        }

        // TextMeshPro 的跳字
        public void SkipText(Text text, float startValue, float endValue, bool bLine, string prefix, string suffix, float duration, bool showUnit = true, bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {
            SkipTextIner(null, text, null, startValue, endValue, bLine, prefix, suffix, duration, showUnit, null, keepTwoDecimal, finishCallback);
        }

        // TextMeshPro 的跳字
        public void SkipText(TextMeshPro tmp, float startValue, float endValue, bool bLine, string prefix, string suffix, float duration, bool showUnit = true, bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {
            SkipTextIner(tmp, null, null, startValue, endValue, bLine, prefix, suffix, duration, showUnit, null, keepTwoDecimal, finishCallback);
        }

        public void SkipText(TextMeshProUGUI tmp, float startValue, float endValue, bool bLine, string prefix, string suffix, float duration, bool showUnit = true, bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {
            SkipTextIner(null, null, tmp, startValue, endValue, bLine, prefix, suffix, duration, showUnit, null, keepTwoDecimal, finishCallback);
        }

        // TextMeshProUGUI 的跳字 支持定义不同单位形式
        public void SkipText(TextMeshProUGUI tmp, float startValue, float endValue, bool bLine, string prefix, string suffix, float duration, Func<float, string> CustomFormat, bool showUnit, bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {
            SkipTextIner(null, null, tmp, startValue, endValue, bLine, prefix, suffix, duration, showUnit, CustomFormat, keepTwoDecimal, finishCallback);
        }

        //跳字内部实现
        private void SkipTextIner(TextMeshPro tmp, Text text, TextMeshProUGUI tmpugui, float startValue, float endValue, bool bLine, string prefix, string suffix, float duration, bool showUnit = true, Func<float, string> CustomFormat = null
                                , bool keepTwoDecimal = false, TweenCallback finishCallback = null)
        {
            int nID = -1;
            if (tmpugui != null)
            {
                nID = tmpugui.GetHashCode();
            }
            else if (tmp != null)
            {
                nID = tmp.GetHashCode();
            }
            else
            {
                nID = text.GetHashCode();
            }
            //int nID = tmp != null ? tmp.GetHashCode() : text.GetHashCode();
            SkipInfoNode node = null;
            if (m_dicRuns.ContainsKey(nID))
            {
                node = m_dicRuns[nID];
            }
            else
            {
                node = AlocNode();
                m_dicRuns.Add(nID, node);
            }
            if (null != node)
            {
                node.Play(nID, tmp, text, tmpugui, startValue, endValue, bLine, prefix, suffix, duration, showUnit, CustomFormat, keepTwoDecimal, finishCallback);
            }

            //订阅帧更新事件
            if (m_registerFrame == false)
            {
                m_registerFrame = true;
                //XGameComs.Get<IFrameUpdateManager>().UnregLateUpdateCallback(LateUpdate);
                XGameComs.Get<IFrameUpdateManager>().RegLateUpdateCallback(LateUpdate, updateDesc);
            }
        }

        public SkipInfoNode AlocNode()
        {
            IItemPoolManager itemPools = XGame.XGameComs.Get<IItemPoolManager>();
            return itemPools.PopObjectItem<SkipInfoNode>();
        }

        public void Recycle(SkipInfoNode node)
        {
            if (m_dicRuns.ContainsKey(node.nID))
            {
                m_dicRuns.Remove(node.nID);
            }
            IItemPoolManager itemPools = XGame.XGameComs.Get<IItemPoolManager>();
            itemPools.PushObjectItem(node);
        }

        void LateUpdate()
        {
            m_listRecycle.Clear();
            float curTime = Time.realtimeSinceStartup;
            foreach (int key in m_dicRuns.Keys)
            {
                SkipInfoNode node = m_dicRuns[key];
                //已经被释放了，放回对象池
                if (node.Update(curTime)) //|| node.t.IsPlaying()==false)
                {
                    m_listRecycle.Add(node);
                }
            }
            for (int i = 0; i < m_listRecycle.Count; ++i)
            {
                Recycle(m_listRecycle[i]);
            }
            m_listRecycle.Clear();

            //退订帧更新事件
            if (m_dicRuns.Count == 0)
            {
                m_registerFrame = false;
                XGameComs.Get<IFrameUpdateManager>().UnregLateUpdateCallback(LateUpdate);
            }
        }
    }

}