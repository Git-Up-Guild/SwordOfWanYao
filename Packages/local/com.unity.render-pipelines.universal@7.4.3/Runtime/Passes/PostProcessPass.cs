using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal
{
    // TODO: xmldoc
    public interface IPostProcessComponent
    {
        bool IsActive();
        bool IsTileCompatible();
    }

    //后处理完成回调
    public interface IPostProcesssSink
    {
        void OnPostProcess(Camera camera, bool hadProcessPass);
    }
}

namespace UnityEngine.Rendering.Universal.Internal
{
    // TODO: TAA
    // TODO: Motion blur
    /// <summary>
    /// Renders the post-processing effect stack.
    /// </summary>
    public class PostProcessPass : ScriptableRenderPass
    {
        RenderTextureDescriptor m_Descriptor;
        RenderTargetHandle m_Source;
        RenderTargetHandle m_Destination;
        RenderTargetHandle m_Depth;
        RenderTargetHandle m_InternalLut;

        const string k_RenderPostProcessingTag = "Render PostProcessing Effects";
        const string k_RenderFinalPostProcessingTag = "Render Final PostProcessing Pass";

        MaterialLibrary m_Materials;
        PostProcessData m_Data;

        // Builtin effects settings
        DepthOfField m_DepthOfField;
        MotionBlur m_MotionBlur;
        PaniniProjection m_PaniniProjection;
        Bloom m_Bloom;
        LensDistortion m_LensDistortion;
        ChromaticAberration m_ChromaticAberration;
        Vignette m_Vignette;
        ColorLookup m_ColorLookup;
        ColorAdjustments m_ColorAdjustments;
        Tonemapping m_Tonemapping;
        FilmGrain m_FilmGrain;

        //custom
        ColorMultiply m_ColorMultiply;
        CaptureBlur m_CaptureBlur;

        // Misc
        const int k_MaxPyramidSize = 16;

        //bloom的设置
        int k_MaxWidth = 360;
        int k_MaxHeight = 640;
        int k_MaxBloomIterations = k_MaxPyramidSize;
        public static bool s_enableBloom = true;


        readonly GraphicsFormat m_DefaultHDRFormat;
        bool m_UseRGBM;
        readonly GraphicsFormat m_SMAAEdgeFormat;
        readonly GraphicsFormat m_GaussianCoCFormat;
        Matrix4x4 m_PrevViewProjM = Matrix4x4.identity;
        bool m_ResetHistory;
        int m_DitheringTextureIndex;
        RenderTargetIdentifier[] m_MRT2;
        Vector4[] m_BokehKernel;
        int m_BokehHash;
        bool m_IsStereo;

        // True when this is the very last pass in the pipeline
        bool m_IsFinalPass;

        // If there's a final post process pass after this pass.
        // If yes, Film Grain and Dithering are setup in the final pass, otherwise they are setup in this pass.
        bool m_HasFinalPass;

        // Some Android devices do not support sRGB backbuffer
        // We need to do the conversion manually on those
        bool m_EnableSRGBConversionIfNeeded;

        Material m_BlitMaterial;

        //是否有后处理pass
        static IPostProcesssSink s_postProcessPass = null;

        //设置后处理回调
        public static void SetProcessPassSink(IPostProcesssSink sink)
        {
            s_postProcessPass = sink;
        }

        public PostProcessPass(RenderPassEvent evt, PostProcessData data, Material blitMaterial = null)
        {
           
            renderPassEvent = evt;
            m_Data = data;
            m_Materials = new MaterialLibrary(data);
            m_BlitMaterial = blitMaterial;

            // Texture format pre-lookup
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Linear | FormatUsage.Render))
            {
                m_DefaultHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
                m_UseRGBM = false;
            }
            else
            {
                m_DefaultHDRFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;
                m_UseRGBM = true;
            }

            // Only two components are needed for edge render texture, but on some vendors four components may be faster.
            if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, FormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
                m_SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
            else
                m_SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;

            if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, FormatUsage.Linear | FormatUsage.Render))
                m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
            else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, FormatUsage.Linear | FormatUsage.Render))
                m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
            else // Expect CoC banding
                m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;

            // Bloom pyramid shader ids - can't use a simple stackalloc in the bloom function as we
            // unfortunately need to allocate strings
            ShaderConstants._BloomMipUp = new int[k_MaxPyramidSize];
            ShaderConstants._BloomMipDown = new int[k_MaxPyramidSize];

            for (int i = 0; i < k_MaxPyramidSize; i++)
            {
                ShaderConstants._BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
                ShaderConstants._BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            }

            m_MRT2 = new RenderTargetIdentifier[2];
            m_ResetHistory = true;
        }

        public void Cleanup() => m_Materials.Cleanup();

        public void Setup(in RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source, in RenderTargetHandle destination, in RenderTargetHandle depth, in RenderTargetHandle internalLut, bool hasFinalPass, bool enableSRGBConversion)
        {
            m_Descriptor = baseDescriptor;
            m_Source = source;
            m_Destination = destination;
            m_Depth = depth;
            m_InternalLut = internalLut;
            m_IsFinalPass = false;
            m_HasFinalPass = hasFinalPass;
            m_EnableSRGBConversionIfNeeded = enableSRGBConversion;

            k_MaxBloomIterations = UniversalRenderPipeline.asset.bloomIterations;
            k_MaxWidth = UniversalRenderPipeline.asset.resolutionWidth >> 1;
            k_MaxHeight = UniversalRenderPipeline.asset.resolutionHeight >> 1;

        }

        public void SetupFinalPass(in RenderTargetHandle source)
        {
            m_Source = source;
            m_Destination = RenderTargetHandle.CameraTarget;
            m_IsFinalPass = true;
            m_HasFinalPass = false;
            m_EnableSRGBConversionIfNeeded = true;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (m_Destination == RenderTargetHandle.CameraTarget)
                return;

            var desc = cameraTextureDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_Destination.id, desc, FilterMode.Point);
        }

        public void ResetHistory()
        {
            m_ResetHistory = true;
        }

        public bool CanRunOnTile()
        {
            // Check builtin & user effects here
            return false;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Start by pre-fetching all builtin effect settings we need
            // Some of the color-grading settings are only used in the color grading lut pass
            var stack = VolumeManager.instance.stack;
            m_DepthOfField = stack.GetComponent<DepthOfField>();
            m_MotionBlur = stack.GetComponent<MotionBlur>();
            m_PaniniProjection = stack.GetComponent<PaniniProjection>();
            m_Bloom = stack.GetComponent<Bloom>();
            m_LensDistortion = stack.GetComponent<LensDistortion>();
            m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
            m_Vignette = stack.GetComponent<Vignette>();
            m_ColorLookup = stack.GetComponent<ColorLookup>();
            m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
            m_Tonemapping = stack.GetComponent<Tonemapping>();
            m_FilmGrain = stack.GetComponent<FilmGrain>();
            m_ColorMultiply = stack.GetComponent<ColorMultiply>();
            m_CaptureBlur = stack.GetComponent<CaptureBlur>();

            if (m_IsFinalPass)
            {
                var cmd = CommandBufferPool.Get(k_RenderFinalPostProcessingTag);
                RenderFinalPass(cmd, ref renderingData);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            else if (CanRunOnTile())
            {
                // TODO: Add a fast render path if only on-tile compatible effects are used and we're actually running on a platform that supports it
                // Note: we can still work on-tile if FXAA is enabled, it'd be part of the final pass
            }
            else
            {
                // Regular render path (not on-tile) - we do everything in a single command buffer as it
                // makes it easier to manage temporary targets' lifetime
                var cmd = CommandBufferPool.Get(k_RenderPostProcessingTag);
                Render(cmd, ref renderingData);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            m_ResetHistory = false;
        }

        RenderTextureDescriptor GetStereoCompatibleDescriptor()
            => GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat, m_Descriptor.depthBufferBits);

        RenderTextureDescriptor GetStereoCompatibleDescriptor(int width, int height, GraphicsFormat format, int depthBufferBits = 0)
        {
            // Inherit the VR setup from the camera descriptor
            var desc = m_Descriptor;
            desc.depthBufferBits = depthBufferBits;
            desc.msaaSamples = 1;
            desc.width = width;
            desc.height = height;
            desc.graphicsFormat = format;
            return desc;
        }

        bool RequireSRGBConversionBlitToBackBuffer(CameraData cameraData)
        {
            bool requiresSRGBConversion = Display.main.requiresSrgbBlitToBackbuffer;
            // For stereo case, eye texture always want color data in sRGB space.
            // If eye texture color format is linear, we do explicit sRGB convertion
#if ENABLE_VR && ENABLE_VR_MODULE
            if (cameraData.isStereoEnabled)
                requiresSRGBConversion = !XRGraphics.eyeTextureDesc.sRGB;
#endif
            return requiresSRGBConversion;
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            bool hadProcessPass = false;

            ref var cameraData = ref renderingData.cameraData;
            m_IsStereo = renderingData.cameraData.isStereoEnabled;

            // Don't use these directly unless you have a good reason to, use GetSource() and
            // GetDestination() instead
            bool tempTargetUsed = false;
            bool tempTarget2Used = false;
            int source = m_Source.id;
            int destination = -1;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;

            // Utilities to simplify intermediate target management
            int GetSource() => source;

            int GetDestination()
            {
                if (destination == -1)
                {
                    cmd.GetTemporaryRT(ShaderConstants._TempTarget, GetStereoCompatibleDescriptor(), FilterMode.Bilinear);
                    destination = ShaderConstants._TempTarget;
                    tempTargetUsed = true;
                }
                else if (destination == m_Source.id && m_Descriptor.msaaSamples > 1)
                {
                    // Avoid using m_Source.id as new destination, it may come with a depth buffer that we don't want, may have MSAA that we don't want etc
                    cmd.GetTemporaryRT(ShaderConstants._TempTarget2, GetStereoCompatibleDescriptor(), FilterMode.Bilinear);
                    destination = ShaderConstants._TempTarget2;
                    tempTarget2Used = true;
                }

                return destination;
            }

            void Swap() => CoreUtils.Swap(ref source, ref destination);

            // Setup projection matrix for cmd.DrawMesh()
            cmd.SetGlobalMatrix(ShaderConstants._FullscreenProjMat, GL.GetGPUProjectionMatrix(Matrix4x4.identity, true));

            // Optional NaN killer before post-processing kicks in
            // stopNaN may be null on Adreno 3xx. It doesn't support full shader level 3.5, but SystemInfo.graphicsShaderLevel is 35.
            if (cameraData.isStopNaNEnabled && m_Materials.stopNaN != null)
            {
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.StopNaNs)))
                {
                    cmd.Blit(GetSource(), BlitDstDiscardContent(cmd, GetDestination()), m_Materials.stopNaN);
                    Swap();
                }
            }

            // Anti-aliasing
            if (cameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
            {

                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.SMAA)))
                {

                    destination =  m_Destination.id;
                    // GetDestination()
                    DoSubpixelMorphologicalAntialiasing(ref cameraData, cmd, GetSource(), destination);

                    //CoreUtils.Swap(ref m_Source.id, ref dstID);
                    //if(destination!=source)
                    Swap();
                }
            }

            // Depth of Field
            if (m_DepthOfField.IsActive() && !isSceneViewCamera)
            {
                var markerName = m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian
                    ? URPProfileId.GaussianDepthOfField
                    : URPProfileId.BokehDepthOfField;

                using (new ProfilingScope(cmd, ProfilingSampler.Get(markerName)))
                {
                    DoDepthOfField(cameraData.camera, cmd, GetSource(), GetDestination(), cameraData.pixelRect);
                    Swap();
                }
            }

            // Motion blur
            if (m_MotionBlur.IsActive() && !isSceneViewCamera)
            {
                hadProcessPass = true;
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.MotionBlur)))
                {
                    DoMotionBlur(cameraData.camera, cmd, GetSource(), GetDestination());
                    Swap();
                }
            }

            // Panini projection is done as a fullscreen pass after all depth-based effects are done
            // and before bloom kicks in
            if (m_PaniniProjection.IsActive() && !isSceneViewCamera)
            {
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.PaniniProjection)))
                {
                    DoPaniniProjection(cameraData.camera, cmd, GetSource(), GetDestination());
                    Swap();
                }
            }

            //生成模糊纹理
            m_CaptureBlur.Setup();
            if (m_CaptureBlur.IsActive() && !cameraData.isSceneViewCamera)
            {
                hadProcessPass = true;
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.CaptureBlur)))
                {
                    DoCaptureBlur(cameraData.camera, cmd, GetSource(), GetDestination());
                }
            }


            //变色
            if (m_ColorMultiply.IsActive() && !cameraData.isSceneViewCamera)
            {
                hadProcessPass = true;
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.Multiply)))
                {
                    DoMutiplyColor(cameraData.camera, cmd, GetSource(), GetDestination());
                    Swap();
                }
            }



            // Combined post-processing stack
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UberPostProcess)))
            {
                // Reset uber keywords
                m_Materials.uber.shaderKeywords = null;

                // Bloom goes first
                bool bloomActive = m_Bloom.IsActive() && s_enableBloom;
                if (bloomActive)
                {
                    hadProcessPass = true;
                    using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.Bloom)))
                        SetupBloom(cmd, GetSource(), m_Materials.uber);
                }

                // Setup other effects constants
                SetupLensDistortion(m_Materials.uber, isSceneViewCamera);
                SetupChromaticAberration(m_Materials.uber);
                SetupVignette(m_Materials.uber);

                if(ColorGradingLutPass.enableLUT)
                {
                    SetupColorGrading(cmd, ref renderingData, m_Materials.uber);
                }
                

                // Only apply dithering & grain if there isn't a final pass.
                SetupGrain(cameraData, m_Materials.uber);
                SetupDithering(cameraData, m_Materials.uber);

                if (RequireSRGBConversionBlitToBackBuffer(cameraData) && m_EnableSRGBConversionIfNeeded)
                    m_Materials.uber.EnableKeyword(ShaderKeywordStrings.LinearToSRGBConversion);

                // Done with Uber, blit it
                cmd.SetGlobalTexture("_BlitTex", GetSource());

                var colorLoadAction = RenderBufferLoadAction.DontCare;
                if (m_Destination == RenderTargetHandle.CameraTarget && !cameraData.isDefaultViewport)
                    colorLoadAction = RenderBufferLoadAction.Load;

                // Note: We rendering to "camera target" we need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
                // Overlay cameras need to output to the target described in the base camera while doing camera stack.
                RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
                cameraTarget = (m_Destination == RenderTargetHandle.CameraTarget) ? cameraTarget : m_Destination.Identifier();
               

                // With camera stacking we not always resolve post to final screen as we might run post-processing in the middle of the stack.
                bool finishPostProcessOnScreen = cameraData.resolveFinalTarget || (m_Destination == RenderTargetHandle.CameraTarget || m_HasFinalPass == true);

                if (m_IsStereo)
                {

                    cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

                    Blit(cmd, GetSource(), BuiltinRenderTextureType.CurrentActive, m_Materials.uber);

                    // TODO: We need a proper camera texture swap chain in URP.
                    // For now, when render post-processing in the middle of the camera stack (not resolving to screen)
                    // we do an extra blit to ping pong results back to color texture. In future we should allow a Swap of the current active color texture
                    // in the pipeline to avoid this extra blit.
                    if (!finishPostProcessOnScreen)
                    {
                        cmd.SetGlobalTexture("_BlitTex", cameraTarget);
                        Blit(cmd, BuiltinRenderTextureType.CurrentActive, m_Source.id, m_BlitMaterial);
                    }
                }
                else
                {

                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

                    if (m_Destination == RenderTargetHandle.CameraTarget)
                        cmd.SetViewport(cameraData.pixelRect);

                    if(finishPostProcessOnScreen|| bloomActive)
                    {
                        cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Materials.uber);
                    }
                   

                    // TODO: We need a proper camera texture swap chain in URP.
                    // For now, when render post-processing in the middle of the camera stack (not resolving to screen)
                    // we do an extra blit to ping pong results back to color texture. In future we should allow a Swap of the current active color texture
                    // in the pipeline to avoid this extra blit.
                    if (!finishPostProcessOnScreen)
                    {
                        if(bloomActive)
                        {
                            cmd.SetGlobalTexture("_BlitTex", cameraTarget);
                        }
                       

                        if(hadProcessPass)
                        {
                            cmd.SetRenderTarget(m_Source.id, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
                        }

                        
                    }
                   

                    cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
                }



                // Cleanup
                if (bloomActive)
                    cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[0]);

                if (tempTargetUsed)
                    cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget);

                if (tempTarget2Used)
                    cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget2);
            }

            //后处理完成，回调到外部
            if(null!= s_postProcessPass)
            {
                s_postProcessPass.OnPostProcess(renderingData.cameraData.camera,hadProcessPass);
            }


           

        }

        private BuiltinRenderTextureType BlitDstDiscardContent(CommandBuffer cmd, RenderTargetIdentifier rt)
        {

            // We set depth to DontCare because rt might be the source of PostProcessing used as a temporary target
            // Source typically comes with a depth buffer and right now we don't have a way to only bind the color attachment of a RenderTargetIdentifier
            cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            return BuiltinRenderTextureType.CurrentActive;
        }

        #region Sub-pixel Morphological Anti-aliasing

        void DoSubpixelMorphologicalAntialiasing(ref CameraData cameraData, CommandBuffer cmd, int source, int destination)
        {
            var camera = cameraData.camera;
            var pixelRect = cameraData.pixelRect;
            var material = m_Materials.subpixelMorphologicalAntialiasing;
            const int kStencilBit = 64;

            // Globals
            material.SetVector(ShaderConstants._Metrics, new Vector4(1f / m_Descriptor.width, 1f / m_Descriptor.height, m_Descriptor.width, m_Descriptor.height));
            material.SetTexture(ShaderConstants._AreaTexture, m_Data.textures.smaaAreaTex);
            material.SetTexture(ShaderConstants._SearchTexture, m_Data.textures.smaaSearchTex);
            material.SetInt(ShaderConstants._StencilRef, kStencilBit);
            material.SetInt(ShaderConstants._StencilMask, kStencilBit);

            // Quality presets
            material.shaderKeywords = null;

            switch (cameraData.antialiasingQuality)
            {
                case AntialiasingQuality.Low:
                    material.EnableKeyword(ShaderKeywordStrings.SmaaLow);
                    break;
                case AntialiasingQuality.Medium:
                    material.EnableKeyword(ShaderKeywordStrings.SmaaMedium);
                    break;
                case AntialiasingQuality.High:
                    material.EnableKeyword(ShaderKeywordStrings.SmaaHigh);
                    break;
            }

            // Intermediate targets
            RenderTargetIdentifier stencil; // We would only need stencil, no depth. But Unity doesn't support that.
            int tempDepthBits;
            if (m_Depth == RenderTargetHandle.CameraTarget || m_Descriptor.msaaSamples > 1)
            {
                // In case m_Depth is CameraTarget it may refer to the backbuffer and we can't use that as an attachment on all platforms
                stencil = ShaderConstants._EdgeTexture;
                tempDepthBits = 24;
            }
            else
            {
                stencil = m_Depth.Identifier();
                tempDepthBits = 0;
            }
            cmd.GetTemporaryRT(ShaderConstants._EdgeTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_SMAAEdgeFormat, tempDepthBits), FilterMode.Point);
            cmd.GetTemporaryRT(ShaderConstants._BlendTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8G8B8A8_UNorm), FilterMode.Point);

            // Prepare for manual blit
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(pixelRect);

            // Pass 1: Edge detection
            cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._EdgeTexture, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, stencil,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, 0);

            // Pass 2: Blend weights
            cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._BlendTexture, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, stencil,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._EdgeTexture);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, 1);

            // Pass 3: Neighborhood blending
            cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
            cmd.SetGlobalTexture(ShaderConstants._BlendTexture, ShaderConstants._BlendTexture);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, 2);

            // Cleanup
            cmd.ReleaseTemporaryRT(ShaderConstants._EdgeTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._BlendTexture);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        }

        #endregion

        #region Depth Of Field

        // TODO: CoC reprojection once TAA gets in LW
        // TODO: Proper LDR/gamma support
        void DoDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination, Rect pixelRect)
        {
            if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
                DoGaussianDepthOfField(camera, cmd, source, destination, pixelRect);
            else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
                DoBokehDepthOfField(cmd, source, destination, pixelRect);
        }

        void DoGaussianDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination, Rect pixelRect)
        {
            var material = m_Materials.gaussianDepthOfField;
            int wh = m_Descriptor.width / 2;
            int hh = m_Descriptor.height / 2;
            float farStart = m_DepthOfField.gaussianStart.value;
            float farEnd = Mathf.Max(farStart, m_DepthOfField.gaussianEnd.value);

            // Assumes a radius of 1 is 1 at 1080p
            // Past a certain radius our gaussian kernel will look very bad so we'll clamp it for
            // very high resolutions (4K+).
            float maxRadius = m_DepthOfField.gaussianMaxRadius.value * (wh / 1080f);
            maxRadius = Mathf.Min(maxRadius, 2f);

            CoreUtils.SetKeyword(material, ShaderKeywordStrings.HighQualitySampling, m_DepthOfField.highQualitySampling.value);
            material.SetVector(ShaderConstants._CoCParams, new Vector3(farStart, farEnd, maxRadius));

            // Temporary textures
            cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat), FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._HalfCoCTexture, GetStereoCompatibleDescriptor(wh, hh, m_GaussianCoCFormat), FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetStereoCompatibleDescriptor(wh, hh, m_DefaultHDRFormat), FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetStereoCompatibleDescriptor(wh, hh, m_DefaultHDRFormat), FilterMode.Bilinear);
            // Note: fresh temporary RTs don't require explicit RenderBufferLoadAction.DontCare, only when they are reused (such as PingTexture)

            // Compute CoC
            cmd.Blit(source, ShaderConstants._FullCoCTexture, material, 0);

            // Downscale & prefilter color + coc
            m_MRT2[0] = ShaderConstants._HalfCoCTexture;
            m_MRT2[1] = ShaderConstants._PingTexture;

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(pixelRect);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
            cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
            cmd.SetRenderTarget(m_MRT2, ShaderConstants._HalfCoCTexture, 0, CubemapFace.Unknown, -1);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, 1);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

            // Blur
            cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, ShaderConstants._HalfCoCTexture);
            cmd.Blit(ShaderConstants._PingTexture, ShaderConstants._PongTexture, material, 2);
            cmd.Blit(ShaderConstants._PongTexture, BlitDstDiscardContent(cmd, ShaderConstants._PingTexture), material, 3);

            // Composite
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._PingTexture);
            cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
            cmd.Blit(source, BlitDstDiscardContent(cmd, destination), material, 4);

            // Cleanup
            cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._HalfCoCTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
        }

        private static int[] _BlurMipDowns;
        private static int[] _BlurMipUps;
        private static int _BlurDownPass = 0;
        private static int _BlurUpPass = 1;
        private static int _BlurColorFlagPropID = Shader.PropertyToID("_BlurColorFlag");
        private static int _BlurColorPropID = Shader.PropertyToID("_BlurColor");

        void DoCaptureBlur(Camera camera, CommandBuffer cmd, int source, int destination)
        {



            CaptureBlur.s_Enable = false;
   

            if (m_CaptureBlur._RT.value != null)
            {
                Material material = m_CaptureBlur._Mat.value;

               // Debug.LogError("  if (m_CaptureBlur._RT.value != null)");
                /*
                RenderTargetIdentifier blurOutputTarget2 = new RenderTargetIdentifier(m_CaptureBlur._RT.value);
                Debug.LogError(" cmd.Blit(source, blurOutputTarget2, material, 3)");
                cmd.Blit(source, blurOutputTarget2, material, 3);
                Debug.LogError(" cmd.Blit(source, blurOutputTarget2, material, 2)");
                cmd.Blit(source, blurOutputTarget2, material, 2);
                return;
                */

                //初始化
                if (_BlurMipDowns == null)
                {
                    _BlurMipDowns = new int[CaptureBlur.BLUR_MIP_MAX_LEVL];
                    _BlurMipUps = new int[CaptureBlur.BLUR_MIP_MAX_LEVL];

                    //申请临时ID
                    for (int t = 0; t < CaptureBlur.BLUR_MIP_MAX_LEVL; t++)
                    {
                        _BlurMipDowns[t] = Shader.PropertyToID("_BlurMipDown" + t);
                        _BlurMipUps[t] = Shader.PropertyToID("_BlurMipUp" + t);
                    }
                }

               



               

                //模糊范围
                material.SetFloat(ShaderConstants._Blur, m_CaptureBlur._Blur.value);

                //第一次降采样是使用的参数，后面就是除2去降采样了
                int width = m_CaptureBlur._RT.value.width / m_CaptureBlur._BlurDownSample.value;
                int height = m_CaptureBlur._RT.value.height / m_CaptureBlur._BlurDownSample.value;

                //down
                //把初始图像作为lastdown的起始图去计算
                RenderTargetIdentifier lastDown = source;

                int loop = m_CaptureBlur._BlurLoop.value > CaptureBlur.BLUR_MIP_MAX_LEVL ? CaptureBlur.BLUR_MIP_MAX_LEVL : m_CaptureBlur._BlurLoop.value;
                for (int t = 0; t < loop; t++)
                {
                    int midDown = _BlurMipDowns[t];
                    int midUp = _BlurMipUps[t];

                    //对指定高宽申请RT，每个循环的指定RT都会变小为原来一半
                    cmd.GetTemporaryRT(midDown, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                    //同上，但是这里申请了并未计算，先把位置霸占了，这样在UP的循环里就不用申请RT了
                    cmd.GetTemporaryRT(midUp, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                    //计算down的pass
                    cmd.Blit(lastDown, midDown, material, _BlurDownPass);

                    lastDown = midDown;

                    //每次循环都降尺寸
                    width = Mathf.Max(width / 2, 1);
                    height = Mathf.Max(height / 2, 1);
                }

                //up
                //把down的最后一次图像当成up的第一张图去计算up
                int lastUp = _BlurMipDowns[loop - 1];

                //只需要在最后一次开启颜色混合标记
                material.SetInt(_BlurColorFlagPropID, 0);

                //这里减2是因为第一次已经有了要减去1，但是第一次是直接复制的，所以循环完后还得补一次up
                for (int j = loop - 2; j >= 0; j--)
                {
                    int midUp = _BlurMipUps[j];

                    //这里直接开干就是因为在down过程中已经把RT的位置霸占好了，这里直接用
                    cmd.Blit(lastUp, midUp, material, _BlurUpPass);

                    lastUp = midUp;
                }

                //最后一次了，需要开启标记
                material.SetInt(_BlurColorFlagPropID, 1);
                material.SetColor(_BlurColorPropID, m_CaptureBlur._BlurColor.value);
                RenderTargetIdentifier blurOutputTarget = new RenderTargetIdentifier(m_CaptureBlur._RT.value);
                cmd.Blit(lastUp, blurOutputTarget, material, _BlurUpPass);

                //释放临时RT
                for (int t = 0; t < loop; t++)
                {
                    cmd.ReleaseTemporaryRT(_BlurMipDowns[t]);
                    cmd.ReleaseTemporaryRT(_BlurMipUps[t]);
                }
            }

        }

        void DoMutiplyColor(Camera camera, CommandBuffer cmd, int source, int destination)
        {
            Material mat = m_ColorMultiply.m_mat.value;
            cmd.SetGlobalTexture("_BlitTex", source);
            cmd.SetGlobalColor("_Color", m_ColorMultiply.color.value);
            cmd.Blit(source, BlitDstDiscardContent(cmd, destination), mat, 0);
        }

        void PrepareBokehKernel()
        {
            const int kRings = 4;
            const int kPointsPerRing = 7;

            // Check the existing array
            if (m_BokehKernel == null)
                m_BokehKernel = new Vector4[42];

            // Fill in sample points (concentric circles transformed to rotated N-Gon)
            int idx = 0;
            float bladeCount = m_DepthOfField.bladeCount.value;
            float curvature = 1f - m_DepthOfField.bladeCurvature.value;
            float rotation = m_DepthOfField.bladeRotation.value * Mathf.Deg2Rad;
            const float PI = Mathf.PI;
            const float TWO_PI = Mathf.PI * 2f;

            for (int ring = 1; ring < kRings; ring++)
            {
                float bias = 1f / kPointsPerRing;
                float radius = (ring + bias) / (kRings - 1f + bias);
                int points = ring * kPointsPerRing;

                for (int point = 0; point < points; point++)
                {
                    // Angle on ring
                    float phi = 2f * PI * point / points;

                    // Transform to rotated N-Gon
                    // Adapted from "CryEngine 3 Graphics Gems" [Sousa13]
                    float nt = Mathf.Cos(PI / bladeCount);
                    float dt = Mathf.Cos(phi - (TWO_PI / bladeCount) * Mathf.Floor((bladeCount * phi + Mathf.PI) / TWO_PI));
                    float r = radius * Mathf.Pow(nt / dt, curvature);
                    float u = r * Mathf.Cos(phi - rotation);
                    float v = r * Mathf.Sin(phi - rotation);

                    m_BokehKernel[idx] = new Vector4(u, v);
                    idx++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float GetMaxBokehRadiusInPixels(float viewportHeight)
        {
            // Estimate the maximum radius of bokeh (empirically derived from the ring count)
            const float kRadiusInPixels = 14f;
            return Mathf.Min(0.05f, kRadiusInPixels / viewportHeight);
        }

        void DoBokehDepthOfField(CommandBuffer cmd, int source, int destination, Rect pixelRect)
        {
            var material = m_Materials.bokehDepthOfField;
            int wh = m_Descriptor.width / 2;
            int hh = m_Descriptor.height / 2;

            // "A Lens and Aperture Camera Model for Synthetic Image Generation" [Potmesil81]
            float F = m_DepthOfField.focalLength.value / 1000f;
            float A = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
            float P = m_DepthOfField.focusDistance.value;
            float maxCoC = (A * F) / (P - F);
            float maxRadius = GetMaxBokehRadiusInPixels(m_Descriptor.height);
            float rcpAspect = 1f / (wh / (float)hh);

            cmd.SetGlobalVector(ShaderConstants._CoCParams, new Vector4(P, maxCoC, maxRadius, rcpAspect));

            // Prepare the bokeh kernel constant buffer
            int hash = m_DepthOfField.GetHashCode();
            if (hash != m_BokehHash)
            {
                m_BokehHash = hash;
                PrepareBokehKernel();
            }

            cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, m_BokehKernel);

            // Temporary textures
            cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm), FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetStereoCompatibleDescriptor(wh, hh, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetStereoCompatibleDescriptor(wh, hh, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);

            // Compute CoC
            cmd.Blit(source, ShaderConstants._FullCoCTexture, material, 0);
            cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);

            // Downscale & prefilter color + coc
            cmd.Blit(source, ShaderConstants._PingTexture, material, 1);

            // Bokeh blur
            cmd.Blit(ShaderConstants._PingTexture, ShaderConstants._PongTexture, material, 2);

            // Post-filtering
            cmd.Blit(ShaderConstants._PongTexture, BlitDstDiscardContent(cmd, ShaderConstants._PingTexture), material, 3);

            // Composite
            cmd.SetGlobalTexture(ShaderConstants._DofTexture, ShaderConstants._PingTexture);
            cmd.Blit(source, BlitDstDiscardContent(cmd, destination), material, 4);

            // Cleanup
            cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
        }

        #endregion

        #region Motion Blur

        void DoMotionBlur(Camera camera, CommandBuffer cmd, int source, int destination)
        {
            var material = m_Materials.cameraMotionBlur;

            // This is needed because Blit will reset viewproj matrices to identity and UniversalRP currently
            // relies on SetupCameraProperties instead of handling its own matrices.
            // TODO: We need get rid of SetupCameraProperties and setup camera matrices in Universal
            var proj = camera.nonJitteredProjectionMatrix;
            var view = camera.worldToCameraMatrix;
            var viewProj = proj * view;

            material.SetMatrix("_ViewProjM", viewProj);

            if (m_ResetHistory)
                material.SetMatrix("_PrevViewProjM", viewProj);
            else
                material.SetMatrix("_PrevViewProjM", m_PrevViewProjM);

            material.SetFloat("_Intensity", m_MotionBlur.intensity.value);
            material.SetFloat("_Clamp", m_MotionBlur.clamp.value);
            cmd.Blit(source, BlitDstDiscardContent(cmd, destination), material, (int)m_MotionBlur.quality.value);

            m_PrevViewProjM = viewProj;
        }

        #endregion

        #region Panini Projection

        // Back-ported & adapted from the work of the Stockholm demo team - thanks Lasse!
        void DoPaniniProjection(Camera camera, CommandBuffer cmd, int source, int destination)
        {
            float distance = m_PaniniProjection.distance.value;
            var viewExtents = CalcViewExtents(camera);
            var cropExtents = CalcCropExtents(camera, distance);

            float scaleX = cropExtents.x / viewExtents.x;
            float scaleY = cropExtents.y / viewExtents.y;
            float scaleF = Mathf.Min(scaleX, scaleY);

            float paniniD = distance;
            float paniniS = Mathf.Lerp(1f, Mathf.Clamp01(scaleF), m_PaniniProjection.cropToFit.value);

            var material = m_Materials.paniniProjection;
            material.SetVector(ShaderConstants._Params, new Vector4(viewExtents.x, viewExtents.y, paniniD, paniniS));
            material.EnableKeyword(
                1f - Mathf.Abs(paniniD) > float.Epsilon
                ? ShaderKeywordStrings.PaniniGeneric : ShaderKeywordStrings.PaniniUnitDistance
            );

            cmd.Blit(source, BlitDstDiscardContent(cmd, destination), material);
        }

        Vector2 CalcViewExtents(Camera camera)
        {
            float fovY = camera.fieldOfView * Mathf.Deg2Rad;
            float aspect = m_Descriptor.width / (float)m_Descriptor.height;

            float viewExtY = Mathf.Tan(0.5f * fovY);
            float viewExtX = aspect * viewExtY;

            return new Vector2(viewExtX, viewExtY);
        }

        Vector2 CalcCropExtents(Camera camera, float d)
        {
            // given
            //    S----------- E--X-------
            //    |    `  ~.  /,´
            //    |-- ---    Q
            //    |        ,/    `
            //  1 |      ,´/       `
            //    |    ,´ /         ´
            //    |  ,´  /           ´
            //    |,`   /             ,
            //    O    /
            //    |   /               ,
            //  d |  /
            //    | /                ,
            //    |/                .
            //    P
            //    |              ´
            //    |         , ´
            //    +-    ´
            //
            // have X
            // want to find E

            float viewDist = 1f + d;

            var projPos = CalcViewExtents(camera);
            var projHyp = Mathf.Sqrt(projPos.x * projPos.x + 1f);

            float cylDistMinusD = 1f / projHyp;
            float cylDist = cylDistMinusD + d;
            var cylPos = projPos * cylDistMinusD;

            return cylPos * (viewDist / cylDist);
        }

        #endregion

        #region Bloom



        void SetupBloom(CommandBuffer cmd, int source, Material uberMaterial)
        {
            // Start at half-res


            //int tw = UniversalRenderPipeline.GetMaxSize(m_Descriptor.width >> 1,k_MaxWidth);
            //int th = UniversalRenderPipeline.GetMaxSize(m_Descriptor.height >> 1,k_MaxHeight);

            int tw = m_Descriptor.width >> 1;
            int th = m_Descriptor.height >> 1;

            UniversalRenderPipeline.GetFixResolution(k_MaxWidth, k_MaxHeight, ref tw, ref th);

            // Determine the iteration count
            int maxSize = Mathf.Max(tw, th);
            int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);

            int mipCount = Mathf.Clamp(iterations, 1, k_MaxPyramidSize);
            mipCount = Mathf.Min(k_MaxBloomIterations, mipCount);

            // Pre-filtering parameters
            float clamp = m_Bloom.clamp.value;
            float threshold = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
            float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee

            // Material setup
            float scatter = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
            var bloomMaterial = m_Materials.bloom;
            bloomMaterial.SetVector(ShaderConstants._Params, new Vector4(scatter, clamp, threshold, thresholdKnee));
            CoreUtils.SetKeyword(bloomMaterial, ShaderKeywordStrings.BloomHQ, m_Bloom.highQualityFiltering.value);
            CoreUtils.SetKeyword(bloomMaterial, ShaderKeywordStrings.UseRGBM, m_UseRGBM);

            // Prefilter
            var desc = GetStereoCompatibleDescriptor(tw, th, m_DefaultHDRFormat);
            cmd.GetTemporaryRT(ShaderConstants._BloomMipDown[0], desc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._BloomMipUp[0], desc, FilterMode.Bilinear);
            cmd.Blit(source, ShaderConstants._BloomMipDown[0], bloomMaterial, 0);

            // Downsample - gaussian pyramid
            int lastDown = ShaderConstants._BloomMipDown[0];
            for (int i = 1; i < mipCount; i++)
            {
                tw = Mathf.Max(1, tw >> 1);
                th = Mathf.Max(1, th >> 1);
                int mipDown = ShaderConstants._BloomMipDown[i];
                int mipUp = ShaderConstants._BloomMipUp[i];

                desc.width = tw;
                desc.height = th;

                cmd.GetTemporaryRT(mipDown, desc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(mipUp, desc, FilterMode.Bilinear);

                // Classic two pass gaussian blur - use mipUp as a temporary target
                //   First pass does 2x downsampling + 9-tap gaussian
                //   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering
                cmd.Blit(lastDown, mipUp, bloomMaterial, 1);
                cmd.Blit(mipUp, mipDown, bloomMaterial, 2);
                lastDown = mipDown;
            }

            // Upsample (bilinear by default, HQ filtering does bicubic instead
            for (int i = mipCount - 2; i >= 0; i--)
            {
                int lowMip = (i == mipCount - 2) ? ShaderConstants._BloomMipDown[i + 1] : ShaderConstants._BloomMipUp[i + 1];
                int highMip = ShaderConstants._BloomMipDown[i];
                int dst = ShaderConstants._BloomMipUp[i];

                cmd.SetGlobalTexture(ShaderConstants._MainTexLowMip, lowMip);
                cmd.Blit(highMip, BlitDstDiscardContent(cmd, dst), bloomMaterial, 3);
            }

            // Cleanup
            for (int i = 0; i < mipCount; i++)
            {
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipDown[i]);
                if (i > 0) cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[i]);
            }

            // Setup bloom on uber
            var tint = m_Bloom.tint.value.linear;
            var luma = ColorUtils.Luminance(tint);
            tint = luma > 0f ? tint * (1f / luma) : Color.white;

            var bloomParams = new Vector4(m_Bloom.intensity.value, tint.r, tint.g, tint.b);
            uberMaterial.SetVector(ShaderConstants._Bloom_Params, bloomParams);
            uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);

            cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, ShaderConstants._BloomMipUp[0]);

            // Setup lens dirtiness on uber
            // Keep the aspect ratio correct & center the dirt texture, we don't want it to be
            // stretched or squashed
            var dirtTexture = m_Bloom.dirtTexture.value == null ? Texture2D.blackTexture : m_Bloom.dirtTexture.value;
            float dirtRatio = dirtTexture.width / (float)dirtTexture.height;
            float screenRatio = m_Descriptor.width / (float)m_Descriptor.height;
            var dirtScaleOffset = new Vector4(1f, 1f, 0f, 0f);
            float dirtIntensity = m_Bloom.dirtIntensity.value;

            if (dirtRatio > screenRatio)
            {
                dirtScaleOffset.x = screenRatio / dirtRatio;
                dirtScaleOffset.z = (1f - dirtScaleOffset.x) * 0.5f;
            }
            else if (screenRatio > dirtRatio)
            {
                dirtScaleOffset.y = dirtRatio / screenRatio;
                dirtScaleOffset.w = (1f - dirtScaleOffset.y) * 0.5f;
            }

            uberMaterial.SetVector(ShaderConstants._LensDirt_Params, dirtScaleOffset);
            uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, dirtIntensity);
            uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, dirtTexture);

            // Keyword setup - a bit convoluted as we're trying to save some variants in Uber...
            if (m_Bloom.highQualityFiltering.value)
                uberMaterial.EnableKeyword(dirtIntensity > 0f ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
            else
                uberMaterial.EnableKeyword(dirtIntensity > 0f ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
        }

        #endregion

        #region Lens Distortion

        void SetupLensDistortion(Material material, bool isSceneView)
        {
            float amount = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
            float theta = Mathf.Deg2Rad * Mathf.Min(160f, amount);
            float sigma = 2f * Mathf.Tan(theta * 0.5f);
            var center = m_LensDistortion.center.value * 2f - Vector2.one;
            var p1 = new Vector4(
                center.x,
                center.y,
                Mathf.Max(m_LensDistortion.xMultiplier.value, 1e-4f),
                Mathf.Max(m_LensDistortion.yMultiplier.value, 1e-4f)
            );
            var p2 = new Vector4(
                m_LensDistortion.intensity.value >= 0f ? theta : 1f / theta,
                sigma,
                1f / m_LensDistortion.scale.value,
                m_LensDistortion.intensity.value * 100f
            );

            material.SetVector(ShaderConstants._Distortion_Params1, p1);
            material.SetVector(ShaderConstants._Distortion_Params2, p2);

            if (m_LensDistortion.IsActive() && !isSceneView)
                material.EnableKeyword(ShaderKeywordStrings.Distortion);
        }

        #endregion

        #region Chromatic Aberration

        void SetupChromaticAberration(Material material)
        {
            material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);

            if (m_ChromaticAberration.IsActive())
                material.EnableKeyword(ShaderKeywordStrings.ChromaticAberration);
        }

        #endregion

        #region Vignette

        void SetupVignette(Material material)
        {
            var color = m_Vignette.color.value;
            var center = m_Vignette.center.value;
            var aspectRatio = m_Descriptor.width / (float)m_Descriptor.height;

            if (m_IsStereo && XRGraphics.stereoRenderingMode == XRGraphics.StereoRenderingMode.SinglePass)
                aspectRatio *= 0.5f;

            var v1 = new Vector4(
                color.r, color.g, color.b,
                m_Vignette.rounded.value ? aspectRatio : 1f
            );
            var v2 = new Vector4(
                center.x, center.y,
                m_Vignette.intensity.value * 3f,
                m_Vignette.smoothness.value * 5f
            );

            material.SetVector(ShaderConstants._Vignette_Params1, v1);
            material.SetVector(ShaderConstants._Vignette_Params2, v2);
        }

        #endregion

        #region Color Grading

        void SetupColorGrading(CommandBuffer cmd, ref RenderingData renderingData, Material material)
        {
            ref var postProcessingData = ref renderingData.postProcessingData;
            bool hdr = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
            int lutHeight = postProcessingData.lutSize;
            int lutWidth = lutHeight * lutHeight;

            // Source material setup
            float postExposureLinear = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
            cmd.SetGlobalTexture(ShaderConstants._InternalLut, m_InternalLut.Identifier());
            material.SetVector(ShaderConstants._Lut_Params, new Vector4(1f / lutWidth, 1f / lutHeight, lutHeight - 1f, postExposureLinear));
            material.SetTexture(ShaderConstants._UserLut, m_ColorLookup.texture.value);
            material.SetVector(ShaderConstants._UserLut_Params, !m_ColorLookup.IsActive()
                ? Vector4.zero
                : new Vector4(1f / m_ColorLookup.texture.value.width,
                              1f / m_ColorLookup.texture.value.height,
                              m_ColorLookup.texture.value.height - 1f,
                              m_ColorLookup.contribution.value)
            );

            if (hdr)
            {
                material.EnableKeyword(ShaderKeywordStrings.HDRGrading);
            }
            else
            {
                switch (m_Tonemapping.mode.value)
                {
                    case TonemappingMode.Neutral: material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral); break;
                    case TonemappingMode.ACES: material.EnableKeyword(ShaderKeywordStrings.TonemapACES); break;
                    default: break; // None
                }
            }
        }

        #endregion

        #region Film Grain

        void SetupGrain(in CameraData cameraData, Material material)
        {
            if (!m_HasFinalPass && m_FilmGrain.IsActive())
            {
                material.EnableKeyword(ShaderKeywordStrings.FilmGrain);
                PostProcessUtils.ConfigureFilmGrain(
                    m_Data,
                    m_FilmGrain,
                    cameraData.pixelWidth, cameraData.pixelHeight,
                    material
                );
            }
        }

        #endregion

        #region 8-bit Dithering

        void SetupDithering(in CameraData cameraData, Material material)
        {
            if (!m_HasFinalPass && cameraData.isDitheringEnabled)
            {
                material.EnableKeyword(ShaderKeywordStrings.Dithering);
                m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(
                    m_Data,
                    m_DitheringTextureIndex,
                    cameraData.pixelWidth, cameraData.pixelHeight,
                    material
                );
            }
        }

        #endregion

        #region Final pass

        void RenderFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var material = m_Materials.finalPass;
            material.shaderKeywords = null;

            // FXAA setup
            if (cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing)
                material.EnableKeyword(ShaderKeywordStrings.Fxaa);

            SetupGrain(cameraData, material);
            SetupDithering(cameraData, material);

            if (RequireSRGBConversionBlitToBackBuffer(cameraData) && m_EnableSRGBConversionIfNeeded)
                material.EnableKeyword(ShaderKeywordStrings.LinearToSRGBConversion);

            cmd.SetGlobalTexture("_BlitTex", m_Source.Identifier());

            var colorLoadAction = cameraData.isDefaultViewport ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;

            // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
            // Overlay cameras need to output to the target described in the base camera while doing camera stack.
            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
            cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

            if (cameraData.isStereoEnabled)
            {
                Blit(cmd, m_Source.Identifier(), BuiltinRenderTextureType.CurrentActive, material);
            }
            else
            {
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.SetViewport(cameraData.pixelRect);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
                cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
            }
        }

        #endregion

        #region Internal utilities

        class MaterialLibrary
        {
            public readonly Material stopNaN;
            public readonly Material subpixelMorphologicalAntialiasing;
            public readonly Material gaussianDepthOfField;
            public readonly Material bokehDepthOfField;
            public readonly Material cameraMotionBlur;
            public readonly Material paniniProjection;
            public readonly Material bloom;
            public readonly Material uber;
            public readonly Material finalPass;

            public MaterialLibrary(PostProcessData data)
            {
                stopNaN = Load(data.shaders.stopNanPS);
                subpixelMorphologicalAntialiasing = Load(data.shaders.subpixelMorphologicalAntialiasingPS);
                gaussianDepthOfField = Load(data.shaders.gaussianDepthOfFieldPS);
                bokehDepthOfField = Load(data.shaders.bokehDepthOfFieldPS);
                cameraMotionBlur = Load(data.shaders.cameraMotionBlurPS);
                paniniProjection = Load(data.shaders.paniniProjectionPS);
                bloom = Load(data.shaders.bloomPS);
                uber = Load(data.shaders.uberPostPS);
                finalPass = Load(data.shaders.finalPostPassPS);
            }

            Material Load(Shader shader)
            {
                if (shader == null)
                {
                    Debug.LogErrorFormat($"Missing shader. {GetType().DeclaringType.Name} render pass will not execute. Check for missing reference in the renderer resources.");
                    return null;
                }
                else if (!shader.isSupported)
                {
                    return null;
                }

                return CoreUtils.CreateEngineMaterial(shader);
            }

            internal void Cleanup()
            {
                CoreUtils.Destroy(stopNaN);
                CoreUtils.Destroy(subpixelMorphologicalAntialiasing);
                CoreUtils.Destroy(gaussianDepthOfField);
                CoreUtils.Destroy(bokehDepthOfField);
                CoreUtils.Destroy(cameraMotionBlur);
                CoreUtils.Destroy(paniniProjection);
                CoreUtils.Destroy(bloom);
                CoreUtils.Destroy(uber);
                CoreUtils.Destroy(finalPass);
            }
        }

        // Precomputed shader ids to same some CPU cycles (mostly affects mobile)
        static class ShaderConstants
        {
            public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");
            public static readonly int _TempTarget2 = Shader.PropertyToID("_TempTarget2");

            public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");
            public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

            public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");
            public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");
            public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");
            public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");
            public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");
            public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");
            public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");

            public static readonly int _Metrics = Shader.PropertyToID("_Metrics");
            public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");
            public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");
            public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");
            public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

            public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");
            public static readonly int _Params = Shader.PropertyToID("_Params");
            public static readonly int _MainTexLowMip = Shader.PropertyToID("_MainTexLowMip");
            public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");
            public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");
            public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");
            public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");
            public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");
            public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");
            public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");
            public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");
            public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");
            public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");
            public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");
            public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");
            public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");
            public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");
            public static readonly int _UserLut = Shader.PropertyToID("_UserLut");
            public static readonly int _Blur = Shader.PropertyToID("_Blur");

            public static readonly int _FullscreenProjMat = Shader.PropertyToID("_FullscreenProjMat");

            public static int[] _BloomMipUp;
            public static int[] _BloomMipDown;
        }

        #endregion


        public static void EnableBloom(bool enable)
        {
            s_enableBloom = enable;
        }
    }
}
