using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using XGame.Asset;
using XGame.Asset.Load;
using XGame.Utils;
using XGame.AssetScript.SDK.Base;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XGame.Update
{

    enum UPDATE_STATE
    {
        STATE_UPDATE_FINISH,
        STATE_UPDATE_GAME_INI_FINISH,
    };

    public class UpdateScene : MonoBehaviour
    {
        private const string IsNotFirstEnterGame = "IsNotFirstEnterGame";

        [Tooltip("测试用：启动先删除外包所有热更资源")]
        public bool isTestDeleteUpdateFile = false;
        public UpdateClient updateClient;
       // public UpdateIL2CPP updateIL2CPP;
        public UpdateMessageBoxUI messageBoxUI;
        public UIMoviePlayer fristEnterMoviePlayer;

        //需要设置为不删除的背景
        public GameObject bg;

        public Button startUpdateBtn;
        public Image updateSlider;
        public RectTransform sliderDot;
        public Text updateProgress;
        public Text updateNextVersion;
        public Text textCurVersion;

        //跳过更新按钮
        public Button btnSkip;

        //退出的时候需要销毁的对象
        //public GameObject destroyObject;

        //登录的UI包名
        public string strAssetBundleName;

        //启动的预制体
        public string strAssetName;

        //预制体名称
        public string strPrefabName;

        //国际化配置
        public UpdateI18NConfig il8nConfig;

        public List<string> listTips;

        //成功回调
        private Action<int> finishCallback = null;

        private string apkURL;

        //是否发布版
        bool publishReleae = true;

        //是否已经转换成登录状态
        bool m_bChangeLogin = false;

        private ulong cur_Version = 0xFFFFFFFFFFFFFFFF;

        private void Awake()
        {


            if (null == il8nConfig)
            {
                UpdateDebug.LogError(" 没有配置国际化表格: UpdateI18NConfig");
                il8nConfig = new UpdateI18NConfig();

            }

            il8nConfig.Init();

            //设置国际化文本给messagebox
            messageBoxUI.il8nConfig = il8nConfig;

            startUpdateBtn.onClick.AddListener(StartUpdate);
            updateSlider.fillAmount = 0;
            UpdateSliderDot();

#if UNITY_EDITOR
            if (isTestDeleteUpdateFile)
                DeleteUpdateFile();
#endif

            //设置 uwa的可见性
            GameObject uwa = GameObject.Find("UWA_Launcher");
            if (uwa != null)
            {
                publishReleae = false;
                uwa.BetterSetActive(!publishReleae);
            }

            //设置跳过按钮的可见性
            btnSkip.gameObject.BetterSetActive(!publishReleae);
            startUpdateBtn.gameObject.BetterSetActive(!publishReleae);
            if (publishReleae == false)
            {
                //监听跳过按钮
                btnSkip.onClick.AddListener(OnSkipUpdate);
            }

            if (updateClient == null)
                updateClient = GetComponent<UpdateClient>();
        }

        void Start()
        {
            //开始热更的时候打点
          //  TalkingDataAPI.OnEvent("热更_启动");

            if (null != bg)
            {
                GameObject.DontDestroyOnLoad(GameObject.Instantiate(bg));
                bg.BetterSetActive(false);
            }
            // UpdateDebug.LogError("void Start()");
            // PlayFirstEnterGameMovie(StartUpdate);
            //非发布版本，等等点击更新按钮
            //if(publishReleae==true)
            {
                StartUpdate();
            }
        }

        //内网使用功能 ： 跳过热更新
        public void OnSkipUpdate()
        {
            UpdateDebug.Log("跳过热更，直接进入游戏！");

            UpdateFlow_Verify updateFlow_Verify = UpdateEngine.Instance.GetUpdateFlow(UpdateFlow_Type.Verify) as UpdateFlow_Verify;
            if (null == updateFlow_Verify)
            {
                updateFlow_Verify = new UpdateFlow_Verify();
            }

            //检查外包是否存在版本配置信息
            //string filePath = UpdateConfig.updateConfigPath;
            //string resFile = FileManger.Instance.GetStreamingAssets() + "/" + UpdateConfig.UPDATE_CONFIG;
            //string resFile = Application.streamingAssetsPath + "/" + UpdateConfig.UPDATE_CONFIG;
            //if (File.Exists(filePath) == false)
            //{
            //    StartCoroutine(updateFlow_Verify.VersionCopy(resFile, filePath));
            //}
            //else
            //{
            //    StartCoroutine(updateFlow_Verify.VersionComparison(resFile, filePath));
            //}

            string externalPath = updateFlow_Verify.GetUpdateConfigFileExternalPath();
            if (File.Exists(externalPath))
                updateFlow_Verify.CompareVerison();
            else
                updateFlow_Verify.CopyUpdateConfigFileToExternal();

            ChangeToLogin();
        }

        // 播放第一次进入游戏视频
        public void PlayFirstEnterGameMovie(Action action)
        {
            bool isFirstEnterGame = PlayerPrefs.GetInt(IsNotFirstEnterGame, 0) != 1;

            //UpdateDebug.LogError("void isFirstEnterGame()  :  "+ isFirstEnterGame.ToString());

            if (isFirstEnterGame && fristEnterMoviePlayer)
            {
                UpdateDebug.Log("播放启动影片：" + isFirstEnterGame.ToString());

                //先保存变量
                PlayerPrefs.SetInt(IsNotFirstEnterGame, 1);

                fristEnterMoviePlayer.gameObject.BetterSetActive(true);
                fristEnterMoviePlayer.onMovieDone = (isDone) =>
                {
                    //UpdateDebug.LogError("void isFirstEnterGame()  :    fristEnterMoviePlayer.onMovieDone = (isDone) =>");
                    fristEnterMoviePlayer.gameObject.BetterSetActive(false);
                    action?.Invoke();
                };


                //UpdateDebug.LogError("void isFirstEnterGame()  :   fristEnterMoviePlayer.Play()");
                fristEnterMoviePlayer.Play();

            }
            else
            {
                //UpdateDebug.LogError("void isFirstEnterGame()  :  action?.Invoke()");

                action?.Invoke();
            }
        }

        // 启动更新
        public void StartUpdate()
        {
            startUpdateBtn.gameObject.BetterSetActive(false);
            if (CheckNetwork())
            {
                _lastDownloadTime = 0f;
                _lastDownloadComplete = 0f;

                string outterPath = Application.persistentDataPath;
                string innerPath = Application.streamingAssetsPath;
                string outterPathDll = Application.persistentDataPath + "/DLL";
                string arch_abi = "arm8";
                UpdateDebug.Log($"Update更新场景初始化\n:{arch_abi}\npersistentDataPath:{outterPath}\nstreamingAssetsPath:{innerPath}\npathDll:{outterPathDll}");
                //更新系统初始化
                updateClient.Init(outterPath, innerPath, outterPathDll, arch_abi);

                //下面是下载进度回调、解压进度回调，更新大小回调、更新错误回调，整包更新回调、更新结束回调设置

                updateClient.SetReinstallCallback(ReInstallAPP);

                updateClient.SetDownLoadProgressCallback(DownloadCallback);

                updateClient.SetUnZipProgressCallback(UnZipCallback);

                updateClient.SetUpdateFinishCallback(UpdateFinish);

                updateClient.SetUpdateErrorCallback(UpdateError);

                updateClient.SetGetUpdateSizeCallback(UpdateSize);

                updateClient.SetDownLoadFileNameCallback(FileNameCallback);

                updateClient.SetShowVerinfoCallback(ShowVerinfo);
                updateClient.SetShowTipsCallback(ShowTips);

                updateClient.StartUpdate();
            }
            else
            {
                //网络不可达的时候打点
               // TalkingDataAPI.OnEvent("热更_网络检测不通");
            }
        }


        /// <summary>
        /// 检查网络状况
        /// </summary>
        public bool CheckNetwork()
        {
            //这里不判断网络是否通，Application.internetReachability藐似不可靠
            return true;
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    messageBoxUI.Show(il8nConfig.GetLangString("当前网络不可用，请检查您的网络状态"), () => { StartUpdate(); /*startUpdateBtn.gameObject.BetterSetActive(true);*/ });
                    return false;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return true;
            }
            return false;
        }

        public void SetFinishCallback(Action<int> callback)
        {
            finishCallback = callback;
        }

        private float _lastDownloadTime = 0f;
        private float _lastDownloadComplete = 0f;
        private float _lastDownloadSpeed = 0f;
        private float _lastDownloadTotal = 0f;

        /// <summary>
        /// 下载进度回调
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="total"></param>
        private void DownloadCallback(float complete, float total)
        {
            complete /= 1024f;
            total /= 1024f;
            //updateProgress.text = updateSlider.value * 100 + "%";
            var curTime = Time.realtimeSinceStartup;
            if (curTime - _lastDownloadTime > 0.8f)
            {
                var newSpeed = (complete - _lastDownloadComplete) / (curTime - _lastDownloadTime);
                var avgSpeed = (_lastDownloadSpeed + newSpeed) / 2f;
                _lastDownloadSpeed = avgSpeed < 0.1f ? 0 : avgSpeed;

                _lastDownloadTime = curTime;
                _lastDownloadComplete = complete;
                _lastDownloadTotal = total;
            }
            //UpdateDebug.Log($"下载进度 total:{total}  complete:{complete} _lastDownloadComplete:{_lastDownloadComplete}");
            //UpdateDebug.Log($"下载进度 速度：{_lastDownloadSpeed}KB/s\ntotal:{total}  complete:{complete} _lastDownloadComplete:{_lastDownloadComplete}\ncurTime:{curTime}   _lastDownloadTime:{_lastDownloadTime}");
            SetDownloadUI(complete, total, _lastDownloadSpeed);

        }

        private void SetDownloadUI(float complete, float total, float speed)
        {
            string tips = il8nConfig.GetLangString("下载最新版本中");


            updateSlider.fillAmount = total == 0 ? 0 : complete / total;
            UpdateSliderDot();

            complete = complete / 1024;
            total = total / 1024;

            string cur = string.Format("{0:F2}", complete);
            string max = string.Format("{0:F2}", total);

            updateProgress.text = tips + $" {cur}M/{max}M";//({updateSlider.value * 100f}%) {speed}KB/s";

            //updateProgress.text = tips + $" [{complete}KB/{total}KB]({updateSlider.value * 100f}%) {speed}KB/s";

        }

        /// <summary>
        /// 解压进度回调
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="total"></param>
        private void UnZipCallback(int complete, int total)
        {
            float value = total == 0 ? 0 : complete * 1.0f / total;
            updateSlider.fillAmount = value;
            UpdateSliderDot();
            //updateProgress.text = updateSlider.value * 100 + "%";

            string tips = il8nConfig.GetLangString("正在解压资源");

            string progress = string.Format("{0:F2}", updateSlider.fillAmount * 100f);


            updateProgress.text = tips + " " + progress + "%";
            //UpdateDebug.Log(updateProgress.text);
        }

        /// <summary>
        /// 更新完成回调
        /// </summary>
        /// <param name="state"></param>
        /// <param name="restart"></param>
        /// <param name="msg"></param>
        private void UpdateFinish(bool state, bool restart, string msg, bool isUpdated)
        {
            //固件都没有下载完成，这种是强行修正的
            if (UpdateConfig.isNeedDownloadFixed)
            {
                UpdateError("多次下载错误。。。。", EnUpdateErrorReason.DownLoadFail);
                return;
            }


            //热更完成时候打点
            //TalkingDataAPI.OnEvent("热更_完成");

            //更新当前版本显示
            UpdateVersion();

            UpdateDebug.Log("热更下载完成 isUpdated:" + isUpdated + "state:" + state + ",restart:" + restart + ",msg:" + msg);
            if (restart)
            {
                // updateIL2CPP.DoUpdate((isSuccess, desc) =>
                {
                    bool isSuccess = true;
                    string desc = il8nConfig.GetLangString("本次更新需要重启客户端");
                    //updateSlider.gameObject.BetterSetActive(false);
                    if (isSuccess)
                    {
                        string tips = il8nConfig.GetLangString("更新完成");
                        updateProgress.text = tips;
                        messageBoxUI.Show(tips + "！\n" + desc, () =>
                        {

#if UNITY_ANDROID
                            BaseSDK_Android sdk = new BaseSDK_Android();
                            sdk.Initialize(this.gameObject.name, "OnRestartCallback");
                            sdk.RestartApp();
#endif

                        });
                    }
                    else
                    {
                        string tips = il8nConfig.GetLangString("更新失败");
                        updateProgress.text = tips;
                        messageBoxUI.Show(tips + "：\n" + desc);
                    }
                }
                /*
                ,
                (progress, desc) =>
                {
                    updateProgress.text = $"{desc}({progress * 100}%)";
                });
                */
            }
            else
            {
                updateSlider.fillAmount = 1;
                UpdateSliderDot();

                //updateSlider.gameObject.BetterSetActive(false);
                updateProgress.gameObject.BetterSetActive(true);

                if (!isUpdated)
                {
                    string tips = il8nConfig.GetLangString("当前已是最新版本");
                    updateProgress.text = tips;

                }
                else
                {
                    updateNextVersion.gameObject.BetterSetActive(false);
                    updateProgress.text = il8nConfig.GetLangString("更新完成");
                }

                ChangeToLogin();
                Update();
            }
        }

        private void UpdateSliderDot()
        {
            Vector2 oldPos = sliderDot.anchoredPosition;
            sliderDot.anchoredPosition = new Vector2(updateSlider.preferredWidth * updateSlider.fillAmount, oldPos.y);
        }

        /// <summary>
        /// 加载登录场景
        /// </summary>
        public void ChangeToLogin()
        {
            //避免多次进入
            if (m_bChangeLogin)
            {
                UpdateDebug.LogWarning("ChangeToLogin 重复进入,正在切回场景");
                return;
            }

            m_bChangeLogin = true;

            //通知外包热更完成
            if (null != finishCallback)
            {
                finishCallback.Invoke((int)UPDATE_STATE.STATE_UPDATE_FINISH);
            }

            UpdateDebug.LogWarning("同步关键文件资源");
            SyncFile2External.Execute(gameObject, UpdateConfig.m_AssetFileName, () =>
            {
                GameObject loginGo = null;

                IGAssetLoader loaderMgr = LoadSystem.CreateLoadSystem();

                //监控资源加载
#if UNITY_EDITOR
                if (RecordResTool.g_RecordResTools != null)
                {
                    RecordResTool.g_RecordResTools.Init(loaderMgr);
                }
#endif
                
                LoadDll.Load();

                //构建XGameApp环境
                var buildContext = new XGameAppEnvBuilderContext();
                buildContext.configFilePath = "GameConfig.ini";
                XGameAppEnv.Build(buildContext, (isSuc)=> {
                    if (isSuc)
                    {
                        UpdateDebug.LogWarning("加载 启动预制体:"+ strPrefabName);

                        uint handle = 0;
                        UnityEngine.Object res = loaderMgr.LoadResSync<UnityEngine.Object>(strPrefabName, out handle, false) as UnityEngine.Object;

#if UNITY_EDITOR
                        if (null == res)
                        {
                            res = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/" + strPrefabName);
                        }
#endif
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

                            UpdateDebug.Log("加载启动LoginUI成功: " + strAssetBundleName);
                        }
                        else
                        {
                            UpdateDebug.Log("加载启动LoginUI失败: " + strAssetBundleName);
                        }

                        //通知外部初始化完成
                        if (null != finishCallback)
                        {
                            finishCallback.Invoke((int)UPDATE_STATE.STATE_UPDATE_GAME_INI_FINISH);
                        }
                    }
                    else
                    {
                        UpdateDebug.Log("加载启动LoginUI失败: " + strAssetBundleName);
                    }
                });

            }, false);
        }

        /// <summary>
        /// 更新错误回调
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateError(string msg, EnUpdateErrorReason reason)
        {
            //热更错误的时候打点
            string errorCode = "" + (int)reason;
            //TalkingDataAPI.OnEvent("热更_出错","错误码", errorCode);
            Debug.LogError("热更_出错 :ErrorCode=" + errorCode);

            //只要不是固件下载失败，统一当下载完成处理
            if (UpdateConfig.isNeedDownloadFixed==false)
            {
               
                //MMPEventAPI.OnEvent("热更_强行完成", "错误码", errorCode);
               // TalkingDataAPI.OnEvent("热更_强行完成", "错误码", errorCode);
                Debug.LogError("热更_强行完成 :ErrorCode="+ errorCode);
                UpdateFinish(true, false, "", true);
                return;
            }

           

            switch (reason)
            {
                case EnUpdateErrorReason.UnzipFail:
                    messageBoxUI.Show(il8nConfig.GetLangString("解压热更资源失败，请检查您的网络状态"), () => { StartUpdate(); /*startUpdateBtn.gameObject.BetterSetActive(true);*/ });
                    break;
                case EnUpdateErrorReason.DownLoadFail:
                    SetDownloadUI(_lastDownloadComplete, _lastDownloadTotal, 0);
                    messageBoxUI.Show(il8nConfig.GetLangString("下载热更资源失败，请检查您的网络状态"), () => { StartUpdate(); /*startUpdateBtn.gameObject.BetterSetActive(true);*/ });
                    break;
                case EnUpdateErrorReason.GetVerionFileFail:
                    updateProgress.text = string.Empty;
                    messageBoxUI.Show(il8nConfig.GetLangString("检查热更版本失败，请检查您的网络状态"), () => { StartUpdate(); /*startUpdateBtn.gameObject.BetterSetActive(true);*/});
                    break;
                case EnUpdateErrorReason.GetFixUpdateFileFail:
                    updateProgress.text = string.Empty;
                    messageBoxUI.Show(il8nConfig.GetLangString("下载固件版本文件失败，请检查您的网络状态"), () => { StartUpdate(); /*startUpdateBtn.gameObject.BetterSetActive(true);*/ });
                    break;
                case EnUpdateErrorReason.GetVerUpdateFileFail:
                    updateProgress.text = string.Empty;
                    messageBoxUI.Show(il8nConfig.GetLangString("下载热更版本文件失败，请检查您的网络状态"), () => { StartUpdate(); /*startUpdateBtn.gameObject.BetterSetActive(true);*/ });
                    break;
                default:
                    string tips1 = il8nConfig.GetLangString("本次更新出现错误,需要下载安装包,错误码");
                    string tips2 = il8nConfig.GetLangString("错误信息");
                    messageBoxUI.Show($"{tips1}:{reason}\n{tips2}：{msg}", OpenApkURL, null, il8nConfig.GetLangString("版本更新"), false); //, QuitGame
                    break;
            }
        }

        /// <summary>
        /// 获取更新大小回调
        /// </summary>
        /// <param name="size"></param>
        private void UpdateSize(float size)
        {

            string tips = il8nConfig.GetLangString("当前处于流量连接状态");

            string networkState = string.Empty;
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                networkState = $"<color=#ff0000>{tips}</color>";
            }

            tips = il8nConfig.GetLangString("本次更新需要下载");

            messageBoxUI.Show($"{tips}【<color=#ff0000>" + size + "</color>KB】，是否确认下载？\n" + networkState,
                () => { updateClient.Run(); },
                () => { StartUpdate();/* startUpdateBtn.gameObject.BetterSetActive(true);*/ });
        }

        private void FileNameCallback(List<string> fileNames)
        {
            return;
            /*
            List<UpdateMicroData> microUpdateList = new List<UpdateMicroData>();
            if (fileNames != null)
            {
                string fileName, fullPath;
                for (int i = 0; i < fileNames.Count; i++)
                {
                    fullPath = fileNames[i].Replace("//", "/");
                    fileName = Path.GetFileName(fileNames[i]);
                    Debug.LogFormat("=====>>>HotUpdate: {0}  {1}", fileName, fullPath);
                    microUpdateList.Add(new UpdateMicroData() { filefullPath = fullPath, filename = fileName });
                }
            }
            // 通知微端更新一下本地配置表
            //MicroClientUtil.Instance.ModifyLocalFileIndex(microUpdateList);
            */
        }

        /// <summary>
        /// 整包更新回调
        /// </summary>
        /// <param name="url"></param>
        private void ReInstallAPP(string url, string reason)
        {

            //编辑器模式下跳过重新更新，直接进入游戏

            {
                string tips1 = il8nConfig.GetLangString("本次更新需要下载");
                string tips2 = il8nConfig.GetLangString("原因");
                reason = il8nConfig.GetLangString(reason);
                apkURL = url;
                messageBoxUI.Show($"{tips1}\n{tips2}：{reason}", OpenApkURL, null, il8nConfig.GetLangString("版本更新"), false); //QuitGame
            }
        }

        /// <summary>
        /// 版本信息回调
        /// </summary>
        /// <param name="localVer"></param>
        /// <param name="updateVer"></param>
        private void ShowVerinfo(string localVer, string updateVer)
        {
            //textCurVersion.text = "当前版本号:" + localVer;
            //updateVerText.text = "更新版本号:" + updateVer;

            if (textCurVersion.gameObject.activeSelf == false)
            {
                textCurVersion.gameObject.BetterSetActive(true);
            }

            if (updateNextVersion.gameObject.activeSelf == false)
            {
                updateNextVersion.gameObject.BetterSetActive(true);
            }

            textCurVersion.text = il8nConfig.GetLangString("当前版本") + ":" + localVer;
            updateNextVersion.text = il8nConfig.GetLangString("更新版本号") + ":" + updateVer;
        }

        private void ShowTips(string decs)
        {
            if (null == updateProgress)
            {
                return;
            }

            updateProgress.gameObject.BetterSetActive(true);
            updateProgress.text = il8nConfig.GetLangString(decs);

            
        }

        private void OpenApkURL()
        {
            if (!string.IsNullOrEmpty(apkURL))
            {
                Application.OpenURL(apkURL+"?ver="+DateTime.Now.Ticks);
            }
            //startUpdateBtn.gameObject.BetterSetActive(true);
        }

        private void QuitGame()
        {
            UpdateDebug.LogError("点击退出游戏");
            Application.Quit();
        }

        float m_lastShowTime = -20;
        private void Update()
        {
            //滚动提示
            if (m_bChangeLogin)
            {
                if (listTips.Count > 0)
                {
                    if (Time.realtimeSinceStartup - m_lastShowTime > 2.0f)
                    {
                        m_lastShowTime = Time.realtimeSinceStartup;

                        int nIndex = ((int)(m_lastShowTime * 100)) % listTips.Count;
                        ShowTips(listTips[nIndex]);
                    }
                }

            }

            //更新当前版本显示
            UpdateVersion();

        }

        private void UpdateVersion()
        {
            //显示版本号
            if (textCurVersion && cur_Version != UpdateConfig.cur_Version)
            {
                cur_Version = UpdateConfig.cur_Version;
                if (cur_Version > 0)
                {
                    textCurVersion.text = il8nConfig.GetLangString("当前版本:") + cur_Version;
                }
                else
                {
                    textCurVersion.text = "";
                }
            }
        }

        /// <summary>
        /// 测试使用，删除下载的文件
        /// </summary>
        private void DeleteUpdateFile()
        {
            UpdateDebug.Log("DeleteUpdateFile........");
            PlayerPrefs.SetInt(IsNotFirstEnterGame, 0);
            string path = Application.persistentDataPath + "/Data";
            string pathDll = Application.persistentDataPath + "/DLL";
            string fileName = Application.persistentDataPath + "/updateconfig.xml";
            DeleteDir(path);
            DeleteDir(pathDll);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        private void OnRestartCallback()
        {
        }

        private void DeleteDir(string file)
        {
            try
            {
                //判断文件夹是否还存在
                if (Directory.Exists(file))
                {
                    //去除文件夹和子文件的只读属性
                    //去除文件夹的只读属性
                    System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                    fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                    //去除文件的只读属性
                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {

                        if (File.Exists(f))
                        {
                            //如果有子文件删除文件
                            File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f);
                        }

                    }

                    //删除空文件夹

                    Directory.Delete(file);

                }

            }
            catch (Exception ex) // 异常处理
            {
                Debug.LogError(ex.Message.ToString());// 异常信息
            }
        }
    }
}

