/*****************************************************
** 文 件 名：CSchemeModule
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/11/9 10:58:18
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/
using System;
using XClient.Common;
using XGame;
using XGame.FrameUpdate;

namespace XClient.Client.Scheme
{
    /// <summary>
    /// 脚本模块
    /// </summary>
    public class CSchemeModule : ISchemeModule
    {
        private bool m_isFinished = false;
        private int m_totalCount;
        private int m_loadedCount;

        //每一帧加载脚本的数量
        private int m_loadCountPerFrame = 2000;

        //加载完成回调
        private Action m_funcOnSchemeLoadFinished;

        private GameSchemeCom m_gameSchemeCom;
        private GameSchemeCom gameSchemeCom
        {
            get
            {
                if (m_gameSchemeCom == null)
                {
                    m_gameSchemeCom = GameGlobal.ComAndModule.GetCom(DComID.COM_ID_GAME_SCHEME) as GameSchemeCom;
                }
                return m_gameSchemeCom;
            }
        }

        // 表接口
        public Cgamescheme GameScheme => gameSchemeCom.gameScheme;

        public string ModuleName { get; set; }

        public ModuleState State { get; set; }

        public float Progress { get; set; }

        public int ID { get; set; }

        public bool Start()
        {
            m_isFinished = false;
            m_totalCount = gameSchemeCom.GetSchemeCount();

            //订阅帧更新
            XGameComs.Get<IFrameUpdateManager>()?.RegLateUpdateCallback(this.Update, "CSchemeModule:Start");

            return true;
        }

        public void Stop()
        {
            XGameComs.Get<IFrameUpdateManager>()?.UnregLateUpdateCallback(this.Update);
            m_isFinished = false;
        }

        public bool IsLoadFinish()
        {
            return m_isFinished;
        }

        public float GetProgress()
        {
            if (m_totalCount == 0)
                return 1.0f;
            return (float)m_loadedCount/ m_totalCount;
        }

        public void AddSchemeLoadFinishedCallback(Action callback)
        {
            m_funcOnSchemeLoadFinished += callback;
        }

        private void RemoveSchemeLoadFinishedCallback(Action callback)
        {
            m_funcOnSchemeLoadFinished -= callback;
        }

        public void Update()
        {
            if (!m_isFinished)
            {
                for (var i = 0; i < m_loadCountPerFrame; ++i)
                {
                    if (m_loadedCount < m_totalCount)
                    {
                        gameSchemeCom.LoadScheme(m_loadedCount++);
                        //Debug.Log($"加载脚本进度{m_loadedCount}/{m_loadedCount}");
                    }
                }

                if (m_loadedCount >= m_totalCount)
                {
                    m_isFinished = true;
                }

            }
            else //延后一帧初始化，完成回调有时序问题
            {
                m_funcOnSchemeLoadFinished?.Invoke();

                //退订更新检测
                XGameComs.Get<IFrameUpdateManager>()?.UnregLateUpdateCallback(this.Update);

            }
        }

        /// <summary>
        /// 获取C#的表
        /// </summary>
        /// <returns></returns>
        public Igamescheme GetCgamescheme()
        {
            return GameScheme;
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }

        public bool Create(object context, object config = null)
        {
            State = ModuleState.Success;
            return true;
        }

        public void Release()
        {
        }

        public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了

        public void Clear(int param)
        {
            
        }

        public void OnRoleDataReady()
        {
        }
    }
}

