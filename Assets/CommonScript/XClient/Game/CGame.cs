/*******************************************************************
** 文件名:	CGame.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程 
** 日  期:	2020/10/30
** 版  本:	1.0
** 描  述:	游戏对象
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using UnityEngine;
using XClient.Common;
using XClient.Client;
using XClient.Client.Scheme;
using XGame;
using XGame.EventEngine;
using XGame.Timer;
using XGame.Trace;
using XGame.UnityObjPool;
using XGame.Utils;
using XGame.Asset;
using XGame.Preload;
using XGame.EtaMonitor;
using XGame.Asset.Load;
using XGame.EcoMode;
using XClient.Game.Lua;
using XGame.Audio;
using XClient.GameInit;
using XGame.Cam;
using XClient.Network;
using System.Collections.Generic;
using XGame.UI.Framework;
using XGame.Reddot;
using System.Collections.Generic;
using XClient.Reddot;

namespace XClient.Game
{
    // 游戏主体,管理所有游戏相关模块,单实例
    public class CGame : MonoBehaviourEX<CGame>, IGame,
        IEventExecuteSink, IGameComAndModule, ILowPowerSink, ILockScreenSink
    {
        private float m_LastTickUpdate = 0;
        private float m_LastTickLateUpdate = 0;
        private float m_LastTickFixedUpdate = 0;

        private Transform m_transform = null;

        /// <summary>
        /// 游戏初始化配置
        /// </summary>
        [SerializeField]
        private GameInitConfig m_GameInitConfigs;
        public GameInitConfig GameInitConfig
        {
            get
            {
                return m_GameInitConfigs;
            }
            set
            {
                m_GameInitConfigs = value;
            }
        }

        //初始化完成标志
        public bool IsInit { get; private set; }

        //退出时候的回调
        public Action<int> m_destroyCallbck = null;

        private ICom[] m_lsComs;
        private IModule[] m_lsModules;

        // 游戏流程状态
        GameStateManager m_gameStateManager = null;

        public IGameStateManager GameStateManager => m_gameStateManager;

        //全局游戏Transform对象
        public Transform GlobalTransform { get { return m_transform; } }

        //日志模块
        private ITraceModule m_tracer;
        public ITraceModule TraceModule
        {
            get { return Get<ITraceModule>(ref m_tracer); }
        }

        //加载器，测试打印加载列表
        private IGAssetLoader m_assetLoader;

        public IGAssetLoader GAssetLoader { get { return Get<IGAssetLoader>(ref m_assetLoader); } }

        //对象池
        private IUnityObjectPool m_unityObjectPool;
        public IUnityObjectPool UnityObjectPool { get { return Get<IUnityObjectPool>(ref m_unityObjectPool); } }

        // 定时器管理器
        private ITimerManager m_timerManager;
        public ITimerManager TimerManager { get { return Get<ITimerManager>(ref m_timerManager); } }

        // 获得事件引擎
        private IEventEngine m_eventEngine;
        public IEventEngine EventEngine { get { return Get<IEventEngine>(ref m_eventEngine); } }

        public ILuaEngine LuaEngine { get; set; }

        private T Get<T>(ref T t) where T : class, ICom
        {
            if (t == null)
            {
                t = XGame.XGameComs.Get<T>();
            }
            return t;
        }

        //网络组建
        public IGameNetCom NetCom
        {
            get
            {
                NetModule netModule = this.NetModule as NetModule;
                return netModule.NetCom;
            }
        }

        // 脚本管理
        public ISchemeModule SchemeModule { get { return m_lsModules[DModuleID.MODULE_ID_SCHEME] as ISchemeModule; } }

        public INetModule NetModule { get { return m_lsModules[DModuleID.MODULE_ID_NET] as INetModule; } }

        public ILua Lua { get; private set; }

        //组件激活
        public override void Awake()
        {
            base.Awake();

            ILuaProxyMono xLuaProxyMono = GetComponent<ILuaProxyMono>();
            if (xLuaProxyMono != null)
            {
#if SUPPORT_XLUA
                Lua = xLuaProxyMono.CreateLua();
                Lua.Initialize();
#endif
                DestroyImmediate(xLuaProxyMono);
            }
            else
            {
#if SUPPORT_XLUA
                Debug.LogError("XLUA初始化失败！没有找到代理组件！！");
#endif
            }

            GameGlobal.SetGame(this);
            GameGlobal.SetGameComAndMudule(this);
            IsInit = false;

            m_lsComs = new XGame.ICom[(int)DComID.MAX];
            for (int i = DComID.MIN; i < DComID.MAX; ++i)
            {
                m_lsComs[i] = null;
            }

            m_lsModules = new IModule[DModuleID.MAX];
            for (int i = DModuleID.MIN; i < DModuleID.MAX; ++i)
            {
                m_lsModules[i] = null;
            }

            m_transform = transform;

            //初始化
            Init();
        }

        /// <summary>
        /// 焦点变更
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
            Lua?.OnApplicationFocus(focus);
        }

        void Start()
        {
            //等到所有的脚本都加载完了，在启动lua
            (SchemeModule as CSchemeModule).AddSchemeLoadFinishedCallback(() =>
            {
                var mgr = XGameComs.Get<IReddotManager>();
                if (mgr != null)
                {
                    List<object> configs = new List<object>();
                    var scheme = SchemeModule.GetCgamescheme();
                    var num = scheme.Reddot_nums();
                    HashSet<int> usedIDs = new HashSet<int>();

                    for (var i = 0; i < num; i++)
                    {
                        var config = scheme.Reddot(i);
                        if (usedIDs.Contains(config.iID))
                        {
                            Debug.LogWarning($"跳过重复红点配置 ID={config.iID}");
                            continue;
                        }

                        usedIDs.Add(config.iID);
                        configs.Add(config);
                    }

                    mgr.SetupConfigs<ReddotConfigData>(configs);
                }
                else
                {
                    Debug.LogWarning("IReddotManager 获取失败！");
                }

                // 无论如何都进入登录状态
                m_gameStateManager.EnterLogin();
            });
        }

        /// <summary>
        /// 初始化系统模块
        /// </summary>
        private void Init()
        {
            //先创建所有的组件
            m_lsComs[DComID.COM_ID_GAME_SCHEME] = new GameSchemeCom();

            //组件要先运行起来
            XGame.ICom com = null;
            for (int i = DComID.MIN; i < DComID.MAX; ++i)
            {
                com = m_lsComs[i];
                if (com != null)
                {
                    com.ID = i;
                    com.Create(this, null);
                    com.Start();
                }
            }

            //游戏状态管理
            m_gameStateManager = new GameStateManager();

            Create();

            //创建基础模块
            ModuleSetup.SetupModules(m_lsModules);

            //创建外部扩展模块
            IGameExtensions gameExtensions = GetComponent<IGameExtensions>();
            gameExtensions?.OnSetupExtenModules(m_lsModules);

            //创建模块
            for (int i = DModuleID.MIN; i < DModuleID.MAX; ++i)
            {
                IModule module = m_lsModules[i];
                if (module != null)
                {
                    module.ID = i;
                    module.Create(this, null);
                    module.Start();
                }
            }

            //模块的扩展初始化
            gameExtensions?.OnAfterSetupExtenModules();

            IsInit = true;

            SubStateChange();

            //加载模块使用网络模块，判断网络是否通
            if (GAssetLoader is IAssetBundleLoadManager)
                (GAssetLoader as IAssetBundleLoadManager).SetNetworkSink(m_lsModules[DModuleID.MODULE_ID_NET] as ICheckNetworkSink);

            //进入游戏默认加载场景
            if (GameInitConfig.gameStateBuildinSceneIndex >= 0 || string.IsNullOrEmpty(GameInitConfig.gameScenePath) == false)
                GameSceneManager.Instance.Setup(GameInitConfig.gameStateBuildinSceneIndex, GameInitConfig.gameScenePath);

            //相机跟随设置
            if (GameInitConfig.cameraFollowSettings.isEnable)
                CameraControllerManager.Instance.Setup(GameInitConfig.Instance.cameraFollowSettings.camreaFollowMode,
                   GameInitConfig.Instance.cameraFollowSettings.positionOffset);

        }

        private void SubStateChange()
        {
            EventEngine.Subscibe(this, DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE, (byte)DEventSourceType.SOURCE_TYPE_SYSTEM, 0, "Game State Change");
            EventEngine.Subscibe(this, DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE, (byte)DEventSourceType.SOURCE_TYPE_SYSTEM, 0, "Game State Change");
        }

        /// <summary>
        /// 获取一个组件
        /// </summary>
        /// <param name="nID"></param>
        /// <returns></returns>
        public XGame.ICom GetCom(int nID)
        {
            if (nID < DComID.MIN || nID >= DComID.MAX)
            {
                Debug.LogError("错误的组件ID");
                return null;
            }
            return m_lsComs[(int)nID];
        }

        /// <summary>
        /// 获取一个模块
        /// </summary>
        /// <param name="nModuleID"></param>
        /// <returns></returns>
        public IModule GetModule(int nModuleID)
        {
            if (nModuleID < DModuleID.MIN || nModuleID >= DModuleID.MAX)
            {
                Debug.LogError("错误的模块ID");
                return null;
            }
            return m_lsModules[(int)nModuleID];
        }

        /// <summary>
        /// 创建系统模块
        /// </summary>
        public void Create()
        {
            //先创建游戏客户端
            m_gameStateManager.Create();

            // 启动游戏状态加载与管理
            m_gameStateManager.Start();

            IEcoMode ecoMode = XGame.XGameComs.Get<IEcoMode>();
            if (ecoMode != null)
            {
                ecoMode.AddLockScreenSink(this);
                ecoMode.AddLowPowerSink(this);
            }
        }

        /// <summary>
        /// 模块被销毁的时候，这里同步
        /// </summary>
        /// <param name="moduleID"></param>
        public void OnModuleReleased(int moduleID)
        {
            if (moduleID <= 0 || moduleID >= DModuleID.MAX)
            {
                TRACE.ErrorLn("OnModuleReleased>> 错误的模块ID：" + moduleID);
                return;
            }

            IModule module = m_lsModules[moduleID];
            if (module != null)
            {
                m_lsModules[moduleID] = null;
            }
        }

        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param = 0)
        {
            for (int i = DModuleID.MAX - 1; i >= DModuleID.MIN; --i)
            {
                IModule module = m_lsModules[i];
                if (module != null)
                {
                    module.Clear(param);
                }
            }
        }

        public void Stop()
        {
            //先停止模块
            StopModules();

            //然后销毁模块
            ReleaseModules();

            //销毁游戏状态管理
            if (m_gameStateManager != null)
            {
                m_gameStateManager.Stop();
                m_gameStateManager = null;
            }

            //停止组件
            StopComs();

        }

        private void StopModules()
        {
            IModule module;
            for (int i = 0; i < DModuleID.MAX - 1; ++i)
            {
                module = m_lsModules[i];
                if (module != null)
                {
                    module.Stop();
                }
            }
        }

        private void ReleaseModules()
        {
            IModule module;
            for (int i = 0; i < DModuleID.MAX - 1; ++i)
            {
                module = m_lsModules[i];
                if (module != null)
                {
                    module.Release();
                    m_lsModules[i] = null;
                }
            }
        }

        private void StopComs()
        {
            XGame.ICom com = null;
            for (int i = DComID.MIN; i < DComID.MAX; ++i)
            {
                com = m_lsComs[i];
                if (com != null)
                {
                    com.Stop();
                }
            }
        }

        private void ReleaseComs()
        {
            XGame.ICom com = null;
            for (int i = DComID.MIN; i < DComID.MAX; ++i)
            {
                com = m_lsComs[i];
                if (com != null)
                {
                    com.Release();
                }
            }
            m_lsComs = null;
        }

        //销毁
        public void Release()
        {
            GameSceneManager.Instance.Clear();

            //销毁游戏状态管理
            if (m_gameStateManager != null)
            {
                m_gameStateManager.Release();
                m_gameStateManager = null;
            }

            //销毁组件
            ReleaseComs();

            UnSubStateChange();

        }

        private void UnSubStateChange()
        {
            EventEngine.UnSubscibe(this, DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE, (byte)DEventSourceType.SOURCE_TYPE_SYSTEM, 0);
            EventEngine.UnSubscibe(this, DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE, (byte)DEventSourceType.SOURCE_TYPE_SYSTEM, 0);
        }

        public void OnRoleDataReady()
        {
            foreach (var m in m_lsModules)
            {
                m?.OnRoleDataReady();
            }
        }

        public void OnDestroy()
        {
            GameSceneManager.Instance.Clear();

            if (null != m_destroyCallbck)
                m_destroyCallbck(0);

            //xlua先做处理
            Lua?.WillDispose();

            Stop();
            Release();

            Lua?.Dispose();
        }

        //每帧渲染时调用
        public void Update()
        {
            if (m_lsComs != null)
            {
                int count = m_lsComs.Length;
                for (int i = 0; i < count; i++)
                {
                    if (m_lsComs[i] != null)
                    {
                        m_lsComs[i].Update();
                    }
                }
            }

            //需要的模块自己开Update 使用 帧更新  FrameUpdatMgr
            /*
            if (m_lsModules != null)
            {
                int modulesCount = m_lsModules.Length;
                for (int j = 0; j < modulesCount; j++)
                {
                    if (m_lsModules[j] != null)
                    {
                        m_lsModules[j].Update();
                    }
                }
            }*/


            // 降低刷新频率 
            float fTime = Time.realtimeSinceStartup;
            if (fTime - m_LastTickUpdate < 0.05f)
            {
                return;
            }
            m_LastTickUpdate = fTime;

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.L))
            {
                bool flag = XGame.Trace.TRACE.IsQuiet();
                XGame.Trace.TRACE.SetQuite(!flag);
            }
#endif

            //网络对象
            NetworkManager.Instance.Update();
        }

        //最后更新
        public void LateUpdate()
        {
            // 降低刷新频率 
            float fTime = Time.realtimeSinceStartup;
            if (fTime - m_LastTickLateUpdate < 0.05f)
            {
                return;
            }
            m_LastTickLateUpdate = fTime;
        }

        //固定时间调用，与图像渲染分离
        public void FixedUpdate()
        {
            // 尽快处理网络层队列，每次处理少量,尽量避免网络阻塞，导致一次性处理太多，导致卡顿。
            // 降低刷新频率 
            float fTime = Time.realtimeSinceStartup;
            if (fTime - m_LastTickFixedUpdate < 0.05f)
            {
                return;
            }
            m_LastTickFixedUpdate = fTime;

#if OpenDebugInfo_Profiler
            XGame.Utils.ProfilerUtil.PP_BY_NAME_START("CTimerAxis.Dispatch()");
#endif
            TimerManager.Dispatch(20);
#if OpenDebugInfo_Profiler
            XGame.Utils.ProfilerUtil.PP_BY_NAME_STOP();
#endif

        }

        //程序暂停
        public void OnApplicationPause(bool pause)
        {
        }

        //程序退出
        public void OnApplicationQuit()
        {
            //退出时候强制关闭一下网络连接，这样服务器可以马上获取网络连接断开的通知。
            //否则，要过几分钟的超时时间，服务器才会检测到连接已经断开了。
            NetModule?.Release();
            //XGame.XGameComs.Get<IFlowTextManager>()?.Release();
        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            switch (wEventID)
            {
                //切换游戏状态
                case DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE:
                    GameStateChangeEventContext stateData = (GameStateChangeEventContext)pContext;
                    IPreloader preloader = XGame.XGameComs.Get<IPreloader>();

                    //通知模块状态切换
                    foreach (var m in m_lsModules)
                        m?.OnGameStateChange(stateData.nNewState, stateData.nOldState);

                    if ((int)GameState.Login == stateData.nNewState)
                    {
                        preloader.UnloadGroup(1);
                        preloader.UnloadGroup(2);
                        preloader.LoadGroup(0, null);
                    }
                    else if ((int)GameState.Game == stateData.nNewState)
                    {
                        preloader.UnloadGroup(1);
                        preloader.UnloadGroup(0);
                        preloader.LoadGroup(2, null);

                        OnPreEnterGameState();
                    }
                    break;

                case DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE:
                    {
                        var ctx = pContext as GameStateChangeEventContext;
                        UIWindowManager.Instance?.OnLeaveState(ctx.nOldState);

                        if (ctx.nNewState == (int)GameState.Login && ctx.nOldState != (int)GameState.None)
                            UIWindowManager.Instance?.OnExitGame();

                        UIWindowManager.Instance?.OnEnterState(ctx.nNewState);
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnPreEnterGameState()
        {
            m_gameStateManager.gameState.AddReadyStateValidator((state) =>
            {
                return true;

            }, XGame.State.ReadyStateValidatorWorkMode.First);
        }

        public MonitorBase GetEtaMonitor(int id)
        {
            return GameApp.Instance.GetMonitor(id);
        }

        public void OnLockScreen()
        {
            Debug.LogWarning("进入了锁屏，这里进行对应的处理！！");
        }

        public void OnUnLockScreen()
        {
            //Debug.LogWarning("退出了锁屏，这里进行对应的处理！！");
        }

        public void OnEnterLowPower()
        {
            //进行一些设置：比如画质、模型数量、光效数量等！！
            //Debug.LogWarning("进入了省电模式，这里进行对应的处理！！");
        }

        public void OnLeaveLowPower()
        {
            //进行一些恢复：比如画质、模型数量、光效数量等！！
            //Debug.LogWarning("离开了省电模式，这里进行对应的处理！！");
        }

        // 服务器时间
        private long _startServerTick;
        private long _startClientTick;

        /// <summary>
        /// 服务器时间，毫秒
        /// </summary>
        public long StartServerTick
        {
            get => _startServerTick;
            set
            {
                var curTick = CurClientGameTime;
#if DEBUG_LOG
///#                ///#                //Debug.Log($"设置服务器时间：{_startServerTick} -> {value}, 客户端时间：{_startClientTick} -> {curTick}");
#endif
                _startClientTick = curTick;
                _startServerTick = value;
            }
        }
        /// <summary>
        /// 当前时间，精确到毫秒
        /// </summary>
        public long CurClientGameTime { get => (long)(Time.realtimeSinceStartup * 1000); }

        /// <summary>
        /// 进入场景服开始时间，毫秒
        /// </summary>
        public long CurServerTick
        {
            get
            {
                var curTick = CurClientGameTime;
                var curServerTick = curTick - _startClientTick + _startServerTick;
#if DEBUG_LOG
///#                ///#                //Debug.Log($"获取当前服务器时间Tick：{curServerTick}, 客户端时间：{curTick}, 服务器开始时间：{_startServerTick}, 客户端开始时间：{_startClientTick} ,客户端经过时间：{ (curTick - _startClientTick) / 1000}");
#endif
                return curServerTick;
            }
        }

        public IAudioCom AudioCom => XGameComs.Get<IAudioCom>();
    }
}
