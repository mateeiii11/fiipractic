Shader "Retro Shaders Pro/Post Processing/CRT"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment frag

			#pragma multi_compile_local_fragment _ _CHROMATIC_ABERRATION_ON
			#pragma multi_compile_local_fragment _ _TRACKING_ON
			#pragma multi_compile_local_fragment _ _INTERLACING_ON
			#pragma multi_compile_local_fragment _ _POINT_FILTERING_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			TEXTURE2D(_InputTexture);
			TEXTURE2D(_RGBTex);
			TEXTURE2D(_ScanlineTex);
			TEXTURE2D(_TrackingTex);

#if UNITY_VERSION < 600000
			float4 _BlitTexture_TexelSize;
#endif
			float4 _BackgroundColor;
			float _DistortionStrength;
			float _DistortionSmoothing;
			int _Size;
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
			int _Interlacing;

			// Code 'liberated' from Shader Graph's Simple Noise node.
			inline float randomValue(float2 uv)
			{
				return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
			}

			float3 rgb2yiq(float3 col)
			{
				static float3x3 yivMatrix = float3x3(
					0.299f,	 0.587f,	 0.114f,
					0.596f,	-0.275f,	-0.321f,
					0.212f,	-0.523f,	 0.311f
					);

				return mul(yivMatrix, col);
			}

			float3 yiq2rgb(float3 col)
			{
				static float3x3 rgbMatrix = float3x3(
					1.000f,	 0.956f,	 0.619f,
					1.000f,	-0.272f,	-0.647f,
					1.000f,	-1.106f,	 1.703f
					);

				return mul(rgbMatrix, col);
			}

            float4 frag (Varyings i) : SV_Target
            {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				// Apply barrel distortion to UVs.
				float2 originalUVs = i.texcoord - 0.5f;
				float2 UVs = originalUVs * (1 + _DistortionStrength * length(originalUVs) * length(originalUVs)) + 0.5f;

				// Save UVs to use for barrel distortion later.
				float2 distortedUVs = UVs;

				// Set up UVs to use for screen-space effects.
				float2 screenUVs = UVs * _ScreenSize.xy / _Size;

				// Get RGB overlay texture.
				float3 rgbCells = SAMPLE_TEXTURE2D(_RGBTex, sampler_LinearRepeat, screenUVs).rgb;

				// Get scanline overlay texture.
				screenUVs.y += _Time.y * _ScrollSpeed;
				float3 scanlines = SAMPLE_TEXTURE2D(_ScanlineTex, sampler_LinearRepeat, screenUVs).rgb;

#ifdef _TRACKING_ON
				float2 trackingUVs = float2(UVs.y * _TrackingSize + _Time.y * _TrackingSpeed + randomValue(_Time.xx) * _TrackingJitter, 0.5f);

				// Get tracking amount.
				float3 trackingSample = (SAMPLE_TEXTURE2D(_TrackingTex, sampler_LinearRepeat, trackingUVs) - 0.5f) * 2.0f;
				float trackingStrength = trackingSample.r;

				// Offset UVs horizontally based on tracking amount.
				float trackingOffset = trackingStrength * _BlitTexture_TexelSize.x * _TrackingStrength;
				UVs.x += trackingOffset;
#endif

				// Offset UVs horizontally based on random tape wear.
				float randomOffset = randomValue(float2(i.texcoord.y, _Time.x));
				UVs.x += randomOffset * _BlitTexture_TexelSize.x * _RandomWear;

				// Sample the blit texture, applying chromatic aberration if enabled.
#ifdef _CHROMATIC_ABERRATION_ON
				float2 redUVs = UVs + originalUVs * _AberrationStrength * _BlitTexture_TexelSize.xy;
				float2 blueUVs = UVs - originalUVs * _AberrationStrength * _BlitTexture_TexelSize.xy;
	#ifdef _POINT_FILTERING_ON
				float red = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, redUVs).r;
				float green = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, UVs).g;
				float blue = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, blueUVs).b;
	#else
				float red = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, redUVs).r;
				float green = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, UVs).g;
				float blue = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, blueUVs).b;
	#endif
				float3 col = float3(red, green, blue);
#else
	#ifdef _POINT_FILTERING_ON
				float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, UVs).rgb;
	#else
				float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, UVs).rgb;
	#endif
#endif

				// Apply brightness and contrast modifiers.
				col = saturate(col * _Brightness);
				col = col - _Contrast * (col - 1.0f) * col * (col - 0.5f);

#ifdef _TRACKING_ON
				// Apply tracking lines.
				float t = _Time.x % 1.0f + 2.307f;
				float x = step(_TrackingLinesThreshold, randomValue(float2((UVs.x + UVs.y * 28.303f) * 0.00005f, t)));
				float y = step(0.7f, randomValue(float2(UVs.y * 236.2144f, t)));

				float trackingLines = abs(trackingSample.g) * saturate(x * y);
				col = lerp(col, _TrackingLinesColor.rgb, trackingLines * _TrackingLinesColor.a);

				// Rotate to new chrominance values in YIV color space.
				float3 yiqCol = rgb2yiq(col);

				float rotationAmount = _TrackingColorDamage * 2.0f * PI * abs(trackingStrength);
				float s = sin(rotationAmount);
				float c = cos(rotationAmount);
				float2x2 rotMatrix = float2x2(c, -s, s, c);
				yiqCol.yz = mul(yiqCol.yz, rotMatrix);

				col = yiq2rgb(yiqCol);
#endif

				// Apply RGB and scanline overlay texture.
				col = lerp(col, col * rgbCells, _RGBStrength);
				col = lerp(col, col * scanlines, _ScanlineStrength);

				// Apply interlacing if enabled.
#ifdef _INTERLACING_ON
				float3 inputCol = SAMPLE_TEXTURE2D(_InputTexture, sampler_LinearClamp, i.texcoord).rgb;
				col = lerp(col, inputCol, floor(UVs.y * _BlitTexture_TexelSize.w + _Interlacing) % 2.0f);
#endif

				UVs = distortedUVs;

				float2 smoothedEdges = smoothstep(0.0f, _DistortionSmoothing, UVs.xy);
				smoothedEdges *= (1.0f - smoothstep(1.0f - _DistortionSmoothing, 1.0f, UVs.xy));

				// Apply border to pixels outside barrel distortion 0-1 range.
				//col = (UVs.x >= 0.0f && UVs.x <= 1.0f && UVs.y >= 0.0f && UVs.y <= 1.0f) ? col : _BackgroundColor;

				col = lerp(_BackgroundColor, col, min(smoothedEdges.x, smoothedEdges.y));

				return float4(col, 1.0f);
            }
            ENDHLSL
        }
    }
}
