using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XGame;
using XGame.FrameUpdate;
using XGame.Poolable;
using XGame.Timer;
using Object = UnityEngine.Object;

namespace XClient.Common
{
    interface ICountdownInfo
    {
        int GetID();
        void SetID(int idValue);
        bool HasInstance();
        int GetInstanceID();
        void SetInstanceID(int idValue);
        void OnRemove();
        void SetParam(params object[] param);
        bool Update(float curTime);
        void SetLeftTime(float leftTime);
        void SetAct(Action<float> updateAct, Action endAct = null);
    }

    class CountdownInfo : LitePoolableObject, ICountdownInfo
    {
        protected int id = 0;

        public int GetID()
        {
            return id;
        }

        public void SetID(int idVal)
        {
            id = idVal;
        }

        protected bool hasIntance = false;

        public bool HasInstance()
        {
            return hasIntance;
        }

        protected int instanceID = 0;

        public int GetInstanceID()
        {
            return instanceID;
        }

        public void SetInstanceID(int idVal)
        {
            this.instanceID = idVal;
            hasIntance = true;
        }

        public virtual void OnRemove()
        {
            LitePoolableObject.Recycle(this);
        }

        private float _endTime;
        private Action<float> _updateAct;
        private Action _updateEndAct;

        public virtual void SetParam(params object[] param)
        {
        }

        /// <summary>
        /// 倒计时tick函数 返回值true为倒计时结束
        /// </summary>
        /// <param name="curTime"></param>
        /// <returns></returns>
        public bool Update(float curTime)
        {
            float leftTime = _endTime - curTime;
            leftTime = leftTime < 0 ? 0 : leftTime;
            OnUpdate(leftTime);
            if (0 == leftTime)
            {
                OnUpdateEnd();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置倒计时接口 调用时自动调用updateAct
        /// </summary>
        /// <param name="leftTime"></param>
        public void SetLeftTime(float leftTime)
        {
            _endTime = Time.realtimeSinceStartup + leftTime;
            OnUpdate(leftTime);
        }

        public void SetAct(Action<float> updateAct, Action endAct = null)
        {
            _updateAct = updateAct;
            _updateEndAct = endAct;
        }

        protected virtual void OnUpdate(float leftTime)
        {
            _updateAct?.Invoke(leftTime);
        }

        protected virtual void OnUpdateEnd()
        {
            _updateEndAct?.Invoke();
        }

        protected override void OnRecycle()
        {
            id = 0;
            hasIntance = false;
            instanceID = 0;
            _updateAct = null;
            _updateEndAct = null;
        }
    }

    class TextCountdownInfo : CountdownInfo
    {
        private TextMeshProUGUI textComp;
        private string _template;

        private bool transSecond = false;
        private bool transMinute = false;
        private bool transHour = false;
        private bool transDay = false;

        public void SetTextComp(TextMeshProUGUI text)
        {
            textComp = text;
        }

        public override void OnRemove()
        {
            LitePoolableObject.Recycle(this);
        }

        public override void SetParam(params object[] param)
        {
            base.SetParam(param);
            _template = (string)param[0];
            CountdownTextFormatter.GetFormatInfo(_template, out transSecond, out transMinute, out transHour,
                out transDay);
        }

        protected override void OnUpdate(float leftTime)
        {
            textComp.text =
                CountdownTextFormatter.FormatTimeStr2(leftTime, _template, transSecond, transMinute, transHour,
                    transDay);
            base.OnUpdate(leftTime);
        }
    }

    static class CountdownTextFormatter
    {
        private static readonly int secondPerMin = 60;
        private static readonly int secondPerHour = 60 * 60;
        private static readonly int secondPerDay = 60 * 60 * 24;

        /// <summary>
        /// 直接根据剩余秒数跟格式决定展示时间
        /// 例1
        /// template = ”#m分#s秒“, leftSecond = 80.1
        /// 返回 1分20秒
        /// 例2
        /// template = ”#m：#s“, leftSecond = 80.1
        /// 返回 1：20
        /// 例3
        /// template = ”#ss“, leftSecond = 80.1
        /// 返回 80s
        /// </summary>
        /// <param name="leftSecond"></param>
        /// <param name="template">
        ///#d表示展示天数
        ///#h表示展示小时
        ///#m表示展示分
        ///#s表示展示秒
        /// </param>
        /// <returns></returns>
        public static string FormatTimeStr(float leftSecond, string template = "#h:#m:#s")
        {
            if (null == template)
            {
                template = "#h:#m:#s";
            }

            bool transSecond = false;
            bool transMinute = false;
            bool transHour = false;
            bool transDay = false;
            GetFormatInfo(template, out transSecond, out transMinute, out transHour, out transDay);
            return FormatTimeStr2(leftSecond, template, transSecond, transMinute, transHour, transDay);
        }

        public static string FormatTimeStr2(float leftSecond, string template = "#h:#m:#s"
            , bool transSecond = true, bool transMinute = false, bool transHour = false
            , bool transDay = false)
        {
            bool onlySecond = true;
            if (null == template)
            {
                template = "#h:#m:#s";
            }

            string str = template;

            if (transDay)
            {
                onlySecond = false;
                int dayNum = Mathf.FloorToInt(leftSecond / secondPerDay);
                leftSecond -= secondPerDay * dayNum;
                str = str.Replace("#d", dayNum.ToString("D2"));
            }

            if (transHour)
            {
                onlySecond = false;
                int hourNum = Mathf.FloorToInt(leftSecond / secondPerHour);
                leftSecond -= secondPerHour * hourNum;
                str = str.Replace("#h", hourNum.ToString("D2"));
            }

            if (transMinute)
            {
                onlySecond = false;
                int minuteNum = Mathf.FloorToInt(leftSecond / secondPerMin);
                leftSecond -= secondPerMin * minuteNum;
                str = str.Replace("#m", minuteNum.ToString("D2"));
            }

            if (transSecond)
            {
                string secondStr = onlySecond && leftSecond < 1 ? leftSecond.ToString("F1") : Mathf.FloorToInt(leftSecond).ToString("D2");
                str = str.Replace("#s", secondStr);
            }

            return str;
        }

        public static void GetFormatInfo(string template, out bool transSecond, out bool transMinute,
            out bool transHour, out bool transDay)
        {
            transSecond = false;
            transMinute = false;
            transHour = false;
            transDay = false;
            bool findIndex;
            for (var index = 0; index < template.Length - 1; ++index)
            {
                findIndex = false;
                if ('#' == template[index])
                {
                    switch (template[index + 1])
                    {
                        case 's':
                            transSecond = true;
                            findIndex = true;
                            break;
                        case 'm':
                            transMinute = true;
                            findIndex = true;
                            break;
                        case 'h':
                            transHour = true;
                            findIndex = true;
                            break;
                        case 'd':
                            transDay = true;
                            findIndex = true;
                            break;
                    }
                }

                if (findIndex)
                {
                    index++;
                }
            }
        }
    }

    public class CountdownManager
    {
        private static CountdownManager _ins = null;

        public static CountdownManager Instance
        {
            get
            {
                if (null == _ins)
                {
                    _ins = new CountdownManager();
                }

                return _ins;
            }
        }

        public static void DestoryIns()
        {
            if (null != _ins)
            {
                _ins.Release();
                _ins = null;
            }
        }

        private CountdownManager()
        {
            Create();
        }

        private int idCnt;

        //所有在跑的countdown
        private Dictionary<int, ICountdownInfo> _dicCountdown;

        //回收的id池子
        private Stack<int> _idPool;

        //待回收的countdown
        private List<ICountdownInfo> _removeList;

        //组件的映射
        private Dictionary<int, int> _dicInstanceCountdownID;

        private void Create()
        {
            idCnt = 0;
            if (null == _dicCountdown)
                _dicCountdown = new Dictionary<int, ICountdownInfo>();
            if (null == _removeList)
                _removeList = new List<ICountdownInfo>();
            if (null == _idPool)
                _idPool = new Stack<int>();
            if (null == _dicInstanceCountdownID)
                _dicInstanceCountdownID = new Dictionary<int, int>();

            XGameComs.Get<IFrameUpdateManager>()?.RegUpdateCallback(Update, "BulletManager");
        }

        private void Release()
        {
            DestroyAllCountdown();
            idCnt = 0;
            _dicCountdown = null;
            _removeList = null;
            _idPool.Clear();
            _idPool = null;
            _dicInstanceCountdownID = null;
            XGameComs.Get<IFrameUpdateManager>()?.UnregUpdateCallback(Update);
        }


        /// <summary>
        /// 倒计时
        /// 例1
        /// template = ”#m分#s秒“, leftTime = 80.1
        /// 展示 1分20秒
        /// 例2
        /// template = ”#m：#s“, leftTime = 80.1
        /// 展示 1：20
        /// 例3
        /// template = ”#ss“, leftTime = 80.1
        /// 展示 80s
        /// </summary>
        /// <param name="textComp"></param>
        /// <param name="leftTime"></param>
        /// <param name="leftSecond"></param>
        /// <param name="template">
        ///#d表示展示天数
        ///#h表示展示小时
        ///#m表示展示分
        ///#s表示展示秒
        /// <param name="updateAct"></param>
        /// <param name="endAct"></param>
        public void AddCountdownText(TextMeshProUGUI textComp, float leftTime, string strTemplate = null,
            Action<float> updateAct = null,
            Action endAct = null)
        {
            int compInsID = textComp.GetInstanceID();
            TextCountdownInfo countdownInfo;
            if (_dicInstanceCountdownID.ContainsKey(compInsID))
            {
                var InfoID = _dicInstanceCountdownID[compInsID];
                countdownInfo = _dicCountdown[InfoID] as TextCountdownInfo;
            }
            else
            {
                countdownInfo = SpawnCountdown<TextCountdownInfo>();
                countdownInfo.SetTextComp(textComp);
                countdownInfo.SetInstanceID(compInsID);
                _dicInstanceCountdownID.Add(compInsID, countdownInfo.GetID());
            }

            SetCountdownInfo(countdownInfo, leftTime, updateAct, endAct, strTemplate);
        }

        public void RemoveCountdown<T>(T textComp) where T : Object
        {
            int compInsID = textComp.GetInstanceID();
            if (_dicInstanceCountdownID.TryGetValue(compInsID, out int infoID))
            {
                _dicInstanceCountdownID.Remove(compInsID);
                RemoveCountdown(infoID);
            }
        }

        /// <summary>
        /// 添加倒计时 返回值是倒计时的管理ID
        /// </summary>
        /// <param name="leftTime">倒计时剩余时间</param>
        /// <param name="updateAct">倒计时更新回调</param>
        /// <param name="endAct">倒计时结束回调</param>
        /// <returns></returns>
        public int AddCountdown(float leftTime, Action<float> updateAct, Action endAct = null)
        {
            var countdonwInfo = SpawnCountdown<CountdownInfo>();
            SetCountdownInfo(countdonwInfo, leftTime, updateAct, endAct);
            return countdonwInfo.GetID();
        }

        private void SetCountdownInfo(CountdownInfo countdonwInfo, float leftTime,
            Action<float> updateAct, Action endAct = null, params object[] param)
        {
            countdonwInfo.SetParam(param);
            countdonwInfo.SetAct(updateAct, endAct);
            countdonwInfo.SetLeftTime(leftTime);
        }

        /// <summary>
        /// 重新设置倒计时时间
        /// </summary>
        /// <param name="countdownID">倒计时的管理ID</param>
        /// <param name="leftTime">倒计时剩余时间</param>
        /// <returns></returns>
        public bool SetCountdownTime(int countdownID, float leftTime)
        {
            if (_dicCountdown.TryGetValue(countdownID, out ICountdownInfo countdownInfo))
            {
                countdownInfo.SetLeftTime(leftTime);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除倒计时
        /// </summary>
        /// <param name="countdownID">倒计时的管理ID</param>
        public void RemoveCountdown(int countdownID)
        {
            if (_dicCountdown.TryGetValue(countdownID, out ICountdownInfo countdownInfo))
            {
                RecycleCountdown(countdownInfo);
            }
        }

        private void DestroyAllCountdown()
        {
            foreach (var item in _dicCountdown)
            {
                _removeList.Add(item.Value);
            }

            int removeCount = _removeList.Count;
            for (var index = 0; index < removeCount; ++index)
            {
                RecycleCountdown(_dicCountdown[index]);
            }

            _dicCountdown.Clear();
            _removeList.Clear();
            _dicInstanceCountdownID.Clear();
        }

        private T SpawnCountdown<T>() where T : CountdownInfo, new()
        {
            int newID = 0;
            if (_idPool.Count > 0)
            {
                newID = _idPool.Pop();
            }
            else
            {
                newID = ++idCnt;
            }

            var newCountdown = LitePoolableObject.Instantiate<T>();
            newCountdown.SetID(newID);
            _dicCountdown.Add(newID, newCountdown);
            return newCountdown;
        }

        private void RecycleCountdown<T>(T countdownInfo) where T : ICountdownInfo
        {
            int recycleID = countdownInfo.GetID();
            _idPool.Push(recycleID);
            _dicCountdown.Remove(recycleID);
            if (countdownInfo.HasInstance())
            {
                _dicInstanceCountdownID.Remove(countdownInfo.GetInstanceID());
            }

            countdownInfo.OnRemove();
        }

        private void Update()
        {
            float curTime = Time.realtimeSinceStartup;
            foreach (var item in _dicCountdown)
            {
                var countdown = item.Value;
                if (countdown.Update(curTime))
                {
                    _removeList.Add(countdown);
                }
            }

            int removeCount = _removeList.Count;
            for (var index = 0; index < removeCount; ++index)
            {
                RecycleCountdown(_removeList[index]);
            }

            _removeList.Clear();
        }
    }
}