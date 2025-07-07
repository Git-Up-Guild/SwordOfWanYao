/*******************************************************************
** 文件名:	SpinePrefabPart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.12.13
** 版  本:	1.0
** 描  述:	
** 应  用:  负责加载spine资源的预制体部件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameScripts;
using Spine.Unity;
using UnityEngine;
using XClient.Entity;
using XClient.Scripts.Api;
using XGame.Load;
using XGame.LOP;
using XGame.Utils;

namespace XGame.Entity.Part
{
    public class SpinePrefabPart : PrefabPart
    {
        private bool m_bResLoaded = false;
        public bool ResLoadEnd => m_bResLoaded;

        private SpineAni m_spineAniComp = null;
        private string m_spineAniName;
        private bool m_bSpineLoop;
        private bool m_bSpineStay;
        private string m_spineEndAniName;
        public bool m_doAction = true;

        protected override void OnInit(object context)
        {
            base.OnInit(context);
            m_bResLoaded = false;
            m_spineAniName = "idle";
            m_bSpineLoop = true;
            m_bSpineStay = false;
        }

        protected override void OnReset()
        {
            base.OnReset();

            if (null != transform)
            {
                var arrGraphics = ListPool<SkeletonGraphic>.Get();
                transform.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

                //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
                foreach (var ani in arrGraphics)
                {
                    ani.DOKill();
                }

                ListPool<SkeletonGraphic>.Release(arrGraphics);
                //GameObjectAniCtrl.FreezeSpineAni(transform.gameObject, false);
            }
            if (null != m_spineAniComp)
            {
                //Debug.LogError("=================OnReset" + master.id);
                //强制刷新动画状态  free为false
                m_freeze = false;
                m_spineAniComp.SetFreezing(m_freeze);
                m_spineAniComp = null;
            }

            m_bResLoaded = false;
            m_doAction = true;
        }

        protected override void LoadRes()
        {
            UnloadRes();

            m_bResLoaded = false;

            if (resHandle == 0 && string.IsNullOrEmpty(resPath) == false)
            {
                IResourcesLoader laoder = SpineResourcesLoader<GameObject>.Instance();
                //Debug.Log($"LoadSpinePrefabRes:{resPath}");
                //string resPath1 = "Game/ImmortalFamily/GameResources/Artist/Creature/Role1/F_1_GongJianTest.prefab";
                resHandle = (uint)laoder.LoadRes(resPath, OnResLoadCallback, true, 0, 0, this.transform);
            }else
            {
                //子弹不检查路径
                if (master.type != EntityType.NormalBulletType)
                    Debug.LogError("加载资源失败，资源路径为null configId=" + master.configId);
            }
        }

        private bool m_freeze = false;
        public void SetFreeze(bool freeze)
        {
            if (m_freeze != freeze)
            {
                m_freeze = freeze;
                if (m_spineAniComp != null)
                {
                    m_spineAniComp.SetFreezing(m_freeze);
                }
            }
        }

        protected override void UnloadRes()
        {
            if (resHandle > 0)
            {
                IResourcesLoader laoder = SpineResourcesLoader<GameObject>.Instance();
                laoder.UnloadRes((int)resHandle);

                resHandle = 0;
            }
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);
            if (id == EntityMessageID.ResLoaded)
            {
                ICreatureEntity ce = master as ICreatureEntity;
                m_spineAniComp = ce.GetComponent<SpineAni>();
                if (m_doAction && null != m_spineAniComp)
                {
                    m_spineAniComp.DoAction(m_spineAniName, m_bSpineLoop, m_bSpineStay, true, m_spineEndAniName);
                    m_spineAniComp.SetFreezing(m_freeze);
                }

                var arrGraphics = ListPool<SkeletonGraphic>.Get();
                transform.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

                //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
                foreach (var ani in arrGraphics)
                {
                    var track = ani.AnimationState.GetCurrent(0);
                    track.TrackTime = 0f;
                }

                ListPool<SkeletonGraphic>.Release(arrGraphics);
            }
        }

        void OnResLoadCallback(int resID, int reqKey, int userData = 0)
        {
            m_bResLoaded = true;
            GameObject res = LOPObjectManagerInstance.obj.Get(resID) as GameObject;
            OnUnityObjectLoadComplete(res, (uint)reqKey, userData);
        }

        public void DoAction(string name, bool loop = false, bool stay = false, bool force = false,
            string endAniName = null)
        {
            m_spineAniName = name;
            m_bSpineLoop = loop;
            m_bSpineStay = stay;
            m_spineEndAniName = endAniName;
            if (null != m_spineAniComp)
            {
                m_spineAniComp.DoAction(m_spineAniName, m_bSpineLoop, m_bSpineStay, force, endAniName);
            }
        }

        public void ResetColor()
        {
            var arrGraphics = ListPool<SkeletonGraphic>.Get();
            transform.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

            //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
            foreach (var ani in arrGraphics)
            {
                ani.color = Color.white;
            }

            ListPool<SkeletonGraphic>.Release(arrGraphics);
        }
        
        public void DoFake(float duration, float alpha)
        {
            var arrGraphics = ListPool<SkeletonGraphic>.Get();
            transform.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

            //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
            foreach (var ani in arrGraphics)
            {
                ani.DOFade(alpha, duration);
            }

            ListPool<SkeletonGraphic>.Release(arrGraphics);
        }
    }
}