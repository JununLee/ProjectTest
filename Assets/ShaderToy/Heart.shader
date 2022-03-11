Shader "Custom/Heart"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_Scale("Scale",float) = 1
		_XScale("XScale",float) = 1
		_YScale("YScale",float) = 1
		_XOffset("XOffset",float) = 0
		_YOffset("YOffset",float) = 0
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
			float _Scale;
			float _XScale;
			float _YScale;
			float _XOffset;
			float _YOffset;
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 iResolution : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.iResolution = _ScreenParams;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 p = (i.uv - float2(0.5, 0.5));
				p += float2(_XOffset, _YOffset);

				float3 bcol = float3(1.0, 0.8, 0.7 - 0.07 * p.y) * (1.0 - length(p));
				
				float3 hcol = float3(_Color.r, _Color.g -  0.1 * p.y, _Color.b)* (1.0 - length(p));

				float tt = fmod(_Time.y, 1.5) / 1.5;
				float ss = pow(tt, 0.2) * 0.5 + 0.5;
				ss = 1.0 + ss * 0.5 * sin(tt * 6.2831 * 3.0 + p.y * 0.5) * exp(-tt * 4.0);
				p *= float2(0.5, 1.5) + ss * float2(0.5, -0.5);

				p.y *= _ScreenParams.y / _ScreenParams.x;
				p /= _Scale;
				p.x /= _XScale;
				p.y /= _YScale;

				float x = abs(p.x);
				float f1 = sqrt(1 - x * x) + pow(x, 2 / 3.0);
				float f2 = -sqrt(1 - x * x) + pow(x, 2 / 3.0);

				float3 col = lerp(bcol, hcol, smoothstep(-0.01, 0.01, (f1 - p.y) * (p.y - f2)));
				return float4(col, 1);
			}
			ENDCG
		}
	}
}

