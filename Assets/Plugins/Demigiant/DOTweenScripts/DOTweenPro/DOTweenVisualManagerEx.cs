using System.Collections;
using UnityEngine;

namespace DG.Tweening
{
    public class DOTweenVisualManagerEx : MonoBehaviour
    {
        public enum EnEnableAction
        {
            None,

            //重新开始
            Restart,

            //继续播放，如果动画不存在，则什么都不做
            Resum,

            //继续播放，如果动画不存在，则开始播放
            ResumOrStart,
        }

        public enum EnDisableAction
        {
            None,

            //杀死并且完成
            KillAndComplete,

            //杀死，没有完成
            Kill,

            //停止
            Pause,

            //回到出发点
            Rewind,
        }

        [HideInInspector]
        public DOTweenAnimationEx[] tweenAnims;

        public EnEnableAction enableAction = EnEnableAction.Restart;
        public EnDisableAction disableAction = EnDisableAction.KillAndComplete;

        private void Awake()
        {
            tweenAnims = GetComponents<DOTweenAnimationEx>();
        }

        private void OnEnable()
        {
            switch(enableAction)
            {
                case EnEnableAction.Restart:
                    {
                        DORestart();
                    }
                    break;
                case EnEnableAction.Resum:
                    {
                        DOPlay();
                    }
                    break;
                case EnEnableAction.ResumOrStart:
                    {
                        foreach (var anim in tweenAnims)
                        {
                            anim.CheckNandCreateTween();
                        }
                        DOPlay();
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnDisable()
        {
            switch (disableAction)
            {
                case EnDisableAction.Kill:
                    DOKill();
                    break;
                case EnDisableAction.KillAndComplete:
                    DOComplete();
                    DOKill();
                    break;
                case EnDisableAction.Pause:
                    DOPause();
                    break;
                case EnDisableAction.Rewind:
                    DORewind();
                    break;
                default:
                    break;
            }
        }

        public void DOPlay()
        {
            foreach (var anim in tweenAnims)
                anim.OnManagerPlayTween();
            DOTween.Play(this.gameObject);
        }

        public void DOPlayBackwards()
        {
            foreach (var anim in tweenAnims)
                anim.OnManagerPlayBackwardsTween();
            DOTween.PlayBackwards(this.gameObject);
        }

        public void DOPlayForward()
        {
            foreach (var anim in tweenAnims)
                anim.OnManagerPlayForwardTween();
            DOTween.PlayForward(this.gameObject);
        }

        public void DOPause()
        {
            foreach (var anim in tweenAnims)
                anim.OnManagerPauseTween();
            DOTween.Pause(this.gameObject);
        }

        public void DOTogglePause()
        {
            foreach (var anim in tweenAnims)
                anim.OnManagerTogglePauseTween();
            DOTween.TogglePause(this.gameObject);
        }

        public void DORewind()
        {
            foreach(var anim in tweenAnims)
                anim.OnManagerRewindTween();
        }

        public void DORestart()
        { 
            DORestart(false); 
        }

        public void DORestart(bool fromHere)
        {
            foreach (var anim in tweenAnims)
            {
                anim.OnManagerRestartTween(fromHere);
            }
            DOTween.Restart(this.gameObject);
        }

        public void DOComplete()
        {
            DOTween.Complete(this.gameObject);
        }

        public void DOKill()
        {
            foreach (var anim in tweenAnims)
            {
                anim.OnManagerKillTween();
            }
            DOTween.Kill(this.gameObject);
        }
    }
}