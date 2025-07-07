// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 15:55

using System;
using System.Collections.Generic;
using DG.Tweening.Core;
using UnityEngine;
#if true // UI_MARKER
using UnityEngine.UI;
#endif
#if false // TEXTMESHPRO_MARKER
using TMPro;
#endif

#pragma warning disable 1591
namespace DG.Tweening
{
    /// <summary>
    /// Attach this to a GameObject to create a tween
    /// </summary>
    [AddComponentMenu("DOTween/DOTween AnimationEx")]
    public class DOTweenAnimationEx : DOTweenAnimation
    {


        //完成回调
        private Action<DOTweenAnimationEx> m_compeleteCallback;

        /*
        private Vector3 localPos;
        private Vector3 scale;
        private Vector2 offsetMax;
        private Vector2 offsetMin;
        private CanvasGroup cg = null;
        private bool bFirstTime = true;
        */
        DOTweenRecover cmpRecover = null;


        /// <summary>
        /// 检查Tween是否有效，如果无效就创建
        /// </summary>
        public void CheckNandCreateTween()
        {
            

            if (!_tweenCreated || tween == null || !tween.IsActive())
            {
                CreateTween();
                _tweenCreated = true;
            }
        }

        void Awake()
        {
            //需要记录的才添加脚本
            //if(recover)
            {
                cmpRecover = this.gameObject.GetComponent<DOTweenRecover>();
                if (null == cmpRecover)
                {
                    cmpRecover = this.gameObject.AddComponent<DOTweenRecover>();
                }
            }
           


        }

        void Start()
        {
        }

        private void OnEnable()
        {
            
            if(recover&& cmpRecover!=null)
            {
                cmpRecover.RecoverState();
            }
           

            if (isActive && isValid && autoPlay)
                CheckNandCreateTween();
        }


        private void OnDisable()
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill(true);
                tween = null;
            }
        }



        void OnDestroy()
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill();
                tween = null;
            }

        }

        public void OnManagerPlayTween()
        {
            CheckNandCreateTween();
        }

        public override void DOPlay()
        {
            CheckNandCreateTween();
            base.DOPlay();
        }

        public void OnManagerPlayBackwardsTween()
        {
     
        }

        public void OnManagerPlayForwardTween()
        {
        }

        public void OnManagerPauseTween()
        {
        }

        public void OnManagerTogglePauseTween()
        {
        }

        public void OnManagerRewindTween()
        {
            if (tween != null && tween.IsActive())
                tween.Rewind();
        }
        
        public void OnManagerRestartTween(bool fromHere)
        {
            CheckNandCreateTween();

            if (fromHere && isRelative) 
                ReEvaluateRelativeTween();
        }

        public void OnManagerCompleteTween()
        {
        }

        public void OnManagerKillTween()
        {
            if(tween!=null)
            {
                tween.Kill(true);
                tween = null;
            }
           
            _tweenCreated = false;
        }

        public void SetCompleteCallback(Action<DOTweenAnimationEx> callback)
        {
            //RecordState();
            m_compeleteCallback = callback;
            if(tween!=null)
            {
                tween.onComplete = OnComplete;
            }

           
        }

        //完成回调
        public void OnComplete()
        {
           if(null!= m_compeleteCallback)
            {
                m_compeleteCallback.Invoke(this);
                m_compeleteCallback = null;
            }

            //恢复状态
            if (recover && cmpRecover != null)
            {
                cmpRecover.RecoverState();
            }
        }

        public void SetTemplateID(int id)
        {
            if (0 == templateID)
                return;
            templateID = id;
        }

        /*
        private void RecordState()
        {
           
            if (recover)
            {
#if UNITY_EDITOR
            //    Debug.LogError("记录位置：RecordState: " + this.gameObject.name);
#endif

                RectTransform rectTrans = this.transform as RectTransform;
                if (null != rectTrans)
                {
                    offsetMax = rectTrans.offsetMax;
                    offsetMin = rectTrans.offsetMin;
                }

                localPos = this.transform.localPosition;
                scale = this.transform.localScale;
                cg = this.GetComponent<CanvasGroup>();


            }
        }

        private void RecoverState()
        {
            if (recover)
            {

#if UNITY_EDITOR
          //      Debug.LogError("恢复位置：RecoverState: " + this.gameObject.name);
#endif

                RectTransform rectTrans = this.transform as RectTransform;
                if (null != rectTrans)
                {
                    rectTrans.offsetMax = offsetMax;
                    rectTrans.offsetMin = offsetMin;
                }

                this.transform.localPosition = localPos;
                this.transform.localScale = scale;

              


                if (cg)
                {
                    cg.alpha = 1.0f;
                }
            }
        }
        */
    }

}
