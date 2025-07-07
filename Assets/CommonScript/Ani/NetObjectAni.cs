/*******************************************************************
** 文件名:	NetObjectAni.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.9.20
** 版  本:	1.0
** 描  述:	
** 应  用:  网络动画对象

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Network;
using static GameScripts.NetObjectAni;



namespace GameScripts
{
    public class NetObjectAni : NetObjectBehaviour<NetObjectAni.NetObjectAniData>
    {


        public class NetObjectAniData : MonoNetObject
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

        //动画状态控制器
        public Animator animator;

        //序列帧动画
        public SpriteRenderAni spriteRenderAni;

        //默认动作
        public string defaultAniName = "idle";

        //当前播放动作的版本号
        private long m_playVersion = 0;



        private void Awake()
        {
            if (null == skeletonAnimation)
            {
                skeletonAnimation = this.GetComponentInChildren<SkeletonAnimation>();
            }

            if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.Complete += OnSkeletonAnimationCompleteEvent;

                skeletonAnimation.AnimationState.SetAnimation(0, defaultAniName, true);
            }

            if (null == animator)
            {
                animator = this.GetComponentInChildren<Animator>();
            }

            if (null == spriteRenderAni)
            {
                spriteRenderAni = this.GetComponentInChildren<SpriteRenderAni>();
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }


        // Update is called once per frame
        void Update()
        {
            if (m_playVersion != NetObj.m_version.Value)
            {
                m_playVersion = NetObj.m_version.Value;


                //动画状态机方式控制
                if (animator != null)
                {
                    animator.Play(NetObj.m_aniName.Value);
                }
                else if (spriteRenderAni != null)
                {
                    spriteRenderAni.DoAction(NetObj.m_aniName.Value, NetObj.m_loop.Value);
                }
                else
                {
                    //spine动画方式控制
                    if (skeletonAnimation == null || skeletonAnimation.AnimationState == null)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(NetObj.m_aniName.Value) == false)
                    {

                        if (skeletonAnimation.AnimationState.Data.SkeletonData.FindAnimation(NetObj.m_aniName.Value) == null)
                        {
                            Debug.LogError("不存在的动作: " + NetObj.m_aniName.Value);
                            return;

                        }
                        skeletonAnimation.AnimationState.SetAnimation(0, NetObj.m_aniName.Value, NetObj.m_loop.Value);
                        skeletonAnimation.AnimationState.GetCurrent(0).TrackTime = 0;
                    }
                }




            }
        }

        private void OnDisable()
        {
            m_playVersion = 0;
        }

        private void OnDestroy()
        {
            if (skeletonAnimation != null && skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.Complete -= OnSkeletonAnimationCompleteEvent;
            }

            skeletonAnimation = null;
            animator          = null;
            m_playVersion     = 0;
        }

        public void DoAction(string name, bool loop = false, bool stay = false)
        {

            if (NetObj.m_aniName.Value != name || NetObj.m_loop.Value != loop || NetObj.m_stay.Value != stay)
            {

                NetObj.m_version.Value = NetObj.m_version.Value + 1;
                NetObj.m_loop.Value = loop;
                NetObj.m_stay.Value = stay;
                NetObj.m_aniName.Value = name;
            }

        }

        //spine动画播放完成
        public void OnSkeletonAnimationCompleteEvent(Spine.TrackEntry trackEntry)
        {
            if (NetObj.m_loop.Value == false)
            {
                if (NetObj.m_stay.Value == false && NetObj.m_aniName.Value == trackEntry.Animation.Name && skeletonAnimation.AnimationState.Data.SkeletonData.FindAnimation(defaultAniName) != null)
                {
                    DoAction(defaultAniName, true);

                }

            }
        }
    }

}

