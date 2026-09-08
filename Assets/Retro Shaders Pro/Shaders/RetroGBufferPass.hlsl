#ifndef RETRO_GBUFFER_PASS_INCLUDED
#define RETRO_GBUFFER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

struct appdata
{
	float4 positionOS : POSITION;
	float4 color : COLOR;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	float2 staticLightmapUV : TEXCOORD1;
#ifdef DYNAMICLIGHTMAP_ON
	float2 dynamicLightmapUV : TEXCOORD2;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 positionCS : SV_POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD0;
	float3 affineUV : TEXCOORD1;

	DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 2);
	float3 normalWS : TEXCOORD3;
	float3 positionWS : TEXCOORD4;

#ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	float4 shadowCoord : TEXCOORD5;
#endif

#ifdef DYNAMICLIGHTMAP_ON
	float2 dynamicLightmapUV : TEXCOORD6;
#endif

	float4 positionSS : TEXCOORD8;

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

FragmentOutput GBuffer(InputData inputData, SurfaceData surfaceData)
{
	FragmentOutput o;

	uint materialFlags = 0;
	materialFlags |= kMaterialFlagSpecularHighlightsOff;
	float materialFlagsPacked = PackMaterialFlags(materialFlags);

	float3 normalWS = PackNormal(inputData.normalWS);

	Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
	MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
	float3 giEmission = (inputData.bakedGI * surfaceData.albedo) + surfaceData.emission;

	o.GBuffer0.rgb = surfaceData.albedo;
	o.GBuffer0.a = materialFlagsPacked;

	o.GBuffer1.rgb = surfaceData.specular;
	o.GBuffer1.a = 0.0f;

	o.GBuffer2.rgb = normalWS;
	o.GBuffer2.a = surfaceData.smoothness;

	o.GBuffer3.rgb = giEmission; // GI + Emission.
	o.GBuffer3.a = 1.0f;

#if OUTPUT_SHADOWMASK
	o.GBUFFER_SHADOWMASK = inputData.shadowMask;
#endif

	return o;
}

v2f gBufferVert(appdata v)
{
	v2f o = (v2f)0;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float4 positionVS = mul(UNITY_MATRIX_MV, v.positionOS);
	positionVS = floor(positionVS * _SnapsPerUnit) / _SnapsPerUnit;
	o.positionCS = mul(UNITY_MATRIX_P, positionVS);

	o.positionWS = mul(UNITY_MATRIX_M, v.positionOS);
	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
	o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
    o.affineUV = float3(TRANSFORM_TEX(v.uv, _BaseMap) * o.positionCS.w, o.positionCS.w);

	OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, o.staticLightmapUV);
	OUTPUT_SH(o.normalWS, o.vertexSH);

#ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
#endif

#ifdef DYNAMICLIGHTMAP_ON
	o.dynamicLightmapUV = v.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

    o.positionSS = ComputeScreenPos(o.positionCS);
	o.color = v.color;

	return o;
}

FragmentOutput gBufferFrag(v2f i, float facing : VFACE)
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	int targetResolution = (int)log2(_ResolutionLimit);
	int actualResolution = (int)log2(_BaseMap_TexelSize.zw);
	int lod = actualResolution - targetResolution;
	
    float2 uv = lerp(i.uv, i.affineUV.xy / i.affineUV.z, _AffineTextureStrength);

#if _USE_POINT_FILTER_ON
	float4 baseColor = _BaseColor * SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_PointRepeat, uv, lod) * i.color;
#else
	float4 baseColor = _BaseColor * SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_LinearRepeat, uv, lod) * i.color;
#endif

	Alpha(baseColor.a, _BaseColor, _Cutoff);
	
	// Posterize the base color.
    float colorBitDepth = max(2, _ColorBitDepth);

    float r = max((baseColor.r - EPSILON) * colorBitDepth, 0.0f);
    float g = max((baseColor.g - EPSILON) * colorBitDepth, 0.0f);
    float b = max((baseColor.b - EPSILON) * colorBitDepth, 0.0f);

    float divisor = colorBitDepth - 1.0f;

#ifdef _USE_DITHERING
	float3 remainders = float3(frac(r), frac(g), frac(b));
	float3 ditheredColor = saturate(dither(remainders, uv * _BaseMap_TexelSize.zw));
	ditheredColor = step(0.5f, ditheredColor);
#else
    float3 ditheredColor = 0.0f;
#endif

    float3 posterizedColor = float3(floor(r), floor(g), floor(b)) + ditheredColor;
    posterizedColor /= divisor;
    posterizedColor += 1.0f / colorBitDepth * _ColorBitDepthOffset;

#ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	float4 shadowCoord = i.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
#else
	float4 shadowCoord = 0;
#endif

	InputData inputData = (InputData)0;
	inputData.positionCS = i.positionCS;
	inputData.positionWS = i.positionWS;
    inputData.normalWS = i.normalWS * facing;
	inputData.viewDirectionWS = normalize(GetWorldSpaceViewDir(i.positionWS));
	inputData.shadowCoord = shadowCoord;
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
	inputData.shadowMask = SAMPLE_SHADOWMASK(i.staticLightmapUV);

#ifdef DYNAMICLIGHTMAP_ON
	inputData.bakedGI = SAMPLE_GI(i.staticLightmapUV, i.dynamicLightmapUV, i.vertexSH, normalWS);
#else
	inputData.bakedGI = SAMPLE_GI(i.staticLightmapUV, i.vertexSH, i.normalWS);
#endif

	SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = posterizedColor.rgb;
	surfaceData.alpha = baseColor.a;
	surfaceData.emission = 0.0f;
	surfaceData.metallic = 0.0f;
	surfaceData.occlusion = 1.0f;
	surfaceData.smoothness = 0.0f;
	surfaceData.specular = 0.0f;
	//surfaceData.normalTS = normalTS;
	
	FragmentOutput output = GBuffer(inputData, surfaceData);
	return output;
}

#endif // RETRO_GBUFFER_PASS_INCLUDED
