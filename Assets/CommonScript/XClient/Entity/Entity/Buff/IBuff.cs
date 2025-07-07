/*******************************************************************
** 文件名:	IBuff.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  Buff相关的接口

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Poolable;

namespace XClient.Entity
{

    //创建效果的现场
    public class CreateEffectContext
    {
        //全局实体对象
        public static CreateEffectContext Instance = new CreateEffectContext();

        //buff组件的所属实体的UID
        public ulong srcID;

        //效果配置现场
        public EffectCmdContext effectCmdContext;
    }

    //技能命令现场
    public class EffectCmdContext
    {
        public string cmd;
        public List<int> param = new List<int>();
    }

    //效果创建接口
    public interface IEffectActionCreate
    {
        //创建
        IEffectAction CreateEffectAction(string id, object context);

        //释放对象
        void ReleaseEffectAction(IEffectAction action);

        //通过BuffID，获取已经解析过的effect现场
        List<EffectCmdContext> GetEffectCmdContexts(int buffID,string commandBuffList);
    }
    

    //Buff的创建现场
    public class BuffCreateContext
    {
        //拥有者
        public ulong srcID;

        //buff的ID
        public int buffID;
        //buff的层数
        public int buffLayer;

        //clientID
        public long clientID;

        //效果创建器
        public IEffectActionCreate effectActionCreate;

    }



    //buff接口
    public interface IBuff:IPoolable 
    {
        //开始
        void Start();

        //停止
        void Stop();

        //获取BuffID
        int GetBuffID();

        //获取层
        int GetLayer();

        //设置层
        void SetLayer(int layer);

        //释放已经完成
        bool IsFinish();

        //推动更新
        void OnUpdate();

        //设置一个SID
        void SetSID(int sid);

        //获取sid
        int GetSID();

        //设置添加buff的客户端
        void SetClientMaster(long clientID);

        //获取添加的客户端
        long GetClientMaster();
    }
}

