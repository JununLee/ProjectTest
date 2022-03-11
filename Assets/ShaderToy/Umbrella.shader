Shader "Custom/Umbrella"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
		
		float sdfCircle(float2 center, float radius, float2 coord){
			 
			return  distance(coord,center) - radius;
		}

		float sdfEllipse(float2 center, float a, float b, float2 coord){
			
			float a2 = a * a;
			float b2 = b * b;
			return (b2 * (coord.x - center.x) * (coord.x - center.x) + a2 * (coord.y - center.y) * (coord.y - center.y) - a2 * b2)/(a2 * b2);
		}

		float sdfRectangle(float2 ori, float2 rect, float2 coord){
		
			coord = coord - ori;
			float2 d = abs(coord) - rect;
			float insideDistance = min(max(d.x, d.y), 0);
			float outsidedistance = length(max(d, 0));
			return insideDistance + outsidedistance;
		}

		float sdfintersection(float a, float b){
			return max(a, b);		}

		float sdfdifference(float a, float b){
			return max(a, -b);
		}

		float sdfunion(float a, float b){
			return min(a, b);
		}




	ENDCG

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
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 p = ((i.uv - float2(0.5, 0.5)) * _ScreenParams.xy)/min(_ScreenParams.x,_ScreenParams.y);
				//p.y -= 0.1;
				float3 bcol = float3(1.0, 0.8, 0.7 - 0.07 * p.y) * (1.0 - length(p));
				float3 circol = float3(0, 0, 1);
				float v = sdfdifference(sdfCircle(float2(0,-0.2),0.015,p), sdfCircle(float2(0,-0.2),0.010,p));

				v = sdfintersection(v,p.y+0.2);

				v = sdfunion(v, sdfRectangle(float2(0.0125, -0.003), float2(0.0025, 0.2), p));

				v = sdfunion(v, sdfCircle(float2(0.0125, 0.2), 0.005,p));

				
				float3 col = lerp(circol, bcol, smoothstep(-0.003, 0.003, v));
				col = lerp(float3(0, 0, 0), col, smoothstep(0, 0.002, abs(v)));

				float e = sdfintersection(sdfEllipse(float2(0.0125, -0.0025), 0.2, 0.2, p),sdfEllipse(float2(0.0125, 0.2), 0.6, 0.2, p));

				v = sdfunion(v, e);

				col = lerp(circol, col, smoothstep(-0.003, 0.003,e));
				col = lerp(float3(0, 0, 0), col, smoothstep(0, 0.025, abs(e)));

				//col = pow(col,1/2.2);

				return float4(col, 1);
			}
			ENDCG
		}
	}
}

