/*******************************************************************
** 文件名:	Buff.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  buff的实现

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using I18N.Common;
using UnityEngine;
using XClient.Common;
using XClient.Entity;


namespace XGame.Entity
{
    public enum eBuffViewEffectType
    {
        Slient = 16,
    }


    public class Buff : IBuff
    {
        //拥有者ID
        private ulong m_master;

        //buff特效ID
        private uint m_effectHandle = 0;

        //头顶图标句柄
        private uint m_headIconHandle = 0;

        //效果创建器
        private IEffectActionCreate m_effectActionCreate;

        //buff的ID
        private int m_buffID;

        //当前的层
        private int m_layer;

        //buff起作用的时间
        private float m_finishTime;

        // buff自带的effect
        private List<IEffectAction> m_listEffectAction;

        //buff ID
        private cfg_Buff m_cfg = null;

        //sid
        private int m_sid = 0;

        //添加的客户端
        private long m_clientID;


        // static private CreateEffectContext createEffectContext  = new CreateEffectContext();

        public bool Create()
        {
            m_listEffectAction = new List<IEffectAction>();
            return true;
        }

        public int GetBuffID()
        {
            return m_buffID;
        }

        public long GetClientMaster()
        {
            return m_clientID;
        }

        public int GetLayer()
        {
            return m_layer;
        }

        public int GetSID()
        {
            return m_sid;
        }

        public void Init(object context = null)
        {
            BuffCreateContext buffCreateContext = (BuffCreateContext)context;
            m_effectActionCreate = buffCreateContext.effectActionCreate;
            m_buffID = buffCreateContext.buffID;
            m_clientID = buffCreateContext.clientID;
            m_master = buffCreateContext.srcID;

            //启动时间
            m_finishTime = 0; // Time.realtimeSinceStartup;

            m_cfg = GameGlobal.GameScheme.Buff_0(m_buffID);
            if (null == m_cfg)
            {
                Debug.LogError("不存在的buffID m_buffID=" + m_buffID);
                return;
            }

            //创建Effect
            //if (!string.IsNullOrEmpty(m_cfg.commandBuffList))
            //{
            //    List<EffectCmdContext> listCmdContext =
            //        m_effectActionCreate.GetEffectCmdContexts(m_buffID, m_cfg.commandBuffList);
            //    int nCount = listCmdContext.Count;
            //    EffectCmdContext effectContext = null;
            //    for (int i = 0; i < nCount; ++i)
            //    {
            //        effectContext = listCmdContext[i];
//
            //        CreateEffectContext.Instance.srcID = buffCreateContext.srcID;
            //        CreateEffectContext.Instance.effectCmdContext = effectContext;
//
            //        IEffectAction action =
            //            m_effectActionCreate.CreateEffectAction(effectContext.cmd, CreateEffectContext.Instance);
            //        if (null != action)
            //        {
            //            m_listEffectAction.Add(action);
            //        }
            //    }
            //}


            //添加buff特效
            string szBuffEffect = m_cfg.buffEffect;
            if (string.IsNullOrEmpty(szBuffEffect) == false)
            {

                IBulletEnvProvider bulletEnvProvider = EnvProviderMgr.Instance.GetBulletEnvProvider();  

                //每次都拆分字符串， 效率低下，将拆分结果缓存起来
                EffectContext effectContext = bulletEnvProvider.GetBuffEffectContext(szBuffEffect);
                if (effectContext.valid)
                {

                    SkillCompontBase ksComp = GameGlobal.EntityWorld.Local?.GetComponent<SkillCompontBase>(m_master);
                    var effPosObj = ksComp.GetSkillEffNode(effectContext.effectPos);
                    //var effectPos = (ECreaturePos)skillEffectData[1];
                    BodyEffectCompnent bc = GameGlobal.EntityWorld.Local?.GetComponent<BodyEffectCompnent>(m_master);
                    if (null != bc)
                    {
                        // m_effectHandle = bc.PlayEffect(m_cfg.szBuffEffect, m_cfg.duration + 99999);
                        //客户端先999999999
                        m_effectHandle = bc.PlayEffect(effectContext.effectPath, 9999999, effPosObj);
                    }
                }

                
            }

            //添加头顶图标
            if (m_cfg.buffIcon > 0)
            {
                //TitleBuffComponent tbc = manager.GetComponent<TitleBuffComponent>(m_master);
                if (null != tbc)
                {
                    m_headIconHandle = tbc.AddIcon(m_cfg.buffIcon, m_master, m_buffID);
                }
            }

            SetLayer(buffCreateContext.buffLayer);

            m_finishTime = Time.realtimeSinceStartup + m_cfg.buffIcon;
        }
        private TitleBuffComponent m_tbc;
        public TitleBuffComponent tbc
        {
            get
            {
                if (m_tbc == null)
                {
                    m_tbc = GameGlobal.EntityWorld.Local?.GetComponent<TitleBuffComponent>(m_master);
                }
                return m_tbc;
            }
        }
        public bool IsFinish()
        {
            return false;
            return Time.realtimeSinceStartup > m_finishTime;
        }

        public void OnUpdate()
        {
            int nCount = m_listEffectAction.Count;
            for (int i = 0; i < nCount; ++i)
            {
                m_listEffectAction[i].OnUpdate();
            }
        }

        public void Release()
        {
            Reset();
            m_listEffectAction = null;

        }

        public void Reset()
        {
            GameGlobal.EntityWorld.Local?.GetEntity(m_master);
            if (m_effectHandle > 0)
            {
                BodyEffectCompnent bc = GameGlobal.EntityWorld.Local.GetComponent<BodyEffectCompnent>(m_master);
                if (null != bc)
                {
                    bc.StopEffect(m_effectHandle);
                    m_effectHandle = 0;
                }
            }

            if (m_headIconHandle > 0)
            {
                if (null != tbc)
                {
                    tbc.RemoveIcon(m_headIconHandle);
                    m_headIconHandle = 0;
                }
            }
            m_tbc = null;

            int nCount = m_listEffectAction.Count;
            for (int i = 0; i < nCount; ++i)
            {
                m_effectActionCreate.ReleaseEffectAction(m_listEffectAction[i]);
            }

            m_listEffectAction.Clear();
            m_effectActionCreate = null;
            m_sid = 0;
            m_master = 0;
        }

        public void SetClientMaster(long clientID)
        {
            m_clientID = clientID;
        }

        public void SetLayer(int layer)
        {
            m_layer = layer;
            if (null != tbc)
            {
                tbc.SetLayer(m_headIconHandle, m_layer);
            }
        }

        public void SetSID(int sid)
        {
            m_sid = sid;
        }


        public void Start()
        {
            int nCount = m_listEffectAction.Count;
            for (int i = 0; i < nCount; ++i)
            {
                m_listEffectAction[i].Start();
            }
        }

        public void Stop()
        {
            int nCount = m_listEffectAction.Count;
            for (int i = 0; i < nCount; ++i)
            {
                m_listEffectAction[i].Stop();
            }
        }


       
    }
}