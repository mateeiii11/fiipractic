using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RetroShadersPro.URP
{
    [System.Serializable, VolumeComponentMenu("Retro Shaders Pro/CRT")]
    public class CRTSettings : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter showInSceneView = new BoolParameter(true);
        public BoolParameter enabled = new BoolParameter(false);
        public RenderPassEventParameter renderPassEvent = new RenderPassEventParameter(PostProcessRenderPassEvent.AfterURPPostProcessing);
        public ClampedFloatParameter distortionStrength = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
        public ClampedFloatParameter distortionSmoothing = new ClampedFloatParameter(0.01f, 0.0f, 0.1f);
        public ColorParameter backgroundColor = new ColorParameter(Color.black);
        public BoolParameter scaleParameters = new BoolParameter(false);
        public IntParameter verticalReferenceResolution = new IntParameter(1080);
        public BoolParameter forcePointFiltering = new BoolParameter(false);
        public TextureParameter rgbTex = new TextureParameter(null);
        public ClampedFloatParameter rgbStrength = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
        public TextureParameter scanlineTex = new TextureParameter(null);
        public ClampedFloatParameter scanlineStrength = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
        public ClampedIntParameter scanlineSize = new ClampedIntParameter(8, 1, 64);
        public ClampedFloatParameter scrollSpeed = new ClampedFloatParameter(0.0f, 0.0f, 10.0f);
        public ClampedIntParameter pixelSize = new ClampedIntParameter(1, 1, 256);
        public ClampedFloatParameter randomWear = new ClampedFloatParameter(0.2f, 0.0f, 5.0f);
        public ClampedFloatParameter aberrationStrength = new ClampedFloatParameter(0.5f, 0.0f, 10.0f);

        public TextureParameter trackingTexture = new TextureParameter(null);
        public ClampedFloatParameter trackingSize = new ClampedFloatParameter(1.0f, 0.1f, 2.0f);
        public ClampedFloatParameter trackingStrength = new ClampedFloatParameter(0.1f, 0.0f, 50.0f);
        public ClampedFloatParameter trackingSpeed = new ClampedFloatParameter(0.1f, -2.5f, 2.5f);
        public ClampedFloatParameter trackingJitter = new ClampedFloatParameter(0.01f, 0.0f, 0.1f);
        public ClampedFloatParameter trackingColorDamage = new ClampedFloatParameter(0.05f, 0.0f, 1.0f);
        public ClampedFloatParameter trackingLinesThreshold = new ClampedFloatParameter(0.9f, 0.0f, 1.0f);
        public ColorParameter trackingLinesColor = new ColorParameter(new Color(1.0f, 1.0f, 1.0f, 0.5f));

        public ClampedFloatParameter brightness = new ClampedFloatParameter(1.0f, 0.0f, 3.0f);
        public ClampedFloatParameter contrast = new ClampedFloatParameter(1.0f, 0.0f, 3.0f);
        public BoolParameter enableInterlacing = new BoolParameter(false);

        public bool IsActive()
        {
            return enabled.value && active;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }

    public enum PostProcessRenderPassEvent
    {
        BeforeURPPostProcessing,
        AfterURPPostProcessing
    }

    // Allow each volume settings object to track the render pass event.
    [Serializable]
    public sealed class RenderPassEventParameter : VolumeParameter<PostProcessRenderPassEvent>
    {
        public RenderPassEventParameter(PostProcessRenderPassEvent value, bool overrideState = false) : base(value, overrideState) { }
    }

    public static class ParameterTypeExtensions
    {
        public static RenderPassEvent Convert(this PostProcessRenderPassEvent renderPassEvent)
        {
            if (renderPassEvent == PostProcessRenderPassEvent.BeforeURPPostProcessing)
            {
                return RenderPassEvent.BeforeRenderingPostProcessing;
            }

            return RenderPassEvent.AfterRenderingPostProcessing;
        }
    }
}

