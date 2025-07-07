/*******************************************************************
** 文件名:	IModule.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	宋文武 (sww8@163.com)
** 日  期:	2016-01-20
** 版  本:	1.0
** 描  述:	模块基类
** 应  用:  子系统模块需要继承IModule接口，如果需要获得Update或者FixedUpdate相关驱动，
**          需要重写相关方法。然后通过Game的RegisterModuleEvent方法可注册相关驱动事件。
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections;

namespace XClient.Common
{
	/// <summary>
	/// 模块状态
	/// </summary>
	public enum ModuleState
	{
		None = 0,
		Create,             // 创建中
		Loading,            // 加载中
		Success,            // 加载成功
		Fail,               // 加载失败
		Close,              // 已释放
		MAX,                // 最大值
	};

	/// <summary>
	/// 模块基类
	/// 注释:模块初始化步骤划分为两个阶段，创建成功(Create方法返回成功)和加载成功(模块内部设置ModuleLoadState = Success)。
	/// 如果是同步模块，创建成功时加载也成功。
	/// 如果是异步模块，创建成功并不保证加载也成功。
	/// </summary>
	public interface IModule : XGame.ICom
	{
		/// <summary>
		/// 模块名称(模块实现者不用设置)
		/// </summary>
		string ModuleName { get; set; }

		/// <summary>
		/// 异步模块的加载状态(注释:异步模块专用)
		/// </summary>
		ModuleState State { get; set; }

		/// <summary>
		/// 异步模块加载的进度,范围(0.0f,1.0f)(注释:异步模块专用)
		/// </summary>
		float Progress { get; set; }

        /// <summary>
        /// 游戏状态变化回调
        /// </summary>
        /// <param name="newStateID"></param>
        /// <param name="oldStateID"></param>
        void OnGameStateChange(int newStateID, int oldStateID);

		/// <summary>
		/// 每逻辑帧
		/// </summary>
		void FixedUpdate();

		/// <summary>
		/// LateUpdate更新
		/// </summary>
		void LateUpdate();

        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        void Clear(int param);

		/// <summary>
		/// 玩家数据准备好后回调
		/// </summary>
		void OnRoleDataReady();
    }
}
