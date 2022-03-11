// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SingleDiffuseShader"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1, 1, 1, 1)
		[HDR]_OutLine("OutLineColor",COLOR) = (1, 1, 1, 1)
		_Scale("Scale",Range(0,1)) = 1 

	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			float4 _MainColor;
			float4 _OutLine;
			float _Scale;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNor : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNor = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 worldNormal = normalize(i.worldNor);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float halfLambert = dot(worldNormal,lightDir) * 0.5 + 0.5;
				float3 diffuse = _MainColor.rgb * halfLambert;
				//float3 outLine = _OutLine.rgb * step(0.8,1 - (saturate(dot(viewDir,worldNormal))))* _Scale;
				float3 outLine = _OutLine.rgb * (1 - (saturate(dot(viewDir,worldNormal))))* _Scale;
				return float4(diffuse + outLine,_MainColor.a);
			}
			ENDCG
		}
	}
}

