using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting.APIUpdating;

#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
using UnityEngine.Rendering.PostProcessing;
#endif

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Contains properties and helper functions that you can use when rendering.
    /// </summary>
    [MovedFrom("UnityEngine.Rendering.LWRP")] public static class RenderingUtils
    {
        static List<ShaderTagId> m_LegacyShaderPassNames = new List<ShaderTagId>()
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        static Mesh s_FullscreenMesh = null;

        /// <summary>
        /// Returns a mesh that you can use with <see cref="CommandBuffer.DrawMesh(Mesh, Matrix4x4, Material)"/> to render full-screen effects.
        /// </summary>
        public static Mesh fullscreenMesh
        {
            get
            {
                if (s_FullscreenMesh != null)
                    return s_FullscreenMesh;

                float topV = 1.0f;
                float bottomV = 0.0f;

                s_FullscreenMesh = new Mesh { name = "Fullscreen Quad" };
                s_FullscreenMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f)
                });

                s_FullscreenMesh.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, bottomV),
                    new Vector2(0.0f, topV),
                    new Vector2(1.0f, bottomV),
                    new Vector2(1.0f, topV)
                });

                s_FullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
                s_FullscreenMesh.UploadMeshData(true);
                return s_FullscreenMesh;
            }
        }

#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
        static readonly int m_PostProcessingTemporaryTargetId = Shader.PropertyToID("_TemporaryColorTexture");
        static PostProcessRenderContext m_PostProcessRenderContext;

        [Obsolete("The use of the Post-processing Stack V2 is deprecated in the Universal Render Pipeline. Use the builtin post-processing effects instead.")]
        public static PostProcessRenderContext postProcessRenderContext
        {
            get => m_PostProcessRenderContext ?? (m_PostProcessRenderContext = new PostProcessRenderContext());
        }
#endif

        internal static bool useStructuredBuffer
        {
            // There are some performance issues with StructuredBuffers in some platforms.
            // We fallback to UBO in those cases.
            get
            {
                // TODO: For now disabling SSBO until figure out Vulkan binding issues.
                // When enabling this also enable it in shader side in Input.hlsl
                return false;

                // We don't use SSBO in D3D because we can't figure out without adding shader variants if platforms is D3D10.
                //GraphicsDeviceType deviceType = SystemInfo.graphicsDeviceType;
                //return !Application.isMobilePlatform &&
                //    (deviceType == GraphicsDeviceType.Metal || deviceType == GraphicsDeviceType.Vulkan ||
                //     deviceType == GraphicsDeviceType.PlayStation4 || deviceType == GraphicsDeviceType.XboxOne);
            }
        }

        static Material s_ErrorMaterial;
        static Material errorMaterial
        {
            get
            {
                if (s_ErrorMaterial == null)
                {
                    // TODO: When importing project, AssetPreviewUpdater::CreatePreviewForAsset will be called multiple times.
                    // This might be in a point that some resources required for the pipeline are not finished importing yet.
                    // Proper fix is to add a fence on asset import.
                    try
                    {
                        s_ErrorMaterial = new Material(Shader.Find("Hidden/Universal Render Pipeline/FallbackError"));
                    }
                    catch{ }
                }

                return s_ErrorMaterial;
            }
        }
		
		/// <summary>
        /// Set view and projection matrices.
        /// This function will set <c>UNITY_MATRIX_V</c>, <c>UNITY_MATRIX_P</c>, <c>UNITY_MATRIX_VP</c> to given view and projection matrices.
        /// If <c>setInverseMatrices</c> is set to true this function will also set <c>UNITY_MATRIX_I_V</c> and <c>UNITY_MATRIX_I_VP</c>.
        /// </summary>
        /// <param name="cmd">CommandBuffer to submit data to GPU.</param>
        /// <param name="viewMatrix">View matrix to be set.</param>
        /// <param name="projectionMatrix">Projection matrix to be set.</param>
        /// <param name="setInverseMatrices">Set this to true if you also need to set inverse camera matrices.</param>
        public static void SetViewAndProjectionMatrices(CommandBuffer cmd, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, bool setInverseMatrices)
        {
            Matrix4x4 viewAndProjectionMatrix = projectionMatrix * viewMatrix;
            cmd.SetGlobalMatrix(ShaderPropertyId.viewMatrix, viewMatrix);
            cmd.SetGlobalMatrix(ShaderPropertyId.projectionMatrix, projectionMatrix);
            cmd.SetGlobalMatrix(ShaderPropertyId.viewAndProjectionMatrix, viewAndProjectionMatrix);

            if (setInverseMatrices)
            {
                Matrix4x4 inverseMatrix = Matrix4x4.Inverse(viewMatrix);
                // Note: inverse projection is currently undefined
                Matrix4x4 inverseViewProjection = Matrix4x4.Inverse(viewAndProjectionMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, inverseMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, inverseViewProjection);
            }
        }

#if POST_PROCESSING_STACK_2_0_0_OR_NEWER
#pragma warning disable 0618 // Obsolete
        internal static void RenderPostProcessingCompat(CommandBuffer cmd, ref CameraData cameraData, RenderTextureDescriptor sourceDescriptor,
                                                        RenderTargetIdentifier source, RenderTargetIdentifier destination, bool opaqueOnly, bool flip)
        {
            var layer = cameraData.postProcessLayer;
            int effectsCount;

            if (opaqueOnly)
            {
                effectsCount = layer.sortedBundles[PostProcessEvent.BeforeTransparent].Count;
            }
            else
            {
                effectsCount = layer.sortedBundles[PostProcessEvent.BeforeStack].Count +
                               layer.sortedBundles[PostProcessEvent.AfterStack].Count;
            }

            var camera = cameraData.camera;
            var postProcessRenderContext = RenderingUtils.postProcessRenderContext;
            postProcessRenderContext.Reset();
            postProcessRenderContext.camera = camera;
            postProcessRenderContext.source = source;
            postProcessRenderContext.sourceFormat = sourceDescriptor.colorFormat;
            postProcessRenderContext.destination = destination;
            postProcessRenderContext.command = cmd;
            postProcessRenderContext.flip = flip;

            // If there's only one effect in the stack and soure is same as dest we	
            // create an intermediate blit rendertarget to handle it.	
            // Otherwise, PostProcessing system will create the intermediate blit targets itself.	
            if (effectsCount == 1 && source == destination)
            {
                var rtId = new RenderTargetIdentifier(m_PostProcessingTemporaryTargetId);
                var descriptor = sourceDescriptor;
                descriptor.msaaSamples = 1;
                descriptor.depthBufferBits = 0;

                postProcessRenderContext.destination = rtId;
                cmd.GetTemporaryRT(m_PostProcessingTemporaryTargetId, descriptor, FilterMode.Point);

                if (opaqueOnly)
                    cameraData.postProcessLayer.RenderOpaqueOnly(postProcessRenderContext);
                else
                    cameraData.postProcessLayer.Render(postProcessRenderContext);

                cmd.Blit(rtId, destination);
                cmd.ReleaseTemporaryRT(m_PostProcessingTemporaryTargetId);
            }
            else if (opaqueOnly)
            {
                cameraData.postProcessLayer.RenderOpaqueOnly(postProcessRenderContext);
            }
            else
            {
                cameraData.postProcessLayer.Render(postProcessRenderContext);
            }
        }
#pragma warning restore 0618
#endif

        // This is used to render materials that contain built-in shader passes not compatible with URP. 
        // It will render those legacy passes with error/pink shader.
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        internal static void RenderObjectsWithError(ScriptableRenderContext context, ref CullingResults cullResults, Camera camera, FilteringSettings filterSettings, SortingCriteria sortFlags)
        {

            // TODO: When importing project, AssetPreviewUpdater::CreatePreviewForAsset will be called multiple times.
            // This might be in a point that some resources required for the pipeline are not finished importing yet.
            // Proper fix is to add a fence on asset import.
            if (errorMaterial == null)
                return;

            SortingSettings sortingSettings = new SortingSettings(camera) { criteria = sortFlags };

           
            if(false== s_InitErrorSetting)
            {
                s_InitErrorSetting = true;

                s_errorSettings = new DrawingSettings(m_LegacyShaderPassNames[0], sortingSettings)
                {
                    perObjectData = PerObjectData.None,
                    overrideMaterial = errorMaterial,
                    overrideMaterialPassIndex = 0
                };
                for (int i = 1; i < m_LegacyShaderPassNames.Count; ++i)
                    s_errorSettings.SetShaderPassName(i, m_LegacyShaderPassNames[i]);

            }

            s_errorSettings.sortingSettings = sortingSettings;




            context.DrawRenderers(cullResults, ref s_errorSettings, ref filterSettings);
        }


        static bool s_InitErrorSetting = false;
        static DrawingSettings s_errorSettings;


        // Caches render texture format support. SystemInfo.SupportsRenderTextureFormat and IsFormatSupported allocate memory due to boxing.
        static Dictionary<RenderTextureFormat, bool> m_RenderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();
        static Dictionary<GraphicsFormat, bool> m_GraphicsFormatSupport = new Dictionary<GraphicsFormat, bool>();

        internal static void ClearSystemInfoCache()
        {
            m_RenderTextureFormatSupport.Clear();
            m_GraphicsFormatSupport.Clear();
        }

        /// <summary>
        /// Checks if a render texture format is supported by the run-time system.
        /// Similar to <see cref="SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat)"/>, but doesn't allocate memory.
        /// </summary>
        /// <param name="format">The format to look up.</param>
        /// <returns>Returns true if the graphics card supports the given <c>RenderTextureFormat</c></returns>
        public static bool SupportsRenderTextureFormat(RenderTextureFormat format)
        {
            if (!m_RenderTextureFormatSupport.TryGetValue(format, out var support))
            {
                support = SystemInfo.SupportsRenderTextureFormat(format);
                m_RenderTextureFormatSupport.Add(format, support);
            }

            return support;
        }

        /// <summary>
        /// Checks if a texture format is supported by the run-time system.
        /// Similar to <see cref="SystemInfo.IsFormatSupported"/>, but doesn't allocate memory.
        /// </summary>
        /// <param name="format">The format to look up.</param>
        /// <param name="usage">The format usage to look up.</param>
        /// <returns>Returns true if the graphics card supports the given <c>GraphicsFormat</c></returns>
        public static bool SupportsGraphicsFormat(GraphicsFormat format, FormatUsage usage)
        {
            if (!m_GraphicsFormatSupport.TryGetValue(format, out var support))
            {
                support = SystemInfo.IsFormatSupported(format, usage);
                m_GraphicsFormatSupport.Add(format, support);
            }

            return support;
        }

        /// <summary>
        /// Return the last colorBuffer index actually referring to an existing RenderTarget
        /// </summary>
        /// <param name="colorBuffers"></param>
        /// <returns></returns>
        internal static int GetLastValidColorBufferIndex(RenderTargetIdentifier[] colorBuffers)
        {
            int i = colorBuffers.Length - 1;
            for(; i>=0; --i)
            {
                if (colorBuffers[i] != 0)
                    break;
            }
            return i;
        }

        /// <summary>
        /// Return the number of items in colorBuffers actually referring to an existing RenderTarget
        /// </summary>
        /// <param name="colorBuffers"></param>
        /// <returns></returns>
        internal static uint GetValidColorBufferCount(RenderTargetIdentifier[] colorBuffers)
        {
            uint nonNullColorBuffers = 0;
            if (colorBuffers != null)
            {
                foreach (var identifier in colorBuffers)
                {
                    if (identifier != 0)
                        ++nonNullColorBuffers;
                }
            }
            return nonNullColorBuffers;
        }

        /// <summary>
        /// Return true if colorBuffers is an actual MRT setup
        /// </summary>
        /// <param name="colorBuffers"></param>
        /// <returns></returns>
        internal static bool IsMRT(RenderTargetIdentifier[] colorBuffers)
        {
            return GetValidColorBufferCount(colorBuffers) > 1;
        }

        /// <summary>
        /// Return true if value can be found in source (without recurring to Linq)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool Contains(RenderTargetIdentifier[] source, RenderTargetIdentifier value)
        {
            foreach (var identifier in source)
            {
                if (identifier == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return the index where value was found source. Otherwise, return -1. (without recurring to Linq)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int IndexOf(RenderTargetIdentifier[] source, RenderTargetIdentifier value)
        {
            for (int i = 0; i < source.Length; ++i)
            {
                if (source[i] == value)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Return the number of RenderTargetIdentifiers in "source" that are valid (not 0) and different from "value" (without recurring to Linq)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static uint CountDistinct(RenderTargetIdentifier[] source, RenderTargetIdentifier value)
        {
            uint count = 0;
            for (int i = 0; i < source.Length; ++i)
            {
                if (source[i] != value && source[i] != 0)
                    ++count;
            }
            return count;
        }

        /// <summary>
        /// Return the index of last valid (i.e different from 0) RenderTargetIdentifiers in "source" (without recurring to Linq)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static int LastValid(RenderTargetIdentifier[] source)
        {
            for (int i = source.Length-1; i >= 0; --i)
            {
                if (source[i] != 0)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Return true if ClearFlag a contains ClearFlag b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static bool Contains(ClearFlag a, ClearFlag b)
        {
            return (a & b) == b;
        }

        /// <summary>
        /// Return true if "left" and "right" are the same (without recurring to Linq)
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        internal static bool SequenceEqual(RenderTargetIdentifier[] left, RenderTargetIdentifier[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; ++i)
                if (left[i] != right[i])
                    return false;

            return true;
        }
    }
}
