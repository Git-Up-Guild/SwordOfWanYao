/************************************************************************************
* Copyright (c) 2020 All Rights Reserved.
*命名空间：XGame.AssetScript
*文件名： VideoPlayerController.cs
*创建人： 郑秀程
*创建时间：2020/7/7 17:33:18 
*描述：视频展示器
*=====================================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using XGame.Attr;
using XGame.UnityObjPool;
using XGame.Utils;

namespace XGame.AssetScript.Video
{
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(RawImage))]
    public class RawImageVideoDisplay : MonoBehaviour, IUnityObjectPoolSinkWithObj
    {
        //显示目标
        [SerializeField]
        [AutoBind(typeof(RawImage))]
        private RawImage m_RawImage;
        public RawImage rawImage => m_RawImage;

        //播放目标
        [SerializeField]
        [AutoBind(typeof(VideoPlayer))]
        private VideoPlayer m_Player;
        public VideoPlayer player=>m_Player;

        /// <summary>
        /// 视频资源路径
        /// </summary>
        [SerializeField]
        private ResourceRef m_VideoResPath;

        //是否有效
        private bool isValid => m_RawImage != null && m_Player != null;

        /// <summary>
        /// 适配资源路径
        /// </summary>
        private uint m_VideoResHandle;

        private void Awake()
        {
            m_RawImage = GetComponent<RawImage>();
            m_Player = GetComponent<VideoPlayer>();

            if(m_Player != null)
            {
                m_Player.renderMode = VideoRenderMode.RenderTexture;
                m_Player.loopPointReached += (p) => {
                    Debug.Log($"播放完成！dur={p.length}");
                };
            }
        }

        private void LoadRes()
        {
            if (m_VideoResHandle > 0)
                return;

            IUnityObjectPool pool = XGameComs.Get<IUnityObjectPool>();
            if (pool != null)
            {
                m_VideoResHandle = pool.LoadRes<VideoClip>(m_VideoResPath.path, 0, this, false);
            }
        }

        private void UnloadRes()
        {
            if (m_VideoResHandle > 0)
            {
                m_Player.clip = null;

                m_VideoResHandle = 0;
                IUnityObjectPool pool = XGameComs.Get<IUnityObjectPool>();
                if (pool != null)
                    pool.UnloadRes(m_VideoResHandle);
            }
        }

        private void OnEnable()
        {
            LoadRes();
        }

        private void OnDisable()
        {
            UnloadRes();
        }

        private void OnDestroy()
        {
            UnloadRes();
        }

        void Update()
        {
            if (!isValid || !m_Player.isPlaying || m_Player.clip == null)
                return;
            m_RawImage.texture = m_Player.texture;
        }

        public void OnUnityObjectLoadComplete(Object res, uint handle, object ud)
        {
            if(m_Player != null)
            {
                m_Player.clip = res as VideoClip;
            }
        }

        public void OnUnityObjectLoadCancel(uint handle, object ud)
        {
        }
    }
}