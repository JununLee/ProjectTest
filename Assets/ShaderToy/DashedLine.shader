Shader "Custom/DashedLine"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float liner(float2 p1, float2 p2, float2 coord){
				float m = (p2.y - p1.y) / (p2.x - p1.x);
				float k = p1.y - m * p1.x;
				float dis=0;
				if(dot(coord - p1, p2 - p1) >= 0 && dot(coord - p2, p1 - p2) >= 0)
				{
					dis = abs(m * coord.x - coord.y + k)/(sqrt(m * m + 1));
					float len = dot(coord - p1,normalize(p2 - p1));
					dis = dis + smoothstep(30, 40, abs(fmod(len, 80) - 40)) * 10;
					dis = dis + smoothstep(30, 40, abs(fmod(len + 25,80) - 40)) * 10;
				}
				else if(dot(coord - p1, p2 - p1)<0){
					dis = distance(p1, coord);
					dis += 10;
				}
				else{
					dis = distance(coord, p2);
				}
				return dis;
				
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 p = i.uv - float2(0.5, 0.5);
				//p.y -= 0.1;
				float3 bcol = float3(1.0, 0.8, 0.7 - 0.07 * p.y) * (1.0 - length(p));
				float3 lcol = float3(0.3765, 0.7412, 0.6588);
				float3 rcol = float3(0.9137, 0.5686, 0.4275);
				float3 col = lerp(lcol, bcol, smoothstep(0,3, liner(float2(0.1, 0.1) * _ScreenParams.xy, float2(0.9, 0.9) * _ScreenParams.xy, i.uv.xy * _ScreenParams.xy)));
				col = lerp(rcol, col, smoothstep(0,3, liner(float2(0.1, 0.9) * _ScreenParams.xy, float2(0.9, 0.1) * _ScreenParams.xy, i.uv.xy * _ScreenParams.xy)));
				return float4(col, 1);
			}
			ENDCG
		}
	}
}

