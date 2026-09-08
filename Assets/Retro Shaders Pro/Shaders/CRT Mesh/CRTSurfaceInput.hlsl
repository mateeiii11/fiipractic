#ifndef CRT_INPUT_SURFACE_INCLUDED
#define CRT_INPUT_SURFACE_INCLUDED

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_RGBTex);
TEXTURE2D(_ScanlineTex);
TEXTURE2D(_TrackingTex);

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseMap_TexelSize;
    half4 _BaseColor;
    float4 _BackgroundColor;
    float _DistortionStrength;
    float _DistortionSmoothing;
    int _PixelSize;
    int _RGBPixelSize;
    float _RGBStrength;
    float _ScanlineStrength;
    float _RandomWear;
    float _ScrollSpeed;
    float _AberrationStrength;
    float _TrackingSize;
    float _TrackingStrength;
    float _TrackingSpeed;
    float _TrackingJitter;
    float _TrackingColorDamage;
    float _TrackingLinesThreshold;
    float4 _TrackingLinesColor;
    float _Brightness;
    float _Contrast;
    half _Cutoff;
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED

UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

#define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4 , _BaseColor)
#define _Cutoff             UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float  , _Cutoff)

#endif

///////////////////////////////////////////////////////////////////////////////
//                      Material Property Helpers                            //
///////////////////////////////////////////////////////////////////////////////
half Alpha(half albedoAlpha, half4 color, half cutoff)
{
#if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
    half alpha = albedoAlpha * color.a;
#else
	half alpha = color.a;
#endif

    alpha = AlphaDiscard(alpha, cutoff);

    return alpha;
}

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM( albedoAlphaMap, sampler_albedoAlphaMap))
{
	return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
}

#endif // CRT_INPUT_SURFACE_INCLUDED
