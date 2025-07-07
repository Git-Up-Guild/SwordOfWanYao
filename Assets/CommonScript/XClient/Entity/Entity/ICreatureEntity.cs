/*******************************************************************
** 文件名:	ICreatureEntity.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.26
** 版  本:	1.0
** 描  述:	
** 应  用:  生物实体接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity.Net;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    //生物的可见方式
    public enum CREATURE_VISIBLE_TYPE
    {
        VISIBLE_TYPE_ALL, //所有可见
        VISIBLE_TYPE_LOCAL, //本地可见
        VISIBLE_TYPE_REMOTE, //对方可见
    }

    public enum ECreaturePos
    {
        Head = 1,           //头
        Center = 2,         //身
        Foot = 3,           //脚
    }

    public interface IATTRModifier
    {
        //修改速度
        float OnModifiedSpeed(ICreatureEntity entity, float baseSpeed);

        //修改血量
        int OnModifiedHP(ICreatureEntity entity, int hp);

        //修改int 属性
        int OnModifiedIntProp(int propID, int val);

        //坐标转换
        public Vector3 WorldPositionToBattlePosition(Vector3 worldPosition, bool isMyBattle);
    }

    public interface ICreatureEntity : IVisibleEntity
    {
        //获取阵营
        ulong GetCamp();

        //获取位置
        Vector3 GetPos();
        Vector3 GetLocalPos();

        //获取朝向
        Vector3 GetForward();

        //设置2d左右朝向
        void SetFace(bool faceLeft);
        void SetRotation(Quaternion rotate);

        void SetParent(Transform parent);

        //设置位置
        void SetPos(ref Vector3 pos);
        void SetLocalPos(ref Vector3 localPos);

        //设置朝向
        void SetForward(ref Vector3 forward);

        //获取实体上的组件
        T GetComponent<T>() where T : class;

        //获取所有组件
        void GetComponents<T>(List<T> list) where T : class;

        //是否死亡
        bool IsDie();

        //获取血量
        int GetHP();

        /// <summary>
        /// 获取最大血量
        /// </summary>
        /// <returns></returns>
        int GetMaxHP();

        /// <summary>
        /// 设置最大血量
        /// </summary>
        /// <param name="maxHp"></param>
        void SetMaxHP(int maxHp);

        //设置血量
        void SetHPDelta(int hp);

        //获取移动速度
        float GetSpeed();

        //设置速度
        void SetSpeed(float speed);

        //设置int属性
        int GetIntAttr(int propID);

        //设置Int属性
        void SetIntAttr(int propID, int val);

        //设置属性修改器
        void SetATTRModifier(IATTRModifier modifier);

        //获取显示方式
        CREATURE_VISIBLE_TYPE GetVisibleType();
    }
}