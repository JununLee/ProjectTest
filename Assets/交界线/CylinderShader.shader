Shader "Custom/CylinderShader"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+1"}
		LOD 100
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
				float3 viewPos : TEXCOORD1;
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.viewPos = UnityObjectToViewPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				if (-i.viewPos.z<5) {
					discard;
				}
				return float4(1,0,0,1);
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
				float3 viewPos : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.viewPos = UnityObjectToViewPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				if(-i.viewPos.z<5){
					discard;
				}
				return float4(1,0,0,1);
			}
			ENDCG
		}
	}
}

