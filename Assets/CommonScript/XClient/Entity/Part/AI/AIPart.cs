/*******************************************************************
** 文件名:	AIPart.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物AI决策部件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame.Asset.Manager;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.Poolable;

namespace XClient.Entity
{

    public class AIActionComparer: IComparer<IAIAction>
    {
        public int Compare(IAIAction x, IAIAction y)
        {
            return x.GetPriority() - y.GetPriority();
        }
    }

    public class AIPart : BasePart, IAIPart
    {
        //排序对象
        static AIActionComparer actionComp = new AIActionComparer();

        //当前在跑的AI列表
        List<IAIAction> m_listAIAction = new List<IAIAction>();


        protected override void OnInit(object context)
        {
            cfg_Monster cfg = master.config as cfg_Monster;

            int nLen = cfg.AIID.Length;
            int AIID = 0;
            
            cfg_AI cfg_AI = null;
            IAIAction action = null;
            ICreatureEntity creatureMaster = master as ICreatureEntity;
            for (int i=0;i<nLen;++i)
            {
                //todo
                AIID = cfg.AIID[i];
                if (AIID <= 0)
                    continue;

                cfg_AI = GameGlobal.GameScheme.AI_0((uint)AIID);
                if(null== cfg_AI)
                {
                    Debug.LogError("不存在的AI配置 AIID=" + AIID);
                    continue;
                }

                action = __CreateAIAction(cfg_AI);
                if(null==action)
                {
                    Debug.LogError("创建AI行为失败 AIID=" + AIID);
                    continue;
                }

                action.SetMaster(creatureMaster);
                //action.Start();
                m_listAIAction.Add(action);

            }
            
            //按优先级排序
            __SortAction(m_listAIAction);
        }

        public override void OnAfterEntityInit()
        {
            int nCount = m_listAIAction.Count;
            for (int i = 0; i < nCount; ++i)
            {
                m_listAIAction[i].Start();
            }
        }

        public override void OnUpdate()
        {
            int nCount = m_listAIAction.Count;
            for(int i=0;i< nCount;++i)
            {
                if (m_listAIAction[i].OnExeUpdate())
                {
                    break;
                }
            }
        }

        protected override void OnReset()
        {
            int nCount = m_listAIAction.Count;
            for(int i=0;i<nCount;++i)
            {
                m_listAIAction[i].Stop();
                m_listAIAction[i].Reset();
            }

            m_listAIAction.Clear();
        }

        
        private IAIAction __CreateAIAction(cfg_AI cfg_AI)
        {
            IAIActionCreator creator = MonsterSystem.Instance.GetAICreator();
            if(null== creator)
            {
                return null;
            }
            return creator.CreateAIAction((int)cfg_AI.nID, cfg_AI);
        }
        



        private void __SortAction(List<IAIAction> listAIAction)
        {
            listAIAction.Sort(actionComp);
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            int nCount = m_listAIAction.Count;
            for (int i = 0; i < nCount; ++i)
            {
                m_listAIAction[i].OnReceiveEntityMessage(id, data);
            }

        }


    }

}
