Shader "Custom/SinCurved"
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
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 p = (i.uv - float2(0.5, 0.5)); //* _ScreenParams.xy)/min(_ScreenParams.x,_ScreenParams.y);
				//p.y -= 0.1;
				float3 bcol = float3(1.0, 0.8, 0.7 - 0.07 * p.y) * (1.0 - length(p));
				float3 xycol = float3(0, 0, 0);
				float3 col = lerp(bcol, xycol, 1 - smoothstep(0, 3, abs(p.y)*_ScreenParams.y));
				col = lerp(col, xycol, 1 - smoothstep(0, 3, abs(p.x)*_ScreenParams.x));
				col = lerp(col, xycol, 1 - smoothstep(0, 0.005, distance(p,float2(p.x,0.1 * sin(p.x * 3.14159265358979 * 4)))));
				return float4(col, 1);
			}
			ENDCG
		}
	}
}

