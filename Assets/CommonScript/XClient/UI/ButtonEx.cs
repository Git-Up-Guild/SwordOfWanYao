using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XGame.UI
{
    /// <summary>
    /// 按钮事件类型
    /// </summary>
    public enum EnBtnExEventType
    {
        /// <summary>
        /// 长按
        /// </summary>
        LongPress,

        /// <summary>
        /// 连按
        /// </summary>
        MorePress,
    }

    /// <summary>
    /// 按钮执行失败原因
    /// </summary>
    public enum EnBtnExEventFailReason
    {
        /// <summary>
        /// 间隔时间超时
        /// </summary>
        IntervalTimeOut,

        /// <summary>
        /// 总时间超时
        /// </summary>
        MaxTimeOut,

        /// <summary>
        /// 按钮失效
        /// </summary>
        Disable,

        /// <summary>
        /// 指针超出范围
        /// </summary>
        PonterExit,

        /// <summary>
        /// 指针放手
        /// </summary>
        PointerUp,
    }


    public class ButtonEx : Button
    {

        [System.Serializable]
        public class EventContext
        {
            public int id;                      // 句柄
            public bool isChecking;             // 正在执行
            public EnBtnExEventType eventType;       // 事件类型
            public float startTime;             // 起始时间
            public float maxDuration;           // 最大持续时间
            public float interval;              // 间隔时间
            public int totalClickCount;         // 点击次数
            public int curClickCount;           // 当前已点击次数
            public float nextIntervalTime;      // 下一次间隔完成时间
            public BtnExEventCallback callback;      // 成功回调
        }

        /// <summary>
        /// 事件回调
        /// </summary>
        public class BtnExEventCallback
        {
            public uint handle;
            public int id;
            public Action onSuccessCallback;    // 成功回调
            public Action<EnBtnExEventFailReason> onFailCallback;       // 失败回调
        }

        // 手动添加的事件句柄
        private uint MaxHandle = 0;
        // 事件类型 对应数据现场
        public List<EventContext> eventContexList = new List<EventContext>();

        // 额外注册回调列表
        private List<BtnExEventCallback> _extendCallbackList = new List<BtnExEventCallback>();

        public void Update()
        {
            var curTime = Time.time;
            UpdateLongPress(curTime);   // 长按更新
            UpdateMorePress(curTime);   // 连点更新
        }

        protected override void OnDisable()
        {
            DoAllLongPressFail(EnBtnExEventFailReason.Disable);
            DoAllMorePressFail(EnBtnExEventFailReason.Disable);
            base.OnDisable();
        }

        /// <summary>
        /// 遍历正在执行的事件现场
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="action"></param>
        private void ForeachEventContext(EnBtnExEventType eventType, bool isOnlyChecking, Action<EventContext> action)
        {
            int count = eventContexList.Count;
            for (int i = 0; i < count; i++)
            {
                var context = eventContexList[i];
                if (context.eventType != eventType)
                    continue;
                if (!isOnlyChecking || context.isChecking)  // 如果要检测就检测
                {
                    action.Invoke(context);
                }
            }
        }

        /// <summary>
        /// 添加回调，返回句柄
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="onSuccess">成功回调</param>
        /// <param name="onFail">失败回调，参数为失败原因</param>
        public uint AddCallback(int id, Action onSuccess, Action<EnBtnExEventFailReason> onFail)
        {
            var callback = new BtnExEventCallback()
            {
                handle = ++MaxHandle, 
                id = id,
                onSuccessCallback = onSuccess,
                onFailCallback = onFail,
            };
            _extendCallbackList.Add(callback);
            return callback.handle;
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="handle">句柄</param>
        public void RemoveCallback(uint handle)
        {
            int count = _extendCallbackList.Count;
            for (int i = 0; i < count; i++)
            {
                if (_extendCallbackList[i].handle == handle)
                {
                    _extendCallbackList.Remove(_extendCallbackList[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// 遍历所有ID匹配的回调
        /// </summary>
        /// <param name="id"></param>
        /// <param name="action"></param>
        private void ForeachExtendCallbackList(int id, Action<BtnExEventCallback> action)
        {
            int count = _extendCallbackList.Count;
            for (int i = 0; i < count; i++)
            {
                if (_extendCallbackList[i] != null && _extendCallbackList[i].id == id)
                {
                    action(_extendCallbackList[i]);
                }
            }
        }

        /// <summary>
        /// 执行事件完成
        /// </summary>
        /// <param name="id"></param>
        private void DoEventSuccess(int id)
        {
            ForeachExtendCallbackList(id, (callback) =>
            {
                callback.onSuccessCallback?.Invoke();
            });
        }

        /// <summary>
        /// 执行事件失败
        /// </summary>
        /// <param name="id"></param>
        private void DoEventFail(int id, EnBtnExEventFailReason reason)
        {
            ForeachExtendCallbackList(id, (callback) =>
            {
                callback.onFailCallback?.Invoke(reason);
            });
        }


        #region 长按处理
        /// <summary>
        /// 更新长按处理
        /// </summary>
        private void UpdateLongPress(float curTime)
        {
            ForeachEventContext(EnBtnExEventType.LongPress, true, (context) =>
            {
                if (curTime - context.startTime >= context.maxDuration)
                {
                    OnLongPressSuccess(context);
                }
            });
        }

        /// <summary>
        /// 长按成功
        /// </summary>
        /// <param name="context"></param>
        private void OnLongPressSuccess(EventContext context)
        {
            if (context.isChecking)
            {
                //Debug.LogError("长按成功");
                context.isChecking = false;
                DoEventSuccess(context.id);
            }
        }

        /// <summary>
        /// 长按失败
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        private void OnLongPressFail(EventContext context, EnBtnExEventFailReason reason)
        {
            if (context.isChecking)
            {
                //Debug.LogError($"长按失败！reason:{reason}");
                context.isChecking = false;
                DoEventFail(context.id, reason);
            }
        }

        /// <summary>
        /// 执行所有按钮失败
        /// </summary>
        /// <param name="reason"></param>
        private void DoAllLongPressFail(EnBtnExEventFailReason reason)
        {
            ForeachEventContext(EnBtnExEventType.LongPress, true, (context) =>
            {
                OnLongPressFail(context, reason);
            });
        }
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            var curTime = Time.time;
            ForeachEventContext(EnBtnExEventType.LongPress, false, (context) =>
            {
                context.startTime = curTime;
                context.isChecking = true;
            });
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            DoAllLongPressFail(EnBtnExEventFailReason.PonterExit);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            DoAllLongPressFail(EnBtnExEventFailReason.PointerUp);
        }
        #endregion

        #region 连点事件
        private void UpdateMorePress(float curTime)
        {
            ForeachEventContext(EnBtnExEventType.MorePress, true, (context) =>
            {
                if (curTime - context.startTime >= context.maxDuration)
                {
                    DoMorePressFail(context, EnBtnExEventFailReason.MaxTimeOut);
                }
                if (curTime >= context.nextIntervalTime)
                {
                    DoMorePressFail(context, EnBtnExEventFailReason.IntervalTimeOut);
                }
            });

        }

        /// <summary>
        /// 执行事件失败回调
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        private void DoMorePressFail(EventContext context, EnBtnExEventFailReason reason)
        {
            if (context.isChecking)
            {
                DoEventFail(context.id, reason);
                context.isChecking = false;
                context.curClickCount = 0;
            }
        }

        /// <summary>
        /// 执行所有按钮失败
        /// </summary>
        /// <param name="reason"></param>
        private void DoAllMorePressFail(EnBtnExEventFailReason reason)
        {
            ForeachEventContext(EnBtnExEventType.MorePress, true, (context) =>
            {
                DoMorePressFail(context, reason);
            });
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            var curTime = Time.time;
            ForeachEventContext(EnBtnExEventType.MorePress, false, (context) =>
            {
                if (!context.isChecking)
                {
                    context.isChecking = true;
                    context.startTime = curTime;
                    context.curClickCount = 0;
                }
                ++context.curClickCount;
                context.nextIntervalTime = curTime + context.interval;
                if (context.curClickCount >= context.totalClickCount)
                {
                    DoEventSuccess(context.id);
                    context.isChecking = false;
                    context.curClickCount = 0;
                }
            });
        }
        #endregion
    }
}
