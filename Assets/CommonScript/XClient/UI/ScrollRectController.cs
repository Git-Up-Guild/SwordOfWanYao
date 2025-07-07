/*******************************************************************
** 文件名:  ScrollRectController.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2020/12/11
** 版  本:	1.0 
** 描  述:  ScrollRect的停靠控制类
** 应  用: 

**************************** 修改记录 ******************************
** 修改人:    
** 日  期:    
** 描  述:    
********************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace Client.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectController : MonoBehaviour, IEndDragHandler, IDragHandler, IBeginDragHandler
    {
        /// <summary>
        /// 滚动模式
        /// </summary>
        public enum ScrollMode
        {
            Horizontal,
            Vertical,
        }

        //目标
        public ScrollRect target { get; private set; }

        //滚动模式
        public ScrollMode scrollMode = ScrollMode.Horizontal;

        //结束位置列表
        public List<float> endPositions = new List<float>();

        //动画滚动速度
        [Range(0.1f, 10.0f)]
        public float posAnimSpeed = 0.4f;

        //边界限制
        [Range(0f, 1f)]
        public float boundaryLimit = 0.05f;

        //切换页面的阈值
        [Range(0f, 1f)]
        public float changeThreshold = 0.33f;

        //滚动到目标位置最小时间
        [Range(0.01f, 10f)]
        public float minPosAnimTime = 0.2f;

        //水平位置滚动值
        private List<float> m_HoriPosScrollVals = new List<float>();

        //垂直位置滚动值
        private List<float> m_VertPosScrollVals = new List<float>();

        //要滚动下一屏幕的值
        private float m_ScrollValueThresholdValue;

        //当前位置的索引
        [SerializeField]
        private int m_CurPosIndex = 0;

        //是否初始化过了
        private bool m_Inited = false;

        //是否处于检查中
        private bool m_IsPosChecking = false;

        //检查的持续时间
        [SerializeField]
        private float m_CheckDuration = 0.3f;

        //开始检查的时间
        private float m_BeginCheckTick = 0f;

        //上次检查的滚动值
        private float m_LastCheckScrollValue = 0f;

        //原始的加速率
        [SerializeField]
        private float m_DecelerationRate = 0f;

        //是否播放动画中
        private bool m_IsPosAnimPlaying = false;

        //动画开始时间
        private float m_PosAnimStartTick = 0f;

        //滚动需要好时间
        private float m_PosAnimDuration = 1.0f;

#if UNITY_EDITOR
        [Header("刷新[编辑器下有效]")]
        public bool refresh = false;
#endif

        private void Awake()
        {
            target = GetComponent<ScrollRect>();
            target.decelerationRate = 0f;
        }

        private void OnTransformParentChanged()
        {
            Init();
        }

        private void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            if (endPositions.Count == 0)
                return;

            var contentTrans = target.content;
            var contentW = contentTrans.rect.width;
            var contentH = contentTrans.rect.height;

            CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
            Vector2 resolution = scaler.referenceResolution;

            float scrollW = contentTrans.rect.width - resolution.x;
            float scrollH = contentTrans.rect.height - resolution.y;

            if (scrollMode == ScrollMode.Horizontal)
            {
                float thresholdValue = resolution.x * changeThreshold;
                m_ScrollValueThresholdValue = thresholdValue / (contentW - resolution.x);

                for (var i = 0; i < endPositions.Count; ++i)
                {
                    m_HoriPosScrollVals.Add((endPositions[i] - resolution.x) / scrollW);
                }
            }
            else
            {
                float thresholdValue = resolution.y * changeThreshold;
                m_ScrollValueThresholdValue = thresholdValue / (contentH - resolution.y);

                for (var i = 0; i < endPositions.Count; ++i)
                {
                    m_VertPosScrollVals.Add((endPositions[i] - resolution.y) / scrollH);
                }
            }

            if (scrollMode == ScrollMode.Horizontal)
                target.horizontalNormalizedPosition = currentPosScrollValue;
            else
                target.verticalNormalizedPosition = currentPosScrollValue;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (refresh)
            {
                refresh = false;
                m_Inited = false;
                Init();
            }
#endif
            //更新滚动位置检查
            UpdatePosCheck();

            //更新动画
            UpdatePosAnim();
        }

        private void UpdatePosCheck()
        {
            if (m_IsPosChecking)
            {
                float passTime = Time.time - m_BeginCheckTick;

                //超过滚动阈值，马上进行动画处理
                if (Mathf.Abs(currentScrollValue - currentPosScrollValue) > m_ScrollValueThresholdValue)
                {
                    //Debug.Log("滚动值超过阈值，进行动画播放！");
                    OnPosCheckFinish();
                }
                //检查时间到了，马上进行动画播放
                else if (Time.time - m_BeginCheckTick > m_CheckDuration)
                {
                    //Debug.Log("超过检查时间，进行动画播放！");
                    OnPosCheckFinish();
                }
                //滚不动了，马上进行动画播放
                else if (passTime > 0.05f && Mathf.Abs(currentScrollValue - m_LastCheckScrollValue) < 0.002f)
                {
                    //Debug.Log("滚动接近停止，进行动画播放！");
                    OnPosCheckFinish();
                }

                m_LastCheckScrollValue = currentScrollValue;
            }
        }

        private void StartPosCheck()
        {
            m_IsPosChecking = true;
            m_BeginCheckTick = Time.time;
            m_LastCheckScrollValue = currentScrollValue;
        }

        private void OnPosCheckFinish(bool bRecalcPosIndex = true)
        {
            m_IsPosChecking = false;
            target.decelerationRate = 0f;
            PlayPosAnim(bRecalcPosIndex);
        }

        public void OnDestroy()
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Init();

            if (endPositions.Count == 0)
                return;

            StartPosCheck();
        }

        private int posCount
        {
            get
            {
                if (scrollMode == ScrollMode.Horizontal)
                    return m_HoriPosScrollVals.Count;
                else
                    return m_VertPosScrollVals.Count;
            }
        }

        private float currentPosScrollValue
        {
            get
            {
                if (scrollMode == ScrollMode.Horizontal)
                    return m_HoriPosScrollVals[m_CurPosIndex];
                else
                    return m_VertPosScrollVals[m_CurPosIndex];
            }
        }

        private float currentScrollValue
        {
            get
            {
                if (scrollMode == ScrollMode.Horizontal)
                    return target.horizontalNormalizedPosition;
                else
                    return target.verticalNormalizedPosition;
            }
        }

        private int CalcNewPosIndex()
        {
            //计算偏移值
            float delta = currentScrollValue - currentPosScrollValue;

            //新的位置索引
            int newPosIndex = m_CurPosIndex;

            //超过了阈值，需要往前滚动
            if (delta < -m_ScrollValueThresholdValue)
            {
                newPosIndex = newPosIndex - 1;
                if (newPosIndex < 0)
                {
                    newPosIndex = 0;
                }
            }
            //超过了阈值，需要往后滚动
            else if (delta > m_ScrollValueThresholdValue)
            {
                newPosIndex = newPosIndex + 1;
                if (newPosIndex >= posCount)
                {
                    newPosIndex = posCount - 1;
                }
            }

            return newPosIndex;
        }

        //播放位置回归动画
        private void PlayPosAnim(bool bRecalcPosIndex = true)
        {
            //从新获取位置索引
            if(bRecalcPosIndex)
                m_CurPosIndex = CalcNewPosIndex();

            float delta = currentScrollValue - currentPosScrollValue;
            var duration = Mathf.Abs(delta) / posAnimSpeed;
            m_PosAnimDuration = Mathf.Clamp(duration, minPosAnimTime, 10f);
            m_PosAnimStartTick = Time.time;
            m_IsPosAnimPlaying = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            target.verticalNormalizedPosition = Mathf.Clamp(target.verticalNormalizedPosition, -boundaryLimit, 1f + boundaryLimit);
            target.horizontalNormalizedPosition = Mathf.Clamp(target.horizontalNormalizedPosition, -boundaryLimit, 1f + boundaryLimit);
        }

        /// <summary>
        /// 获取当前的滚动位置索引
        /// </summary>
        /// <returns></returns>
        public int GetCurPosIndex()
        {
            float currentScrollValue = scrollMode == ScrollMode.Horizontal ? target.horizontalNormalizedPosition : target.verticalNormalizedPosition;
            var posScrollVals = scrollMode == ScrollMode.Horizontal ? m_HoriPosScrollVals : m_VertPosScrollVals;
            int posCount = posScrollVals.Count;
            
            for(var i = posCount - 1; i >= 0; i--)
            {
                if (currentScrollValue >= posScrollVals[i] - 0.0001f)  //这里减去这个数是为了防止误差
                    return i;
            }

            return 0;
        }

        public void ScrollToIndex(int nCurPosIndex)
        {
            if (m_HoriPosScrollVals.Count <= 0 || m_VertPosScrollVals.Count <= 0)
                Init();
            m_CurPosIndex = nCurPosIndex;

            if (m_CurPosIndex < 0)
                m_CurPosIndex = 0;

            if (scrollMode == ScrollMode.Horizontal)
                m_CurPosIndex = m_CurPosIndex >= m_HoriPosScrollVals.Count ? m_HoriPosScrollVals.Count - 1 : m_CurPosIndex;
            else
                m_CurPosIndex = m_CurPosIndex >= m_VertPosScrollVals.Count ? m_VertPosScrollVals.Count - 1 : m_CurPosIndex;

            OnPosCheckFinish(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_IsPosChecking = false;
            m_IsPosAnimPlaying = false;
            target.decelerationRate = m_DecelerationRate;
        }

        private void UpdatePosAnim()
        {
            if(m_IsPosAnimPlaying)
            {
                float t = 0f;
                float pass = Time.time - m_PosAnimStartTick;
                if(pass > m_PosAnimDuration)
                {
                    m_IsPosAnimPlaying = false;
                    t = 1f;
                }
                else
                {
                    t = pass / m_PosAnimDuration;
                }

                if (scrollMode == ScrollMode.Horizontal)
                    target.horizontalNormalizedPosition = Mathf.Lerp(currentScrollValue, currentPosScrollValue, t);
                else
                    target.verticalNormalizedPosition = Mathf.Lerp(currentScrollValue, currentPosScrollValue, t);
            }
        }
    }

}