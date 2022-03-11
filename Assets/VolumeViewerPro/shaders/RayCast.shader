// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///-----------------------------------------------------------------
/// Shader:             RayCast
/// Description:        Volumetric ray-casting for VolumeComponents.
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

Shader "Hidden/VolumeViewer/RayCast"
{
	Properties
	{
		[HideInInspector] rayOffset2D("rayOffset2D", 2D) = "gray" {}
	}
	SubShader
	{
		Tags { "LightMode" = "ForwardBase" "VolumeTag" = "Volume" }
		Pass
		{
			Cull Front

			CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag

				#pragma exclude_renderers d3d9

				#pragma multi_compile _ VOLUMEVIEWER_RGBA_DATA VOLUMEVIEWER_TF_DATA VOLUMEVIEWER_TF_DATA_2D
				#pragma multi_compile __ VOLUMEVIEWER_RGBA_OVERLAY VOLUMEVIEWER_TF_OVERLAY 
				#pragma multi_compile ___ VOLUMEVIEWER_CULLING VOLUMEVIEWER_INVERT_CULLING
				#pragma multi_compile ____ VOLUMEVIEWER_OVERLAY_VOIDS_CULLING
				#pragma multi_compile _____ VOLUMEVIEWER_DEPTH_TEST
				#pragma multi_compile ______ VOLUMEVIEWER_LIGHT VOLUMEVIEWER_TF_LIGHT

				#define EPSILON_FLOAT	0.0001
				#define EPSILON_HALF	0.001
				#define SKIP_INTENSITY	0.01

				//#define VOLUMEVIEWER_GRADIENT32 1
				//#define VOLUMEVIEWER_GRADIENT54 1

				struct u2v {
					float4 	pos : POSITION;
					half2 	uv 	: TEXCOORD0;
				};

				struct v2f {
					float4 	pos 	: SV_POSITION;
					float3	osPos 	: TEXCOORD0;
					half2 	uv		: TEXCOORD1;
				};

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
#ifdef VOLUMEVIEWER_DEPTH_TEST
				sampler2D depth2D;
#endif
#if (defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING))
				sampler2D cullFront2D;
				sampler2D cullBack2D;
#endif
				uniform fixed hideZeros;
				uniform half leap;
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
#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
				uniform half surfaceThr;
				uniform half surfaceAlpha;
				uniform half surfaceGradientFetches;
				uniform float3 ambientColor;
				uniform float3 diffuseColor;
				uniform float3 specularColor;
				uniform float shininess;
				uniform half maxShadedSamples;
				fixed4 _LightColor0;
				//float4 _WorldSpaceLightPos0;
	#ifdef VOLUMEVIEWER_TF_LIGHT
				sampler2D tfLight2D;
	#endif
#endif
#if ((defined(VOLUMEVIEWER_TF_OVERLAY) || defined(VOLUMEVIEWER_RGBA_OVERLAY)) && defined(VOLUMEVIEWER_OVERLAY_VOIDS_CULLING) && (defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING)))
				fixed overlayPresent;
#endif

				inline half luminance(half3 rgb)
				{
					//return dot(rgb, half3(0.2126, 0.7152, 0.0722));
					return max(max(rgb.r, rgb.g), rgb.b);
				}

#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
				float4 shading(float4 color, float3 g, float3 v, float3 l)
				{
					//Might be a problem when length(g) == 0
					float3 n = normalize(g);
					float3 h = normalize(v + l);
					float n_l = dot(n, l);
					float n_h = dot(n, h);
	#ifdef VOLUMEVIEWER_TF_LIGHT
					float4 tfLight = tex2Dlod(tfLight2D, float4(n_l / 2.0 + 0.5, n_h / 2.0 + 0.5, 0, 0));
					float3 ambient = ambientColor * color.rgb * tfLight.r;
					float3 diffuse = diffuseColor * _LightColor0.rgb * tfLight.g * color.rgb;
					float3 specular = specularColor * _LightColor0.rgb * pow(tfLight.b, shininess);
					color.rgb = ambient + diffuse + specular;
					color.a /= max(EPSILON_HALF, min(1, tfLight.a + (1 - length(g))));
	#else
					float3 ambient = ambientColor * color.rgb;
					float3 diffuse = diffuseColor * _LightColor0.rgb * max(n_l, 0) * color.rgb;
					float3 specular = specularColor * _LightColor0.rgb * pow(max(n_h, 0), shininess)*(n_l > 0);
					color.rgb = ambient + diffuse + specular;
	#endif
					return saturate(color);
				}
#endif

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
					//intensityScaled *= (intensityScaled >= cutValueRangeMin && intensityScaled <= cutValueRangeMax);
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
	#if (defined(VOLUMEVIEWER_OVERLAY_VOIDS_CULLING) && (defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING)))
					overlayPresent = overlayIntensity > 0;
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

#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
				float3 getGradient3(float4 tc0, float3 pos)
				{
					float epsilon = 1.0 / maxSlices;
					float4 tc1 = getColor(pos + float3(epsilon, 0, 0));
					float4 tc2 = getColor(pos + float3(0, epsilon, 0));
					float4 tc3 = getColor(pos + float3(0, 0, epsilon));
					float3 g0;
					float c0 = luminance(tc0.rgb)*tc0.a;
					g0.x = c0 - luminance(tc1.rgb)*tc1.a;
					g0.y = c0 - luminance(tc2.rgb)*tc2.a;
					g0.z = c0 - luminance(tc3.rgb)*tc3.a;
					return g0;
				}

				float3 getGradient6(float3 pos)
				{
					float epsilon = 1.0 / maxSlices;
					float4 tc1 = getColor(pos + float3( epsilon, 0, 0));
					float4 tc2 = getColor(pos + float3(-epsilon, 0, 0));
					float4 tc3 = getColor(pos + float3(0,  epsilon, 0));
					float4 tc4 = getColor(pos + float3(0, -epsilon, 0));
					float4 tc5 = getColor(pos + float3(0, 0,  epsilon));
					float4 tc6 = getColor(pos + float3(0, 0, -epsilon));
					float3 g0;
					g0.x = luminance(tc2.rgb)*tc2.a - luminance(tc1.rgb)*tc1.a;
					g0.y = luminance(tc4.rgb)*tc4.a - luminance(tc3.rgb)*tc3.a;
					g0.z = luminance(tc6.rgb)*tc6.a - luminance(tc5.rgb)*tc5.a;
					return g0;
				}

	#ifdef VOLUMEVIEWER_GRADIENT32
				float3 getGradient32(float4 tc0, float3 pos)
				{
					float epsilon = 1.0 / maxSlices;
					float4 tc1 = getColor(pos + float3( epsilon, 0, 0));
					float4 tc2 = getColor(pos + float3(-epsilon, 0, 0));
					float4 tc3 = getColor(pos + float3(0,  epsilon, 0));
					float4 tc4 = getColor(pos + float3(0, -epsilon, 0));
					float4 tc5 = getColor(pos + float3(0, 0,  epsilon));
					float4 tc6 = getColor(pos + float3(0, 0, -epsilon));
					float3 g0; //Middle
					g0.x = luminance(tc2.rgb)*tc2.a - luminance(tc1.rgb)*tc1.a;
					g0.y = luminance(tc4.rgb)*tc4.a - luminance(tc3.rgb)*tc3.a;
					g0.z = luminance(tc6.rgb)*tc6.a - luminance(tc5.rgb)*tc5.a;
					float4 tc7  = getColor(pos + float3( 2 * epsilon,  0,  0));
					float4 tc8  = getColor(pos + float3(epsilon,  epsilon, 0));
					float4 tc9  = getColor(pos + float3(epsilon, -epsilon, 0));
					float4 tc10 = getColor(pos + float3(epsilon, 0,  epsilon));
					float4 tc11 = getColor(pos + float3(epsilon, 0, -epsilon));
					float3 g1; //Right
					g1.x = luminance(tc0.rgb)*tc0.a - luminance(tc7.rgb)*tc7.a;
					g1.y = luminance(tc9.rgb)*tc9.a - luminance(tc8.rgb)*tc8.a;
					g1.z = luminance(tc11.rgb)*tc11.a - luminance(tc10.rgb)*tc10.a;
					float4 tc12 = getColor(pos + float3(-2 * epsilon,  0,   0));
					float4 tc13 = getColor(pos + float3(-epsilon,  epsilon, 0));
					float4 tc14 = getColor(pos + float3(-epsilon, -epsilon, 0));
					float4 tc15 = getColor(pos + float3(-epsilon, 0,  epsilon));
					float4 tc16 = getColor(pos + float3(-epsilon, 0, -epsilon));
					float3 g2; //Left
					g2.x = luminance(tc12.rgb)*tc12.a - luminance(tc0.rgb)*tc0.a;
					g2.y = luminance(tc14.rgb)*tc14.a - luminance(tc13.rgb)*tc13.a;
					g2.z = luminance(tc16.rgb)*tc16.a - luminance(tc15.rgb)*tc15.a;
					float4 tc17 = getColor(pos + float3( epsilon, 0, epsilon));
					float4 tc18 = getColor(pos + float3(-epsilon, 0, epsilon));
					float4 tc19 = getColor(pos + float3(0,  epsilon, epsilon));
					float4 tc20 = getColor(pos + float3(0, -epsilon, epsilon));
					float4 tc21 = getColor(pos + float3(0,  0,   2 * epsilon));
					float3 g3; //Front
					g3.x = luminance(tc18.rgb)*tc18.a - luminance(tc17.rgb)*tc17.a;
					g3.y = luminance(tc20.rgb)*tc20.a - luminance(tc19.rgb)*tc19.a;
					g3.z = luminance(tc0.rgb)*tc0.a - luminance(tc21.rgb)*tc21.a;
					float4 tc22 = getColor(pos + float3( epsilon, 0, -epsilon));
					float4 tc23 = getColor(pos + float3(-epsilon, 0, -epsilon));
					float4 tc24 = getColor(pos + float3(0,  epsilon, -epsilon));
					float4 tc25 = getColor(pos + float3(0, -epsilon, -epsilon));
					float4 tc26 = getColor(pos + float3(0,  0,   -2 * epsilon));
					float3 g4; //Back
					g4.x = luminance(tc22.rgb)*tc22.a - luminance(tc21.rgb)*tc21.a;
					g4.y = luminance(tc24.rgb)*tc24.a - luminance(tc23.rgb)*tc23.a;
					g4.z = luminance(tc26.rgb)*tc26.a - luminance(tc0.rgb)*tc0.a;
					float4 tc27 = getColor(pos + float3(0, 2 * epsilon, 0));
					float4 tc28 = getColor(pos + float3(0, epsilon, epsilon));
					float4 tc29 = getColor(pos + float3(0, epsilon, -epsilon));
					float3 g5; //Top
					g5.x = luminance(tc13.rgb)*tc13.a - luminance(tc8.rgb)*tc8.a;
					g5.y = luminance(tc0.rgb)*tc0.a   - luminance(tc27.rgb)*tc27.a;
					g5.z = luminance(tc29.rgb)*tc29.a - luminance(tc28.rgb)*tc28.a;
					float4 tc30 = getColor(pos + float3(0, -2 * epsilon, 0));
					float4 tc31 = getColor(pos + float3(0, -epsilon,  epsilon));
					float4 tc32 = getColor(pos + float3(0, -epsilon, -epsilon));
					float3 g6; //Bottom
					g6.x = luminance(tc14.rgb)*tc14.a - luminance(tc9.rgb)*tc9.a;
					g6.y = luminance(tc30.rgb)*tc30.a - luminance(tc0.rgb)*tc0.a;
					g6.z = luminance(tc32.rgb)*tc32.a - luminance(tc31.rgb)*tc31.a;
					return 0.25*g0 + 0.125*g1 + 0.125*g2 + 0.125*g3 + 0.125*g4 + 0.125*g5 + 0.125*g6;
				}
	#endif

	#ifdef VOLUMEVIEWER_GRADIENT54
				float3 getGradient54(float3 pos)
				{
					float epsilon = 1.0 / maxSlices;
					float3 g0 = getGradient6(pos);
					float3 g1 = getGradient6(pos + float3(-epsilon,  epsilon,  epsilon));
					float3 g2 = getGradient6(pos + float3(-epsilon, -epsilon,  epsilon));
					float3 g3 = getGradient6(pos + float3( epsilon,  epsilon,  epsilon));
					float3 g4 = getGradient6(pos + float3( epsilon, -epsilon,  epsilon));
					float3 g5 = getGradient6(pos + float3(-epsilon,  epsilon, -epsilon));
					float3 g6 = getGradient6(pos + float3(-epsilon, -epsilon, -epsilon));
					float3 g7 = getGradient6(pos + float3( epsilon,  epsilon, -epsilon));
					float3 g8 = getGradient6(pos + float3( epsilon, -epsilon, -epsilon));
					return g0*0.2 + g1*0.1 + g2*0.1 + g3*0.1 + g4*0.1 + g5*0.1 + g6*0.1 + g7*0.1 + g8*0.1;
				}
	#endif
#endif

				v2f vert(u2v v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.osPos = v.pos.xyz;
					o.uv = v.uv;
					return o;
				}

				half4 frag(v2f i) : COLOR 
				{
					float3 rayOrigin = _WorldSpaceCameraPos.xyz;
					rayOrigin = mul(unity_WorldToObject, float4(rayOrigin, 1)).xyz;
					float3 rayDirection = normalize(i.osPos - rayOrigin);
					rayOrigin += rayDirection * nearClipPlane;
	    			float tNear, tFar;
					cubeIntersection(rayOrigin, rayDirection, float3(0.5, 0.5, 0.5), float3(-0.5, -0.5, -0.5), tNear, tFar);
					tNear *= tNear >= 0;
	    			float3 startRay = rayOrigin + rayDirection * tNear;
	    			float3 endRay = rayOrigin + rayDirection * tFar;
#if (defined(VOLUMEVIEWER_DEPTH_TEST) || defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING))
	    			float startDepth = -mul(UNITY_MATRIX_MV, float4(startRay, 1)).z * _ProjectionParams.w;
	    			float endDepth= -mul(UNITY_MATRIX_MV, float4(endRay, 1)).z * _ProjectionParams.w;
#endif
					float2 uv = i.pos.xy / _ScreenParams.xy;
#ifdef VOLUMEVIEWER_DEPTH_TEST
					float sceneDepth = tex2D(depth2D, uv).r;
					if (startDepth > sceneDepth)
					{
						return 0;
					}
#endif
	    			startRay =  startRay + 0.5;
	    			endRay =  endRay + 0.5;
					float maxRayLength = length(endRay - startRay);
					float rayLengthStep = 1.732 / maxSamples;
					float rayOffset = (tex2D(rayOffset2D, i.uv).r - 0.5) * rayLengthStep;
					float rayLength = rayOffset;
#if (defined(VOLUMEVIEWER_DEPTH_TEST) || defined(VOLUMEVIEWER_CULLING)  || defined(VOLUMEVIEWER_INVERT_CULLING))
					float rayLength2Depth = (endDepth - startDepth) / max(EPSILON_FLOAT, maxRayLength);
					float depth = startDepth + rayLength * rayLength2Depth;
#endif
#if (defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING))
					float cullDepthFront = tex2D(cullFront2D, uv).r;
					float cullDepthBack = tex2D(cullBack2D, uv).r;
					fixed cullingRelay = 0;
#endif
					half4 dst = 0;
					float opacityCorrection = maxSlices / maxSamples;
#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
					fixed shaded = 0;
					float rayLengthFar = 0;
					float intensityFar = 0;
					float rayLengthNear = 0;
					float intensityNear = 0;
					float shadedSamples = 0;
#endif
					half speed = max(1, ceil(maxSamples * leap / maxSlices));
					half skipProtection = 0;
					half samples = 0;
					[loop]
					while(rayLength < maxRayLength)
					{
#if (defined(VOLUMEVIEWER_DEPTH_TEST) || defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING))
						depth = startDepth + rayLength * rayLength2Depth;
#endif
#ifdef VOLUMEVIEWER_DEPTH_TEST
						if (depth > sceneDepth)
						{
							break;
						}
#endif
#if (defined(VOLUMEVIEWER_CULLING) || defined(VOLUMEVIEWER_INVERT_CULLING))
	#if (defined(VOLUMEVIEWER_OVERLAY_VOIDS_CULLING) && (defined(VOLUMEVIEWER_TF_OVERLAY) || defined(VOLUMEVIEWER_RGBA_OVERLAY)))
						half4 src = getColor(startRay + rayLength * rayDirection);
		#ifdef VOLUMEVIEWER_INVERT_CULLING
						src.a *= (depth <= cullDepthBack && depth >= cullDepthFront) || overlayPresent;
		#else
						src.a *= depth > cullDepthBack || depth < cullDepthFront || overlayPresent;
		#endif
						half intensity = luminance(src.rgb)*src.a;
	#else
		#ifdef VOLUMEVIEWER_INVERT_CULLING
						if (depth < cullDepthFront && !cullingRelay)
						{
							rayLength = (cullDepthFront - startDepth) / max(EPSILON_FLOAT, rayLength2Depth);
			#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
							rayLengthNear = rayLength;
			#else
							rayLength += rayOffset;
			#endif
							cullingRelay = 1;
							continue;
						}
						else if (depth > cullDepthBack)
						{
							break;
						}
		#else
						if (depth >= cullDepthFront && depth < cullDepthBack && !cullingRelay)
						{
							rayLength = (cullDepthBack - startDepth) / max(EPSILON_FLOAT, rayLength2Depth);
			#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
							rayLengthNear = rayLength;
			#else
							rayLength += rayOffset;
			#endif
							cullingRelay = 1;
							continue;
						}
		#endif
						half4 src = getColor(startRay + rayLength * rayDirection);
						half intensity = luminance(src.rgb)*src.a;
	#endif
#else
						half4 src = getColor(startRay + rayLength * rayDirection);
						half intensity = luminance(src.rgb)*src.a;
#endif
						if (speed > 1.01 && intensity > SKIP_INTENSITY)
						{
							skipProtection = speed - 1;
							rayLength -= rayLengthStep * skipProtection;
							speed = 1;
							continue;
						}
						else if (speed < 1.01 && intensity <= SKIP_INTENSITY && skipProtection < 0.01)
						{
							speed = max(1, ceil(maxSamples * leap / maxSlices));
						}
						else if(skipProtection > 0.01)
						{
							skipProtection -= 1;
						}
						src.a *= (hideZeros)*(intensity > 0) + (1-hideZeros);
						src.a *= opacity;
						src.rgb = contrast*(src.rgb - 0.5) + 0.5 + brightness;
#if (defined(VOLUMEVIEWER_LIGHT) || defined(VOLUMEVIEWER_TF_LIGHT))
						src.a *= intensity > surfaceThr;
						if (shaded == 0 && shadedSamples < maxShadedSamples && intensity > surfaceThr && intensityNear <= surfaceThr)
						{
							rayLengthFar = rayLength;
							intensityFar = intensity;
							for (half k = 0; k < 4; k += 1)
							{
								rayLength = rayLengthNear + (rayLengthFar - rayLengthNear) * (surfaceThr - intensityNear) / max(EPSILON_FLOAT, abs(intensityFar - intensityNear));
								src = getColor(startRay + rayLength * rayDirection);
								intensity = luminance(src.rgb)*src.a;
								fixed surfaceTest = intensity < surfaceThr;
								rayLengthNear = rayLength * surfaceTest + rayLengthNear * (1 - surfaceTest);
								rayLengthFar = rayLength * (1 - surfaceTest) + rayLengthFar * surfaceTest;
								intensityNear = intensity * surfaceTest + intensityNear * (1 - surfaceTest);
								intensityFar = intensity * (1 - surfaceTest) + intensityFar * surfaceTest;
							}
							rayLength = rayLengthNear + (rayLengthFar - rayLengthNear) * (surfaceThr - intensityNear) / max(EPSILON_FLOAT, abs(intensityFar - intensityNear));
							src = getColor(startRay + rayLength * rayDirection);
							float3 surfaceGradient;
							if (surfaceGradientFetches == 1)
							{
								surfaceGradient = getGradient6(startRay + rayLength * rayDirection);
							}
							else if (surfaceGradientFetches == 2)
							{
#ifdef VOLUMEVIEWER_GRADIENT32
								surfaceGradient = getGradient32(src, startRay + rayLength * rayDirection);
#else
								surfaceGradient = getGradient6(startRay + rayLength * rayDirection);
#endif
							}
							else if (surfaceGradientFetches == 3)
							{
#ifdef VOLUMEVIEWER_GRADIENT54
								surfaceGradient = getGradient54(startRay + rayLength * rayDirection);
#else
								surfaceGradient = getGradient6(startRay + rayLength * rayDirection);
#endif
							}
							else
							{
								surfaceGradient = getGradient3(src, startRay + rayLength * rayDirection);
							}
							//_WorldSpaceLightPos0 is the direction of a directional light
							float3 lightDir = mul(float4(_WorldSpaceLightPos0.xyz, 0), unity_ObjectToWorld).xyz;
							src = shading(src, surfaceGradient, -rayDirection, lightDir);
							src.a = surfaceAlpha;
							shaded = 1;
							shadedSamples += 1;
						}
						else
						{
							shaded = 0;
							src.a = (1.0 - pow((1.0 - src.a), opacityCorrection));
						}
						rayLengthNear = rayLength;
						intensityNear = intensity;
#else
						src.a = (1.0 - pow((1.0 - src.a), opacityCorrection));
#endif
						src.rgb *= src.a;
						dst = (1.0f - dst.a) * src + dst;
						if (dst.a >= 0.997)
						{
							dst.a = 1.0;
							break;
						}
						rayLength += rayLengthStep * speed;
						samples += 1;
						//Failsafe
						if (samples > maxSamples)
						{
							break;
						}
					}
					return dst;
				}
			ENDCG
		}
	}
	FallBack Off
}