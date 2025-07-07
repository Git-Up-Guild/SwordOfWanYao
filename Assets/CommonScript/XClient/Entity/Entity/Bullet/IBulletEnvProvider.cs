/*******************************************************************
** 文件名:	IBulletEnvProvider.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  子弹射击环境提供器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace XClient.Entity
{

    //构建发射现场
    public class BulletFireContext
    {
        //发射的源ID
        public ulong src;

        //发射目标ID
        public ulong target;

        //发射目标点
        public Vector3 targetPos;

        //发射起点 
        public Vector3 srcPos;
        //发射目标点
        public Vector3 targetLocalPos;

        //发射起点 
        public Vector3 srcLocalPos;

        //发射器的网络对象ID
        public ulong netLauncherObjectID = 0;

        //阵营
        public ulong camp = 0;

        //用户数据(透传数据,skillID)
        public int userData = 0;

        //显示方式 
        public CREATURE_VISIBLE_TYPE visibleType = CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_ALL;

        //子弹延迟时间
        public float delayduration = 0;


        //子弹回调的sink
        public IBulletEnvProvider bulletEnvProvider;

        //友方阵营
        public List<ulong> listFriendCamps;
        //敌方阵营
        public List<ulong> listEnemyCamps;
    }

    //buff的配置结果
    public class EffectContext
    {
        public string effectPath;
        public ECreaturePos effectPos;
        public bool valid;
    }


    public interface IBulletEnvProvider
    {

        //构建发射现场
        BulletFireContext BuildFireContext(ulong srcID,int bulletID, Transform root, int userData);


        //是否需要目标
        bool IsNeedTarget(ulong srcID, int bulletID, int userData);

        //是否需要朝向目标
        bool IsNeedForwardTarget(ulong srcID, int bulletID, int userData);

        //选择一个攻击目标
        IDReco SelectTarget(ulong srcID, int bulletID, IDReco IDReco, int userData,List<IDReco> filters=null);

        //由具体项目实现,判断是否可以发射,包括冷却之类的
        bool CanFire(ulong srcID, int bulletID, int userData);

        //是否需要外部提供命中的阵营,返回false，内部就使用阵营简单选择
        bool IsExternalSupportCamp();

        //获取外部目标阵营
        List<ulong> GetExternalHitCamp(BulletFireContext context, IBullet bullet);

        //是否能击中目标
        bool CanHitTarget(IDReco reco, BulletFireContext context, IBullet bullet);

        //通过ID获取位置
        Vector3 GetPos(ulong entID, BulletFireContext context, IBullet bullet);

        //处理定时伤害,
        bool OnCollisionDamage(BulletFireContext context, IBullet bullet, IDReco target);

        //处理定时伤害,
        bool OnTimedDamage(BulletFireContext context, IBullet bullet,IDReco target);

        //处理爆炸伤害
        bool OnExploreDamage(BulletFireContext context, IBullet bullet,IDReco target);

        //处理子弹消失
        void OnDestroy(BulletFireContext context, IBullet bullet);


        //buff表格转换提供器
         EffectContext GetBuffEffectContext(string szBuffEffect);

        //施法实体的根部
        Transform GetEntityRoot();

    }

}
