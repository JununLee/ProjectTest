// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///-----------------------------------------------------------------
/// Shader:             RayIntersect
/// Description:        Renders intersection of a plane and a 
///							VolumeComponent.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
///-----------------------------------------------------------------

Shader "Hidden/VolumeViewer/RayIntersect"
{
	SubShader
	{
		Tags{ "VolumeTag" = "Volume" }
		Pass
		{
			ZWrite Off
			ZTest Always
			Cull Front

			CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag

				#pragma exclude_renderers d3d9

				#pragma multi_compile _ VOLUMEVIEWER_RGBA_DATA VOLUMEVIEWER_TF_DATA VOLUMEVIEWER_TF_DATA_2D
				#pragma multi_compile __ VOLUMEVIEWER_RGBA_OVERLAY VOLUMEVIEWER_TF_OVERLAY

				#define EPSILON_FLOAT	0.0001
				#define EPSILON_HALF	0.001

				struct u2v {
					float4 	pos : POSITION;
					half2 	uv 	: TEXCOORD0;
				};

				struct v2f {
					float4 	psPos : SV_POSITION;
					float3	osPos : TEXCOORD0;
				};

				uniform float3 planeNormal;
				uniform float planeOffset;
				uniform float focalLength;
				uniform float nearClipPlane;

				sampler3D data3D;
				uniform half4 dataChannelWeight;
				sampler2D rayOffset2D;
#if (defined(VOLUMEVIEWER_TF_DATA) || defined(VOLUMEVIEWER_TF_DATA_2D))
				sampler2D tfData2D;
				uniform half tfDataBlendMode;
#endif
#if (defined(VOLUMEVIEWER_TF_OVERLAY) || defined(VOLUMEVIEWER_RGBA_OVERLAY))
				sampler3D overlay3D;
				uniform half4 overlayChannelWeight;
				uniform half overlayBlendMode;
	#ifdef VOLUMEVIEWER_TF_OVERLAY
				sampler2D tfOverlay2D;
				uniform half tfOverlayBlendMode;
	#endif
#endif
				uniform fixed hideZeros;
				uniform half maxSamples;
				uniform half maxSlices;
				uniform half contrast;
				uniform half brightness;
				uniform half opacity;
				uniform half valueRangeMin;
				uniform half valueRangeMax;
				uniform half cutValueRangeMin;
				uniform half cutValueRangeMax;
#ifdef VOLUMEVIEWER_TF_DATA_2D
				uniform half gradientRangeMin;
				uniform half gradientRangeMax;
				uniform half cutGradientRangeMin;
				uniform half cutGradientRangeMax;
#endif

				inline half luminance(half3 rgb)
				{
					//return dot(rgb, half3(0.2126, 0.7152, 0.0722));
					return max(max(rgb.r, rgb.g), rgb.b);
				}

				fixed cubeIntersection(float3 origin, float3 direction, float3 aabbMax, float3 aabbMin, out float tNear, out float tFar)
				{
					float3 invDir = 1.0 / direction;
					float3 t1 = invDir * (aabbMax - origin);
					float3 t2 = invDir * (aabbMin - origin);
					float3 tMin = min(t1, t2);
					float3 tMax = max(t1, t2);
					tNear = max(max(tMin.x, tMin.y), tMin.z);
					tFar = min(min(tMax.x, tMax.y), tMax.z);
					return tNear <= tFar;
				}

#ifdef VOLUMEVIEWER_TF_DATA_2D
				half getGradientMagnitude(half c0, float3 pos)
				{
					half epsilon = 1.0 / maxSlices;
					half4 tc1 = tex3Dlod(data3D, float4(pos.x + epsilon, pos.y, pos.z, 0)) * dataChannelWeight;
					half4 tc2 = tex3Dlod(data3D, float4(pos.x, pos.y + epsilon, pos.z, 0)) * dataChannelWeight;
					half4 tc3 = tex3Dlod(data3D, float4(pos.x, pos.y, pos.z + epsilon, 0)) * dataChannelWeight;
					half3 g0;
					g0.x = c0 - (tc1.r + tc1.a);
					g0.y = c0 - (tc2.r + tc2.a);
					g0.z = c0 - (tc3.r + tc3.a);
					return length(g0);
				}
#endif

				half4 getColor(float3 pos)
				{
					half4 data = tex3Dlod(data3D, float4(pos.xyz, 0)) * dataChannelWeight;
#ifdef VOLUMEVIEWER_RGBA_DATA
					half intensity = luminance(data.rgb);
#else
					half intensity = data.r + data.a;
					data.rgb = intensity;
					data.a = 1.0;
#endif
					half intensityScaled = (intensity - valueRangeMin) / (valueRangeMax - valueRangeMin);
					intensityScaled *= (intensityScaled >= cutValueRangeMin && intensityScaled <= cutValueRangeMax);
					intensityScaled = saturate(intensityScaled);
					data.rgb *= intensityScaled / max(EPSILON_HALF, intensity);
#if (defined(VOLUMEVIEWER_TF_DATA) || defined(VOLUMEVIEWER_TF_DATA_2D))
					half4 dataUV = half4(intensityScaled, 0, 0, 0);
	#ifdef VOLUMEVIEWER_TF_DATA_2D
					half gradLength = getGradientMagnitude(intensity, pos);
					half gradLengthScaled = (gradLength - gradientRangeMin) / (gradientRangeMax - gradientRangeMin);
					gradLengthScaled *= (gradLength >= cutGradientRangeMin && gradLength <= cutGradientRangeMax);
					gradLengthScaled = saturate(gradLengthScaled);
					dataUV.g = gradLengthScaled;
	#endif
					if (tfDataBlendMode == 1)
					{
						data = tex2Dlod(tfData2D, dataUV);
					}
					else if (tfDataBlendMode == 2)
					{
						data *= tex2Dlod(tfData2D, dataUV);
					}
					else if (tfDataBlendMode == 3)
					{
						data += tex2Dlod(tfData2D, dataUV);
					}
#endif
#if (defined(VOLUMEVIEWER_TF_OVERLAY) || defined(VOLUMEVIEWER_RGBA_OVERLAY))
					half4 overlay = tex3Dlod(overlay3D, float4(pos.xyz, 0)) * overlayChannelWeight;
	#ifdef VOLUMEVIEWER_TF_OVERLAY
					half overlayIntensity = overlay.r + overlay.a;
					overlay.rgb = overlayIntensity;
					overlay.a = 1.0;

					half4 overlayUV = half4(overlayIntensity, 0, 0, 0);
					if (tfOverlayBlendMode == 1)
					{
						overlay = tex2Dlod(tfOverlay2D, overlayUV);
					}
					else if (tfOverlayBlendMode == 2)
					{
						overlay *= tex2Dlod(tfOverlay2D, overlayUV);
					}
					else if (tfOverlayBlendMode == 3)
					{
						overlay += tex2Dlod(tfOverlay2D, overlayUV);
					}
					overlayIntensity = luminance(overlay.rgb)*overlay.a;
	#else
					half overlayIntensity = luminance(overlay.rgb)*overlay.a;
	#endif	
					if (overlayBlendMode == 1)
					{
						data = overlay;
					}
					else if (overlayBlendMode == 2)
					{
						data *= overlay;
					}
					else if (overlayBlendMode == 3)
					{
						data = (overlayIntensity > 0)*overlay + (overlayIntensity <= 0)*data;
					}
#endif
					return saturate(data);
				}

				v2f vert(u2v v)
				{
					v2f o;
					o.psPos = UnityObjectToClipPos(v.pos);
					o.osPos = v.pos.xyz;
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					float3 rayDirection = planeNormal;
					float3 rayOrigin = i.osPos - 2 * planeNormal;
					float tNear, tFar;
					cubeIntersection(rayOrigin, rayDirection, float3(0.5, 0.5, 0.5), float3(-0.5, -0.5, -0.5), tNear, tFar);
					tNear *= tNear >= 0;
					float3 startRay = rayOrigin + rayDirection * tNear;
					float3 endRay = rayOrigin + rayDirection * tFar;

					startRay = startRay + 0.5;
					endRay = endRay + 0.5;
					rayDirection = endRay - startRay;

					float3 planeOrigin = float3(0.5, 0.5, 0.5) + planeOffset * planeNormal;
					float rayLength = dot(-planeNormal, startRay - planeOrigin) / dot(planeNormal, rayDirection);
					float3 pos = startRay + rayLength * rayDirection;

					half4 src = getColor(pos);
					src.rgb = contrast*(src.rgb - 0.5) + 0.5 + brightness;
					src.rgb *= (pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x <= 1 && pos.y <= 1 && pos.z <= 1);
					src.a = 1.0;

					return src;
				}
			ENDCG
		}
	}
	FallBack Off
}