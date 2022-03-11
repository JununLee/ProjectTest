Shader "Unlit/ColorMask"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+3"}
		LOD 100
		
		blend SrcAlpha OneMinusSrcAlpha 
		Pass{

			ColorMask 0
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};

			v2f vert(a2v v)
			{
				v2f o;
				float3 dir = mul(unity_WorldToObject,float3(0,0,1));
				
				v.vertex.xyz -= dir;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				if (i.worldPos.z<-1) {
					discard;
				}
				return float4(1,1,1,1);
			}
			ENDCG

		}
		Pass
		{
			Cull front
			ZTest Less
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				float3 dir = mul(unity_WorldToObject,float3(0,0,1));
				
				v.vertex.xyz -= dir;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if(i.worldPos.z<-1){
					discard;
				}
				return float4(0,1,0,0.8);
			}
			ENDCG
		}
	}
}
