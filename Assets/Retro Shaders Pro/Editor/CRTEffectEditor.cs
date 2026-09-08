using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine;

namespace RetroShadersPro.URP
{
#if UNITY_2022_2_OR_NEWER
    [CustomEditor(typeof(CRTSettings))]
#else
    [VolumeComponentEditor(typeof(CRTSettings))]
#endif
    public class CRTEffectEditor : VolumeComponentEditor
    {
        SerializedDataParameter showInSceneView;
        const string showInSceneViewLabel = "Show in Scene View";
        const string showInSceneViewTooltip = "Should the effect be visible in the Scene View?";

        SerializedDataParameter enabled;
        const string enabledLabel = "Enabled";
        const string enabledTooltip = "Should the effect be rendered?";

        SerializedDataParameter renderPassEvent;
        const string renderPassEventLabel = "Render Pass Event";
        const string renderPassEventTooltip = "Choose where to insert this pass in URP's render loop.\n" +
            "\nURP's internal post processing includes effects like bloom and color-correction, which may impact the appearance of the CRT effect.\n" +
            "\nFor example, with the Before setting, high-intensity HDR colors will be impacted by Bloom.";

        SerializedDataParameter distortionStrength;
        const string distortionStrengthLabel = "Distortion Strength";
        const string distortionStrengthTooltip = "Strength of the barrel distortion. Values above zero cause CRT screen-like distortion; values below zero bulge outwards";

        SerializedDataParameter distortionSmoothing;
        const string distortionSmoothingLabel = "Distortion Smoothing";
        const string distortionSmoothingTooltip = "Amount of smoothing applied to edges of the distorted screen.";

        SerializedDataParameter backgroundColor;
        const string backgroundColorLabel = "Background Color";
        const string backgroundColorTooltip = "Color of the area outside of the barrel-distorted 'screen'.";

        SerializedDataParameter scaleParameters;
        const string scaleParametersLabel = "Scale in Screen Space";
        const string scaleParametersTooltip = "Enable if you want pixelation, scanline, and RGB effects to scale seamlessly with screen size.";

        SerializedDataParameter verticalReferenceResolution;
        const string verticalReferenceResolutionLabel = "Reference Resolution (Vertical)";
        const string verticalReferenceResolutionTooltip = "Base vertical resolution to use as a reference point for scaling properties." +
            "\nIf the real screen resolution matches the reference, then no scaling is performed.";

        SerializedDataParameter forcePointFiltering;
        const string forcePointFilteringLabel = "Force Point Filtering";
        const string forcePointFilteringTooltip = "Should the effect use point filtering when rescaling?";

        SerializedDataParameter rgbTex;
        const string rgbTexLabel = "RGB Subpixel Texture";
        const string rgbTexTooltip = "Small texture denoting the shape of the red, green, and blue subpixels." +
            "\nFor best results, try and make sure the Pixel Size matches the dimensions of this texture.";

        SerializedDataParameter rgbStrength;
        const string rgbStrengthLabel = "RGB Subpixel Strength";
        const string rgbSubpixelTooltip = "How strongly the screen colors get multiplied with the subpixel texture.";

        SerializedDataParameter scanlineTex;
        const string scanlineTexLabel = "Scanline Texture";
        const string scanlineTexTooltip = "Small texture denoting the scanline pattern which scrolls over the screen.";

        SerializedDataParameter scanlineStrength;
        const string scanlineStrengthLabel = "Scanline Strength";
        const string scanlineStrengthTooltip = "How strongly the scanline texture is overlaid onto the screen.";

        SerializedDataParameter scanlineSize;
        const string scanlineSizeLabel = "Scanline/RGB Size";
        const string scanlineSizeTooltip = "The scanline and RGB textures cover this number of pixels." +
            "\nFor best results, this should be a multiple of the Pixel Size.";

        SerializedDataParameter scrollSpeed;
        const string scrollSpeedLabel = "Scanline Scroll Speed";
        const string scrollSpeedTooltip = "How quickly the scanlines scroll vertically over the screen.";

        SerializedDataParameter pixelSize;
        const string pixelSizeLabel = "Pixel Size";
        const string pixelSizeTooltip = "Size of each 'pixel' on the new image, after rescaling the source camera texture.";

        SerializedDataParameter randomWear;
        const string randomWearLabel = "Random Wear";
        const string randomWearTooltip = "How strongly each texture line is offset horizontally.";

        SerializedDataParameter aberrationStrength;
        const string aberrationStrengthLabel = "Aberration Strength";
        const string aberrationStrengthTooltip = "Amount of color channel separation at the screen edges.";

        SerializedDataParameter trackingTexture;
        const string trackingTextureLabel = "Tracking Texture";
        const string trackingTextureTooltip = "A control texture for VHS tracking artifacts." + 
            "\nThe red channel of the texture contains the strength of the UV offsets." + 
            "\nThe green channel of the texture contains tracking line strength." +
            "\nStrength values are centered around 0.5 (gray), and get stronger the closer you get to 0 or 1.";

        SerializedDataParameter trackingSize;
        const string trackingSizeLabel = "Tracking Size";
        const string trackingSizeTooltip = "How many times the tracking texture is tiled on-screen.";

        SerializedDataParameter trackingStrength;
        const string trackingStrengthLabel = "Tracking Strength";
        const string trackingStrengthTooltip = "How strongly the tracking texture offsets screen UVs.";

        SerializedDataParameter trackingSpeed;
        const string trackingSpeedLabel = "Tracking Speed";
        const string trackingSpeedTooltip = "How quickly the tracking texture scrolls across the screen." +
            "\nUse negative values to scroll upwards instead.";

        SerializedDataParameter trackingJitter;
        const string trackingJitterLabel = "Tracking Jitter";
        const string trackingJitterTooltip = "How jittery the scrolling movement is.";

        SerializedDataParameter trackingColorDamage;
        const string trackingColorDamageLabel = "Tracking Color Damage";
        const string trackingColorDamageTooltip = "How strongly the chrominance of the image is distorted." + 
            "\nThe distortion is applied in YIQ color space, to the I and Q channels (chrominance)." + 
            "\nA value of 1 distorts colors back to the original chrominance.";

        SerializedDataParameter trackingLinesThreshold;
        const string trackingLinesThresholdLabel = "Tracking Lines Threshold";
        const string trackingLinesThresholdTooltip = "Higher threshold values mean fewer pixels are registered as tracking lines.";

        SerializedDataParameter trackingLinesColor;
        const string trackingLinesColorLabel = "Tracking Lines Color";
        const string trackingLinesColorTooltip = "Color of the tracking lines. The alpha component acts as a global multiplier on strength.";

        SerializedDataParameter brightness;
        const string brightnessLabel = "Brightness";
        const string brightnessTooltip = "Global brightness adjustment control. 1 represents no change." +
            "\nThis setting can be increased if other features darken your image too much.";

        SerializedDataParameter contrast;
        const string contrastLabel = "Contrast";
        const string contrastTooltip = "Global contrast modifier. 1 represents no change.";

        SerializedDataParameter enableInterlacing;
        const string interlacingLabel = "True Interlacing";
        const string interlacingTooltip = "Should Unity render half of lines this frame, and the other half the next frame?";

        private static GUIStyle _headerStyle;
        private static GUIStyle headerStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(GUI.skin.label)
                    {
                        wordWrap = true,
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                }

                return _headerStyle;
            }
        }

        public override void OnEnable()
        {
            var o = new PropertyFetcher<CRTSettings>(serializedObject);
            showInSceneView = Unpack(o.Find(x => x.showInSceneView));
            enabled = Unpack(o.Find(x => x.enabled));
            renderPassEvent = Unpack(o.Find(x => x.renderPassEvent));
            distortionStrength = Unpack(o.Find(x => x.distortionStrength));
            backgroundColor = Unpack(o.Find(x => x.backgroundColor));
            scaleParameters = Unpack(o.Find(x => x.scaleParameters));
            verticalReferenceResolution = Unpack(o.Find(x => x.verticalReferenceResolution));
            forcePointFiltering = Unpack(o.Find(x => x.forcePointFiltering));
            rgbTex = Unpack(o.Find(x => x.rgbTex));
            rgbStrength = Unpack(o.Find(x => x.rgbStrength));
            scanlineTex = Unpack(o.Find(x => x.scanlineTex));
            scanlineStrength = Unpack(o.Find(x => x.scanlineStrength));
            scanlineSize = Unpack(o.Find(x => x.scanlineSize));
            scrollSpeed = Unpack(o.Find(x => x.scrollSpeed));
            pixelSize = Unpack(o.Find(x => x.pixelSize));
            randomWear = Unpack(o.Find(x => x.randomWear));
            aberrationStrength = Unpack(o.Find(x => x.aberrationStrength));
            distortionSmoothing = Unpack(o.Find(x => x.distortionSmoothing));
            trackingTexture = Unpack(o.Find(x => x.trackingTexture));
            trackingSize = Unpack(o.Find(x => x.trackingSize));
            trackingStrength = Unpack(o.Find(x => x.trackingStrength));
            trackingSpeed = Unpack(o.Find(x => x.trackingSpeed));
            trackingJitter = Unpack(o.Find(x => x.trackingJitter));
            trackingColorDamage = Unpack(o.Find(x => x.trackingColorDamage));
            trackingLinesThreshold = Unpack(o.Find(x => x.trackingLinesThreshold));
            trackingLinesColor = Unpack(o.Find(x => x.trackingLinesColor));
            brightness = Unpack(o.Find(x => x.brightness));
            contrast = Unpack(o.Find(x => x.contrast));
            enableInterlacing = Unpack(o.Find(x => x.enableInterlacing));
        }

        public override void OnInspectorGUI()
        {
            if (!RetroShaderUtility.CheckEffectEnabled<CRTEffect>())
            {
                EditorGUILayout.HelpBox("The CRT effect must be added to your renderer's Renderer Features list.", MessageType.Error);
                if (GUILayout.Button("Add CRT Renderer Feature"))
                {
                    RetroShaderUtility.AddEffectToPipelineAsset<CRTEffect>();
                }
            }

            EditorGUILayout.LabelField("Basic Settings", headerStyle);

            PropertyField(showInSceneView, new GUIContent(showInSceneViewLabel, showInSceneViewTooltip));
            PropertyField(enabled, new GUIContent(enabledLabel, enabledTooltip));
            PropertyField(renderPassEvent, new GUIContent(renderPassEventLabel, renderPassEventTooltip));

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Resolution & Fidelity", headerStyle);

            PropertyField(pixelSize, new GUIContent(pixelSizeLabel, pixelSizeTooltip));
            PropertyField(scaleParameters, new GUIContent(scaleParametersLabel, scaleParametersTooltip));
            PropertyField(verticalReferenceResolution, new GUIContent(verticalReferenceResolutionLabel, verticalReferenceResolutionTooltip));
            PropertyField(forcePointFiltering, new GUIContent(forcePointFilteringLabel, forcePointFilteringTooltip));
            PropertyField(enableInterlacing, new GUIContent(interlacingLabel, interlacingTooltip));

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Barrel Distortion", headerStyle);

            PropertyField(distortionStrength, new GUIContent(distortionStrengthLabel, distortionStrengthTooltip));
            PropertyField(distortionSmoothing, new GUIContent(distortionSmoothingLabel, distortionSmoothingTooltip));
            PropertyField(backgroundColor, new GUIContent(backgroundColorLabel, backgroundColorTooltip));

            GUILayout.Space(8);

            EditorGUILayout.LabelField("RGB Subpixels & Scanlines", headerStyle);

            PropertyField(rgbTex, new GUIContent(rgbTexLabel, rgbTexTooltip));
            PropertyField(rgbStrength, new GUIContent(rgbStrengthLabel, rgbSubpixelTooltip));

            PropertyField(scanlineTex, new GUIContent(scanlineTexLabel, scanlineTexTooltip));
            PropertyField(scanlineStrength, new GUIContent(scanlineStrengthLabel, scanlineStrengthTooltip));
            PropertyField(scanlineSize, new GUIContent(scanlineSizeLabel, scanlineSizeTooltip));
            PropertyField(scrollSpeed, new GUIContent(scrollSpeedLabel, scrollSpeedTooltip));

            GUILayout.Space(8);

            EditorGUILayout.LabelField("VHS Artifacts", headerStyle);

            PropertyField(randomWear, new GUIContent(randomWearLabel, randomWearTooltip));
            PropertyField(aberrationStrength, new GUIContent(aberrationStrengthLabel, aberrationStrengthTooltip));
            PropertyField(trackingTexture, new GUIContent(trackingTextureLabel, trackingTextureTooltip));
            PropertyField(trackingSize, new GUIContent(trackingSizeLabel, trackingSizeTooltip));
            PropertyField(trackingStrength, new GUIContent(trackingStrengthLabel, trackingStrengthTooltip));
            PropertyField(trackingSpeed, new GUIContent(trackingSpeedLabel, trackingSpeedTooltip));
            PropertyField(trackingJitter, new GUIContent(trackingJitterLabel, trackingJitterTooltip));
            PropertyField(trackingColorDamage, new GUIContent(trackingColorDamageLabel, trackingColorDamageTooltip));
            PropertyField(trackingLinesThreshold, new GUIContent(trackingLinesThresholdLabel, trackingLinesThresholdTooltip));
            PropertyField(trackingLinesColor, new GUIContent(trackingLinesColorLabel, trackingLinesColorTooltip));

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Color Adjustments", headerStyle);

            PropertyField(brightness, new GUIContent(brightnessLabel, brightnessTooltip));
            PropertyField(contrast, new GUIContent(contrastLabel, contrastTooltip));
        }
    }
}
