/*******************************************************************
** 文件名:	EffectMgr.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.15
** 版  本:	1.0
** 描  述:	
** 应  用:  增加特效管理器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Pool;
using XClient.Common;
using XGame;
using XClient.Scripts.Api;
using XGame.Load;
using XGame.LOP;
using XGame.Poolable;
using XGame.UnityObjPool;

namespace XClient.LightEffect
{
    class EffectNode
    {
        public uint handle = 0;
        public Vector3 pos;
        public float duration;
        public float startTime;
        public Transform parent;
        public Transform view;

        public bool bLoclPos = false;

        //public bool suportPS = false;
        public List<ParticleSystem> listPS = null;
    }

    public class EffectMgr : IUnityObjectPoolSinkWithObj
    {
        static EffectMgr _Instance = null;

        //private IUnityObjectPool m_unityObjPool;
        private IResourcesLoader m_loader;
        private IItemPoolManager m_itemPoolMgr;
        private Dictionary<uint, EffectNode> dicEffects;
        private float m_curTime = 0;
        private GameObject m_effectRoot = null;
        private List<EffectNode> m_listWaitDelete = new List<EffectNode>(32);

        static public EffectMgr Instance()
        {
            if (null == _Instance)
            {
                _Instance = new EffectMgr();
                _Instance.Init();
            }

            return _Instance;
        }

        public void Init()
        {
            //m_unityObjPool = XGameComs.Get<IUnityObjectPool>();
            m_loader = SpineResourcesLoader<GameObject>.Instance();
            m_itemPoolMgr = XGameComs.Get<IItemPoolManager>();
            dicEffects = new Dictionary<uint, EffectNode>();
            m_curTime = Time.realtimeSinceStartup;
            m_effectRoot = new GameObject("EffectRoot");
        }

        public uint PlayEffect(string path, ref Vector3 pos, float duration, Transform parent = null,
            bool bLoclPos = false)
        {
            if (dicEffects.Count > 300)
            {
#if UNITY_EDITOR
                // Debug.LogError("特效个数超标，跳过播放， 当前个数=    " + dicEffects.Count);
#endif
                return 0;
            }

            m_curTime = Time.realtimeSinceStartup;
            EffectNode node = m_itemPoolMgr.PopObjectItem<EffectNode>();
            node.pos = pos;
            node.duration = duration * GameGlobal.invTimeScale;
            node.startTime = m_curTime;
            node.parent = parent;
            node.bLoclPos = bLoclPos;
            //m_unityObjPool.LoadRes<GameObject>(path, node, this, true, parent);
            node.handle = (uint)m_loader.LoadRes(path, OnResLoadCallback, true, 0, 0, parent);
            dicEffects.Add(node.handle, node);

            return node.handle;
        }

        public void StopEffect(uint handle)
        {
            EffectNode node = null;
            if (dicEffects.TryGetValue(handle, out node))
            {
                dicEffects.Remove(handle);
                ResetEffectNode(node);
            }
        }

        public void Update()
        {
            if (dicEffects.Count == 0)
            {
                return;
            }


            m_curTime = Time.realtimeSinceStartup;
            foreach (EffectNode node in dicEffects.Values)
            {
                if (m_curTime - node.startTime >= node.duration)
                {
                    m_listWaitDelete.Add(node);
                }
            }

            EffectNode nodeDel;
            int nCount = m_listWaitDelete.Count;
            for (int i = 0; i < nCount; ++i)
            {
                nodeDel = m_listWaitDelete[i];
                dicEffects.Remove(nodeDel.handle);
                ResetEffectNode(nodeDel);
            }

            m_listWaitDelete.Clear();
        }

        public void Clear()
        {
            foreach (EffectNode node in dicEffects.Values)
            {
                ResetEffectNode(node);
                //m_unityObjPool.UnloadRes(node.handle, true, true, false, true, 180);
            }

            dicEffects.Clear();
        }

        public void Release()
        {
            Clear();
            if (null != m_effectRoot)
            {
                GameObject.DestroyImmediate(m_effectRoot);
                m_effectRoot = null;
            }

            m_listWaitDelete.Clear();
            m_listWaitDelete = null;
            dicEffects = null;
            _Instance = null;
        }

        void OnResLoadCallback(int resID, int reqKey, int userData = 0)
        {
            uint handle = (uint)reqKey;
            EffectNode node = null;
            if (dicEffects.TryGetValue(handle, out node))
            {
                GameObject res = LOPObjectManagerInstance.obj.Get(resID) as GameObject;
                OnUnityObjectLoadComplete(res, handle, node);
            }
        }

        public void OnUnityObjectLoadComplete(UnityEngine.Object res, uint handle, object ud)
        {
            if (ud == null || null == res)
            {
                return;
            }

            EffectNode node = ud as EffectNode;
            GameObject m_viewItem = res as GameObject;

            Transform parent = node.parent != null ? node.parent : m_effectRoot.transform;

            if (parent != m_viewItem.transform.parent)
            {
                m_viewItem.transform.BetterSetParent(parent);
            }

            m_viewItem.transform.localScale = Vector3.one;

            /*
            else
            {
                Debug.LogError("跳过 SetParent " + m_viewItem.name);
            }*/

            //m_viewItem.transform.localPosition = Vector3.zero;
            if (m_viewItem.transform.localRotation != Quaternion.identity)
            {
                m_viewItem.transform.localRotation = Quaternion.identity;
            }

            /*
            else
            {
                Debug.LogError("m_viewItem.transform.localRotation " + m_viewItem.name);
            }*/

            if (node.bLoclPos == false)
            {
                m_viewItem.transform.position = node.pos;
            }
            else
            {
                if (m_viewItem.transform.localPosition != node.pos)
                    m_viewItem.transform.localPosition = node.pos;
                if (m_viewItem.transform.localScale != Vector3.one)
                    m_viewItem.transform.localScale = Vector3.one;
            }

            node.startTime = m_curTime;
            node.view = m_viewItem.transform;

            List<ParticleSystem> listPS = ListPool<ParticleSystem>.Get();
            node.view.GetComponentsInChildren(false, listPS);
            int nCount = listPS.Count;
            for (int i = 0; i < nCount; ++i)
            {
                listPS[i].Simulate(0);
                listPS[i].Play();
            }

            if (nCount > 0)
            {
                node.listPS = listPS;
            }
            else
            {
                ListPool<ParticleSystem>.Release(listPS);
            }

            var arrGraphics = ListPool<SkeletonGraphic>.Get();
            node.view.GetComponentsInChildren<SkeletonGraphic>(arrGraphics);

            //var arrGraphics = m_res.GetComponentsInChildren<SkeletonGraphic>();
            foreach (var ani in arrGraphics)
            {
                var track = ani.AnimationState.GetCurrent(0);
                track.TrackTime = 0f;
            }

            ListPool<SkeletonGraphic>.Release(arrGraphics);
        }

        public void OnUnityObjectLoadCancel(uint handle, object ud)
        {
        }

        private void ResetEffectNode(EffectNode node)
        {
            node.parent = null;
            if (null != node.view)
            {
                //有粒子系统的
                if (node.listPS != null)
                {
                    List<ParticleSystem> listPS = node.listPS;
                    //node.view.GetComponentsInChildren(false, listPS);
                    int nCount = listPS.Count;
                    for (int i = 0; i < nCount; ++i)
                    {
                        listPS[i].Stop();
                    }

                    ListPool<ParticleSystem>.Release(node.listPS);
                    node.listPS = null;
                }

                node.view = null;
            }

            //m_unityObjPool.UnloadRes(node.handle, false, true, false, true, 180);
            m_loader.UnloadRes((int)node.handle, true);
            node.handle = 0;
            m_itemPoolMgr.PushObjectItem(node);
        }
    }
}