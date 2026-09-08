using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;

namespace RetroShadersPro.URP
{
    internal class RetroLitShaderGUI : ShaderGUI
    {
        MaterialProperty baseColorProp = null;
        const string baseColorName = "_BaseColor";
        const string baseColorLabel = "Base Color";
        const string baseColorTooltip = "Albedo color of the object.";

        MaterialProperty baseTexProp = null;
        const string baseTexName = "_BaseMap";
        const string baseTexLabel = "Base Texture";
        const string baseTexTooltip = "Albedo texture of the object.";

        MaterialProperty resolutionLimitProp = null;
        const string resolutionLimitName = "_ResolutionLimit";
        const string resolutionLimitLabel = "Resolution Limit";
        const string resolutionLimitTooltip = "Limits the resolution of the texture to this value." + 
            "\nNote that this setting only snaps the resolution to powers of two." +
            "\nAlso, make sure the Base Texture has mipmaps enabled.";

        MaterialProperty snapsPerUnitProp = null;
        const string snapsPerUnitName = "_SnapsPerUnit";
        const string snapsPerUnitLabel = "Snaps Per Meter";
        const string snapsPerUnitTooltip = "The mesh vertices snap to a limited number of points in space." +
            "\nThis uses clip space, so the mesh may jitter when the camera rotates.";

        MaterialProperty colorBitDepthProp = null;
        const string colorBitDepthName = "_ColorBitDepth";
        const string colorBitDepthLabel = "Color Depth";
        const string colorBitDepthTooltip = "Limits the total number of values used for each color channel.";

        MaterialProperty colorBitDepthOffsetProp = null;
        const string colorBitDepthOffsetName = "_ColorBitDepthOffset";
        const string colorBitDepthOffsetLabel = "Color Depth Offset";
        const string colorBitDepthOffsetTooltip = "Increase this value if the bit depth offset makes your object too dark.";

        MaterialProperty ambientLightProp = null;
        const string ambientLightName = "_AmbientLight";
        const string ambientLightLabel = "Ambient Light Strength";
        const string ambientLightTooltip = "When the ambient light override is used, apply this much ambient light.";

        MaterialProperty affineTextureStrengthProp = null;
        const string affineTextureStrengthName = "_AffineTextureStrength";
        const string affineTextureStrengthLabel = "Affine Texture Strength";
        const string affineTextureStrengthTooltip = "How strongly the affine texture mapping effect is applied." + 
            "\nWhen this is set to 1, the shader uses affine texture mapping exactly like the PS1." +
            "\nWhen this is set to 0, the shader uses perspective-correct texture mapping, like modern systems.";

        MaterialProperty ambientToggleProp = null;
        const string ambientToggleName = "_USE_AMBIENT_OVERRIDE";
        const string ambientToggleLabel = "Ambient Light Override";
        const string ambientToggleTooltip = "Should the object use Unity's default ambient light, or a custom override amount?";

        MaterialProperty usePointFilteringProp = null;
        const string usePointFilteringName = "_USE_POINT_FILTER";
        const string usePointFilteringLabel = "Point Filtering";
        const string usePointFilteringTooltip = "Should the shader use point filtering?";

        MaterialProperty useDitheringProp = null;
        const string useDitheringName = "_USE_DITHERING";
        const string useDitheringLabel = "Enable Dithering";
        const string useDitheringTooltip = "Should the shader use color dithering?";

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
            resolutionLimitProp = FindProperty(resolutionLimitName, props, true);
            snapsPerUnitProp = FindProperty(snapsPerUnitName, props, true);
            colorBitDepthProp = FindProperty(colorBitDepthName, props, true);
            colorBitDepthOffsetProp = FindProperty(colorBitDepthOffsetName, props, true);
            ambientLightProp = FindProperty(ambientLightName, props, false);
            affineTextureStrengthProp = FindProperty(affineTextureStrengthName, props, true);
            ambientToggleProp = FindProperty(ambientToggleName, props, false);
            usePointFilteringProp = FindProperty(usePointFilteringName, props, false);
            useDitheringProp = FindProperty(useDitheringName, props, true);

            //surfaceTypeProp = FindProperty(kSurfaceTypeProp, props, false);
            cullProp = FindProperty(cullName, props, true);
            alphaClipProp = FindProperty(alphaClipName, props, true);
            alphaClipThresholdProp = FindProperty(alphaClipThresholdName, props, true);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor == null)
            {
                throw new ArgumentNullException("No MaterialEditor found (RetroLitShaderGUI).");
            }

            Material material = materialEditor.target as Material;
            this.materialEditor = materialEditor;

            FindProperties(properties);

            if (firstTimeOpen)
            {
                materialScopeList.RegisterHeaderScope(new GUIContent("Surface Options"), 1u << 0, DrawSurfaceOptions);
                materialScopeList.RegisterHeaderScope(new GUIContent("Retro Properties"), 1u << 1, DrawRetroProperties);
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

        private void DrawRetroProperties(Material material)
        {
            materialEditor.ShaderProperty(baseColorProp, baseColorLabel);
            materialEditor.ShaderProperty(baseTexProp, baseTexLabel);
            materialEditor.ShaderProperty(resolutionLimitProp, resolutionLimitLabel);
            materialEditor.ShaderProperty(snapsPerUnitProp, snapsPerUnitLabel);
            materialEditor.ShaderProperty(colorBitDepthProp, colorBitDepthLabel);
            materialEditor.ShaderProperty(colorBitDepthOffsetProp, colorBitDepthOffsetLabel);
            materialEditor.ShaderProperty(affineTextureStrengthProp, new GUIContent(affineTextureStrengthLabel, affineTextureStrengthTooltip));
            
            //materialEditor.ShaderProperty(useDitheringProp, new GUIContent(useDitheringLabel, useDitheringTooltip));

            if (ambientLightProp != null)
            {
                materialEditor.ShaderProperty(ambientToggleProp, ambientToggleLabel);

                bool ambient = material.GetFloat(ambientToggleName) >= 0.5f;

                if (ambient)
                {
                    material.EnableKeyword(ambientToggleName);

                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(ambientLightProp, ambientLightLabel);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    material.DisableKeyword(ambientToggleName);
                }
            }

            if (usePointFilteringProp != null)
            {
                materialEditor.ShaderProperty(usePointFilteringProp, usePointFilteringLabel);
            }

            if (useDitheringProp != null)
            {
                materialEditor.ShaderProperty(useDitheringProp, new GUIContent(useDitheringLabel, useDitheringTooltip));

                bool dither = material.GetFloat(useDitheringName) >= 0.5f;

                if (dither)
                {
                    material.EnableKeyword(useDitheringName);
                }
                else
                {
                    material.DisableKeyword(useDitheringName);
                }
            }
        }
    }
}
