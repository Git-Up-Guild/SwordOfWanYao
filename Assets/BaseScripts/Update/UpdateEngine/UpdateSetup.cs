using UnityEngine;
using System.IO;
using XGame.Asset.Load;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XGame.Update
{
    public class UpdateSetup : MonoBehaviour
    {
        //热更新的静态配置列表
        //public static string[] s_soHotList = {"jniLibs/arm64-v8a/libbatcore.so", "jniLibs/armeabi-v7a/libbatcore.so" };

        public static string[] s_soHotList = { };

        //游戏是否准备好了
        private static bool s_isGameReady = false;

        //是否google版本，google版本不支持so热更新
        public bool supportSOUpdate = false;

        //是否支持埋点功能
        public bool supportMMP = false;

        public static void SetGameReadyFlag()
        {
            if (!s_isGameReady)
            {
                UpdateDebug.Log("游戏已经准备好！");
                s_isGameReady = true;
            }
        }

        //启动的预制体
        public string strAssetBundleName;

        //启动的预制体
        public string strAssetName;

        //预制体名称
        public string strPrefabName;

        //监听某个对象出现，然后就自己消失
        public string m_strLoginUIName = "";

        //最大的监听时间
        public int m_maxMonitorFrame = 600;

        //是否开始监听
        private bool m_bStartMonitor = false;

        private GameObject m_sceneCamera = null;

        //开始监听时间
        private int m_startMonitorFrame = 0;

        //启动的ab
        private AssetBundle m_setupAB = null;

        //启动的UI 
        private GameObject m_setupObject = null;


        //已经准备好了的时候，这个时刻过几帧再删除
        public int m_readyFrame = -1;

        //启动UI更对象
        [SerializeField]
        private RectTransform m_updateUIRoot;

        void Start()
        {
            //设置异常处理函数
            //ExceptionHandler.SetupExceptionHandler();
            //设置不锁屏幕
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //启动的时候打点
#if UNITY_ANDROID
            //TalkingDataAPI.OnEvent("启动", "Platform","Android");
            //MMPEventAPI.OnEvent("启动", "Platform", "Android");

#else
            //TalkingDataAPI.OnEvent("启动", "Platform","IOS");
            //MMPEventAPI.OnEvent("启动", "Platform", "IOS");
#endif

        




#if UNITY_EDITOR
            UpdateDebug.LogWarning("Start: " + Application.persistentDataPath);
#endif

            Debug.LogWarning("当前缓存目录:  " + Application.persistentDataPath);
            OnStart(false);

            if (supportMMP)
            {
                //最后初始化 firebase sdk
                //FireBaseInstance.Init();
            }
            
        }

        void OnStart(bool bSyncSO)
        {


            UpdateDebug.LogWarning("UpdateSetup：start: ");

            //加载资源
            string hashName = A2BRecord.GetHashCodeString(strAssetBundleName) + ".bin";
            if (false == LoadAssetSyn(hashName, strAssetName, out m_setupAB, out m_setupObject))
            {
                LoadAssetSyn(strAssetBundleName, strAssetName, out m_setupAB, out m_setupObject);
            }

#if UNITY_EDITOR
            if (null == m_setupAB)
            {
                Object rest = AssetDatabase.LoadAssetAtPath<Object>(strPrefabName);
                m_setupObject = Object.Instantiate(rest) as GameObject;
            }
#endif

            if (null != m_setupObject)
            {
                m_setupObject.transform.BetterSetParent(m_updateUIRoot);
                m_setupObject.transform.localPosition = new Vector3(0, 0, 0);
                m_setupObject.transform.localScale = new Vector3(1, 1, 1);
                m_setupObject.transform.eulerAngles = new Vector3(0, 0, 0);

                RectTransform rt = m_setupObject.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2();
                    rt.offsetMin = new Vector2();
                    rt.offsetMax = new Vector2();
                }

                UpdateDebug.Log("加载启动UI成功: " + strAssetBundleName);

                UpdateScene updateScene = m_setupObject.GetComponentInChildren<UpdateScene>();
                if (null != updateScene)
                {
                    updateScene.SetFinishCallback(SetFinishCallback);
                }
                else
                {
                    UpdateDebug.LogError("找不到组件 UpdateScene");
                }
            }
        }

        void Update()
        {
            if (m_bStartMonitor)
            {
                /*
                Canvas c = m_updateUIRoot.GetComponent<Canvas>();
                if (c != null)
                {
                    c.enabled = false;
                    c.enabled = true;
                }
                */

                HideSceneCamera();

                ++m_startMonitorFrame;
                bool isTimeOut = ( m_startMonitorFrame > m_maxMonitorFrame);
                if (s_isGameReady || isTimeOut)
                {
                    if (isTimeOut)
                    {
                        UpdateDebug.LogError("游戏准备超时！");
                    }
                    else
                    {
                       
                        if (m_readyFrame <= 0)
                        {
                            m_readyFrame = m_startMonitorFrame;
                        }

                        //为了减少花屏合黑屏，延后5帧再进入
                        if (m_startMonitorFrame - m_readyFrame < 5)
                        {
                            return;
                        }
                    }
                        

                   

                    ShowSceneCamera();

                    //将热更新的内容全部删除干净
                    GameObject.DestroyImmediate(gameObject);
                    //Destroy(gameObject);
                    return;
                }
            }
        }

        public void LateUpdate()
        {
            HideSceneCamera();

            /*
            Canvas c = m_updateUIRoot.GetComponent<Canvas>();
            if(c!=null)
            {
                c.enabled = false;
                c.enabled = true;
            }
            */
        }

        public void SetFinishCallback(int state)
        {
            UpdateDebug.Log("SetFinishCallback：热更新已经完成，等待游戏准备好");

            m_bStartMonitor = true;
            m_startMonitorFrame = 0;

            HideSceneCamera();
        }

        private void HideSceneCamera()
        {
            return;

            if (m_bStartMonitor)
            {
                if (null == m_sceneCamera)
                {
                    m_sceneCamera = GameObject.Find("SceneCamera");
                    if (m_sceneCamera != null)
                    {
                        // Debug.Log("隐藏  SceneCamera ");
                        m_sceneCamera.BetterSetActive(false);

                    }
                }
                else
                {
                    if (m_sceneCamera.activeSelf)
                    {
                        //Debug.Log("隐藏  SceneCamera ");
                        m_sceneCamera.BetterSetActive(false);
                    }
                }
            }
        }

        private void ShowSceneCamera()
        {
            return;
            //恢复base camera
            if (m_sceneCamera != null)
            {
                //Debug.Log("恢复 SceneCamera 显示");
                m_sceneCamera.BetterSetActive(true);
                m_sceneCamera = null;
            }
        }

        private void OnDestroy()
        {
            ShowSceneCamera();

            if (null != m_setupObject)
            {
                m_setupObject.transform.BetterSetParent(null);
                DestroyImmediate(m_setupObject);
                m_setupObject = null;
            }

            if (null != m_setupAB)
            {
                m_setupAB.Unload(true);
                m_setupAB = null;
            }
        }

        //通过名字加载一个对象回来
        public static bool LoadAssetSyn(string bundleName, string assetName, out AssetBundle assetBundle, out GameObject assetObj)
        {
            assetBundle = null;
            assetObj = null;

            ulong offset = UpdateConfig.isEncryAssetBundle ? UpdateConfig.EncryDataLen : 0;


            string path = UpdateConfig.appHotUpdateDir + bundleName;
            if (File.Exists(path)) //先检查热更新包
            {
                Debug.Log("CommonUtil.LoadAssetSyn: " + path);
                assetBundle = AssetBundle.LoadFromFile(path, 0, offset);
            }
            else
            {
                if (File.Exists(path)) //再检查web资源包
                {
                    path = UpdateConfig.appWebDir + bundleName;

                    Debug.Log("CommonUtil.LoadAssetSyn: " + path);
                    assetBundle = AssetBundle.LoadFromFile(path, 0, offset);
                }
                else //再读取内包资源
                {
                    path = UpdateConfig.appInnerDir + bundleName;

                    Debug.Log("CommonUtil.LoadAssetSyn:" + path);
                    assetBundle = AssetBundle.LoadFromFile(path, 0, offset);
                }
            }

            if (null != assetBundle)
            {
                Object res = assetBundle.LoadAsset(assetName);
                if (res != null)
                {
                    assetObj = Object.Instantiate(res) as GameObject;
                }
                else
                {
                    Debug.LogError("CommonUtil.LoadAssetSyn: 资源类别为NULL: assetName = " + assetName);
                }
            }
            else
            {
                Debug.LogError("CommonUtil.LoadAssetSyn: null == assetBundle path: " + bundleName);
            }

            return assetObj != null;
        }

    }
}
