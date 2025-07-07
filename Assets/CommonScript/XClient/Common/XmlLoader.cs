using XGame;
//using XGame.Asset.Loader;
using XGame.UnityObjPool;
using XGame.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Common
{
    public class XmlLoader
    {
        private static string s_strOutSidePath = "";
        public static string tag = "";

        //========================================================= //
        /// <summary>
        /// 读取散文件的加载路劲
        /// </summary>
        private static void InitOutSidePath()
        {
            string workdir = XGame.GamePath.GetWorkDir();
            string fileName = workdir + "/config.txt";
            if (System.IO.File.Exists(fileName) == false)
            {
                return;
            }
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            if (sr == null)
            {
                return;
            }
            s_strOutSidePath = sr.ReadToEnd();
            sr.Close();

            if ((s_strOutSidePath.Length >= 3) && (s_strOutSidePath.Substring(0, 3) == "../"))
            {
                s_strOutSidePath = s_strOutSidePath.Substring(3);
                workdir = workdir.Substring(0, workdir.LastIndexOf('/') + 1);
            }
            s_strOutSidePath = workdir + s_strOutSidePath;
        }

        public static string GetXmlFile(string name)
        {
            string strPath;
            string text = "";

            if (s_strOutSidePath.Length <= 0)
            {
                InitOutSidePath();
            }

            tag = name;

            if ((name.Length >= 3) && (name.Substring(0, 3) == "../"))
            {
                int len = name.Length;
                name = name.Substring(3, len - 3);
            }
            else
            {
                name = "G_Resources/Game/scp/" + name + ".xml";
            }

            if (Platform.IsMobilePlatform())
            {
                strPath = Application.persistentDataPath + "/" + name;
            }
            else
            {
                strPath = s_strOutSidePath + "/" + tag + ".xml";
            }

#if DEBUG_LOG
///#///#            //XGame.Trace.TRACE.TraceLn("strPath = " + tag + ".xml");
#endif
            // 如果外部存在散文件，则加载散文件
            if (System.IO.File.Exists(strPath) == true)
            {
                try
                {
#if DEBUG_LOG
///#///#                    //XGame.Trace.TRACE.TraceLn("加载散文件：" + strPath);
#endif
                    FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader sr = new StreamReader(fs);
                    if (sr == null)
                    {
                        return "";
                    }
                    text = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                    XGame.Trace.TRACE.ErrorLn("加载散文件失败！ error = " + ex);
                }
            }
            // 从Resource中加载
            else
            {
                //TextAsset pAsset = XGame.XGameComs.Get<XGame.Asset.IGAssetLoader>().Load<TextAsset>(name);
                uint resHandle = 0;
                IUnityObjectPool unityObjectPool = XGame.XGameComs.Get<IUnityObjectPool>();
                TextAsset pAsset = unityObjectPool.LoadRes<TextAsset>(name, out resHandle, true);
                if (pAsset == null)
                {
                    unityObjectPool.UnloadRes(resHandle);
                    return "";
                }
                text = pAsset.text;
                //XGame.XGameComs.Get<XGame.Asset.IGAssetLoader>().UnLoad(name);
                unityObjectPool.UnloadRes(resHandle);
            }
            return text;
        }

        public static string GetXmlFileByPath(string szPath)
        {
            string strPath = string.Format("{0}{1}", szPath, ".xml");
            string text = "";

#if DEBUG_LOG
///#///#            //XGame.Trace.TRACE.TraceLn("strPath = " + tag + ".xml");
#endif
            // 如果外部存在散文件，则加载散文件
            if (System.IO.File.Exists(strPath) == true)
            {
                try
                {
#if DEBUG_LOG
///#///#                    //XGame.Trace.TRACE.TraceLn("加载散文件：" + strPath);
#endif
                    FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader sr = new StreamReader(fs);
                    if (sr == null)
                    {
                        return "";
                    }
                    text = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                    XGame.Trace.TRACE.ErrorLn("加载散文件失败！ error = " + ex);
                }
            }
            // 从Resource中加载
            else
            {
                //TextAsset pAsset = XGame.XGameComs.Get<XGame.Asset.IGAssetLoader>().Load<TextAsset>(szPath);
                uint resHandle = 0;
                IUnityObjectPool unityObjectPool = XGame.XGameComs.Get<IUnityObjectPool>();
                TextAsset pAsset = unityObjectPool.LoadRes<TextAsset>(szPath, out resHandle, true);
                if (pAsset == null)
                {
                    unityObjectPool.UnloadRes(resHandle);
                    return "";
                }
                text = pAsset.text;
                unityObjectPool.UnloadRes(resHandle);
            }
            return text;
        }

        public static string GetXmlFileByPathEx(string szPath)
        {
            string strPath = szPath;
            string text = "";

#if DEBUG_LOG
///#///#            //XGame.Trace.TRACE.TraceLn("strPath = " + tag + ".xml");
#endif
            // 如果外部存在散文件，则加载散文件
            if (System.IO.File.Exists(strPath) == true)
            {
                try
                {
#if DEBUG_LOG
///#///#                    //XGame.Trace.TRACE.TraceLn("加载散文件：" + strPath);
#endif
                    FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader sr = new StreamReader(fs);
                    if (sr == null)
                    {
                        return "";
                    }
                    text = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                    XGame.Trace.TRACE.ErrorLn("加载散文件失败！ error = " + ex);
                }
            }
            // 从Resource中加载
            else
            {
                uint resHandle = 0;
                //TextAsset pAsset = XGame.XGameComs.Get<XGame.Asset.IGAssetLoader>().Load<TextAsset>(szPath);
                IUnityObjectPool unityObjectPool = XGame.XGameComs.Get<IUnityObjectPool>();
                TextAsset pAsset = unityObjectPool.LoadRes<TextAsset>(szPath, out resHandle, true);
                if (pAsset == null)
                {
                    unityObjectPool.UnloadRes(resHandle);
                    return "";
                }
                text = pAsset.text;
                unityObjectPool.UnloadRes(resHandle);
            }
            return text;
        }

        public static XMLNode GetXmlReader(string szFileName)
        {
            if (string.IsNullOrEmpty(szFileName))
            {
                return null;
            }

            string text = GetXmlFile(szFileName);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            return Function.GetXmlReaderByData(text);
        }

        public static XMLNode GetXmlReaderByPath(string szPath, bool bDecrypt)
        {
            if (string.IsNullOrEmpty(szPath))
            {
                XGame.Trace.TRACE.ErrorLn("GetXmlReaderByPath string.IsNullOrEmpty:" + szPath);
                return null;
            }

            if (System.IO.File.Exists(string.Format("{0}.xml", szPath)) == false)
            {
                XGame.Trace.TRACE.ErrorLn("GetXmlReaderByPath Non-Existent :" + szPath);
                return null;
            }

            string text = GetXmlFileByPath(szPath);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            if (bDecrypt)
            {
#if IOS_VEST
				text = XXTea.Decrypt(text, System.Convert.ToString(GameConfig.Key, 16));
#endif
            }

            XMLNode xn = XmlParser.Parse(text);
            if (xn == null)
            {
                XGame.Trace.TRACE.ErrorLn("Api::GetXmlReaderByPath() 读取文件错误 文件：" + szPath);
            }
            return xn;
        }
    }
}
