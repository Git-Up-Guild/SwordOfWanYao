/*******************************************************************
** 文件名:	GlobalGame.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	宋文武 (sww8@163.com)
** 日  期:	2016-01-20
** 版  本:	1.0
** 描  述:	存储全局IGame指针,方便其他模块调用IGame的相关接口。
 *			比如想要获取网络模块接口:
 *			INetManager netManager = GlobalGame.Instance.NetManager;
 *			netManager.DoSometing();
** 应  用:  所有模块都必须通过GlobalGame类间接访问IGame服务。不能直接引用Game命名
 *			空间，通过CGame获取模块接口。因为这样会出现交叉引用的问题，后期做代码
 *			动态更新会出现问题。
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using XClient.Entity;
using XClient.Login;
using XClient.Net;
using XClient.Reddot;
using XClient.RPC;
using XGame;
using XGame.Atlas;
using XGame.Audio;
using XGame.Command;
using XGame.Entity;
using XGame.EventEngine;
using XGame.FlowText;
using XGame.FlyEffect;
using XGame.LightingEff;
using XGame.Timer;

namespace XClient.Common
{
    public class GameGlobal
    {
        public static IGame Instance = null;
        public static IGameComAndModule ComAndModule = null;
        private static float s_timeScale = 1f;
        private static float s_invTimeScale = 1f;
        public static float TimeScale => s_timeScale;
        public static float invTimeScale => s_invTimeScale;

        public static void SetTimeScale(float timeScale)
        {
            s_timeScale = timeScale;
            s_invTimeScale = 0 == timeScale ? 0 : 1 / timeScale;
            Time.timeScale = timeScale;
        }

        /// <summary>
        /// 设置全局IGame指针,该方法由IGame的实现类的初始化方法调用。
        /// </summary>
        /// <param name="game"></param>
        public static void SetGame(IGame game)
        {
            Instance = game;
        }

        /// <summary>
        /// 设置组件和模块的获取接口
        /// </summary>
        /// <param name="game"></param>
        public static void SetGameComAndMudule(IGameComAndModule game)
        {
            ComAndModule = game;
        }

        /// <summary>
        /// 表格读取器
        /// </summary>
        public static Igamescheme GameScheme =>
            (Instance.GetModule(DModuleID.MODULE_ID_SCHEME) as ISchemeModule)?.GetCgamescheme();

        /// <summary>
        /// 光效模块
        /// </summary>
        public static ILightingEffectManager LightingEffectManager => XGameComs.Get<ILightingEffectManager>();

        /// <summary>
        /// 音效处理模块
        /// </summary>
        public static IAudioCom AudioCom => XGameComs.Get<IAudioCom>();

        /// <summary>
        /// 飞行效果
        /// </summary>
        public static IFlyEffectManager FlyEffectManager => XGame.XGameComs.Get<IFlyEffectManager>();

        /// <summary>
        /// 时间服务
        /// </summary>
        public static IEventEngine EventEgnine => Instance.EventEngine;

        /// <summary>
        /// 玩家对象
        /// </summary>
        public static RoleEntity Role
        {
            get
            {
                if (ComAndModule == null)
                    return null;
                return (ComAndModule.GetModule(DModuleID.MODULE_ID_ENTITY) as IEntityModule)?.role as RoleEntity;
            }
        }

        /// <summary>
        /// 玩家代理实体对象
        /// </summary>
        public static AgentEntity RoleAgent =>
            (ComAndModule.GetModule(DModuleID.MODULE_ID_AGENT) as IAgentModule)?.roleAgnent?.entity;

        /// <summary>
        /// 本地实体对象ID生成器：房间模式下使用房间RoleID作为主ID，否则登录角色SID作为主ID
        /// </summary>
        public static EntityIDGenerator LocalEntityIDGenerator => EntityWorld.LocalEntityIDGenerator;

        /// <summary>
        /// 登录模块
        /// </summary>
        public static ILoginModule LoginModule => (ComAndModule.GetModule(DModuleID.MODULE_ID_LOGIN) as ILoginModule);

        /// <summary>
        /// 网络模块
        /// </summary>
        public static INetModule NetModule => (ComAndModule.GetModule(DModuleID.MODULE_ID_NET) as INetModule);

        /// <summary>
        /// 远程调用模块
        /// </summary>
        public static IRPCModule RPC => (ComAndModule.GetModule(DModuleID.MODULE_ID_RPC) as IRPCModule);


        private static GameObject tempCacheObjRoot;
        public static GameObject TempCacheObjRoot
        {
            get
            {
                if (tempCacheObjRoot == null)
                {
                    tempCacheObjRoot = new GameObject("TempCacheObjRoot");
                    tempCacheObjRoot.BetterSetActive(false);
                    GameObject.DontDestroyOnLoad(tempCacheObjRoot);
                }
                return tempCacheObjRoot;
            }
        }

        /// <summary>
        /// 实体模块
        /// </summary>
        public static IEntityModule EntityModule =>
            (ComAndModule.GetModule(DModuleID.MODULE_ID_ENTITY) as IEntityModule);

        /// <summary>
        /// 实体世界
        /// </summary>
        public static IEntityWorld EntityWorld => XGameComs.Get<IEntityWorld>();

        /// <summary>
        /// 图集管理器
        /// </summary>
        public static ISpriteAtlasManager SpriteAtlasManager => XGameComs.Get<ISpriteAtlasManager>();

        /// <summary>
        /// 飘字管理器
        /// </summary>
        public static IFlowTextManager FlowTextManager => XGameComs.Get<IFlowTextManager>();

        /// <summary>
        /// 房间模块
        /// </summary>
        public static IRoomModule Room => (ComAndModule.GetModule(DModuleID.MODULE_ID_ROOM) as IRoomModule);

        /// <summary>
        /// 网络传输
        /// </summary>
        public static INetTransferModule NetTransfer =>
            (ComAndModule.GetModule(DModuleID.MODULE_ID_NET_TRANSFER)) as INetTransferModule;

        /// <summary>
        /// 定时器
        /// </summary>
        public static ITimerManager Timer => XGameComs.Get<ITimerManager>();

        /// <summary>
        /// 命令队列
        /// </summary>
        public static ICommandManager CommandManager => XGameComs.Get<ICommandManager>();

        /// <summary>
        /// 系统命令队列
        /// </summary>
        public static ICommandQueue SystemCommandQueue => XGameComs.Get<ICommandManager>().systemQueue;

        /// <summary>
        /// 代理模块
        /// </summary>
        public static IAgentModule Angent => (ComAndModule.GetModule(DModuleID.MODULE_ID_AGENT) as IAgentModule);

        /// <summary>
        /// 红点模块
        /// </summary>
        public static IReddotModule Reddot => (ComAndModule.GetModule(DModuleID.MODULE_ID_REDDOT) as IReddotModule);

        /// <summary>
        /// 显示系统提示
        /// </summary>
        /// <param name="tip"></param>
        public static void ShowSystemTips(string tip)
        {
            FlowTextManager.AddFlowText(1, tip, 0, 0);
        }
    }
}