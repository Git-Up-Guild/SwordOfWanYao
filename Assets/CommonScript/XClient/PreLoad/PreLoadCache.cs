using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame;
using XGame.UnityObjPool;
using XGame.Utils;

namespace script
{

    //地图相关配置
    [Serializable]
    public class CachemItem
    {
        //需要加载的资源
        public ResourceRef resRef;

        //需要加载的个数
        public int count;
    }


    public class PreLoadCache : MonoBehaviour, IUnityObjectPoolSinkWithInt
    {
        //全局缓存列表
        public List<CachemItem> listCacheItem;

        private List<uint> m_listHandle = new List<uint>();

        private int nFinishCount = 0;



        public void OnUnityObjectLoadCancel(uint handle, int ud)
        {
            ++nFinishCount;
            UnloadAll();
        }

        public void OnUnityObjectLoadComplete(UnityEngine.Object res, uint handle, int ud)
        {
            ++nFinishCount;
            UnloadAll();
        }

        // Start is called before the first frame update
        void Start()
        {
            IUnityObjectPool unityObjectPool = XGameComs.Get<IUnityObjectPool>();
            if (null == unityObjectPool)
            {
                return;
            }

            uint handle = 0;
            CachemItem item = null;
            int nCount = listCacheItem.Count;
            for(int i=0;i<nCount;++i)
            {
                item = listCacheItem[i];
                for(int j=0;j<item.count;++j)
                {
                    handle = unityObjectPool.LoadRes<GameObject>(item.resRef.path, 0, this, true);
                    m_listHandle.Add(handle);
                }

                unityObjectPool.SetReserveCount<GameObject>(item.resRef.path,item.count);
            }
        }

        private void UnloadAll()
        {
            if(nFinishCount>= m_listHandle.Count)
            {
                IUnityObjectPool unityObjectPool = XGameComs.Get<IUnityObjectPool>();
               
                for(int i=0;i < m_listHandle.Count;++i)
                {
                    unityObjectPool.UnloadRes(m_listHandle[i]);
                }

                m_listHandle = null;
                listCacheItem = null;

                UnityEngine.Object.DestroyImmediate(this);
            }

          
        }

      
    }
}

