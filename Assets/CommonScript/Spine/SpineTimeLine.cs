using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using XClient.Scripts.Api;
using XGame.Timeline;

namespace XClient.Scripts
{
    public class SpineTimeLine : MonoBehaviour, ITimeLineStoryPreprocessor
    {
        private List<SkeletonGraphic> m_arySG = null;
        private List<int> m_reqKeys = null;
        private int m_finishCount = 0;

        // Start is called before the first frame update
        void Awake()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            m_arySG = ListPool<SkeletonGraphic>.Get();
            this.GetComponentsInChildren<SkeletonGraphic>(true, m_arySG);
            m_finishCount = 0;
            int nCount = m_arySG.Count;
            if (nCount == 0)
            {
                return;
            }

            // Debug.Log("測試代碼：SpineTimeLine：Awake");


            m_reqKeys = ListPool<int>.Get();


            GameObject spineObj = null;
            SpineResourcesLoader<GameObject> laoder = SpineResourcesLoader<GameObject>.Instance();
            for (int i = 0; i < nCount; ++i)
            {
                m_arySG[i].enabled = false;
                spineObj = m_arySG[i].gameObject;
                // m_arySG[i].dontDestroySkeletonAndState  = true;
                m_reqKeys.Add(laoder.LoadSpineAni(spineObj, OnResLoadCallback, 0, null, m_arySG[i]));
            }
        }

        /*
        private void OnEnable()
        {

            if(null!= m_arySG)
            {
                bool visible = (m_finishCount >= m_arySG.Length);
                //隐藏自己
                if (visible != gameObject.activeSelf)
                {
                    gameObject.BetterSetActive(visible);
                }
            }
          
            //ResetAni();
        }
        */


        void OnResLoadCallback(int resID, int reqKey, int userData = 0)
        {
            ++m_finishCount;
            //都加载完成了，就显示一下
            if (m_finishCount >= m_arySG.Count)
            {
                if (false == gameObject.activeSelf)
                {
                    gameObject.BetterSetActive(true);
                    //ResetAni();
                }

                int nCount = m_arySG.Count;
                for (int i = 0; i < nCount; ++i)
                {
                    m_arySG[i].enabled = true;
                }
            }
        }

        private void ResetAni()
        {
            var arrGraphics = ListPool<SkeletonGraphic>.Get();
            gameObject.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

            //var arrGraphics = gameObject.GetComponentsInChildren<SkeletonGraphic>();
            foreach (var ani in arrGraphics)
            {
                Spine.AnimationState state = ani.AnimationState;
                if (null != state)
                {
                    var track = state.GetCurrent(0);
                    if (track != null)
                    {
                        // Debug.LogError("track.TrackTime = 0f;");
                        track.TrackTime = 0f;
                    }
                }
            }

            ListPool<SkeletonGraphic>.Release(arrGraphics);
        }


        private void OnDestroy()
        {
            //推动一次资源的释放
            SpineResourcesLoader<GameObject> laoder = SpineResourcesLoader<GameObject>.Instance();
            if (null != m_reqKeys)
            {
                int nCount = m_reqKeys.Count;
                for (int i = 0; i < nCount; ++i)
                {
                    laoder.UnLoadSpineAni(m_reqKeys[i]);
                }

                ListPool<int>.Release(m_reqKeys);
            }
            
            ListPool<SkeletonGraphic>.Release(m_arySG);
            m_reqKeys = null;
            m_arySG = null;
        }

        public bool IsFinished()
        {

            if(m_arySG==null)
            {
                return false; 
            }

            if(m_arySG!=null&& m_arySG.Count==0)
            {
                return true;
            }
          
            return (m_finishCount >= m_arySG.Count);
        }
    }
}