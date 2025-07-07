/*******************************************************************
** 文件名: UISound.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	郑秀程
** 日  期:	2020-10-20
** 版  本:	1.0
** 描  述:	UI声音组件
** 应  用:  
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XGame.Audio;

namespace XGame.UI
{
    /// <summary>
    /// 开始播放时机
    /// </summary>
    public enum PlayMode
    {
        //Awake时就播放
        Awake,
        //激活时
        Enable,
        //点击时播放
        Click,
        //手动调用播放
        Manual
    }

    /// <summary>
    /// 停止播放时机
    /// </summary>
    public enum StopMode
    {
        Disable,
        Destroy,
        Auto,
    }

    public class UISound : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 播放模式
        /// </summary>
        public PlayMode playMode = PlayMode.Awake;

        /// <summary>
        /// 停止播放模式
        /// </summary>
        public StopMode stopMode = StopMode.Disable;

        /// <summary>
        /// 声音ID
        /// </summary>
        public int soundID;

        /// <summary>
        /// 播放间隔
        /// </summary>
        public float playInterval = 0.1f;

        /// <summary>
        /// 最后播放时间
        /// </summary>
        private float lastPlayTick = 0f;

        private void Awake()
        {
            if(playMode == PlayMode.Awake)
            {
                XGameComs.Get<IAudioCom>()?.PlayAudio(soundID);
            }
        }

        private void OnEnable()
        {
            if(playMode == PlayMode.Enable)
            {
                XGameComs.Get<IAudioCom>()?.PlayAudio(soundID);
            }
        }

        public void PlaySound()
        {
            if(Time.time - lastPlayTick > playInterval)
            {
                lastPlayTick = Time.time;
                XGameComs.Get<IAudioCom>()?.PlayAudio(soundID);
            }
        }

        private void OnDisable()
        {
            if(stopMode == StopMode.Disable)
            {
                XGameComs.Get<IAudioCom>()?.StopAudio(soundID);
            }
        }

        private void OnDestroy()
        {
            if (stopMode == StopMode.Destroy)
            {
                XGameComs.Get<IAudioCom>()?.StopAudio(soundID);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (playMode == PlayMode.Click)
            {
                PlaySound();
            }
        }
    }
}
