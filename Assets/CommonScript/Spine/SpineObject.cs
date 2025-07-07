using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Scripts.Api;
using XGame.Load;
using XGame.LOP;
using XGame.Utils;

namespace XClient.Scripts
{
    public class SpineObject : MonoBehaviour
    {
        //需要加载的资源
        public ResourceRef resRef;

        //是否重新开始动画
        public bool isRestartAnim = true;

        private int m_resHandle;

        //加载回来的对象
        private GameObject m_res;


        private void OnEnable()
        {
            if (m_resHandle == 0 && string.IsNullOrEmpty(resRef.path) == false)
            {
                IResourcesLoader laoder = SpineResourcesLoader<GameObject>.Instance();
                m_resHandle = laoder.LoadRes(resRef.path, OnResLoadCallback, true, 0, 0, this.transform);
            }
            m_isSetLoop = false;
            RestartAnimation();
        }

        private void OnDestroy()
        {
            if(m_resHandle > 0)
            {
                IResourcesLoader laoder = SpineResourcesLoader<GameObject>.Instance();
                laoder.UnloadRes(m_resHandle);

                m_resHandle = 0;
            }

            m_res = null;
        }
 
        void OnResLoadCallback(int resID, int reqKey, int userData = 0)
        {
            m_res = LOPObjectManagerInstance.obj.Get(resID) as GameObject;
            if (null != m_res)
            {
                if(m_res.transform.parent!= transform)
                    m_res.transform.BetterSetParent(transform);
                m_res.transform.localPosition = Vector3.zero;
                m_res.transform.localRotation = Quaternion.identity;
                m_res.transform.localScale = Vector3.one;
                //重播动画
                if (isRestartAnim)
                    RestartAnimation();
            }
        }

        //add by 崔卫华
        private string m_animation;
        private bool m_loop;
        private bool m_isSetLoop = false;
        private bool m_isClearTrack = false;
        public void SetAnimation(string anim, bool loop = false, bool isClearTrack = false)
        {
            m_animation = anim;
            m_loop = loop;
            m_isSetLoop = true;
            m_isClearTrack = isClearTrack;
            RestartAnimation();
        }

        private void RestartAnimation()
        {
            if (m_res == null)
                return;


            var arrAnims = ListPool<SkeletonAnimation>.Get();
            m_res.GetComponentsInChildren<SkeletonAnimation>(arrAnims);

            //var arrAnims = m_res.GetComponentsInChildren<SkeletonAnimation>();
            foreach (var ani in arrAnims)
            {
                var oldtrack = ani.AnimationState.GetCurrent(0);
                if (!string.IsNullOrEmpty(m_animation))
                {
                    bool trackLoop = oldtrack.Loop;
                    if (m_isClearTrack) ani.AnimationState.ClearTrack(0);
                    var trackEntry = ani.AnimationState.SetAnimation(0, m_animation, m_isSetLoop ? m_loop : trackLoop);
                    trackEntry.MixDuration = 0f;
                }
                oldtrack.TrackTime = 0f;
                //var newTrack = ani.AnimationState.GetCurrent(0);
                //if (newTrack != null)
                //{
                //    newTrack.TrackTime = 0f;
                //}
            }

            ListPool<SkeletonAnimation>.Release(arrAnims);


            var arrGraphics = ListPool<SkeletonGraphic>.Get();
            m_res.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

            //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
            foreach (var ani in arrGraphics)
            {
                var oldtrack = ani.AnimationState.GetCurrent(0);
                if (!string.IsNullOrEmpty(m_animation))
                {
                    bool trackLoop = oldtrack.Loop;
                    if (m_isClearTrack) ani.AnimationState.ClearTrack(0);
                    //Debug.Log("SetAnimation");
                    var trackEntry = ani.AnimationState.SetAnimation(0, m_animation, m_isSetLoop ? m_loop : trackLoop);
                    trackEntry.MixDuration = 0;
                }
                oldtrack.TrackTime = 0f;
                //var newTrack = ani.AnimationState.GetCurrent(0);
                //if (newTrack != null)
                //{
                //    newTrack.TrackTime = 0f;
                //}
            }
            ListPool<SkeletonGraphic>.Release(arrGraphics);
        }

        //临时代码 只支持SkelGraph
        public void Freeze(bool isFreeze)
        {
            if (m_res == null)
                return;


            //var arrAnims = ListPool<SkeletonAnimation>.Get();
            //m_res.GetComponentsInChildren<SkeletonAnimation>(arrAnims);

            ////var arrAnims = m_res.GetComponentsInChildren<SkeletonAnimation>();
            //foreach (var ani in arrAnims)
            //{
            //    ani.is = isFreeze;
            //}

            //ListPool<SkeletonAnimation>.Release(arrAnims);


            var arrGraphics = ListPool<SkeletonGraphic>.Get();
            m_res.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

            //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
            foreach (var ani in arrGraphics)
            {
                ani.freeze = isFreeze;
            }
            ListPool<SkeletonGraphic>.Release(arrGraphics);
        }
    }
}
