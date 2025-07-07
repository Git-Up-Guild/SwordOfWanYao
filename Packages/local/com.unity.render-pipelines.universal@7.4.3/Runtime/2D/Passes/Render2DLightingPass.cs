using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experimental.Rendering.Universal
{
    internal class Render2DLightingPass : ScriptableRenderPass
    {
        static SortingLayer[] s_SortingLayers;
        Renderer2DData m_Renderer2DData;
        static readonly ShaderTagId k_CombinedRenderingPassNameOld = new ShaderTagId("Lightweight2D");
        static readonly ShaderTagId k_CombinedRenderingPassName = new ShaderTagId("Universal2D");
        static readonly ShaderTagId k_NormalsRenderingPassName = new ShaderTagId("NormalsRendering");
        static readonly ShaderTagId k_LegacyPassName = new ShaderTagId("SRPDefaultUnlit");
        static readonly List<ShaderTagId> k_ShaderTags = new List<ShaderTagId>() { k_LegacyPassName, k_CombinedRenderingPassName, k_CombinedRenderingPassNameOld };

        public Render2DLightingPass(Renderer2DData rendererData)
        {
            if (s_SortingLayers == null)
                s_SortingLayers = SortingLayer.layers;

            m_Renderer2DData = rendererData;
        }

        public void GetTransparencySortingMode(Camera camera, ref SortingSettings sortingSettings)
        {
            TransparencySortMode mode = camera.transparencySortMode;

            if (mode == TransparencySortMode.Default)
            {
                mode = m_Renderer2DData.transparencySortMode;
                if (mode == TransparencySortMode.Default)
                    mode = camera.orthographic ? TransparencySortMode.Orthographic : TransparencySortMode.Perspective;
            }

            if (mode == TransparencySortMode.Perspective)
            {
                sortingSettings.distanceMetric = DistanceMetric.Perspective;
            }
            else if (mode == TransparencySortMode.Orthographic)
            {
                sortingSettings.distanceMetric = DistanceMetric.Orthographic;
            }
            else
            {
                sortingSettings.distanceMetric = DistanceMetric.CustomAxis;
                sortingSettings.customAxis = m_Renderer2DData.transparencySortAxis;
            }
        }


        //临时局部变量
        FilteringSettings m_filterSettings = new FilteringSettings();
        const int blendStylesCount = 4;
        bool[] m_hasBeenInitialized = new bool[blendStylesCount];
        SortingLayerRange m_sortLayerRange = new SortingLayerRange();



        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            bool isLitView = true;

#if UNITY_EDITOR
            if (renderingData.cameraData.isSceneViewCamera&& UnityEditor.SceneView.currentDrawingSceneView)
                isLitView = UnityEditor.SceneView.currentDrawingSceneView.sceneLighting;

            if (renderingData.cameraData.camera.cameraType == CameraType.Preview)
                isLitView = false;

            if (!Application.isPlaying)
                s_SortingLayers = SortingLayer.layers;
#endif
            Camera camera = renderingData.cameraData.camera;


            m_filterSettings.renderQueueRange = RenderQueueRange.all;
            m_filterSettings.layerMask = -1;
            m_filterSettings.renderingLayerMask = 0xFFFFFFFF;
            m_filterSettings.sortingLayerRange = SortingLayerRange.all;


            bool isSceneLit = Light2D.IsSceneLit(camera);
            if (isSceneLit)//||Application.isEditor)
            {
                RendererLighting.Setup(renderingData, m_Renderer2DData);

                CommandBuffer cmd = CommandBufferPool.Get("Render 2D Lighting");
                cmd.Clear();


                RenderTargetIdentifier s_tempOldColorBuffer = new RenderTargetIdentifier();
                RenderTargetIdentifier s_tempOlddepthBuffer = new RenderTargetIdentifier();


                cmd.SetGlobalFloat("_HDREmulationScale", m_Renderer2DData.hdrEmulationScale);
                cmd.SetGlobalFloat("_InverseHDREmulationScale", 1.0f / m_Renderer2DData.hdrEmulationScale);
                cmd.SetGlobalFloat("_UseSceneLighting", isLitView ? 1.0f : 0.0f);
                cmd.SetGlobalColor("_RendererColor", Color.white);
                RendererLighting.SetShapeLightShaderGlobals(cmd);

                context.ExecuteCommandBuffer(cmd);

                DrawingSettings combinedDrawSettings = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                DrawingSettings normalsDrawSettings = CreateDrawingSettings(k_NormalsRenderingPassName, ref renderingData, SortingCriteria.CommonTransparent);

                SortingSettings sortSettings = combinedDrawSettings.sortingSettings;
                GetTransparencySortingMode(camera, ref sortSettings);
                combinedDrawSettings.sortingSettings = sortSettings;


                //清理队列数据
                for (int i = 0; i < m_hasBeenInitialized.Length; ++i)
                {
                    m_hasBeenInitialized[i] = false;
                }

                bool bLightEffect = false;
                bool blendStyleUsed = false;
                int blendStyleIndex = 0;
                Light2D.LightStats lightStats;
                //检测一次是否有灯光起作用
                for (int i = 0; i < s_SortingLayers.Length; i++)
                {
                    int layerToRender = s_SortingLayers[i].id;
                    lightStats = Light2D.GetLightStatsByLayer(layerToRender, camera);

                    blendStyleUsed = false;
                    for (blendStyleIndex = 0; blendStyleIndex < blendStylesCount; blendStyleIndex++)
                    {
                        uint blendStyleMask = (uint)(1 << blendStyleIndex);
                        blendStyleUsed = (lightStats.blendStylesUsed & blendStyleMask) > 0;
                        if (blendStyleUsed)
                        {
                            break;
                        }
                    }

                    if (blendStyleUsed && RendererLighting.CanRenderLightSet(camera, blendStyleIndex, layerToRender, Light2D.GetLightsByBlendStyle(blendStyleIndex)))
                    {
                        bLightEffect = true;
                        break;
                    }

                }

                for (int i = 0; i < s_SortingLayers.Length; i++)
                {

                    // Some renderers override their sorting layer value with short.MinValue or short.MaxValue.
                    // When drawing the first sorting layer, we should include the range from short.MinValue to layerValue.
                    // Similarly, when drawing the last sorting layer, include the range from layerValue to short.MaxValue.
                    short layerValue = (short)s_SortingLayers[i].value;
                    var lowerBound = (i == 0) ? short.MinValue : layerValue;
                    var upperBound = (i == s_SortingLayers.Length - 1) ? short.MaxValue : layerValue;


                    m_sortLayerRange.lowerBound = lowerBound;
                    m_sortLayerRange.upperBound = upperBound;

                    //m_filterSettings.sortingLayerRange = new SortingLayerRange(lowerBound, upperBound);
                    m_filterSettings.sortingLayerRange = m_sortLayerRange;


                    int layerToRender = s_SortingLayers[i].id;


                    lightStats = Light2D.GetLightStatsByLayer(layerToRender, camera);

                    cmd.Clear();
                    for (blendStyleIndex = 0; blendStyleIndex < blendStylesCount; blendStyleIndex++)
                    {
                        uint blendStyleMask = (uint)(1 << blendStyleIndex);
                        blendStyleUsed = (lightStats.blendStylesUsed & blendStyleMask) > 0;

                        //有启用的光效才执行，灯光RT的创建
                        if (bLightEffect)
                        {
                            if (blendStyleUsed && !m_hasBeenInitialized[blendStyleIndex])
                            {
                                RendererLighting.CreateBlendStyleRenderTexture(cmd, blendStyleIndex);
                                m_hasBeenInitialized[blendStyleIndex] = true;
                            }
                        }

                        RendererLighting.EnableBlendStyle(cmd, blendStyleIndex, blendStyleUsed&& bLightEffect);
                    }


                    // Start Rendering
                    if (lightStats.totalNormalMapUsage > 0)
                    {
                        //先创建normal 纹理
                        RendererLighting.CreateNormalMapRenderTexture(cmd);
                        context.ExecuteCommandBuffer(cmd);
                        RendererLighting.RenderNormals(context, renderingData.cullResults, normalsDrawSettings, m_filterSettings, depthAttachment);

                    }
                    else
                    {
                        
                        context.ExecuteCommandBuffer(cmd);
                    }

                    cmd.Clear();
                    if (bLightEffect) //有启用的光效才执行，灯光渲染
                    {
                        if (lightStats.totalLights > 0)
                        {
                            RendererLighting.RenderLights(camera, cmd, layerToRender, lightStats.blendStylesUsed);
                        }
                        else
                        {
                            RendererLighting.ClearDirtyLighting(cmd, lightStats.blendStylesUsed);
                        }
                    }


                    if(bLightEffect||s_tempOldColorBuffer != colorAttachment|| s_tempOlddepthBuffer!= depthAttachment)
                    {
                        s_tempOldColorBuffer = colorAttachment;
                        s_tempOlddepthBuffer = depthAttachment;
                        CoreUtils.SetRenderTarget(cmd, colorAttachment, depthAttachment, ClearFlag.None, Color.white);
                    }

                    context.ExecuteCommandBuffer(cmd);

                    Profiler.BeginSample("RenderSpritesWithLighting - Draw Transparent Renderers");
                    context.DrawRenderers(renderingData.cullResults, ref combinedDrawSettings, ref m_filterSettings);
                    Profiler.EndSample();

                    if (lightStats.totalVolumetricUsage > 0)
                    {

                        cmd.Clear();
                        RendererLighting.RenderLightVolumes(camera, cmd, layerToRender, colorAttachment, depthAttachment, lightStats.blendStylesUsed);
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                    }
                }

                cmd.Clear();
                Profiler.BeginSample("RenderSpritesWithLighting - Release RenderTextures");
                RendererLighting.ReleaseRenderTextures(cmd);
                Profiler.EndSample();

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                m_filterSettings.sortingLayerRange = SortingLayerRange.all;
                RenderingUtils.RenderObjectsWithError(context, ref renderingData.cullResults, camera, m_filterSettings, SortingCriteria.None);
            }
            else
            {
                CommandBuffer cmd = CommandBufferPool.Get("Render Unlit");


              
                DrawingSettings unlitDrawSettings = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);

                //DrawingSettings unlitDrawSettings = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);


                CoreUtils.SetRenderTarget(cmd, colorAttachment, depthAttachment, ClearFlag.None, Color.white);

                /*
                cmd.SetGlobalTexture("_ShapeLightTexture0", Texture2D.blackTexture);
                cmd.SetGlobalTexture("_ShapeLightTexture1", Texture2D.blackTexture);
                cmd.SetGlobalTexture("_ShapeLightTexture2", Texture2D.blackTexture);
                cmd.SetGlobalTexture("_ShapeLightTexture3", Texture2D.blackTexture);
                */

                /* 
                

                cmd.SetGlobalTexture("_ShapeLightTexture0", Texture2D.whiteTexture);
                cmd.SetGlobalTexture("_ShapeLightTexture1", Texture2D.whiteTexture);
                cmd.SetGlobalTexture("_ShapeLightTexture2", Texture2D.whiteTexture);
                cmd.SetGlobalTexture("_ShapeLightTexture3", Texture2D.whiteTexture);
                 */

                cmd.DisableShaderKeyword("USE_SHAPE_LIGHT_TYPE_0");
                cmd.SetGlobalFloat("_UseSceneLighting", isLitView ? 1.0f : 0.0f);
                cmd.SetGlobalColor("_RendererColor", Color.white);

                 
                //cmd.SetGlobalFloat("_UseSceneLighting",  0.0f);
                //cmd.DisableShaderKeyword("USE_SHAPE_LIGHT_TYPE_0");
                // cmd.SetGlobalColor("_RendererColor", Color.white);


                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                Profiler.BeginSample("Render Sprites Unlit");
                context.DrawRenderers(renderingData.cullResults, ref unlitDrawSettings, ref m_filterSettings);
                Profiler.EndSample();

                RenderingUtils.RenderObjectsWithError(context, ref renderingData.cullResults, camera, m_filterSettings, SortingCriteria.None);
            }
        }
    }
}
