/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: IRoleModule.cs 
Module: Role
Author: 郑秀程
Date: 2024.06.17
Description: 玩家数据模块
***************************************************************************/

using XClient.Common;
using XGame.Entity;

namespace XClient.Entity
{

    //物品创建通知现场
    public class EntityCreateNtfContext
    {
        public int configID;
        public int num;
        public ulong sid;
    }

    /// <summary>
    /// 角色模块
    /// </summary>
    public interface IEntityModule : IModule
    {
        /// <summary>
        /// 玩家对象
        /// </summary>
        IEntity role { get; }

        //设置主角属性
        void SetRoleIntProp(int id, int value);

        //读取主角属性
        int GetRoleIntProp(int id);

        void SEND_MSG_ENTITY_RENAME_REQ(string name);

    }
}