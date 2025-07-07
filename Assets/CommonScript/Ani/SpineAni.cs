/*******************************************************************
** 文件名:	SpineAni.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.9.03
** 版  本:	1.0
** 描  述:	
** 应  用:  控制spine播放动画

**************************** 修改记录 ******************************
** 修改人: 甘炳钧
** 日  期: 2024.12.03
** 描  述: 支持SkeletonGraphic
********************************************************************/


using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Network;
using static GameScripts.SpineAni;

namespace GameScripts
{
    public class SpineAni : NetObjectBehaviour<SpineAniData>
    {
        public class SpineAniData : MonoNetObject
        {
            //动作播放序列版本号
            public NetVarLong m_version;

            //正在播放的动作
            public NetVarString m_aniName;

            //是否循环
            public NetVarBool m_loop;

            //是否停在最后一个动作
            public NetVarBool m_stay;


            protected override void OnSetupVars()
            {
                //IsDebug = true;
                m_version = SetupVar<NetVarLong>("m_version");
                m_aniName = SetupVar<NetVarString>("m_aniName");
                m_loop = SetupVar<NetVarBool>("m_loop");
                m_stay = SetupVar<NetVarBool>("m_stay");
                m_loop.Value = false;
                m_stay.Value = false;
                m_version.Value = 0;
            }
        }

        //2D骨骼动画
        public SkeletonAnimation skeletonAnimation;
        public SkeletonGraphic skeletonGraphic;
        private IAnimationStateComponent skeletonAnimationState;

        //默认动作
        public string defaultAniName = "idle";

        //当前播放动作的版本号
        private long m_playVersion = 0;
        //add by 崔卫华
        private bool isAddEvent = false;

        // Start is called before the first frame update
        void Start()
        {
            if (null == skeletonAnimation && null == skeletonGraphic)
            {
                skeletonAnimation = this.GetComponentInChildren<SkeletonAnimation>();
            }

            if (null == skeletonAnimation && null == skeletonGraphic)
            {
                skeletonGraphic = this.GetComponentInChildren<SkeletonGraphic>();
            }

            if (null != skeletonAnimation)
            {
                skeletonAnimation.freeze = false;
                skeletonAnimationState = skeletonAnimation as IAnimationStateComponent;
            }

            if (null != skeletonGraphic)
            {
                skeletonGraphic.freeze = false;
                skeletonAnimationState = skeletonGraphic as IAnimationStateComponent;
            }

            if (skeletonAnimationState != null && skeletonAnimationState.AnimationState != null)
            {
                skeletonAnimationState.AnimationState.Complete += completeEvent;
                isAddEvent = true;
            }
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
            if (m_playVersion != NetObj.m_version.Value)
            {
                m_playVersion = NetObj.m_version.Value;

                if (skeletonAnimationState == null || skeletonAnimationState.AnimationState == null)
                {
                    return;
                }
                if (!isAddEvent)
                {
                    skeletonAnimationState.AnimationState.Complete += completeEvent;
                    isAddEvent = true;
                }
                if (string.IsNullOrEmpty(NetObj.m_aniName.Value) == false)
                {
                    if (skeletonAnimationState.AnimationState.Data.SkeletonData
                            .FindAnimation(NetObj.m_aniName.Value) == null)
                    {
                        Debug.LogError("不存在的动作: " + NetObj.m_aniName.Value);
                        return;
                    }

                    /*
                    if(NetObj.m_aniName.Value!="move")
                    {
                        Debug.LogError("播放动作: " + this.gameObject.GetHashCode() + "动作名称" + NetObj.m_aniName.Value);
                    }*/


                    skeletonAnimationState.AnimationState.SetAnimation(0, NetObj.m_aniName.Value, NetObj.m_loop.Value);

                    /*
                    if(skeletonAnimation.AnimationState.GetCurrent(0).TrackTime>0)
                    {
                        Debug.LogError("skeletonAnimation.AnimationState.GetCurrent(0).TrackTime==" + skeletonAnimation.AnimationState.GetCurrent(0).TrackTime);
                    }
                    */

                    skeletonAnimationState.AnimationState.GetCurrent(0).TrackTime = 0;
                }
            }
        }

        private void OnDestroy()
        {
            if (skeletonAnimationState != null && skeletonAnimationState.AnimationState != null)
            {
                skeletonAnimationState.AnimationState.Complete -= completeEvent;
            }
            isAddEvent = false;
            skeletonAnimationState = null;
        }

        public void DoAction(string name, bool loop = false, bool stay = false,bool force = false, string endAniName = null)
        {
            if (null == endAniName)
            {
                endAniName = defaultAniName;
            }
            if (NetObj.m_aniName.Value != name || NetObj.m_loop.Value != loop || NetObj.m_stay.Value != stay|| force)
            {
                NetObj.m_version.Value = NetObj.m_version.Value + 1;
                NetObj.m_loop.Value = loop;
                NetObj.m_stay.Value = stay;
                NetObj.m_aniName.Value = name;
                if (defaultAniName != endAniName && !string.IsNullOrEmpty(endAniName))
                {
                    defaultAniName = endAniName;
                }
                /*
                if (NetObj.m_aniName.Value != "move")
                {
                    Debug.LogError("播放动作: " + this.gameObject.GetHashCode() + "动作名称" + NetObj.m_aniName.Value);
                }
                */
            }
        }

        public void completeEvent(Spine.TrackEntry trackEntry)
        {
            if (NetObj.m_loop.Value == false)
            {
                if (NetObj.m_stay.Value == false && NetObj.m_aniName.Value == trackEntry.Animation.Name &&
                    skeletonAnimationState.AnimationState.Data.SkeletonData.FindAnimation(defaultAniName) != null)
                {
                    DoAction(defaultAniName, true);
                }
            }
        }

        public void SetFreezing(bool freeze)
        {
            if (null != skeletonAnimation)
            {
                skeletonAnimation.freeze = freeze;
               
            }else if (null != skeletonGraphic)
            {
                skeletonGraphic.freeze = freeze;
              
            }
        }

        //是否显示
        public void EnableSpineView(bool enable)
        {
            if (null != skeletonAnimation)
            {
                skeletonAnimation.enabled = enable;

            }
            else if (null != skeletonGraphic)
            {
                skeletonGraphic.enabled = enable;

            }
        }
    }
}