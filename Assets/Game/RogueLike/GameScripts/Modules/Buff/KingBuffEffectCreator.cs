/*******************************************************************
** 文件名:	KingBuffEffectCreator.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.09
** 版  本:	1.0
** 描  述:	
** 应  用:  buff的效果创建器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using GameScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XGame.Entity;
using XGame.Poolable;
using XGame.Utils;

namespace GameScripts.KingHero
{
    //效果枚举
    public class EffectActionType
    {
        //起始坐标
        public readonly static int MAX_ID = 0;


        //反伤
        public readonly static string COUNTERINJURY = "counjy";
        //增加攻击力
        public readonly static string ATTACK_BUFF = "attackBuff";
        //增加暴击
        public readonly static string ATTCRIT_BUFF = "attCritBuff";
        //金钱掉落
        public readonly static string MONEY_DROP = "moneyDrop";
        //眩晕
        public readonly static string DIZZ_BUFF = "dizzBuff";
        //加技能CD
        public readonly static string ADD_CD = "addCD";
        //停止攻击
        public readonly static string STOP_ATTACK = "stopAttack";
        //增加金钱
        public readonly static string ADD_PER_MONEY = "addPerMoney";
        //增加移动速度
        public readonly static string SPEED_PER = "speedPer";
        //攻击增加伤害
        public readonly static string ATTACK_BOOM = "attackBoom";
        //受击增加伤害
        public readonly static string BEHURT_PERT = "beHurtPert";
        //秒杀
        public readonly static string ATTACK_SEC_KILL = "attackSeckill";


        //刷怪
        public readonly static string SUMMON_MONSTER = "summonMonster";
        //加血
        public readonly static string ADD_HP_PER = "addHpPer";
        //回合技能
        public readonly static string IS_TURN_WIN_SKILL = "isTurnWinSkill";
        //赋值卡片
        public readonly static string COPY_CARD = "copyCard";

        //最大效果下标
        public readonly static int EFFECT_TYPE_MAX = ++MAX_ID;
    }




    public class KingBuffEffectCreator : Singleton<KingBuffEffectCreator>, IEffectActionCreate
    {

        //动作字典
        private Dictionary<string,Type> m_dicActionTypes = new Dictionary<string, Type>();

        //buff的效果解析列表(减少GC)
        private Dictionary<int, List<EffectCmdContext>> m_dicEffectCmdList = new Dictionary<int, List<EffectCmdContext>>();

        //构造函数
        public  KingBuffEffectCreator()
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            itemPoolMgr.Register<Buff>();





        }

        //创建一个Effect
        public IEffectAction CreateEffectAction(string id, object context)
        {
            Type t;
            if(m_dicActionTypes.TryGetValue(id,out t))
            {
                IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
                return itemPoolMgr.Pop(t, context) as IEffectAction;
            }else
            {
                Debug.LogError("不存在效果 id=" + id);
            }

            return null;
        }

        public List<EffectCmdContext> GetEffectCmdContexts(int buffID, string commandBuffList)
        {
            List<EffectCmdContext> listContext = null;
            if (m_dicEffectCmdList.TryGetValue(buffID, out listContext) == false)
            {
                //添加新的技能项
                listContext = new List<EffectCmdContext>();
                m_dicEffectCmdList.Add(buffID, listContext);

                string[] aryContext = commandBuffList.Split(";");
                string[] item;
                EffectCmdContext context = null;
                int nLen = aryContext.Length;
                for (int i = 0; i < nLen; ++i)
                {
                    item = aryContext[i].Split("#");
                    if (item.Length < 1)
                    {
                        continue;
                    }

                    context = new EffectCmdContext();
                    context.cmd = item[0];
                    int nCount = item.Length;
                    for (int j = 1; j < nCount; ++j)
                    {
                        context.param.Add(int.Parse(item[j]));
                    }

                    listContext.Add(context);
                }
            }

            return listContext;
        }

        //回收到对象池
        public void ReleaseEffectAction(IEffectAction action)
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            itemPoolMgr.Push(action);
        }
    }
}

