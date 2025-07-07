/*******************************************************************
** 文件名:	SimpleAnimatioEventHandler.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2020.10.21
** 版  本:	1.0
** 描  述:	参考：
** 应  用:   动画事件处理组件

**************************** 修改记录 ********************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using UnityEngine.Events;
using XGame.Attr;
using XGame.NewAnimator;

namespace XGame.AssetScript.Animation
{
    public class SimpleAnimatioEventHandler : MonoBehaviour, IAnimationEventSink
    {
        /// <summary>
        /// 关联的Animator对象
        /// </summary>
        [AutoBind(typeof(Animator))]
        [SerializeField]
        private Animator m_Animator;

        /// <summary>
        /// 事件侦听器
        /// </summary>
        public class AnimationFrameEvent : UnityEvent<int, string, float> { }
        public AnimationFrameEvent onAnimationFrameEvent = new AnimationFrameEvent();

        /// <summary>
        /// 动画开始事件
        /// </summary>
        public class AnimationStartEvent : UnityEvent<int> { }
        public AnimationStartEvent onAnimationStartEvent = new AnimationStartEvent();

        /// <summary>
        /// 动画完成事件
        /// </summary>
        public class AnimationFinishEvent : UnityEvent<int, int> { }
        public AnimationFinishEvent onAnimationFinishEvent = new AnimationFinishEvent();

        /// <summary>
        /// 动画结束事件
        /// </summary>
        public class AnimationEndEvent : UnityEvent<int> { }
        public AnimationEndEvent onAnimationEndEvent = new AnimationEndEvent();

        private void Awake()
        {
            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            //Debug.Log($"[Animator][SimpleAnimatioEventHandler] 注册事件回调， name={name}");
            AnimationEventManager.Instance.AddSink(m_Animator, this);
        }

        private void OnDisable()
        {
            //Debug.Log($"[Animator][SimpleAnimatioEventHandler] 注销事件回调， name={name}");
            AnimationEventManager.Instance.RemoveSink(m_Animator, this);
        }

        public void OnResetAnimator()
        {
        }

        public void OnAnimationEvent(AnimationEvent e)
        {
            onAnimationFrameEvent?.Invoke(e.intParameter, e.stringParameter, e.floatParameter);
        }

        public void OnAnimationStart(AnimatorStateInfo stateInfo, int layerIndex)
        {
            onAnimationStartEvent.Invoke(stateInfo.shortNameHash);
        }

        public void OnAnimationFinish(AnimatorStateInfo stateInfo, int layerIndex, int finishCount)
        {
            //Debug.Log($"[Animator][SimpleAnimatioEventHandler] 完成事件回调：{m_Animator.name}-{m_Animator.GetInstanceID()}, state={stateInfo.shortNameHash}, layerIndex={layerIndex},  finishCount={finishCount}");
            onAnimationFinishEvent.Invoke(stateInfo.shortNameHash, finishCount);
        }

        public void OnAnimationEnd(AnimatorStateInfo stateInfo, int layerIndex)
        {
            onAnimationEndEvent.Invoke(stateInfo.shortNameHash);
        }

        public void OnAnimationBreak()
        {
        }
    }
}
