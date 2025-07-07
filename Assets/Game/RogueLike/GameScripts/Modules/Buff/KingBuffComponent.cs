/*******************************************************************
** 文件名:	KingBuffComponent.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.09
** 版  本:	1.0
** 描  述:	
** 应  用:  王国demo的buff组件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XClient.Common;
using XClient.Entity;


namespace GameScripts.KingHero
{
    public class KingBuffComponent : BuffBaseComponent
    {
        //创建
        public override void Awake()
        {
            base.Awake();

            //初始化效果管理器
            if (effectActionCreate==null)
            {
                effectActionCreate = KingBuffEffectCreator.Instance;
            }
        }

        public override void AddBuff(int buffID, int layer = 1)
        {

            cfg_Buff cfg = GameGlobal.GameScheme.Buff_0(buffID);
            if (null == cfg)
            {
                Debug.LogError("不存在的buffID m_buffID=" + buffID);
                return;
            }

            //独立的直接加
            if(cfg.isUnique>0)
            {
                base.AddBuff(buffID);
            }else
            {
                //不独立的，不存在才加
                if(FindBuffByID(buffID)==null)
                {
                    base.AddBuff(buffID);
                }
            }
           
        }


    }
}


