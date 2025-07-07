/********************************************************************
	created:	2018/03/23
	created:	23:3:2018   12:21
	filename: 	F:\Xgame_Common_source\srcCSharp\BaseToolsLib\BaseToolsLib\DataCenterLite\CDefaultFileSystem.cs
	file path:	F:\Xgame_Common_source\srcCSharp\BaseToolsLib\BaseToolsLib\DataCenterLite
	file base:	CDefaultFileSystem
	file ext:	cs
	author:		noot
	
	purpose:	DataCenterLite 默认使用的内部文件系统
*********************************************************************/

using basetools;
using XGame;
using XGame.Asset;
using XGame.UnityObjPool;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using System.Text;

namespace XClient.Client.Scheme
{
    class CDefaultFileSystem : IExternalFileSystem
    {
        private readonly string AssetsFolder = "G_Resources/Game/GameScp";
        private string m_strLastError = "";

        //发布版本lua资源
        private Dictionary<string, byte[]> m_dicGameScpBytes;

        //加载的包名
        private static string m_bundleName = "g_resources/game/gamescp.bin";

        private string GetResourcePath(string strFilePath)
        {
            string filePath = strFilePath.Substring(strFilePath.IndexOf(AssetsFolder));
            filePath = filePath.Replace(".dat", ".bytes").Replace("//", "/");
            return filePath;
        }

        //从Resources系统中加载脚本
        private byte[] ReadFileFromResources(IGAssetLoader assetLoader, string strFilePath)
        {
            string filePath = GetResourcePath(strFilePath);
            filePath = GamePath.ToResourceLoadPath(filePath);

            uint handle = 0;
            byte[] content = null;

            var asset = assetLoader.LoadResSync<TextAsset>(filePath, out handle) as TextAsset;
            if (asset != null)
                content = asset.bytes;

            assetLoader.UnloadRes(handle);

            return content;
        }

        //从AB系统中加载脚本
        private byte[] ReadFileFromAssetBundle(IGAssetLoader assetLoader, string strFilePath)
        {
            string filePath = GetResourcePath(strFilePath);

           // Debug.Log("加载配置文件，path=" + filePath);

            String fileName = Path.GetFileNameWithoutExtension(filePath);
            byte[] arrContant = null;

            if (null == m_dicGameScpBytes)
            {
                uint handle = 0;
                m_dicGameScpBytes = new Dictionary<string, byte[]>();

                UnityEngine.Object[] allLuaScript = assetLoader.LoadResSync<UnityEngine.Object>(m_bundleName, out handle, true) as UnityEngine.Object[];

                TextAsset data = null;
                int nLen = allLuaScript.Length;
                for (int i = 0; i < nLen; ++i)
                {
                    data = allLuaScript[i] as TextAsset;
                    m_dicGameScpBytes.Add(allLuaScript[i].name, data.bytes);
                }

                assetLoader.UnloadRes(handle);
            }

            if (m_dicGameScpBytes.ContainsKey(fileName))
            {
                arrContant = m_dicGameScpBytes[fileName];
                m_dicGameScpBytes.Remove(fileName);
            }
            else
            {
                string error = string.Format("cannot Load assetbunlde {0}", filePath);
                XGame.Trace.TRACE.ErrorLn(error);
            }

            return arrContant;
        }

        //在编辑器中加载脚本
        private byte[] ReadFileFromEditor(string strFilePath)
        {
            if (File.Exists(strFilePath))
            {
                return File.ReadAllBytes(strFilePath);
            }

            string fileEditorPath = strFilePath.Replace(".dat", ".bytes").Replace("//", "/");

            if (File.Exists(fileEditorPath))
            {
                return File.ReadAllBytes(fileEditorPath);
            }
            else
            {
                return null;
            }
        }


        public byte[] ReadAllBytes(string strFilePath)
        {
#if UNITY_EDITOR
            return ReadFileFromEditor(strFilePath);
#else
            IGAssetLoader assetLoader = XGame.XGameComs.Get<IGAssetLoader>();
            if (assetLoader != null)
            {
                if (assetLoader is IAssetBundleLoadManager)
                    return ReadFileFromAssetBundle(assetLoader, strFilePath);
                else
                    return ReadFileFromResources(assetLoader, strFilePath);
            }
            else
            {
                return null;
            }
#endif
        }


        public bool IsFileExist(string strFilePath)
        {

#if UNITY_EDITOR
            //Debug.LogError("IsFileExist: " + strFilePath);
            if (File.Exists(strFilePath))
            {
                return true;
            }

            string fileEditorPath = strFilePath.Replace(".dat", ".bytes").Replace("//", "/");
            //Debug.LogError("IsFileExist: " + fileEditorPath);

            return File.Exists(fileEditorPath);
#endif

            string filePath = GetResourcePath(strFilePath);
            String fileName = Path.GetFileNameWithoutExtension(filePath);
            if(null!= m_dicGameScpBytes)
            {
                return m_dicGameScpBytes.ContainsKey(fileName);
            }

            uint resHandle = 0;
            IUnityObjectPool unityObjectPool = XGame.XGameComs.Get<IUnityObjectPool>();
            TextAsset pAsset = unityObjectPool.LoadRes<TextAsset>(filePath, out resHandle, true);
            return pAsset != null;
        }

        public bool WriteAllBytes(string strFilePath, byte[] arrMBS)
        {
            throw new Exception("表数据 不准写！！strFilePath " + strFilePath);
        }

        public string GetLastError()
        {
            return m_strLastError;
        }
    }
}
