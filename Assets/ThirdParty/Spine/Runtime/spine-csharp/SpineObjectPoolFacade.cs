using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XGame;
using XGame.Poolable;

namespace Spine
{


    public class SpineObjectPoolFacade
    {
        private static SpineObjectPoolFacade _Instance;

        static private IItemPoolManager itemPoolMgr = null;

        static private Dictionary<int, List<float[]>> m_dicFloatList = new Dictionary<int, List<float[]>>();
        static private Dictionary<int, List<int[]>> m_dicIntList = new Dictionary<int, List<int[]>>();
        static private Dictionary<int, List<int[,]>> m_dicAryIntList = new Dictionary<int, List<int[,]>>();


        private float m_lastClearTime = 0;
        Stack<Mesh> s_meshPool = new Stack<Mesh>();


        static private int DEFAULT_MAX_SIZE = 1024;

#if UNITY_EDITOR
        int m_meshCount = 0;
#endif 

        static public SpineObjectPoolFacade Instance()
        {
            if (null == _Instance)
            {
                _Instance = new SpineObjectPoolFacade();
               
            }

            if (null == itemPoolMgr)
            {
                itemPoolMgr = XGameComs.Get<IItemPoolManager>();

            }

            return _Instance;
        }

        public void Clear()
        {
            float curTime = Time.realtimeSinceStartup;
            if(curTime-m_lastClearTime<90)
            {
                return ;
            }

            m_lastClearTime = curTime;
            m_dicFloatList.Clear();
            m_dicIntList.Clear();
            m_dicAryIntList.Clear();

        }

        //分配对象
        public T Pop<T>(object context = null) where T : class, IPoolable, new()
        {
            if(null!= itemPoolMgr)
            {



                //先走简单路线，免得每次注册
                T t =  itemPoolMgr.PopObjectItem<T>();
                //t.Init(context);

#if UNITY_EDITOR
                itemPoolMgr.SetObjectItemMaxCacheCount<T>(30000);
#endif

                return t;
            }

            return new T();
        }

        //回收对象
        public void Push(IPoolable obj)
        {
            if (null!= obj&&null != itemPoolMgr)
            {
                obj.Reset();
                 itemPoolMgr.PushObjectItem(obj);
            }
        }

        //分配一个float性质数组
       public  float[] AlocFloatArray(int nCount)
        {
            List<float[]> stackFloatArry = null;
            if(m_dicFloatList.TryGetValue(nCount,out stackFloatArry))
            {
                if(stackFloatArry.Count>0)
                {
                    int nInndex = stackFloatArry.Count - 1;
                    float[] v = stackFloatArry[nInndex];
                    stackFloatArry.RemoveAt(nInndex);                  
                    return v;
                }
            }

            return new float[nCount];
        }

        public float[] AlocZeroFloatArray(int nCount)
        {
            float[] ary = AlocFloatArray(nCount);
            for(int i=0;i<ary.Length;++i)
            {
                ary[i] = 0;
            }
            return ary;
        }

            //回收一个float 数组
        public void RecycleFloatArray(float[] arry)
        {
            if(null== arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<float[]> stackFloatArry = null;
            if (m_dicFloatList.TryGetValue(nCount, out stackFloatArry)==false)
            {
                stackFloatArry = new List<float[]>();
                m_dicFloatList.Add(nCount, stackFloatArry);
            }

#if UNITY_EDITOR
            if (stackFloatArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            if (stackFloatArry.Count < DEFAULT_MAX_SIZE)
                stackFloatArry.Add(arry);

        }

        //分配一个int性质数组
        public int[] AlocIntArray(int nCount)
        {
            List<int[]> stackIntArry = null;
            if (m_dicIntList.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    int[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new int[nCount];
        }

        //回收一个int 数组
        public void RecycleIntArray(int[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<int[]> stackIntArry = null;
            if (m_dicIntList.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<int[]>();
                m_dicIntList.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
          if(stackIntArry.IndexOf(arry)>=0)
            {
                Debug.LogError("重复回收数组对象：len="+ arry.Length);
            }
#endif
            if(stackIntArry.Count< DEFAULT_MAX_SIZE)
            {
                stackIntArry.Add(arry);
            }
           

        }


        //分配一个int性质数组
        public int[,] AlocIntArray(int row,int col)
        {
            List<int[,]> stackIntArry = null;
            int key = (row << 16) | col;
            if (m_dicAryIntList.TryGetValue(key, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    int[,] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new int[row,col];
        }

        //回收一个int 数组
        public void RecycleIntArray(int[,] arry)
        {
            if (null == arry)
            {
                return;
            }

            int row = arry.GetLength(0);
            int col = arry.GetLength(1);

            int key = (row << 16) | col;
            List<int[,]> stackIntArry = null;
            if (m_dicAryIntList.TryGetValue(key, out stackIntArry) == false)
            {
                stackIntArry = new List<int[,]>();
                m_dicAryIntList.Add(key, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif
            if (stackIntArry.Count < DEFAULT_MAX_SIZE)
            {
                stackIntArry.Add(arry);
            }


        }

        public  Mesh NewSkeletonMesh()
        {
#if UNITY_EDITOR
            ++m_meshCount;
#endif

            if (s_meshPool.Count > 0)
            {
                return s_meshPool.Pop();

            }

            var m = new Mesh();
            m.MarkDynamic();
            m.name = "Skeleton Mesh";
            m.hideFlags = SpineMesh.MeshHideflags;
            return m;
        }

        public  void RecycleMesh(Mesh m)
        {
#if UNITY_EDITOR
            --m_meshCount;
#endif
            m.Clear();
            if(s_meshPool.Count>128)
            {
                UnityEngine.Object.DestroyImmediate(m,true);
                return;
            }
            s_meshPool.Push(m);
        }

        public  void LimtMeshCount(int nCount)
        {
#if UNITY_EDITOR
            Debug.LogWarning("已经分配个数:m_meshCount "+ m_meshCount+",当前缓存个数 count = "+ s_meshPool.Count);
#endif

            while (s_meshPool.Count > nCount)
            {
                Mesh m = s_meshPool.Pop();
                UnityEngine.Object.DestroyImmediate(m);

            }
        }

        Stack<DoubleBuffered<MeshRendererBuffers.SmartMesh>> s_MRB = new Stack<DoubleBuffered<MeshRendererBuffers.SmartMesh>>();
        public DoubleBuffered<MeshRendererBuffers.SmartMesh> AlocMeshRendererBuffers()
        {
            if(s_MRB.Count>0)
            {
                DoubleBuffered<MeshRendererBuffers.SmartMesh> mrb = s_MRB.Pop();


                if(mrb!=null)
                {
                    MeshRendererBuffers.SmartMesh mb = mrb.GetNext();
                    int validCount = 0;
                    if (mb != null)
                    {
                        ++validCount;
                        mb.Clear();
                    }

                    mb = mrb.GetNext();
                    if (mb != null)
                    {
                        ++validCount;
                        mb.Clear();
                    }
                    

                    if (validCount > 1)
                    {
                        return mrb;
                    }else
                    {
                        Debug.LogError("validCount ==" + validCount);
                        return AlocMeshRendererBuffers();
                        //
                    }
                }

               
                  
            }

            return new DoubleBuffered<MeshRendererBuffers.SmartMesh>();
        }

        public void RecycleMeshRendererBuffers(DoubleBuffered<MeshRendererBuffers.SmartMesh> mrb)
        {

            if(null== mrb)
            {
                return;
            }

            if (s_MRB.Count > 128)
            {
                MeshRendererBuffers.SmartMesh sm = mrb.GetNext();
                if(sm!=null)
                {
                    sm.Dispose();
                }

                sm = mrb.GetNext();
                if (sm != null)
                {
                    sm.Dispose();
                }

                return ;
            }

            s_MRB.Push(mrb);

        }


    }
}
