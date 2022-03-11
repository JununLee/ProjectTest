// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SSS"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
		_BackDepthTex("_BackDepthTex",2D) = "white"{}
		_AttenuationC("AttenuationC",COLOR) = (1,1,1,1)
		_Shininess("Shininess",float) = 0
		_ScatteringFactor("ScatteringFactor",float) = 0
		_Wrap("Warp",float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+100"}
		LOD 100

		GrabPass{
		
		}
		Pass{
		
			Tags{"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			float4 _MainColor;
			sampler2D _GrabTexture;
			sampler2D _BackDepthTex;
			float4 _AttenuationC;
			float _Shininess;
			float _ScatteringFactor;
			float _Wrap;
			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
				float3 worldpos : TEXCOORD2;
				float4 screenUV : TEXCOORD4;
				float4 grabUV : TEXCOORD5;
			};

			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldpos = mul(unity_ObjectToWorld,v.vertex);
				o.screenUV = ComputeScreenPos(o.vertex);
				o.grabUV = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float frontDepth = LinearEyeDepth(i.screenUV.z/i.screenUV.w);
				float2 backDepthUV = i.screenUV.xy/i.screenUV.w;
				float4 backDepthColor = tex2D(_BackDepthTex,backDepthUV);
				float backDepth =(DecodeFloatRGBA(backDepthColor));
				float depth = backDepthColor.r - frontDepth;
				float3 scattering = exp(-_AttenuationC.xyz * depth);

				float3 lightDir = normalize( UnityWorldSpaceLightDir(i.worldpos));
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldpos));
				float3 worldNormal = normalize(i.normal);

				float3 diffuse = _MainColor.xyz * saturate((dot(worldNormal,lightDir)  + _Wrap) / (1 + _Wrap)) * scattering;
				float3 specular = _LightColor0.rgb * pow(saturate((dot(worldNormal,normalize(lightDir+viewDir)) + _Wrap) / (1 + _Wrap)),_Shininess * 8) * _MainColor.a;
				float3 col = diffuse + specular;
 				col = lerp(tex2D(_GrabTexture, i.grabUV.xy/i.grabUV.w), col, _MainColor.a);
				return fixed4(col,1);
			}
			ENDCG
		}
	}
}

