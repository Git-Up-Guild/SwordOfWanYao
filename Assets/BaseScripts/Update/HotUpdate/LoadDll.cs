using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Pool;
using XGame.Asset;
using XGame.Asset.Load;

namespace XGame
{
    public class LoadDll
    {
        public static string DllPath = "G_Resources/CodeUpdate/";

        public static List<string> DllNames = new List<string>
        {
                "XGameEngine",
                "DOTween-Scripts",
                "spine-unity",
                "spine-timeline",
                "uhypertext-scrpits",
                "Assembly-CSharp",
        };

        private static List<string> AOTMetaAssemblyFiles
        {
            get;
        } = new List<string>()
        {
                "DOTween.dll",
                "System.Core.dll",
                "System.dll",
                "Unity.RenderPipelines.Core.Runtime.dll",
                "UnityEngine.CoreModule.dll",
                "XGameBase.dll",
                "XGameEngine.dll",
                "mscorlib.dll",
        };

        private static IGAssetLoader loaderMgr;

        private static Assembly hotUpdateAss;


        public static void Load()
        {
            // 如果不开启热更新，直接返回
            Debug.Log($"是否开启热更新{UpdateConfig.isEnableHybridCLR}");
            if (!UpdateConfig.isEnableHybridCLR)
            {
                return;
            }

            List<uint> listHandles = ListPool<uint>.Get();  

            loaderMgr = LoadSystem.CreateLoadSystem();
            LoadMetadataForAOTAssemblies();
            foreach (var dllName in DllNames)
            {
                Assembly hotUpdateAss = null;
#if UNITY_EDITOR

                // Editor下无需加载，直接查找获得HotUpdate程序集
                hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies()
                                     .First(a => a.GetName().Name == dllName);
#else
                uint handle = 0;
                byte[] dllBytes = LoadDllBytes(dllName,out handle);
                listHandles.Add(handle);
                if (dllBytes == null || dllBytes.Length == 0)
                {
                    //loaderMgr.UnloadRes(handle);
                    Debug.LogError($"Failed to load DLL: {dllName}");
                    continue;
                }



                hotUpdateAss = Assembly.Load(dllBytes);
               // loaderMgr.UnloadRes(handle);
#endif
                Debug.Log($"LoadHotUpdateAssembly:{dllName}");
            }

            //释放ab
            foreach(uint handle in listHandles)
            {
                loaderMgr?.UnloadRes(handle);
            }
            ListPool<uint>.Release(listHandles);


           Debug.Log("加载热更程序集完成");
        }

        public static byte[] LoadDllBytes(string dllName,out uint handle)
        {
             handle = 0;
            string assetPath = $"{DllPath}{dllName}.bytes";
            TextAsset res =
                    loaderMgr.LoadResSync<TextAsset>(assetPath, out handle, false) as TextAsset;

#if UNITY_EDITOR
            if (null == res)
            {
                res = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{assetPath}");
            }

#endif
            if (res != null)
            {
                byte[] dllBytes = res.bytes;
                return dllBytes;
            }

            return null;
        }

        private static void LoadMetadataForAOTAssemblies()
        {
            /*
                        // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
                        // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
                        HomologousImageMode mode = HomologousImageMode.SuperSet;
                        foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
                        {
                            string name = aotDllName.Replace(".dll", ""); // 替换掉 .dll 扩展名
                            byte[] dllBytes = LoadDllBytes(name);
                            if (dllBytes == null || dllBytes.Length == 0)
                            {
                                Debug.LogError($"Failed to load DLL: {name}");
                                continue;
                            }
            
                            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                            Debug.Log($"LoadMetadataForAOTAssembly:{name} mode:{mode} ret:{err}");
                        }
                */
        }
    }
}