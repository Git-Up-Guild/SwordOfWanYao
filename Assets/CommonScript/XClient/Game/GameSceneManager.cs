
using UnityEngine;
using UnityEngine.SceneManagement;
using XClient.Common;
using XGame;
using XGame.EventEngine;

namespace XClient.Game
{
    /// <summary>
    /// 游戏状态关联场景管理器
    /// </summary>
    public class GameSceneManager : IEventExecuteSink
    {
        private GameSceneManager() { }
        public static GameSceneManager Instance = new GameSceneManager();

        public int gameStateSceneBuildIndex { get; private set; }


        public string gameScenePath { get; private set; }

        private AsyncOperation m_LoadOper;

        public void Setup(int sceneBuildIndex,string scenePath)
        {
            gameStateSceneBuildIndex = sceneBuildIndex;
            gameScenePath = scenePath;

            XGameComs.Get<IEventEngine>().Subscibe(this, DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE,
                DEventSourceType.SOURCE_TYPE_SYSTEM, 0, "GameSceneManager");

            XGameComs.Get<IEventEngine>().Subscibe(this, DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE,
                DEventSourceType.SOURCE_TYPE_SYSTEM, 0, "GameSceneManager");
        }

        public void Clear()
        {
            UnloadScene();

            gameStateSceneBuildIndex = -1;

            XGameComs.Get<IEventEngine>().UnSubscibe(this, DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE,
                DEventSourceType.SOURCE_TYPE_SYSTEM, 0);

            XGameComs.Get<IEventEngine>().UnSubscibe(this, DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE,
               DEventSourceType.SOURCE_TYPE_SYSTEM, 0);
        }

        private void LoadScene()
        {


            bool LoadScene = false;

            if (string.IsNullOrEmpty(gameScenePath)==false)
            {
                m_LoadOper = SceneManager.LoadSceneAsync(gameScenePath, LoadSceneMode.Additive);
              
                LoadScene = true;
            }
            else
            {
                if (gameStateSceneBuildIndex >= 0)
                {
                    Scene scene = SceneManager.GetSceneByBuildIndex(gameStateSceneBuildIndex);

                    if (scene != null && !scene.isLoaded)
                    {
                        m_LoadOper = SceneManager.LoadSceneAsync(gameStateSceneBuildIndex, LoadSceneMode.Additive);
                        LoadScene = true;
                       
                      
                    }
                }
            }


            //订阅加载完成消息
            if (LoadScene)
            {
                GameStateManager.Instance.gameState.AddReadyStateValidator((s) => {
                    if (m_LoadOper != null)
                        return m_LoadOper.isDone;
                    return true;

                }, XGame.State.ReadyStateValidatorWorkMode.Temp);
            }



        }

        private void UnloadScene()
        {
            if(gameStateSceneBuildIndex > 0)
            {
                Scene scene = SceneManager.GetSceneByBuildIndex(gameStateSceneBuildIndex);
                if (scene != null && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(gameStateSceneBuildIndex);
                }
            }

            m_LoadOper = null;
        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            GameStateChangeEventContext ctx = (GameStateChangeEventContext)pContext;
            switch(wEventID)
            {
                case DGlobalEvent.EVENT_GAME_STATE_PRE_CHANGE:
                   if(ctx.nNewState == (int)GameState.Game)
                    {
                        LoadScene();
                    }
                    break;
                case DGlobalEvent.EVENT_GAME_STATE_AFTER_CHANGE:
                    {
                        if (ctx.nNewState == (int)GameState.Login)
                        {
                            UnloadScene();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
