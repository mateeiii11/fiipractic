using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
    using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace RetroShadersPro.URP
{
    public class CRTEffect : ScriptableRendererFeature
    {
        CRTRenderPass pass;

        public override void Create()
        {
            pass = new CRTRenderPass();
            name = "CRT";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var settings = VolumeManager.instance.stack.GetComponent<CRTSettings>();

            if (settings != null && settings.IsActive())
            {
#if UNITY_6000_0_OR_NEWER
                pass.CreateInterlacingTexture();
#endif

                renderer.EnqueuePass(pass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            pass.Dispose();
            base.Dispose(disposing);
        }

        class CRTRenderPass : ScriptableRenderPass
        {
            private Material material;
            private RTHandle tempTexHandle;
            private RTHandle interlaceTexHandle;

            private int frameCounter = 0;

            public CRTRenderPass()
            {
                profilingSampler = new ProfilingSampler("CRT Effect");

#if UNITY_6000_0_OR_NEWER
                requiresIntermediateTexture = true;
#endif
            }

            private void CreateMaterial()
            {
                var shader = Shader.Find("Retro Shaders Pro/Post Processing/CRT");

                if (shader == null)
                {
                    Debug.LogError("Cannot find shader: \"Retro Shaders Pro/Post Processing/CRT\".");
                    return;
                }

                material = new Material(shader);
            }

            private static RenderTextureDescriptor GetCopyPassDescriptor(RenderTextureDescriptor descriptor)
            {
                descriptor.msaaSamples = 1;
                descriptor.depthBufferBits = (int)DepthBits.None;

                var settings = VolumeManager.instance.stack.GetComponent<CRTSettings>();

                float modifier = 1.0f;

                if (settings.scaleParameters.value)
                {
                    modifier = (float)settings.verticalReferenceResolution.value / descriptor.height;
                }

                int width = (int)Mathf.Max(4, descriptor.width / (settings.pixelSize.value / modifier));
                int height = (int)Mathf.Max(4, descriptor.height / (settings.pixelSize.value / modifier));

                descriptor.width = width;
                descriptor.height = height;

                return descriptor;
            }

            private static RenderTextureDescriptor GetInterlaceDescriptor(RenderTextureDescriptor descriptor)
            {
                descriptor.msaaSamples = 1;
                descriptor.depthBufferBits = (int)DepthBits.None;

                return descriptor;
            }

#if UNITY_6000_0_OR_NEWER
            // Need to create the interlacing texture somewhere outside of Configure for Render Graph.
            public void CreateInterlacingTexture()
            {
                var descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
                RenderingUtils.ReAllocateHandleIfNeeded(ref interlaceTexHandle, GetInterlaceDescriptor(descriptor), name: "_CRTInterlacingTexture");
            }
#endif

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                ResetTarget();

                RenderingUtils.ReAllocateIfNeeded(ref tempTexHandle, GetCopyPassDescriptor(cameraTextureDescriptor), name: "_CRTColorCopy");
                RenderingUtils.ReAllocateIfNeeded(ref interlaceTexHandle, GetInterlaceDescriptor(cameraTextureDescriptor), name: "_CRTInterlacingTexture");

                base.Configure(cmd, cameraTextureDescriptor);
            }

            private void SetMaterialProperties(RTHandle interlacingTexture, int targetHeight, Material material)
            {
                var settings = VolumeManager.instance.stack.GetComponent<CRTSettings>();

                renderPassEvent = settings.renderPassEvent.value.Convert();

                var rgbTex = settings.rgbTex.value == null ? Texture2D.whiteTexture : settings.rgbTex.value;
                var scanlineTex = settings.scanlineTex.value == null ? Texture2D.whiteTexture : settings.scanlineTex.value;
                var trackingTex = settings.trackingTexture.value == null ? Texture2D.grayTexture : settings.trackingTexture.value;

                // Set CRT effect properties.
                material.SetColor("_BackgroundColor", settings.backgroundColor.value);
                material.SetFloat("_DistortionStrength", settings.distortionStrength.value);
                material.SetFloat("_DistortionSmoothing", settings.distortionSmoothing.value);
                material.SetTexture("_RGBTex", rgbTex);
                material.SetFloat("_RGBStrength", settings.rgbStrength.value);
                material.SetTexture("_ScanlineTex", scanlineTex);
                material.SetFloat("_ScanlineStrength", settings.scanlineStrength.value);
                material.SetFloat("_ScrollSpeed", settings.scrollSpeed.value);
                material.SetFloat("_RandomWear", settings.randomWear.value);
                material.SetFloat("_AberrationStrength", settings.aberrationStrength.value);
                material.SetTexture("_TrackingTex", trackingTex);
                material.SetFloat("_TrackingSize", settings.trackingSize.value);
                material.SetFloat("_TrackingStrength", settings.trackingStrength.value);
                material.SetFloat("_TrackingSpeed", settings.trackingSpeed.value);
                material.SetFloat("_TrackingJitter", settings.trackingJitter.value);
                material.SetFloat("_TrackingColorDamage", settings.trackingColorDamage.value);
                material.SetFloat("_TrackingLinesThreshold", settings.trackingLinesThreshold.value);
                material.SetColor("_TrackingLinesColor", settings.trackingLinesColor.value);
                material.SetFloat("_Brightness", settings.brightness.value);
                material.SetFloat("_Contrast", settings.contrast.value);
                material.SetInteger("_Interlacing", frameCounter++ % 2);
                material.SetTexture("_InputTexture", interlacingTexture);

                if (settings.scaleParameters.value)
                {
                    float modifier = (float)settings.verticalReferenceResolution.value / targetHeight;
                    material.SetInt("_Size", (int)(settings.scanlineSize.value / modifier));
                }
                else
                {
                    material.SetInt("_Size", settings.scanlineSize.value);
                }

                if (settings.enableInterlacing.value && frameCounter > 1)
                {
                    material.EnableKeyword("_INTERLACING_ON");
                }
                else
                {
                    material.DisableKeyword("_INTERLACING_ON");
                }

                if (settings.forcePointFiltering.value)
                {
                    material.EnableKeyword("_POINT_FILTERING_ON");
                }
                else
                {
                    material.DisableKeyword("_POINT_FILTERING_ON");
                }

                if (settings.aberrationStrength.value > 0.01f)
                {
                    material.EnableKeyword("_CHROMATIC_ABERRATION_ON");
                }
                else
                {
                    material.DisableKeyword("_CHROMATIC_ABERRATION_ON");
                }

                if (settings.trackingTexture.value == null ||
                    (settings.trackingStrength.value < 0.001f && settings.trackingColorDamage.value < 0.001f &&
                    settings.trackingLinesThreshold.value > 0.999f))
                {
                    material.DisableKeyword("_TRACKING_ON");
                }
                else
                {
                    material.EnableKeyword("_TRACKING_ON");
                }
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null)
                {
                    CreateMaterial();
                }

                var settings = VolumeManager.instance.stack.GetComponent<CRTSettings>();

                if (renderingData.cameraData.isSceneViewCamera && !settings.showInSceneView.value)
                {
                    return;
                }

                if (renderingData.cameraData.isPreviewCamera)
                {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get();

                RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

                SetMaterialProperties(interlaceTexHandle, cameraTargetHandle.rt.height, material);

                using (new ProfilingScope(cmd, profilingSampler))
                {
                    // Perform the Blit operations for the CRT effect.
                    using (new ProfilingScope(cmd, profilingSampler))
                    {
                        Blitter.BlitCameraTexture(cmd, cameraTargetHandle, tempTexHandle, bilinear: !settings.forcePointFiltering.value);
                        Blitter.BlitCameraTexture(cmd, tempTexHandle, cameraTargetHandle, material, 0);

                        if (settings.enableInterlacing.value)
                        {
                            Blitter.BlitCameraTexture(cmd, cameraTargetHandle, interlaceTexHandle, bilinear: !settings.forcePointFiltering.value);
                        }
                    }
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                tempTexHandle?.Release();
            }

#if UNITY_6000_0_OR_NEWER

            private class CopyPassData
            {
                public TextureHandle inputTexture;
                public bool useBilinear;
            }

            private class MainPassData
            {
                public Material material;
                public TextureHandle inputTexture;
                public TextureHandle interlacingTexture;
                public int targetHeight;
            }

            private class InterlacePassData
            {
                public TextureHandle inputTexture;
                public bool useBilinear;
            }

            private void ExecuteCopyPass(RasterCommandBuffer cmd, RTHandle source, bool useBilinear)
            {
                Blitter.BlitTexture(cmd, source, new Vector4(1, 1, 0, 0), 0.0f, useBilinear);
            }

            private void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle source, RTHandle interlacingTexture, int targetHeight, Material material)
            {
                SetMaterialProperties(interlacingTexture, targetHeight, material);
                Blitter.BlitTexture(cmd, source, new Vector4(1, 1, 0, 0), material, 0);
            }

            private void ExecuteInterlacePass(RasterCommandBuffer cmd, RTHandle source, bool useBilinear)
            {
                Blitter.BlitTexture(cmd, source, new Vector4(1, 1, 0, 0), 0.0f, useBilinear);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
               if (material == null)
                {
                    CreateMaterial();
                }

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                var settings = VolumeManager.instance.stack.GetComponent<CRTSettings>();

                if (cameraData.isSceneViewCamera && !settings.showInSceneView.value)
                {
                    return;
                }

                if (cameraData.isPreviewCamera)
                {
                    return;
                }

                UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;
                var colorCopyDescriptor = GetCopyPassDescriptor(cameraData.cameraTargetDescriptor);
                var interlacingDescriptor = GetInterlaceDescriptor(cameraData.cameraTargetDescriptor);
                TextureHandle copiedColor = TextureHandle.nullHandle;
                TextureHandle interlacingTexture = TextureHandle.nullHandle;

                if(interlaceTexHandle != null)
                {
                    interlacingTexture = renderGraph.ImportTexture(interlaceTexHandle);
                }

                // Perform the intermediate copy pass (source -> temp).
                copiedColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorCopyDescriptor, "_CRTColorCopy", false);

                using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("CRT_CopyColor", out var passData, profilingSampler))
                {
                    passData.inputTexture = resourceData.activeColorTexture;
                    passData.useBilinear = !settings.forcePointFiltering.value;

                    builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(copiedColor, 0, AccessFlags.Write);
                    builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) => ExecuteCopyPass(context.cmd, data.inputTexture, data.useBilinear));
                }

                // Perform main pass (temp -> source).
                using (var builder = renderGraph.AddRasterRenderPass<MainPassData>("CRT_MainPass", out var passData, profilingSampler))
                {
                    passData.material = material;
                    passData.inputTexture = copiedColor;
                    passData.interlacingTexture = interlacingTexture;
                    passData.targetHeight = cameraData.cameraTargetDescriptor.height;

                    builder.UseTexture(copiedColor, AccessFlags.Read);
                    if(interlacingTexture.IsValid())
                    {
                        builder.UseTexture(interlacingTexture, AccessFlags.Read);
                    }
                    
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                    builder.SetRenderFunc((MainPassData data, RasterGraphContext context) => ExecuteMainPass(context.cmd, data.inputTexture, data.interlacingTexture, data.targetHeight, data.material));
                }

                if(settings.enableInterlacing.value && interlacingTexture.IsValid())
                {
                    using (var builder = renderGraph.AddRasterRenderPass<InterlacePassData>("CRT_CopyInterlacingTexture", out var passData, profilingSampler))
                    {
                        passData.inputTexture = resourceData.activeColorTexture;
                        passData.useBilinear = !settings.forcePointFiltering.value;

                        builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                        builder.SetRenderAttachment(interlacingTexture, 0, AccessFlags.Write);
                        builder.SetRenderFunc((InterlacePassData data, RasterGraphContext context) => ExecuteInterlacePass(context.cmd, data.inputTexture, data.useBilinear));
                    }
                }
            }
#endif
        }
    }
}

