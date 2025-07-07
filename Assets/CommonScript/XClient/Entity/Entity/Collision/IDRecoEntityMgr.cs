/*******************************************************************
** 文件名:	IDRecoEntityMgr.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.28
** 版  本:	1.0
** 描  述:	
** 应  用:  所有有身份识别的实体管理器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XGame.Utils;


namespace XClient.Entity
{

    //区域范围类型
    public class  REGION_TYPE
    {
        //单体
        public const int REGION_SHAPE_SINGLE = 0;
        //圆形
        public const int REGION_SHAPE_CIRCLE = 1;
        //扇形
        public const int REGION_SHAPE_FAN = 2;
        //矩形
        public const int REGION_SHAPE_RECT = 3;
        //环形
        public const int REGION_SHAPE_ANNULAR = 4;
    }

    public class IDRecoEntityMgr : Singleton<IDRecoEntityMgr>
    {
        //增加标识系统
        Dictionary<ulong, HashSet<IDReco>> m_dicIDReco = new Dictionary<ulong, HashSet<IDReco>>();

        //身份标识->到阵营的映射
        Dictionary<IDReco, ulong> m_dicIDReco2Camp = new Dictionary<IDReco, ulong>();

        //对外返回结果
        List<IDReco> m_listIDReco = new List<IDReco>();

        //阵营列表
        List<ulong> m_listCamp = new List<ulong>();

        //临时列表
        List<ulong> m_listTempCamp = new List<ulong>();

        //添加身份识别实体
        public void AddIDRecoEntity(IDReco entity)
        {
            ulong camp =0;
            if(m_dicIDReco2Camp.TryGetValue(entity,out camp))
            {
                //同样的阵营,就不用改变了
                if(camp== entity.camp)
                {
                    return;
                }else //移除老的
                {
                    RemoveIDRecoEntity(entity);
                }

            }

            camp = entity.camp;
            m_dicIDReco2Camp.Add(entity, camp);
            HashSet<IDReco> hashIDReco = null;
            if (m_dicIDReco.TryGetValue(camp, out hashIDReco)==false)
            {
                hashIDReco = new HashSet<IDReco>();
                m_dicIDReco.Add(camp, hashIDReco);
            }

            
            if(hashIDReco.Contains(entity)==false)
            {
                hashIDReco.Add(entity);
            }


        }

        //移除身份识别实体
        public void RemoveIDRecoEntity(IDReco entity)
        {
            ulong camp = 0;
            if (m_dicIDReco2Camp.TryGetValue(entity, out camp))
            {
                HashSet<IDReco> hashIDReco = null;
                if (m_dicIDReco.TryGetValue(camp, out hashIDReco))
                {
                    hashIDReco.Remove(entity);
                }

                m_dicIDReco2Camp.Remove(entity);
            }
        }

        //变更阵营
        public void ChangeComp(IDReco entity)
        {
            AddIDRecoEntity(entity);
        }

        //获取阵营列表
        public List<ulong> GetCamps(List<ulong> listFilters)
        {
            m_listCamp.Clear();
            foreach(ulong camp in m_dicIDReco.Keys)
            {
                if(listFilters==null|| listFilters.IndexOf(camp)==-1)
                {
                    m_listCamp.Add(camp);
                }
            }

            return m_listCamp;
        }

        public List<ulong> GetEnemyCamp(ulong camp,List<ulong> listFriendCamps, List<ulong> listEnemyCamps)
        {
            //指定敌方阵营的
            if(null!= listEnemyCamps&& listEnemyCamps.Count>0)
            {
                return listEnemyCamps;
            }

            //没有指定阵营的，自己算阵营
            m_listTempCamp.Clear();

            if(null== listFriendCamps)
            {
                return m_listTempCamp;
            }

            m_listTempCamp.AddRange(listFriendCamps);
            m_listTempCamp.Add(camp);
            return GetCamps(m_listTempCamp);

        }

        //获取阵营实体

        public void GetAllIDRecoByCamp(List<IDReco> listIDReco, ulong camp, int entityType)
        {
            listIDReco.Clear();
            HashSet<IDReco> hashIDReco = null;
            if (m_dicIDReco.TryGetValue(camp, out hashIDReco))
            {
               foreach(IDReco reco in hashIDReco)
                {
                    if(reco.entType== entityType)
                    {
                        listIDReco.Add(reco);   
                    }
                }
            }

        }

        public void  GetIDRecoByCamp(List<IDReco> listIDReco, ulong camp, int entityType, ref Vector3 centerPos, ref Vector3 forward, int rgType, float disX, float disYAndAngle)
        {
            m_listTempCamp.Clear();
            m_listTempCamp.Add(camp);
            GetIDRecoByCamp(listIDReco, m_listTempCamp, entityType, ref centerPos, ref forward, rgType, disX, disYAndAngle);
        }

        public List<IDReco> GetIDRecoByCamp(ulong camp, int entityType, ref Vector3 centerPos, ref Vector3 forward, int rgType, float disX, float disYAndAngle)
        {
            m_listTempCamp.Clear();
            m_listTempCamp.Add(camp);
            GetIDRecoByCamp(m_listIDReco, m_listTempCamp, entityType, ref centerPos, ref forward, rgType, disX, disYAndAngle);
            return m_listIDReco;
        }

        public List<IDReco> GetIDRecoByCamp(List<ulong> listCamp,int entityType,ref Vector3 centerPos,ref Vector3 forward, int rgType, float disX,float disYAndAngle)
        {

            GetIDRecoByCamp(m_listIDReco, listCamp, entityType, ref centerPos, ref forward, rgType, disX, disYAndAngle);
            return m_listIDReco;
        }

        public void GetIDRecoByCamp(List<IDReco> listIDReco,List<ulong> listCamp, int entityType, ref Vector3 centerPos, ref Vector3 forward, int rgType, float disX, float disYAndAngle)
        {

            Vector3 curPos;
            HashSet<IDReco> hashIDReco = null;
            listIDReco.Clear();
            int nCount = listCamp.Count;
            ulong camp = 0;
            for (int i = 0; i < nCount; ++i)
            {
                camp = listCamp[i];
                if (m_dicIDReco.TryGetValue(camp, out hashIDReco))
                {
                    foreach (IDReco entity in hashIDReco)
                    {
                        //返回可以被攻击的
                        if (null != entity && entity.beAttack && (entity.entType == entityType || entityType < 0))
                        {
                            curPos = entity.transform.position;
                            if (disX > Mathf.Abs(centerPos.x - curPos.x))
                            {

                                switch (rgType)
                                {
                                    //圆形
                                    case REGION_TYPE.REGION_SHAPE_SINGLE:
                                    case REGION_TYPE.REGION_SHAPE_CIRCLE:
                                        if (disX > Vector3.Distance(centerPos, curPos))
                                        {
                                            listIDReco.Add(entity);
                                        }
                                        break;
                                    //扇形
                                    case REGION_TYPE.REGION_SHAPE_FAN:
                                        if (disX > Vector3.Distance(centerPos, curPos))
                                        {
                                            //计算角度
                                            Vector3 t = (curPos - centerPos).normalized;
                                            float cos = Vector3.Dot(t, forward);
                                            float angle = Mathf.Acos(cos) * Mathf.Rad2Deg;
                                            if (angle < disYAndAngle)
                                            {
                                                listIDReco.Add(entity);
                                            }

                                        }
                                        break;
                                    //正规矩形
                                    case REGION_TYPE.REGION_SHAPE_RECT:
                                        {
                                            if (disYAndAngle > Mathf.Abs(centerPos.z - curPos.z))
                                            {
                                                listIDReco.Add(entity);
                                            }
                                        }
                                        break;
                                    //环形
                                    case REGION_TYPE.REGION_SHAPE_ANNULAR:
                                        float dis = Vector3.Distance(centerPos, curPos);
                                        if (disYAndAngle >= dis && dis >= disX)
                                        {
                                            listIDReco.Add(entity);
                                        }
                                        break;
                                    default:
                                        Debug.Log("不支持 区域伤害类型：" + rgType);
                                        break;
                                }

                            }

                        }
                    }
                }

            }
        }


    }
}

