using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    /// <summary>
    /// Dynamic material class makes it possible to create custom materials on the fly on a per-Graphic basis,
    /// and still have them get cleaned up correctly.
    /// </summary>
    public static class StencilMaterial
    {
        private class MatEntry
        {
            public Material baseMat;
            public Material customMat;
            public int count;

            public int stencilId;
            public StencilOp operation = StencilOp.Keep;
            public CompareFunction compareFunction = CompareFunction.Always;
            public int readMask;
            public int writeMask;
            public bool useAlphaClip;
            public ColorWriteMask colorMask;
        }

        private static Dictionary<Material,MatEntry> m_diMat2Enryt = new Dictionary<Material, MatEntry>();
        private static Dictionary<string, Stack<MatEntry>> m_dicMatPool = new Dictionary<string, Stack<MatEntry>>();
     
        

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use Material.Add instead.", true)]
        public static Material Add(Material baseMat, int stencilID) { return null; }

        /// <summary>
        /// Add a new material using the specified base and stencil ID.
        /// </summary>
        public static Material Add(Material baseMat, int stencilID, StencilOp operation, CompareFunction compareFunction, ColorWriteMask colorWriteMask)
        {
            return Add(baseMat, stencilID, operation, compareFunction, colorWriteMask, 255, 255);
        }

        static void LogWarningWhenNotInBatchmode(string warning, Object context)
        {
            // Do not log warnings in batchmode (case 1350059)
            if (!Application.isBatchMode)
                Debug.LogWarning(warning, context);
        }

        /// <summary>
        /// Add a new material using the specified base and stencil ID.
        /// </summary>
        public static Material Add(Material baseMat, int stencilID, StencilOp operation, CompareFunction compareFunction, ColorWriteMask colorWriteMask, int readMask, int writeMask)
        {
            if ((stencilID <= 0 && colorWriteMask == ColorWriteMask.All) || baseMat == null)
                return baseMat;

            if (!baseMat.HasProperty("_Stencil"))
            {
# if  UNITY_EDITOR
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _Stencil property", baseMat);
#endif
                return baseMat;
            }
            if (!baseMat.HasProperty("_StencilOp"))
            {
#if UNITY_EDITOR
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _StencilOp property", baseMat);
#endif
                return baseMat;
            }
            if (!baseMat.HasProperty("_StencilComp"))
            {
#if UNITY_EDITOR
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _StencilComp property", baseMat);
#endif
                return baseMat;
            }
            if (!baseMat.HasProperty("_StencilReadMask"))
            {
#if UNITY_EDITOR
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _StencilReadMask property", baseMat);
#endif
                return baseMat;
            }
            if (!baseMat.HasProperty("_StencilWriteMask"))
            {
#if UNITY_EDITOR
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _StencilWriteMask property", baseMat);
#endif
                return baseMat;
            }
            if (!baseMat.HasProperty("_ColorMask"))
            {
#if UNITY_EDITOR
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _ColorMask property", baseMat);
#endif
                return baseMat;
            }

            foreach(MatEntry ent in m_diMat2Enryt.Values)
            {
                if (ent.baseMat == baseMat
                   && ent.stencilId == stencilID
                   && ent.operation == operation
                   && ent.compareFunction == compareFunction
                   && ent.readMask == readMask
                   && ent.writeMask == writeMask
                   && ent.colorMask == colorWriteMask)
                {
                    ++ent.count;
                    return ent.customMat;
                }
            }   
            
            /*
            var listCount = m_diMat2Enryt.Count;
            for (int i = 0; i < listCount; ++i)
            {
                MatEntry ent = m_diMat2Enryt[i];

                if (ent.baseMat == baseMat
                    && ent.stencilId == stencilID
                    && ent.operation == operation
                    && ent.compareFunction == compareFunction
                    && ent.readMask == readMask
                    && ent.writeMask == writeMask
                    && ent.colorMask == colorWriteMask)
                {
                    ++ent.count;
                    return ent.customMat;
                }
            }
            */


            

            var newEnt =  AlocMatEntry(baseMat);
            newEnt.count = 1;
            newEnt.baseMat = baseMat;
            if(null== newEnt.customMat)//|| baseMat!= Graphic.defaultGraphicMaterial)
            {
                newEnt.customMat = new Material(baseMat);
            }else
            {
               
                /*
                if(newEnt.customMat.mainTexture!= baseMat.mainTexture)
                {
                    newEnt.customMat.mainTexture = baseMat.mainTexture;
                }
                */

                //暂时这样标记一下特殊材质 maskbord
                //if (baseMat.HasProperty("_EnableBorder"))
                //{
                    newEnt.customMat.CopyPropertiesFromMaterial(baseMat);
                //}

                   

                //同步额外的属性 mask border mat
                /*
                if (baseMat.HasProperty("_EnableBorder"))
                {
                    Vector4 bound = baseMat.GetVector("_Bound");
                    newEnt.customMat.SetVector("_Bound", bound);
                    Vector4 enableBorder = baseMat.GetVector("_EnableBorder");
                    newEnt.customMat.SetVector("_EnableBorder", enableBorder);
                }
                */
            }
               

            newEnt.customMat.hideFlags = HideFlags.HideAndDontSave;
            newEnt.stencilId = stencilID;
            newEnt.operation = operation;
            newEnt.compareFunction = compareFunction;
            newEnt.readMask = readMask;
            newEnt.writeMask = writeMask;
            newEnt.colorMask = colorWriteMask;
            newEnt.useAlphaClip = operation != StencilOp.Keep && writeMask > 0;

#if UNITY_EDITOR
            //newEnt.customMat.name = string.Format("Stencil Id:{0}, Op:{1}, Comp:{2}, WriteMask:{3}, ReadMask:{4}, ColorMask:{5} AlphaClip:{6} ({7})", stencilID, operation, compareFunction, writeMask, readMask, colorWriteMask, newEnt.useAlphaClip, baseMat.name);
#endif
            newEnt.customMat.SetFloat("_Stencil", (float)stencilID);
            newEnt.customMat.SetFloat("_StencilOp", (float)operation);
            newEnt.customMat.SetFloat("_StencilComp", (float)compareFunction);
            newEnt.customMat.SetFloat("_StencilReadMask", (float)readMask);
            newEnt.customMat.SetFloat("_StencilWriteMask", (float)writeMask);
            newEnt.customMat.SetFloat("_ColorMask", (float)colorWriteMask);
            newEnt.customMat.SetFloat("_UseUIAlphaClip", newEnt.useAlphaClip ? 1.0f : 0.0f);

            if (newEnt.useAlphaClip)
                newEnt.customMat.EnableKeyword("UNITY_UI_ALPHACLIP");
            else
                newEnt.customMat.DisableKeyword("UNITY_UI_ALPHACLIP");

            m_diMat2Enryt.Add(newEnt.customMat,newEnt);
            return newEnt.customMat;
        }

        /// <summary>
        /// Remove an existing material, automatically cleaning it up if it's no longer in use.
        /// </summary>
        public static void Remove(Material customMat)
        {
            //
            if (customMat == null)
                return;


            MatEntry ent = null;
            if(m_diMat2Enryt.TryGetValue(customMat,out ent))
            {
                if (--ent.count == 0)
                {
                    //Misc.DestroyImmediate(ent.customMat);
                    //ent.baseMat = null;
                    m_diMat2Enryt.Remove(customMat);

                    RecycleMatEntry(ent);
                }
            }

        
            /*
            var listCount = m_diMat2Enryt.Count;
            for (int i = listCount-1; i >=0; --i)
            {
                MatEntry ent = m_diMat2Enryt[i];

                if (ent.customMat != customMat)
                    continue;

                if (--ent.count == 0)
                {
                    //Misc.DestroyImmediate(ent.customMat);
                    //ent.baseMat = null;
                    m_diMat2Enryt.RemoveAt(i);

                    RecycleMatEntry(ent);
                }
                return;
            }
            */

        }

        public static void ClearAll()
        {
            //外部有可能还有使用的，清除的时候会导致，只能编辑器下使用
            foreach (MatEntry ent in m_diMat2Enryt.Values)
            {
                Misc.DestroyImmediate(ent.customMat);
                ent.baseMat = null;
            }

            /*
            var listCount = m_diMat2Enryt.Count;
            for (int i = 0; i < listCount; ++i)
            {
                MatEntry ent = m_diMat2Enryt[i];

                Misc.DestroyImmediate(ent.customMat);
                ent.baseMat = null;

                //RecycleMatEntry(ent);


            }
            */
            m_diMat2Enryt.Clear();
            
        }

        //分配一个对象
        private static MatEntry AlocMatEntry(Material m)
        {
#if UNITY_EDITOR
   if(false==UnityEngine.Application.isPlaying)
   {
        return new MatEntry();
   }
#endif

            Stack<MatEntry> stackMatPool = null;
            if(m_dicMatPool.TryGetValue(m.name, out stackMatPool))
            {
                if (stackMatPool.Count > 0)
                {
                    MatEntry item = stackMatPool.Pop();
                    return item;
                }
            }
            

            return new MatEntry();
        }

        //回收一个对象
        private static void RecycleMatEntry(MatEntry item)
        {

#if UNITY_EDITOR
   if(false==UnityEngine.Application.isPlaying)
   {
        if(null!=item.customMat)
        {
             Misc.DestroyImmediate(item.customMat);
            item.customMat = null;
        }
       
        return;
   }
#endif 
            if (null == item)
            {
                return;
            }

            string matName = null;
            //主材质已经被销毁了
            if (item.baseMat == null)
            {
                item.baseMat = null;
                if (item.customMat == null)
                {
                    return;
                }else
                {
                    matName = item.customMat.name;
                }
                
            }else
            {
               matName = item.baseMat.name;
            }


            Stack<MatEntry> stackMatPool = null;
            if (m_dicMatPool.TryGetValue(matName, out stackMatPool)==false)
            {
                stackMatPool = new Stack<MatEntry>();
                m_dicMatPool.Add(matName, stackMatPool);
            }


            //销毁不是默认的材质
            if (stackMatPool.Count>64)
            {
                Misc.DestroyImmediate(item.customMat);
                item.customMat = null;
                return;
            }

            if(stackMatPool.Contains(item)==false)
            {
                stackMatPool.Push(item);

                /*
                if (stackMatPool.Contains(item))
                {
                    Debug.LogError("回收成功" + item);
                }
                */
            }
            else
            {
                Debug.LogError("列表已经回收过模板缓冲材质了"+ item);
            }

           
        }

        static public void ClearPoolMaterials()
        {
            MatEntry ent = null;
            //Stack<MatEntry> stackMatPool = null;
            foreach (var stackMatPool in m_dicMatPool.Values)
            {
                while(stackMatPool.Count>0)
                {
                    ent = stackMatPool.Pop();
                    if(ent!=null)
                    {
                        ent.baseMat = null;
                        if(null!= ent.customMat)
                        {
                            Misc.DestroyImmediate(ent.customMat);
                            ent.customMat = null;
                        }
                    }

                }
            }
            //m_dicMatPool.Clear();
        }
    }
}
