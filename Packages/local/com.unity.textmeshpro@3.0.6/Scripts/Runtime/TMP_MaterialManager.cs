﻿//#define TMP_DEBUG_MODE

using UnityEngine;
using System.Collections.Generic;

using UnityEngine.UI;


namespace TMPro
{

    public static class TMP_MaterialManager
    {
        private static List<MaskingMaterial> m_materialList = new List<MaskingMaterial>();

        private static Dictionary<long, FallbackMaterial> m_fallbackMaterials = new Dictionary<long, FallbackMaterial>();
        private static Dictionary<int, long> m_fallbackMaterialLookup = new Dictionary<int, long>();
        private static List<FallbackMaterial> m_fallbackCleanupList = new List<FallbackMaterial>();


        private static Dictionary<string, Stack<FallbackMaterial>> m_dicMatPool = new Dictionary<string, Stack<FallbackMaterial>>();

        private static bool isFallbackListDirty;

        static TMP_MaterialManager()
        {
            Canvas.willRenderCanvases += OnPreRender;
        }

        static void OnPreRender()
        {
            if (isFallbackListDirty)
            {
                //Debug.Log("2 - Cleaning up Fallback Materials.");
                CleanupFallbackMaterials();
                isFallbackListDirty = false;
            }
        }

        /// <summary>
        /// Create a Masking Material Instance for the given ID
        /// </summary>
        /// <param name="baseMaterial"></param>
        /// <param name="stencilID"></param>
        /// <returns></returns>
        public static Material GetStencilMaterial(Material baseMaterial, int stencilID)
        {
            // Check if Material supports masking
            if (!baseMaterial.HasProperty(ShaderUtilities.ID_StencilID))
            {
                Debug.LogWarning("Selected Shader does not support Stencil Masking. Please select the Distance Field or Mobile Distance Field Shader.");
                return baseMaterial;
            }

            int baseMaterialID = baseMaterial.GetInstanceID();

            // If baseMaterial already has a corresponding masking material, return it.
            for (int i = 0; i < m_materialList.Count; i++)
            {
                if (m_materialList[i].baseMaterial.GetInstanceID() == baseMaterialID && m_materialList[i].stencilID == stencilID)
                {
                    m_materialList[i].count += 1;

                    #if TMP_DEBUG_MODE
                    ListMaterials();
                    #endif

                    return m_materialList[i].stencilMaterial;
                }
            }

            // No matching masking material found. Create and return a new one.

            Material stencilMaterial;

            //Create new Masking Material Instance for this Base Material
            stencilMaterial = new Material(baseMaterial);
            stencilMaterial.hideFlags = HideFlags.HideAndDontSave;

            #if UNITY_EDITOR
                stencilMaterial.name += " Masking ID:" + stencilID;
            #endif

            stencilMaterial.shaderKeywords = baseMaterial.shaderKeywords;

            // Set Stencil Properties
            ShaderUtilities.GetShaderPropertyIDs();
            stencilMaterial.SetFloat(ShaderUtilities.ID_StencilID, stencilID);
            //stencilMaterial.SetFloat(ShaderUtilities.ID_StencilOp, 0);
            stencilMaterial.SetFloat(ShaderUtilities.ID_StencilComp, 4);
            //stencilMaterial.SetFloat(ShaderUtilities.ID_StencilReadMask, stencilID);
            //stencilMaterial.SetFloat(ShaderUtilities.ID_StencilWriteMask, 0);

            MaskingMaterial temp = new MaskingMaterial();
            temp.baseMaterial = baseMaterial;
            temp.stencilMaterial = stencilMaterial;
            temp.stencilID = stencilID;
            temp.count = 1;

            m_materialList.Add(temp);

            #if TMP_DEBUG_MODE
            ListMaterials();
            #endif

            return stencilMaterial;
        }


        /// <summary>
        /// Function to release the stencil material.
        /// </summary>
        /// <param name="stencilMaterial"></param>
        public static void ReleaseStencilMaterial(Material stencilMaterial)
        {
            int stencilMaterialID = stencilMaterial.GetInstanceID();

            for (int i = 0; i < m_materialList.Count; i++)
            {
                if (m_materialList[i].stencilMaterial.GetInstanceID() == stencilMaterialID)
                {
                    if (m_materialList[i].count > 1)
                        m_materialList[i].count -= 1;
                    else
                    {
                        Object.DestroyImmediate(m_materialList[i].stencilMaterial);
                        m_materialList.RemoveAt(i);
                        stencilMaterial = null;
                    }

                    break;
                }
            }


            #if TMP_DEBUG_MODE
            ListMaterials();
            #endif
        }


        // Function which returns the base material associated with a Masking Material
        public static Material GetBaseMaterial(Material stencilMaterial)
        {
            // Check if maskingMaterial already has a base material associated with it.
            int index = m_materialList.FindIndex(item => item.stencilMaterial == stencilMaterial);

            if (index == -1)
                return null;
            else
                return m_materialList[index].baseMaterial;

        }


        /// <summary>
        /// Function to set the Material Stencil ID
        /// </summary>
        /// <param name="material"></param>
        /// <param name="stencilID"></param>
        /// <returns></returns>
        public static Material SetStencil(Material material, int stencilID)
        {
            material.SetFloat(ShaderUtilities.ID_StencilID, stencilID);

            if (stencilID == 0)
                material.SetFloat(ShaderUtilities.ID_StencilComp, 8);
            else
                material.SetFloat(ShaderUtilities.ID_StencilComp, 4);

            return material;
        }


        public static void AddMaskingMaterial(Material baseMaterial, Material stencilMaterial, int stencilID)
        {
            // Check if maskingMaterial already has a base material associated with it.
            int index = m_materialList.FindIndex(item => item.stencilMaterial == stencilMaterial);

            if (index == -1)
            {
                MaskingMaterial temp = new MaskingMaterial();
                temp.baseMaterial = baseMaterial;
                temp.stencilMaterial = stencilMaterial;
                temp.stencilID = stencilID;
                temp.count = 1;

                m_materialList.Add(temp);
            }
            else
            {
                stencilMaterial = m_materialList[index].stencilMaterial;
                m_materialList[index].count += 1;
            }
        }



        public static void RemoveStencilMaterial(Material stencilMaterial)
        {
            // Check if maskingMaterial is already on the list.
            int index = m_materialList.FindIndex(item => item.stencilMaterial == stencilMaterial);

            if (index != -1)
            {
                m_materialList.RemoveAt(index);
            }

            #if TMP_DEBUG_MODE
            ListMaterials();
            #endif
        }



        public static void ReleaseBaseMaterial(Material baseMaterial)
        {
            // Check if baseMaterial already has a masking material associated with it.
            int index = m_materialList.FindIndex(item => item.baseMaterial == baseMaterial);

            if (index == -1)
            {
                Debug.Log("No Masking Material exists for " + baseMaterial.name);
            }
            else
            {
                if (m_materialList[index].count > 1)
                {
                    m_materialList[index].count -= 1;
                    Debug.Log("Removed (1) reference to " + m_materialList[index].stencilMaterial.name + ". There are " + m_materialList[index].count + " references left.");
                }
                else
                {
                    Debug.Log("Removed last reference to " + m_materialList[index].stencilMaterial.name + " with ID " + m_materialList[index].stencilMaterial.GetInstanceID());
                    Object.DestroyImmediate(m_materialList[index].stencilMaterial);
                    m_materialList.RemoveAt(index);
                }
            }

            #if TMP_DEBUG_MODE
            ListMaterials();
            #endif
        }


        public static void ClearMaterials()
        {
            if (m_materialList.Count == 0)
            {
                Debug.Log("Material List has already been cleared.");
                return;
            }

            for (int i = 0; i < m_materialList.Count; i++)
            {
                //Material baseMaterial = m_materialList[i].baseMaterial;
                Material stencilMaterial = m_materialList[i].stencilMaterial;

                Object.DestroyImmediate(stencilMaterial);
            }
            m_materialList.Clear();
        }


        /// <summary>
        /// Function to get the Stencil ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetStencilID(GameObject obj)
        {
            // Implementation is almost copied from Unity UI

            var count = 0;

            var transform = obj.transform;
            var stopAfter = FindRootSortOverrideCanvas(transform);
            if (transform == stopAfter)
                return count;

            var t = transform.parent;
            var components = TMP_ListPool<Mask>.Get();
            while (t != null)
            {
                t.GetComponents<Mask>(components);
                for (var i = 0; i < components.Count; ++i)
                {
                    var mask = components[i];
                    if (mask != null && mask.MaskEnabled() && mask.graphic.IsActive())
            {
                        ++count;
                        break;
                    }
            }

                if (t == stopAfter)
                    break;

                t = t.parent;
            }
            TMP_ListPool<Mask>.Release(components);

            return Mathf.Min((1 << count) - 1, 255);
        }


        public static Material GetMaterialForRendering(MaskableGraphic graphic, Material baseMaterial)
        {
            if (baseMaterial == null)
                return null;

            var modifiers = TMP_ListPool<IMaterialModifier>.Get();
            graphic.GetComponents(modifiers);

            var result = baseMaterial;
            for (int i = 0; i < modifiers.Count; i++)
                result = modifiers[i].GetModifiedMaterial(result);

            TMP_ListPool<IMaterialModifier>.Release(modifiers);

            return result;
        }

        private static Transform FindRootSortOverrideCanvas(Transform start)
        {
            // Implementation is copied from Unity UI

            var canvasList = TMP_ListPool<Canvas>.Get();
            start.GetComponentsInParent(false, canvasList);
            Canvas canvas = null;

            for (int i = 0; i < canvasList.Count; ++i)
            {
                canvas = canvasList[i];

                // We found the canvas we want to use break
                if (canvas.overrideSorting)
                    break;
            }
            TMP_ListPool<Canvas>.Release(canvasList);

            return canvas != null ? canvas.transform : null;
        }


        internal static Material GetFallbackMaterial(TMP_FontAsset fontAsset, Material sourceMaterial, int atlasIndex)
        {
            int sourceMaterialID = sourceMaterial.GetInstanceID();
            Texture tex = fontAsset.atlasTextures[atlasIndex];
            int texID = tex.GetInstanceID();
            long key = (long)sourceMaterialID << 32 | (long)(uint)texID;
            FallbackMaterial fallback;

            if (m_fallbackMaterials.TryGetValue(key, out fallback))
            {
                // Check if source material properties have changed.
                int sourceMaterialCRC = sourceMaterial.ComputeCRC();
                if (sourceMaterialCRC == fallback.sourceMaterialCRC)
                    return fallback.fallbackMaterial;

                CopyMaterialPresetProperties(sourceMaterial, fallback.fallbackMaterial);
                fallback.sourceMaterialCRC = sourceMaterialCRC;
                return fallback.fallbackMaterial;
            }



            fallback = AlocFallbackMaterial(sourceMaterial, tex);//new FallbackMaterial();
            fallback.fallbackID = key;
            fallback.sourceMaterial = fontAsset.material;
            fallback.sourceMaterialCRC = sourceMaterial.ComputeCRC();
            //fallback.fallbackMaterial = fallbackMaterial;
            fallback.count = 0;

            //和缓存的材质不一样要销毁
            /*
            if (null!= fallback.fallbackMaterial&&fallback.fallbackMaterial.name != sourceMaterial.name)
            {
                //Object.DestroyImmediate(fallback.fallbackMaterial);
                fallback.fallbackMaterial = null;
            }
            */

            Material fallbackMaterial = fallback.fallbackMaterial;
            // Create new material from the source material and assign relevant atlas texture
            if (null== fallbackMaterial)
            {
                fallbackMaterial = new Material(sourceMaterial);
            }else
            {
                //拷贝材质属性
                //CopyMaterialPresetProperties(sourceMaterial, fallbackMaterial,true);
                fallbackMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            }
            
            fallbackMaterial.SetTexture(ShaderUtilities.ID_MainTex, tex);

            fallbackMaterial.hideFlags = HideFlags.HideAndDontSave;

#if UNITY_EDITOR
            fallbackMaterial.name = sourceMaterial.name+" + " + tex.name;
#endif

            fallback.fallbackMaterial = fallbackMaterial;
            m_fallbackMaterials.Add(key, fallback);
            m_fallbackMaterialLookup.Add(fallbackMaterial.GetInstanceID(), key);

            #if TMP_DEBUG_MODE
            ListFallbackMaterials();
            #endif

            return fallbackMaterial;
        }


        /// <summary>
        /// This function returns a material instance using the material properties of a previous material but using the font atlas texture of the new font asset.
        /// </summary>
        /// <param name="sourceMaterial">The material containing the source material properties to be copied to the new material.</param>
        /// <param name="targetMaterial">The font atlas texture that should be assigned to the new material.</param>
        /// <returns></returns>
        public static Material GetFallbackMaterial (Material sourceMaterial, Material targetMaterial)
        {
            int sourceID = sourceMaterial.GetInstanceID();
            Texture tex = targetMaterial.GetTexture(ShaderUtilities.ID_MainTex);
            int texID = tex.GetInstanceID();
            long key = (long)sourceID << 32 | (long)(uint)texID;
            FallbackMaterial fallback;

            if (m_fallbackMaterials.TryGetValue(key, out fallback))
            {
                // Check if source material properties have changed.
                int sourceMaterialCRC = sourceMaterial.ComputeCRC();
                if (sourceMaterialCRC == fallback.sourceMaterialCRC)
                    return fallback.fallbackMaterial;

                CopyMaterialPresetProperties(sourceMaterial, fallback.fallbackMaterial);
                fallback.sourceMaterialCRC = sourceMaterialCRC;
                return fallback.fallbackMaterial;
            }





            // Create new material from the source material and copy properties if using distance field shaders.
            Material fallbackMaterial = null;// 
            if (sourceMaterial.HasProperty(ShaderUtilities.ID_GradientScale) && targetMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                fallback = AlocFallbackMaterial(sourceMaterial, tex);  //new FallbackMaterial();
                fallbackMaterial = fallback.fallbackMaterial;
                if (null== fallbackMaterial)
                {
                    fallbackMaterial = new Material(sourceMaterial);
                }else
                {
                    //CopyMaterialPresetProperties(sourceMaterial, fallbackMaterial,true);
                    fallbackMaterial.CopyPropertiesFromMaterial(sourceMaterial);
                }

                fallback.sourceMaterial = sourceMaterial;
                fallbackMaterial.hideFlags = HideFlags.HideAndDontSave;

                #if UNITY_EDITOR
                fallbackMaterial.name = sourceMaterial.name+" + " + tex.name;
                //Debug.Log("Creating new fallback material for " + fallbackMaterial.name);
                #endif

                fallbackMaterial.SetTexture(ShaderUtilities.ID_MainTex, tex);
                // Retain material properties unique to target material.
                fallbackMaterial.SetFloat(ShaderUtilities.ID_GradientScale, targetMaterial.GetFloat(ShaderUtilities.ID_GradientScale));
                fallbackMaterial.SetFloat(ShaderUtilities.ID_TextureWidth, targetMaterial.GetFloat(ShaderUtilities.ID_TextureWidth));
                fallbackMaterial.SetFloat(ShaderUtilities.ID_TextureHeight, targetMaterial.GetFloat(ShaderUtilities.ID_TextureHeight));
                fallbackMaterial.SetFloat(ShaderUtilities.ID_WeightNormal, targetMaterial.GetFloat(ShaderUtilities.ID_WeightNormal));
                fallbackMaterial.SetFloat(ShaderUtilities.ID_WeightBold, targetMaterial.GetFloat(ShaderUtilities.ID_WeightBold));
            }
            else
            {

                fallback = AlocFallbackMaterial(targetMaterial, tex);  //new FallbackMaterial();
                fallbackMaterial = fallback.fallbackMaterial;
                if (null == fallbackMaterial)
                {
                    fallbackMaterial = new Material(targetMaterial);
                }
                else
                {
                    // CopyMaterialPresetProperties(targetMaterial, fallbackMaterial,true);
                    fallbackMaterial.CopyPropertiesFromMaterial(targetMaterial);
                }
                fallback.sourceMaterial = targetMaterial;
            }


            fallback.fallbackID = key;
            
            fallback.sourceMaterialCRC = sourceMaterial.ComputeCRC();
            fallback.count = 0;
            fallback.fallbackMaterial = fallbackMaterial;

            m_fallbackMaterials.Add(key, fallback);
            m_fallbackMaterialLookup.Add(fallbackMaterial.GetInstanceID(), key);

            #if TMP_DEBUG_MODE
            ListFallbackMaterials();
            #endif

            return fallbackMaterial;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="targetMaterial"></param>
        public static void AddFallbackMaterialReference(Material targetMaterial)
        {
            if (targetMaterial == null) return;

            int sourceID = targetMaterial.GetInstanceID();
            long key;

            // Lookup key to retrieve
            if (m_fallbackMaterialLookup.TryGetValue(sourceID, out key))
            {
                FallbackMaterial fallback;
                if (m_fallbackMaterials.TryGetValue(key, out fallback))
                {
                    //Debug.Log("Adding Fallback material " + fallback.fallbackMaterial.name + " with reference count of " + (fallback.count + 1));
                    fallback.count += 1;
                }
            }
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="targetMaterial"></param>
        public static void RemoveFallbackMaterialReference(Material targetMaterial)
        {
            if (targetMaterial == null) return;

            int sourceID = targetMaterial.GetInstanceID();
            long key;

            // Lookup key to retrieve
            if (m_fallbackMaterialLookup.TryGetValue(sourceID, out key))
            {
                FallbackMaterial fallback;
                if (m_fallbackMaterials.TryGetValue(key, out fallback))
                {
                    fallback.count -= 1;

                    if (fallback.count < 1)
                        m_fallbackCleanupList.Add(fallback);
                }
            }
        }


        /// <summary>
        ///
        /// </summary>
        public static void CleanupFallbackMaterials()
        {
            // Return if the list is empty.
            if (m_fallbackCleanupList.Count == 0) return;

            for (int i = 0; i < m_fallbackCleanupList.Count; i++)
            {
                FallbackMaterial fallback = m_fallbackCleanupList[i];

                if (fallback.count < 1)
                {
                    //Debug.Log("Cleaning up " + fallback.fallbackMaterial.name);

                    Material mat = fallback.fallbackMaterial;
                    m_fallbackMaterials.Remove(fallback.fallbackID);
                    m_fallbackMaterialLookup.Remove(mat.GetInstanceID());

                    RecycleFallbackMaterial(fallback);
                    //ItemPool<FallbackMaterial>.RecycleItem(fallback);
                    //Object.DestroyImmediate(mat);
                    //mat = null;
                }
            }

            m_fallbackCleanupList.Clear();
        }


        /// <summary>
        /// Function to release the fallback material.
        /// </summary>
        /// <param name="fallbackMaterial">Material to be released.</param>
        public static void ReleaseFallbackMaterial(Material fallbackMaterial)
        {
            if (fallbackMaterial == null) return;

            int materialID = fallbackMaterial.GetInstanceID();
            long key;

            if (m_fallbackMaterialLookup.TryGetValue(materialID, out key))
            {
                FallbackMaterial fallback;
                if (m_fallbackMaterials.TryGetValue(key, out fallback))
                {
                    //Debug.Log("Releasing Fallback material " + fallback.fallbackMaterial.name + " with remaining reference count of " + (fallback.count - 1));

                    fallback.count -= 1;

                    if (fallback.count < 1)
                        m_fallbackCleanupList.Add(fallback);
                }
            }

            isFallbackListDirty = true;

            #if TMP_DEBUG_MODE
            ListFallbackMaterials();
            #endif
        }


        private class FallbackMaterial
        {
            public long fallbackID;
            public Material sourceMaterial;
            internal int sourceMaterialCRC;
            public Material fallbackMaterial;
            public int count;
        }


        private class MaskingMaterial
        {
            public Material baseMaterial;
            public Material stencilMaterial;
            public int count;
            public int stencilID;
        }


        /// <summary>
        /// Function to copy the properties of a source material preset to another while preserving the unique font asset properties of the destination material.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyMaterialPresetProperties(Material source, Material destination,bool force = false)
        {
            if (!force&&(!source.HasProperty(ShaderUtilities.ID_GradientScale) || !destination.HasProperty(ShaderUtilities.ID_GradientScale)))
                return;

            // Save unique material properties
            Texture dst_texture = destination.GetTexture(ShaderUtilities.ID_MainTex);
            float dst_gradientScale = destination.GetFloat(ShaderUtilities.ID_GradientScale);
            float dst_texWidth = destination.GetFloat(ShaderUtilities.ID_TextureWidth);
            float dst_texHeight = destination.GetFloat(ShaderUtilities.ID_TextureHeight);
            float dst_weightNormal = destination.GetFloat(ShaderUtilities.ID_WeightNormal);
            float dst_weightBold = destination.GetFloat(ShaderUtilities.ID_WeightBold);

            // Copy all material properties
            destination.CopyPropertiesFromMaterial(source);

            // Copy shader keywords
            destination.shaderKeywords = source.shaderKeywords;

            // Restore unique material properties
            destination.SetTexture(ShaderUtilities.ID_MainTex, dst_texture);
            destination.SetFloat(ShaderUtilities.ID_GradientScale, dst_gradientScale);
            destination.SetFloat(ShaderUtilities.ID_TextureWidth, dst_texWidth);
            destination.SetFloat(ShaderUtilities.ID_TextureHeight, dst_texHeight);
            destination.SetFloat(ShaderUtilities.ID_WeightNormal, dst_weightNormal);
            destination.SetFloat(ShaderUtilities.ID_WeightBold, dst_weightBold);
        }

        //分配一个对象
        private static FallbackMaterial AlocFallbackMaterial(Material m, Texture mainTexture)
        {
            Stack<FallbackMaterial> stackMatPool = null;
            if (m_dicMatPool.TryGetValue(m.name, out stackMatPool))
            {
                if (stackMatPool.Count > 0)
                {
                    FallbackMaterial item = stackMatPool.Pop();

                    //纹理不一样了,可能切换了语言
                    if(item.fallbackMaterial!=null&& item.fallbackMaterial.mainTexture!=mainTexture)
                    {

                        Object.DestroyImmediate(item.fallbackMaterial);
                        item.fallbackMaterial = null;
                    }

                    return item;
                }
            }


            return new FallbackMaterial();
        }

        //回收一个对象
        private static void RecycleFallbackMaterial(FallbackMaterial item)
        {
            if (null == item)
            {
                return;
            }

            //已经销毁了的，不放入缓存
            //item.sourceMaterial = null;
            if (null== item.sourceMaterial)
            {
                if(item.fallbackMaterial!=null)
                {
                    Object.DestroyImmediate(item.fallbackMaterial);
                }
                return;
            }

            string matName = item.sourceMaterial.name;

            Stack<FallbackMaterial> stackMatPool = null;
            if (m_dicMatPool.TryGetValue(matName, out stackMatPool) == false)
            {
                stackMatPool = new Stack<FallbackMaterial>();
                m_dicMatPool.Add(matName, stackMatPool);
            }


            //销毁不是默认的材质
            if (stackMatPool.Count > 64)
            {
                Object.DestroyImmediate(item.fallbackMaterial);
                item.fallbackMaterial = null;
                return;
            }

            stackMatPool.Push(item);
        }

        static public void ClearPoolMaterials()
        {
            FallbackMaterial ent = null;
            //Stack<MatEntry> stackMatPool = null;
            foreach (var stackMatPool in m_dicMatPool.Values)
            {
                while (stackMatPool.Count > 0)
                {
                    ent = stackMatPool.Pop();
                    if (ent != null)
                    {
                        ent.sourceMaterial = null;
                        if (null != ent.fallbackMaterial)
                        {
                            Object.DestroyImmediate(ent.fallbackMaterial);
                            ent.fallbackMaterial = null;
                        }
                    }

                }
            }
           // m_dicMatPool.Clear();
        }


#if TMP_DEBUG_MODE
        /// <summary>
        ///
        /// </summary>
        public static void ListMaterials()
        {

            if (m_materialList.Count == 0)
            {
                Debug.Log("Material List is empty.");
                return;
            }

            //Debug.Log("List contains " + m_materialList.Count() + " items.");

            for (int i = 0; i < m_materialList.Count; i++)
            {
                Material baseMaterial = m_materialList[i].baseMaterial;
                Material stencilMaterial = m_materialList[i].stencilMaterial;

                Debug.Log("Item #" + (i + 1) + " - Base Material is [" + baseMaterial.name + "] with ID " + baseMaterial.GetInstanceID() + " is associated with [" + (stencilMaterial != null ? stencilMaterial.name : "Null") + "] Stencil ID " + m_materialList[i].stencilID + " with ID " + (stencilMaterial != null ? stencilMaterial.GetInstanceID() : 0) + " and is referenced " + m_materialList[i].count + " time(s).");
            }
        }


        /// <summary>
        ///
        /// </summary>
        public static void ListFallbackMaterials()
        {

            if (m_fallbackMaterials.Count == 0)
            {
                Debug.Log("Material List is empty.");
                return;
            }

            Debug.Log("List contains " + m_fallbackMaterials.Count + " items.");

            int count = 0;
            foreach (var fallback in m_fallbackMaterials)
            {
                Material baseMaterial = fallback.Value.baseMaterial;
                Material fallbackMaterial = fallback.Value.fallbackMaterial;

                string output = "Item #" + (count++);
                if (baseMaterial != null)
                    output += " - Base Material is [" + baseMaterial.name + "] with ID " + baseMaterial.GetInstanceID();
                if (fallbackMaterial != null)
                    output += " is associated with [" + fallbackMaterial.name + "] with ID " + fallbackMaterial.GetInstanceID() + " and is referenced " + fallback.Value.count + " time(s).";

                Debug.Log(output);
            }
        }
#endif
    }

}

