/*******************************************************************
** 文件名:	ResourcesManager.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	郑秀程
** 日  期:	2018.6.22
** 版  本:	1.0
** 描  述:	
** 应  用:  通用资源管理器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.Load;
using UnityEngine;
using XGame.Asset;
using XGame.Poolable;
using XGame;

namespace XClient.Scripts.Api
{
    public class ResourcesType
    {
        public static int CUR = 0;
        public static readonly int MIN = CUR;
        public static readonly int DEF = CUR++;
        public static readonly int PREFAB = CUR++;
        public static readonly int SPRITE = CUR++;
        public static readonly int TEX2D = CUR++;
        public static readonly int MAT = CUR++;
        public static readonly int SHADER = CUR++;
        public static readonly int MAX = CUR;
    }

    public class ResourcesManager
    {
        //加载器列表
        private IResourcesLoader[] m_arrLoaders;

        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        public bool Create()
        {
            XGameComs.Get<IItemPoolManager>().Register<ResInfo>(50);

            m_arrLoaders = new IResourcesLoader[ResourcesType.MAX];
            m_arrLoaders[ResourcesType.DEF] = new ResourcesLoader<UnityEngine.Object>();
            m_arrLoaders[ResourcesType.PREFAB] = new ResourcesLoader<UnityEngine.GameObject>();
            m_arrLoaders[ResourcesType.SPRITE] = new ResourcesLoader<UnityEngine.Sprite>();
            m_arrLoaders[ResourcesType.TEX2D] = new ResourcesLoader<UnityEngine.Texture2D>();
            m_arrLoaders[ResourcesType.MAT] = new ResourcesLoader<UnityEngine.Material>();
            m_arrLoaders[ResourcesType.SHADER] = new ResourcesLoader<UnityEngine.Shader>();

            //初始化spine预制体的特殊加载器
           // SpineResourcesLoader.instance = m_arrLoaders[ResourcesType.SPINE_PREFAB];

            for (int i = ResourcesType.MIN; i < ResourcesType.MAX; ++i)
            {
                m_arrLoaders[i].Create();
            }

            return true;
        }

        public void Update()
        {
            for (int i = ResourcesType.MIN; i < ResourcesType.MAX; ++i)
            {
                m_arrLoaders[i].Update();
            }
        }

        /// <summary>
        /// 设置加载器
        /// </summary>
        /// <param name="loader"></param>
        public void SetLoader(IResLoader loader)
        {
            for (int i = ResourcesType.MIN; i < ResourcesType.MAX; ++i)
            {
                if(null!= m_arrLoaders[i])
                    m_arrLoaders[i].SetLoader(loader);
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Release()
        {
            for (int i = ResourcesType.MIN; i < ResourcesType.MAX; ++i)
            {
                if (null != m_arrLoaders[i])
                    m_arrLoaders[i].Release();
            }

            m_arrLoaders = null;
        }

        /// <summary>
        /// 获取加载器
        /// </summary>
        /// <param name="nID"></param>
        /// <returns></returns>
        public IResourcesLoader GetResourcesLoad(int nID)
        {
            if (nID < ResourcesType.MIN || nID >= ResourcesType.MAX)
            {
                Debug.LogError("获取资源加载器失败！！！nID=" + nID);
            }
            else
            {
                return m_arrLoaders[nID];
            }
            return null;
        }

        //dump 依赖资源
        public void DumpDepency(string bunldName,string mainBunldName)
        {
            IGAssetLoader loadMgr = XGame.XGameComs.Get<IGAssetLoader>();
            if(loadMgr is IAssetBundleLoadManager)
                (loadMgr as IAssetBundleLoadManager).DumpDepncy(bunldName,mainBunldName);
        }

        public void DumpAllAssetsName()
        {
            IGAssetLoader loadMgr = XGame.XGameComs.Get<IGAssetLoader>();
            loadMgr.DumpAllAssetsName();
        }

        public void OutputDebugInfo(string filePath)
        {
            IGAssetLoader loadMgr = XGame.XGameComs.Get<IGAssetLoader>();
            loadMgr.OutputDebugInfo(filePath);
        }
    }
}
