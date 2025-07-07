/*******************************************************************
** 文件名:	SkillCompontBase.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.08
** 版  本:	1.0
** 描  述:	
** 应  用:  管理技能释放的基类

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
using XGame.Ini;

public class SkillCompontBase : MonoBehaviour
{
    //
    [Header("技能关联的子弹发射器,处理技能释放效果")] public BulletLauncher bulletLauncher;

    [Header("头部技能特效挂点")] public GameObject headSkillEffPos;
    [Header("身体技能特效挂点")] public GameObject bodySkillEffPos;
    [Header("脚部技能特效挂点")] public GameObject footSkillEffPos;

    [Header("技能列表")] public List<int> skillIDs = new List<int>();

    [Header("技能冷却时间")] public List<float> skillCooldingTime = new List<float>();

    [Header("身份识别器")] public IDReco reco;

    [Header("Buff组件")] public BuffBaseComponent buffComponent;

    //记录上次发射时间
    protected List<float> listLastCastTime = new List<float>();

    // Start is called before the first frame update
    void Awake()
    {
        if (bulletLauncher != null)
        {
            bulletLauncher.bulletEnvProvider = GetBulletEnvProvider();
        }

        if (null == reco)
        {
            reco = GetComponent<IDReco>();
        }

        if (null == buffComponent)
        {
            buffComponent = GetComponent<BuffBaseComponent>();
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        int nCount = skillIDs.Count;

        float curTime = Time.realtimeSinceStartup;

        //默认上次施法时间
        while (listLastCastTime.Count < nCount)
        {
            listLastCastTime.Add(curTime);
        }

        //默认冷却时间
        while (skillCooldingTime.Count < nCount)
        {
            skillCooldingTime.Add(2);
        }


        int skillID = 0;
        for (int i = 0; i < nCount; ++i)
        {
            skillID = skillIDs[i];

            // skillID = 100041;
            //if (curTime - listCooldingTime[i] >= skillCooldingTime[i])
            if (IsCooling(curTime, listLastCastTime[i], skillCooldingTime[i], skillID) == false)
            {
                if (CanAttack(skillID))
                {
                    //listLastCastTime[i] = curTime;
                    cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillID);
                    OnAttack(skillID);

                    //更改施法CD
                    listLastCastTime[i] = curTime;
                    //if (bulletLauncher)
                    //{
                    //    cfg_BattleSkill cfg = GameGlobal.GameScheme.BattleSkill_0(skillID);
                    //    if (cfg != null && cfg.iBulletGroupID > 0)
                    //    {

                    //        //没有释放成功，直接返回，不扣CD
                    //        if (FireBullet(skillID, cfg.iBulletGroupID) ==false)
                    //        {
                    //            //Debug.LogWarning("发射子弹失败 cfg.iBulletGroupID=" + cfg.iBulletGroupID + "skillID="+skillID);
                    //            continue;
                    //        }
                    //    }

                    //    OnAttack(skillID);

                    //    //更改施法CD
                    //    listLastCastTime[i] = curTime;
                    //}
                }
            }
        }
    }

    public bool DoAttack(int skillID, bool needAct = true)
    {
        float curTime = Time.realtimeSinceStartup;
        if (bulletLauncher)
        {
            cfg_Skill cfg = GameGlobal.GameScheme.Skill_0(skillID);
            //现在不发射子弹了
            /*if (cfg != null && cfg.iBulletGroupID > 0)
            {

                FireBullet(skillID, cfg.iBulletGroupID);
                //bulletLauncher.FireBullet();
            }*/

            //技能攻击
            OnAttack(skillID, needAct);

            //播放施法动作

            return true;
        }

        return false;
    }

    public GameObject GetSkillEffNode(ECreaturePos pos)
    {
        GameObject node = null;
        switch (pos)
        {
            case ECreaturePos.Head:
            {
                node = headSkillEffPos;
            }
                break;
            case ECreaturePos.Center:
            {
                node = bodySkillEffPos;
            }
                break;
            case ECreaturePos.Foot:
            {
                node = footSkillEffPos;
            }
                break;
        }

        if (null == node)
        {
            if (GameIni.Instance.enableDebug)
            {
                if (null != reco)
                {
                    Debug.LogWarning($"entID:{reco.entID}，取不到ECreaturePos：{pos} 特效挂点");
                }
                else
                {
                    Debug.LogWarning($"实体:{transform.gameObject.name}，取不到ECreaturePos：{pos} 特效挂点");
                }
            }

            return transform.gameObject;
        }

        return node;
    }


    //发射子弹
    public virtual bool FireBullet(int skillID, int iBulletGroupID)
    {
        if (null != bulletLauncher)
        {
            //设置发射现场
            cfg_BulletGroup group = GameGlobal.GameScheme.BulletGroup_0(iBulletGroupID);
            if (null == group)
            {
                Debug.LogError("不存在的子弹组 iBulletGroupID=" + iBulletGroupID + ", skillID = " + skillID);
                return false;
            }

            bulletLauncher.count = group.iBulletNum > 1 ? group.iBulletNum : 1;
            bulletLauncher.fireType = (BULLET_FIRE_TYPE)group.iEmitterType;
            bulletLauncher.interval = group.fFireInterval;
            bulletLauncher.AngeleRange = group.fAngelRange;
            bulletLauncher.startDistance = group.fStartDistance;
            bulletLauncher.randomAngle = group.iRandomAngle > 0;

            return bulletLauncher.FireBullet(skillID, group.iBulletID);
        }

        return false;
    }

    //是否正在冷却
    public virtual bool IsCooling(float curTime, float lastCastTime, float cooldingTime, int skillID)
    {
        return (curTime - lastCastTime) < cooldingTime;
    }

    //判断释放可以释放
    public virtual bool CanAttack(int skillID)
    {
        return true;
    }


    //即将释放
    public virtual void OnAttack(int skillID, bool needAct = true)
    {
    }

    //施法动作
    public virtual void OnCastAcrion(int skillID, string aniName)
    {
    }

    public virtual void Reset()
    {
        skillIDs.Clear();
        listLastCastTime.Clear();
        skillCooldingTime.Clear();
    }

    //获取子弹环境提供器
    public virtual IBulletEnvProvider GetBulletEnvProvider()
    {
        return BulletEnvProviderDefault.Instance;
    }
}