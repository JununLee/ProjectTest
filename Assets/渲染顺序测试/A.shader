// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/A"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
		//ZWrite off
		ZTest Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ REDTESST

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainColor;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 worldNor : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.worldNor = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				fixed3 diffuse = _LightColor0.rgb * _MainColor * (dot(i.worldNor,lightDir) / 2 + 0.5);
				float4  col = float4(1,0,0,1);
#ifdef REDTESST
				col = float4(0,1,0,1);
#endif
				//return fixed4(diffuse + ambient,1);
				return col;
			}
			ENDCG
		}
	}
}
