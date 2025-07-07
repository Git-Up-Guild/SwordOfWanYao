/*******************************************************************
** 文件名:	GameApp.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程 
** 日  期:	2019/10/30
** 版  本:	1.0
** 描  述:	负责 GameApp 的初始化 和相关组件的创建，以及将游戏逻辑模块的加载创建
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using XClient.Common;
using System;

using XGame.CoroutinePool;
using XGame.Poolable;
using XGame;
using XGame.Asset;
using Game;
using XGame.Trace;
using XGame.Utils;
using XGame.UnityObjPool;
using XGame.EventEngine;
using XGame.Timer;
using XGame.Quality;
using XGame.FlowText;
using XGame.TouchInput;
using XGame.Timeline;
using XGame.EcoMode;
using XGame.Command;
using XGame.Effect;
using XGame.SysSetting;
using XGame.ScreenTouchEffect;
using XGame.FrameUpdate;

using XGame.Preload;
using XGame.Atlas;
using XGame.GameObjCache;
using XGame.EtaMonitor;
using XGame.UI;
using XGame.Def;
using XGame.State;
using XGame.Audio;
using XGame.LightingEff;

using XGame.Guide;

#if SUPPORT_I18N
using XGame.I18N;
using XClient.Game.I18N;
#endif

using XGame.LOP;
using XGame.Ini;
using XGame.UI.Framework;
using XGame.Entity;
using XGame.FlyEffect;
using XGame.Reddot;
using XClient.Scripts.Api;
using XGame.FunctionOpen;
using XGame.FunctionIcon;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XClient.Game
{
    public enum AppAction
    {
        Min = 0,
        OnDestoryBefore,//销毁前
        OnDestoryAfter,//销毁后
        Max,
    }

    public class GameApp : MonoBehaviourEX<GameApp>, IXGameAppSink, ILoaderProviderSink
    {
        //发布版本
        public bool runInPublish = false;
        public static bool RunInPublish { get; private set; }

        //是否在编辑器模式下运行
        public static bool RunInEditor { get; private set; }

        //资源加载模式
        public LoadMode loadMode = LoadMode.Release;

        //加载系统
        public IGAssetLoader m_loadMgr = null;

        //挂载系统设置配置
        public SysSettingOptionConfig sysOptionConfig;
        //红点系统配置
        public ReddotSettings reddotSettings;

        //引导系统设置
        public GuideSettings guideSettings;

        //挂载系统设置列表配置
        public SysSettingOptionValGroupConfig sysOptionValGroupConfig;

        [Header("自动锁屏时间(秒)")]
        public int audoLockScreenTime = 120;

        //预加载系统
        public PreloadConfig preloadConfig;

        //屏幕点击特效配置
        public ScreenTouchEffectConfig screenTouchEffectConfig;

        //默认的画质品级
        public EMPerformanceLevel defaultPerformanceLevel = EMPerformanceLevel.High;

        //Go对象
        private GameObject m_goGameManager;
        public GameObject goGameManager => m_goGameManager;

        //定时器
        private ITimerManager m_TimerManager;

        //资源对象池
        private UnityObjectPoolResLoader m_unityObjectPoolResLoader;

#if SUPPORT_I18N
        //国际化配置
        public I18NConfig m_I18NConfig;
#endif

        //声音合成器配置
        public AudioMixerSettings audioMixerSettings;

        //游戏管理器预制体路径
        public ResourceRef m_gameManagerPrefabPath;

        private XGameSink m_GameSink;

        //默认的图片
        [Header("Sprite卸载后显示的图片")]
        public Sprite m_DefSprite;

        //飘字的配置
        [SerializeField]
        private FlowTextConfigAsset m_flowTextConfigs;
        //窗口的配置
        public UIWindowExSetting windowexSetting;
        //是否显示uilobby 默认不显示()
        public bool isShowUiLobby;
        public static bool IsShowUiLobby { get; private set; }
        //是否显示最底层的测试按钮 默认不显示()
        public bool isShowTestButtons = false;

        //是否启用Transform调试
        [HideInInspector]
        [SerializeField]
        private bool m_isEnableTransformDebug = false;
        public bool isEnableTransformDebug
        {
            get { return m_isEnableTransformDebug; }
            set
            {
                m_isEnableTransformDebug = value;
                TransformDebug.Enable = value;
            }
        }

        //调试模式
        [HideInInspector]
        [SerializeField]
        private TransformDebugMode m_transformDebugMode;
        public TransformDebugMode transformDebugMode
        {
            get { return m_transformDebugMode; }
            set
            {
                m_transformDebugMode = value;
                TransformDebug.Mode = value;
            }
        }

        //Transform调试的名称关键词
        [HideInInspector]
        [SerializeField]
        private string m_transformDebugName = string.Empty;
        public string transformDebugName
        {
            get { return m_transformDebugName; }
            set
            {
                m_transformDebugName = value;
                TransformDebug.NameContent = value;
            }
        }

        //Transform调试的名称关键词
        [HideInInspector]
        [SerializeField]
        private Transform m_transformDebugTarget;
        public Transform transformDebugTarget
        {
            get { return m_transformDebugTarget; }
            set
            {
                m_transformDebugTarget = value;
                TransformDebug.Target = value;
            }
        }

        //是否开启对象池调试
        [HideInInspector]
        [SerializeField]
        private bool m_isEnablePoolableLog = false;
        public bool isEnablePoolableLog
        {
            get { return m_isEnablePoolableLog; }
            set
            {
                m_isEnablePoolableLog = value;
                PoolableDebug.EnableLog = value;
            }
        }

        [HideInInspector]
        [SerializeField]
        private bool m_isEnableLightingEffectDebug = false;
        public bool isEnableLightingEffectDebug
        {
            get
            {
                return m_isEnableLightingEffectDebug;
            }
            set
            {
                LightingEffectDebug.OpenLog = value;
                m_isEnableLightingEffectDebug = value;
            }
        }

        public static bool IsShowTestButtons { get; private set; }

        class AppActionHandler : UnityEngine.Events.UnityEvent { }

        List<AppActionHandler> m_actionTypeList;

        //是否开启监控
        public bool isOpenMonitor;

        //***************调用效率监控相关*******************
        //是否开启调用效率监控
        [Tooltip("是否开启调用效率监控")]
        public bool isOpenEtaMonitor;
        public List<EtaMonitorConfig> etaMonitorConfigs = new List<EtaMonitorConfig>();
        private MonitorManager monitorMgr;

        [Header("启用手动登录(编辑器下有用)")]
        public bool isEnableManulLogin;



        //初始化逻辑资源
        private ResourcesManager m_resMgr = new ResourcesManager();

        protected void RunAtion(AppAction actionType)
        {
            AppActionHandler actionHandler = m_actionTypeList[(int)actionType];
            if (actionHandler != null)
            {
                actionHandler.Invoke();
            }
        }

        public void RegisterAction(AppAction actionType, UnityEngine.Events.UnityAction action)
        {
            m_actionTypeList[(int)actionType].AddListener(action);
        }
        public void UnRegisterAction(AppAction actionType, UnityEngine.Events.UnityAction action)
        {
            m_actionTypeList[(int)actionType].RemoveListener(action);
        }

        public void SetLoadMgr(IGAssetLoader loadMgr)
        {
            m_loadMgr = loadMgr;
        }

        public void Start()
        {
            //非编辑器下先关闭监控
            if (IsRunWindowsEditor() == false)
            {
                isOpenMonitor = false;
                isOpenEtaMonitor = false;
            }

            RunInPublish = runInPublish;
            RunInEditor = Application.isEditor;

            IsShowUiLobby = isShowUiLobby;
            IsShowTestButtons = isShowTestButtons;

            m_actionTypeList = new List<AppActionHandler>();
            for (int i = 0; i < (int)AppAction.Max; i++)
            {
                m_actionTypeList.Add(new AppActionHandler());//update 
            }
            GameTime.Init();

            //初始化监控器
            if (isOpenEtaMonitor)
            {
                CreateMonitors();
            }

            //Transform调试
            TransformDebug.Enable = isEnableTransformDebug;
            TransformDebug.NameContent = transformDebugName;
            TransformDebug.Target = transformDebugTarget;

            m_GameSink = new XGameSink();
            m_GameSink.Create();

            //初始化Q1Game应用。由于某些核心文件是要从内包拷贝到外包的，等
            List<string> coreFiles = new List<string>();
            coreFiles.AddRange(UpdateConfig.m_AssetFileName);
            coreFiles.Add("service.xml");
            coreFiles.Add("serverlist.xml");
            XGameApp.Initialize(this, coreFiles.ToArray());

        
        }

        private void CreateMonitors()
        {
            monitorMgr = new MonitorManager(isOpenEtaMonitor);
            MonitorInitContext initContext = new MonitorInitContext();
            initContext.OutPutCount = 10;
            initContext.RecordDirPath = Application.persistentDataPath + "/EtaMonitorLog";
            monitorMgr.OnCreate(initContext);

            //创建各监控器
            foreach (var config in etaMonitorConfigs)
            {
                MonitorBase monitorBase = null;
                switch (config.monitorType)
                {
                    case EtaMonitorType.Timer:
                        monitorBase = new TimerMonitor(config.monitorType.ToString(), config);
                        break;
                    case EtaMonitorType.Event:
                        monitorBase = new EventMonitor(config.monitorType.ToString(), config);
                        break;
                    case EtaMonitorType.Net:
                        monitorBase = new NetMonitor(config.monitorType.ToString(), config);
                        break;
                    case EtaMonitorType.Frame:
                        monitorBase = new FrameMonitor(config.monitorType.ToString(), config);
                        break;
                    case EtaMonitorType.XGameComs:
                        monitorBase = new XGameComsMonitor(config.monitorType.ToString(), config);
                        XGame.XGameComs.SetEtaMonitor(monitorBase as IXGameComsMonitor);
                        break;
                }
                monitorMgr.AddMonitor((int)config.monitorType, monitorBase);
            }
        }

        public MonitorBase GetMonitor(int id)
        {
            if (isOpenEtaMonitor && monitorMgr != null)
            {
                return monitorMgr.GetMonitor(id);
            }

            return null;
        }

        private IGAssetLoader InitLoadSystem()
        {
            return LoadSystem.CreateLoadSystem();
        }

        void UpdateOpenMonitor()
        {
            IFrameUpdateManager frameUpdateManager = XGame.XGameComs.Get<IFrameUpdateManager>();
            if (frameUpdateManager != null)
            {
                frameUpdateManager.SetOpenMonitorFlag(isOpenMonitor);
            }
        }
       
        //是否在window平台运行编辑器
        public static bool IsRunWindowsEditor()
        {
#if UNITY_EDITOR
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
            {
                return true;
            }
#endif
            return false;
        }

        void update()
        {
            if(m_resMgr!=null)
            {
                m_resMgr.Update();
            }
        }


        void OnDestroy()
        {
            if (m_resMgr != null)
            {
                m_resMgr.Release();
                m_resMgr = null;
            }
        }

        private void LoadGameManager()
        {
            //初始化监听
            IInputManager inputManager = XGame.XGameComs.Get<IInputManager>();
            if (inputManager != null)
            {
                List<string> canHitLayerNames = new List<string>()
                {
                    LayerConfig.LayerMonster,
                    LayerConfig.LayerHero,
                    LayerConfig.LayerGround
                };

                inputManager.SetHitLayerNames(canHitLayerNames);
            }

            InputEventPoint inputEventPoint = new InputEventPoint();
            inputEventPoint.Create(inputManager);

            //定时器组件
            m_TimerManager = XGame.XGameComs.Get<ITimerManager>();

            //保存预制体对象，用来做卸载
            if(!string.IsNullOrEmpty(m_gameManagerPrefabPath.path))
            {
                uint handle;
                GameObject obj = XGame.XGameComs.Get<XGame.Asset.IGAssetLoader>().LoadResSync<GameObject>(m_gameManagerPrefabPath.path, out handle) as GameObject;
                if (obj == null)
                {
                    Debug.LogError($"加载游戏管理器失败！！{m_gameManagerPrefabPath.ToString()}");
                    return;
                }
                m_goGameManager = GameObject.Instantiate(obj);
            }

            //注册特效播放回调
            var settings = XGameComs.Get<IUIFramework>().Settings;
            if (settings.CameraUIContainer != null)
            {
                IScreenTouchEffectCom screenTouchEffectCom = XGame.XGameComs.Get<IScreenTouchEffectCom>();
                screenTouchEffectCom?.SetEffectBeginPlayCallback(OnScreenTouchEffectBeginPlay);
                screenTouchEffectCom?.ChangeParent(settings.CameraUIContainer);
            }

            m_resMgr.Create();

        }

        private void OnScreenTouchEffectBeginPlay(GameObject goEffect)
        {
            EffectPlayer effect = goEffect.GetComponent<EffectPlayer>();
            if (effect == null)
            {
                return;
            }
            effect.PlayEffect();
        }

        /// <summary>
        /// 处理器
        /// </summary>
        private GamePreProcesser[] m_PreProcessers;

        /// <summary>
        /// 启动游戏预处理器
        /// </summary>
        private void StartGamePreProcessers()
        {
            foreach(var p in m_PreProcessers)
            {
                if(p.enabled)
                {
                    p.Execute();
                }
            }

            InvokeRepeating("CheckGamePreProgressers", 0.2f, 0.2f);
        }

        /// <summary>
        /// 检查游戏预处理器
        /// </summary>
        /// <returns></returns>
        private void CheckGamePreProgressers()
        {
            bool isAllFinished = true;
            foreach (var p in m_PreProcessers)
            {
                if (p.enabled && !p.isFinished)
                {
                    isAllFinished = false;
                    break;
                }
            }

            if(isAllFinished)
            {
                CancelInvoke("CheckGamePreProgressers");
                DisposeGamePreProcessers();
                OnGamePreProcessorFinished();
            }
        }

        /// <summary>
        /// 销毁游戏预处理器
        /// </summary>
        private void DisposeGamePreProcessers()
        {
            foreach(var p in m_PreProcessers)
            {
                UnityEngine.Object.DestroyImmediate(p);
            }
            m_PreProcessers = null;
        }

        /// <summary>
        /// 预处理完成调用
        /// </summary>
        private void OnGamePreProcessorFinished()
        {
            LoadGameManager();
        }

        /// <summary>
        /// 游戏App初始化完成
        /// </summary>
        public void OnXGameAppInited()
        {
            m_PreProcessers = GetComponents<GamePreProcesser>();
            if(m_PreProcessers != null)
            {
                StartGamePreProcessers();
            }
            else
            {
                OnGamePreProcessorFinished();
            }
        }

        /// <summary>
        /// 游戏App将要退出
        /// </summary>
        public void OnXGameAppWillQuit()
        {
            RunAtion(AppAction.OnDestoryBefore);

            if (m_goGameManager != null)
            {
                GameObject.DestroyImmediate(m_goGameManager);
                m_goGameManager = null;
            }
        }

        /// <summary>
        /// 游戏App销毁
        /// </summary>
        public void OnXGameAppQuited()
        {
            if (monitorMgr != null)
                monitorMgr.OnDestroy();

            //资源加载器销毁
            if (m_unityObjectPoolResLoader != null)
            {
                m_unityObjectPoolResLoader.Release();
                m_unityObjectPoolResLoader = null;
            }

            m_GameSink?.Release();
            m_GameSink = null;

            RunAtion(AppAction.OnDestoryAfter);
        }


        //准备时候游戏模块和组件
        public void OnPreInit()
        {
            runInPublish = GameIni.Instance.GetInt("Publish", 1) > 0;
            Debug.Log("runInPublish=" + runInPublish);
            RunInPublish = runInPublish;
        }

        /// <summary>
        /// 注册游戏中要使用到的各种组件
        /// </summary>
        public void OnRegisterXGameComs()
        {
            //对象池组件
            XGame.XGameComs.Reg<IItemPoolManager>(new ItemPoolManagerNewer());
            PoolableDebug.EnableLog = isEnablePoolableLog;

            //GameObject缓存组件
            XGame.XGameComs.Reg<IGameObjectCache>(new IGameObjectCacheNewer());

            //ILOPObjectManager 管理器
            XGame.XGameComs.Reg<ILOPObjectManager>(new ILOPObjectManagerNewer());

            //协程池组件
            XGame.XGameComs.Reg<ICoroutineManager, CoroutineManager>(ComBehaviorFlag.Update);

            IFrameMonitor frameMonitor = GetMonitor((int)EtaMonitorType.Frame) as IFrameMonitor;
            //帧更新器
            XGame.XGameComs.Reg<IFrameUpdateManager>(new FrameUpdateManagerNewer(), ComBehaviorFlag.AllUpdate, frameMonitor);

            ITimerMonitor timerMonitor = GetMonitor((int)EtaMonitorType.Timer) as ITimerMonitor;
            //定时器组件
            XGame.XGameComs.Reg<ITimerManager, TimerManager>(timerMonitor);

            //初始化加载系统，在发布版中，这个初始在update启动场景
            if(null == m_loadMgr)
            {
                m_loadMgr = InitLoadSystem();
            }

            XGame.XGameComs.Reg<IGAssetLoader>(m_loadMgr, ComBehaviorFlag.Update);
			
            //MeshInstance组件
            //XGame.XGameComs.Reg<IMeshInstanceManager>(new MeshInstanceManagerNewer(), ComBehaviorFlag.Update, null, null);

            //Sprite图集处理组件
            var spriteAtlasManager =  XGame.XGameComs.Reg<ISpriteAtlasManager>(new SpriteAtlasManagerNewer(), ComBehaviorFlag.Update | ComBehaviorFlag.LateUpdate, null, null);
            spriteAtlasManager.SetDefaultSprite(m_DefSprite);

			//性能监控组件
            IEventMonitor eventMonitor = GetMonitor((int)EtaMonitorType.Event) as IEventMonitor;
			
            //事件服组件
            XGame.XGameComs.Reg<IEventEngine, EventEngine>(eventMonitor);

            //Unity对象池组件
            m_unityObjectPoolResLoader = new UnityObjectPoolResLoader();
            m_unityObjectPoolResLoader.Create();
            UnityObjectPoolConfig unityObjectPoolConfig = new UnityObjectPoolConfig();
            unityObjectPoolConfig.resLoader = m_unityObjectPoolResLoader;
            XGame.XGameComs.Reg<IUnityObjectPool>(new UnityObjectPoolNewer(), ComBehaviorFlag.Update, unityObjectPoolConfig);

            //音频播放组件
            AudioComInitContext audioContext = new AudioComInitContext();
            audioContext.audioCapacity = 16;
            audioContext.audioConfigPath = "";
            audioContext.mixerSettings = audioMixerSettings;
            IAudioCom audioCom = XGame.XGameComs.Reg<IAudioCom>(new AudioComNewer(), ComBehaviorFlag.Update, audioContext);

            //设置配置解析器
            audioCom.SetPlayAudioParamsParser((audioID, p) =>
            {
                p.Reset();
                p.audioID = audioID;

                ISchemeModule schemeModule = GameGlobal.ComAndModule.GetModule(DModuleID.MODULE_ID_SCHEME) as ISchemeModule;
                cfg_Audio cfg = schemeModule.GetCgamescheme().Audio_0(audioID);
                if (cfg == null)
                {
                    AudioDebug.LogError("错误的声音配置ID，ID=" + audioID);
                    p.isValid = false;
                    return false;
                }

                p.path = cfg.assetPath;
                p.loop = cfg.loop == 1;
                p.volume = cfg.volume;
                p.pitch = cfg.pitch;
                p.userData1 = null;
                p.userData2 = null;
                p.audioType = cfg.audioType;
                p.audioMixerGroupType = audioCom.AudioTypeToAudioMixerGroupType(cfg.audioType);
                p.fadeInTime = cfg.fFadeInTime;
                p.fadeOutTime = cfg.fFadeOutTime;
                p.is3D = cfg.i3D > 0;
                p.minDistance = cfg.fMinDistance;
                p.maxDistance = cfg.fMaxDistance;
                p.isValid = true;

                return true;
            });

            //最多播放50个声音
            audioCom.SetAudioCapacity(50);

            //状态机组件
            XGame.XGameComs.Reg<IStateMachineManager>(new StateMachineManagerNewer(), ComBehaviorFlag.Update);

            //画质组件
            var qualityComContext = new QualityComContext();
            qualityComContext.defaultLevel = defaultPerformanceLevel;
            XGame.XGameComs.Reg<IQualityCom>(new QaulityComNewer(), qualityComContext, null);

            //新版光效组件
            var lightingEffectManager = XGameComs.Reg<ILightingEffectManager>(new LightingEffectManagerNewer(), ComBehaviorFlag.Update, null);
            LightingEffectDebug.OpenLog = m_isEnableLightingEffectDebug;
            lightingEffectManager.SetPlayEffectParamsParser((id, param) => {
                cfg_LightingEffect cfg = GameGlobal.GameScheme.LightingEffect_0((uint)id);
                if (cfg == null)
                {
                    param.isValid = false;
                    LightingEffectDebug.LogError("获取光效配置失败，nEffectID=" + id);
                    return false;
                }

                param.id = (int)cfg.nID;
                param.resPath = cfg.strResPath;
                param.type = (LightingEffectType)cfg.iType;
                param.scale = cfg.fScale;
                param.maxLiveTime = cfg.fMaxLiveTime;
                param.isNeedQualityControl = false; // cfg.iNeedQualityCtrl == 0;
                param.isNeedQualityControlByDistance = false;// cfg.iNeedQualityCtrlByDistance == 0;

                param.isDisableBindFollow = cfg.iDisableBindFollow == 1;

#if SUPPORT_SPINE
                param.isSpineRes = cfg.bSpineRes == 1;
                param.spineAnimName = cfg.szSpineAnimName;
#endif
                
                if (cfg.arrBindOffset.Length > 2)
                    param.bindOffset.Set(cfg.arrBindOffset[0], cfg.arrBindOffset[1], cfg.arrBindOffset[2]);
                else
                    param.bindOffset.Set(0f, 0f, 0f);

                param.bindPointName = cfg.szBindPoint;
                param.isValid = true;

                return true;
            });


            //Timeline管理组件
            XGame.XGameComs.Reg<ITimelineStoryManager>(new TimelineStoryManagerNewer(), ComBehaviorFlag.Update);

            //屏幕输入组件
            XGame.XGameComs.Reg<IInputManager, InputManager>(ComBehaviorFlag.Update);

            //Effect组件
            XGame.XGameComs.Reg<IEffectCom, EffectCom>();

            //预加载组件
            IPreloader preloader = XGame.XGameComs.Reg<IPreloader>(new PreloaderNewer(), null, preloadConfig);

            //引导组件
            var guideCreateCtx = new GuideManagerCreateContext();
            guideCreateCtx.Settings = guideSettings;
            IGuideManager guideMgr = XGameComs.Reg<IGuideManager>(new GuideManagerNewer(), ComBehaviorFlag.Update, guideCreateCtx);

            //飘字组件
            FlowTextManagerContext flowTextManagerContext = new FlowTextManagerContext();
            flowTextManagerContext.configs = m_flowTextConfigs;
            flowTextManagerContext.sink = m_GameSink;
            XGame.XGameComs.Reg<IFlowTextManager>(new FlowTextManagerNewer(), ComBehaviorFlag.Update, flowTextManagerContext);

            //命令管理组件
            XGameComs.Reg<ICommandManager>(new CommandManagerNewer(), ComBehaviorFlag.Update);

            //功能开放组件
            XGameComs.Reg<IFunctionOpenManager>(new FunctionOpenManagerNewer(), ComBehaviorFlag.Update);

            //系统设置组件
            if (sysOptionConfig != null && sysOptionValGroupConfig != null)
            {
                SysSettingContext sysSettingContext = new SysSettingContext(sysOptionConfig, sysOptionValGroupConfig);
                ISysSettingCom sysSetting = XGame.XGameComs.Reg<ISysSettingCom>(new SysSettingComNewer(), sysSettingContext);
                sysSetting.AddChangeCallback(OnSystemSettingChanged);
                SystemSettings.InitSystemSettings();
            }
            else
            {
                TRACE.ErrorLn("SysSettingOptionConfig 或 SysSettingOptionValGroupConfig 未赋值");
            }

            //省电模式，锁屏组件
            EcoModeContext ecoModeContext = new EcoModeContext();
            ecoModeContext.autoLockScreenTime = audoLockScreenTime * 1000;
            XGameComs.Reg<IEcoMode>(new EcoModeNewer(), ComBehaviorFlag.Update, ecoModeContext);

            //屏幕点击特效组件
            //XGameComs.Reg<IScreenTouchEffectCom>(new ScreenTouchEffectComNewer(), ComBehaviorFlag.Update, screenTouchEffectConfig);

#if SUPPORT_I18N
            //国际化组件
            var i18NManager = XGame.XGameComs.Reg<II18NManager, I18NManager>(0, m_I18NConfig);
            if(null!= i18NManager)
            {
                i18NManager.SetSpriteLoader(new DefualtSpriteLoader());
                i18NManager.SetTextTranslater(new I18NTranslater());
            }
            else
            {
                UnityEngine.Debug.LogError("null== i18NManager");
            }
#endif

            // UI排序处理组件
            XGame.XGameComs.Reg<IUISortingManager>(new UISortingManagerNewer());

            //注册UI框架组件
            XGame.XGameComs.Reg<IUIFramework>(new UIFrameworkNewer());

            // 动画重载
            //XGame.XGameComs.Reg<XGame.NewAnimator.IAnimationOverridesMgr, XGame.NewAnimator.AnimationOverridesMgr>();

            //实体管理组件
            XGame.XGameComs.Reg<IEntityWorld>(new EntityWorldNewer(), ComBehaviorFlag.Update);

            //飞行特效组件
            XGame.XGameComs.Reg<IFlyEffectManager>(new FlyEffectManagerNewer(), ComBehaviorFlag.Update);

            //红点组件
            var reddotCreateCtx = new ReddotCreateContext();
            reddotCreateCtx.Settings = reddotSettings;
            XGame.XGameComs.Reg<IReddotManager>(new ReddotManagerNewer(), ComBehaviorFlag.Update, reddotCreateCtx);

            //功能图标组件
            XGameComs.Reg<IFunctionIconManager>(new FunctionIconManagerNewer(), ComBehaviorFlag.Update, null);

            //更新各个组件对应的监控
            UpdateOpenMonitor();
        }

        /// <summary>
        /// 系统设置变更回调
        /// </summary>
        /// <param name="lsOptionNames"></param>
        private void OnSystemSettingChanged(List<string> lsOptionNames)
        {
            //ISysSettingCom settingCom = XGameComs.Get<ISysSettingCom>();
            SystemSettings.OnSystemSettingChanged(lsOptionNames);
        }

        /// <summary>
        /// 重启游戏
        /// </summary>
        /// <returns></returns>
        public static void Restart()
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
                UnityEditor.EditorApplication.playModeStateChanged += (state)=>{ 
                    if(state == PlayModeStateChange.EnteredEditMode)
                    {
                        UnityEditor.EditorApplication.isPlaying = true;
                    }
                };
            }
#endif
        }

        public static string TestString => GetVersionStr() + "\n" + DateTime.Now;

        /// <summary>
        /// 获取版本字符串
        /// </summary>
        /// <returns></returns>
        public static string GetVersionStr()
        {
            var info = GetVersionConfigInfo();
            if (info != null)
                return info.localVerNo.ToString();
            Debug.LogError("获取版本号失败:" + UpdateConfig.updateConfigPath);
            return "获取版本号失败:" + UpdateConfig.updateConfigPath;
        }

        private static VersionConfigInfo GetVersionConfigInfo()
        {
            var path = UpdateConfig.updateConfigPath;
#if UNITY_ANDROID
            if (!File.Exists(path)) //手机只能读外包，内包不能COPY到外包 要用WWW，是异步的
            {
                return null;
            }
#endif
            StreamReader sr = null;
            try
            {
                if (!File.Exists(path))
                {
                    if (!File.Exists(UpdateConfig.appConfigPath))
                    {
                        return null;
                    }
                    File.Copy(UpdateConfig.appConfigPath, path);
                }

                if (!File.Exists(path))
                {
                    return null;
                }
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    var xmlVersionInfo = new XMLVersionInfo();
                    sr = new StreamReader(path);
                    var info = xmlVersionInfo.GetVersionInfo(sr.BaseStream);
                    sr.Close();
                    sr.Dispose();
                    return info;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("获取版本信息异常：" + e);
                throw;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
            }
            return null;
        }

        /// <summary>
        /// 打开下载APK URL
        /// </summary>
        /// <returns></returns>
        public static bool OpenDownloadURL()
        {
            var versionInfo = GetVersionConfigInfo();
            if (versionInfo == null)
                return false;

            var url = versionInfo.downloadUrl;
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
            Debug.Log("打开下载APK URL ：" + url);
            return true;
        }
    }
}

