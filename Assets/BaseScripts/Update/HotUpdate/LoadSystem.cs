/*******************************************************************
** 文件名:	LoadSystem.cs
** 版  权:	(C) 
** 创建人:	许德纪
** 日  期:	2021/1/13
** 版  本:	1.0
** 描  述:	
** 应  用:  帮助初始化加载系统

********************************************************************/

using DG.Tweening.Plugins.Core.PathCore;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TextCore.Text;
using XGame.Asset;
using XGame.Asset.Load;

namespace XGame
{
    public class LoadSystem
    {
        //全局依赖文件
        static private AssetBundleManifest m_globalManifest = null;

        //ab包的路径
        static private List<string> m_listABDir;

        //是否检查存在
        static private bool[] m_checkExist = new bool[2];

        //获取全局依赖对象
        static public AssetBundleManifest GetManifest()
        {
            string platformDir = GetPlatformFolderForAssetBundles(Application.platform);

            m_listABDir = new List<string>();
            m_listABDir.Add(UpdateConfig.appHotUpdateDir);
            m_listABDir.Add(UpdateConfig.appInnerDir);
            m_checkExist[0] = true;
            m_checkExist[1] = false;

            return null;

            if (null == m_globalManifest)
            {
                int nCount = m_listABDir.Count;
                string path = null;
                for (int i = 0; i < nCount; ++i)
                {
                    path = m_listABDir[i] + UpdateConfig.MANIFEST_NAME_DAT;
                    if (m_checkExist.Length > i && m_checkExist[i] || Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        if (!File.Exists(path))
                        {
                            continue;
                        }
                    }
                    AssetBundle manifestAB = AssetBundle.LoadFromFile(path);
#if DEBUG_LOG
                    ///#                    ///#                    Debug.Log("加载android");
#endif
                    if (null == manifestAB)
                    {
#if DEBUG_LOG
                        ///#                        ///#                        Debug.Log("不存在" + path);
#endif
                        //return null;
                        continue;
                    }

                    m_globalManifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    if (null != m_globalManifest)
                    {
#if DEBUG_LOG
                        ///#                        ///#                        Debug.Log("加载 AssetBundleManifest 成功" + path);
                        ///
#endif
                        manifestAB.Unload(false);
                        break;
                    }
                    else
                    {
                        Debug.LogError("加载 AssetBundleManifest 失败" + path);
                        break;
                    }

                }

            }
            return m_globalManifest;
        }

        //创建加载系统
        static public IGAssetLoader CreateLoadSystem()
        {
            Debug.Log("Init LoaderMgr");
            //IGAssetLoader loaderMgr = Q1GameComs.Reg<IGAssetLoader, LoadMgr>(ComBehaviorFlag.Update);

            var loaderMgr = new LoadMgr();
            loaderMgr.Create(null);

            Debug.Log("Init Asset2BundleRecords");
            IAsset2Bundle asset2bundle = null;
            string Asset2BundleRecordsPath = UpdateConfig.appHotUpdateDir + UpdateConfig.ASSET_BUNDLE_RECORD;
            if (File.Exists(Asset2BundleRecordsPath))
            {
                Debug.Log("读取: " + Asset2BundleRecordsPath);
                asset2bundle = new Asset2BundleRecords();
                asset2bundle.Load(Asset2BundleRecordsPath);
                loaderMgr.SetAsset2BundleRecords(asset2bundle);

            }


            if (null == asset2bundle)
            {
                Debug.LogWarning("null== asset2bundle");
                loaderMgr.SetAsset2BundleRecords(new Asset2BundleRecords());
            }

            //设置全局依赖
            m_globalManifest = GetManifest();

            /*
            if (null == m_globalManifest)
            {
                Debug.LogError("全局依赖表为NULL,null== m_globalManifest");
            }*/

            loaderMgr.SetManifest(m_globalManifest);
            Debug.Log("Init m_globalManifest");

            ILoadSystem loaderSystem = null;
            LoadMode loadMode = LoadMode.Development;

#if (UNITY_EDITOR && !EDITOR_ASSET_BUNDLE_RES_MODE) || DLL_SUPPORT_EDITOR

            //开发模式加载系统
            if (loadMode == LoadMode.Development)
            {
                Asset2BundleRecordsDev a2bRecores = new Asset2BundleRecordsDev();
                loaderMgr.SetAsset2BundleRecords(a2bRecores);
                LoadSystemContext loadContext = new LoadSystemContext();
                loadContext.baseDir = "Assets/";

                loaderSystem = new DevLoadSystem();
                loaderSystem.Create(loadContext, 0);
                loaderMgr.AddLoadSystem(loaderSystem);
            }
            else
            {
                string strEditorDir = Application.dataPath + "/";
                loaderSystem = new EditorLoadSystem();
                LoadSystemContext editorLoadContext = new LoadSystemContext();
                loaderSystem.Create(editorLoadContext);
                loaderMgr.AddLoadSystem(loaderSystem);
#if DEBUG_LOG
                ///#                //Debug.Log("编辑器加载系统 >> 初始化完毕！");
#endif
            }

#endif

            //初始化热更新加载系统
            loaderSystem = new AssetBundleLoadSystem();
            LoadSystemContext updateLoadContext = new LoadSystemContext();
            updateLoadContext.bCheckExist = true;
            updateLoadContext.baseDir = UpdateConfig.appHotUpdateDir;
            loaderSystem.Create(updateLoadContext);
            loaderMgr.AddLoadSystem(loaderSystem);

            Debug.Log("Init WebLoadInfoConfig");
            WebLoadInfoConfig webCfg = null;
            string WebcfgPath = UpdateConfig.appHotUpdateDir + UpdateConfig.WEB_INFO_CFG;
            if (File.Exists(WebcfgPath))
            {
                webCfg = WebLoadInfoConfig.Load(WebcfgPath);
            }

            loaderSystem = new WebLoadSystem();
            WebLoadSystemContext webLoadContext = new WebLoadSystemContext();
            webLoadContext.bCheckExist = true;
            webLoadContext.baseDir = UpdateConfig.appWebDir;
            webLoadContext.isCloseWifi = false;
            webLoadContext.webinfoCfg = webCfg;
            webLoadContext.urlDir = UpdateConfig.webDownloadUrl + UpdateConfig.WEB_REAL_DIR + "/";
            loaderSystem.Create(webLoadContext);
            loaderMgr.AddLoadSystem(loaderSystem);

            //初始化apk内包加载系统
            loaderSystem = new AssetBundleLoadSystem();
            LoadSystemContext innerLoadContext = new LoadSystemContext();
            innerLoadContext.bCheckExist = false;
            innerLoadContext.baseDir = UpdateConfig.appInnerDir;
            loaderSystem.Create(innerLoadContext);
            loaderMgr.AddLoadSystem(loaderSystem);
            loaderMgr.Start();

            Debug.Log("finish InitLoadSystem");

            return loaderMgr;
        }

        private static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.WindowsPlayer:
                    return "windows";
                case RuntimePlatform.OSXPlayer:
                    return "osx";
                // Add more build platform for your own.
                // If you add more platforms, don't forget to add the same targets to GetPlatformFolderForAssetBundles(BuildTarget) function.
                default:
#if UNITY_ANDROID
                    return "android";
#elif UNITY_IOS
                    return "ios";
#elif UNITY_STANDALONE_OSX
                    return "osx";
#elif UNITY_STANDALONE
                    return "windows";
#else
                    return null;
#endif
            }
        }
    }
}
