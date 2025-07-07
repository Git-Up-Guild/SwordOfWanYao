using UnityEditor;
using UnityEngine;
using XClient.Game;
using XGame.Asset;
using XGame.Update;
using XGame.Utils;

namespace XGame.AssetScript
{
    public class XGameAppBuilder : MonoBehaviour
    {
        public ResourceRef gameAppRes;

        private GameObject gameRoot;

        void Start()
        {
#if UNITY_EDITOR
            var context = new XGameAppEnvBuilderContext();
            context.configFilePath = "GameConfig.ini";

            XGameAppEnv.Build(context, (b) =>
            {
                if(!b)
                {
                    XGameAppEnv.LogError("XGameApp运行环境构建失败");
                }
                else
                {
                    if(string.IsNullOrEmpty(gameAppRes.path))
                    {
                        XGameAppEnv.LogError("启动游戏失败，没有指定游戏根对象");
                    }
                    else
                    {
                        var gameRootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/{gameAppRes.path}");
                        gameRoot = GameObject.Instantiate(gameRootPrefab);
                        gameRoot.transform.BetterSetParent(transform);
                    }
                }
            });

#else
            Setup();
#endif
        }

        void Update()
        {
        }

        private void Setup()
        {
            UpdateDebug.LogWarning("同步关键文件资源");
            SyncFile2External.Execute(gameObject, UpdateConfig.m_AssetFileName, () =>
            {
                GameObject loginGo = null;

                IGAssetLoader loaderMgr = LoadSystem.CreateLoadSystem();

                //构建XGameApp环境
                var buildContext = new XGameAppEnvBuilderContext();
                buildContext.configFilePath = "GameConfig.ini";
                XGameAppEnv.Build(buildContext, (isSuc) => {
                    if (isSuc)
                    {
                        UpdateDebug.LogWarning("加载LoginRoot");

                        uint handle = 0;
                        UnityEngine.Object res = loaderMgr.LoadResSync<UnityEngine.Object>(gameAppRes.path, out handle, false) as UnityEngine.Object;


                        if (null != res)
                        {
                            loginGo = UnityEngine.Object.Instantiate(res) as GameObject;

                            ILoaderProviderSink gameApp = loginGo.GetComponentInChildren<ILoaderProviderSink>();
                            gameApp.SetLoadMgr(loaderMgr);
                        }

                        if (null != loginGo)
                        {
                            loginGo.transform.BetterSetParent(null);
                            loginGo.transform.localPosition = new Vector3(0, 0, 0);

                            UpdateDebug.Log("加载启动LoginUI成功: " + gameAppRes.path);
                        }
                        else
                        {
                            UpdateDebug.Log("加载启动LoginUI失败: " + gameAppRes.path);
                        }

                    }
                    else
                    {
                        UpdateDebug.Log("加载启动LoginUI失败: " + gameAppRes.path);
                    }
                });

            }, true);
        }
    }
}