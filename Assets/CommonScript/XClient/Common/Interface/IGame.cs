/*******************************************************************
** 文件名:	IGame.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	宋文武 (sww8@163.com)
** 日  期:	2016-02-01
** 版  本:	1.0
** 描  述:	Game接口文件
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using XGame.EventEngine;
using XGame.Timer;
using XGame.FlowText;
using XGame.UnityObjPool;
using XGame.EtaMonitor;
using XGame;
using XGame.Audio;
using XClient.GameInit;

namespace XClient.Common
{
    /// <summary>
    /// 组件和模块获取接口
    /// </summary>
    public interface IGameComAndModule
    {
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        ICom GetCom(int ID);

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        IModule GetModule(int ID);
    }

    public interface IGame
    {
        //获取一个组件
        XGame.ICom GetCom(int nID);

        /// 获取一个模块
        IModule GetModule(int nModuleID);

        //全局游戏Transform对象
        Transform GlobalTransform { get; }

        //获取游戏加载控制器
        IGameStateManager GameStateManager { get; }

        //对象池
        IUnityObjectPool UnityObjectPool { get; }

        // 定时器管理器
        ITimerManager TimerManager { get; }

        // 获得事件引擎
        IEventEngine EventEngine { get; }

        //脚本配置模块
        ISchemeModule SchemeModule { get; }

        //网络模块
        INetModule NetModule { get; }

        //声音模块
        IAudioCom AudioCom { get; }

        /// <summary>
        /// 进入场景服开始时间(服务器开启时间)，毫秒
        /// </summary>
        long StartServerTick { get; set; }

        /// <summary>
        /// 当前客户端时间，毫秒
        /// </summary>
        long CurClientGameTime { get; }

        /// <summary>
        /// 获取Lua引擎
        /// </summary>
        ILuaEngine LuaEngine { get; set; }

        /// <summary>
        /// 当前服务器时间，毫秒
        /// </summary>
        long CurServerTick { get; }

        /// <summary>
        /// 获取Eta监控器
        /// </summary>
        /// <returns></returns>
        MonitorBase GetEtaMonitor(int id);

        /// <summary>
        /// 游戏初始化设置
        /// </summary>
        GameInitConfig GameInitConfig { get; }
		
		//断线重连的时候清理数据调用
        void Clear(int param = 0);
    };
}


