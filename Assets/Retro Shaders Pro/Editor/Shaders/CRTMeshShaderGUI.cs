using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;

namespace RetroShadersPro.URP
{
    internal class CRTMeshShaderGUI : ShaderGUI
    {
        MaterialProperty baseColorProp = null;
        const string baseColorName = "_BaseColor";
        const string baseColorLabel = "Base Color";
        const string baseColorTooltip = "Albedo color of the object.";

        MaterialProperty baseTexProp = null;
        const string baseTexName = "_BaseMap";
        const string baseTexLabel = "Base Texture";
        const string baseTexTooltip = "Albedo texture of the object.";

        MaterialProperty distortionStrengthProp = null;
        const string distortionStrengthName = "_DistortionStrength";
        const string distortionStrengthLabel = "Distortion Strength";
        const string distortionStrengthTooltip = "Strength of the barrel distortion. Values above zero cause CRT screen-like distortion; values below zero bulge outwards";

        MaterialProperty distortionSmoothingProp = null;
        const string distortionSmoothingName = "_DistortionSmoothing";
        const string distortionSmoothingLabel = "Distortion Smoothing";
        const string distortionSmoothingTooltip = "Amount of smoothing applied to edges of the distorted screen.";

        MaterialProperty backgroundColorProp = null;
        const string backgroundColorName = "_BackgroundColor";
        const string backgroundColorLabel = "Background Color";
        const string backgroundColorTooltip = "Color of the area outside of the barrel-distorted 'screen'.";

        MaterialProperty pixelSizeProp = null;
        const string pixelSizeName = "_PixelSize";
        const string pixelSizeLabel = "Pixel Size";
        const string pixelSizeTooltip = "Size of each 'pixel' on the new image, after rescaling the source camera texture.";

        MaterialProperty forcePointFilteringProp = null;
        const string forcePointFilteringName = "_POINT_FILTERING";
        const string forcePointFilteringLabel = "Force Point Filtering";
        const string forcePointFilteringTooltip = "Should the effect use point filtering when rescaling?";

        MaterialProperty rgbTexProp = null;
        const string rgbTexName = "_RGBTex";
        const string rgbTexLabel = "RGB Subpixel Texture";
        const string rgbTexTooltip = "Small texture denoting the shape of the red, green, and blue subpixels." +
            "\nFor best results, try and make sure the Pixel Size matches the dimensions of this texture.";

        MaterialProperty rgbStrengthProp = null;
        const string rgbStrengthName = "_RGBStrength";
        const string rgbStrengthLabel = "RGB Subpixel Strength";
        const string rgbStrengthTooltip = "How strongly the screen colors get multiplied with the subpixel texture.";

        MaterialProperty scanlineTexProp = null;
        const string scanlineTexName = "_ScanlineTex";
        const string scanlineTexLabel = "Scanline Texture";
        const string scanlineTexTooltip = "Small texture denoting the scanline pattern which scrolls over the screen.";

        MaterialProperty scanlineStrengthProp = null;
        const string scanlineStrengthName = "_ScanlineStrength";
        const string scanlineStrengthLabel = "Scanline Strength";
        const string scanlineStrengthTooltip = "How strongly the scanline texture is overlaid onto the screen.";

        MaterialProperty scanlineSizeProp = null;
        const string scanlineSizeName = "_RGBPixelSize";
        const string scanlineSizeLabel = "Scanline/RGB Size";
        const string scanlineSizeTooltip = "The scanline and RGB textures cover this number of pixels." +
            "\nFor best results, this should be a multiple of the Pixel Size.";

        MaterialProperty scrollSpeedProp = null;
        const string scrollSpeedName = "_ScrollSpeed";
        const string scrollSpeedLabel = "Scanline Scroll Speed";
        const string scrollSpeedTooltip = "How quickly the scanlines scroll vertically over the screen.";

        MaterialProperty randomWearProp = null;
        const string randomWearName = "_RandomWear";
        const string randomWearLabel = "Random Wear";
        const string randomWearTooltip = "How strongly each texture line is offset horizontally.";

        MaterialProperty aberrationStrengthProp = null;
        const string aberrationStrengthName = "_AberrationStrength";
        const string aberrationStrengthLabel = "Aberration Strength";
        const string aberrationStrengthTooltip = "Amount of color channel separation at the screen edges.";

        const string useAberrationName = "_CHROMATIC_ABERRATION_ON";

        MaterialProperty trackingTextureProp = null;
        const string trackingTextureName = "_TrackingTex";
        const string trackingTextureLabel = "Tracking Texture";
        const string trackingTextureTooltip = "A control texture for VHS tracking artifacts." +
            "\nThe red channel of the texture contains the strength of the UV offsets." +
            "\nThe green channel of the texture contains tracking line strength." +
            "\nStrength values are centered around 0.5 (gray), and get stronger the closer you get to 0 or 1.";

        MaterialProperty trackingSizeProp = null;
        const string trackingSizeName = "_TrackingSize";
        const string trackingSizeLabel = "Tracking Size";
        const string trackingSizeTooltip = "How many times the tracking texture is tiled on-screen.";

        MaterialProperty trackingStrengthProp = null;
        const string trackingStrengthName = "_TrackingStrength";
        const string trackingStrengthLabel = "Tracking Strength";
        const string trackingStrengthTooltip = "How strongly the tracking texture offsets screen UVs.";

        MaterialProperty trackingSpeedProp = null;
        const string trackingSpeedName = "_TrackingSpeed";
        const string trackingSpeedLabel = "Tracking Speed";
        const string trackingSpeedTooltip = "How quickly the tracking texture scrolls across the screen." +
            "\nUse negative values to scroll upwards instead.";

        MaterialProperty trackingJitterProp = null;
        const string trackingJitterName = "_TrackingJitter";
        const string trackingJitterLabel = "Tracking Jitter";
        const string trackingJitterTooltip = "How jittery the scrolling movement is.";

        MaterialProperty trackingColorDamageProp = null;
        const string trackingColorDamageName = "_TrackingColorDamage";
        const string trackingColorDamageLabel = "Tracking Color Damage";
        const string trackingColorDamageTooltip = "How strongly the chrominance of the image is distorted." +
            "\nThe distortion is applied in YIQ color space, to the I and Q channels (chrominance)." +
            "\nA value of 1 distorts colors back to the original chrominance.";

        MaterialProperty trackingLinesThresholdProp = null;
        const string trackingLinesThresholdName = "_TrackingLinesThreshold";
        const string trackingLinesThresholdLabel = "Tracking Lines Threshold";
        const string trackingLinesThresholdTooltip = "Higher threshold values mean fewer pixels are registered as tracking lines.";

        MaterialProperty trackingLinesColorProp = null;
        const string trackingLinesColorName = "_TrackingLinesColor";
        const string trackingLinesColorLabel = "Tracking Lines Color";
        const string trackingLinesColorTooltip = "Color of the tracking lines. The alpha component acts as a global multiplier on strength.";

        MaterialProperty brightnessProp = null;
        const string brightnessName = "_Brightness";
        const string brightnessLabel = "Brightness";
        const string brightnessTooltip = "Global brightness adjustment control. 1 represents no change." +
            "\nThis setting can be increased if other features darken your image too much.";

        MaterialProperty contrastProp = null;
        const string contrastName = "_Contrast";
        const string contrastLabel = "Contrast";
        const string contrastTooltip = "Global contrast modifier. 1 represents no change.";

        MaterialProperty alphaClipProp = null;
        const string alphaClipName = "_AlphaClip";
        const string alphaClipLabel = "Alpha Clip";
        const string alphaClipTooltip = "Should the shader clip pixels based on alpha using a threshold value?";

        MaterialProperty alphaClipThresholdProp = null;
        const string alphaClipThresholdName = "_Cutoff";
        const string alphaClipThresholdLabel = "Threshold";
        const string alphaClipThresholdTooltip = "The threshold value to use for alpha clipping.";

        private MaterialProperty cullProp;
        private const string cullName = "_Cull";
        private const string cullLabel = "Render Face";
        private const string cullTooltip = "Should Unity render Front, Back, or Both faces of the mesh?";

        private const string surfaceTypeName = "_Surface";
        private const string surfaceTypeLabel = "Surface Type";
        private const string surfaceTypeTooltip = "Should the object be transparent or opaque?";

        private const string alphaTestName = "_ALPHATEST_ON";

        private static readonly string[] surfaceTypeNames = Enum.GetNames(typeof(SurfaceType));
        private static readonly string[] renderFaceNames = Enum.GetNames(typeof(RenderFace));

        private enum SurfaceType
        {
            Opaque = 0,
            Transparent = 1
        }

        private enum RenderFace
        {
            Front = 2,
            Back = 1,
            Both = 0
        }

        private SurfaceType surfaceType = SurfaceType.Opaque;
        private RenderFace renderFace = RenderFace.Front;

        protected readonly MaterialHeaderScopeList materialScopeList = new MaterialHeaderScopeList(uint.MaxValue);
        protected MaterialEditor materialEditor;
        private bool firstTimeOpen = true;

        private void FindProperties(MaterialProperty[] props)
        {
            baseColorProp = FindProperty(baseColorName, props, true);
            baseTexProp = FindProperty(baseTexName, props, true);
            distortionStrengthProp = FindProperty(distortionStrengthName, props, true);
            distortionSmoothingProp = FindProperty(distortionSmoothingName, props, true);
            backgroundColorProp = FindProperty(backgroundColorName, props, true);
            pixelSizeProp = FindProperty(pixelSizeName, props, true);
            forcePointFilteringProp = FindProperty(forcePointFilteringName, props, true);
            rgbTexProp = FindProperty(rgbTexName, props, true);
            rgbStrengthProp = FindProperty(rgbStrengthName, props, true);
            scanlineTexProp = FindProperty(scanlineTexName, props, true);
            scanlineStrengthProp = FindProperty(scanlineStrengthName, props, true);
            scanlineSizeProp = FindProperty(scanlineSizeName, props, true);
            scrollSpeedProp = FindProperty(scrollSpeedName, props, true);
            randomWearProp = FindProperty(randomWearName, props, true);
            aberrationStrengthProp = FindProperty(aberrationStrengthName, props, true);
            trackingTextureProp = FindProperty(trackingTextureName, props, true);
            trackingSizeProp = FindProperty(trackingSizeName, props, true);
            trackingStrengthProp = FindProperty(trackingStrengthName, props, true);
            trackingSpeedProp = FindProperty(trackingSpeedName, props, true);
            trackingJitterProp = FindProperty(trackingJitterName, props, true);
            trackingColorDamageProp = FindProperty(trackingColorDamageName, props, true);
            trackingLinesThresholdProp = FindProperty(trackingLinesThresholdName, props, true);
            trackingLinesColorProp = FindProperty(trackingLinesColorName, props, true);
            brightnessProp = FindProperty(brightnessName, props, true);
            contrastProp = FindProperty(contrastName, props, true);

            //surfaceTypeProp = FindProperty(kSurfaceTypeProp, props, false);
            cullProp = FindProperty(cullName, props, true);
            alphaClipProp = FindProperty(alphaClipName, props, true);
            alphaClipThresholdProp = FindProperty(alphaClipThresholdName, props, true);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor == null)
            {
                throw new ArgumentNullException("No MaterialEditor found (CRTMeshShaderGUI).");
            }

            Material material = materialEditor.target as Material;
            this.materialEditor = materialEditor;

            FindProperties(properties);

            if (firstTimeOpen)
            {
                materialScopeList.RegisterHeaderScope(new GUIContent("Surface Options"), 1u << 0, DrawSurfaceOptions);
                materialScopeList.RegisterHeaderScope(new GUIContent("Basic Properties"), 1u << 1, DrawBasicProperties);
                materialScopeList.RegisterHeaderScope(new GUIContent("Barrel Distortion"), 1u << 2, DrawDistortionProperties);
                materialScopeList.RegisterHeaderScope(new GUIContent("RGB Subpixels & Scanlines"), 1u << 3, DrawRGBScanlineProperties);
                materialScopeList.RegisterHeaderScope(new GUIContent("VHS Artifacts"), 1u << 4, DrawVHSProperties);
                materialScopeList.RegisterHeaderScope(new GUIContent("Color Adjustments"), 1u << 5, DrawColorProperties);
                firstTimeOpen = false;
            }

            materialScopeList.DrawHeaders(materialEditor, material);
            materialEditor.serializedObject.ApplyModifiedProperties();
        }

        private void DrawSurfaceOptions(Material material)
        {
            surfaceType = (SurfaceType)material.GetFloat(surfaceTypeName);
            renderFace = (RenderFace)material.GetFloat(cullName);

            // Display opaque/transparent options.
            bool surfaceTypeChanged = false;
            EditorGUI.BeginChangeCheck();
            {
                surfaceType = (SurfaceType)EditorGUILayout.EnumPopup(new GUIContent(surfaceTypeLabel, surfaceTypeTooltip), surfaceType);
            }
            if (EditorGUI.EndChangeCheck())
            {
                surfaceTypeChanged = true;
            }

            // Display culling options.
            EditorGUI.BeginChangeCheck();
            {
                renderFace = (RenderFace)EditorGUILayout.EnumPopup(cullLabel, renderFace);
            }
            if (EditorGUI.EndChangeCheck())
            {
                switch (renderFace)
                {
                    case RenderFace.Both:
                        {
                            material.SetFloat(cullName, 0);
                            break;
                        }
                    case RenderFace.Back:
                        {
                            material.SetFloat(cullName, 1);
                            break;
                        }
                    case RenderFace.Front:
                        {
                            material.SetFloat(cullName, 2);
                            break;
                        }
                }
            }

            // Display alpha clip options.
            EditorGUI.BeginChangeCheck();
            {
                materialEditor.ShaderProperty(alphaClipProp, alphaClipLabel);
            }
            if (EditorGUI.EndChangeCheck())
            {
                surfaceTypeChanged = true;
            }

            bool alphaClip;

            if (surfaceTypeChanged)
            {
                switch (surfaceType)
                {
                    case SurfaceType.Opaque:
                        {
                            material.SetOverrideTag("RenderType", "Opaque");
                            material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                            material.SetFloat("_ZWrite", 1);
                            material.SetFloat(surfaceTypeName, 0);

                            alphaClip = material.GetFloat(alphaClipName) >= 0.5f;
                            if (alphaClip)
                            {
                                material.EnableKeyword(alphaTestName);
                                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                                material.SetOverrideTag("RenderType", "TransparentCutout");
                            }
                            else
                            {
                                material.DisableKeyword(alphaTestName);
                                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                                material.SetOverrideTag("RenderType", "Opaque");
                            }


                            break;
                        }
                    case SurfaceType.Transparent:
                        {
                            alphaClip = material.GetFloat(alphaClipName) >= 0.5f;
                            if (alphaClip)
                            {
                                material.EnableKeyword(alphaTestName);
                            }
                            else
                            {
                                material.DisableKeyword(alphaTestName);
                            }
                            material.SetOverrideTag("RenderType", "Transparent");
                            material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetFloat("_ZWrite", 0);
                            material.SetFloat(surfaceTypeName, 1);

                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                            break;
                        }
                }
            }

            alphaClip = material.GetFloat(alphaClipName) >= 0.5f;
            if (alphaClip)
            {
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(alphaClipThresholdProp, alphaClipThresholdLabel);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawBasicProperties(Material material)
        {
            materialEditor.ShaderProperty(baseColorProp, new GUIContent(baseColorLabel, baseColorTooltip));
            materialEditor.ShaderProperty(baseTexProp, new GUIContent(baseTexLabel, baseTexTooltip));

            materialEditor.ShaderProperty(pixelSizeProp, new GUIContent(pixelSizeLabel, pixelSizeTooltip));

            if (forcePointFilteringProp != null)
            {
                materialEditor.ShaderProperty(forcePointFilteringProp, forcePointFilteringLabel);
            }
        }

        private void DrawDistortionProperties(Material material)
        {
            materialEditor.ShaderProperty(distortionStrengthProp, new GUIContent(distortionStrengthLabel, distortionStrengthTooltip));
            materialEditor.ShaderProperty(distortionSmoothingProp, new GUIContent(distortionSmoothingLabel, distortionSmoothingTooltip));
            materialEditor.ShaderProperty(backgroundColorProp, new GUIContent(backgroundColorLabel, backgroundColorTooltip));
        }

        private void DrawRGBScanlineProperties(Material material)
        {
            materialEditor.ShaderProperty(rgbTexProp, new GUIContent(rgbTexLabel, rgbTexTooltip));
            materialEditor.ShaderProperty(rgbStrengthProp, new GUIContent(rgbStrengthLabel, rgbStrengthTooltip));
            materialEditor.ShaderProperty(scanlineTexProp, new GUIContent(scanlineTexLabel, scanlineTexTooltip));
            materialEditor.ShaderProperty(scanlineStrengthProp, new GUIContent(scanlineStrengthLabel, scanlineStrengthTooltip));
            materialEditor.ShaderProperty(scanlineSizeProp, new GUIContent(scanlineSizeLabel, scanlineSizeTooltip));
            materialEditor.ShaderProperty(scrollSpeedProp, new GUIContent(scrollSpeedLabel, scrollSpeedTooltip));
        }

        private void DrawVHSProperties(Material material)
        {
            materialEditor.ShaderProperty(randomWearProp, new GUIContent(randomWearLabel, randomWearTooltip));

            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(aberrationStrengthProp, new GUIContent(aberrationStrengthLabel, aberrationStrengthTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                float aberrationStrength = material.GetFloat(aberrationStrengthName);

                if(aberrationStrength > 0.01f)
                {
                    material.EnableKeyword(useAberrationName);
                }
                else
                {
                    material.DisableKeyword(useAberrationName);
                }
            }

            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(trackingTextureProp, new GUIContent(trackingTextureLabel, trackingTextureTooltip));
            materialEditor.ShaderProperty(trackingSizeProp, new GUIContent(trackingSizeLabel, trackingSizeTooltip));
            materialEditor.ShaderProperty(trackingStrengthProp, new GUIContent(trackingStrengthLabel, trackingStrengthTooltip));
            materialEditor.ShaderProperty(trackingSpeedProp, new GUIContent(trackingSpeedLabel, trackingSpeedTooltip));
            materialEditor.ShaderProperty(trackingJitterProp, new GUIContent(trackingJitterLabel, trackingJitterTooltip));
            materialEditor.ShaderProperty(trackingColorDamageProp, new GUIContent(trackingColorDamageLabel, trackingColorDamageTooltip));
            materialEditor.ShaderProperty(trackingLinesThresholdProp, new GUIContent(trackingLinesThresholdLabel, trackingLinesThresholdTooltip));
            materialEditor.ShaderProperty(trackingLinesColorProp, new GUIContent(trackingLinesColorLabel, trackingLinesColorTooltip));
            if (EditorGUI.EndChangeCheck())
            {
                Texture trackingTexture = material.GetTexture(trackingTextureName);
                float trackingStrength = material.GetFloat(trackingStrengthName);
                float trackingColorDamage = material.GetFloat(trackingColorDamageName);
                float trackingLinesThreshold = material.GetFloat(trackingLinesThresholdName);

                if (trackingTexture == null ||
                    (trackingStrength < 0.001f && trackingColorDamage < 0.001f &&
                    trackingLinesThreshold > 0.999f))
                {
                    material.DisableKeyword("_TRACKING_ON");
                }
                else
                {
                    material.EnableKeyword("_TRACKING_ON");
                }
            }
        }
        
        private void DrawColorProperties(Material material)
        {
            materialEditor.ShaderProperty(brightnessProp, new GUIContent(brightnessLabel, brightnessTooltip));
            materialEditor.ShaderProperty(contrastProp, new GUIContent(contrastLabel, contrastTooltip));
        }
    }
}
