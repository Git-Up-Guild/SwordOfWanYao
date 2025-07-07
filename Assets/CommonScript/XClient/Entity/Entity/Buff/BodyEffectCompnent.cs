/*******************************************************************
** 文件名:	BodyEffectCompnent.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.23
** 版  本:	1.0
** 描  述:	
** 应  用:  挂接在身上的部件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.LightEffect;

namespace XGame.Entity
{
    public class BodyEffectCompnent : VisibleComponent
    {
        private void Awake()
        {
            //if (null != visibleRoot)
            //{
            //    visibleRoot = this.gameObject;
            //}
        }
        [NonSerialized]
        private uint m_dizzyEffectHandle = 0;
        //因为添加眩晕Effect的Act和结束眩晕的Act不是一个Act
        public uint PlayDizzyEffect(string path, float duration, GameObject effPosObj)
        {
            StopDizzyEffect();
            m_dizzyEffectHandle = PlayEffect(path, duration, effPosObj);
            return m_dizzyEffectHandle;
        }
        public void StopDizzyEffect()
        {
            if (m_dizzyEffectHandle > 0)
            {
                StopEffect(m_dizzyEffectHandle);
                m_dizzyEffectHandle = 0;
            }

        }
        public uint PlayEffect(string path, float duration, GameObject effPosObj)
        {
            bool local = (effPosObj != null);
            Vector3 pos = local ? Vector3.zero : transform.position;
            //return EffectMgr.Instance().PlayEffect(path, ref pos, duration, visibleRoot?visibleRoot.transform:null, local);
            return EffectMgr.Instance()
                .PlayEffect(path, ref pos, duration, effPosObj ? effPosObj.transform : null, local);
        }

        public void StopEffect(uint handle)
        {
            EffectMgr.Instance().StopEffect(handle);
        }
    }
}